package com.example.bank.Models;

import com.google.gson.annotations.SerializedName;

public class UsersModel {
    @SerializedName("Id")
    public int id = 0;

    @SerializedName("Email")
    public String Email;

    @SerializedName("Password")
    public String Password;

    @SerializedName("Name")
    public String Name;

    @SerializedName("CreatedAt")
    public String CreatedAt;

    @SerializedName("IsActive")
    public boolean IsActive;

    @SerializedName("Post")
    public int Post;

    // Конструктор без параметров (ВАЖНО для GSON)
    public UsersModel() {
        // GSON требует пустой конструктор
    }
    public void setId(int Id) {this.id = Id;}

    public int getId() {return this.id;}
    public String getFormattedDate() {
        if (CreatedAt == null || CreatedAt.isEmpty()) {
            return "—";
        }

        try {
            // Попробуем разобрать дату из разных форматов
            if (CreatedAt.contains("T")) {
                // Формат ISO: "2025-01-01T10:30:00"
                String datePart = CreatedAt.split("T")[0];
                String timePart = CreatedAt.split("T")[1].split("\\.")[0]; // Убираем миллисекунды
                return datePart + " " + timePart;
            } else {
                return CreatedAt;
            }
        } catch (Exception e) {
            return CreatedAt; // Вернем как есть
        }
    }
}