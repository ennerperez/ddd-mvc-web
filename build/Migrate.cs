using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.EntityFramework;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable UsageOfDefaultStructEquality
// ReSharper disable UnusedMember.Local
public partial class Build
{
    [Parameter("The project is also known as the target project because it's where the commands add or remove files.")]
    public readonly string TargetProject;

    [Parameter("The startup project is the one that the tools build and run.")]
    public readonly string StartupProject;

    Project Persistence => Solution.AllProjects.FirstOrDefault(m => m.Name == TargetProject);
    Project Startup => Solution.AllProjects.FirstOrDefault(m => m.Name == StartupProject);

    static string MigrationsPath => "Migrations";

    static string ScriptsPath => "Scripts";

    IEnumerable<Tuple<string, string, string, string>> GetConnectionStringsCombinations()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(Startup.Directory, path2: "appsettings.json"), false, true)
            .AddJsonFile(Path.Combine(Startup.Directory, $"appsettings.{Environment}.json"), true, true)
            .Build();

        var connectionStrings = new Dictionary<string, string>();
        config.Bind(key: "ConnectionStrings", connectionStrings);

        var combinations = from item in connectionStrings
            let split = item.Key.Split(".")
            where split.Length > 1
            let context = split.First()
            let provider = split.Last()
            select new Tuple<string, string, string, string>(context, context.Replace(oldValue: "Context", newValue: ""), provider, item.Value);

        return combinations;
    }

    Target FastCompile => d => d
        .DependsOn(Restore)
        .Executes(() =>
        {
            var projects = Solution.AllProjects
                .Where(m => !m.Name.StartsWith("."))
                .Where(m => new[]
                {
                    Persistence, Startup
                }.Contains(m))
                .ToArray();
            DotNetBuild(s => s
                .SetWarningLevel(0)
                .CombineWith(projects, configurator: (buildSettings, v) => buildSettings
                    .SetProjectFile(v)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()));
        });

    Target MigrationAdd => d => d
        .DependsOn(FastCompile)
        .Executes(() =>
        {
            var combinations = GetConnectionStringsCombinations();
            foreach (var item in combinations)
            {
                var folderName = item.Item2 == item.Item3 ? "" : item.Item3;
                EntityFrameworkTasks.EntityFrameworkMigrationsAdd(c => c
                    .SetProcessWorkingDirectory(SourceDirectory)
                    .EnableNoBuild()
                    .SetProject(Persistence)
                    .SetStartupProject(Startup)
                    .SetName(DateTime.Now.Ticks.ToString())
                    .SetContext(item.Item1)
                    .SetOutputDirectory(Path.Combine(MigrationsPath, item.Item2, folderName))
                );
            }
        });

    Target MigrationRemove => d => d
        .DependsOn(FastCompile)
        .Executes(() =>
        {
            var combinations = GetConnectionStringsCombinations();
            foreach (var item in combinations)
            {
                EntityFrameworkTasks.EntityFrameworkMigrationsRemove(c => c
                    .SetProcessWorkingDirectory(SourceDirectory)
                    .EnableNoBuild()
                    .SetProject(Persistence)
                    .SetStartupProject(Startup)
                    .SetContext(item.Item1)
                );
            }
        });

    Target MigrationOutput => d => d
        .DependsOn(FastCompile)
        .Executes(() =>
        {
            var combinations = GetConnectionStringsCombinations().Where(i => i.Item3 != "Sqlite");
            foreach (var item in combinations)
            {
                var folderName = item.Item2 == item.Item3 ? "" : item.Item3;
                var fileName = Path.Combine(Persistence?.Directory ?? string.Empty, ScriptsPath, item.Item2, folderName, $"{DateTime.Now:yyyyMMdd}.sql");
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                EntityFrameworkTasks.EntityFrameworkMigrationsScript(c => c
                    .SetProcessWorkingDirectory(SourceDirectory)
                    .EnableIdempotent()
                    .EnableNoBuild()
                    .SetProject(Persistence)
                    .SetStartupProject(Startup)
                    .SetContext(item.Item1)
                    .SetOutput(fileName)
                );
            }
        });

    Target DatabaseUpdate => d => d
        .DependsOn(FastCompile)
        .Executes(() =>
        {
            var combinations = GetConnectionStringsCombinations();
            foreach (var item in combinations)
            {
                EntityFrameworkTasks.EntityFrameworkDatabaseUpdate(c => c
                    .SetProcessWorkingDirectory(SourceDirectory)
                    .EnableNoBuild()
                    .SetProject(Persistence)
                    .SetStartupProject(Startup)
                    .SetContext(item.Item1)
                );
            }
        });

    Target DatabaseClear => d => d
        .DependsOn(FastCompile)
        .Executes(() =>
        {
            var combinations = GetConnectionStringsCombinations();
            foreach (var item in combinations)
            {
                EntityFrameworkTasks.EntityFrameworkDatabaseDrop(c => c
                    .SetProcessWorkingDirectory(SourceDirectory)
                    .EnableNoBuild()
                    .EnableForce()
                    .SetProject(Persistence)
                    .SetStartupProject(Startup)
                    .SetContext(item.Item1)
                );
            }
        });

    Target DatabaseRollback => d => d
        .DependsOn(FastCompile)
        .Executes(() =>
        {
            var combinations = GetConnectionStringsCombinations();
            foreach (var item in combinations)
            {
                var migrations = EntityFrameworkTasks.EntityFrameworkMigrationsList(c => c
                    .SetProcessWorkingDirectory(SourceDirectory)
                    .EnableNoBuild()
                    .SetProject(Persistence)
                    .SetStartupProject(Startup)
                    .SetContext(item.Item1)
                ).Where(m => !m.Text.EndsWith("(Pending)")).ToList();

                if (migrations.Count == 0)
                {
                    continue;
                }

                var lastIndex = migrations.IndexOf(migrations.Last());
                lastIndex--;
                if (lastIndex < 0)
                {
                    continue;
                }

                var lastMigration = migrations[lastIndex].Text;
                EntityFrameworkTasks.EntityFrameworkDatabaseUpdate(c => c
                    .SetProcessWorkingDirectory(SourceDirectory)
                    .EnableNoBuild()
                    .SetProject(Persistence)
                    .SetStartupProject(Startup)
                    .SetContext(item.Item1)
                    .SetMigration(lastMigration)
                );
            }
        });
}
