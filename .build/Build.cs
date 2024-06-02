using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.DotNet.EF;
using Nuke.Common.Tools.DotNet.EF.Commands;
using Nuke.Common.Tools.Git;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.DotNet.EF.Tasks;

// ReSharper disable UnusedMember.Local
public partial class Build : NukeBuild
{
    readonly string[] _publishProjects = { "Web" };

    readonly string[] _testsProjects = { "Tests.Business" };

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)", Name = "configuration")]
    public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Environment to build - Default is 'Development' (local) or 'Production' (server)", Name = "environment")]
    public readonly Environment Environment = IsLocalBuild ? Environment.Development : Environment.Production;

    [GitRepository]
    public readonly GitRepository Repository;

    [Solution]
    public readonly Solution Solution;

    [Parameter("Warning Level", Name = "warningLevel")]
    public readonly int WarningLevel;

    string _hash = string.Empty;

    IEnumerable<PublishProjectRecord> _projects = Array.Empty<PublishProjectRecord>();
    IEnumerable<Project> _tests = Array.Empty<Project>();

    Version _version = new("1.0.0.0");

    static AbsolutePath SourceDirectory => RootDirectory / "src";

    static AbsolutePath TestsDirectory => RootDirectory / "tests";

    static AbsolutePath PublishDirectory => RootDirectory / "publish";

    static AbsolutePath ArtifactsDirectory => RootDirectory / "output";

    static AbsolutePath TestResultsDirectory => RootDirectory / "tests" / "results";

    static AbsolutePath ScriptsDirectory => RootDirectory / "scripts";

    Target Prepare => d => d
        .Before(Compile)
        .Before(Publish)
        .Before(Pack)
        .Executes(() =>
        {
            #region Version

            var versionRegex = new Regex(@"v?\=?((?:[0-9]{1,}\.{0,}){1,})", RegexOptions.Compiled);
            _version = Repository.Tags
                .Select(m => versionRegex.Match(m))
                .Where(m => m.Success)
                .Select(m => Version.Parse(m.Groups[1].Value))
                .OrderDescending()
                .FirstOrDefault();
            if (_version == null)
            {
                try
                {
                    _version = GitTasks.Git("tag --sort=-v:refname")
                        .Select(m => m.Text)
                        .Select(m => versionRegex.Match(m))
                        .Where(m => m.Success)
                        .Select(m => Version.Parse(m.Groups[1].Value))
                        .OrderDescending()
                        .FirstOrDefault();
                }
                catch (Exception)
                {
                    Log.Warning("Unable to get repository tags using CLI");
                    using (var gitTag = new Process())
                    {
                        gitTag.StartInfo = new ProcessStartInfo("git", "tag --sort=-v:refname") { WorkingDirectory = RootDirectory, RedirectStandardOutput = true, UseShellExecute = false };
                        gitTag.Start();
                        _version = new[] { gitTag.StandardOutput.ReadToEnd().Trim() }
                            .Select(m => versionRegex.Match(m))
                            .Where(m => m.Success)
                            .Select(m => Version.Parse(m.Groups[1].Value))
                            .OrderDescending()
                            .FirstOrDefault();
                    }
                }
            }

            _hash = Repository.Commit;

            Log.Information("Version: {Version}", _version);
            Log.Information("Hash: {Hash}", _hash);

            #endregion

            #region Projects

            var internalProjects = Solution.AllProjects.Where(s => _publishProjects.Contains(s.Name)).ToArray();
            foreach (var item in internalProjects)
            {
                var project = item.GetMSBuildProject();
                var options = project.Imports.FirstOrDefault(m => m.ImportedProject.EscapedFullPath.EndsWith("Options.props"));
                if (options.ImportedProject != null)
                {
                    // Variables
                    var pg1 = options.ImportedProject.PropertyGroups.FirstOrDefault(m => m.Label == "Environment");
                    if (pg1 != null)
                    {
                        pg1.SetProperty("Environment", Environment.ToString());
                    }

                    if (options.ImportedProject.HasUnsavedChanges)
                    {
                        options.ImportedProject.Save();
                    }

                    options.ImportedProject.Reload();
                }
                else
                {
                    if (project.GetProperty("Environment") != null)
                    {
                        project.SetProperty("Environment", Environment);
                        project.Save();
                    }
                }

                project.ReevaluateIfNecessary();
            }

            _projects = (from project in internalProjects
                let info = project.GetMSBuildProject()
                select new PublishProjectRecord(
                    project
                )).ToArray();

            Log.Information("Projects: {Projects}", _projects.Count());

            #endregion

            #region Tests

            _tests = Solution.AllProjects.Where(s => _testsProjects.Contains(s.Name)).ToArray();

            Log.Information("Tests: {Projects}", _tests.Count());

            #endregion
        });

    Target Clean => d => d
        .Before(Restore)
        .Executes(() =>
        {
            Log.Information("Cleaning bin/obj Directories");
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(path => path.DeleteDirectory());
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(path => path.DeleteDirectory());

            Log.Information("Cleaning Test Results Directory");
            AbsolutePath.Create(TestResultsDirectory).CreateOrCleanDirectory();
            Log.Information("Cleaning Publish Directory");
            AbsolutePath.Create(PublishDirectory).CreateOrCleanDirectory();
            Log.Information("Cleaning Artifacts Directory");
            AbsolutePath.Create(ArtifactsDirectory).CreateOrCleanDirectory();
            Log.Information("Cleaning Scripts Directory");
            AbsolutePath.Create(ScriptsDirectory).CreateOrCleanDirectory();
        });

    Target Restore => d => d
        .Executes(() =>
        {
            Log.Information("Restoring tools");
            DotNetToolRestore();

            DotNetRestore(s => s
                .CombineWith(_projects, (x, v) => x
                    .SetProjectFile(v.project)));
        });

    Target Compile => d => d
        .DependsOn(Clean)
        .DependsOn(Restore)
        .DependsOn(Prepare)
        .Executes(() =>
        {
            var target = from item in _projects
                from framework in item.project.GetTargetFrameworks()
                select new { framework, item.project };

            DotNetBuild(s => s
                .SetWarningLevel(WarningLevel)
                .SetProperty("Environment", Environment.ToString())
                .EnableNoRestore()
                .CombineWith(target, (x, v) => x
                    .SetProjectFile(v.project.Path)
                    .SetFramework(v.framework)
                    .SetConfiguration(Configuration)
                ));
        });

    Target Versioning => d => d
        .DependsOn(Prepare)
        .After(Prepare)
        .Executes(() =>
        {
            if (Environment != Environment.Development)
            {
                var assemblyInfoFiles = SourceDirectory.GetFiles("AssemblyInfo*.cs", int.MaxValue);
                foreach (var assemblyInfoVersionFile in assemblyInfoFiles)
                {
                    Log.Information("Patching: {File}", assemblyInfoVersionFile);
                    if (_version != null)
                    {
                        var content = File.ReadAllText(assemblyInfoVersionFile);
                        var assemblyVersionRegEx = new Regex(@"\[assembly: AssemblyVersion\(.*\)\]", RegexOptions.Compiled);
                        var assemblyFileVersionRegEx = new Regex(@"\[assembly: AssemblyFileVersion\(.*\)\]", RegexOptions.Compiled);
                        var assemblyInformationalVersionRegEx = new Regex(@"\[assembly: AssemblyInformationalVersion\(.*\)\]", RegexOptions.Compiled);

                        content = assemblyVersionRegEx.Replace(content, $"[assembly: AssemblyVersion(\"{_version}\")]");
                        content = assemblyFileVersionRegEx.Replace(content, $"[assembly: AssemblyFileVersion(\"{_version}\")]");
                        content = assemblyInformationalVersionRegEx.Replace(content, $"[assembly: AssemblyInformationalVersion(\"{_version:3}+{_hash}\")]");

                        File.WriteAllText(assemblyInfoVersionFile, content);
                    }
                    else
                    {
                        Log.Warning("Version was not found in {File}", assemblyInfoVersionFile);
                    }
                }
            }
            else
            {
                Log.Information("Skiping on {Environment}", Environment);
            }
        });

    Target Test => d => d
        .DependsOn(Prepare)
        .Executes(() =>
        {
            DotNetToolRestore();
            AbsolutePath.Create(TestResultsDirectory).CreateOrCleanDirectory();

            DotNetTest(s => s
                .SetConfiguration(Configuration)
                .When(true, x => x
                    .SetLoggers("trx")
                    .SetResultsDirectory(TestResultsDirectory))
                .CombineWith(_tests, (x, v) => x
                    .SetProjectFile(v.Path)));

            TestResultsDirectory.GlobFiles("*.trx")
                .ForEach(m =>
                {
                    DotNet($"trx2junit {m}");
                });
        });

    Target Publish => d => d
        .DependsOn(Clean)
        .DependsOn(Restore)
        .DependsOn(Prepare)
        .DependsOn(Versioning)
        .Executes(() =>
        {
            var persistence = Solution.AllProjects.FirstOrDefault(m => m.Name == "Persistence")?.Path;
            var startup = Solution.AllProjects.FirstOrDefault(m => m.Name == "Web")?.Path;
            var startupPath = Solution.AllProjects.FirstOrDefault(m => m.Name == "Web")?.Directory ?? string.Empty;

            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(startupPath, "appsettings.json"), false, true)
                .AddJsonFile(Path.Combine(startupPath, $"appsettings.{Environment}.json"), true, true)
                .Build();

            var connectionStrings = new Dictionary<string, string>();
            config.Bind("ConnectionStrings", connectionStrings);

            var combinations = from item in connectionStrings
                let split = item.Key.Split(".")
                where split.Length > 1
                let context = split.First()
                let provider = split.Last()
                where provider != "Sqlite"
                select new { Context = context, Name = context.Replace("Context", ""), Provider = provider, item.Value };

            foreach (var item in combinations)
            {
                var fileName = Path.Combine(ScriptsDirectory, $"{item.Name}_{item.Provider}_{DateTime.Now:yyyyMMdd}.sql");
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                DotNetEf(_ => new MigrationsSettings(Migrations.Script)
                    .EnableIdempotent()
                    .SetProjectFile(persistence)
                    .SetStartupProjectFile(startup)
                    .SetContext(item.Context)
                    .SetOutput(fileName)
                );
            }

            var publishCombinations =
                from project in _publishProjects.Select(m => Solution.AllProjects.FirstOrDefault(p => p.Name == m))
                from framework in project.GetTargetFrameworks()
                select new { project, framework };

            DotNetPublish(s => s
                .SetWarningLevel(WarningLevel)
                .SetConfiguration(Configuration)
                .DisablePublishSingleFile()
                .CombineWith(publishCombinations, (x, v) => x
                    .SetProject(v.project)
                    .SetFramework(v.framework)
                    .SetOutput(PublishDirectory / v.project.Name / v.framework)));
        });

    Target Pack => d => d
        .DependsOn(Publish)
        .Executes(() =>
        {
            var publishCombinations =
                from project in _publishProjects.Select(m => Solution.AllProjects.FirstOrDefault(p => p.Name == m))
                from framework in project.GetTargetFrameworks()
                select new { project, framework };

            CopyDirectoryRecursively(ScriptsDirectory, ArtifactsDirectory / "Scripts");
            CopyDirectoryRecursively(TestResultsDirectory, ArtifactsDirectory / "Tests");
            foreach (var v in publishCombinations)
            {
                ZipFile.CreateFromDirectory($"{PublishDirectory}/{v.project.Name}/{v.framework}", $"{ArtifactsDirectory}/{v.project.Name}.zip");
            }

            AbsolutePath.Create(PublishDirectory).CreateOrCleanDirectory();
            AbsolutePath.Create(TestResultsDirectory).CreateOrCleanDirectory();
            AbsolutePath.Create(ScriptsDirectory).CreateOrCleanDirectory();
            Log.Information($"Output: {ArtifactsDirectory}");
        });

    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Pack);

    public record PublishProjectRecord(Project project);
}
