using System.Windows;
using System.Windows.Controls;
using PiggyBankAdmin.Views.Pages;

namespace PiggyBankAdmin.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateUserInfo();
            ShowUsers_Click(null, null);
        }

        private void UpdateUserInfo()
        {
            if (App.CurrentUser != null)
            {
                CurrentUserText.Text = $"{App.CurrentUser.Name} (Администратор)";
            }
        }

        private void ShowUsers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UsersPage());
        }

        private void ShowTransactions_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TransactionsPage());
        }

        private void ShowGoals_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new GoalsPage());
        }

        private void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new StatisticsPage());
        }

        private void ShowAdmins_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AdminsPage());
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                App.CurrentUser = null;

                var loginWindow = new LoginWindow();
                loginWindow.Show();

                this.Close();
            }
        }
    }
}