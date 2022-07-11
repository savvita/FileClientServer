using Client.ViewModel;
using System.Windows;
namespace Client.View
{
    /// <summary>
    /// Interaction logic for ClientView.xaml
    /// </summary>
    public partial class ClientView : Window
    {
        private readonly ClientViewModel clientView = new ClientViewModel();
        public ClientView()
        {
            InitializeComponent();
            this.DataContext = clientView;
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadFileId.Clear();
            DownloadFileId.Paste();
        }
    }
}
