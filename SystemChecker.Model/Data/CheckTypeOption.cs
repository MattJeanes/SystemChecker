using SystemChecker.Model.Enums;

namespace SystemChecker.Model.Data
{
    public class CheckTypeOption
    {
        public int ID { get; set; }
        public CheckTypeOptionType OptionType { get; set; }
        public string Label { get; set; }
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }
    }
}