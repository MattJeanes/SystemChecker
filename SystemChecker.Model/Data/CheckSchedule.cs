namespace SystemChecker.Model.Data
{
    public class CheckSchedule
    {
        public int ID { get; set; }
        public Check Check { get; set; }
        public string Expression { get; set; }
        public bool Active { get; set; }
    }
}