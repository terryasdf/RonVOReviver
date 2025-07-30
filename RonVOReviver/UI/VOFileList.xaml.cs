using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for VOFileList.xaml
    /// </summary>
    public partial class VOFileList : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(VOFileList));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly RoutedEvent SelectEvent = EventManager.RegisterRoutedEvent(
            "Select", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VOFileList));

        public event RoutedEventHandler Select
        {
            add => AddHandler(SelectEvent, value);
            remove => RemoveHandler(SelectEvent, value);
        }

        public string FolderPath { get; protected set; } = string.Empty;

        public VOFileList()
        {
            InitializeComponent();
        }

        public void ClearItems()
        {
            ItemList.Items.Clear();
            TextBlockItemCount.Text = string.Empty;
        }

        public void AddItem(string item)
        {
            ItemList.Items.Add(item);
            TextBlockItemCount.Text = ItemList.Items.Count.ToString();
        }

        private void FolderSelector_Select(object sender, RoutedEventArgs e)
        {
            FolderPath = FolderSelector.FolderPath;
            RaiseEvent(new RoutedEventArgs(SelectEvent));
        }
    }
}
