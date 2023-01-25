using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Nuke.Common;
using Nuke.Common.Tools.DotNet.EF;
using Nuke.Common.Tools.DotNet.EF.Commands;
using static Nuke.Common.Tools.DotNet.EF.Tasks;

partial class Build
{
	Target MigrationAdd => _ => _
		.Executes(() =>
		{
			var persistence = Solution.GetProject("Persistence")?.Path;
			var startup = Solution.GetProject("Web")?.Path;
			var startupPath = Solution.GetProject("Web")?.Directory ?? string.Empty;

			var config = new ConfigurationBuilder()
				.AddJsonFile(Path.Combine(startupPath, "appsettings.json"), false, true)
				.AddJsonFile(Path.Combine(startupPath, $"appsettings.{Environment}.json"), true, true)
				.Build();

			var connectionStrings = new Dictionary<string, string>();
			config.Bind("ConnectionStrings", connectionStrings);

			var combinations = from item in connectionStrings
				let split = item.Key.Split(".")
				let context = split.First()
				let provider = split.Last()
				select new {Context = context, Name = context.Replace("Context", ""), Provider = provider, item.Value};

			foreach (var item in combinations)
			{
				DotNetEf(_ => new MigrationsSettings(Migrations.Add)
					.EnableNoBuild()
					.SetProjectFile(Solution.GetProject(persistence))
					.SetStartupProjectFile(Solution.GetProject(startup))
					.SetName(DateTime.Now.Ticks.ToString())
					.SetContext(item.Context)
					.SetOutputDir(Path.Combine("Migrations", item.Name, item.Provider))
				);
			}
		});

	Target MigrationRemove => _ => _
		.Executes(() =>
		{
			var persistence = Solution.GetProject("Persistence")?.Path;
			var startup = Solution.GetProject("Web")?.Path;
			var startupPath = Solution.GetProject("Web")?.Directory ?? string.Empty;

			var config = new ConfigurationBuilder()
				.AddJsonFile(Path.Combine(startupPath, "appsettings.json"), false, true)
				.AddJsonFile(Path.Combine(startupPath, $"appsettings.{Environment}.json"), true, true)
				.Build();

			var connectionStrings = new Dictionary<string, string>();
			config.Bind("ConnectionStrings", connectionStrings);

			var combinations = from item in connectionStrings
				let split = item.Key.Split(".")
				let context = split.First()
				let provider = split.Last()
				select new {Context = context, Name = context.Replace("Context", ""), Provider = provider, item.Value};

			foreach (var item in combinations)
			{
				DotNetEf(_ => new MigrationsSettings(Migrations.Remove)
					.EnableNoBuild()
					.SetProjectFile(Solution.GetProject(persistence))
					.SetStartupProjectFile(Solution.GetProject(startup))
					.SetName(DateTime.Now.Ticks.ToString())
					.SetContext(item.Context)
					.SetOutputDir(Path.Combine("Migrations", item.Name, item.Provider))
				);
			}
		});

	Target MigrationOutput => _ => _
		.Executes(() =>
		{
			var persistence = Solution.GetProject("Persistence")?.Path;
			var startup = Solution.GetProject("Web")?.Path;
			var startupPath = Solution.GetProject("Web")?.Directory ?? string.Empty;

			var config = new ConfigurationBuilder()
				.AddJsonFile(Path.Combine(startupPath, "appsettings.json"), false, true)
				.AddJsonFile(Path.Combine(startupPath, $"appsettings.{Environment}.json"), true, true)
				.Build();

			var connectionStrings = new Dictionary<string, string>();
			config.Bind("ConnectionStrings", connectionStrings);

			var combinations = from item in connectionStrings
				let split = item.Key.Split(".")
				let context = split.First()
				let provider = split.Last()
				where provider != "Sqlite"
				select new {Context = context, Name = context.Replace("Context", ""), Provider = provider, item.Value};

			foreach (var item in combinations)
			{
				var fileName = Path.Combine(Solution.GetProject(persistence)?.Directory ?? string.Empty, "Scripts", item.Name, item.Provider, $"{DateTime.Now:yyyyMMdd}.sql");
				if (File.Exists(fileName)) File.Delete(fileName);
				DotNetEf(_ => new MigrationsSettings(Migrations.Script)
					.SetProjectFile(Solution.GetProject(persistence))
					.SetStartupProjectFile(Solution.GetProject(startup))
					.SetContext(item.Context)
					.SetOutput(fileName)
				);
			}
		});

	Target DatabaseUpdate => _ => _
		.Executes(() =>
		{
			var persistence = Solution.GetProject("Persistence")?.Path;
			var startup = Solution.GetProject("Web")?.Path;
			var startupPath = Solution.GetProject("Web")?.Directory ?? string.Empty;

			var config = new ConfigurationBuilder()
				.AddJsonFile(Path.Combine(startupPath, "appsettings.json"), false, true)
				.AddJsonFile(Path.Combine(startupPath, $"appsettings.{Environment}.json"), true, true)
				.Build();

			var connectionStrings = new Dictionary<string, string>();
			config.Bind("ConnectionStrings", connectionStrings);

			var combinations = from item in connectionStrings
				let split = item.Key.Split(".")
				let context = split.First()
				let provider = split.Last()
				select new {Context = context, Name = context.Replace("Context", ""), Provider = provider, item.Value};

			foreach (var item in combinations)
			{
				DotNetEf(_ => new DatabaseSettings(Database.Update)
					.SetProjectFile(Solution.GetProject(persistence))
					.SetStartupProjectFile(Solution.GetProject(startup))
					.SetContext(item.Context)
				);
			}
		});

	Target DatabaseClear => _ => _
		.Executes(() =>
		{
			var persistence = Solution.GetProject("Persistence")?.Path;
			var startup = Solution.GetProject("Web")?.Path;
			var startupPath = Solution.GetProject("Web")?.Directory ?? string.Empty;

			var config = new ConfigurationBuilder()
				.AddJsonFile(Path.Combine(startupPath, "appsettings.json"), false, true)
				.AddJsonFile(Path.Combine(startupPath, $"appsettings.{Environment}.json"), true, true)
				.Build();

			var connectionStrings = new Dictionary<string, string>();
			config.Bind("ConnectionStrings", connectionStrings);

			var combinations = from item in connectionStrings
				let split = item.Key.Split(".")
				let context = split.First()
				let provider = split.Last()
				select new {Context = context, Name = context.Replace("Context", ""), Provider = provider, item.Value};

			foreach (var item in combinations)
			{
				DotNetEf(_ => new DatabaseSettings(Database.Drop)
					.EnableNoBuild()
					.EnableForce()
					.SetProjectFile(Solution.GetProject(persistence))
					.SetStartupProjectFile(Solution.GetProject(startup))
					.SetContext(item.Context)
				);
			}
		});

}
