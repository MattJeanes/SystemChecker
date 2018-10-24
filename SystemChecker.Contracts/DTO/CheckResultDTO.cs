using System;
using SystemChecker.Contracts.Enums;

namespace SystemChecker.Contracts.DTO
{
    public class CheckResultDTO
    {
        public int ID { get; set; }

        public int CheckID { get; set; }

        public DateTime DTS { get; set; }

        public CheckResultStatus Status { get; set; }

        public int TimeMS { get; set; }
    }
}