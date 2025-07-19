.PHONY: * #since no targets will produce files, saves us from needing to specify on all https://www.gnu.org/software/make/manual/html_node/Phony-Targets.html

environment = Test
configuration = Release
nukeproject = "build\_build.csproj"

# DOTNET #

restore:
	dotnet restore

# NUKE BUILD #

nuke:
	dotnet build ${nukeproject} /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet

nuke-clean: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Clean

nuke-prepare: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Prepare

nuke-versioning: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Versioning

nuke-restore: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Restore

nuke-compile: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Compile

nuke-publish: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Publish --configuration Release --environment $environment

nuke-pack: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Pack

nuke-migration-add: nuke
	dotnet run --project ${nukeproject} --no-build -- --target MigrationAdd

nuke-migration-remove: nuke
	dotnet run --project ${nukeproject} --no-build -- --target MigrationRemove

nuke-migration-output: nuke
	dotnet run --project ${nukeproject} --no-build -- --target MigrationOutput

nuke-database-update: nuke
	dotnet run --project ${nukeproject} --no-build -- --target DatabaseUpdate

nuke-database-clear: nuke
	dotnet run --project ${nukeproject} --no-build -- --target DatabaseClear

nuke-database-rollback: nuke
	dotnet run --project ${nukeproject} --no-build -- --target DatabaseRollback

nuke-dbcontext-optimize: nuke
	dotnet run --project ${nukeproject} --no-build -- --target DbContextOptimize

nuke-test-unittest: nuke
	dotnet run --project ${nukeproject} --no-build -- --target UnitTest

nuke-test-uitest: nuke
	dotnet run --project ${nukeproject} --no-build -- --target UITest

nuke-analyze: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Analyze