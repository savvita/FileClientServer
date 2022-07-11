using Client.Model;
using System.Windows;

namespace Client.View
{
    /// <summary>
    /// Interaction logic for BuyPremiumView.xaml
    /// </summary>
    public partial class BuyPremiumView : Window
    {
        private readonly ClientModel? client;

        public BuyPremiumView()
        {
            InitializeComponent();
        }

        public BuyPremiumView(ClientModel client) : this()
        {
            this.client = client;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Buy_Click(object sender, RoutedEventArgs e)
        {
            client?.AddPremium();
            this.Close();
        }
    }
}
