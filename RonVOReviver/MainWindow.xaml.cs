using Microsoft.Win32;
using NLog;
using RonVOReviver.Reviver;
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
        private static readonly ResourceDictionary DictionaryENUS = new();
        private static readonly ResourceDictionary DictionaryZHCN = new();

        private VOReviver _reviver = new();

        public MainWindow()
        {
            InitializeComponent();
            DictionaryENUS.Source = new Uri("Languages/en-us.xaml", UriKind.Relative);
            DictionaryZHCN.Source = new Uri("Languages/zh-cn.xaml", UriKind.Relative);
        }

        private void VOFileListOriginal_FolderSelect(object sender, RoutedEventArgs e)
        {
            Logger.Info($"Original VO Folder chosen: {VOFileListOriginal.FolderPath}");
            VOFileListOriginal.IsEnabled = false;
            _reviver.OriginalVoFolderPath = VOFileListOriginal.FolderPath;
            VOFileListOriginal.IsEnabled = true;
        }

        private void VOFileListModded_FolderSelect(object sender, RoutedEventArgs e)
        {
            Logger.Info($"Modded VO Folder chosen: {VOFileListOriginal.FolderPath}");
            VOFileListModded.IsEnabled = false;
            _reviver.ModdedVoFolderPath = VOFileListModded.FolderPath;
            VOFileListModded.IsEnabled = true;
        }

        private void ButtonENUS_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources.MergedDictionaries[0] = DictionaryENUS;
        }

        private void ButtonZHCN_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources.MergedDictionaries[0] = DictionaryZHCN;
        }
    }
}