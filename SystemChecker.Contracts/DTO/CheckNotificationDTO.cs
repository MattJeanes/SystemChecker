namespace SystemChecker.Contracts.DTO
{
    public class CheckNotificationDTO
    {
        public int ID { get; set; }
        public int TypeID { get; set; }
        public int CheckID { get; set; }
        public bool Active { get; set; }
        public dynamic Options { get; set; }
        public int? FailCount { get; set; }
        public int? FailMinutes { get; set; }
    }
}