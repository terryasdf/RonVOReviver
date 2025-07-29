using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RonVOReviver.UI
{
    /// <summary>
    /// Interaction logic for Selector.xaml
    /// </summary>
    public partial class FolderSelector : UserControl
    {
        //public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
        //    "IsEnabled", typeof(bool), typeof(UserControl));

        //public bool IsEnabled
        //{
        //    get => (bool)GetValue(IsEnabledProperty);
        //    set => SetValue(IsEnabledProperty, value);
        //}

        public static readonly RoutedEvent SelectEvent = EventManager.RegisterRoutedEvent(
            "Select", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FolderSelector));

        public event RoutedEventHandler Select
        {
            add => AddHandler(SelectEvent, value);
            remove => RemoveHandler(SelectEvent, value);
        }

        public string FolderPath { get; protected set; } = string.Empty;

        public FolderSelector()
        {
            InitializeComponent();
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
            TextBoxFolderPath.Text = FolderPath;
            RaiseEvent(new RoutedEventArgs(SelectEvent));
        }
    }
}
