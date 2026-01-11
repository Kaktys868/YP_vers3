using System;
using System.Windows;
using PiggyBankAdmin.Models;
using System.Security.Cryptography;
using System.Text;

namespace PiggyBankAdmin.Views
{
    public partial class RegisterWindow : Window
    {
        public User NewUser { get; private set; }

        public RegisterWindow()
        {
            InitializeComponent();
            NewUser = new User();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var name = NameTextBox.Text;
            var email = EmailTextBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Заполните все поля");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Пароль должен содержать минимум 6 символов");
                return;
            }

            try
            {
                // Хешируем пароль
                var passwordHash = HashPassword(password);

                NewUser.Name = name;
                NewUser.Email = email;
                NewUser.Password = passwordHash;
                NewUser.CreatedAt = DateTime.Now;
                NewUser.IsActive = true;
                NewUser.IsAdmin = true; // Все новые пользователи через это окно - админы

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка регистрации: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        // Простое хеширование пароля (в реальном приложении используй BCrypt)
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