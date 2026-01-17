# FileExplorer Tweak

FileExplorer Tweak is a small Windows utility that enables the `FolderType` registry value and sets it to `NotSpecified` under:
`HKCU\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell`.
This can improve File Explorer responsiveness by preventing automatic folder type detection.


## How to build
1. Run the app with `build-and-run.ps1` (requires the .NET SDK installed).
2. Optional: use `release-build.ps1` to publish release builds.

## How to use
A release executable will be added to GitHub, and this section will explain how to run it.

![FileExplorer Tweak UI](FileExplorer%20Tweak_screenshot.png)

## Credit
This idea came from this article: <a href="https://www.makeuseof.com/i-fixed-windows-11-file-explorer-lag-by-disabling-this-old-service/" target="_blank" rel="noopener noreferrer">makeuseof.com</a>
Reading it led me to create an easy tool to apply the tweak.

