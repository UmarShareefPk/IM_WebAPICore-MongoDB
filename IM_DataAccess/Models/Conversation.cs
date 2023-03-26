using System;

namespace IM_Core.Models
{
    public class Conversation
    {
        public string Id { get; set; }
        public string User1 { get; set; }
        public string User2 { get; set; }
        public DateTime LastMessageTime { get; set; }
        public string LastMessage { get; set; }
        public int UnReadCount { get; set; }

    }
}
