
using IM.Models;
using IM_DataAccess.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Data;
using System.Net;

namespace IM_DataAccess.DataService
{
    public class IncidentService : IIncidentService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Incident> _incidentCollection;
        private readonly IMongoCollection<IncidentAttachments> _incidentAttachmentCollection;

        

        private readonly IConfiguration _config;

        public IncidentService(IConfiguration config)
        {
            _config = config;
            MongoClient client = new MongoClient(_config.GetValue<string>("MongoDbCon"));
            IMongoDatabase database = client.GetDatabase(_config.GetValue<string>("DB"));
            _userCollection = database.GetCollection<User>("Users");
            _incidentCollection = database.GetCollection<Incident>("Incidents");
            _incidentAttachmentCollection = database.GetCollection<IncidentAttachments>("IncidentAttachments");
        }

        public async Task<Incident> AddIncident(Incident incident)
        {
            await _incidentCollection.InsertOneAsync(incident);   return incident;
        }

        public async Task<IncidentAttachments> AddIncidentAttachmentsAsync(IncidentAttachments incidentAttachments)
        {
            await _incidentAttachmentCollection.InsertOneAsync(incidentAttachments); return incidentAttachments;
        }

    }
}
