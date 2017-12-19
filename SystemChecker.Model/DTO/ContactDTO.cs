using SystemChecker.Model.Enums;

namespace SystemChecker.Model.DTO
{
    public class ContactDTO
    {
        public int ID { get; set; }
        public int TypeID { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}