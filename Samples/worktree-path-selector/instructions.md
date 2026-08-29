# Install Push-Worktree with PowerShell

These instructions are intended for an AI agent. Windows is the primary target, but Linux and macOS are also supported when the additional requirements below are met. Use PowerShell 7 or later (`pwsh`) for every PowerShell command.

## Goal

Install the `worktree-path-selector` .NET global tool and add the `Push-Worktree` PowerShell function to the current user's `$PROFILE`.

## Prerequisites

Confirm that `git`, `dotnet`, and PowerShell 7 are available:

```powershell
git --version
dotnet --version
$PSVersionTable.PSVersion
```

Stop and report the missing prerequisite if any command is unavailable. Do not run downloaded scripts before inspecting them.

### Linux and macOS requirements

The installation and `$PROFILE` steps work on Windows, Linux, and macOS. Apply these platform-specific requirements:

- Linux requires a graphical desktop session with a working X11 clipboard. Install both `xsel`, which the tool's TextCopy dependency uses, and `xclip`, which PowerShell's `Get-Clipboard` command uses. For example, run `sudo apt-get install xsel xclip` on Debian or Ubuntu, `sudo dnf install xsel xclip` on Fedora, or `sudo pacman -S xsel xclip` on Arch Linux. Clipboard operations are not expected to work in a headless session without an available display and clipboard.
- macOS uses the built-in `pbcopy` and `pbpaste` utilities. Verify both are available with `Get-Command pbcopy, pbpaste`; no additional clipboard package is normally required.
- Do not run `Unblock-File` on Linux or macOS. That step only applies to files downloaded on Windows.
- Use Unix paths such as `$HOME/code` instead of Windows paths such as `C:\code`.
- A .NET global tool is installed under `$HOME/.dotnet/tools`. Ensure that directory is on `PATH`. If it is missing, add the following idempotent block to `$PROFILE` before the `Push-Worktree` function:

```powershell
$dotnetTools = Join-Path $HOME '.dotnet/tools'
if (($env:PATH -split [System.IO.Path]::PathSeparator) -notcontains $dotnetTools) {
    $env:PATH = $dotnetTools + [System.IO.Path]::PathSeparator + $env:PATH
}
```

## Obtain the files

The required files are in the same folder as this document:

- `worktree-path-selector.cs`
- `install.ps1`
- `push-worktree.ps1`

Choose the applicable case.

### If reading this on GitHub

Download the contents of this folder to a persistent local directory. Do not use a temporary directory because `$PROFILE` will contain the installed function.

Prefer cloning the repository at the branch or commit currently shown on GitHub, then use the local `Samples\worktree-path-selector` folder. If a full clone is not appropriate, download the repository ZIP for that same branch or commit and extract it. Verify that all three required files listed above exist together before continuing.

Do not download only `instructions.md`, and do not execute a script directly from a remote URL.

### If the repository is already cloned

Do not download another copy. Locate this document in the clone and use its containing directory:

```powershell
$ToolDirectory = Split-Path -Parent (Resolve-Path -LiteralPath '.\instructions.md')
```

If the current directory is elsewhere, set `$ToolDirectory` to the absolute path of the cloned `Samples\worktree-path-selector` directory instead. Confirm the inputs:

```powershell
@('worktree-path-selector.cs', 'install.ps1', 'push-worktree.ps1') | ForEach-Object {
    $path = Join-Path $ToolDirectory $_
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required file is missing: $path"
    }
}
```

## Install the tool

On Windows only, if the files were downloaded from GitHub as a ZIP, unblock the downloaded files first:

```powershell
Get-ChildItem -LiteralPath $ToolDirectory -File | Unblock-File
```

Run the installer from its own directory:

```powershell
& (Join-Path $ToolDirectory 'install.ps1')
```

The installer packs `worktree-path-selector.cs`, removes an existing global installation of the same package if present, and installs the newly packed version as a .NET global tool.

Verify the installation before changing `$PROFILE`:

```powershell
dotnet tool list --global
worktree-path-selector --help
```

The global tool list must contain `worktree-path-selector`, and the help command must exit successfully.

## Add Push-Worktree to `$PROFILE`

Read `push-worktree.ps1` and add its `Push-Worktree` **function definition only** to the current user's current-host profile. Do not copy the final standalone `Push-Worktree` line from the script: that line invokes the function immediately and would launch the selector whenever PowerShell starts.

Create the profile file if needed:

```powershell
$profileDirectory = Split-Path -Parent $PROFILE
if (-not (Test-Path -LiteralPath $profileDirectory -PathType Container)) {
    New-Item -ItemType Directory -Path $profileDirectory -Force | Out-Null
}
if (-not (Test-Path -LiteralPath $PROFILE -PathType Leaf)) {
    New-Item -ItemType File -Path $PROFILE -Force | Out-Null
}
```

Inspect the existing profile before editing it. Preserve all existing content. If a `Push-Worktree` function already exists, replace that function rather than adding a duplicate. Copy the complete function from `push-worktree.ps1`, starting at `function Push-Worktree {` and ending at its matching closing brace, into `$PROFILE`.

After editing, load and verify the profile in the current session:

```powershell
. $PROFILE
Get-Command Push-Worktree -CommandType Function -ErrorAction Stop
```

## Test

From a directory containing a Git repository or its worktree directories, run:

```powershell
Push-Worktree
```

Alternatively, pass a starting directory:

```powershell
Push-Worktree -Directory C:\code
```

On Linux or macOS, use a Unix path instead:

```powershell
Push-Worktree -Directory $HOME/code
```

Select a worktree and verify that the current location changes to it. Run `Pop-Location` to return to the previous directory.

Report the installed tool version, the profile path changed, and whether the final test succeeded.
