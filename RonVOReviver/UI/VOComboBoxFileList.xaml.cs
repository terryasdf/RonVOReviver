using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace RonVOReviver.UI;

/// <summary>
/// Interaction logic for VOComboBoxFileList.xaml
/// </summary>
public partial class VOComboBoxFileList : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        "Title", typeof(string), typeof(VOComboBoxFileList));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string SelectedCharacter
    {
        get => ComboBoxCharacter.SelectedItem as string ?? string.Empty;
        set => ComboBoxCharacter.SelectedItem = value;
    }

    public IEnumerable? Characters
    {
        get => ComboBoxCharacter.ItemsSource;
        set => ComboBoxCharacter.ItemsSource = value;
    }

    public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
        "SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(VOComboBoxFileList));

    public event SelectionChangedEventHandler SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    public VOComboBoxFileList()
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

    private void ComboBoxCharacter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, e.RemovedItems, e.AddedItems));
    }
}

