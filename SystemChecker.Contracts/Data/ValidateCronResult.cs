namespace SystemChecker.Contracts.Data
{
    public class ValidateCronResult
    {
        public bool Valid { get; set; }
        public string Next { get; set; }
        public string Error { get; set; }
    }
}