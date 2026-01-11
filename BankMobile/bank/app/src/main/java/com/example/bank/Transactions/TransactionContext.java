package com.example.bank.Transactions;

import com.example.bank.Models.TransactionModel;
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

public class TransactionContext {

    private static final String BASE_URL = "http://10.0.2.2:5081/api/Transaction";
    private static final int TIMEOUT = 10000; // 10 секунд

    // =============== Интерфейсы обратного вызова ===============
    public interface GetAllCallback {
        void onSuccess(List<TransactionModel> transactions);
        void onError(String error);
    }

    public interface GetCallback {
        void onSuccess(TransactionModel transaction);
        void onError(String error);
    }

    public interface ActionCallback {
        void onSuccess();
        void onError(String error);
    }

    // =============== МЕТОД 1: Получить все транзакции ===============
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

                            Type listType = new TypeToken<List<TransactionModel>>(){}.getType();
                            List<TransactionModel> list = gson.fromJson(json, listType);

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

    // =============== МЕТОД 2: Получить по ID ===============
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
                        TransactionModel tx = gson.fromJson(json, TransactionModel.class);
                        postOnMainThread(() -> callback.onSuccess(tx));
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

    // =============== МЕТОД 3: Добавить транзакцию (POST через Form Data) ===============
    public static void add(TransactionModel tx, ActionCallback callback) {
        new Thread(() -> {
            try {
                Connection.Response res = Jsoup.connect(BASE_URL + "/Add")
                        .method(Connection.Method.POST)
                        .header("Content-Type", "application/x-www-form-urlencoded")
                        .data("id", String.valueOf(tx.id))
                        .data("userId", String.valueOf(tx.userId))
                        .data("amount", String.valueOf(tx.amount))
                        .data("description", tx.description != null ? tx.description : "")
                        .data("date", tx.date != null ? tx.date : getCurrentIsoDate())
                        .data("type", tx.type != null ? tx.type : "")
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

    // =============== МЕТОД 4: Обновить транзакцию (PUT через Form Data) ===============
    public static void update(TransactionModel tx, ActionCallback callback) {
        new Thread(() -> {
            try {
                Connection.Response res = Jsoup.connect(BASE_URL + "/Update")
                        .method(Connection.Method.PUT)
                        .header("Content-Type", "application/x-www-form-urlencoded")
                        .data("id", String.valueOf(tx.id))
                        .data("userId", String.valueOf(tx.userId))
                        .data("amount", String.valueOf(tx.amount))
                        .data("description", tx.description != null ? tx.description : "")
                        .data("date", tx.date != null ? tx.date : getCurrentIsoDate())
                        .data("type", tx.type != null ? tx.type : "")
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

    // =============== МЕТОД 5: Удалить транзакцию ===============
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

    // =============== Вспомогательные методы ===============
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
}