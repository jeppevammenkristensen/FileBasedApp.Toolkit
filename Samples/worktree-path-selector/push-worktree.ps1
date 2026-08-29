function Push-Worktree {
    <#
    .SYNOPSIS
    Interactively selects a Git worktree and enters its directory.

    .DESCRIPTION
    Runs the globally installed work-path-selector tool, reads the selected
    worktree path, and adds the current location to the PowerShell location
    stack before entering the selected directory. Use Pop-Location to return.

    .PARAMETER Directory
    The directory from which to search for a Git repository. When omitted, the
    selector uses the current PowerShell directory.

    .EXAMPLE
    Push-Worktree

    Selects a worktree starting from the current directory.

    .EXAMPLE
    Push-Worktree -Directory C:\code

    Selects a worktree starting from C:\code.

    .NOTES
    Requires the work-path-selector .NET tool to be installed. Copy this
    function into $PROFILE to make it available in every PowerShell session. 
    You can edit $PROFILE from powershell. 
    For instance with code $PROFILE (assuming visual studio code is installed)
    #>
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [string] $Directory
    )

    $selectorArguments = if ($PSBoundParameters.ContainsKey('Directory')) { @($Directory) } else { @() }
    & worktree-path-selector @selectorArguments
    if ($LASTEXITCODE -ne 0) {
        Write-Error "worktree-path-selector failed with exit code $LASTEXITCODE."
        return
    }

    $selectedPath = (Get-Clipboard -Raw).Trim()
    if (-not [System.IO.Directory]::Exists($selectedPath)) {
        Write-Error "The selected worktree directory does not exist: $selectedPath"
        return
    }

    Push-Location -LiteralPath $selectedPath
}

Push-Worktree
