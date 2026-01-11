using System.Data;
using System.Windows;
using PiggyBankAdmin.Models;

namespace PiggyBankAdmin.Views.Dialogs
{
    public partial class CreateUserDialog : Window
    {
        public User User { get; private set; }

        public CreateUserDialog(User user = null)
        {
            InitializeComponent();

            if (user != null)
            {
                User = user;
                NameTextBox.Text = user.Name;
                EmailTextBox.Text = user.Email;
                IsActiveCheckBox.IsChecked = user.IsActive;
                Title = "Редактирование пользователя";
                title.Text = "Редактирование пользователя";
            }
            else
            {
                User = new User();
                Title = "Добавление пользователя";
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || string.IsNullOrWhiteSpace(EmailTextBox.Text) || string.IsNullOrWhiteSpace(Password1TextBox.Text) || string.IsNullOrWhiteSpace(Password2TextBox.Text))
            {
                MessageBox.Show("Заполните все поля");
                return;
            }
            if(Password1TextBox.Text != Password2TextBox.Text)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }
            if(User == null) {
                User.Name = NameTextBox.Text;
                User.Email = EmailTextBox.Text;
                User.Password = Password1TextBox.Text;
                User.Post = 1;
                User.CreatedAt = System.DateTime.Today;
                User.IsActive = IsActiveCheckBox.IsChecked ?? false;

                var result = await ApiHelper.CreateUserAsync(User);
                if (result.Success)
                {
                    MessageBox.Show("Пользователь успешно добавлен");
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show($"Ошибка при добавлении пользователя: {result.Message}");
                    return;
                }
            }
            else
            {
                User.Name = NameTextBox.Text;
                User.Email = EmailTextBox.Text;
                User.Password = Password1TextBox.Text;
                User.Post = 1;
                User.CreatedAt = System.DateTime.Today;
                User.IsActive = IsActiveCheckBox.IsChecked ?? false;

                var result = await ApiHelper.UpdateUserAsync(User);
                if (result.Success)
                {
                    MessageBox.Show("Пользователь успешно обновлён");
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show($"Ошибка при добавлении пользователя: {result.Message}");
                    return;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}