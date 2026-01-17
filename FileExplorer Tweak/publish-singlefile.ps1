param(
    [ValidateSet("win-x64", "win-x86")]
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "FileExplorer Tweak.csproj"

Write-Host "Publishing single-file Release ($Runtime)..." -ForegroundColor Cyan
dotnet publish $project -c Release -r $Runtime --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeAllContentForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true
