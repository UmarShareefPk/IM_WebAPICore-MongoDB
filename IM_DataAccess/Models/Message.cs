using System;

namespace IM_Core.Models
{
    public class Message
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string MessageText { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public bool Deleted { get; set; }
        public string ConversationId { get; set; }

    }
}
