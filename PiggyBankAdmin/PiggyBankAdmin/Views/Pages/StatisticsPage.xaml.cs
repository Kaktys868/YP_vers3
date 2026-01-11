using System.Windows;
using System.Windows.Controls;
using PiggyBankAdmin.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System.Linq;
using System.Windows.Documents;
using System.Threading.Tasks;

namespace PiggyBankAdmin.Views.Pages
{
    public partial class StatisticsPage : Page
    {
        private List<User> _users;
        private List<Transaction> _transactions;
        private List<Goal> _goals;

        public StatisticsPage()
        {
            InitializeComponent();
            LoadStatistics();
        }

        private async void LoadStatistics()
        {
            // Загружаем актуальные данные
            _users = await GetUsers();
            _transactions = await GetTransactions();
            _goals = await GetGoals();

            // Обновляем карточки статистики
            TotalUsersText.Text = _users.Count.ToString();
            TotalTransactionsText.Text = _transactions.Count.ToString();

            var totalIncome = _transactions.Where(t => t.Type == "income").Sum(t => t.Amount);
            var totalExpenses = _transactions.Where(t => t.Type == "expense").Sum(t => t.Amount);

            TotalIncomeText.Text = $"{totalIncome}₽";
            TotalExpensesText.Text = $"{totalExpenses}₽";
        }

        // Методы для получения данных (в реальном приложении - из API)
        private async Task<List<User>> GetUsers()
        {
            var (success, users, message) = await ApiHelper.GetUsersAsync();
            return users;
        }

        private async Task<List<Transaction>> GetTransactions()
        {
            var (success, transactions, message) = await ApiHelper.GetTransactionsAsync();
            return transactions; 
        }

        private async Task<List<Goal>> GetGoals()
        {
            var (success, goals, message) = await ApiHelper.GetGoalsAsync();
            return goals;
        }

        // ЭКСПОРТ ПОЛЬЗОВАТЕЛЕЙ
        private void ExportUsers_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"users_export_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var csv = new StringBuilder();
                    csv.AppendLine("ID;Email;Имя;Активен");

                    foreach (var user in _users)
                    {
                        csv.AppendLine($"{user.Id};{user.Email};{user.Name};{user.IsActive}");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Данные пользователей экспортированы!\nФайл: {saveDialog.FileName}", "Экспорт завершен");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка");
                }
            }
        }

        // ЭКСПОРТ ТРАНЗАКЦИЙ
        private void ExportTransactions_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"transactions_export_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var csv = new StringBuilder();
                    csv.AppendLine("ID;Сумма;Тип");

                    foreach (var transaction in _transactions)
                    {
                        csv.AppendLine($"{transaction.Id};{transaction.Amount};{transaction.Type}");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Данные транзакций экспортированы!\nФайл: {saveDialog.FileName}", "Экспорт завершен");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка");
                }
            }
        }

        // ЭКСПОРТ ЦЕЛЕЙ
        private void ExportGoals_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"goals_export_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var csv = new StringBuilder();
                    csv.AppendLine("ID;Название;Целевая сумма;Текущая сумма;Прогресс %;Выполнена");

                    foreach (var goal in _goals)
                    {
                        var progress = goal.TargetAmount > 0 ? (goal.CurrentAmount / goal.TargetAmount) * 100 : 0;
                        csv.AppendLine($"{goal.Id};{goal.Name};{goal.TargetAmount};{goal.CurrentAmount};{progress:F1}%;{goal.IsCompleted}");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Данные целей экспортированы!\nФайл: {saveDialog.FileName}", "Экспорт завершен");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка");
                }
            }
        }
    }
}