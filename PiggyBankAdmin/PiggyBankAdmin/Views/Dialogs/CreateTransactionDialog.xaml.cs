using System;
using System.Windows;
using System.Linq;
using PiggyBankAdmin.Models;
using System.Collections.Generic;
using System.Windows.Controls;

namespace PiggyBankAdmin.Views.Dialogs
{
    public partial class CreateTransactionDialog : Window
    {
        public Transaction Transaction { get; private set; }
        private List<User> _users;

        public CreateTransactionDialog(List<User> users, Transaction existingTransaction = null)
        {
            InitializeComponent();
            _users = users;

            InitializeControls();

            if (existingTransaction != null)
            {
                // Режим редактирования
                Transaction = existingTransaction;
                PopulateFields();
                Title = "Редактирование транзакции";
            }
            else
            {
                // Режим создания
                Transaction = new Transaction();
                Title = "Добавление транзакции";
            }
        }

        private void InitializeControls()
        {
            // Заполняем комбобокс пользователей
            UserComboBox.ItemsSource = _users;
            if (_users.Any())
                UserComboBox.SelectedIndex = 0;

            // Устанавливаем текущую дату
            DatePicker.SelectedDate = DateTime.Now;

            // Фокус на поле суммы
            AmountTextBox.Focus();
        }

        private void PopulateFields()
        {
            // Заполняем поля данными существующей транзакции
            if (Transaction.Type == "income")
                IncomeRadioButton.IsChecked = true;
            else
                ExpenseRadioButton.IsChecked = true;

            // Находим пользователя
            var user = _users.FirstOrDefault(u => u.Id == Transaction.UserId);
            if (user != null)
                UserComboBox.SelectedItem = user;

            AmountTextBox.Text = Transaction.Amount.ToString("F2");
            DescriptionTextBox.Text = Transaction.Description;
            DatePicker.SelectedDate = Transaction.Date;
            IsRecurringCheckBox.IsChecked = Transaction.IsRecurring;

            if (Transaction.IsRecurring)
            {
                RecurringPanel.Visibility = Visibility.Visible;
                // Устанавливаем выбранный период повторения
                SetRecurringComboBoxSelection(Transaction.RecurringType);
            }
        }

        private void SetRecurringComboBoxSelection(string recurringType)
        {
            string displayName = GetRecurringDisplayName(recurringType);
            foreach (ComboBoxItem item in RecurringComboBox.Items)
            {
                if (item.Content.ToString() == displayName)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private string GetRecurringDisplayName(string recurringType)
        {
            switch (recurringType)
            {
                case "daily": return "Ежедневно";
                case "weekly": return "Еженедельно";
                case "monthly": return "Ежемесячно";
                default: return "Ежедневно";
            }
        }

        private string GetRecurringTypeFromDisplay(string displayName)
        {
            switch (displayName)
            {
                case "Ежедневно": return "daily";
                case "Еженедельно": return "weekly";
                case "Ежемесячно": return "monthly";
                default: return "none";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                // Сохраняем данные в объект Transaction
                Transaction.Type = IncomeRadioButton.IsChecked == true ? "income" : "expense";

                var selectedUser = (User)UserComboBox.SelectedItem;
                Transaction.UserId = selectedUser.Id;
                Transaction.UserName = selectedUser.Name;

                Transaction.Amount = decimal.Parse(AmountTextBox.Text);
                Transaction.Description = DescriptionTextBox.Text.Trim();
                Transaction.Date = DatePicker.SelectedDate ?? DateTime.Now;
                Transaction.IsRecurring = IsRecurringCheckBox.IsChecked ?? false;

                if (Transaction.IsRecurring)
                {
                    var selectedRecurring = (RecurringComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    Transaction.RecurringType = GetRecurringTypeFromDisplay(selectedRecurring);
                }
                else
                {
                    Transaction.RecurringType = "none";
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            // Проверка пользователя
            if (UserComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                UserComboBox.Focus();
                return false;
            }

            // Проверка суммы
            if (string.IsNullOrWhiteSpace(AmountTextBox.Text))
            {
                MessageBox.Show("Введите сумму", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму (больше 0)", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountTextBox.SelectAll();
                AmountTextBox.Focus();
                return false;
            }

            // Проверка описания
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Введите описание транзакции", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                DescriptionTextBox.Focus();
                return false;
            }

            // Проверка даты
            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                DatePicker.Focus();
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void IsRecurringCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            RecurringPanel.Visibility = IsRecurringCheckBox.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Обработчик для форматирования суммы
        private void AmountTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Разрешаем только цифры и запятую/точку
            if (!char.IsDigit(e.Text, 0) && e.Text != "," && e.Text != ".")
            {
                e.Handled = true;
            }

            // Проверяем, что запятая/точка только одна
            if ((e.Text == "," || e.Text == ".") && AmountTextBox.Text.Contains(","))
            {
                e.Handled = true;
            }
        }
    }
}