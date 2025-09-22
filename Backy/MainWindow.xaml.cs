using Backy.Classes;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text.Json;
using System.Windows;

namespace Backy
{
    public partial class MainWindow : Window
    {
        // global store for games because all the games will be saved here
        public string storePath;
        private string configFile = "config.json";

        public MainWindow()
        {
            InitializeComponent();

            var config = LoadConfig();

            storePath = config.StorePath;
            lblStorePth.Content = string.IsNullOrEmpty(storePath)
                ? "Store Location: Not Set"
                : $"Store Location: {storePath}";

            mainList.ItemsSource = config.Games;

            foreach (var g in config.Games)
            {
                g.IsBackedUp = IsGameBackedUp(g);
            }
        }

        private void addAGame_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddGameWindow
            {
                Owner = this
            };

            if (addWindow.ShowDialog() == true)
            {
                var game = addWindow.NewGame;
                game.id = mainList.Items.Count + 1; // safer unique ID
                game.storeLocation = storePath;      // set store path to store files

                var currentGames = mainList.ItemsSource as List<GameData>;
                if (currentGames == null)
                    currentGames = new List<GameData>();

                currentGames.Add(game);

                mainList.ItemsSource = null; // refresh
                mainList.ItemsSource = currentGames;

                SaveConfig(currentGames);
            }
        }

        private void StoreBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog();

            if (dialog.ShowDialog() == true)
            {
                storePath = dialog.FolderName;
                lblStorePth.Content = $"Store Location: {storePath}";
                MessageBox.Show($"Store Location set to: {storePath}", "Store Path");
            }
        }

        private void SaveConfig(List<GameData> games)
        {
            var config = new AppConfig
            {
                StorePath = storePath,
                Games = games,
            };

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configFile, json);
        }

        private AppConfig LoadConfig()
        {
            if (File.Exists(configFile))
            {
                var json = File.ReadAllText(configFile);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }

            return new AppConfig();
        }

        public static bool IsRunAsAdmin()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static void RestartAsAdmin()
        {
            var exeName = Process.GetCurrentProcess().MainModule.FileName;

            var startInfo = new ProcessStartInfo(exeName)
            {
                UseShellExecute = true,
                Verb = "runas" // triggers UAC prompt
            };

            try
            {
                Process.Start(startInfo);
            }
            catch
            {
                return; // user pressed "No" in UAC
            }

            Environment.Exit(0); // close current instance
        }

        private List<string> CopyData(string source, string dest)
        {
            List<string> skipped = new List<string>();

            try
            {
                Directory.CreateDirectory(dest);

                // Copy files
                foreach (var file in Directory.GetFiles(source))
                {
                    try
                    {
                        string destFile = Path.Combine(dest, Path.GetFileName(file));
                        File.Copy(file, destFile, true);
                    }
                    catch (Exception ex)
                    {
                        skipped.Add($"{file} → {ex.Message}");
                    }
                }

                // Copy sub-directories
                foreach (var dir in Directory.GetDirectories(source))
                {
                    try
                    {
                        string destSubDir = Path.Combine(dest, Path.GetFileName(dir));
                        skipped.AddRange(CopyData(dir, destSubDir));
                    }
                    catch (Exception ex)
                    {
                        skipped.Add($"{dir} → {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                skipped.Add($"{source} → {ex.Message}");
            }

            return skipped;
        }

        private void delBtn_Click(object sender, RoutedEventArgs e)
        {
            var currentGames = mainList.ItemsSource as List<GameData>;
            var selectedGame = mainList.SelectedItem as GameData;

            if (currentGames == null || selectedGame == null)
                return;

            // Confirm game removal
            var result = MessageBox.Show(
                $"Are you sure you want to remove '{selectedGame.name}' from the list?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            // Remove from list + save config
            currentGames.Remove(selectedGame);
            mainList.ItemsSource = null;
            mainList.ItemsSource = currentGames;
            SaveConfig(currentGames);

            // Ask if also delete from Store
            var delResult = MessageBox.Show(
                $"Do you also want to delete '{selectedGame.name}' from the Store?",
                "Delete Backup?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (delResult == MessageBoxResult.Yes)
            {
                try
                {
                    var folderName = new DirectoryInfo(selectedGame.saveLocation).Name;
                    var fullSourcePath = Path.Combine(storePath, folderName);

                    if (Directory.Exists(fullSourcePath))
                    {
                        Directory.Delete(fullSourcePath, true); // recursive
                        MessageBox.Show($"Backup for '{selectedGame.name}' deleted from the Store.", "Deleted");
                    }
                    else
                    {
                        MessageBox.Show("No backup folder found in the Store.", "Not Found");
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show($"Permission denied: {ex.Message}", "Error");
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error deleting directory: {ex.Message}", "Error");
                }
            }

            // Update indicator
            selectedGame.IsBackedUp = IsGameBackedUp(selectedGame);
            mainList.Items.Refresh();
        }
        private void BackupBtn_Click(object sender, RoutedEventArgs e)
        {
            var currentGames = mainList.ItemsSource as List<GameData>;

            if (string.IsNullOrEmpty(storePath))
            {
                MessageBox.Show("Please set a store location first.", "Store Not Set");
                return;
            }

            if (currentGames == null || mainList.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a game to backup.", "No Game Selected");
                return;
            }

            var game = mainList.SelectedItem as GameData;
            if (game == null)
            {
                MessageBox.Show("Please select a game first", "Backup Failed");
                return;
            }

            if (string.IsNullOrEmpty(game.saveLocation) || !Directory.Exists(game.saveLocation))
            {
                MessageBox.Show("No save location found for the game.", "Backup Failed");
                return;
            }

            try
            {
                var source = game.saveLocation;                          // e.g. C:\...\Blacklist
                var folderName = new DirectoryInfo(source).Name;        // "Blacklist"

                // Option A — copy into Store\Blacklist\
                var destRoot = Path.Combine(storePath, folderName);

                // Option B — copy into Store\Blacklist\2025-09-22_15-30-00 (timestamped backups)
                // var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                // var destRoot = Path.Combine(storePath, folderName, timestamp);

                // Prevent copying into itself (avoid recursive mess)
                var sourceFull = Path.GetFullPath(source).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var destFull = Path.GetFullPath(destRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (destFull.StartsWith(sourceFull, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Destination is inside the source folder. Choose a different store location.", "Invalid Destination");
                    return;
                }
                var skipped = CopyData(source, destRoot);

                if (skipped.Count == 0)
                {
                    MessageBox.Show($"Backup complete for {game.name}!", "Success");
                    game.IsBackedUp = true;
                    mainList.Items.Refresh();
                }
                else
                {
                    var message = $"Backup finished for {game.name}, but some files/folders were skipped:\n\n";
                    message += string.Join("\n", skipped.Take(10)); // show only first 10 for readability
                    if (skipped.Count > 10)
                        message += $"\n...and {skipped.Count - 10} more.";

                    MessageBox.Show(message, "Partial Backup");
                    game.IsBackedUp = true;
                    mainList.Items.Refresh();
                }
            }
            catch (UnauthorizedAccessException)
            {
                if (!IsRunAsAdmin())
                {
                    MessageBox.Show("Some files need admin rights. Restarting as administrator...", "Permission Required");
                    RestartAsAdmin();
                }
                else
                {
                    MessageBox.Show("Some files could not be accessed.", "Backup Failed");
                }
            }
        }

        private void restoreBtn_Click(object sender, RoutedEventArgs e)
        {
            var currentGames = mainList.ItemsSource as List<GameData>;

            if (string.IsNullOrEmpty(storePath))
            {
                MessageBox.Show("Please set a store location first.", "Store Not Set");
                return;
            }

            if (currentGames == null || mainList.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a backup to restore.", "No Game Selected");
                return;
            }

            var game = mainList.SelectedItem as GameData;
            if (game == null)
            {
                MessageBox.Show("Please select a backup first", "Restore Failed");
                return;
            }

            if (string.IsNullOrEmpty(game.saveLocation))
            {
                MessageBox.Show("No save location found for the game.", "Restore Failed");
                return;
            }

            try
            {
                var source = storePath;
                var dest = game.saveLocation;

                var folderName = new DirectoryInfo(dest).Name;
                var fullSourcePath = Path.Combine(source, folderName);

                var res = CopyData(fullSourcePath, dest);

                MessageBox.Show("Restore Successful ✅", "Restore Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Got an Error: {ex}");
            }
        }

        private bool IsGameBackedUp(GameData game)
        {
            if (string.IsNullOrEmpty(storePath) || string.IsNullOrEmpty(game.saveLocation))
                return false;

            var folderName = new DirectoryInfo(game.saveLocation).Name;
            var destRoot = Path.Combine(storePath, folderName);

            return Directory.Exists(destRoot);
        }
    }
}
