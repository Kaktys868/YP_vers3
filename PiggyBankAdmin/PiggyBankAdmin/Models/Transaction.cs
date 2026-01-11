using System;
using System.ComponentModel;

namespace PiggyBankAdmin.Models
{
    public class Transaction : INotifyPropertyChanged
    {
        private int _id;
        private int _userId;
        private string _userName;
        private decimal _amount;
        private string _description;
        private DateTime _date;
        private string _type; // "income" или "expense"
        private bool _isRecurring;
        private string _recurringType; // "none", "daily", "weekly", "monthly"

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

        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(nameof(Amount)); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(nameof(Date)); }
        }

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public bool IsRecurring
        {
            get => _isRecurring;
            set { _isRecurring = value; OnPropertyChanged(nameof(IsRecurring)); }
        }

        public string RecurringType
        {
            get => _recurringType;
            set { _recurringType = value; OnPropertyChanged(nameof(RecurringType)); }
        }

        // Вычисляемые свойства для удобства отображения
        public string TypeDisplay
        {
            get
            {
                if (Type == "income")
                    return "Доход";
                else
                    return "Расход";
            }
        }

        public string AmountColor
        {
            get
            {
                if (Type == "income")
                    return "Green";
                else
                    return "Red";
            }
        }

        public string RecurringDisplay => IsRecurring ? "🔄" : "";

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}