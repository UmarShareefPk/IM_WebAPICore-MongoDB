using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IM_WebAPICore_MongoDB.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        //[BsonElement("Name")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string HubId { get; set; }
    }
}
