using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Text.Json;
using CoreLogic.Models;
using CoreLogic.Services;

namespace LocalChatUI.Desktop
{
    public partial class MainWindow : Window
    {
        private bool isDarkMode = false;
        private readonly string _configPath;
        private readonly string _defaultConfigPath;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize config paths
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _configPath = Path.Combine(baseDirectory, "server_config.json");
            _defaultConfigPath = Path.Combine(baseDirectory, "server_config.json.default");

            // Set initial theme icon
            ThemeToggle.Content = new System.Windows.Controls.TextBlock
            {
                Text = "\U0001F319", // Moon emoji
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(31, 31, 31)) // Dark gray for light mode
            };

            ApplyLightTheme(); // Start with light theme

            // Auto-populate server dropdowns on startup
            LoadServerDropdowns();
        }

        private void LoadServerDropdowns()
        {
            try
            {
                var config = ConfigService.LoadServerConfig(_configPath);
                if (config == null)
                    return;

                // Populate the 3 dropdown comparisons
                ModernServerComboBox.ItemsSource = config.Servers;
                MinimalistServerComboBox.ItemsSource = config.Servers;
                MaterialServerComboBox.ItemsSource = config.Servers;

                // Set active server as selected in all 3 dropdowns
                if (!string.IsNullOrEmpty(config.ActiveServer))
                {
                    var activeServer = config.Servers.Find(s => s.Name == config.ActiveServer);
                    if (activeServer != null)
                    {
                        ModernServerComboBox.SelectedItem = activeServer;
                        MinimalistServerComboBox.SelectedItem = activeServer;
                        MaterialServerComboBox.SelectedItem = activeServer;
                    }
                }
            }
            catch
            {
                // Silently fail on startup if config doesn't exist
            }
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            isDarkMode = !isDarkMode;

            if (isDarkMode)
            {
                ApplyDarkTheme();
                ThemeToggle.Content = new System.Windows.Controls.TextBlock
                {
                    Text = "\u2600\uFE0F", // Sun emoji
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)) // Very light gray for dark mode
                };
            }
            else
            {
                ApplyLightTheme();
                ThemeToggle.Content = new System.Windows.Controls.TextBlock
                {
                    Text = "\U0001F319", // Moon emoji
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(31, 31, 31)) // Dark gray for light mode
                };
            }
        }

        private void ApplyLightTheme()
        {
            Resources["BackgroundBrush"] = Resources["LightBackground"];
            Resources["SurfaceBrush"] = Resources["LightSurface"];
            Resources["BorderBrush"] = Resources["LightBorder"];
            Resources["TextBrush"] = Resources["LightText"];
            Resources["TextSecondaryBrush"] = Resources["LightTextSecondary"];
            Resources["AccentBrush"] = Resources["LightAccent"];
            Resources["AccentHoverBrush"] = Resources["LightAccentHover"];
            Resources["ButtonHoverBrush"] = Resources["LightButtonHover"];

            this.Background = (SolidColorBrush)Resources["LightBackground"];

            // Update theme icon color for light mode
            if (ThemeToggle.Content is System.Windows.Controls.TextBlock iconBlock)
            {
                iconBlock.Foreground = new SolidColorBrush(Color.FromRgb(31, 31, 31)); // Dark gray for light mode
            }
        }

        private void ApplyDarkTheme()
        {
            Resources["BackgroundBrush"] = Resources["DarkBackground"];
            Resources["SurfaceBrush"] = Resources["DarkSurface"];
            Resources["BorderBrush"] = Resources["DarkBorder"];
            Resources["TextBrush"] = Resources["DarkText"];
            Resources["TextSecondaryBrush"] = Resources["DarkTextSecondary"];
            Resources["AccentBrush"] = Resources["DarkAccent"];
            Resources["AccentHoverBrush"] = Resources["DarkAccentHover"];
            Resources["ButtonHoverBrush"] = Resources["DarkButtonHover"];

            this.Background = (SolidColorBrush)Resources["DarkBackground"];

            // Update theme icon color for dark mode
            if (ThemeToggle.Content is System.Windows.Controls.TextBlock iconBlock)
            {
                iconBlock.Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)); // Very light gray for dark mode
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string inputText = InputTextBox.Text;
            int wordCount = ConfigService.CountWords(inputText);

            WordCountLabel.Text = $"Word Count: {wordCount}";
            OriginalTextDisplay.Text = inputText;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            InputTextBox.Clear();
            WordCountLabel.Text = "";
            OriginalTextDisplay.Text = "";
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = ConfigService.LoadServerConfig(_configPath);
                if (config == null)
                    return;

                string formattedDisplay = ConfigService.FormatServerDisplay(config);
                OriginalTextDisplay.Text = formattedDisplay;
                WordCountLabel.Text = $"Server Count: {config.Servers.Count}";

                // Refresh dropdowns
                LoadServerDropdowns();
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Invalid JSON format: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = ConfigService.LoadServerConfig(_configPath);

                string? firstServerName = ConfigService.GetFirstServerName(config);
                if (firstServerName == null)
                {
                    MessageBox.Show("No servers to delete.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Show confirmation dialog (UI layer responsibility)
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{firstServerName}'?",
                    "Confirm Delete",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Exclamation);

                if (result == MessageBoxResult.OK)
                {
                    ConfigService.DeleteFirstServer(config!);
                    ConfigService.SaveServerConfig(config!, _configPath);

                    // Reload and display updated list
                    string formattedDisplay = ConfigService.FormatServerDisplay(config);
                    OriginalTextDisplay.Text = formattedDisplay;
                    WordCountLabel.Text = $"Server Count: {config!.Servers.Count}";

                    // Refresh dropdowns
                    LoadServerDropdowns();
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConfigService.RestoreDefaultConfig(_defaultConfigPath, _configPath);
                MessageBox.Show("Configuration restored to default.", "Success", MessageBoxButton.OK, MessageBoxImage.None);

                // Reload and display default servers
                var config = ConfigService.LoadServerConfig(_configPath);
                if (config != null)
                {
                    string formattedDisplay = ConfigService.FormatServerDisplay(config);
                    OriginalTextDisplay.Text = formattedDisplay;
                    WordCountLabel.Text = $"Server Count: {config.Servers.Count}";

                    // Refresh dropdowns
                    LoadServerDropdowns();
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restoring configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
