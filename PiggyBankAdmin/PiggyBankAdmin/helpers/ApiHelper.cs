using PiggyBankAdmin.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

public static class ApiHelper
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private const string BaseUrl = "https://localhost:7107/api/";

    public static string CurrentUserEmail { get; set; }
    public static string CurrentUserName { get; set; }
    public static int CurrentUserId { get; set; }

    static ApiHelper()
    {
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public static bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUserEmail);

    public static async Task<(bool Success, string Message)> LoginAsync(string email, string password)
    {
        try
        {
            MessageBox.Show($"Попытка входа для: {email}");

            var loginData = new { Email = email, Password = password };
            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var doc = JsonDocument.Parse(responseContent);
                    var root = doc.RootElement;

                    var userEmail = root.GetProperty("email").GetString();
                    var userName = root.GetProperty("name").GetString();
                    var userId = root.GetProperty("userId").GetInt32();
                    var message = root.GetProperty("message").GetString();

                    CurrentUserEmail = userEmail;
                    CurrentUserName = userName;
                    CurrentUserId = userId;

                    MessageBox.Show($" Успешный вход: {userName} ({userEmail})");

                    return (true, message);
                }
                catch (Exception parseEx)
                {
                    MessageBox.Show($" Ошибка парсинга: {parseEx.Message}");
                    return (false, "Ошибка обработки ответа сервера");
                }
            }
            else
            {
                MessageBox.Show($" Ошибка входа: {response.StatusCode} - {responseContent}");
                return (false, "Неверный email или пароль");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($" Исключение при входе: {ex.Message}");
            return (false, $"Ошибка подключения: {ex.Message}");
        }
    }

    // === ПОЛЬЗОВАТЕЛИ ===
    // Получить всех пользователей
    public static async Task<(bool Success, List<User> Users, string Message)> GetUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("user/List");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var users = JsonSerializer.Deserialize<List<User>>(responseContent, options);
                return (true, users ?? new List<User>(), "Успешно");
            }

            return (false, new List<User>(), $"Ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, new List<User>(), $"Ошибка подключения: {ex.Message}");
        }
    }

    // Получить пользователя по ID
    public static async Task<(bool Success, User User, string Message)> GetUserAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"user/Item?id={userId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var user = JsonSerializer.Deserialize<User>(responseContent, options);
                return (true, user, "Успешно");
            }
            else
            {
                return (false, null, $"Ошибка: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return (false, null, $"Ошибка подключения: {ex.Message}");
        }
    }

    // Добавить пользователя
    public static async Task<(bool Success, string Message)> CreateUserAsync(User user)
    {
        try
        {
            var formData = new MultipartFormDataContent
        {
            { new StringContent(user.Email ?? ""), "Email" },
            { new StringContent(user.Name ?? ""), "Name" },
            { new StringContent(user.Password ?? ""), "Password" },
            { new StringContent(user.IsActive.ToString()), "IsActive" },
            { new StringContent(user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")), "CreatedAt" },
            { new StringContent(user.Post.ToString()), "Post" },
        };

            var response = await _httpClient.PutAsync("user/Add", formData);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Пользователь добавлен успешно");

            return (false, $"Ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }

    // Обновить пользователя
    public static async Task<(bool Success, string Message)> UpdateUserAsync(User user)
    {
        try
        {
            var formData = new MultipartFormDataContent
        {
            { new StringContent(user.Id.ToString()), "Id" },
            { new StringContent(user.Email ?? ""), "Email" },
            { new StringContent(user.Name ?? ""), "Name" },
            { new StringContent(user.Password ?? ""), "Password" },
            { new StringContent(user.IsActive.ToString()), "IsActive" },
            { new StringContent(user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")), "CreatedAt" }
        };

            var response = await _httpClient.PutAsync("user/Update", formData);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Пользователь обновлен успешно");

            // Если пользователь не найден
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return (false, $"Пользователь не найден: {errorMessage}");
            }

            return (false, $"Ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }

    // Удалить пользователя
    public static async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"user/Delete?id={userId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Пользователь удален успешно");

            // Если пользователь не найден
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return (false, $"Пользователь не найден: {errorMessage}");
            }

            return (false, $"Ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }

    // Блокировать/разблокировать пользователя
    public static async Task<(bool Success, string Message)> ToggleUserActiveAsync(int userId, bool isActive)
    {
        try
        {
            var (success, user, message) = await GetUserAsync(userId);

            if (!success)
                return (false, message);

            user.IsActive = isActive;
            return await UpdateUserAsync(user);
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }
    // ========== ТРАНЗАКЦИИ ==========
    // Получить все транзакции
    public static async Task<(bool Success, List<Transaction> Transactions, string Message)> GetTransactionsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("transaction/List");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var transactions = JsonSerializer.Deserialize<List<Transaction>>(responseContent, options);
                return (true, transactions ?? new List<Transaction>(), "Успешно");
            }
            else
            {
                return (false, new List<Transaction>(), $"Ошибка: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return (false, new List<Transaction>(), $"Ошибка подключения: {ex.Message}");
        }
    }

    // Получить транзакцию по ID
    public static async Task<(bool Success, Transaction Transaction, string Message)> GetTransactionAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"transaction/Item?id={id}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var transaction = JsonSerializer.Deserialize<Transaction>(responseContent, options);
                return (true, transaction, "Успешно");
            }
            else
            {
                return (false, null, $"Ошибка: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return (false, null, $"Ошибка подключения: {ex.Message}");
        }
    }

    // Добавить транзакцию
    public static async Task<(bool Success, string Message)> AddTransactionAsync(Transaction transaction)
    {
        try
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(transaction.UserId.ToString()), "UserId" },
                { new StringContent(transaction.Amount.ToString()), "Amount" },
                { new StringContent(transaction.Description ?? ""), "Description" },
                { new StringContent(transaction.Type ?? ""), "Type" },
                { new StringContent(transaction.Date.ToString("yyyy-MM-ddTHH:mm:ss")), "Date" }
            };

            var response = await _httpClient.PutAsync("transaction/Add", formData);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Транзакция добавлена успешно");

            return (false, $"Ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }

    // Обновить транзакцию
    public static async Task<(bool Success, string Message)> UpdateTransactionAsync(Transaction transaction)
    {
        try
        {
            var formData = new MultipartFormDataContent
        {
            { new StringContent(transaction.Id.ToString()), "Id" },
            { new StringContent(transaction.UserId.ToString()), "UserId" },
            { new StringContent(transaction.Amount.ToString()), "Amount" },
            { new StringContent(transaction.Description ?? ""), "Description" },
            { new StringContent(transaction.Type ?? ""), "Type" },
            { new StringContent(transaction.Date.ToString("yyyy-MM-ddTHH:mm:ss")), "Date" }
        };

            var response = await _httpClient.PutAsync("transaction/Update", formData);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Транзакция обновлена успешно");

            // Если транзакция не найдена
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return (false, $"Транзакция не найдена: {errorMessage}");
            }

            return (false, $"Ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }

    // Удалить транзакцию
    public static async Task<(bool Success, string Message)> DeleteTransactionAsync(int transactionId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"transaction/Delete?id={transactionId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Транзакция удалена успешно");

            // Если транзакция не найдена
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return (false, $"Транзакция не найдена: {errorMessage}");
            }

            return (false, $"Ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }// ========== ЦЕЛИ ==========

    // Получить все цели
    public static async Task<(bool Success, List<Goal> Goals, string Message)> GetGoalsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("goal/List");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var goals = JsonSerializer.Deserialize<List<Goal>>(responseContent, options);
                return (true, goals ?? new List<Goal>(), "Успешно");
            }
            else
            {
                return (false, new List<Goal>(), $"Ошибка: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return (false, new List<Goal>(), $"Ошибка подключения: {ex.Message}");
        }
    }

    // Получить цель по ID
    public static async Task<(bool Success, Goal Goal, string Message)> GetGoalAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"goal/Item?id={id}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var goal = JsonSerializer.Deserialize<Goal>(responseContent, options);
                return (true, goal, "Успешно");
            }
            else
            {
                return (false, null, $"Ошибка: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return (false, null, $"Ошибка подключения: {ex.Message}");
        }
    }

    // Создать цель
    public static async Task<(bool Success, string Message)> CreateGoalAsync(Goal goal)
    {
        try
        {
            var formData = new MultipartFormDataContent
        {
            { new StringContent(goal.Name ?? ""), "Name" },
            { new StringContent(goal.TargetAmount.ToString()), "TargetAmount" },
            { new StringContent(goal.CurrentAmount.ToString()), "CurrentAmount" },
            { new StringContent(goal.Deadline?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""), "Deadline" },
            { new StringContent(goal.UserId.ToString()), "UserId" }
        };

            var response = await _httpClient.PutAsync("goal/Add", formData);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Цель создана успешно");

            return (false, $"Ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }

    // Добавить деньги к цели
    public static async Task<(bool Success, string Message)> AddMoneyToGoalAsync(int goalId, decimal amount)
    {
        try
        {
            var formData = new MultipartFormDataContent
        {
            { new StringContent(goalId.ToString()), "GoalId" },
            { new StringContent(amount.ToString()), "Amount" }
        };

            var response = await _httpClient.PutAsync("goal/AddMoney", formData);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Средства добавлены к цели успешно");

            return (false, $"Ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }

    // Обновить цель
    public static async Task<(bool Success, string Message)> UpdateGoalAsync(Goal goal)
    {
        try
        {
            var formData = new MultipartFormDataContent
        {
            { new StringContent(goal.Id.ToString()), "Id" },
            { new StringContent(goal.Name ?? ""), "Name" },
            { new StringContent(goal.TargetAmount.ToString()), "TargetAmount" },
            { new StringContent(goal.CurrentAmount.ToString()), "CurrentAmount" },
            { new StringContent(goal.Deadline?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""), "Deadline" },
            { new StringContent(goal.UserId.ToString()), "UserId" }
        };

            var response = await _httpClient.PutAsync("goal/Update", formData);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Цель обновлена успешно");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return (false, $"Цель не найдена: {errorMessage}");
            }

            return (false, $"Ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }

    // Удалить цель
    public static async Task<(bool Success, string Message)> DeleteGoalAsync(int goalId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"goal/Delete?id={goalId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Цель удалена успешно");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return (false, $"Цель не найдена: {errorMessage}");
            }

            return (false, $"Ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }

    public static void Logout()
    {
        CurrentUserEmail = null;
        CurrentUserName = null;
        CurrentUserId = 0;
    }
}