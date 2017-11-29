using SystemChecker.Model.Enums;

namespace SystemChecker.Model.DTO
{
    public class SubCheckDTO
    {
        public int ID { get; set; }
        public int TypeID { get; set; }
        public int CheckID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public dynamic Options { get; set; }
    }
}