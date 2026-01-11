package com.example.bank.Models;

public class TransactionModel {
    public int id;
    public int userId;
    public int amount;
    public String description;
    public String date;
    public String type;

    // Конструкторы
    public TransactionModel() {}

    public TransactionModel(int userId, int amount, String description, String date, String type) {
        this.userId = userId;
        this.amount = amount;
        this.description = description;
        this.date = date;
        this.type = type;
        this.id = 0;
    }
}