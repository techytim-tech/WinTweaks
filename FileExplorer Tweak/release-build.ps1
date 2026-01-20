param(
    [switch]$FullTrim
)

$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "FileExplorer Tweak.csproj"

$runtimes = @(
    "win-x86",
    "win-x64"
)

$trimArgs = @()
if ($FullTrim) {
    $trimArgs = @("-p:PublishTrimmed=true", "-p:TrimMode=link")
}

foreach ($rid in $runtimes) {
    $label = $FullTrim ? "with full trim" : "standard"
    Write-Host "Publishing Release for Windows 11 ($rid) ($label)..." -ForegroundColor Cyan
    dotnet publish $project -c Release -r $rid --self-contained true @trimArgs
}
