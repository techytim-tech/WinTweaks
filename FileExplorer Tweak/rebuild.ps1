$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "FileExplorer Tweak.csproj"

Write-Host "Cleaning..." -ForegroundColor Cyan
dotnet clean $project

Write-Host "Building..." -ForegroundColor Cyan
dotnet build $project

if ($args -contains "--run") {
    Write-Host "Running..." -ForegroundColor Cyan
    dotnet run --project $project
}
