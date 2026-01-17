using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

namespace FileExplorer_Tweak
{
    public partial class MainWindow : Window    
    {
        private const string ShellBagsPath =
            @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell";
        private const string FolderTypeValueName = "FolderType";
        private const string FolderTypeValueData = "NotSpecified";
        private readonly TextBlock _statusText;

        public MainWindow()
        {
            InitializeComponent();

            var backup = this.FindControl<Button>("BackupButton");
            var enable = this.FindControl<Button>("EnableTweakButton");
            var remove = this.FindControl<Button>("RemoveTweakButton");
            var restore = this.FindControl<Button>("RestoreBackupButton");
            var linkButton = this.FindControl<Button>("LinkButton");
            _statusText = this.FindControl<TextBlock>("StatusText")
                ?? throw new InvalidOperationException("Control lookup failed; verify XAML names and BuildAction.");

            if (backup is null || enable is null || remove is null || restore is null || linkButton is null)
                throw new InvalidOperationException("Control lookup failed; verify XAML names and BuildAction.");

            backup.Click += async (_, _) => await OnBackupClicked();
            enable.Click += async (_, _) => await OnEnableTweakClicked();
            remove.Click += async (_, _) => await OnRemoveTweakClicked();
            restore.Click += async (_, _) => await OnRestoreBackupClicked();
            linkButton.Click += (_, _) => OnLinkClicked();

            SetWindowTitle();
        }

        private void SetWindowTitle()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionText = version is null ? "Unknown" : $"{version.Major}.{version.Minor}.{version.Build}";
            Title = $"FileExplorer Tweak v{versionText}";
        }

        private void OnLinkClicked()
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "https://techys.cc",
                    UseShellExecute = true
                };
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open link: {ex.Message}");
            }
        }

        private Task OnRemoveTweakClicked()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(ShellBagsPath, writable: true);
                if (key is null)
                {
                    UpdateStatus("Already removed.");
                    return Task.CompletedTask;
                }

                var currentValue = key.GetValue(FolderTypeValueName) as string;
                if (string.IsNullOrEmpty(currentValue))
                {
                    UpdateStatus("Already removed.");
                    return Task.CompletedTask;
                }

                key.DeleteValue(FolderTypeValueName, throwOnMissingValue: false);
                RestartWindowsExplorer();
                UpdateStatus("Tweak removed. Explorer restarted.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to remove tweak: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        private Task OnEnableTweakClicked()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(ShellBagsPath, writable: true);
                if (key is null)
                {
                    UpdateStatus("Unable to open registry path.");
                    return Task.CompletedTask;
                }

                var currentValue = key.GetValue(FolderTypeValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                if (currentValue is string textValue &&
                    string.Equals(textValue, FolderTypeValueData, StringComparison.OrdinalIgnoreCase))
                {
                    UpdateStatus("Already enabled.");
                    return Task.CompletedTask;
                }

                key.SetValue(FolderTypeValueName, FolderTypeValueData, RegistryValueKind.String);
                RestartWindowsExplorer();
                UpdateStatus("Tweak enabled. Explorer restarted.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to enable tweak: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        private void UpdateStatus(string message)
        {
            _statusText.Text = message;
        }

        private void RestartWindowsExplorer()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("explorer"))
                {
                    process.Kill(true);
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Tweak applied, but failed to restart Explorer: {ex.Message}");
            }
        }

        private async Task OnBackupClicked()
        {
            try
            {
                if (StorageProvider is null)
                {
                    UpdateStatus("File picker unavailable.");
                    return;
                }

                var suggestedName = GetBackupFileName();
                var saveResult = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Registry Backup",
                    SuggestedFileName = suggestedName,
                    DefaultExtension = ".reg",
                    ShowOverwritePrompt = true
                });

                if (saveResult is null)
                {
                    UpdateStatus("Backup canceled.");
                    return;
                }

                var filePath = saveResult.Path.LocalPath;
                await ExportRegistryAsync(filePath);
                UpdateStatus("Backup saved.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to create backup: {ex.Message}");
            }
        }

        private async Task OnRestoreBackupClicked()
        {
            try
            {
                if (StorageProvider is null)
                {
                    UpdateStatus("File picker unavailable.");
                    return;
                }

                var openResult = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Registry Backup",
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("Registry Backup")
                        {
                            Patterns = ["*.reg"]
                        }
                    ]
                });

                if (openResult is null || openResult.Count == 0)
                {
                    UpdateStatus("Restore canceled.");
                    return;
                }

                var filePath = openResult[0].Path.LocalPath;
                RemoveTweakValueIfExists();
                await ImportRegistryAsync(filePath);
                RestartWindowsExplorer();
                UpdateStatus("Backup restored. Explorer restarted.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to restore backup: {ex.Message}");
            }
        }

        private static string GetBackupFileName()
        {
            var format = ResolveDateStampFormat(CultureInfo.CurrentCulture);
            var dateStamp = DateTime.Now.ToString(format, CultureInfo.InvariantCulture);
            return $"ShellTweakBackup_{dateStamp}.reg";
        }

        private static string ResolveDateStampFormat(CultureInfo culture)
        {
            var pattern = culture.DateTimeFormat.ShortDatePattern ?? string.Empty;
            var trimmed = pattern.TrimStart();
            return trimmed.StartsWith("M", StringComparison.OrdinalIgnoreCase) ? "MM_dd_yyyy" : "dd_MM_yyyy";
        }

        private static Task ExportRegistryAsync(string filePath)
        {
            var args = $"export \"HKCU\\{ShellBagsPath}\" \"{filePath}\" /y";
            var startInfo = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            if (process is null)
            {
                throw new InvalidOperationException("Failed to start registry export.");
            }

            return process.WaitForExitAsync();
        }

        private static async Task ImportRegistryAsync(string filePath)
        {
            var args = $"import \"{filePath}\"";
            var startInfo = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            if (process is null)
            {
                throw new InvalidOperationException("Failed to start registry import.");
            }

            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Registry import failed with exit code {process.ExitCode}.");
            }
        }

        private static void RemoveTweakValueIfExists()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(ShellBagsPath, writable: true);
                if (key is null)
                {
                    return;
                }

                key.DeleteValue(FolderTypeValueName, throwOnMissingValue: false);
            }
            catch
            {
                // Best-effort cleanup before import; failures shouldn't block restore.
            }
        }
    }
}
