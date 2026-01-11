package com.example.bank.Goals;

import android.os.Handler;
import android.os.Looper;

import com.example.bank.Models.GoalModel;
import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;

import org.jsoup.Connection;
import org.jsoup.Jsoup;

import java.lang.reflect.Type;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;

public class GoalContext {

    private static final String BASE_URL = "http://10.0.2.2:5081/api/Goal";

    // =============== Интерфейсы обратного вызова ===============
    public interface GetAllCallback {
        void onSuccess(List<GoalModel> goals);
        void onError(String error);
    }

    public interface GetCallback {
        void onSuccess(GoalModel goal);
        void onError(String error);
    }

    public interface ActionCallback {
        void onSuccess();
        void onError(String error);
    }

    // =============== МЕТОД 1: Получить все цели ===============
    public static void getAll(GetAllCallback callback) {
        new Thread(() -> {
            try {
                Connection.Response res = Jsoup.connect(BASE_URL + "/List")
                        .ignoreContentType(true)
                        .ignoreHttpErrors(true)
                        .execute();

                if (res.statusCode() == 200) {
                    String json = res.body();

                    if (json != null && !json.trim().isEmpty()) {
                        try {
                            if (!json.trim().startsWith("[")) {
                                postError(callback::onError, "Некорректный формат данных");
                                return;
                            }

                            Gson gson = new Gson();
                            Type listType = new TypeToken<List<GoalModel>>(){}.getType();
                            List<GoalModel> list = gson.fromJson(json, listType);

                            postOnMainThread(() -> callback.onSuccess(list != null ? list : new ArrayList<>()));

                        } catch (Exception e) {
                            postError(callback::onError, "Ошибка парсинга: " + e.getMessage());
                        }
                        return;
                    } else {
                        postError(callback::onError, "Пустой ответ от сервера");
                    }
                } else {
                    postError(callback::onError, "HTTP ошибка: " + res.statusCode());
                }
            } catch (Exception e) {
                postError(callback::onError, e.getMessage());
            }
        }).start();
    }

    // =============== МЕТОД 2: Получить по ID ===============
    public static void getById(int id, GetCallback callback) {
        new Thread(() -> {
            try {
                String url = BASE_URL + "/Item?id=" + id;

                Connection.Response res = Jsoup.connect(url)
                        .ignoreContentType(true)
                        .execute();

                if (res.statusCode() == 200) {
                    String json = res.body();
                    if (json != null && !json.trim().isEmpty()) {
                        Gson gson = new Gson();
                        GoalModel goal = gson.fromJson(json, GoalModel.class);
                        postOnMainThread(() -> callback.onSuccess(goal));
                        return;
                    }
                }
                postError(callback::onError, "Пустой или некорректный ответ");
            } catch (Exception e) {
                postError(callback::onError, "Ошибка: " + e.getMessage());
            }
        }).start();
    }

    // =============== МЕТОД 3: Добавить цель ===============
    public static void add(GoalModel goal, ActionCallback callback) {
        new Thread(() -> {
            try {
                Connection.Response res = Jsoup.connect(BASE_URL + "/Add")
                        .method(Connection.Method.PUT)
                        .data("userId", String.valueOf(goal.userId))
                        .data("Name", goal.name)
                        .data("TargetAmount", String.valueOf(goal.targetAmount))
                        .data("CurrentAmount", String.valueOf(goal.currentAmount))
                        .data("Deadline", goal.deadline)
                        .ignoreContentType(true)
                        .execute();

                if (res.statusCode() == 200) {
                    postOnMainThread(callback::onSuccess);
                } else {
                    postError(callback::onError, "Сервер: " + res.statusCode());
                }
            } catch (Exception e) {
                postError(callback::onError, e.getMessage());
            }
        }).start();
    }

    // =============== МЕТОД 4: Обновить цель ===============
    public static void update(GoalModel goal, ActionCallback callback) {
        new Thread(() -> {
            try {
                Connection.Response res = Jsoup.connect(BASE_URL + "/Update")
                        .method(Connection.Method.PUT)
                        .data("Id", String.valueOf(goal.id))
                        .data("userId", String.valueOf(goal.userId))
                        .data("Name", goal.name)
                        .data("TargetAmount", String.valueOf(goal.targetAmount))
                        .data("CurrentAmount", String.valueOf(goal.currentAmount))
                        .data("Deadline", goal.deadline)
                        .ignoreContentType(true)
                        .execute();

                if (res.statusCode() == 200) {
                    postOnMainThread(callback::onSuccess);
                } else {
                    postError(callback::onError, "Сервер: " + res.statusCode());
                }
            } catch (Exception e) {
                postError(callback::onError, e.getMessage());
            }
        }).start();
    }

    // =============== МЕТОД 5: Удалить цель ===============
    public static void delete(int id, ActionCallback callback) {
        new Thread(() -> {
            try {
                String url = BASE_URL + "/Delete?id=" + id;

                Connection.Response res = Jsoup.connect(url)
                        .method(Connection.Method.DELETE)
                        .ignoreContentType(true)
                        .execute();

                if (res.statusCode() == 200) {
                    postOnMainThread(callback::onSuccess);
                } else {
                    postError(callback::onError, "Сервер вернул: " + res.statusCode());
                }
            } catch (Exception e) {
                postError(callback::onError, "Сетевая ошибка: " + e.getMessage());
            }
        }).start();
    }

    // =============== Вспомогательные методы ===============
    private static void postOnMainThread(Runnable runnable) {
        new Handler(Looper.getMainLooper()).post(runnable);
    }

    private static void postError(voidCallback errorCallback, String message) {
        new Handler(Looper.getMainLooper()).post(() -> errorCallback.call(message));
    }

    @FunctionalInterface
    private interface voidCallback {
        void call(String error);
    }

    // Расчет прогресса
    public static int calculateProgress(int current, int target) {
        if (target == 0) return 0;
        return (int) ((current * 100) / target);
    }
}