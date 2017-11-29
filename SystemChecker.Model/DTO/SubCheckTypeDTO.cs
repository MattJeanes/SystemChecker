using System.Collections.Generic;

namespace SystemChecker.Model.DTO
{
    public class SubCheckTypeDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<OptionDTO> Options { get; set; }
    }
}