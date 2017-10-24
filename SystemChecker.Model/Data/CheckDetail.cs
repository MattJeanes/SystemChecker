using System.Collections.Generic;

namespace SystemChecker.Model.Data
{
    public class CheckDetail
    {
        public List<CheckSchedule> Schedules { get; set; }
        public CheckData Data { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public int TypeID { get; set; }
    }
}