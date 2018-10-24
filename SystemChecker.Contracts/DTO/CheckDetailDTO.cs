using System.Collections.Generic;

namespace SystemChecker.Contracts.DTO
{
    public class CheckDetailDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string Description { get; set; }
        public int TypeID { get; set; }
        public int? GroupID { get; set; }
        public int EnvironmentID { get; set; }
        public List<CheckScheduleDTO> Schedules { get; set; }
        public CheckDataDTO Data { get; set; }
        public List<SubCheckDTO> SubChecks { get; set; }
        public List<CheckResultDTO> Results { get; set; }
        public List<CheckNotificationDTO> Notifications { get; set; }
    }
}