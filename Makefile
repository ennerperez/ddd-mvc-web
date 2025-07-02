.PHONY: * #since no targets will produce files, saves us from needing to specify on all https://www.gnu.org/software/make/manual/html_node/Phony-Targets.html

environment = Test
nukeproject = "build\_build.csproj"

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

publish: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Publish --configuration Release --environment $environment
	
pack: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Pack

migration-add: nuke
	dotnet run --project ${nukeproject} --no-build -- --target MigrationAdd

migration-remove: nuke
	dotnet run --project ${nukeproject} --no-build -- --target MigrationRemove

migration-output: nuke
	dotnet run --project ${nukeproject} --no-build -- --target MigrationOutput

database-update: nuke
	dotnet run --project ${nukeproject} --no-build -- --target DatabaseUpdate

database-clear: nuke
	dotnet run --project ${nukeproject} --no-build -- --target DatabaseClear

database-rollback: nuke
	dotnet run --project ${nukeproject} --no-build -- --target DatabaseRollback

dbcontext-optimize: nuke
	dotnet run --project ${nukeproject} --no-build -- --target DbContextOptimize
	
test-unittest: nuke
	dotnet run --project ${nukeproject} --no-build -- --target UnitTest

test-uitest: nuke
	dotnet run --project ${nukeproject} --no-build -- --target UITest

analyze: nuke
	dotnet run --project ${nukeproject} --no-build -- --target Analyze