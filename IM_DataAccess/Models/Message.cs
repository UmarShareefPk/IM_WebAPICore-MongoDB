using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace IM_DataAccess.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string MessageText { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public bool Deleted { get; set; }
        public string ConversationId { get; set; }

    }
}
