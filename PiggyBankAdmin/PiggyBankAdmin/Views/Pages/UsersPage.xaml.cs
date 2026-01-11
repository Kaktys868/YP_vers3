using System.Windows;
using System.Windows.Controls;
using PiggyBankAdmin.Models;
using PiggyBankAdmin.Views.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PiggyBankAdmin.Views.Pages
{
    public partial class UsersPage : Page
    {
        private List<User> _users;

        public UsersPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private async void LoadUsers()
        {
            try
            {

                var (success, users, message) = await ApiHelper.GetUsersAsync();

                if (success)
                {
                    _users = users;
                    UsersGrid.ItemsSource = _users;
                    MessageBox.Show($"✅ Загружено {users.Count} пользователей", "Успех");
                }
                else
                {
                    MessageBox.Show($"❌ Не удалось загрузить пользователей: {message}", "Ошибка");
                    _users = new List<User>();
                    UsersGrid.ItemsSource = _users;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка при загрузке пользователей: {ex.Message}", "Ошибка");
                _users = new List<User>();
                UsersGrid.ItemsSource = _users;
            }
        }

        private async void RefreshUsers_Click(object sender, RoutedEventArgs e)
        {
             LoadUsers();
        }

        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateUserDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (!string.IsNullOrEmpty(dialog.User.Password))
                    {
                        dialog.User.Password = HashPassword(dialog.User.Password);
                    }

                    dialog.User.CreatedAt = DateTime.Now;
                    dialog.User.IsActive = true;

                    var (success, message) = await ApiHelper.CreateUserAsync(dialog.User);

                    if (success)
                    {
                        MessageBox.Show($"✅ Пользователь успешно добавлен!", "Успех");
                        LoadUsers();
                    }
                    else
                    {
                        MessageBox.Show($"❌ Ошибка при добавлении пользователя: {message}", "Ошибка");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
                }
            }
        }

        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var userId = (int)button.Tag;
            var user = _users.FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                try
                {
                    // Создаем копию пользователя для редактирования
                    var userToEdit = new User
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Password = user.Password, // сохраняем текущий хэш пароля
                        Post = user.Post,
                        CreatedAt = user.CreatedAt,
                        IsActive = user.IsActive
                    };

                    var dialog = new CreateUserDialog(userToEdit);
                    dialog.Title = "Редактирование пользователя";

                    if (dialog.ShowDialog() == true)
                    {
                        if (!string.IsNullOrEmpty(dialog.User.Password) &&
                            dialog.User.Password != user.Password)
                        {
                            if (dialog.User.Password != user.Password)
                            {
                                dialog.User.Password = HashPassword(dialog.User.Password);
                            }
                        }
                        else
                        {
                            // Сохраняем старый хэш пароля
                            dialog.User.Password = user.Password;
                        }

                        var (success, message) = await ApiHelper.UpdateUserAsync(dialog.User);

                        if (success)
                        {
                            MessageBox.Show("✅ Пользователь успешно обновлен", "Успех");
                            LoadUsers(); // Перезагружаем список
                        }
                        else
                        {
                            MessageBox.Show($"❌ Ошибка при обновлении пользователя: {message}", "Ошибка");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
                }
            }
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var userId = (int)button.Tag;
            var user = _users.FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                // ЗАЩИТА ГЛАВНОГО АДМИНА - нельзя удалить пользователя с ID = 1
                if (user.Id == 1)
                {
                    MessageBox.Show("Нельзя удалить главного администратора системы!", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Нельзя удалить самого себя
                if (App.CurrentUser != null && App.CurrentUser.Id == userId)
                {
                    MessageBox.Show("Вы не можете удалить свою собственную учетную запись!", "Ошибка");
                    return;
                }

                var result = MessageBox.Show($"Удалить пользователя {user.Name}?\nEmail: {user.Email}",
                                           "Подтверждение удаления",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var (success, message) = await ApiHelper.DeleteUserAsync(userId);

                        if (success)
                        {
                            MessageBox.Show("✅ Пользователь успешно удален", "Успех");
                            LoadUsers(); // Перезагружаем список
                        }
                        else
                        {
                            MessageBox.Show($"❌ Ошибка при удалении пользователя: {message}", "Ошибка");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
                    }
                }
            }
        }

        private async void ToggleUserActive_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var userId = (int)button.Tag;
            var user = _users.FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                try
                {
                    // Нельзя деактивировать самого себя
                    if (App.CurrentUser != null && App.CurrentUser.Id == userId)
                    {
                        MessageBox.Show("Вы не можете деактивировать свою собственную учетную запись!", "Ошибка");
                        return;
                    }

                    user.IsActive = !user.IsActive;
                    var (success, message) = await ApiHelper.UpdateUserAsync(user);

                    if (success)
                    {
                        MessageBox.Show($"✅ Пользователь {(user.IsActive ? "активирован" : "деактивирован")}", "Успех");
                        LoadUsers(); // Перезагружаем список
                    }
                    else
                    {
                        MessageBox.Show($"❌ Ошибка при изменении статуса: {message}", "Ошибка");
                        // Возвращаем предыдущее состояние
                        user.IsActive = !user.IsActive;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
                    // Возвращаем предыдущее состояние
                    user.IsActive = !user.IsActive;
                }
            }
        }

        private void ExportUsers_Click(object sender, RoutedEventArgs e)
        {
            if (_users == null || _users.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Информация");
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"users_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var csv = new StringBuilder();
                    // Заголовок
                    csv.AppendLine("ID;Email;Имя;Дата регистрации;Активен");

                    // Данные
                    foreach (var user in _users)
                    {
                        csv.AppendLine($"{user.Id};{user.Email};{user.Name};{user.CreatedAt:yyyy-MM-dd};{user.IsActive}");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show($"✅ Данные пользователей экспортированы!\nФайл: {saveDialog.FileName}", "Экспорт завершен");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Ошибка при экспорте: {ex.Message}", "Ошибка");
                }
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}