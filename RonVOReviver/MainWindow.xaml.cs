using Microsoft.Win32;
using NLog;
using RonVOReviver.Reviver;
using RonVOReviver.UI;
using System.Text;
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

        private string _messageBoxErrorCaption = (string)Application.Current.
            Resources["MainWindow.MessageBoxError.Caption"];
        private string _messageBoxFormatExceptionText = (string)Application.Current.
            Resources["MainWindow.MessageBoxFormatException.Text"];
        private string _messageBoxFolderErrorText = (string)Application.Current.
            Resources["MainWindow.MessageBoxFolderError.Text"];
        private string _messageBoxFileErrorText = (string)Application.Current.
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

        private void VOFileListOriginal_FolderSelect(object sender, RoutedEventArgs e)
        {
            Logger.Info($"Original VO Folder chosen: {VOFileListOriginal.FolderPath}");
            VOFileListOriginal.IsEnabled = false;
            VOFileListOriginal.ClearItems();

            List<string> skippedVOFiles = [];
            bool isSuccessful = _reviver.SetOriginalVOFolderPath(VOFileListOriginal.FolderPath,
                (path) => VOFileListOriginal.AddItem(path),
                (path) => skippedVOFiles.Add(path));
            if (!isSuccessful)
            {
                string message = $"{_messageBoxFolderErrorText}\n{VOFileListOriginal.FolderPath}";
                MessageBox.Show(message, _messageBoxErrorCaption);
                return;
            }
            if (skippedVOFiles.Count > 0)
            {
                string message = $"{_messageBoxFormatExceptionText}\n{String.Join("\n", skippedVOFiles)}";
                MessageBox.Show(message);
            }

            VOFileListOriginal.IsEnabled = true;
            // CheckCanRevive() is called after changing TextBoxCharacter
            TextBoxCharacter.Text = System.IO.Path.GetFileName(VOFileListOriginal.FolderPath);
            TextBlockProgress.SetResourceReference(TextBlock.TextProperty,
                "MainWindow.TextBlockProgess.LoadedOriginal.Text");
        }

        private void VOFileListModded_FolderSelect(object sender, RoutedEventArgs e)
        {
            Logger.Info($"Modded VO Folder chosen: {VOFileListModded.FolderPath}");
            VOFileListModded.IsEnabled = false;
            VOFileListModded.ClearItems();

            List<string> skippedVOFiles = [];
            List<string> failedSubtitles = [];
            bool isSuccessful = _reviver.SetModdedVOFolderPath(VOFileListModded.FolderPath,
                (path) => VOFileListModded.AddItem(path),
                (path) => skippedVOFiles.Add(path),
                (path) => failedSubtitles.Add(path));
            if (!isSuccessful)
            {
                string message = $"{_messageBoxFolderErrorText}\n{VOFileListModded.FolderPath}";
                MessageBox.Show(message, _messageBoxErrorCaption);
                return;
            }

            if (skippedVOFiles.Count > 0)
            {
                string message = $"{_messageBoxFormatExceptionText}\n{String.Join("\n", skippedVOFiles)}";
                MessageBox.Show(message, _messageBoxErrorCaption);
            }

            VOFileListModded.IsEnabled = true;
            TextBlockProgress.SetResourceReference(TextBlock.TextProperty,
                "MainWindow.TextBlockProgess.LoadedModded.Text");
            CheckCanRevive();
        }

        private void TextBoxPakName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _reviver.SetDestionationFolderPath($"{VOFileListDst.FolderPath}\\{TextBoxPakName.Text}");
            CheckCanRevive();
        }

        private void TextBoxCharacter_TextChanged(object sender, TextChangedEventArgs e)
        {
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

            List<string> FailedFiles = [];
            bool isSuccessful = _reviver.CopyVOFiles(out List<string> missingVOTypes,
                (path) => ListBoxExtra.Items.Add(path),
                (path) =>
                {
                    TextBlockProgress.Text = path;
                    VOFileListDst.AddItem(path);
                },
                (path) => FailedFiles.Add(path));
            if (!isSuccessful)
            {
                string message = $"{_messageBoxFolderErrorText}\n{VOFileListDst.FolderPath}";
                MessageBox.Show(message, _messageBoxErrorCaption);
                return;
            }

            ListBoxMissing.ItemsSource = missingVOTypes;
            if (FailedFiles.Count > 0)
            {
                string message = $"{_messageBoxFileErrorText}\n{String.Join("\n", FailedFiles)}";
                MessageBox.Show(message, _messageBoxErrorCaption);
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
