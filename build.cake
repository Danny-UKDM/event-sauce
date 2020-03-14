#addin "Cake.Docker&version=0.10.1"
#addin "Cake.FileHelpers&version=3.2.1"
#addin nuget:?package=AWSSDK.CloudFormation&version=3.3.100.3&loaddependencies=true
#addin nuget:?package=AWSSDK.IdentityManagement&version=3.3.100.3&loaddependencies=true
#addin nuget:?package=AWSSDK.SecurityToken&version=3.3.100.3&loaddependencies=true
using System.Text.RegularExpressions;
using System.Threading;
using Amazon;
using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

var stackName = "event-sauce";

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var profileName = Argument("profile", "dev");
var awsRegion = RegionEndpoint.GetBySystemName(Argument("region", "eu-west-1"));
var stackType = Argument("stack", "").ToLowerInvariant();

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var sourceDir = Directory("./src");
var solutions = GetFiles("./**/*.sln");

// BUILD OUTPUT DIRECTORIES
var publishDir = Directory("./publish/");
var artifactsDir = Directory("./artifacts/");

// VERBOSITY
var dotNetCoreVerbosity = Cake.Common.Tools.DotNetCore.DotNetCoreVerbosity.Quiet;

AWSCredentials localAwsCredentials = null;
AWSCredentials remoteAwsCredentials = null;
var dockerComposeFile = "./infrastructure/docker-compose.yml";

///////////////////////////////////////////////////////////////////////////////
// COMMON FUNCTION DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

string GetProjectName(string project)
{
    return project
        .Split(new [] {'/'}, StringSplitOptions.RemoveEmptyEntries)
        .Last()
        .Replace(".csproj", string.Empty);
}

string PascalToKebabCase(string value)
{
    if (string.IsNullOrEmpty(value))
        return value;

    return Regex.Replace(
        value,
        "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
        "-$1",
        RegexOptions.Compiled)
        .Trim()
        .ToLower();
}

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    // Executed BEFORE the first task.
    EnsureDirectoryExists(publishDir);
    EnsureDirectoryExists(artifactsDir);
    Information("Running tasks...");
});
Teardown(ctx =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Cleans all bin and obj directories.")
    .Does(() =>
    {
        foreach(var solution in solutions)
        {
            Information("Cleaning {0}", solution.FullPath);
            CleanDirectories(solution.FullPath + "/**/bin/" + configuration);
            CleanDirectories(solution.FullPath + "/**/obj/" + configuration);
            Information("{0} was clean.", solution.FullPath);
        }

        CleanDirectory(publishDir);
        CleanDirectory(artifactsDir);
    });

Task("Restore")
    .Description("Restores all the NuGet Packages that are used by the specified Solution.")
    .Does(() =>
    {
        var settings = new DotNetCoreRestoreSettings
        {
            DisableParallel = false,
            NoCache = true,
            Verbosity = dotNetCoreVerbosity
        };

        foreach(var solution in solutions)
        {
            Information("Restoring NuGet packages for '{0}'...", solution);
            DotNetCoreRestore(solution.FullPath, settings);
            Information("NuGet packages restored for '{0}'.", solution);
        }
    });

Task("Build")
    .Description("Builds all Projects.")
    .Does(() =>
    {
        var msBuildSettings = new DotNetCoreMSBuildSettings
        {
            TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error,
            Verbosity = dotNetCoreVerbosity
        };

        var settings = new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            MSBuildSettings = msBuildSettings,
            NoRestore = true
        };

        foreach(var solution in solutions)
        {
            Information("Building '{0}'...", solution);
            DotNetCoreBuild(solution.FullPath, settings);
            Information("'{0}' has been built.", solution);
        }
    });

Task("Test-Unit")
    .Description("Runs all Unit Tests.")
    .Does(() =>
    {
        TestInParallel(GetFiles("./test/**/*.csproj"));
    });

public void TestInParallel(
    FilePathCollection files,
    int maxDegreeOfParallelism = -1,
    CancellationToken cancellationToken = default(CancellationToken))
{
    var settings = new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
        Logger = "trx",
        ResultsDirectory = artifactsDir
    };

    var actions = new List<Action>();
    foreach (var file in files) {
        actions.Add(() => {
            Information("Testing '{0}'...", file);
            DotNetCoreTest(file.FullPath, settings);
            Information("'{0}' has been tested.", file);
        });
    }

    var options = new ParallelOptions {
        MaxDegreeOfParallelism = maxDegreeOfParallelism,
        CancellationToken = cancellationToken
    };

    Parallel.Invoke(options, actions.ToArray());
}

Task("Publish")
    .Description("Publishes the Lambda Functions.")
    .Does(() =>
    {
        //Publish Functional Lambdas
        var projectFiles = GetFiles("./src/**/*.csproj");
        foreach (var project in projectFiles)
        {
            var projectName = GetProjectName(project.FullPath);

            var outputDirectory = System.IO.Path.Combine(publishDir, projectName);

            var msBuildSettings = new DotNetCoreMSBuildSettings
            {
                TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error,
                Verbosity = dotNetCoreVerbosity
            };

            var settings = new DotNetCorePublishSettings
            {
                Configuration = configuration,
                MSBuildSettings = msBuildSettings,
                NoRestore = true,
                OutputDirectory = outputDirectory,
                Runtime = "linux-x64",
                Verbosity = dotNetCoreVerbosity
            };

            Information("Publishing '{0}'...", projectName);
            DotNetCorePublish(project.FullPath, settings);
            Information("'{0}' has been published.", projectName);
        }
    });

Task("Local-AWS")
    .Description("Spins up Local Infrastucture.")
    .Does(() =>
    {
        Information("Ensuring AWS profile is defined");
        EnsureAwsProfile();

        Information("Ensuring local infrastructure is running");
        DockerComposeUp(new DockerComposeUpSettings
        {
            Files = new [] { dockerComposeFile },
            DetachedMode = true,
            RemoveOrphans = true
        });

        Information("Configured Local AWS");
    });

void EnsureAwsProfile()
{
    var profileName = "localstack";
    CredentialProfile awsProfile;
    var credentialStore = new SharedCredentialsFile();
    if (!credentialStore.TryGetProfile(profileName, out awsProfile))
    {
        var options = new CredentialProfileOptions
        {
            AccessKey = "localstack",
            SecretKey = "localstack"
        };
        awsProfile = new CredentialProfile(profileName, options) {Region = RegionEndpoint.EUWest1};
        credentialStore.RegisterProfile(awsProfile);
        Information($"Profile: '{profileName}' has been created");
    }
    localAwsCredentials = awsProfile.GetAWSCredentials(credentialStore);
}

Task("Local-AWS-Down")
    .Description("Shuts down local infrastructure.")
    .Does(CleanupEnvironment);

void CleanupEnvironment() {
    Information("Shutting down local infrastructure");
    DockerComposeDown(new DockerComposeDownSettings{
        Files = new [] { dockerComposeFile }
    });
}

Task("Test-E2E")
    .Description("Runs end-to-end tests.")
    .IsDependentOn("Local-AWS")
    .Does(() =>
    {
        var settings = new DotNetCoreTestSettings
        {
            Configuration = configuration,
            NoRestore = true,
            NoBuild = true,
            Logger = "trx",
            ResultsDirectory = artifactsDir
        };

        var projectFiles = GetFiles("./e2e/**/*.csproj");
        foreach(var file in projectFiles)
        {
            Information("Testing '{0}'...", file);
            DotNetCoreTest(file.FullPath, settings);
            Information("'{0}' has been tested.", file);
        }
    })
    .OnError(exception =>
    {
        CleanupEnvironment();
        throw new Exception("Tests failed");
    });

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Package")
    .Description("This is the task which will run if target Package is passed in.")
    .IsDependentOn("Default")
    .IsDependentOn("Test-Unit")
    .IsDependentOn("Publish")
    .Does(() => { Information("Package target ran."); });

Task("Test")
    .Description("Runs just the unit tests.")
    .IsDependentOn("Default")
    .IsDependentOn("Test-Unit")
    .Does(() => { Information("Test target ran."); });

Task("Test-All")
    .Description("Runs all tests in solution.")
    .IsDependentOn("Package")
    .IsDependentOn("Test-E2E")
    .Does(() => { Information("Test-All target ran."); })
    .Finally(CleanupEnvironment);

Task("Default")
    .Description("This is the default task which will run if no specific target is passed in.")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);