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

namespace PiggyBankAdmin.Views.Pages
{
    public partial class GoalsPage : Page
    {
        private List<Goal> _goals;
        private List<User> _users;

        public GoalsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                // Загружаем пользователей
                var (usersSuccess, users, usersMessage) = await ApiHelper.GetUsersAsync();
                if (usersSuccess)
                {
                    _users = users;
                }
                else
                {
                    MessageBox.Show($"❌ Ошибка загрузки пользователей: {usersMessage}", "Ошибка");
                    _users = new List<User>();
                }

                // Загружаем цели
                var (goalsSuccess, goals, goalsMessage) = await ApiHelper.GetGoalsAsync();
                if (goalsSuccess)
                {
                    _goals = goals;
                    UpdateGoalUserNames();
                    GoalsGrid.ItemsSource = _goals;
                }
                else
                {
                    MessageBox.Show($"❌ Ошибка загрузки целей: {goalsMessage}", "Ошибка");
                    _goals = new List<Goal>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private async void CreateGoal_Click(object sender, RoutedEventArgs e)
        {
            if (_users == null || _users.Count == 0)
            {
                MessageBox.Show("❌ Нет пользователей для создания цели", "Ошибка");
                return;
            }

            var dialog = new CreateGoalDialog(_users);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    MessageBox.Show($"✅ Цель '{dialog.Goal.Name}' успешно создана!", "Успех");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
                }
            }
        }

        private async void EditGoal_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var goalId = (int)button.Tag;

            try
            {
                // Получаем цель с сервера
                var (success, goal, message) = await ApiHelper.GetGoalAsync(goalId);

                if (success && goal != null)
                {
                    var dialog = new CreateGoalDialog(goal, _users);
                    if (dialog.ShowDialog() == true)
                    {
                        LoadData(); // Перезагружаем данные
                    }
                }
                else
                {
                    MessageBox.Show($"❌ Ошибка загрузки цели: {message}", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private async void DeleteGoal_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var goalId = (int)button.Tag;
            var goal = _goals?.FirstOrDefault(g => g.Id == goalId);

            if (goal != null)
            {
                var result = MessageBox.Show($"Удалить цель '{goal.Name}'?\nПользователь: {goal.UserName}",
                                           "Подтверждение удаления",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var (success, message) = await ApiHelper.DeleteGoalAsync(goalId);

                        if (success)
                        {
                            MessageBox.Show("✅ Цель успешно удалена!", "Успех");
                            LoadData(); // Перезагружаем данные
                        }
                        else
                        {
                            MessageBox.Show($"❌ Ошибка при удалении цели: {message}", "Ошибка");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
                    }
                }
            }
        }

        private void UpdateGoalUserNames()
        {
            if (_users == null || _goals == null) return;

            foreach (var transaction in _goals)
            {
                transaction.UserName = GetUserNameById(transaction.UserId);
            }
        }
        private string GetUserNameById(int userId)
        {
            if (_users == null) return "Неизвестный пользователь";

            var user = _users.FirstOrDefault(u => u.Id == userId);
            return user?.Name ?? "Неизвестный пользователь";
        }

        private async void AddMoneyToGoal_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var goalId = (int)button.Tag;
            var goal = _goals?.FirstOrDefault(g => g.Id == goalId);

            if (goal != null)
            {
                // Создаем простой диалог для ввода суммы
                var inputDialog = new Window
                {
                    Title = "Добавление средств",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize
                };

                var stackPanel = new StackPanel { Margin = new Thickness(15) };

                // Текст запроса
                var textBlock = new TextBlock
                {
                    Text = $"Введите сумму для цели '{goal.Name}':",
                    Margin = new Thickness(0, 0, 0, 10)
                };

                // Поле для ввода суммы
                var textBox = new TextBox
                {
                    Height = 25,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                // Кнопки
                var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                var okButton = new Button { Content = "OK", Width = 75, Margin = new Thickness(0, 0, 10, 0), IsDefault = true };
                var cancelButton = new Button { Content = "Отмена", Width = 75, IsCancel = true };

                okButton.Click += (s, args) =>
                {
                    inputDialog.DialogResult = true;
                    inputDialog.Close();
                };

                cancelButton.Click += (s, args) =>
                {
                    inputDialog.DialogResult = false;
                    inputDialog.Close();
                };

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);

                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(textBox);
                stackPanel.Children.Add(buttonPanel);

                inputDialog.Content = stackPanel;

                // Показываем диалог и обрабатываем результат
                if (inputDialog.ShowDialog() == true)
                {
                    if (decimal.TryParse(textBox.Text, out decimal amount) && amount > 0)
                    {
                        try
                        {
                            var (success, message) = await ApiHelper.AddMoneyToGoalAsync(goalId, amount);

                            if (success)
                            {
                                MessageBox.Show("✅ Средства успешно добавлены к цели!", "Успех");
                                LoadData(); // Перезагружаем данные
                            }
                            else
                            {
                                MessageBox.Show($"❌ Ошибка при добавлении средств: {message}", "Ошибка");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
                        }
                    }
                    else
                    {
                        MessageBox.Show("❌ Введите корректную сумму", "Ошибка");
                    }
                }
            }
        }

        private async void CompleteGoal_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var goalId = (int)button.Tag;
            var goal = _goals?.FirstOrDefault(g => g.Id == goalId);

            if (goal != null && !goal.IsCompleted)
            {
                var remainingAmount = goal.TargetAmount - goal.CurrentAmount;
                if (remainingAmount > 0)
                {
                    var result = MessageBox.Show(
                        $"Для завершения цели '{goal.Name}' необходимо добавить {remainingAmount:C}.\nДобавить эту сумму?",
                        "Завершение цели",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var (success, message) = await ApiHelper.AddMoneyToGoalAsync(goalId, remainingAmount);

                            if (success)
                            {
                                MessageBox.Show($"✅ Цель '{goal.Name}' успешно завершена!", "Успех");
                                LoadData(); // Перезагружаем данные
                            }
                            else
                            {
                                MessageBox.Show($"❌ Ошибка при завершении цели: {message}", "Ошибка");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("✅ Цель уже выполнена!", "Информация");
                }
            }
        }

        private void RefreshGoals_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void ExportGoals_Click(object sender, RoutedEventArgs e)
        {
            if (_goals == null || _goals.Count == 0)
            {
                MessageBox.Show("❌ Нет данных для экспорта", "Ошибка");
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"goals_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var csv = new StringBuilder();
                    // Заголовок
                    csv.AppendLine("ID;ID пользователя;Пользователь;Название цели;Целевая сумма;Текущая сумма;Прогресс %;Дедлайн;Выполнена;Дата создания");

                    // Данные
                    foreach (var goal in _goals)
                    {
                        var progress = goal.TargetAmount > 0 ? (goal.CurrentAmount / goal.TargetAmount) * 100 : 0;
                        csv.AppendLine($"{goal.Id};{goal.UserId};{goal.UserName};{goal.Name};" +
                                     $"{goal.TargetAmount};{goal.CurrentAmount};{progress:F1};" +
                                     $"{goal.Deadline:yyyy-MM-dd};{goal.IsCompleted};{goal.CreatedAt:yyyy-MM-dd}");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show($"✅ Данные целей экспортированы!\nФайл: {saveDialog.FileName}", "Экспорт завершен");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Ошибка при экспорте: {ex.Message}", "Ошибка");
                }
            }
        }

        private void RefreshGrid()
        {
            GoalsGrid.ItemsSource = null;
            GoalsGrid.ItemsSource = _goals;
        }
    }
}