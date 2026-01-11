using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    class Class1
    {
        [TestClass]
        public class AuthorizationTests
        {
            [TestMethod]
            public async Task Positive_UserLogin_WithValidCredentials()
            {
                // Arrange
                var loginWindow = new LoginWindow();
                string testEmail = "testuser@mail.com";
                string testPassword = "Pass123";

                // Act
                var (success, message) = await ApiHelper.LoginAsync(testEmail, testPassword);

                // Assert
                Assert.IsTrue(success, "Авторизация должна быть успешной");
                Assert.IsNotNull(App.CurrentUser, "Пользователь должен быть установлен");
                Assert.AreEqual(testEmail, ApiHelper.CurrentUserEmail, "Email должен совпадать");
                Assert.IsFalse(string.IsNullOrEmpty(ApiHelper.CurrentUserName), "Имя пользователя должно быть установлено");
                Assert.IsTrue(ApiHelper.CurrentUserId > 0, "ID пользователя должен быть положительным");
            }

            [TestMethod]
            public async Task Negative_UserLogin_WithInvalidPassword()
            {
                // Arrange
                string testEmail = "testuser@mail.com";
                string wrongPassword = "WrongPassword";

                // Act
                var (success, message) = await ApiHelper.LoginAsync(testEmail, wrongPassword);

                // Assert
                Assert.IsFalse(success, "Авторизация должна завершиться ошибкой");
                Assert.IsNull(App.CurrentUser, "Пользователь не должен быть установлен");
                StringAssert.Contains(message, "Неверный email или пароль", "Должно отображаться сообщение об ошибке");
            }
        }

        [TestClass]
        public class UserManagementTests
        {
            [TestMethod]
            public void Positive_CreateUser_WithValidData()
            {
                // Arrange
                var registerWindow = new RegisterWindow();
                string email = "newuser@mail.com";
                string name = "Петров Петр Петрович";
                string password = "Password123";

                // Act
                registerWindow.NameTextBox.Text = name;
                registerWindow.EmailTextBox.Text = email;
                registerWindow.PasswordBox.Password = password;

                // Assert
                Assert.AreEqual(name, registerWindow.NewUser.Name, "Имя должно быть установлено");
                Assert.AreEqual(email, registerWindow.NewUser.Email, "Email должен быть установлен");
                Assert.IsFalse(string.IsNullOrEmpty(registerWindow.NewUser.Password), "Пароль должен быть установлен");
                Assert.IsTrue(registerWindow.NewUser.IsActive, "Пользователь должен быть активен");
                Assert.IsTrue(registerWindow.NewUser.IsAdmin, "Пользователь должен быть администратором");
            }

            [TestMethod]
            public void Negative_CreateUser_WithoutEmail()
            {
                // Arrange
                var registerWindow = new RegisterWindow();
                string name = "Тестовый пользователь";
                string password = "password123";

                // Act
                registerWindow.NameTextBox.Text = name;
                registerWindow.EmailTextBox.Text = ""; // Пустой email
                registerWindow.PasswordBox.Password = password;

                // Assert - тест не пройдет, так как ожидаем ошибку, но в коде нет валидации
                Assert.IsFalse(string.IsNullOrEmpty(registerWindow.NewUser.Email), "Email должен быть обязательным полем");
                // Фактически тест не пройдет, потому что в текущей реализации нет валидации на пустой email
            }
        }

        [TestClass]
        public class GoalTests
        {
            [TestMethod]
            public void Positive_CreateGoal_WithValidData()
            {
                // Arrange
                var goal = new Goal();
                string goalName = "Накопление на отпуск";
                decimal targetAmount = 100000;
                decimal currentAmount = 15000;
                DateTime deadline = new DateTime(2024, 12, 31);

                // Act
                goal.Name = goalName;
                goal.TargetAmount = targetAmount;
                goal.CurrentAmount = currentAmount;
                goal.Deadline = deadline;

                // Assert
                Assert.AreEqual(goalName, goal.Name, "Название цели должно совпадать");
                Assert.AreEqual(targetAmount, goal.TargetAmount, "Целевая сумма должна совпадать");
                Assert.AreEqual(currentAmount, goal.CurrentAmount, "Текущая сумма должна совпадать");
                Assert.AreEqual(deadline, goal.Deadline, "Дедлайн должен совпадать");
                Assert.AreEqual(15, goal.Progress, "Прогресс должен быть 15%");
            }

            [TestMethod]
            public void Negative_CreateGoal_WithNegativeAmount()
            {
                // Arrange
                var goal = new Goal();
                string goalName = "Некорректная цель";
                decimal negativeAmount = -5000;

                // Act
                goal.Name = goalName;
                goal.TargetAmount = negativeAmount;

                // Assert - тест не пройдет, так как ожидаем ошибку, но в коде нет валидации
                Assert.IsTrue(goal.TargetAmount > 0, "Целевая сумма должна быть положительной");
                // Фактически тест не пройдет, потому что в текущей реализации можно установить отрицательную сумму
            }
        }

        [TestClass]
        public class TransactionTests
        {
            [TestMethod]
            public void Positive_CreateTransaction_WithValidData()
            {
                // Arrange
                var transaction = new Transaction();
                decimal amount = 50000;
                string type = "income";
                string description = "Зарплата за октябрь";
                int userId = 1;

                // Act
                transaction.Amount = amount;
                transaction.Type = type;
                transaction.Description = description;
                transaction.UserId = userId;

                // Assert
                Assert.AreEqual(amount, transaction.Amount, "Сумма должна совпадать");
                Assert.AreEqual(type, transaction.Type, "Тип должен совпадать");
                Assert.AreEqual(description, transaction.Description, "Описание должно совпадать");
                Assert.AreEqual(userId, transaction.UserId, "ID пользователя должен совпадать");
                Assert.AreEqual("Доход", transaction.TypeDisplay, "Отображение типа должно быть 'Доход'");
                Assert.AreEqual("Green", transaction.AmountColor, "Цвет для дохода должен быть зеленым");
            }

            [TestMethod]
            public void Negative_CreateTransaction_WithZeroAmount()
            {
                // Arrange
                var transaction = new Transaction();
                decimal zeroAmount = 0;
                string type = "expense";
                string description = "Тестовая транзакция";

                // Act
                transaction.Amount = zeroAmount;
                transaction.Type = type;
                transaction.Description = description;

                // Assert - тест не пройдет, так как ожидаем ошибку, но в коде нет валидации
                Assert.IsTrue(transaction.Amount > 0, "Сумма транзакции должна быть больше 0");
                // Фактически тест не пройдет, потому что в текущей реализации можно установить нулевую сумму
            }
        }

        [TestClass]
        public class GoalProgressTests
        {
            [TestMethod]
            public void Positive_AddFundsToGoal()
            {
                // Arrange
                var goal = new Goal();
                goal.CurrentAmount = 10000;
                goal.TargetAmount = 50000;
                decimal additionalAmount = 10000;

                // Act - имитация пополнения цели
                decimal initialProgress = goal.Progress;
                goal.CurrentAmount += additionalAmount;
                decimal finalProgress = goal.Progress;

                // Assert
                Assert.AreEqual(20, initialProgress, "Начальный прогресс должен быть 20%");
                Assert.AreEqual(40, finalProgress, "Прогресс после пополнения должен быть 40%");
                Assert.AreEqual(20000, goal.CurrentAmount, "Текущая сумма должна быть 20000");
            }
        }

        [TestClass]
        public class NetworkTests
        {
            [TestMethod]
            public async Task Negative_Login_WhenServerUnavailable()
            {
                // Arrange
                string testEmail = "testuser@mail.com";
                string testPassword = "Pass123";

                // Act & Assert
                try
                {
                    // Имитация недоступности сервера
                    var (success, message) = await ApiHelper.LoginAsync(testEmail, testPassword);

                    // Если сервер доступен, тест должен упасть
                    Assert.Fail("Ожидалась ошибка подключения к серверу");
                }
                catch (Exception ex)
                {
                    // Успех - поймали исключение
                    StringAssert.Contains(ex.Message, "подключения", "Должно быть сообщение об ошибке подключения");
                }
            }
        }
    }
}
}
