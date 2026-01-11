using System.Windows;
using PiggyBankAdmin.Models;
using PiggyBankAdmin.Views;

namespace PiggyBankAdmin
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Всегда показываем окно авторизации
            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}