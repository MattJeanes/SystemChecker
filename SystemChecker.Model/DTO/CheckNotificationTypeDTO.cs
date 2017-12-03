using System.Collections.Generic;

namespace SystemChecker.Model.DTO
{
    public class CheckNotificationTypeDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<OptionDTO> Options { get; set; }
    }
}