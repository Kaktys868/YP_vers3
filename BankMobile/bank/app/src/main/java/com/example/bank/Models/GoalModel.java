package com.example.bank.Models;

public class GoalModel {
    public int id;
    public int userId;
    public String name;
    public int targetAmount;
    public int currentAmount;
    public int progress;
    public String deadline;
    public GoalModel() {}

    public GoalModel(int userId, int targetAmount, int currentAmount, int progress, String deadline) {
        this.userId = userId;
        this.targetAmount = targetAmount;
        this.currentAmount = currentAmount;
        this.progress = progress;
        this.deadline = deadline;
        this.id = 0;
    }
}
