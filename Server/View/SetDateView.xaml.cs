using System;
using System.Windows;

namespace Server.View
{
    /// <summary>
    /// Interaction logic for SetDateView.xaml
    /// </summary>
    public partial class SetDateView : Window
    {
        public SetDateView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Setted value of the living time
        /// </summary>
        public DateTime? LiveTo { get; private set; }

        /// <summary>
        /// Set the living time value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            LiveTo = Date.SelectedDate?.Add(Clock.Time.TimeOfDay);
            this.Close();
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
    }
}
