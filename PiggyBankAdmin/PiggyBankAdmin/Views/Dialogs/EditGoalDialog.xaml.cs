using System;
using System.Windows;
using PiggyBankAdmin.Models;
using System.Collections.Generic;
using System.Linq;

namespace PiggyBankAdmin.Views.Dialogs
{
    public partial class EditGoalDialog : Window
    {
        public Goal Goal { get; private set; }
        private List<User> _users;

        public EditGoalDialog(List<User> users, Goal goal)
        {
            InitializeComponent();
            _users = users;
            Goal = goal;

            InitializeControls();
            PopulateFields();
        }

        private void InitializeControls()
        {
            UserComboBox.ItemsSource = _users;
        }

        private void PopulateFields()
        {
            // Заполняем поля данными цели
            var user = _users.FirstOrDefault(u => u.Id == Goal.UserId);
            if (user != null)
                UserComboBox.SelectedItem = user;

            NameTextBox.Text = Goal.Name;
            TargetAmountTextBox.Text = Goal.TargetAmount.ToString();
            CurrentAmountTextBox.Text = Goal.CurrentAmount.ToString();
            DeadlineDatePicker.SelectedDate = Goal.Deadline;
            IsCompletedCheckBox.IsChecked = Goal.IsCompleted;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(TargetAmountTextBox.Text) || string.IsNullOrWhiteSpace(CurrentAmountTextBox.Text) ||
                DeadlineDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            if (!decimal.TryParse(TargetAmountTextBox.Text, out decimal targetAmount) || targetAmount <= 0)
            {
                MessageBox.Show("Введите корректную целевую сумму");
                return;
            }

            if (!decimal.TryParse(CurrentAmountTextBox.Text, out decimal currentAmount) || currentAmount < 0)
            {
                MessageBox.Show("Введите корректную текущую сумму");
                return;
            }

            if (currentAmount > targetAmount)
            {
                MessageBox.Show("Текущая сумма не может превышать целевую");
                return;
            }

            var selectedUser = (User)UserComboBox.SelectedItem;

            // Обновляем цель
            Goal.UserId = selectedUser.Id;
            Goal.UserName = selectedUser.Name;
            Goal.Name = NameTextBox.Text;
            Goal.TargetAmount = targetAmount;
            Goal.CurrentAmount = currentAmount;
            Goal.Deadline = DeadlineDatePicker.SelectedDate.Value;
            Goal.IsCompleted = IsCompletedCheckBox.IsChecked ?? false;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}