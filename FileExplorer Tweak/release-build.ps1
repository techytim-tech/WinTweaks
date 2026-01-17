$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "FileExplorer Tweak.csproj"

$runtimes = @(
    "win-x86",
    "win-x64"
)

foreach ($rid in $runtimes) {
    Write-Host "Publishing Release for Windows 11 ($rid)..." -ForegroundColor Cyan
    dotnet publish $project -c Release -r $rid --self-contained true
}
