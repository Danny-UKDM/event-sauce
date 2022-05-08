using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    public Build() => DockerTasks.DockerLogger = (_, e) => Log.Debug(e);

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;

    static AbsolutePath SrcDirectory => RootDirectory / "src";
    static AbsolutePath TestDirectory => RootDirectory / "test";
    static AbsolutePath E2ETestDirectory => RootDirectory / "e2e";
    static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    static AbsolutePath PublishDirectory => RootDirectory / "publish";
    static AbsolutePath DockerComposeDirectory => RootDirectory / "infrastructure";

    static IEnumerable<AbsolutePath> AllProjects => RootDirectory.GlobFiles("**/*.csproj").Where(path => !path.ToString().Contains("_build"));
    static IEnumerable<AbsolutePath> SrcProjects => SrcDirectory.GlobFiles("**/*.csproj");
    static IEnumerable<AbsolutePath> TestProjects => TestDirectory.GlobFiles("**/*.csproj");
    static IEnumerable<AbsolutePath> E2EProjects => E2ETestDirectory.GlobFiles("**/*.csproj");

    static AWSCredentials LocalCredentials { get; set; }

    static ParallelOptions ParallelOptions => new() { MaxDegreeOfParallelism = -1 };


    Target Clean => _ => _
        .Executes(() =>
        {
            CleanInParallel(AllProjects);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            TestInParallel(TestProjects);
        });


    Target Publish => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            foreach (var srcProject in SrcProjects)
            {
                var dockerFile = srcProject.Parent.GlobFiles("Dockerfile").FirstOrDefault();

                if (dockerFile is null) continue;

                var projectId = srcProject.NameWithoutExtension.ToKebabCase();
                var assemblyVersion = XmlTasks.XmlPeek(srcProject, "//PropertyGroup/Version").First() ?? "0.0.1";
                var gitSha = GitTasks.Git("rev-parse --short HEAD").First().Text;
                var version = $"{assemblyVersion}-{gitSha}";

                DockerTasks.DockerImageBuild(s => s
                    .SetFile(dockerFile)
                    .SetPath(".")
                    .SetProcessWorkingDirectory(dockerFile.Parent)
                    .AddBuildArg($"VERSION={version}")
                    .AddTag($"event-sauce/{projectId}:latest", $"event-sauce/{projectId}:{version}")
                    );
            }
        });

    Target LocalAwsUp => _ => _
        .DependsOn(Publish)
        .Executes(async () =>
        {
            Log.Information("Ensuring Localstack AWS profile is defined...");
            EnsureLocalstackProfile();

            Log.Information("Ensuring local infrastructure is running...");
            ProcessTasks.StartProcess(
                "docker-compose",
                "up -d --remove-orphans",
                workingDirectory: DockerComposeDirectory,
                customLogger: (_, s) => Log.Debug(s)
            ).WaitForExit();

            Log.Information("Creating local DynamoDb tables...");

            await CreateTable(new CreateTableRequest
            {
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("PK", ScalarAttributeType.S),
                    new("SK", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("PK", KeyType.HASH),
                    new("SK", KeyType.RANGE)
                },
                ProvisionedThroughput = new ProvisionedThroughput(5, 5),
                TableName = "event-sauce-events"
            });
        });

    Target LocalAwsDown => _ => _
        .Executes(() =>
        {
            Log.Information("Ensuring Localstack AWS profile is defined...");
            EnsureLocalstackProfile();

            Log.Information("Shutting down local infrastructure...");
            ProcessTasks.StartProcess(
                "docker-compose",
                "down",
                workingDirectory: DockerComposeDirectory,
                customLogger: (_, s) => Log.Debug(s)
            ).WaitForExit();
        });

    static void CleanInParallel(IEnumerable<AbsolutePath> files)
    {
        Parallel.Invoke(ParallelOptions, files.Select(file =>
            (Action)(() =>
            {
                DotNetClean(s => s
                    .SetProject(file)
                    .SetVerbosity(DotNetVerbosity.Quiet));
            })).ToArray());
    }

    static void TestInParallel(IEnumerable<AbsolutePath> testFiles)
    {
        Parallel.Invoke(ParallelOptions, testFiles.Select(file =>
            (Action)(() =>
            {
                DotNetTest(s => s
                    .SetProjectFile(file)
                    .EnableNoRestore()
                    .EnableNoBuild());
            })).ToArray());
    }

    static void EnsureLocalstackProfile()
    {
        const string profileName = "localstack";
        var credentialsFile = new SharedCredentialsFile();

        if (credentialsFile.TryGetProfile(profileName, out var awsProfile)) return;

        var options = new CredentialProfileOptions
        {
            AccessKey = "localstack",
            SecretKey = "localstack"
        };
        awsProfile = new CredentialProfile(profileName, options) { Region = RegionEndpoint.EUWest1 };
        credentialsFile.RegisterProfile(awsProfile);

        LocalCredentials = awsProfile.GetAWSCredentials(credentialsFile);

        Log.Information($"Profile: '{profileName}' has been created");
    }

    static async Task CreateTable(CreateTableRequest request)
    {
        using var dynamo = new AmazonDynamoDBClient(LocalCredentials, new AmazonDynamoDBConfig
        {
            ServiceURL = "http://localhost:4566",
            AuthenticationRegion = "eu-west-1"
        });

        try
        {
            await dynamo.DeleteTableAsync(request.TableName);
        }
        catch (ResourceNotFoundException)
        {
        }

        var response = await dynamo.CreateTableAsync(request);

        var tableDescription = response.TableDescription;

        Log.Information("DynamoDB Status: {0} ReadsPerSec: {1} WritesPerSec: {2} Table: {3}",
            tableDescription.TableStatus.ToString(),
            tableDescription.ProvisionedThroughput.ReadCapacityUnits.ToString(),
            tableDescription.ProvisionedThroughput.WriteCapacityUnits.ToString(),
            tableDescription.TableName);
    }
}
