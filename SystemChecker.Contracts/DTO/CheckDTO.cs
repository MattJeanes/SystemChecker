namespace SystemChecker.Contracts.DTO
{
    public class CheckDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string Description { get; set; }
        public int TypeID { get; set; }
        public int? GroupID { get; set; }
        public int EnvironmentID { get; set; }
        public int? LastResultStatus { get; set; }
        public int? LastResultType { get; set; }
    }
}