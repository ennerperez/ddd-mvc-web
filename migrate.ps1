[CmdletBinding()]
Param(
    [string]$context = "Default",
    [string][AllowNull()]$name,
    [switch]$clear = $false,
    [switch]$update = $false,
    [switch]$rollback = $false,
    [string]$output
)

$context_name = "$context" + "Context";

if ($clear)
{
    dotnet ef database drop -f --context $context_name --project src/Persistence --startup-project src/Web
    if (Test-Path src/Persistence/Migrations/$context)
    {
	   Remove-Item -r -fo src/Persistence/Migrations/$context
    }
}
elseif ($rollback)
{
        dotnet ef migrations remove --force --context $context_name  --project src/Persistence --startup-project src/Web
}
if (![string]::IsNullOrEmpty($output))
{
    $script_name = [DateTime]::Now.ToString("yyyyMMdd") + "_$context"+"Context_"+"$output"
    dotnet ef migrations script -i --context $context_name -o src/Persistence/Migrations/$context/Scripts/$script_name.sql  --project src/Persistence --startup-project src/Web
    Write-Output "Done: $script_name"
}
else
{
    if (![string]::IsNullOrEmpty($name))
    {
        dotnet ef migrations add $name -o "Migrations/$context" --context $context_name  --project src/Persistence --startup-project src/Web
        if ($update)
        {
           dotnet ef database update $name --context $context_name --project src/Persistence --startup-project src/Web
        }
    }
    else
    {
        if ($update)
        {
           dotnet ef database update --context $context_name --project src/Persistence --startup-project src/Web
        }
    }
}