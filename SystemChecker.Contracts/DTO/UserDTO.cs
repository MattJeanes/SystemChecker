using System.Collections.Generic;

namespace SystemChecker.Contracts.DTO
{
    public class UserDTO
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsWindowsUser { get; set; }
        public List<ApiKeyDTO> ApiKeys { get; set; }
    }
}