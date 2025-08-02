using System.Windows;
using System.Windows.Controls;

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

        public string FolderPath
        {
            get => FolderSelector.FolderPath;
            set => FolderSelector.FolderPath = value;
        }

        public static readonly RoutedEvent SelectEvent = EventManager.RegisterRoutedEvent(
            "Select", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VOFileList));

        public event RoutedEventHandler Select
        {
            add => AddHandler(SelectEvent, value);
            remove => RemoveHandler(SelectEvent, value);
        }

        public VOFileList()
        {
            InitializeComponent();
            FolderPath = string.Empty;
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
