using Microsoft.Win32;
using NLog;
using RonVOReviver.Reviver;
using RonVOReviver.UI;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RonVOReviver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly ResourceDictionary DictionaryENUS = [];
        private static readonly ResourceDictionary DictionaryZHCN = [];
        private static readonly string DefaultPakName = "pakchunk-99_RevivedVO";
        private static readonly string RegexInvalidChars =
            $"[{String.Concat(System.IO.Path.GetInvalidPathChars())}]";

        private static string _messageBoxErrorCaption = (string)Application.Current.
            Resources["MainWindow.MessageBoxError.Caption"];
        private static string _messageBoxFormatExceptionText = (string)Application.Current.
            Resources["MainWindow.MessageBoxFormatException.Text"];
        private static string _messageBoxFolderErrorText = (string)Application.Current.
            Resources["MainWindow.MessageBoxFolderError.Text"];
        private static string _messageBoxFileErrorText = (string)Application.Current.
            Resources["MainWindow.MessageBoxFileError.Text"];

        private readonly VOReviver _reviver = new();

        public MainWindow()
        {
            InitializeComponent();
            TextBoxPakName.Text = DefaultPakName;
            DictionaryENUS.Source = new Uri("Languages/en-us.xaml", UriKind.Relative);
            DictionaryZHCN.Source = new Uri("Languages/zh-cn.xaml", UriKind.Relative);
        }

        private void ResetDynamicResourcesMessageTexts()
        {
            _messageBoxErrorCaption = (string)Application.Current.
                Resources["MainWindow.MessageBoxError.Caption"];
            _messageBoxFormatExceptionText = (string)Application.Current.
                Resources["MainWindow.MessageBoxFormatException.Text"];
            _messageBoxFolderErrorText = (string)Application.Current.
                Resources["MainWindow.MessageBoxFolderError.Text"];
            _messageBoxFileErrorText = (string)Application.Current.
                Resources["MainWindow.MessageBoxFileError.Text"];
        }

        private void CheckCanRevive()
        {
            ButtonRevive.IsEnabled = false;
            if (VOFileListOriginal.FolderPath.Equals(string.Empty) ||
                VOFileListModded.FolderPath.Equals(string.Empty) ||
                VOFileListDst.FolderPath.Equals(string.Empty) ||
                TextBoxPakName.Text.Equals(string.Empty) ||
                TextBoxCharacter.Text.Equals(string.Empty))
            {
                return;
            }
            ButtonRevive.IsEnabled = true;
        }

        public static void ShowErrorMessageBox(string text)
        {
            MessageBox.Show(text, _messageBoxErrorCaption, MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        public static void ShowWarningMessageBox(string text)
        {
            MessageBox.Show(text, _messageBoxErrorCaption,MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private void VOFileListOriginal_FolderSelect(object sender, RoutedEventArgs e)
        {
            Logger.Info($"Original VO Folder chosen: {VOFileListOriginal.FolderPath}");
            VOFileListOriginal.IsEnabled = false;
            VOFileListOriginal.ClearItems();

            List<string> skippedVOFiles = [];
            try
            {
                _reviver.SetOriginalVOFolderPath(VOFileListOriginal.FolderPath,
                    (path) => VOFileListOriginal.AddItem(path),
                    (path) => skippedVOFiles.Add(path));

                if (skippedVOFiles.Count > 0)
                {
                    string message = $"{_messageBoxFormatExceptionText}\n{String.Join("\n", skippedVOFiles)}";
                    MessageBox.Show(message);
                }

                VOFileListOriginal.IsEnabled = true;
                TextBoxCharacter.Text = System.IO.Path.GetFileName(VOFileListOriginal.FolderPath);
                TextBlockProgress.SetResourceReference(TextBlock.TextProperty,
                    "MainWindow.TextBlockProgess.LoadedOriginal.Text");
            }
            catch (UnauthorizedAccessException ex)
            {
                VOFileListOriginal.FolderPath = string.Empty;
                string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
                ShowWarningMessageBox(message);
            }
            catch (IOException ex)
            {
                VOFileListOriginal.FolderPath = string.Empty;
                string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
                ShowWarningMessageBox(message);
            }
            
            CheckCanRevive();
        }

        private void VOFileListModded_FolderSelect(object sender, RoutedEventArgs e)
        {
            Logger.Info($"Modded VO Folder chosen: {VOFileListModded.FolderPath}");
            VOFileListModded.IsEnabled = false;
            VOFileListModded.ClearItems();

            List<string> skippedVOFiles = [];
            List<string> failedSubtitles = [];
            try
            {
                _reviver.SetModdedVOFolderPath(VOFileListModded.FolderPath,
                    (path) => VOFileListModded.AddItem(path),
                    (path) => skippedVOFiles.Add(path));

                if (skippedVOFiles.Count > 0)
                {
                    string message = $"{_messageBoxFormatExceptionText}\n{String.Join("\n", skippedVOFiles)}";
                    MessageBox.Show(message, _messageBoxErrorCaption);
                }

                VOFileListModded.IsEnabled = true;
                TextBlockProgress.SetResourceReference(TextBlock.TextProperty,
                    "MainWindow.TextBlockProgess.LoadedModded.Text");
            }
            catch (UnauthorizedAccessException ex)
            {
                VOFileListModded.FolderPath = string.Empty;
                string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
                ShowWarningMessageBox(message);
            }
            catch (IOException ex)
            {
                VOFileListModded.FolderPath = string.Empty;
                string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
                ShowWarningMessageBox(message);
            }

            CheckCanRevive();
        }

        private void TextBoxPakName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxPakName.Text = Regex.Replace(TextBoxPakName.Text, RegexInvalidChars, string.Empty);
            _reviver.SetDestionationFolderPath($"{VOFileListDst.FolderPath}\\{TextBoxPakName.Text}");
            CheckCanRevive();
        }

        private void TextBoxCharacter_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxCharacter.Text = Regex.Replace(TextBoxCharacter.Text, RegexInvalidChars, string.Empty);
            _reviver.Character = TextBoxCharacter.Text;
            CheckCanRevive();
        }

        private void VOFileListDst_FolderSelect(object sender, RoutedEventArgs e)
        {
            _reviver.SetDestionationFolderPath($"{VOFileListDst.FolderPath}\\{TextBoxPakName.Text}");
            CheckCanRevive();
        }

        private void ButtonRevive_Click(object sender, RoutedEventArgs e)
        {
            ButtonRevive.IsEnabled = false;
            VOFileListDst.ClearItems();

            ListBoxExtra.Items.Clear();
            List<string> FailedFiles = [];
            try
            {
                _reviver.CopyVOFiles(out List<string> missingVOTypes,
                    (path) => ListBoxExtra.Items.Add(path),
                    (path) =>
                    {
                        TextBlockProgress.Text = path;
                        VOFileListDst.AddItem(path);
                    },
                    (path) => FailedFiles.Add(path));

                ListBoxMissing.ItemsSource = missingVOTypes;
                if (FailedFiles.Count > 0)
                {
                    string message = $"{_messageBoxFileErrorText}\n{String.Join("\n", FailedFiles)}";
                    MessageBox.Show(message, _messageBoxErrorCaption);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                VOFileListDst.FolderPath = string.Empty;
                string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
                ShowWarningMessageBox(message);
            }
            catch (IOException ex)
            {
                VOFileListDst.FolderPath = string.Empty;
                string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
                ShowWarningMessageBox(message);
            }

            CheckCanRevive();
        }

        #region languages

        private void ButtonENUS_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources.MergedDictionaries[0] = DictionaryENUS;
            ResetDynamicResourcesMessageTexts();
        }

        private void ButtonZHCN_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources.MergedDictionaries[0] = DictionaryZHCN;
            ResetDynamicResourcesMessageTexts();
        }

        #endregion
    }
}
