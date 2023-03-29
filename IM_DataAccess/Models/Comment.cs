using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IM_DataAccess.Models
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string IncidentId { get; set; }
        public string UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public string CommentText { get; set; }
        public List<CommentAttachments> attachments { get; set; }
    }
}