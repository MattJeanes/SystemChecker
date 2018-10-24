using System.Collections.Generic;

namespace SystemChecker.Contracts.DTO
{
    public class ChannelDTO
    {
        public string name;
        public string creator;
        public bool is_archived;
        public bool is_member;
        public bool is_general;
        public bool is_channel;
        public bool is_group;
        public int num_members;
        public List<string> members;

        public bool IsPrivateGroup { get; }
    }
}
