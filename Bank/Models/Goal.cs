namespace Bank.Models
{
    public class Goal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public int TargetAmount { get; set; }
        public int CurrentAmount { get; set; }
        public int Progress => TargetAmount > 0 ? (CurrentAmount / TargetAmount) * 100 : 0;
        public DateTime? Deadline { get; set; }
        public bool IsCompleted => CurrentAmount == TargetAmount;
    }
}
