using System;
using System.Windows;
using System.Windows.Controls;
using PiggyBankAdmin.Models;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace PiggyBankAdmin.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailTextBox.Text;
            var password = PasswordBox.Password;

            // Базовая валидация
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите email и пароль");
                return;
            }

            try
            {
                LoginButton.IsEnabled = false;
                LoginButton.Content = "Вход...";

                var (success, message) = await ApiHelper.LoginAsync(email, password);

                if (success)
                {
                    // Создаем объект пользователя для приложения
                    App.CurrentUser = new User
                    {
                        Id = ApiHelper.CurrentUserId,
                        Email = ApiHelper.CurrentUserEmail,
                        Name = ApiHelper.CurrentUserName
                    };

                    MessageBox.Show($"Добро пожаловать, {ApiHelper.CurrentUserName}!",
                                  "Успешный вход",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    // Открываем главное окно
                    var mainWindow = new MainWindow();
                    mainWindow.Show();

                    this.Close();
                }
                else
                {
                    ShowError(message);
                    PasswordBox.Clear();
                    PasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка подключения: {ex.Message}");
                PasswordBox.Focus();
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Войти";
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorText.Visibility = Visibility.Collapsed;
        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideError();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HideError();
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            // Здесь можно добавить логику регистрации нового администратора
            MessageBox.Show("Функция регистрации новых администраторов временно недоступна",
                          "Информация",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}