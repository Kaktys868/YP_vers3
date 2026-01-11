using System;
using System.Windows;
using System.Windows.Controls;
using PiggyBankAdmin.Models;
using PiggyBankAdmin.Views.Dialogs;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PiggyBankAdmin.Views.Pages
{
    public partial class TransactionsPage : Page
    {
        private ObservableCollection<Transaction> _transactions;
        private List<Transaction> _allTransactions;
        private ObservableCollection<Transaction> _filteredTransactions;
        private List<User> _users;
        private int _nextTransactionId = 1;

        public TransactionsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                // Загружаем пользователей из API
                await LoadUsersFromApi();
                // Загружаем транзакции из API
                await LoadTransactions();

                // Устанавливаем даты по умолчанию (последние 30 дней)
                EndDatePicker.SelectedDate = DateTime.Now;
                StartDatePicker.SelectedDate = DateTime.Now.AddDays(-30);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadUsersFromApi()
        {
            try
            {
                var (success, users, message) = await ApiHelper.GetUsersAsync();

                if (success)
                {
                    _users = users; // Инициализируем список пользователей
                    MessageBox.Show($"✅ Загружено {users.Count} пользователей");
                }
                else
                {
                    MessageBox.Show($"⚠️ Не удалось загрузить пользователей: {message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠️ Ошибка при загрузке пользователей: {ex.Message}");
            }
        }

        private async Task LoadTransactions()
        {
            try
            {
                if (_transactions == null)
                {
                    _transactions = new ObservableCollection<Transaction>();
                }
                else
                {
                    _transactions.Clear();
                }

                var (success, transactions, message) = await ApiHelper.GetTransactionsAsync();

                if (success)
                {
                    _allTransactions = transactions;

                    foreach (var transaction in transactions)
                    {
                        _transactions.Add(transaction);
                    }

                    // Обновляем имена пользователей для транзакций
                    UpdateTransactionUserNames();

                    // Обновляем UI
                    TransactionsGrid.ItemsSource = _transactions;

                    MessageBox.Show($"✅ Загружено {transactions.Count} транзакций");
                }
                else
                {
                    MessageBox.Show(message, "Ошибка загрузки транзакций",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки транзакций: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetUserNameById(int userId)
        {
            if (_users == null) return "Неизвестный пользователь";

            var user = _users.FirstOrDefault(u => u.Id == userId);
            return user?.Name ?? "Неизвестный пользователь";
        }

        private void UpdateTransactionUserNames()
        {
            if (_users == null || _transactions == null) return;

            foreach (var transaction in _transactions)
            {
                transaction.UserName = GetUserNameById(transaction.UserId);
            }
        }

        private void ApplyFilters()
        {
            if (_transactions == null) return;

            var filtered = _transactions.AsEnumerable();

            // Фильтр по типу
            var selectedType = (TypeFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            switch (selectedType)
            {
                case "Доходы":
                    filtered = filtered.Where(t => t.Type == "income");
                    break;
                case "Расходы":
                    filtered = filtered.Where(t => t.Type == "expense");
                    break;
            }

            // Фильтр по дате
            if (StartDatePicker.SelectedDate.HasValue)
            {
                filtered = filtered.Where(t => t.Date >= StartDatePicker.SelectedDate.Value);
            }
            if (EndDatePicker.SelectedDate.HasValue)
            {
                filtered = filtered.Where(t => t.Date <= EndDatePicker.SelectedDate.Value);
            }

            _filteredTransactions = new ObservableCollection<Transaction>(filtered);
            TransactionsGrid.ItemsSource = _filteredTransactions;

            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            if (_filteredTransactions == null) return;

            var totalIncome = _filteredTransactions.Where(t => t.Type == "income").Sum(t => t.Amount);
            var totalExpenses = _filteredTransactions.Where(t => t.Type == "expense").Sum(t => t.Amount);
            var balance = totalIncome - totalExpenses;
        }

        private async void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (_users == null || _users.Count == 0)
            {
                MessageBox.Show("Сначала необходимо загрузить список пользователей", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new CreateTransactionDialog(_users);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Сохраняем транзакцию через API
                    var (success, message) = await ApiHelper.AddTransactionAsync(dialog.Transaction);

                    if (success)
                    {
                        // Перезагружаем транзакции для получения актуальных данных
                        await LoadTransactions();
                        ApplyFilters();

                        MessageBox.Show($"Транзакция на сумму {dialog.Transaction.Amount}₽ успешно добавлена!", "Успех");
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка при добавлении транзакции: {message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении транзакции: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EditTransaction_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var transactionId = (int)button.Tag;
            var transaction = _transactions.FirstOrDefault(t => t.Id == transactionId);

            if (transaction != null)
            {
                var dialog = new CreateTransactionDialog(_users, transaction);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        // Обновляем транзакцию через API
                        var (success, message) = await ApiHelper.UpdateTransactionAsync(dialog.Transaction);

                        if (success)
                        {
                            // Перезагружаем транзакции для получения актуальных данных
                            await LoadTransactions();
                            ApplyFilters();

                            MessageBox.Show("Транзакция успешно обновлена", "Успех");
                        }
                        else
                        {
                            MessageBox.Show($"Ошибка при обновлении транзакции: {message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при обновлении транзакции: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var transactionId = (int)button.Tag;
            var transaction = _transactions.FirstOrDefault(t => t.Id == transactionId);

            if (transaction != null)
            {
                var result = MessageBox.Show($"Удалить транзакцию?\n{transaction.Description}\nСумма: {transaction.Amount}₽",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Удаляем транзакцию через API
                        var (success, message) = await ApiHelper.DeleteTransactionAsync(transactionId);

                        if (success)
                        {
                            // Перезагружаем транзакции для получения актуальных данных
                            await LoadTransactions();
                            ApplyFilters();

                            MessageBox.Show("Транзакция успешно удалена", "Успех");
                        }
                        else
                        {
                            MessageBox.Show($"Ошибка при удалении транзакции: {message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении транзакции: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private async void RefreshTransactions_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void ExportTransactions_Click(object sender, RoutedEventArgs e)
        {
            if (_filteredTransactions == null || _filteredTransactions.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"transactions_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var csv = new StringBuilder();
                    csv.AppendLine("ID;ID пользователя;Пользователь;Сумма;Тип;Описание;Дата;Повторяющаяся");

                    foreach (var transaction in _filteredTransactions)
                    {
                        csv.AppendLine($"{transaction.Id};{transaction.UserId};{transaction.UserName};" +
                                      $"{transaction.Amount};{transaction.TypeDisplay};" +
                                      $"\"{transaction.Description}\";{transaction.Date:yyyy-MM-dd};{transaction.IsRecurring}");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Данные экспортированы в файл: {saveDialog.FileName}", "Экспорт завершен");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка");
                }
            }
        }
    }
}