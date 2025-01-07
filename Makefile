.PHONY: * #since no targets will produce files, saves us from needing to specify on all https://www.gnu.org/software/make/manual/html_node/Phony-Targets.html

environment = Test
nukeproject = ".build\_build.csproj"

nuke:
	dotnet build ${nukeproject} /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet

clean: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Clean

prepare: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Prepare
	
versioning: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Versioning

restore: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Restore

compile: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Compile

test: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Test

publish: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Publish --configuration Release --environment $environment
	
pack: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Pack

migration_add: nuke
	dotnet run --project ${nukeproject} --no-build -- --target MigrationAdd

migration remove: nuke
	dotnet run --project ${nukeproject} --no-build -- --target MigrationRemove

migration_output: nuke
	dotnet run --project ${nukeproject} --no-build -- --target MigrationOutput

database_update: nuke
	dotnet run --project ${nukeproject} --no-build -- --target DatabaseUpdate

database_clear: nuke
	dotnet run --project ${nukeproject} --no-build -- --target DatabaseClear

database_rollback: nuke
	dotnet run --project ${nukeproject} --no-build -- --target DatabaseRollback

dbcontext_optimize: nuke
	dotnet run --project ${nukeproject} --no-build -- --target DbContextOptimize