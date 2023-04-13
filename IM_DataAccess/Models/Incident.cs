using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IM_DataAccess.Models
{
    [BsonIgnoreExtraElements]
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

        [BsonIgnore]
        public List<IncidentAttachments> Attachments { get; set; }        
        public DateTime StartTime { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        
        [BsonIgnore]
        public List<Comment> Comments { get; set; }

        [BsonIgnore]
        public List<IncidentLogs> Logs { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class IncidentResults
    {
        public ObjectId Id { get; set; }
        public string CreatedBy { get; set; }
        public string AssignedTo { get; set; }
        public DateTime CreatedAT { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AdditionalData { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }

        public User createdByUser { get; set; }
        public User assignedToUser { get; set; }
    }
}