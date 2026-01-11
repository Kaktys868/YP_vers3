package com.example.bank;

import android.content.Context;
import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import java.io.*;
import java.lang.reflect.Type;
import java.util.List;

public class JSONHelper {
    private static final Gson GSON = new Gson();

    // Сохранить один объект
    public static <T> boolean saveObject(Context ctx, String filename, T obj) {
        try (FileOutputStream fos = ctx.openFileOutput(filename, Context.MODE_PRIVATE);
             OutputStreamWriter osw = new OutputStreamWriter(fos)) {
            GSON.toJson(obj, osw);
            return true;
        } catch (Exception e) {
            e.printStackTrace();
            return false;
        }
    }

    // Загрузить один объект
    public static <T> T loadObject(Context ctx, String filename, Class<T> clazz) {
        try (FileInputStream fis = ctx.openFileInput(filename);
             InputStreamReader isr = new InputStreamReader(fis)) {
            return GSON.fromJson(isr, clazz);
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }

    // Сохранить список
    public static <T> boolean saveList(Context ctx, String filename, List<T> list) {
        try (FileOutputStream fos = ctx.openFileOutput(filename, Context.MODE_PRIVATE);
             OutputStreamWriter osw = new OutputStreamWriter(fos)) {
            GSON.toJson(list, osw);
            return true;
        } catch (Exception e) {
            e.printStackTrace();
            return false;
        }
    }

    // Загрузить список
    public static <T> List<T> loadList(Context ctx, String filename, Class<T> itemClass) {
        try (FileInputStream fis = ctx.openFileInput(filename);
             InputStreamReader isr = new InputStreamReader(fis)) {
            Type listType = TypeToken.getParameterized(List.class, itemClass).getType();
            return GSON.fromJson(isr, listType);
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }

    public static <T> List<T> fromJsonList(String json, Class<T> clazz) {
        Type listType = TypeToken.getParameterized(List.class, clazz).getType();
        return GSON.fromJson(json, listType);
    }

    // Получить JSON-строку из объекта (для отправки на API)
    public static <T> String toJson(T obj) {
        return GSON.toJson(obj);
    }

    // Создать объект из JSON-строки
    public static <T> T fromJson(String json, Class<T> clazz) {
        return GSON.fromJson(json, clazz);
    }
}