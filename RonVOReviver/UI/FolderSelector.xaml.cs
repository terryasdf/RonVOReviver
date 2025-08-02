using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace RonVOReviver.UI
{
    /// <summary>
    /// Interaction logic for Selector.xaml
    /// </summary>
    public partial class FolderSelector : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static readonly RoutedEvent SelectEvent = EventManager.RegisterRoutedEvent(
            "Select", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FolderSelector));

        public event RoutedEventHandler Select
        {
            add => AddHandler(SelectEvent, value);
            remove => RemoveHandler(SelectEvent, value);
        }

        public static readonly DependencyProperty FolderPathProperty = DependencyProperty.Register(
            "FolderPath", typeof(string), typeof(VOFileList));

        public string FolderPath
        {
            get => (string)GetValue(FolderPathProperty);
            set
            {
                SetValue(FolderPathProperty, value);
                OnPropertyChanged();
            }
        }

        public FolderSelector()
        {
            InitializeComponent();
            FolderPath = string.Empty;
        }

        private void ButtonSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dlg = new();
            bool? result = dlg.ShowDialog();
            if (result != true)
            {
                return;
            }
            FolderPath = dlg.FolderName;
            RaiseEvent(new RoutedEventArgs(SelectEvent));
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
