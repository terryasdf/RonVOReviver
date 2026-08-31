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
    }
}
