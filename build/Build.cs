using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;

    static AbsolutePath SourceDirectory => RootDirectory / "src";
    static AbsolutePath TestDirectory => RootDirectory / "test";
    static AbsolutePath E2ETestDirectory => RootDirectory / "e2e";
    static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    static IEnumerable<AbsolutePath> AllProjects => RootDirectory.GlobFiles("**/*.csproj").Where(path => !path.ToString().Contains("_build"));
    static IEnumerable<AbsolutePath> TestProjects => TestDirectory.GlobFiles("**/*.csproj");
    static IEnumerable<AbsolutePath> E2EProjects => E2ETestDirectory.GlobFiles("**/*.csproj");

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

    Target Pack => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(Solution)
                .SetOutputDirectory(ArtifactsDirectory)
                .EnableIncludeSymbols()
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .EnableNoBuild());
        });

    static void CleanInParallel(
        IEnumerable<AbsolutePath> files,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default)
    {
        var options = new ParallelOptions {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        Parallel.Invoke(options, files.Select(file =>
            (Action)(() =>
            {
                DotNetClean(s => s
                    .SetProject(file)
                    .SetVerbosity(DotNetVerbosity.Quiet));
            })).ToArray());
    }

    static void TestInParallel(
        IEnumerable<AbsolutePath> files,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default)
    {
        var options = new ParallelOptions {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        Parallel.Invoke(options, files.Select(file =>
            (Action)(() =>
            {
                DotNetTest(s => s
                    .SetProjectFile(file)
                    .EnableNoRestore()
                    .EnableNoBuild());
            })).ToArray());
    }
}
