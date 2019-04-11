namespace SystemChecker.Contracts.DTO
{
    public class CheckScheduleDTO
    {
        public int ID { get; set; }
        public CheckDTO Check { get; set; }
        public string Expression { get; set; }
        public bool Active { get; set; }
        public bool SkipPublicHolidays { get; set; }
    }
}