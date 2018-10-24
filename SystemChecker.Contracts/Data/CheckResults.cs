using System.Collections.Generic;
using SystemChecker.Contracts.DTO;

namespace SystemChecker.Contracts.Data
{
    public class CheckResults
    {
        public string MinDate { get; set; }
        public string MaxDate { get; set; }
        public List<CheckResultDTO> Results { get; set; }
    }
}