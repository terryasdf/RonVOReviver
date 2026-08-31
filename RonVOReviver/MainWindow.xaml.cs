using NLog;
using RonVOReviver.Reviver;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
    private static readonly string RegexInvalidChars =
        $"[{string.Concat(Path.GetInvalidFileNameChars())} ]";

    private static string _messageBoxErrorCaption = string.Empty;
    private static string _messageBoxFormatExceptionText = string.Empty;
    private static string _messageBoxFolderErrorText = string.Empty;
    private static string _messageBoxFileErrorText = string.Empty;

    private VOManager? _originalVOManager;
    private ModdedVOManager? _moddedVOManager;
    private bool _isProcessing = false;

    public MainWindow()
    {
        InitializeComponent();
        Title = $"RON VO Reviver (by terryzzz) {Application.ResourceAssembly.GetName().Version}";
        TextBoxPakName.Text = DefaultPakName;
        DictionaryENUS.Source = new Uri("Languages/en-us.xaml", UriKind.Relative);
        DictionaryZHCN.Source = new Uri("Languages/zh-cn.xaml", UriKind.Relative);
        ResetDynamicResourcesMessageTexts();
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
        _originalVOManager = null;

        List<string> skippedVOFiles = [];
        try
        {
            var progress = new Progress<VOManagerProgressReport>(report =>
            {
                switch (report.Type)
                {
                    case VOManagerProgressType.Success:
                        VOFileListOriginal.AddItem(report.Path);
                        break;
                    case VOManagerProgressType.FormatError:
                        skippedVOFiles.Add(report.Path);
                        break;
                }
            });
            _originalVOManager = new VOManager(VOFileListOriginal.FolderPath, progress);

            if (skippedVOFiles.Count > 0)
            {
                string message = $"{_messageBoxFormatExceptionText}\n{String.Join("\n", skippedVOFiles)}";
                ShowWarningMessageBox(message);
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
            ShowErrorMessageBox(message);
        }
        catch (IOException ex)
        {
            VOFileListOriginal.FolderPath = string.Empty;
            string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
            ShowErrorMessageBox(message);
        }
    }

    private void VOFileListModded_FolderSelect(object sender, RoutedEventArgs e)
    {
        Logger.Info($"Modded VO Folder chosen: {VOFileListModded.FolderPath}");
        VOFileListModded.IsEnabled = false;
        VOFileListModded.ClearItems();
        _moddedVOManager = null;

        List<string> skippedVOFiles = [];
        try
        {
            var progress = new Progress<VOManagerProgressReport>(report =>
            {
                switch (report.Type)
                {
                    case VOManagerProgressType.Success:
                        VOFileListModded.AddItem(report.Path);
                        break;
                    case VOManagerProgressType.FormatError:
                        skippedVOFiles.Add(report.Path);
                        break;
                }
            });
            _moddedVOManager = new ModdedVOManager(VOFileListModded.FolderPath, progress);

            if (skippedVOFiles.Count > 0)
            {
                string message = $"{_messageBoxFormatExceptionText}\n{String.Join("\n", skippedVOFiles)}";
                ShowWarningMessageBox(message);
            }

            VOFileListModded.IsEnabled = true;
            TextBlockProgress.SetResourceReference(TextBlock.TextProperty,
                "MainWindow.TextBlockProgess.LoadedModded.Text");
        }
        catch (UnauthorizedAccessException ex)
        {
            VOFileListModded.FolderPath = string.Empty;
            string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
            ShowErrorMessageBox(message);
        }
        catch (IOException ex)
        {
            VOFileListModded.FolderPath = string.Empty;
            string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
            ShowErrorMessageBox(message);
        }
    }

    private void TextBoxPakName_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBoxPakName.Text = Regex.Replace(TextBoxPakName.Text, RegexInvalidChars, string.Empty);
    }

    private void TextBoxCharacter_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBoxCharacter.Text = Regex.Replace(TextBoxCharacter.Text, RegexInvalidChars, string.Empty);
    }

    private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (_isProcessing ||
            _originalVOManager == null ||
            _moddedVOManager == null ||
            String.IsNullOrEmpty(VOFileListOriginal.FolderPath) ||
            String.IsNullOrEmpty(VOFileListModded.FolderPath) ||
            String.IsNullOrEmpty(VOFileListDst.FolderPath) ||
            String.IsNullOrEmpty(TextBoxPakName.Text) ||
            String.IsNullOrEmpty(TextBoxCharacter.Text))
        {
            e.CanExecute = false;
            return;
        }
        e.CanExecute = true;
    }

    private async void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (_originalVOManager == null || _moddedVOManager == null)
        {
            return;
        }

        Debug.Assert(!String.IsNullOrEmpty(VOFileListDst.FolderPath));
        Debug.Assert(!String.IsNullOrEmpty(TextBoxPakName.Text));

        _isProcessing = true;
        CommandManager.InvalidateRequerySuggested();
        VOFileListDst.ClearItems();

        VOFileListMissing.ClearItems();
        VOFileListExtra.ClearItems();
        List<string> FailedFiles = [];

        try
        {
            string destinationFolderPath = Path.Combine(VOFileListDst.FolderPath, TextBoxPakName.Text);
            VOReviver reviver = new(
                _originalVOManager,
                _moddedVOManager,
                destinationFolderPath,
                TextBoxCharacter.Text);

            var progress = new Progress<VOProgressReport>(report =>
            {
                switch (report.Type)
                {
                    case VOProgressType.FileCopied:
                        TextBlockProgress.Text = report.Path;
                        VOFileListDst.AddItem(report.Path);
                        break;
                    case VOProgressType.ExtraVOType:
                        VOFileListExtra.AddItem(report.Path);
                        break;
                    case VOProgressType.MissingVOType:
                        VOFileListMissing.AddItem(report.Path);
                        break;
                    case VOProgressType.Error:
                        FailedFiles.Add(report.Path);
                        break;
                }
            });

            await reviver.CopyVOFilesAsync(progress);
            if (FailedFiles.Count > 0)
            {
                string message = $"{_messageBoxFileErrorText}\n{String.Join("\n", FailedFiles)}";
                ShowWarningMessageBox(message);
            }

            await reviver.PakVOFilesAsync();
            TextBlockProgress.SetResourceReference(TextBlock.TextProperty,
                "MainWindow.TextBlockProgess.PakSuccess.Text");
        }
        catch (UnauthorizedAccessException ex)
        {
            VOFileListDst.FolderPath = string.Empty;
            string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
            ShowErrorMessageBox(message);
        }
        catch (IOException ex)
        {
            VOFileListDst.FolderPath = string.Empty;
            string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
            ShowErrorMessageBox(message);
        }
        finally
        {
            _isProcessing = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = !_isProcessing &&
            !String.IsNullOrEmpty(VOFileListDst.FolderPath) &&
            !String.IsNullOrEmpty(TextBoxPakName.Text);
    }

    private async void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _isProcessing = true;
        CommandManager.InvalidateRequerySuggested();
        try
        {
            string destinationFolderPath = Path.Combine(VOFileListDst.FolderPath, TextBoxPakName.Text);
            await Packer.PackAsync(destinationFolderPath);
            TextBlockProgress.SetResourceReference(TextBlock.TextProperty,
                "MainWindow.TextBlockProgess.PakSuccess.Text");
        }
        catch (DirectoryNotFoundException ex)
        {
            string message = $"{_messageBoxFolderErrorText}\n{ex.Message}";
            ShowErrorMessageBox(message);
        }
        finally
        {
            _isProcessing = false;
            CommandManager.InvalidateRequerySuggested();
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