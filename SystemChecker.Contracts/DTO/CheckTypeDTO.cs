using System.Collections.Generic;

namespace SystemChecker.Contracts.DTO
{
    public class CheckTypeDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<OptionDTO> Options { get; set; }
    }
}