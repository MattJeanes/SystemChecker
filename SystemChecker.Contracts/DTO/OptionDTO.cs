namespace SystemChecker.Contracts.DTO
{
    public class OptionDTO
    {
        public string ID { get; set; }
        public int OptionType { get; set; }
        public string Label { get; set; }
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }
        public bool Multiple { get; set; }
    }
}