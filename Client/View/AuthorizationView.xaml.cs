using Client.Model;
using System;
using System.Linq;
using System.Windows;

namespace Client.View
{
    /// <summary>
    /// Interaction logic for AuthorizationView.xaml
    /// </summary>
    public partial class AuthorizationView : Window
    {
        private readonly ClientModel? client;
        public AuthorizationView()
        {
            InitializeComponent();
        }

        public AuthorizationView(ClientModel model) : this()
        {
            client = model;
        }

        /// <summary>
        /// Set the living time value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            bool success = client?.Authorizate(login.Text, password.Password);

            if (!success)
            {
                MessageBox.Show("Authorization failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                this.Close();
            }
        }
        
        /// <summary>
        /// Set the living time value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (regPassword.Password.Equals(confirmPassword.Password))
            {
                bool success = client?.Register(regLogin.Text, regPassword.Password);

                if (!success)
                {
                    MessageBox.Show("Register failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Passwords do not match", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        /// <summary>
        /// Cancel setting the living time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void text_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if(e.Text.Any(x => !Char.IsDigit(x) && !Char.IsLetter(x) && x != '_'))
            {
                e.Handled = true;
            }
        }
    }
}
