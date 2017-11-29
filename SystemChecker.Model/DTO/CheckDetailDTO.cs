using System.Collections.Generic;

namespace SystemChecker.Model.DTO
{
    public class CheckDetailDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public int TypeID { get; set; }
        public List<CheckScheduleDTO> Schedules { get; set; }
        public CheckDataDTO Data { get; set; }
        public List<SubCheckDTO> SubChecks { get; set; }
    }
}