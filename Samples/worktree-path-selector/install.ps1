$ErrorActionPreference = 'Stop'

$packageId = 'worktree-path-selector'
#$version = '1.0.1'
$sourcePath = Join-Path $PSScriptRoot 'worktree-path-selector.cs'
$artifactsPath = Join-Path $PSScriptRoot 'artifacts'

New-Item -ItemType Directory -Path $artifactsPath -Force | Out-Null

dotnet pack $sourcePath --configuration Release --output $artifactsPath
if ($LASTEXITCODE -ne 0) {
    throw "Failed to pack $sourcePath."
}

$installedPackageIds = dotnet tool list --global | Select-Object -Skip 2 | ForEach-Object {
    ($_ -split '\s+')[0]
}

if ($installedPackageIds -contains $packageId) {
    dotnet tool uninstall --global $packageId
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to uninstall the existing $packageId tool."
    }
}

dotnet tool install --global $packageId --add-source $artifactsPath
if ($LASTEXITCODE -ne 0) {
    throw "Failed to install $packageId from $artifactsPath."
}
