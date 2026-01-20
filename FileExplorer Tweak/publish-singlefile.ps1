param(
    [ValidateSet("win-x64", "win-x86")]
    [string]$Runtime = "win-x64",
    [switch]$FullTrim
)

$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "FileExplorer Tweak.csproj"

$trimArgs = @()
if ($FullTrim) {
    $trimArgs = @("-p:PublishTrimmed=true", "-p:TrimMode=link")
    Write-Host "Publishing single-file Release ($Runtime) with full trim..." -ForegroundColor Cyan
} else {
    Write-Host "Publishing single-file Release ($Runtime)..." -ForegroundColor Cyan
}

dotnet publish $project -c Release -r $Runtime --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeAllContentForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    @trimArgs
