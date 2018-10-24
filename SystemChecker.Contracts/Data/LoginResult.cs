namespace SystemChecker.Contracts.Data
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Error { get; set; }
        public bool InitRequired { get; set; }
    }
}