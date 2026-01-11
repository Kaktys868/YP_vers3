using System.Windows;
using System.Windows.Controls;
using PiggyBankAdmin.Models;
using System.Collections.Generic;
using System.Linq;

namespace PiggyBankAdmin.Views.Pages
{
    public partial class AdminsPage : Page
    {
        private List<User> _admins;

        public AdminsPage()
        {
            InitializeComponent();
            LoadAdmins();
        }

        private void LoadAdmins()
        {
            // Тестовые данные
            _admins = new List<User>
            {
                new User { Id = 1, Name = "Главный администратор", Email = "admin@piggybank.ru", IsActive = true, IsAdmin = true }
            };

            AdminsGrid.ItemsSource = _admins;
        }

        private void AddAdmin_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new Views.RegisterWindow();
            if (registerWindow.ShowDialog() == true)
            {
                // Добавляем нового администратора
                registerWindow.NewUser.Id = _admins.Count + 1;
                _admins.Add(registerWindow.NewUser);
                RefreshGrid();

                MessageBox.Show($"Администратор {registerWindow.NewUser.Name} успешно добавлен!", "Успех");
            }
        }

        private void EditAdmin_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var adminId = (int)button.Tag;
            MessageBox.Show($"Редактирование администратора ID: {adminId}", "Информация");
        }

        private void BlockAdmin_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var adminId = (int)button.Tag;

            // ЗАЩИТА ГЛАВНОГО АДМИНА - нельзя заблокировать пользователя с ID = 1
            if (adminId == 1)
            {
                MessageBox.Show("Нельзя заблокировать главного администратора системы!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Нельзя заблокировать самого себя
            if (App.CurrentUser != null && App.CurrentUser.Id == adminId)
            {
                MessageBox.Show("Вы не можете заблокировать свою собственную учетную запись", "Ошибка");
                return;
            }

            var result = MessageBox.Show($"Заблокировать администратора ID: {adminId}?", "Подтверждение",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var admin = _admins.FirstOrDefault(a => a.Id == adminId);
                if (admin != null)
                {
                    admin.IsActive = false;
                    RefreshGrid();
                    MessageBox.Show("Администратор заблокирован", "Успех");
                }
            }
        }

        private void RefreshGrid()
        {
            AdminsGrid.ItemsSource = null;
            AdminsGrid.ItemsSource = _admins;
        }
    }
}