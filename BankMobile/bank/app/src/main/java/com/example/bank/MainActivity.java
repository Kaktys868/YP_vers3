package com.example.bank;

import static com.example.bank.Users.UsersContext.getCurrentUser;
import static com.example.bank.Users.UsersContext.setCurrentUser;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.os.AsyncTask;
import android.os.Bundle;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.RadioButton;
import android.graphics.Color;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.bank.Models.TransactionModel;
import com.example.bank.Models.GoalModel;
import com.example.bank.Transactions.TransactionContext;
import com.example.bank.Goals.GoalContext;
import java.util.ArrayList;
import java.util.List;
import android.content.Context;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import com.example.bank.Models.LoginModel;
import com.example.bank.Models.UsersModel;
import com.example.bank.Users.UsersContext;
import com.google.gson.Gson;
import org.jsoup.Connection;
import org.jsoup.Jsoup;
import java.io.IOException;
import com.google.android.material.bottomnavigation.BottomNavigationView;

public class MainActivity extends AppCompatActivity {

    public UsersModel currentUser;
    private String login, password;
    private TextView userInfoText;
    private BottomNavigationView bottomNavigation;
    private RecyclerView goalsRecyclerView;
    private TextView emptyStateText;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        try {
            setContentView(R.layout.activity_welcome);

            UsersModel savedUser = getCurrentUser(this);
            if (savedUser != null && savedUser.id > 0) {
                currentUser = savedUser;
                setContentView(R.layout.activity_main);
                setupBottomNavigation();
                updateMainUI();
            } else {
                setupWelcomeButtons();
            }
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка запуска приложения", Toast.LENGTH_SHORT).show();
            try {
                setContentView(R.layout.activity_welcome);
                setupWelcomeButtons();
            } catch (Exception ex) {
            }
        }
    }

    private void setupWelcomeButtons() {
        try {
            Button loginButton = findViewById(R.id.loginButton);
            Button registerButton = findViewById(R.id.registerButton);

            if (loginButton != null) {
                loginButton.setOnClickListener(v -> fromWelcome(v));
            }

            if (registerButton != null) {
                registerButton.setOnClickListener(v -> toRegister(v));
            }
        } catch (Exception e) {
        }
    }

    private void updateBalance() {
        TextView balanceText = findViewById(R.id.balanceText);
        if (balanceText == null || currentUser == null) return;

        TransactionContext.getAll(new TransactionContext.GetAllCallback() {
            @Override
            public void onSuccess(List<TransactionModel> allTransactions) {
                int balance = calculateUserBalance(allTransactions);

                runOnUiThread(() -> {
                    if (balanceText != null) {
                        String formattedBalance = formatBalance(balance);
                        balanceText.setText(formattedBalance + " ₽");

                        if (balance >= 0) {
                            balanceText.setTextColor(Color.parseColor("#4CAF50"));
                        } else {
                            balanceText.setTextColor(Color.parseColor("#F44336"));
                        }
                    }
                });
            }

            @Override
            public void onError(String error) {
                runOnUiThread(() -> {
                    if (balanceText != null) {
                        balanceText.setText("Ошибка загрузки");
                        balanceText.setTextColor(Color.parseColor("#FF9800"));
                    }
                });
            }
        });
    }

    private int calculateUserBalance(List<TransactionModel> allTransactions) {
        if (currentUser == null || allTransactions == null) return 0;

        int balance = 0;
        for (TransactionModel tx : allTransactions) {
            if (tx.userId == currentUser.id) {
                if ("income".equals(tx.type)) {
                    balance += tx.amount;
                } else if ("expense".equals(tx.type)) {
                    balance -= tx.amount;
                }
            }
        }
        return balance;
    }

    private String formatBalance(int balance) {
        return String.format("%,d", balance).replace(',', ' ');
    }

    private void updateBalanceAfterTransaction(int amount, String type) {
        TextView balanceText = findViewById(R.id.balanceText);
        if (balanceText == null) return;

        try {
            String currentText = balanceText.getText().toString();
            currentText = currentText.replace(" ₽", "").replace(" ", "");
            int currentBalance = 0;

            if (!currentText.isEmpty() && !currentText.equals("Ошибка загрузки")) {
                try {
                    currentBalance = Integer.parseInt(currentText);
                } catch (NumberFormatException e) {
                    currentBalance = 0;
                }
            }

            int newBalance;
            if ("income".equals(type)) {
                newBalance = currentBalance + amount;
            } else {
                newBalance = currentBalance - amount;
            }

            String formattedBalance = formatBalance(newBalance);
            balanceText.setText(formattedBalance + " ₽");

            if (newBalance >= 0) {
                balanceText.setTextColor(Color.parseColor("#4CAF50"));
            } else {
                balanceText.setTextColor(Color.parseColor("#F44336"));
            }
        } catch (Exception e) {
            updateBalance();
        }
    }

    private void setupBottomNavigation() {
        try {
            bottomNavigation = findViewById(R.id.bottomNavigation);
            if (bottomNavigation != null) {
                bottomNavigation.setOnNavigationItemSelectedListener(item -> {
                    int itemId = item.getItemId();

                    if (itemId == R.id.nav_home) {
                        onMain(null);
                        return true;
                    } else if (itemId == R.id.nav_goals) {
                        openGoals(null);
                        return true;
                    } else if (itemId == R.id.nav_profile) {
                        openProfile(null);
                        return true;
                    }
                    return false;
                });
            }
        } catch (Exception e) {
        }
    }

    public void fromWelcome(View view) {
        try {
            setContentView(R.layout.activity_login);
            setupBottomNavigation();
            setupLoginButton();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка загрузки экрана входа", Toast.LENGTH_SHORT).show();
        }
    }

    public void toRegister(View view) {
        try {
            setContentView(R.layout.activity_register);
            setupBottomNavigation();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка загрузки экрана регистрации", Toast.LENGTH_SHORT).show();
        }
    }

    private void setupLoginButton() {
        try {
            Button loginButton = findViewById(R.id.loginButton);
            if (loginButton != null) {
                loginButton.setOnClickListener(v -> {
                    try {
                        EditText etEmail = findViewById(R.id.emailEditText);
                        EditText etPassword = findViewById(R.id.passwordEditText);

                        if (etEmail == null || etPassword == null) {
                            Toast.makeText(this, "Ошибка загрузки полей ввода", Toast.LENGTH_SHORT).show();
                            return;
                        }

                        login = etEmail.getText().toString().trim();
                        password = etPassword.getText().toString().trim();

                        if (login.isEmpty() || password.isEmpty()) {
                            Toast.makeText(this, "Заполните все поля", Toast.LENGTH_SHORT).show();
                            return;
                        }

                        new AuthConnection().execute();
                    } catch (Exception e) {
                        Toast.makeText(this, "Ошибка входа", Toast.LENGTH_SHORT).show();
                    }
                });
            }

            TextView registerLink = findViewById(R.id.registerLink);
            if (registerLink != null) {
                registerLink.setOnClickListener(v -> toRegister(v));
            }
        } catch (Exception e) {
        }
    }

    private void setupRegisterAndLoginLink() {
        try {
            Button registerButton = findViewById(R.id.registerButton);
            if (registerButton != null) {
                registerButton.setOnClickListener(v -> onRegister(v));
            }

            TextView loginLink = findViewById(R.id.loginLink);
            if (loginLink != null) {
                loginLink.setOnClickListener(v -> fromWelcome(v));
            }
        } catch (Exception e) {
        }
    }

    public void onRegister(View view) {
        try {
            if (view == null) {
                Toast.makeText(this, "Ошибка: view is null", Toast.LENGTH_SHORT).show();
                return;
            }

            View rootView = view.getRootView();

            EditText etName = rootView.findViewById(R.id.nameEditText);
            EditText etEmail = rootView.findViewById(R.id.emailEditText);
            EditText etPassword = rootView.findViewById(R.id.passwordEditText);
            EditText etConfirmPassword = rootView.findViewById(R.id.confirmPasswordEditText);

            if (etName == null || etEmail == null || etPassword == null || etConfirmPassword == null) {
                Toast.makeText(this, "Ошибка загрузки полей ввода", Toast.LENGTH_SHORT).show();
                return;
            }

            String name = etName.getText().toString().trim();
            String email = etEmail.getText().toString().trim();
            String password = etPassword.getText().toString().trim();
            String confirmPassword = etConfirmPassword.getText().toString().trim();

            if (name.isEmpty() || email.isEmpty() || password.isEmpty() || confirmPassword.isEmpty()) {
                Toast.makeText(this, "Заполните все поля", Toast.LENGTH_SHORT).show();
                return;
            }

            if (!android.util.Patterns.EMAIL_ADDRESS.matcher(email).matches()) {
                Toast.makeText(this, "Введите корректный email", Toast.LENGTH_SHORT).show();
                return;
            }

            if (password.length() < 6) {
                Toast.makeText(this, "Пароль должен быть не менее 6 символов", Toast.LENGTH_SHORT).show();
                return;
            }

            if (!password.equals(confirmPassword)) {
                Toast.makeText(this, "Пароли не совпадают", Toast.LENGTH_SHORT).show();
                return;
            }

            Toast.makeText(this, "Регистрация...", Toast.LENGTH_SHORT).show();
            performRegistration(name, email, password);
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка регистрации: " + e.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

    private void performRegistration(String name, String email, String password) {
        try {
            UsersModel newUser = new UsersModel();
            newUser.Name = name;
            newUser.Email = email;
            newUser.Password = password;
            newUser.IsActive = true;
            newUser.Post = 1;

            // Используем НОВЫЙ метод add() вместо AddUserTask
            UsersContext.add(newUser, new UsersContext.ActionCallback() {
                @Override
                public void onSuccess() {
                    runOnUiThread(() -> {
                        Toast.makeText(MainActivity.this,
                                "Регистрация успешна! Теперь войдите.",
                                Toast.LENGTH_SHORT).show();
                        fromWelcome(null);
                    });
                }

                @Override
                public void onError(String error) {
                    runOnUiThread(() -> {
                        new AlertDialog.Builder(MainActivity.this)
                                .setTitle("Ошибка регистрации")
                                .setMessage("Не удалось зарегистрироваться.\n\n" +
                                        "Возможные причины:\n" +
                                        "1. Email уже занят\n" +
                                        "2. Проблемы с сервером\n" +
                                        "3. Некорректные данные\n\n" +
                                        "Ошибка: " + error)
                                .setPositiveButton("OK", null)
                                .show();
                    });
                }
            });
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка: " + e.getMessage(), Toast.LENGTH_LONG).show();
        }
    }

    private class AuthConnection extends AsyncTask<Void, Void, Connection.Response> {

        @Override
        protected Connection.Response doInBackground(Void... voids) {
            try {
                LoginModel loginModel = new LoginModel(login, password);
                String jsonBody = new Gson().toJson(loginModel);

                return Jsoup.connect("http://10.0.2.2:5081/api/Auth/login")
                        .ignoreContentType(true)
                        .requestBody(jsonBody)
                        .header("Content-Type", "application/json")
                        .method(Connection.Method.POST)
                        .timeout(10000)
                        .execute();
            } catch (IOException e) {
                return null;
            } catch (Exception e) {
                return null;
            }
        }

        @Override
        protected void onPostExecute(Connection.Response response) {
            try {
                if (response == null) {
                    showErrorDialog("Ошибка подключения к серверу");
                    return;
                }

                if (response.statusCode() == 200) {
                    try {
                        String body = response.body();
                        if (body == null || body.trim().isEmpty()) {
                            showErrorDialog("Пустой ответ от сервера");
                            return;
                        }

                        com.google.gson.JsonObject json = com.google.gson.JsonParser.parseString(body).getAsJsonObject();

                        int userId = json.has("userId") ? json.get("userId").getAsInt() : 0;
                        String userName = json.has("name") ? json.get("name").getAsString() : "";
                        String userEmail = json.has("email") ? json.get("email").getAsString() : "";

                        if (userId <= 0) {
                            showErrorDialog("Некорректный ID пользователя");
                            return;
                        }

                        UsersModel user = new UsersModel();
                        user.id = userId;
                        user.Name = userName;
                        user.Email = userEmail;
                        user.IsActive = true;
                        user.Post = 1;

                        java.text.SimpleDateFormat sdf = new java.text.SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", java.util.Locale.getDefault());
                        user.CreatedAt = sdf.format(new java.util.Date());

                        setCurrentUser(MainActivity.this, user);
                        currentUser = user;

                        setContentView(R.layout.activity_main);
                        setupBottomNavigation();
                        updateMainUI();

                        Toast.makeText(MainActivity.this,
                                "Добро пожаловать, " + (!userName.isEmpty() ? userName : "Пользователь") + "!",
                                Toast.LENGTH_SHORT).show();

                    } catch (Exception e) {
                        showErrorDialog("Ошибка: " + e.getMessage());
                    }
                } else {
                    String msg = "Ошибка авторизации: " + response.statusCode();
                    if (response.body() != null && !response.body().trim().isEmpty()) {
                        try {
                            com.google.gson.JsonObject json = com.google.gson.JsonParser.parseString(response.body()).getAsJsonObject();
                            if (json.has("message")) {
                                msg = json.get("message").getAsString();
                            } else if (json.has("error")) {
                                msg = json.get("error").getAsString();
                            } else {
                                msg = response.body();
                            }
                        } catch (Exception e) {
                            msg = response.body();
                        }
                    }
                    showErrorDialog(msg);
                }
            } catch (Exception e) {
                Toast.makeText(MainActivity.this, "Ошибка обработки ответа", Toast.LENGTH_SHORT).show();
            }
        }
    }

    public void openGoals(View view) {
        try {
            setContentView(R.layout.activity_goals);
            setupBottomNavigation();

            goalsRecyclerView = findViewById(R.id.goalsRecyclerView);
            emptyStateText = findViewById(R.id.emptyStateText);

            if (currentUser == null) {
                currentUser = UsersContext.getCurrentUser(this);
            }

            loadUserGoals();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка загрузки целей", Toast.LENGTH_SHORT).show();
        }
    }

    public void onMain(View view) {
        try {
            setContentView(R.layout.activity_main);
            setupBottomNavigation();
            UsersModel savedUser = getCurrentUser(this);
            if (savedUser != null) {
                currentUser = savedUser;
                updateMainUI();
            } else {
                Toast.makeText(this, "Пользователь не найден", Toast.LENGTH_SHORT).show();
            }
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка загрузки главного экрана", Toast.LENGTH_SHORT).show();
        }
    }

    public void openProfile(View view) {
        try {
            setContentView(R.layout.activity_profile);
            setupBottomNavigation();

            UsersModel user = getCurrentUser(this);
            if (user != null) {
                TextView nameText = findViewById(R.id.userNameText);
                TextView emailText = findViewById(R.id.userEmailText);
                TextView dateText = findViewById(R.id.registrationDateText);
                TextView idText = findViewById(R.id.userIdText);

                if (nameText != null) {
                    nameText.setText(user.Name != null && !user.Name.isEmpty() ? user.Name : "Не указано");
                }

                if (emailText != null) {
                    emailText.setText(user.Email != null && !user.Email.isEmpty() ? user.Email : "Не указан");
                }

                if (dateText != null) {
                    String formattedDate = user.getFormattedDate();
                    dateText.setText("Дата регистрации: " + formattedDate);
                }

                if (idText != null) {
                    idText.setText("ID: " + user.id);
                }
            } else {
                Toast.makeText(this, "Пользователь не найден", Toast.LENGTH_SHORT).show();
            }
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка загрузки профиля: " + e.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

    public void exitprof(View view) {
        logout();
    }

    public void updateprof(View view) {
        try {
            showEditProfileDialog();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка обновления профиля", Toast.LENGTH_SHORT).show();
        }
    }

    public void deleteprof(View view) {
        try {
            showDeleteProfileDialog();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка удаления профиля", Toast.LENGTH_SHORT).show();
        }
    }

    public void addGoal(View view) {
        try {
            showAddGoalDialog();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка добавления цели", Toast.LENGTH_SHORT).show();
        }
    }

    public void addTransact(View view) {
        try {
            setContentView(R.layout.dialog_add_transaction);
            setupBottomNavigation();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка добавления транзакции", Toast.LENGTH_SHORT).show();
        }
    }

    public void AddTrans(View view) {
        try {
            EditText amountInput = findViewById(R.id.amountInput);
            EditText descriptionInput = findViewById(R.id.descriptionInput);
            RadioButton incomeRadio = findViewById(R.id.incomeRadio);

            if (amountInput == null || descriptionInput == null || incomeRadio == null) {
                Toast.makeText(this, "Ошибка загрузки полей", Toast.LENGTH_SHORT).show();
                return;
            }

            String amountStr = amountInput.getText().toString().trim();
            String description = descriptionInput.getText().toString().trim();

            if (amountStr.isEmpty() || description.isEmpty()) {
                Toast.makeText(this, "Заполните все поля", Toast.LENGTH_SHORT).show();
                return;
            }

            int amount;
            try {
                amount = Integer.parseInt(amountStr);
            } catch (NumberFormatException e) {
                Toast.makeText(this, "Некорректная сумма (должна быть целым числом)", Toast.LENGTH_SHORT).show();
                return;
            }

            String type = incomeRadio.isChecked() ? "income" : "expense";

            if (this.currentUser == null) {
                this.currentUser = UsersContext.getCurrentUser(this);
            }

            TransactionModel tx = new TransactionModel();
            tx.userId = currentUser.id;
            tx.amount = amount;
            tx.description = description;
            tx.date = TransactionContext.getCurrentIsoDate();
            tx.type = type;

            TransactionContext.add(tx, new TransactionContext.ActionCallback() {
                @Override
                public void onSuccess() {
                    runOnUiThread(() -> {
                        Toast.makeText(MainActivity.this, "Транзакция добавлена", Toast.LENGTH_SHORT).show();
                        updateBalanceAfterTransaction(amount, type);
                        onMain(null);
                    });
                }

                @Override
                public void onError(String error) {
                    runOnUiThread(() -> {
                        Toast.makeText(MainActivity.this, "Ошибка: " + error, Toast.LENGTH_LONG).show();
                    });
                }
            });
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка добавления транзакции", Toast.LENGTH_SHORT).show();
        }
    }

    public void Back(View view){
        try {
            onMain(null);
        } catch (Exception e) {
        }
    }

    private void updateMainUI() {
        try {
            userInfoText = findViewById(R.id.userInfoText);
            if (userInfoText != null && currentUser != null) {
                String userName = (currentUser.Name != null && !currentUser.Name.isEmpty())
                        ? currentUser.Name : "Пользователь";
                userInfoText.setText("👤 " + userName);

                userInfoText.setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        openProfile(v);
                    }
                });
            }

            updateBalance();

            RecyclerView recyclerView = findViewById(R.id.transactionsRecyclerView);
            if (recyclerView == null) {
                return;
            }

            recyclerView.setLayoutManager(new LinearLayoutManager(this));
            setupLoadingAdapter(recyclerView);
            loadUserTransactions(recyclerView);
        } catch (Exception e) {
        }
    }

    private void logout() {
        try {
            UsersContext.clearCurrentUser(this);
            setContentView(R.layout.activity_welcome);
            setupWelcomeButtons();
            Toast.makeText(this, "Вы вышли из системы", Toast.LENGTH_SHORT).show();
        } catch (Exception e) {
        }
    }

    private void loadUserGoals() {
        GoalContext.getAll(new GoalContext.GetAllCallback() {
            @Override
            public void onSuccess(List<GoalModel> allGoals) {
                runOnUiThread(() -> {
                    try {
                        List<GoalModel> userGoals = new ArrayList<>();
                        for (GoalModel goal : allGoals) {
                            if (goal.userId == currentUser.id) {
                                goal.progress = GoalContext.calculateProgress(goal.currentAmount, goal.targetAmount);
                                userGoals.add(goal);
                            }
                        }

                        if (userGoals.isEmpty()) {
                            showEmptyGoalsState();
                        } else {
                            setupGoalsAdapter(userGoals);
                        }
                    } catch (Exception e) {
                        setupErrorGoalsState("Ошибка обработки данных");
                    }
                });
            }

            @Override
            public void onError(String error) {
                runOnUiThread(() -> {
                    setupErrorGoalsState(error);
                });
            }
        });
    }

    private void showEmptyGoalsState() {
        try {
            if (emptyStateText != null) {
                emptyStateText.setVisibility(View.VISIBLE);
            }
            if (goalsRecyclerView != null) {
                goalsRecyclerView.setVisibility(View.GONE);
            }
        } catch (Exception e) {
        }
    }

    private void setupGoalsAdapter(List<GoalModel> goals) {
        try {
            if (emptyStateText != null) {
                emptyStateText.setVisibility(View.GONE);
            }
            if (goalsRecyclerView != null) {
                goalsRecyclerView.setVisibility(View.VISIBLE);
                goalsRecyclerView.setLayoutManager(new LinearLayoutManager(MainActivity.this));
                goalsRecyclerView.setAdapter(new GoalsAdapter(MainActivity.this, goals));
            }
        } catch (Exception e) {
        }
    }

    private void setupErrorGoalsState(String error) {
        try {
            if (emptyStateText != null) {
                emptyStateText.setText("Ошибка загрузки: " + error);
                emptyStateText.setVisibility(View.VISIBLE);
            }
            if (goalsRecyclerView != null) {
                goalsRecyclerView.setVisibility(View.GONE);
            }
        } catch (Exception e) {
        }
    }

    private void showAddGoalDialog() {
        try {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.setTitle("Новая цель");

            View dialogView = getLayoutInflater().inflate(R.layout.dialog_add_goal, null);
            builder.setView(dialogView);

            EditText titleInput = dialogView.findViewById(R.id.titleInput);
            EditText targetAmountInput = dialogView.findViewById(R.id.targetAmountInput);
            EditText currentAmountInput = dialogView.findViewById(R.id.currentAmountInput);
            EditText deadlineInput = dialogView.findViewById(R.id.deadlineInput);

            builder.setPositiveButton("Добавить", (dialog, which) -> {
                try {
                    String title = titleInput.getText().toString().trim();
                    String targetStr = targetAmountInput.getText().toString().trim();
                    String currentStr = currentAmountInput.getText().toString().trim();
                    String deadline = deadlineInput.getText().toString().trim();

                    if (title.isEmpty() || targetStr.isEmpty() || deadline.isEmpty()) {
                        Toast.makeText(MainActivity.this, "Заполните обязательные поля", Toast.LENGTH_SHORT).show();
                        return;
                    }

                    GoalModel goal = new GoalModel();
                    goal.userId = currentUser.id;
                    goal.name = title;
                    goal.targetAmount = Integer.parseInt(targetStr);
                    goal.currentAmount = currentStr.isEmpty() ? 0 : Integer.parseInt(currentStr);
                    goal.deadline = deadline;
                    goal.progress = GoalContext.calculateProgress(goal.currentAmount, goal.targetAmount);

                    GoalContext.add(goal, new GoalContext.ActionCallback() {
                        @Override
                        public void onSuccess() {
                            runOnUiThread(() -> {
                                Toast.makeText(MainActivity.this, "Цель добавлена", Toast.LENGTH_SHORT).show();
                                loadUserGoals();
                            });
                        }

                        @Override
                        public void onError(String error) {
                            runOnUiThread(() -> {
                                Toast.makeText(MainActivity.this, "Ошибка: " + error, Toast.LENGTH_LONG).show();
                            });
                        }
                    });
                } catch (NumberFormatException e) {
                    Toast.makeText(MainActivity.this, "Некорректная сумма", Toast.LENGTH_SHORT).show();
                } catch (Exception e) {
                    Toast.makeText(MainActivity.this, "Ошибка добавления цели", Toast.LENGTH_SHORT).show();
                }
            });

            builder.setNegativeButton("Отмена", null);
            builder.show();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка создания диалога", Toast.LENGTH_SHORT).show();
        }
    }

    public void onLogin(View view) {
        try {
            EditText etEmail = findViewById(R.id.emailEditText);
            EditText etPassword = findViewById(R.id.passwordEditText);

            if (etEmail == null || etPassword == null) {
                Toast.makeText(this, "Ошибка загрузки полей ввода", Toast.LENGTH_SHORT).show();
                return;
            }

            login = etEmail.getText().toString().trim();
            password = etPassword.getText().toString().trim();

            if (login.isEmpty() || password.isEmpty()) {
                Toast.makeText(this, "Заполните все поля", Toast.LENGTH_SHORT).show();
                return;
            }

            new AuthConnection().execute();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка входа", Toast.LENGTH_SHORT).show();
        }
    }

    private void showEditProfileDialog() {
        try {
            UsersModel user = getCurrentUser(this);
            if (user == null) return;

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.setTitle("Редактировать профиль");

            View dialogView = getLayoutInflater().inflate(R.layout.dialog_edit_profile, null);
            builder.setView(dialogView);

            TextView currentNameText = dialogView.findViewById(R.id.currentNameText);
            TextView currentEmailText = dialogView.findViewById(R.id.currentEmailText);
            EditText newNameInput = dialogView.findViewById(R.id.newNameInput);
            EditText newEmailInput = dialogView.findViewById(R.id.newEmailInput);

            if (currentNameText != null) currentNameText.setText("Текущее имя: " + user.Name);
            if (currentEmailText != null) currentEmailText.setText("Текущий email: " + user.Email);

            builder.setPositiveButton("Сохранить", (dialog, which) -> {
                try {
                    String newName = newNameInput.getText().toString().trim();
                    String newEmail = newEmailInput.getText().toString().trim();

                    if (newName.isEmpty() && newEmail.isEmpty()) {
                        Toast.makeText(MainActivity.this,
                                "Введите хотя бы одно поле для изменения",
                                Toast.LENGTH_SHORT).show();
                        return;
                    }

                    // Создаем пользователя для обновления
                    UsersModel updatedUser = new UsersModel();
                    updatedUser.id = user.id;
                    updatedUser.Email = !newEmail.isEmpty() ? newEmail : user.Email;
                    updatedUser.Name = !newName.isEmpty() ? newName : user.Name;
                    updatedUser.Password = ""; // Пустой пароль - не меняем
                    updatedUser.CreatedAt = user.CreatedAt; // Оставляем как есть
                    updatedUser.IsActive = user.IsActive;
                    updatedUser.Post = user.Post;

                    // Используем НОВЫЙ метод update() вместо UpdateUserTask
                    UsersContext.update(updatedUser, new UsersContext.ActionCallback() {
                        @Override
                        public void onSuccess() {
                            runOnUiThread(() -> {
                                // Обновляем локальные данные
                                user.Name = updatedUser.Name;
                                user.Email = updatedUser.Email;
                                setCurrentUser(MainActivity.this, user);
                                currentUser = user;

                                Toast.makeText(MainActivity.this,
                                        "Профиль успешно обновлен!",
                                        Toast.LENGTH_SHORT).show();

                                openProfile(null);
                            });
                        }

                        @Override
                        public void onError(String error) {
                            runOnUiThread(() -> {
                                Toast.makeText(MainActivity.this,
                                        "Ошибка: " + error,
                                        Toast.LENGTH_LONG).show();
                            });
                        }
                    });
                } catch (Exception e) {
                    Toast.makeText(MainActivity.this,
                            "Ошибка: " + e.getMessage(),
                            Toast.LENGTH_SHORT).show();
                }
            });

            builder.setNegativeButton("Отмена", null);
            builder.show();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка редактирования профиля", Toast.LENGTH_SHORT).show();
        }
    }

    private void showDeleteProfileDialog() {
        try {
            new AlertDialog.Builder(this)
                    .setTitle("Удаление профиля")
                    .setMessage("Вы уверены, что хотите удалить свой профиль? Это действие нельзя отменить.")
                    .setPositiveButton("Удалить", (dialog, which) -> {
                        try {
                            if (currentUser != null) {
                                // Используем НОВЫЙ метод delete() вместо DeleteUserTask
                                UsersContext.delete(currentUser.id, new UsersContext.ActionCallback() {
                                    @Override
                                    public void onSuccess() {
                                        runOnUiThread(() -> {
                                            UsersContext.clearCurrentUser(MainActivity.this);
                                            Toast.makeText(MainActivity.this, "Профиль удален", Toast.LENGTH_SHORT).show();
                                            setContentView(R.layout.activity_welcome);
                                            setupWelcomeButtons();
                                        });
                                    }

                                    @Override
                                    public void onError(String error) {
                                        runOnUiThread(() -> {
                                            Toast.makeText(MainActivity.this, "Ошибка: " + error, Toast.LENGTH_LONG).show();
                                        });
                                    }
                                });
                            }
                        } catch (Exception e) {
                            Toast.makeText(MainActivity.this, "Ошибка удаления профиля", Toast.LENGTH_SHORT).show();
                        }
                    })
                    .setNegativeButton("Отмена", null)
                    .show();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка удаления профиля", Toast.LENGTH_SHORT).show();
        }
    }

    private void loadUserTransactions(RecyclerView recyclerView) {
        TransactionContext.getAll(new TransactionContext.GetAllCallback() {
            @Override
            public void onSuccess(List<TransactionModel> allTransactions) {
                runOnUiThread(() -> {
                    try {
                        List<TransactionModel> userTransactions = new ArrayList<>();
                        int totalBalance = 0;

                        for (TransactionModel tx : allTransactions) {
                            if (tx.userId == currentUser.id) {
                                userTransactions.add(tx);
                                if ("income".equals(tx.type)) {
                                    totalBalance += tx.amount;
                                } else if ("expense".equals(tx.type)) {
                                    totalBalance -= tx.amount;
                                }
                            }
                        }

                        updateBalanceTextView(totalBalance);

                        if (userTransactions.isEmpty()) {
                            setupEmptyAdapter(recyclerView);
                        } else {
                            setupDataAdapter(recyclerView, userTransactions);
                        }
                    } catch (Exception e) {
                        setupErrorAdapter(recyclerView, "Ошибка обработки данных");
                    }
                });
            }

            @Override
            public void onError(String error) {
                runOnUiThread(() -> setupErrorAdapter(recyclerView, error));
            }
        });
    }

    private void updateBalanceTextView(int balance) {
        TextView balanceText = findViewById(R.id.balanceText);
        if (balanceText != null) {
            String formattedBalance = formatBalance(balance);
            balanceText.setText(formattedBalance + " ₽");

            if (balance >= 0) {
                balanceText.setTextColor(Color.parseColor("#4CAF50"));
            } else {
                balanceText.setTextColor(Color.parseColor("#F44336"));
            }
        }
    }

    private void setupLoadingAdapter(RecyclerView recyclerView) {
        try {
            RecyclerView.Adapter<RecyclerView.ViewHolder> adapter = new RecyclerView.Adapter<RecyclerView.ViewHolder>() {
                @NonNull
                @Override
                public RecyclerView.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
                    TextView textView = new TextView(MainActivity.this);
                    textView.setPadding(50, 30, 50, 30);
                    textView.setTextSize(16);
                    textView.setGravity(17);
                    textView.setLayoutParams(new RecyclerView.LayoutParams(
                            RecyclerView.LayoutParams.MATCH_PARENT,
                            120
                    ));
                    return new RecyclerView.ViewHolder(textView) {};
                }

                @Override
                public void onBindViewHolder(@NonNull RecyclerView.ViewHolder holder, int position) {
                    TextView textView = (TextView) holder.itemView;
                    textView.setText("Загрузка транзакций...");
                    textView.setTextColor(Color.GRAY);
                }

                @Override
                public int getItemCount() {
                    return 1;
                }
            };

            recyclerView.setAdapter(adapter);
        } catch (Exception e) {
        }
    }

    private void setupEmptyAdapter(RecyclerView recyclerView) {
        try {
            RecyclerView.Adapter<RecyclerView.ViewHolder> adapter = new RecyclerView.Adapter<RecyclerView.ViewHolder>() {
                @NonNull
                @Override
                public RecyclerView.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
                    TextView textView = new TextView(MainActivity.this);
                    textView.setPadding(50, 30, 50, 30);
                    textView.setTextSize(16);
                    textView.setGravity(17);
                    textView.setLayoutParams(new RecyclerView.LayoutParams(
                            RecyclerView.LayoutParams.MATCH_PARENT,
                            120
                    ));
                    return new RecyclerView.ViewHolder(textView) {};
                }

                @Override
                public void onBindViewHolder(@NonNull RecyclerView.ViewHolder holder, int position) {
                    TextView textView = (TextView) holder.itemView;
                    textView.setText("Нет транзакций\nНажмите 'Добавить транзакцию'");
                    textView.setTextColor(Color.DKGRAY);
                }

                @Override
                public int getItemCount() {
                    return 1;
                }
            };

            recyclerView.setAdapter(adapter);
        } catch (Exception e) {
        }
    }

    private void setupErrorAdapter(RecyclerView recyclerView, String error) {
        try {
            RecyclerView.Adapter<RecyclerView.ViewHolder> adapter = new RecyclerView.Adapter<RecyclerView.ViewHolder>() {
                @NonNull
                @Override
                public RecyclerView.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
                    TextView textView = new TextView(MainActivity.this);
                    textView.setPadding(50, 30, 50, 30);
                    textView.setTextSize(16);
                    textView.setGravity(17);
                    textView.setLayoutParams(new RecyclerView.LayoutParams(
                            RecyclerView.LayoutParams.MATCH_PARENT,
                            120
                    ));
                    return new RecyclerView.ViewHolder(textView) {};
                }

                @Override
                public void onBindViewHolder(@NonNull RecyclerView.ViewHolder holder, int position) {
                    TextView textView = (TextView) holder.itemView;
                    textView.setText("Ошибка загрузки:\n" + error);
                    textView.setTextColor(Color.RED);
                }

                @Override
                public int getItemCount() {
                    return 1;
                }
            };

            recyclerView.setAdapter(adapter);
        } catch (Exception e) {
        }
    }

    private void setupDataAdapter(RecyclerView recyclerView, List<TransactionModel> transactions) {
        try {
            RecyclerView.Adapter<RecyclerView.ViewHolder> adapter = new RecyclerView.Adapter<RecyclerView.ViewHolder>() {

                @NonNull
                @Override
                public RecyclerView.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
                    TextView textView = new TextView(MainActivity.this);
                    textView.setPadding(40, 25, 40, 25);
                    textView.setTextSize(16);
                    textView.setBackgroundColor(Color.parseColor("#F8F8F8"));

                    RecyclerView.LayoutParams params = new RecyclerView.LayoutParams(
                            RecyclerView.LayoutParams.MATCH_PARENT,
                            240
                    );
                    textView.setLayoutParams(params);

                    return new RecyclerView.ViewHolder(textView) {};
                }

                @Override
                public void onBindViewHolder(@NonNull RecyclerView.ViewHolder holder, int position) {
                    try {
                        TransactionModel tx = transactions.get(position);
                        TextView textView = (TextView) holder.itemView;
                        String typeSymbol = "income".equals(tx.type) ? "+" : "-";
                        String displayText = String.format(
                                "%s %d руб.\n%s\n%s",
                                typeSymbol,
                                tx.amount,
                                tx.description != null ? tx.description : "Без описания",
                                tx.date != null ? (tx.date.split("T")[0] + " " + tx.date.split("T")[1]) : "Без даты"
                        );

                        textView.setText(displayText);

                        if ("income".equals(tx.type)) {
                            textView.setTextColor(Color.parseColor("#2E7D32"));
                        } else {
                            textView.setTextColor(Color.parseColor("#C62828"));
                        }
                    } catch (Exception e) {
                    }
                }

                @Override
                public int getItemCount() {
                    return transactions.size();
                }
            };

            recyclerView.setAdapter(adapter);
        } catch (Exception e) {
        }
    }

    private class GoalsAdapter extends RecyclerView.Adapter<GoalsAdapter.GoalViewHolder> {
        private Context context;
        private List<GoalModel> goals;

        public GoalsAdapter(Context context, List<GoalModel> goals) {
            this.context = context;
            this.goals = goals;
        }

        @NonNull
        @Override
        public GoalViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
            try {
                View view = LayoutInflater.from(context).inflate(R.layout.item_goal, parent, false);
                return new GoalViewHolder(view);
            } catch (Exception e) {
                return new GoalViewHolder(new View(context));
            }
        }

        @Override
        public void onBindViewHolder(@NonNull GoalViewHolder holder, int position) {
            try {
                GoalModel goal = goals.get(position);

                if (holder.titleText != null) holder.titleText.setText(goal.name);
                if (holder.targetAmountText != null) holder.targetAmountText.setText("Цель: " + goal.targetAmount + " ₽");
                if (holder.currentAmountText != null) holder.currentAmountText.setText("Собрано: " + goal.currentAmount + " ₽");

                int remaining = goal.targetAmount - goal.currentAmount;
                if (holder.remainingText != null) holder.remainingText.setText("Осталось: " + Math.max(0, remaining) + " ₽");
                if (holder.deadlineText != null) holder.deadlineText.setText("До: " + goal.deadline);

                if (holder.progressBar != null) {
                    holder.progressBar.setMax(100);
                    holder.progressBar.setProgress(goal.progress);
                }
                if (holder.progressText != null) holder.progressText.setText(goal.progress + "%");

                if (holder.editButton != null) {
                    holder.editButton.setOnClickListener(v -> showEditGoalDialog(goal));
                }
                if (holder.deleteButton != null) {
                    holder.deleteButton.setOnClickListener(v -> showDeleteGoalDialog(goal.id));
                }
                if (holder.addMoneyButton != null) {
                    holder.addMoneyButton.setOnClickListener(v -> showAddMoneyDialog(goal));
                }
            } catch (Exception e) {
            }
        }

        @Override
        public int getItemCount() {
            return goals != null ? goals.size() : 0;
        }

        class GoalViewHolder extends RecyclerView.ViewHolder {
            TextView titleText, targetAmountText, currentAmountText, remainingText, deadlineText, progressText;
            ProgressBar progressBar;
            Button editButton, deleteButton, addMoneyButton;

            public GoalViewHolder(@NonNull View itemView) {
                super(itemView);
                try {
                    titleText = itemView.findViewById(R.id.titleText);
                    targetAmountText = itemView.findViewById(R.id.targetAmountText);
                    currentAmountText = itemView.findViewById(R.id.currentAmountText);
                    remainingText = itemView.findViewById(R.id.remainingText);
                    deadlineText = itemView.findViewById(R.id.deadlineText);
                    progressText = itemView.findViewById(R.id.progressText);
                    progressBar = itemView.findViewById(R.id.progressBar);
                    editButton = itemView.findViewById(R.id.editButton);
                    deleteButton = itemView.findViewById(R.id.deleteButton);
                    addMoneyButton = itemView.findViewById(R.id.addMoneyButton);
                } catch (Exception e) {
                }
            }
        }
    }

    private void showEditGoalDialog(GoalModel goal) {
        try {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.setTitle("Редактировать цель");

            View dialogView = getLayoutInflater().inflate(R.layout.dialog_add_goal, null);
            builder.setView(dialogView);

            EditText titleInput = dialogView.findViewById(R.id.titleInput);
            EditText targetAmountInput = dialogView.findViewById(R.id.targetAmountInput);
            EditText currentAmountInput = dialogView.findViewById(R.id.currentAmountInput);
            EditText deadlineInput = dialogView.findViewById(R.id.deadlineInput);

            if (titleInput != null) titleInput.setText(goal.name);
            if (targetAmountInput != null) targetAmountInput.setText(String.valueOf(goal.targetAmount));
            if (currentAmountInput != null) currentAmountInput.setText(String.valueOf(goal.currentAmount));
            if (deadlineInput != null) deadlineInput.setText(goal.deadline);

            builder.setPositiveButton("Сохранить", (dialog, which) -> {
                try {
                    String title = titleInput != null ? titleInput.getText().toString().trim() : "";
                    String targetStr = targetAmountInput != null ? targetAmountInput.getText().toString().trim() : "";
                    String currentStr = currentAmountInput != null ? currentAmountInput.getText().toString().trim() : "";
                    String deadline = deadlineInput != null ? deadlineInput.getText().toString().trim() : "";

                    if (title.isEmpty() || targetStr.isEmpty() || deadline.isEmpty()) {
                        Toast.makeText(MainActivity.this, "Заполните обязательные поля", Toast.LENGTH_SHORT).show();
                        return;
                    }

                    goal.name = title;
                    goal.targetAmount = Integer.parseInt(targetStr);
                    goal.currentAmount = currentStr.isEmpty() ? 0 : Integer.parseInt(currentStr);
                    goal.deadline = deadline;
                    goal.progress = GoalContext.calculateProgress(goal.currentAmount, goal.targetAmount);

                    GoalContext.update(goal, new GoalContext.ActionCallback() {
                        @Override
                        public void onSuccess() {
                            runOnUiThread(() -> {
                                Toast.makeText(MainActivity.this, "Цель обновлена", Toast.LENGTH_SHORT).show();
                                loadUserGoals();
                            });
                        }

                        @Override
                        public void onError(String error) {
                            runOnUiThread(() -> {
                                Toast.makeText(MainActivity.this, "Ошибка: " + error, Toast.LENGTH_LONG).show();
                            });
                        }
                    });
                } catch (NumberFormatException e) {
                    Toast.makeText(MainActivity.this, "Некорректная сумма", Toast.LENGTH_SHORT).show();
                } catch (Exception e) {
                    Toast.makeText(MainActivity.this, "Ошибка обновления цели", Toast.LENGTH_SHORT).show();
                }
            });

            builder.setNegativeButton("Отмена", null);
            builder.show();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка редактирования цели", Toast.LENGTH_SHORT).show();
        }
    }

    private void showDeleteGoalDialog(int goalId) {
        try {
            new AlertDialog.Builder(this)
                    .setTitle("Удаление цели")
                    .setMessage("Вы уверены, что хотите удалить эту цель?")
                    .setPositiveButton("Удалить", (dialog, which) -> {
                        try {
                            GoalContext.delete(goalId, new GoalContext.ActionCallback() {
                                @Override
                                public void onSuccess() {
                                    runOnUiThread(() -> {
                                        Toast.makeText(MainActivity.this, "Цель удалена", Toast.LENGTH_SHORT).show();
                                        loadUserGoals();
                                    });
                                }

                                @Override
                                public void onError(String error) {
                                    runOnUiThread(() -> {
                                        Toast.makeText(MainActivity.this, "Ошибка: " + error, Toast.LENGTH_LONG).show();
                                    });
                                }
                            });
                        } catch (Exception e) {
                            Toast.makeText(MainActivity.this, "Ошибка удаления цели", Toast.LENGTH_SHORT).show();
                        }
                    })
                    .setNegativeButton("Отмена", null)
                    .show();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка удаления цели", Toast.LENGTH_SHORT).show();
        }
    }

    private void showAddMoneyDialog(GoalModel goal) {
        try {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.setTitle("Добавить деньги в цель");

            View dialogView = getLayoutInflater().inflate(R.layout.dialog_add_money, null);
            builder.setView(dialogView);

            TextView goalTitle = dialogView.findViewById(R.id.goalTitle);
            EditText amountInput = dialogView.findViewById(R.id.amountInput);

            if (goalTitle != null) goalTitle.setText("Цель: " + goal.name);

            builder.setPositiveButton("Добавить", (dialog, which) -> {
                try {
                    String amountStr = amountInput != null ? amountInput.getText().toString().trim() : "";
                    if (amountStr.isEmpty()) {
                        Toast.makeText(this, "Введите сумму", Toast.LENGTH_SHORT).show();
                        return;
                    }

                    int amount = Integer.parseInt(amountStr);
                    goal.currentAmount += amount;
                    goal.progress = GoalContext.calculateProgress(goal.currentAmount, goal.targetAmount);

                    GoalContext.update(goal, new GoalContext.ActionCallback() {
                        @Override
                        public void onSuccess() {
                            runOnUiThread(() -> {
                                Toast.makeText(MainActivity.this, "Деньги добавлены", Toast.LENGTH_SHORT).show();
                                loadUserGoals();
                            });
                        }

                        @Override
                        public void onError(String error) {
                            runOnUiThread(() -> {
                                Toast.makeText(MainActivity.this, "Ошибка: " + error, Toast.LENGTH_LONG).show();
                            });
                        }
                    });
                } catch (NumberFormatException e) {
                    Toast.makeText(this, "Некорректная сумма", Toast.LENGTH_SHORT).show();
                } catch (Exception e) {
                    Toast.makeText(this, "Ошибка добавления денег", Toast.LENGTH_SHORT).show();
                }
            });

            builder.setNegativeButton("Отмена", null);
            builder.show();
        } catch (Exception e) {
            Toast.makeText(this, "Ошибка добавления денег", Toast.LENGTH_SHORT).show();
        }
    }

    private void showErrorDialog(String message) {
        runOnUiThread(() -> {
            try {
                new AlertDialog.Builder(MainActivity.this)
                        .setTitle("Ошибка")
                        .setMessage(message)
                        .setPositiveButton("OK", null)
                        .show();
            } catch (Exception e) {
                Toast.makeText(MainActivity.this, message, Toast.LENGTH_LONG).show();
            }
        });
    }
}