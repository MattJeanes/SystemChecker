using SystemChecker.Model.Enums;

namespace SystemChecker.Model.DTO
{
    public class OptionDTO
    {
        public int ID { get; set; }
        public OptionType OptionType { get; set; }
        public string Label { get; set; }
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }
    }
}