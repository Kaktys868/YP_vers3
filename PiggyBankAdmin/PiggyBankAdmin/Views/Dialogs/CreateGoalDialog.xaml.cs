using System;
using System.Windows;
using PiggyBankAdmin.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace PiggyBankAdmin.Views.Dialogs
{
    public partial class CreateGoalDialog : Window
    {
        public Models.Goal Goal { get; private set; }
        public decimal AddedAmount { get; private set; }
        public bool IsAddMoneyMode { get; private set; }

        public CreateGoalDialog(List<User> users)
        {
            InitializeComponent();
            Goal = new Models.Goal();
            UserComboBox.ItemsSource = users;

            // Устанавливаем дедлайн на 1 месяц вперед по умолчанию
            DeadlineDatePicker.SelectedDate = DateTime.Now.AddMonths(1);
        }

        // Конструктор для редактирования существующей цели
        public CreateGoalDialog(Models.Goal goal, List<User> users) : this(users)
        {
            Goal = goal;
            Title = "Редактирование цели";

            // Заполняем поля данными цели
            NameTextBox.Text = goal.Name;
            TargetAmountTextBox.Text = goal.TargetAmount.ToString();
            DeadlineDatePicker.SelectedDate = goal.Deadline;

            // Выбираем пользователя
            var user = users.FirstOrDefault(u => u.Id == goal.UserId);
            if (user != null)
            {
                UserComboBox.SelectedItem = user;
            }
        }

        // Конструктор для добавления средств
        public CreateGoalDialog(Models.Goal goal, List<User> users, bool addMoneyMode) : this(users)
        {
            Goal = goal;
            IsAddMoneyMode = addMoneyMode;
            Title = $"Добавление средств: {goal.Name}";

            // В вашем XAML нет этих элементов, поэтому просто меняем текст и скрываем все кроме одного поля
            // Скрываем все элементы кроме TargetAmountTextBox (будем использовать его для ввода суммы)
            var titleText = (TextBlock)FindName("TextBlock"); // Заголовок
            if (titleText != null)
                titleText.Text = $"💰 Добавление средств: {goal.Name}";

            var userLabel = (TextBlock)FindName("TextBlock"); // Метка пользователя
            if (userLabel != null)
                userLabel.Visibility = Visibility.Collapsed;

            UserComboBox.Visibility = Visibility.Collapsed;

            var nameLabel = (TextBlock)FindName("TextBlock"); // Метка названия
            if (nameLabel != null)
                nameLabel.Visibility = Visibility.Collapsed;

            NameTextBox.Visibility = Visibility.Collapsed;

            var targetLabel = (TextBlock)FindName("TextBlock"); // Метка целевой суммы
            if (targetLabel != null)
                targetLabel.Text = "Сумма для добавления:";

            var deadlineLabel = (TextBlock)FindName("TextBlock"); // Метка дедлайна
            if (deadlineLabel != null)
                deadlineLabel.Visibility = Visibility.Collapsed;

            DeadlineDatePicker.Visibility = Visibility.Collapsed;

            // Находим кнопку по имени и меняем текст
            var createButton = (Button)FindName("CreateButton");
            if (createButton != null)
                createButton.Content = "Добавить средства";
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAddMoneyMode)
            {
                // Режим добавления средств - используем TargetAmountTextBox для ввода суммы
                if (!decimal.TryParse(TargetAmountTextBox.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Введите корректную сумму для добавления");
                    return;
                }

                AddedAmount = amount;
                DialogResult = true;
                Close();
            }
            else
            {
                // Режим создания/редактирования цели
                if (UserComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(TargetAmountTextBox.Text) || DeadlineDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Заполните все поля");
                    return;
                }

                if (!decimal.TryParse(TargetAmountTextBox.Text, out decimal targetAmount) || targetAmount <= 0)
                {
                    MessageBox.Show("Введите корректную целевую сумму");
                    return;
                }

                var selectedUser = (User)UserComboBox.SelectedItem;

                try
                {
                    // Если цель новая (Id = 0)
                    if (Goal.Id == 0)
                    {
                        Goal.UserId = selectedUser.Id;
                        Goal.Name = NameTextBox.Text;
                        Goal.TargetAmount = targetAmount;
                        Goal.CurrentAmount = 0;
                        Goal.Deadline = DeadlineDatePicker.SelectedDate.Value;

                        var (success, message) = await ApiHelper.CreateGoalAsync(Goal);

                        if (success)
                        {
                            MessageBox.Show("✅ Цель успешно создана!", "Успех");
                            DialogResult = true;
                            Close();
                        }
                        else
                        {
                            MessageBox.Show($"❌ Ошибка при создании цели: {message}", "Ошибка");
                        }
                    }
                    else
                    {
                        Goal.UserId = selectedUser.Id;
                        Goal.Name = NameTextBox.Text;
                        Goal.TargetAmount = targetAmount;
                        Goal.Deadline = DeadlineDatePicker.SelectedDate.Value;

                        var (success, message) = await ApiHelper.UpdateGoalAsync(Goal);

                        if (success)
                        {
                            MessageBox.Show("✅ Цель успешно обновлена!", "Успех");
                            DialogResult = true;
                            Close();
                        }
                        else
                        {
                            MessageBox.Show($"❌ Ошибка при обновлении цели: {message}", "Ошибка");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
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