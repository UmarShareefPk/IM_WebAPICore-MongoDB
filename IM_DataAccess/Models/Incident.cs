using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IM_DataAccess.Models
{
    public class Incident
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public string AssignedTo { get; set; }
        public DateTime CreatedAT { get; set; } = DateTime.UtcNow;
        public string Title { get; set; }
        public string Description { get; set; }
        public string AdditionalData { get; set; }
        public List<IncidentAttachments> Attachments { get; set; }        
        public DateTime StartTime { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public List<Comment> Comments { get; set; } 
        public List<IncidentLogs> Logs { get; set; }

    }
}