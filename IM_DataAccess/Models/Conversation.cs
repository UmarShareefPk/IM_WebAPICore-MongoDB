using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace IM_DataAccess.Models
{
    [BsonIgnoreExtraElements]
    public class Conversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string User1 { get; set; }
        public string User2 { get; set; }
        public DateTime LastMessageTime { get; set; }
        public string LastMessage { get; set; }

        [BsonIgnore]
        public int UnReadCount { get; set; }

    }
}
