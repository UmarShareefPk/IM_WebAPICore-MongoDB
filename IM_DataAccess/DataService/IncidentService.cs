
using IM.Models;
using IM_DataAccess.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Data;
using System.Net;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        public IncidentService(IConfiguration config, INotificationService notificationService, IUserService userService)
        {
            _config = config;
            MongoClient client = new MongoClient(_config.GetValue<string>("MongoDbCon"));
            IMongoDatabase database = client.GetDatabase(_config.GetValue<string>("DB"));
            _userCollection = database.GetCollection<User>("Users");
            _incidentCollection = database.GetCollection<Incident>("Incidents");
            _incidentAttachmentCollection = database.GetCollection<IncidentAttachments>("IncidentAttachments");
            _commentCollection = database.GetCollection<Comment>("Comments");
            _commentAttachmentCollection = database.GetCollection<CommentAttachments>("CommentAttachments");
            _notificationService = notificationService;
            _userService = userService;
        }

        public async Task<Incident> AddIncident(Incident incident)
        {
            await _incidentCollection.InsertOneAsync(incident);
            await _notificationService.AddToWatchList(incident.Id, incident.CreatedBy);
            await _notificationService.AddToWatchList(incident.Id, incident.AssignedTo);
            await _notificationService.AddNotification(new IncidentNotification
            {
                CreateDate = DateTime.UtcNow,
                IsRead = false,
                NotifyAbout = await _userService.GetNameByUserId(incident.CreatedBy) + " created and incident and assigned it to you." ,
                SourceUserId = incident.CreatedBy,
                UserId = incident.AssignedTo,
                IncidentId = incident.Id,

            });
            return incident;
        }

        public async Task<IncidentAttachments> AddIncidentAttachmentsAsync(IncidentAttachments incidentAttachments)
        {
            await _incidentAttachmentCollection.InsertOneAsync(incidentAttachments); return incidentAttachments;
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {          
            await _commentCollection.InsertOneAsync(comment);
            await _notificationService.AddToWatchList(comment.IncidentId, comment.UserId);

            var watchList = await _notificationService.GetWatchListByIncident(comment.IncidentId);

            foreach(var watch in watchList)
            {
                if (watch.UserId == comment.UserId)
                    continue;
                await _notificationService.AddNotification(new IncidentNotification
                {
                    CreateDate = DateTime.UtcNow,
                    IsRead = false,
                    NotifyAbout = await _userService.GetNameByUserId(comment.UserId) + " added a comment to incident.",
                    SourceUserId = comment.UserId,
                    UserId = watch.UserId,
                    IncidentId = comment.IncidentId,
                });
            }

            return comment;

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
                > 0)
            {
                var comment = await _commentCollection.Find(c => c.Id == commentId).FirstAsync();              
                var watchList = await _notificationService.GetWatchListByIncident(comment.IncidentId);

                foreach (var watch in watchList)
                {
                    if (watch.UserId == userId)
                        continue;
                    await _notificationService.AddNotification(new IncidentNotification
                    {
                        CreateDate = DateTime.UtcNow,
                        IsRead = false,
                        NotifyAbout = await _userService.GetNameByUserId(comment.UserId) + " deleted a comment.",
                        SourceUserId = userId,
                        UserId = watch.UserId,
                        IncidentId = comment.IncidentId,
                    });
                }
                return true;
            }
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
            {
                await _notificationService.AddToWatchList(incidentId, userId);
                var watchList = await _notificationService.GetWatchListByIncident(incidentId);

                foreach (var watch in watchList)
                {
                    if (watch.UserId == userId)
                        continue;
                    await _notificationService.AddNotification(new IncidentNotification
                    {
                        CreateDate = DateTime.UtcNow,
                        IsRead = false,
                        NotifyAbout = await _userService.GetNameByUserId(userId) + " updated " + parameter  +" for an incident.",
                        SourceUserId = userId,
                        UserId = watch.UserId,
                        IncidentId = incidentId,
                    });
                }
                return true;
            }

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
            {
                var comment = await _commentCollection.Find(c => c.Id == commentId).FirstAsync();
                var watchList = await _notificationService.GetWatchListByIncident(comment.IncidentId);

                foreach (var watch in watchList)
                {
                    if (watch.UserId == userId)
                        continue;
                    await _notificationService.AddNotification(new IncidentNotification
                    {
                        CreateDate = DateTime.UtcNow,
                        IsRead = false,
                        NotifyAbout = await _userService.GetNameByUserId(comment.UserId) + " updated a comment.",
                        SourceUserId = userId,
                        UserId = watch.UserId,
                        IncidentId = comment.IncidentId,
                    });
                }

                return true;
            }
               
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

        public async Task<object> KPIAsync(string userId)
        {
            var counts = await (from incident in _incidentCollection.AsQueryable()
                                group incident by incident.Status into g
                                select new { name = g.Key, count = g.Count() }).ToListAsync();

            var late = await (from incident in _incidentCollection.AsQueryable()
                              where (incident.Status == "N" || incident.Status == "I") && incident.DueDate < DateTime.UtcNow
                              select incident).CountAsync();

            var assignedTo = await (from incident in _incidentCollection.AsQueryable()
                                    where incident.AssignedTo == userId
                                    select incident).CountAsync();


            return new
            {
                New = counts.Where(c => c.name == "N").First().count,
                InProgress = counts.Where(c => c.name == "I").First().count,
                Closed = counts.Where(c => c.name == "C").First().count,
                Approved = counts.Where(c => c.name == "A").First().count,
                Late = late,
                AssignedToMe = assignedTo
            };
        }

        public async Task<object> OverallWidgetAsync()
        {
            var counts = await (from incident in _incidentCollection.AsQueryable()
                                group incident by incident.Status into g
                                select new { name = g.Key, count = g.Count() }).ToListAsync();

            var late = await (from incident in _incidentCollection.AsQueryable()
                              where (incident.Status == "N" || incident.Status == "I") && incident.DueDate < DateTime.UtcNow
                              select incident).CountAsync();

            return new
            {
                New = counts.Where(c => c.name == "N").First().count,
                InProgress = counts.Where(c => c.name == "I").First().count,
                Closed = counts.Where(c => c.name == "C").First().count,
                Approved = counts.Where(c => c.name == "A").First().count,
                Late = late
            };
        }

        public async Task<List<Incident>> Last5IncidentsAsync()
        {
            return await (from incident in _incidentCollection.AsQueryable()
                          orderby incident.CreatedAT descending
                          select incident).Take(5).ToListAsync();
        }
        public async Task<List<Incident>> Oldest5UnresolvedIncidentsAsync()
        {
            try {
                return await (from incident in _incidentCollection.AsQueryable()
                              where incident.Status == "N" || incident.Status == "I"
                              orderby incident.CreatedAT ascending
                              select incident).Take(5).ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<object> MostAssignedToUsersIncidentsAsync()
        {
            var countsQuery = (from incident in _incidentCollection.AsQueryable()
                               group incident by incident.AssignedTo into g
                               select new { name = g.Key, count = g.Count() });

            var sorted = countsQuery.OrderByDescending(c => c.count);
            var counts = sorted.Take(5).ToList();

            return (from data in counts.AsEnumerable()
                    select new {
                        UserId = data.name,
                        Name =  _userService.GetNameByUserId(data.name).Result,
                        Count = data.count
                    })
                    .ToList();
        }

    } //end of class
}
