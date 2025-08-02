using System.Windows;
using System.Windows.Threading;

namespace RonVOReviver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string message = $"An error has occurred:\n{e.Exception.Message}";
            RonVOReviver.MainWindow.ShowErrorMessageBox(message);
            e.Handled = true;
            Current.Shutdown(e.GetHashCode());
        }
    }
}
