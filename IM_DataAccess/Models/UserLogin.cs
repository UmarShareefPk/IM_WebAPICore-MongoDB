using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IM_DataAccess.Models
{
    public class UserLogin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string userId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }      
        public DateTime CreateDate { get; set; }
     
    }
}