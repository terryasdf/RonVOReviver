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

namespace RonVOReviver;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly ResourceDictionary DictionaryENUS = [];
    private static readonly ResourceDictionary DictionaryZHCN = [];
    private const string DefaultPakName = "pakchunk99-RevivedVO";
    private const string ZFillPreviewVOType = "PreviewVO";
    private const int ZFillPreviewIndex1 = 1;
    private const int ZFillPreviewIndex2 = 12;
    private static readonly string RegexInvalidChars =
        $"[{string.Concat(System.IO.Path.GetInvalidFileNameChars())}]";

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
        UpdateZeroFill();
        DictionaryENUS.Source = new Uri("Languages/en-us.xaml", UriKind.Relative);
        DictionaryZHCN.Source = new Uri("Languages/zh-cn.xaml", UriKind.Relative);
    }

    private static void ResetDynamicResourcesMessageTexts()
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

    private void UpdateZeroFill()
    {
        _reviver.ZeroFillLength = NumericUpDownZFill.Value;
        TextBlockZFillPreview1.Text = $"{ZFillPreviewVOType}_{_reviver.ZeroFill(ZFillPreviewIndex1)}.ogg";
        TextBlockZFillPreview2.Text = $"{ZFillPreviewVOType}_{_reviver.ZeroFill(ZFillPreviewIndex2)}.ogg";
    }

    public static void ShowErrorMessageBox(string text)
    {
        MessageBox.Show(text, _messageBoxErrorCaption, MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    public static void ShowWarningMessageBox(string text)
    {
        MessageBox.Show(text, _messageBoxErrorCaption, MessageBoxButton.OK,
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
            NumericUpDownZFill.Value = _reviver.ZeroFillLength;
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
    }

    private void TextBoxPakName_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBoxPakName.Text = Regex.Replace(TextBoxPakName.Text, RegexInvalidChars, string.Empty);
        _reviver.SetDestionationFolderPath($"{VOFileListDst.FolderPath}\\{TextBoxPakName.Text}");
    }

    private void TextBoxCharacter_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBoxCharacter.Text = Regex.Replace(TextBoxCharacter.Text, RegexInvalidChars, string.Empty);
        _reviver.Character = TextBoxCharacter.Text;
    }

    private void VOFileListDst_FolderSelect(object sender, RoutedEventArgs e)
    {
        _reviver.SetDestionationFolderPath($"{VOFileListDst.FolderPath}\\{TextBoxPakName.Text}");
    }

    private void ZFillUpdateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void ZFillUpdateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        UpdateZeroFill();
    }

    private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (VOFileListOriginal.FolderPath.Equals(string.Empty) ||
            VOFileListModded.FolderPath.Equals(string.Empty) ||
            VOFileListDst.FolderPath.Equals(string.Empty) ||
            TextBoxPakName.Text.Equals(string.Empty) ||
            TextBoxCharacter.Text.Equals(string.Empty))
        {
            e.CanExecute = false;
            return;
        }
        e.CanExecute = true;
    }

    private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
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

            _reviver.PakVOFiles();
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
    }

    private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = !VOFileListDst.FolderPath.Equals(string.Empty);
    }

    private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            _reviver.PakVOFiles();
        }
        catch (DirectoryNotFoundException ex)
        {
            string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
            ShowErrorMessageBox(message);
        }
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

public static class NumericUpDownZFillCommands
{
    public static readonly RoutedUICommand Update = new(
        "Update", "Update", typeof(NumericUpDownZFillCommands));
}