using SystemChecker.Model.Enums;

namespace SystemChecker.Model.DTO
{
    public class ApiKeyDTO
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
    }
}