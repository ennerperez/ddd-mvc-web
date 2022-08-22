[CmdletBinding()]
Param(
    [string]$context = "Default",
    [string][AllowNull()]$name,
    [switch]$clear = $false,
    [switch]$update = $false,
    [switch]$rollback = $false,
    [string]$startup = "Web",
    [string]$output
)

$context_name = "$context" + "Context";

if ($clear)
{
  dotnet ef database drop -f --context $context_name --project src/Persistence --startup-project src/$startup
  if (Test-Path src/Persistence/Migrations/$context)
  {
    Remove-Item -r -fo src/Persistence/Migrations/$context
  }
}
elseif ($rollback)
{
  dotnet ef migrations remove --force --context $context_name  --project src/Persistence --startup-project src/$startup
}
if (![string]::IsNullOrEmpty($output))
{
  $script_name = [DateTime]::Now.ToString("yyyyMMdd") + "_$context"+"Context_"+"$output"
  dotnet ef migrations script -i --context $context_name -o src/Persistence/Migrations/$context/Scripts/$script_name.sql  --project src/Persistence --startup-project src/$startup
  Write-Output "Done: $script_name"
}
else
{
  if (![string]::IsNullOrEmpty($name))
  {
    dotnet ef migrations add $name -o "Migrations/$context" --context $context_name  --project src/Persistence --startup-project src/$startup
    if ($update)
    {
      dotnet ef database update $name --context $context_name --project src/Persistence --startup-project src/$startup
    }
  }
  else
  {
    if ($update)
    {
      dotnet ef database update --context $context_name --project src/Persistence --startup-project src/$startup
    }
  }
}