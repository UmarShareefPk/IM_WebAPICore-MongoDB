
using IM.Models;
using IM_DataAccess.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Data;
using System.Net;

namespace IM_DataAccess.DataService
{
    public class IncidentService : IIncidentService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Incident> _incidentCollection;
        private readonly IMongoCollection<IncidentAttachments> _incidentAttachmentCollection;
        private readonly IMongoCollection<Comment> _commentCollection;
        private readonly IMongoCollection<CommentAttachments> _commentAttachmentCollection;



        private readonly IConfiguration _config;

        public IncidentService(IConfiguration config)
        {
            _config = config;
            MongoClient client = new MongoClient(_config.GetValue<string>("MongoDbCon"));
            IMongoDatabase database = client.GetDatabase(_config.GetValue<string>("DB"));
            _userCollection = database.GetCollection<User>("Users");
            _incidentCollection = database.GetCollection<Incident>("Incidents");
            _incidentAttachmentCollection = database.GetCollection<IncidentAttachments>("IncidentAttachments");
            _commentCollection = database.GetCollection<Comment>("Comments");
            _commentAttachmentCollection = database.GetCollection<CommentAttachments>("CommentAttachments");
        }

        public async Task<Incident> AddIncident(Incident incident)
        {
            await _incidentCollection.InsertOneAsync(incident); return incident;
        }

        public async Task<IncidentAttachments> AddIncidentAttachmentsAsync(IncidentAttachments incidentAttachments)
        {
            await _incidentAttachmentCollection.InsertOneAsync(incidentAttachments); return incidentAttachments;
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            await _commentCollection.InsertOneAsync(comment); return comment;
        }

        public async Task<CommentAttachments> AddCommentAttachmentsAsync(CommentAttachments commentAttachments)
        {
            await _commentAttachmentCollection.InsertOneAsync(commentAttachments); return commentAttachments;
        }

        public async Task<Comment> GetCommentByIdAsync(string commentId)
        {
            Comment comment = await _commentCollection.Find(c => c.Id == commentId).FirstOrDefaultAsync();
            comment.attachments = await _commentAttachmentCollection.Find(attch => attch.CommentId == comment.Id).ToListAsync();
            return comment;
        }

        public async Task<List<IncidentAttachments>> GetIncidentAttachmentAsync(string incidentId)
        {
            return await _incidentAttachmentCollection.Find(attachment => attachment.IncidentId == incidentId).ToListAsync();
        }
        public async Task<string> DeleteFileAsync(string type, string filetId, string userId)
        {
            if (type.ToLower() == "comment")
            {
                var response = await _commentAttachmentCollection.DeleteOneAsync(attachment => attachment.Id == filetId);
                if (response.DeletedCount
                    > 0) return "Success";
                else return "error";
            }
            else
            {
                var response = await _incidentAttachmentCollection.DeleteOneAsync(attachment => attachment.Id == filetId);
                if (response.DeletedCount
                    > 0) return "Success";
                else return "error";
            }

        }

        public async Task<Incident> GetIncidentrByIdAsync(string incidentId)
        {
            Incident incident = await _incidentCollection.Find(i => i.Id == incidentId).FirstOrDefaultAsync();
            incident.Attachments = await _incidentAttachmentCollection.Find(attch => attch.IncidentId == incidentId).ToListAsync();
            incident.Comments = await _commentCollection.Find(c => c.IncidentId == incidentId).ToListAsync();

            foreach (var comment in incident.Comments)
            {
                comment.attachments = await _commentAttachmentCollection.Find(attch => attch.CommentId == comment.Id).ToListAsync();
            }


            return incident;
        }

        public async Task<bool> DeleteCommentAsync(string commentId, string userId)
        {
            var response = await _commentCollection.DeleteOneAsync(c => c.Id == commentId);
            if (response.DeletedCount
                > 0) return true;
            else return false;
        }

        public async Task<IncidentsWithPage> GetIncidentsPageAsync(int pageSize, int pageNumber, string? sortBy, string? sortDirection, string? serach)
        {
            var incidentQuery = from incident in _incidentCollection.AsQueryable()
                                where incident.Title.ToLower().Contains(serach.ToLower()) || incident.Description.ToLower().Contains(serach.ToLower())
                                orderby incident.CreatedAT descending
                                select incident;



            int total = incidentQuery.ToEnumerable().Count();

            var incidents = incidentQuery.ToEnumerable().Skip(pageSize * (pageNumber - 1)).Take(pageSize).OrderByDescending(u => u.CreatedAT).ToList();

            return new IncidentsWithPage
            {
                Total_Incidents = total,
                Incidents = incidents
            };
        }

        public async Task<bool> UpdateIncidentAsync(string incidentId, string parameter, string value, string userId)
        {
            var filter = Builders<Incident>.Filter
                .Eq(i => i.Id, incidentId);
            var update = Builders<Incident>.Update
                .Set(parameter, value);

            var updateResult = await _incidentCollection.UpdateOneAsync(filter, update);
            if (updateResult.ModifiedCount > 0)
                return true;
            return false;
        }

        public async Task<bool> UpdateCommentAsync(string commentId, string commentText, string userId)
        {
            var filter = Builders<Comment>.Filter
                .Eq(c => c.Id, commentId);
            var update = Builders<Comment>.Update
                .Set(c => c.CommentText, commentText);

            var updateResult = await _commentCollection.UpdateOneAsync(filter, update);
            if (updateResult.ModifiedCount > 0)
                return true;
            return false;
        }


        public void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }
            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }
    

} //end of class
}
