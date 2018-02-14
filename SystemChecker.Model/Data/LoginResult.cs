using SystemChecker.Model.Enums;

namespace SystemChecker.Model.Data
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Error { get; set; }
    }
}