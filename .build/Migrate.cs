using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.DotNet.EF;
using Nuke.Common.Tools.DotNet.EF.Commands;
using Nuke.Common.ProjectModel;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.DotNet.EF.Tasks;

partial class Build
{

	Project Persistence => Solution.GetProject("Persistence");
	Project Startup => Solution.GetProject("Web");
	string MigrationsPath => "Migrations";
	string ScriptsPath => "Scripts";
	IEnumerable<Tuple<string, string, string, string>> GetConnectionStringsCombinations()
	{
		var config = new ConfigurationBuilder()
			.AddJsonFile(Path.Combine(Startup.Directory, "appsettings.json"), false, true)
			.AddJsonFile(Path.Combine(Startup.Directory, $"appsettings.{Environment}.json"), true, true)
			.Build();

		var connectionStrings = new Dictionary<string, string>();
		config.Bind("ConnectionStrings", connectionStrings);

		var combinations = from item in connectionStrings
			let split = item.Key.Split("_")
			where split.Length > 1
			let context = split.First()
			let provider = split.Last()
			select new Tuple<string, string, string, string>(context, context.Replace("Context", ""), provider, item.Value);

		return combinations;
	}

	Target FastCompile => _ => _
		.DependsOn(Restore)
		.Executes(() =>
		{
			DotNetBuild(s => s
				.SetProjectFile(Solution)
				.SetConfiguration(Configuration)
				.EnableNoRestore());
		});

	Target MigrationAdd => _ => _
		.DependsOn(FastCompile)
		.Executes(() =>
		{
			var combinations = GetConnectionStringsCombinations();
			foreach (var item in combinations)
			{
				var folderName = (item.Item2 == item.Item3 ? "" : item.Item3);
				DotNetEf(_ => new MigrationsSettings(Migrations.Add)
					.EnableNoBuild()
					.SetProjectFile(Persistence)
					.SetStartupProjectFile(Startup)
					.SetName(DateTime.Now.Ticks.ToString())
					.SetContext(item.Item1)
					.SetOutputDir(Path.Combine(MigrationsPath, item.Item2, folderName))
				);
			}
		});

	Target MigrationRemove => _ => _
		.DependsOn(FastCompile)
		.Executes(() =>
		{
			var combinations = GetConnectionStringsCombinations();
			foreach (var item in combinations)
			{
				var folderName = (item.Item2 == item.Item3 ? "" : item.Item3);
				DotNetEf(_ => new MigrationsSettings(Migrations.Remove)
					.EnableNoBuild()
					.SetProjectFile(Persistence)
					.SetStartupProjectFile(Startup)
					.SetName(DateTime.Now.Ticks.ToString())
					.SetContext(item.Item1)
					.SetOutputDir(Path.Combine(MigrationsPath, item.Item2, folderName))
				);
			}
		});

	Target MigrationOutput => _ => _
		.DependsOn(FastCompile)
		.Executes(() =>
		{
			var combinations = GetConnectionStringsCombinations().Where(i => i.Item3 != "Sqlite");
			foreach (var item in combinations)
			{
				var folderName = (item.Item2 == item.Item3 ? "" : item.Item3);
				var fileName = Path.Combine(Persistence?.Directory ?? string.Empty, ScriptsPath, item.Item2, folderName, $"{DateTime.Now:yyyyMMdd}.sql");
				if (File.Exists(fileName)) File.Delete(fileName);
				DotNetEf(_ => new MigrationsSettings(Migrations.Script)
					.EnableIdempotent()
					.EnableNoBuild()
					.SetProjectFile(Persistence)
					.SetStartupProjectFile(Startup)
					.SetContext(item.Item1)
					.SetOutput(fileName)
				);
			}
		});

	Target DatabaseUpdate => _ => _
		.DependsOn(FastCompile)
		.Executes(() =>
		{
			var combinations = GetConnectionStringsCombinations();
			foreach (var item in combinations)
			{
				DotNetEf(_ => new DatabaseSettings(Database.Update)
					.EnableNoBuild()
					.SetProjectFile(Persistence)
					.SetStartupProjectFile(Startup)
					.SetContext(item.Item1)
				);
			}
		});

	Target DatabaseClear => _ => _
		.Executes(() =>
		{
			var combinations = GetConnectionStringsCombinations();
			foreach (var item in combinations)
			{
				DotNetEf(_ => new DatabaseSettings(Database.Drop)
					.EnableNoBuild()
					.EnableForce()
					.SetProjectFile(Persistence)
					.SetStartupProjectFile(Startup)
					.SetContext(item.Item1)
				);
			}
		});

}
