using NLog;
using RonVOReviver.Core;
using RonVOReviver.Models;
using RonVOReviver.Services;
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
    private static readonly string DefaultCharacter = "SWATJudge";
    private static readonly string DefaultPakName = "pakchunk99-RevivedVO";
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
        DictionaryENUS.Source = new Uri("Resources/Localization/en-us.xaml", UriKind.Relative);
        DictionaryZHCN.Source = new Uri("Resources/Localization/zh-cn.xaml", UriKind.Relative);
        ResetDynamicResourcesMessageTexts();
        PopulateOriginalCharacters();
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

    private static string GetOriginalOggListsDirectory()
    {
        return Path.Combine(AppContext.BaseDirectory, "VanillaVoiceLists");
    }

    private void PopulateOriginalCharacters()
    {
        string originalOggDir = GetOriginalOggListsDirectory();
        if (!Directory.Exists(originalOggDir))
        {
            Logger.Warn($"Original OGG lists directory not found: {originalOggDir}");
            return;
        }

        var characterFiles = Directory.GetFiles(originalOggDir, "*.txt")
            .Where(f => new FileInfo(f).Length > 0)
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();

        VOFileListOriginal.Characters = characterFiles;

        if (characterFiles.Contains(DefaultCharacter))
        {
            VOFileListOriginal.SelectedCharacter = DefaultCharacter;
        }
        else if (characterFiles.Count > 0)
        {
            VOFileListOriginal.SelectedCharacter = characterFiles[0];
        }
    }

    private void VOFileListOriginal_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string selectedCharacter = VOFileListOriginal.SelectedCharacter;
        if (string.IsNullOrWhiteSpace(selectedCharacter))
        {
            return;
        }

        LoadOriginalCharacter(selectedCharacter);
    }

    private void LoadOriginalCharacter(string character)
    {
        string txtPath = Path.Combine(GetOriginalOggListsDirectory(), $"{character}.txt");
        if (!File.Exists(txtPath))
        {
            return;
        }

        Logger.Debug($"Original character chosen: {character} ({txtPath})");
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
            _originalVOManager = new OriginalVOManager(txtPath, progress);

            if (skippedVOFiles.Count > 0)
            {
                string message = $"{_messageBoxFormatExceptionText}\n{string.Join("\n", skippedVOFiles)}";
                ShowWarningMessageBox(message);
            }

            TextBoxCharacter.Text = character;
            TextBlockProgress.SetResourceReference(TextBlock.TextProperty,
                "MainWindow.TextBlockProgess.LoadedOriginal.Text");
        }
        catch (Exception ex)
        {
            string message = $"{_messageBoxFileErrorText}\n{ex.Message}";
            ShowErrorMessageBox(message);
        }
        finally
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private async Task LoadModdedFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return;
        }

        Logger.Info($"Modded VO Folder chosen: {folderPath}");
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
            _moddedVOManager = await Task.Run(() => new ModdedVOManager(folderPath, progress));

            if (skippedVOFiles.Count > 0)
            {
                string message = $"{_messageBoxFormatExceptionText}\n{string.Join("\n", skippedVOFiles)}";
                ShowWarningMessageBox(message);
            }

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
        finally
        {
            VOFileListModded.IsEnabled = true;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private async void VOFileListModded_FolderSelect(object sender, RoutedEventArgs e)
    {
        await LoadModdedFolder(VOFileListModded.FolderPath);
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
            string.IsNullOrEmpty(VOFileListModded.FolderPath) ||
            string.IsNullOrEmpty(VOFileListDst.FolderPath) ||
            string.IsNullOrEmpty(TextBoxPakName.Text) ||
            string.IsNullOrEmpty(TextBoxCharacter.Text))
        {
            e.CanExecute = false;
            return;
        }
        e.CanExecute = true;
    }

    private async void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _isProcessing = true;
        CommandManager.InvalidateRequerySuggested();

        try
        {
            await LoadModdedFolder(VOFileListModded.FolderPath);

            if (_originalVOManager == null || _moddedVOManager == null)
            {
                return;
            }

            Debug.Assert(!string.IsNullOrEmpty(VOFileListDst.FolderPath));
            Debug.Assert(!string.IsNullOrEmpty(TextBoxPakName.Text));

            VOFileListDst.ClearItems();
            VOFileListMissing.ClearItems();
            VOFileListExtra.ClearItems();
            List<string> FailedFiles = [];

            string pakFolderPath = Path.Combine(VOFileListDst.FolderPath, TextBoxPakName.Text);
            VOReviver reviver = new(
                _originalVOManager,
                _moddedVOManager,
                pakFolderPath,
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
                string message = $"{_messageBoxFileErrorText}\n{string.Join("\n", FailedFiles)}";
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
            !string.IsNullOrEmpty(VOFileListDst.FolderPath) &&
            !string.IsNullOrEmpty(TextBoxPakName.Text);
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