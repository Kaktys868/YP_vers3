package com.example.bank.Users;

import android.content.Context;
import android.util.Log;

import com.example.bank.Models.UsersModel;
import com.google.gson.FieldNamingPolicy;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.reflect.TypeToken;

import org.jsoup.Connection;
import org.jsoup.Jsoup;

import java.lang.reflect.Type;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;

public class UsersContext {

    private static final String BASE_URL = "http://10.0.2.2:5081/api/User";
    private static final String PREF_NAME = "UserPrefs";
    private static final String KEY_CURRENT_USER = "CurrentUser";
    private static final String TAG = "UsersContext";
    private static final int TIMEOUT = 10000; // 10 секунд

    // =============== Интерфейсы обратного вызова (точно как в TransactionContext) ===============
    public interface GetAllCallback {
        void onSuccess(List<UsersModel> users);
        void onError(String error);
    }

    public interface GetCallback {
        void onSuccess(UsersModel user);
        void onError(String error);
    }

    public interface ActionCallback {
        void onSuccess();
        void onError(String error);
    }

    // =============== МЕТОД 1: Получить всех пользователей (как TransactionContext.getAll) ===============
    public static void getAll(GetAllCallback callback) {
        new Thread(() -> {
            try {
                Connection.Response res = Jsoup.connect(BASE_URL + "/List")
                        .header("Content-Type", "application/x-www-form-urlencoded")
                        .ignoreContentType(true)
                        .ignoreHttpErrors(true)
                        .timeout(TIMEOUT)
                        .execute();

                if (res.statusCode() == 200) {
                    String json = res.body();

                    if (json != null && !json.trim().isEmpty()) {
                        try {
                            Gson gson = new GsonBuilder()
                                    .setFieldNamingPolicy(FieldNamingPolicy.IDENTITY)
                                    .create();

                            Type listType = new TypeToken<List<UsersModel>>(){}.getType();
                            List<UsersModel> list = gson.fromJson(json, listType);

                            postOnMainThread(() -> callback.onSuccess(list != null ? list : new ArrayList<>()));

                        } catch (Exception e) {
                            postError(callback::onError, "Ошибка парсинга: " + e.getMessage());
                        }
                    } else {
                        postError(callback::onError, "Пустой ответ от сервера");
                    }
                } else {
                    postError(callback::onError, "HTTP ошибка: " + res.statusCode());
                }
            } catch (Exception e) {
                postError(callback::onError, "Ошибка сети: " + e.getMessage());
            }
        }).start();
    }

    // =============== МЕТОД 2: Получить пользователя по ID (как TransactionContext.getById) ===============
    public static void getById(int id, GetCallback callback) {
        new Thread(() -> {
            try {
                String url = BASE_URL + "/Item?id=" + id;

                Connection.Response res = Jsoup.connect(url)
                        .header("Content-Type", "application/x-www-form-urlencoded")
                        .ignoreContentType(true)
                        .timeout(TIMEOUT)
                        .execute();

                if (res.statusCode() == 200) {
                    String json = res.body();
                    if (json != null && !json.trim().isEmpty()) {
                        Gson gson = new Gson();
                        UsersModel user = gson.fromJson(json, UsersModel.class);
                        postOnMainThread(() -> callback.onSuccess(user));
                    } else {
                        postError(callback::onError, "Пустой ответ");
                    }
                } else {
                    postError(callback::onError, "HTTP ошибка: " + res.statusCode());
                }
            } catch (Exception e) {
                postError(callback::onError, "Ошибка: " + e.getMessage());
            }
        }).start();
    }

    // =============== МЕТОД 3: Добавить пользователя (как TransactionContext.add) ===============
    public static void add(UsersModel user, ActionCallback callback) {
        new Thread(() -> {
            try {
                Connection.Response res = Jsoup.connect(BASE_URL + "/Add")
                        .method(Connection.Method.POST)
                        .header("Content-Type", "application/x-www-form-urlencoded")
                        .data("Email", user.Email != null ? user.Email : "")
                        .data("Password", user.Password != null ? user.Password : "")
                        .data("Name", user.Name != null ? user.Name : "")
                        .data("CreatedAt", user.CreatedAt != null ? user.CreatedAt : getCurrentIsoDate())
                        .data("IsActive", String.valueOf(user.IsActive))
                        .data("Post", String.valueOf(user.Post))
                        .ignoreContentType(true)
                        .ignoreHttpErrors(true)
                        .timeout(TIMEOUT)
                        .execute();

                if (res.statusCode() == 200 || res.statusCode() == 201) {
                    postOnMainThread(callback::onSuccess);
                } else {
                    postError(callback::onError, "Ошибка сервера: " + res.statusCode() + " - " + res.body());
                }
            } catch (Exception e) {
                postError(callback::onError, "Ошибка соединения: " + e.getMessage());
            }
        }).start();
    }

    // =============== МЕТОД 4: Обновить пользователя (как TransactionContext.update) ===============
    public static void update(UsersModel user, ActionCallback callback) {
        new Thread(() -> {
            try {
                Connection.Response res = Jsoup.connect(BASE_URL + "/Update")
                        .method(Connection.Method.PUT)
                        .header("Content-Type", "application/x-www-form-urlencoded")
                        .data("Id", String.valueOf(user.id))
                        .data("Email", user.Email != null ? user.Email : "")
                        .data("Password", user.Password != null ? user.Password : "")
                        .data("Name", user.Name != null ? user.Name : "")
                        .data("CreatedAt", user.CreatedAt != null ? user.CreatedAt : "")
                        .data("IsActive", String.valueOf(user.IsActive))
                        .data("Post", String.valueOf(user.Post))
                        .ignoreContentType(true)
                        .ignoreHttpErrors(true)
                        .timeout(TIMEOUT)
                        .execute();

                if (res.statusCode() == 200) {
                    postOnMainThread(callback::onSuccess);
                } else {
                    postError(callback::onError, "Ошибка сервера: " + res.statusCode() + " - " + res.body());
                }
            } catch (Exception e) {
                postError(callback::onError, "Ошибка соединения: " + e.getMessage());
            }
        }).start();
    }

    // =============== МЕТОД 5: Удалить пользователя (как TransactionContext.delete) ===============
    public static void delete(int id, ActionCallback callback) {
        new Thread(() -> {
            try {
                String url = BASE_URL + "/Delete?id=" + id;

                Connection.Response res = Jsoup.connect(url)
                        .method(Connection.Method.DELETE)
                        .header("Content-Type", "application/x-www-form-urlencoded")
                        .ignoreContentType(true)
                        .ignoreHttpErrors(true)
                        .timeout(TIMEOUT)
                        .execute();

                if (res.statusCode() == 200) {
                    postOnMainThread(callback::onSuccess);
                } else {
                    postError(callback::onError, "Сервер вернул: " + res.statusCode() + " - " + res.body());
                }
            } catch (Exception e) {
                postError(callback::onError, "Сетевая ошибка: " + e.getMessage());
            }
        }).start();
    }

    // =============== Вспомогательные методы (точно как в TransactionContext) ===============
    private static void postOnMainThread(Runnable runnable) {
        new android.os.Handler(android.os.Looper.getMainLooper()).post(runnable);
    }

    private static void postError(VoidCallback errorCallback, String message) {
        new android.os.Handler(android.os.Looper.getMainLooper()).post(() -> errorCallback.call(message));
    }

    @FunctionalInterface
    private interface VoidCallback {
        void call(String error);
    }

    public static String getCurrentIsoDate() {
        return new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault()).format(new Date());
    }

    public static String getCurrentDateOnly() {
        return new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault()).format(new Date());
    }

    // =============== Дополнительные методы для работы с SharedPreferences ===============
    public static void setCurrentUser(Context context, UsersModel user) {
        if (user == null || context == null) return;

        android.content.SharedPreferences prefs = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
        android.content.SharedPreferences.Editor editor = prefs.edit();
        Gson gson = new Gson();
        String json = gson.toJson(user);
        editor.putString(KEY_CURRENT_USER, json).apply();
        Log.d(TAG, "Текущий пользователь сохранен: " + user.Email + " (ID: " + user.id + ")");
    }

    public static UsersModel getCurrentUser(Context context) {
        if (context == null) return null;

        android.content.SharedPreferences prefs = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
        String json = prefs.getString(KEY_CURRENT_USER, null);
        if (json != null) {
            Gson gson = new Gson();
            return gson.fromJson(json, UsersModel.class);
        }
        return null;
    }

    public static void clearCurrentUser(Context context) {
        if (context == null) return;

        context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE)
                .edit()
                .remove(KEY_CURRENT_USER)
                .apply();
        Log.d(TAG, "Текущий пользователь очищен");
    }
}