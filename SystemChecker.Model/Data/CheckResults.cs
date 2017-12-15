using System.Collections.Generic;
using SystemChecker.Model.DTO;

namespace SystemChecker.Model.Data
{
    public interface ICheckResults
    {
        string MinDate { get; set; }
        string MaxDate { get; set; }
        List<CheckResultDTO> Results { get; set; }
    }
    public class CheckResults : ICheckResults
    {
        public string MinDate { get; set; }
        public string MaxDate { get; set; }
        public List<CheckResultDTO> Results { get; set; }
    }
}