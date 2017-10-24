using System.Collections.Generic;

namespace SystemChecker.Model.Data
{
    public class CheckType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<CheckTypeOption> Options { get; set; }
    }
}