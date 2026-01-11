using System;
using System.ComponentModel;

namespace PiggyBankAdmin.Models
{
    public class Goal : INotifyPropertyChanged
    {
        private int _id;
        private int _userId;
        private string _userName;
        private string _name;
        private decimal _targetAmount;
        private decimal _currentAmount;
        private DateTime? _deadline;
        private bool _isCompleted;
        private DateTime _createdAt;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public int UserId
        {
            get => _userId;
            set { _userId = value; OnPropertyChanged(nameof(UserId)); }
        }

        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(nameof(UserName)); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public decimal TargetAmount
        {
            get => _targetAmount;
            set { _targetAmount = value; OnPropertyChanged(nameof(TargetAmount)); }
        }

        public decimal CurrentAmount
        {
            get => _currentAmount;
            set { _currentAmount = value; OnPropertyChanged(nameof(CurrentAmount)); }
        }

        public decimal Progress => TargetAmount > 0 ? (CurrentAmount / TargetAmount) * 100 : 0;

        public DateTime? Deadline
        {
            get => _deadline;
            set { _deadline = value; OnPropertyChanged(nameof(Deadline)); }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set { _isCompleted = value; OnPropertyChanged(nameof(IsCompleted)); }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(nameof(CreatedAt)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}