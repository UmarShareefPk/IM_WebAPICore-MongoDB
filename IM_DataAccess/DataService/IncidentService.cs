
using IM.Models;
using IM_DataAccess.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;
using System.Net;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using IM_DataAccess.Extensions;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using System.Runtime.InteropServices.JavaScript;
using System.Collections.Immutable;

namespace IM_DataAccess.DataService
{
    public interface IIncidentService
    {
        Task<Comment> AddCommentAsync(Comment comment);
        Task<CommentAttachments> AddCommentAttachmentsAsync(CommentAttachments commentAttachments);
        Task<Incident> AddIncident(Incident incident);
        Task<IncidentAttachments> AddIncidentAttachmentsAsync(IncidentAttachments incidentAttachments);
        Task<bool> DeleteCommentAsync(string commentId, string userId);
        void DeleteDirectory(string path);
        Task<string> DeleteFileAsync(string type, string filetId, string userId);
        Task<Comment> GetCommentByIdAsync(string commentId);
        Task<List<IncidentAttachments>> GetIncidentAttachmentAsync(string incidentId);
        Task<Incident> GetIncidentrByIdAsync(string incidentId);
        Task<IncidentsWithPage> GetIncidentsPageAsync(int pageSize, int pageNumber, string? sortBy, string? sortDirection, string? serach);
        Task<object> KPIAsync(string userId);
        Task<List<Incident>> Last5IncidentsAsync();
        Task<object> MostAssignedToUsersIncidentsAsync();
        Task<List<Incident>> Oldest5UnresolvedIncidentsAsync();
        Task<object> OverallWidgetAsync();
        Task<bool> UpdateCommentAsync(string commentId, string commentText, string userId);
        Task<bool> UpdateIncidentAsync(string incidentId, string parameter, string value, string userId);
        Task<bool> UpdateCreateDateAsync();
    }

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
                NotifyAbout = await _userService.GetNameByUserId(incident.CreatedBy) + " created and incident and assigned it to you.",
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

            foreach (var watch in watchList)
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
            //return await GetIncidentsPageAsyncV2(pageSize,pageNumber, sortBy, sortDirection, serach);

            var sortCol = "CreatedAT"; // for aggregation pipeline

            sortCol = sortBy.ToLower() switch
            {
                "createdat" => "CreatedAT",
                "createdby" => "createdByUser.FirstName",
                "assignedto" => "assignedToUser.FirstName",
                "title" => "Title",
                "description" => "Description",
                "starttime" => "StartTime",
                "duedate" => "DueDate",
                "status" => "Status",
                _ => "CreatedAT",
            };
            sortDirection = sortDirection.ToLower() switch
            {
                "asc" => "asc",
                "desc" => "desc",
                _ => "desc",
            };         
            List<Incident> incidents = new List<Incident>();
            int count = 0;

            var pResults = _incidentCollection.Aggregate()
                            .Match(incident => incident.Title.ToLower().Contains(serach.ToLower()) || incident.Description.ToLower().Contains(serach.ToLower()))
                            .Project(new BsonDocument{
                                    { "_id", 1 },
                                    { "Title", 1 },
                                    {"Description", 1  },
                                    {"Status", 1 },
                                    {"StartTime", 1 },
                                    {"DueDate", 1 },
                                    {"CreatedAT", 1 },
                                    {
                                        "CreatedBy", new BsonDocument{
                                            {

                                                "$convert", new BsonDocument{
                                                    { "input", "$CreatedBy" },
                                                    { "to", "objectId" }
                                                }
                                                }
                                            }
                                        },
                                        {
                                        "AssignedTo", new BsonDocument{
                                            {
                                                "$convert", new BsonDocument{
                                                    { "input", "$AssignedTo" },
                                                    { "to", "objectId" }
                                                }
                                                }
                                            }
                                        }
                                })
                            .Lookup("Users", "CreatedBy", "_id", "createdByUser")
                            .Lookup("Users", "AssignedTo", "_id", "assignedToUser")
                            .Sort(new BsonDocument
                               (sortCol, sortDirection == "asc" ? 1 : -1)
                                );

            count = pResults.ToEnumerable().Count();
            var incidentsData = await pResults.Skip(pageSize * (pageNumber - 1)).Limit(pageSize).ToListAsync();

            foreach (var i in incidentsData)
            {
                string id = i.GetValue("_id").RawValue.ToString();
                string title = i.GetValue("Title").RawValue.ToString();
                string des = i.GetValue("Description").RawValue.ToString();
                DateTime startTime = i.GetValue("StartTime").AsDateTime;
                DateTime duedate = i.GetValue("DueDate").AsDateTime;
                DateTime createdat = i.GetValue("CreatedAT").AsDateTime;
                string status = i.GetValue("Status").RawValue.ToString();
                BsonDocument user = i.GetValue("createdByUser")[0].AsBsonDocument;
                string createdBy = user.GetValue("FirstName").RawValue.ToString() + " " + user.GetValue("LastName").RawValue.ToString();
                user = i.GetValue("assignedToUser")[0].AsBsonDocument;
                string assignedTo = user.GetValue("FirstName").RawValue.ToString() + " " + user.GetValue("LastName").RawValue.ToString();

                var incident = new Incident
                {
                    Id = id,
                    CreatedBy = createdBy,
                    AssignedTo = assignedTo,
                    Title = title,
                    Status = status,
                    Description = des,
                    CreatedAT = createdat,
                    StartTime = startTime,
                    DueDate = duedate
                };

                incidents.Add(incident);
            }

            return new IncidentsWithPage
            {
                Total_Incidents = count,
                Incidents = incidents
            };
        }

        Func<Incident, object> orderByFunc = null;
        public async Task<IncidentsWithPage> GetIncidentsPageAsyncBasic(int pageSize, int pageNumber, string? sortBy, string? sortDirection, string? serach)
        {
            switch (sortBy.ToLower())
            {
                case "createdat":
                    sortBy = "CreatedAT";
                    orderByFunc = incident => incident.CreatedAT;
                    break;
                case "createdby":
                   sortBy = "CreatedBy";
                    orderByFunc = incident => incident.CreatedBy;
                    break;
                case "assignedto":
                    sortBy = "AssignedTo";
                    orderByFunc = incident => incident.AssignedTo;
                    break;
                case "title":
                   sortBy = "Title";
                    orderByFunc = incident => incident.Title;
                    break;
                case "description":
                    sortBy = "Description";
                    orderByFunc = incident => incident.Description;
                    break;
                case "starttime":
                    sortBy = "StartTime";
                    orderByFunc = incident => incident.StartTime;
                    break;
                case "duedate":
                    sortBy = "DueDate";
                    orderByFunc = incident => incident.DueDate;
                    break;
                case "status":
                    sortBy = "Status";
                    orderByFunc = incident => incident.Status;
                    break;
                default:
                    sortBy = "CreatedAT";
                    orderByFunc = incident => incident.CreatedAT;
                    break;
            }
            sortDirection = sortDirection.ToLower() switch
            {
                "asc" => "asc",
                "desc" => "desc",
                _ => "desc",
            };
            
            IEnumerable<Incident> incidentQuery = null;

            if (sortDirection == "asc")
                incidentQuery = _incidentCollection.AsQueryable()
                 .OrderBy(orderByFunc)
                 .Where(incident => incident.Title.ToLower().Contains(serach.ToLower()) || incident.Description.ToLower().Contains(serach.ToLower()));
            else
                incidentQuery = _incidentCollection.AsQueryable()
                    .OrderByDescending(orderByFunc)
                    .Where(incident => incident.Title.ToLower().Contains(serach.ToLower()) || incident.Description.ToLower().Contains(serach.ToLower()));
                     
            
            var count = incidentQuery.Count();

            var incidents = incidentQuery
                    .Skip(pageSize * (pageNumber - 1))
                    .Take(pageSize)
                    .Select(incident => incident)
                    .ToList();

            return new IncidentsWithPage
            {
                Total_Incidents = count,
                Incidents = incidents
            };
        }

        public async Task<IncidentsWithPage> GetIncidentsPageAsyncV2(int pageSize, int pageNumber, string? sortBy, string? sortDirection, string? serach)
        {
           
            switch (sortBy.ToLower())
            {
                case "createdat":
                    sortBy = "CreatedAT";
                    orderByFunc = incident => incident.CreatedAT;
                    break;
                case "createdby":                  
                    sortBy = "CreatedBy";
                    orderByFunc = incident => incident.CreatedBy;
                    break;
                case "assignedto":                   
                    sortBy = "AssignedTo";
                    orderByFunc = incident => incident.AssignedTo;
                    break;
                case "title":                   
                    sortBy = "Title";
                    orderByFunc = incident => incident.Title;
                    break;
                case "description":
                    sortBy = "Description";
                    orderByFunc = incident => incident.Description;
                    break;
                case "starttime":
                    sortBy = "StartTime";
                    orderByFunc = incident => incident.StartTime;
                    break;
                case "duedate":
                    sortBy = "DueDate";
                    orderByFunc = incident => incident.DueDate;
                    break;
                case "status":
                    sortBy = "Status";
                    orderByFunc = incident => incident.Status;
                    break;
                default:
                    sortBy = "CreatedAT";
                    orderByFunc = incident => incident.CreatedAT;
                    break;
            }
            sortDirection = sortDirection.ToLower() switch
            {
                "asc" => "asc",
                "desc" => "desc",
                _ => "desc",
            };
            // var convertProperty = typeof(Incident).GetProperty(sortBy);

            try
            {



                var query = _incidentCollection.AsQueryable();
                var userQuery = _userCollection.AsQueryable();
                IOrderedEnumerable<Incident> sorted = null;

                if (sortBy != "CreatedBy" && sortBy != "AssignedTo" && sortDirection == "desc")
                    sorted = query.OrderByDescending(orderByFunc);
                else if (sortBy != "CreatedBy" && sortBy != "AssignedTo" && sortDirection == "asc")
                    sorted = query.OrderBy(orderByFunc);
                else if (sortBy == "CreatedBy" || sortBy == "AssignedTo")
                    sorted = query.OrderBy(orderByFunc);

                var join = sorted.Join(userQuery, i => i.AssignedTo, u => u.Id, (i, u) => new { incident = i, assUser = u })
                    .Join(userQuery, i => i.incident.CreatedBy, u => u.Id, (i, u) => new { incident2 = i, creUser = u })
                    ;


                if (sortBy == "CreatedBy" && sortDirection == "desc")
                {
                    join = join.OrderByDescending(iu => iu.creUser.FirstName);
                }
                else if (sortBy == "CreatedBy" && sortDirection == "asc")
                {
                    join = join.OrderBy(iu => iu.creUser.FirstName);
                }
                if (sortBy == "AssignedTo" && sortDirection == "desc")
                {
                    join = join.OrderByDescending(iu => iu.incident2.assUser.FirstName);
                }
                else if (sortBy == "AssignedTo" && sortDirection == "asc")
                {
                    join = join.OrderBy(iu => iu.incident2.assUser.FirstName);
                }


                var filtered = join.Where(j => j.incident2.incident.Title.ToLower().Contains(serach.ToLower()) || j.incident2.incident.Description.ToLower().Contains(serach.ToLower()));


                long total = filtered.Count();

                var final = filtered.Skip(pageSize * (pageNumber - 1))
                        .Take(pageSize)
                        .Select(incident => incident);

                var incidentQuery = (from data in final
                                     select new Incident
                                     {
                                         Id = data.incident2.incident.Id,
                                         CreatedAT = data.incident2.incident.CreatedAT,
                                         CreatedBy = data.creUser.FirstName + " Shareef",
                                         AssignedTo = data.incident2.assUser.FirstName + " Umar",
                                         StartTime = data.incident2.incident.StartTime,
                                         DueDate = data.incident2.incident.DueDate,
                                         Status = data.incident2.incident.Status,
                                         Title = data.incident2.incident.Title,
                                         Description = data.incident2.incident.Description,
                                         AdditionalData = data.incident2.incident.AdditionalData
                                     });


                // long total = await _incidentCollection.Find(_=>true).CountDocumentsAsync();

                List<Incident> incidents = incidentQuery.ToList();
                return new IncidentsWithPage
                {
                    Total_Incidents = (int)total,
                    Incidents = incidents
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IncidentsWithPage> GetIncidentsPageAsyncExp(int pageSize, int pageNumber, string? sortBy, string? sortDirection, string? serach)
        {
            var sortCol = "CreatedAT"; // for aggregation pipeline

            switch (sortBy.ToLower())
            {
                case "createdat":
                    sortCol = "CreatedAT";
                    sortBy = "CreatedAT";
                    orderByFunc = incident => incident.CreatedAT;
                    break;
                case "createdby":
                    sortCol = "createdByUser.FirstName";
                    sortBy = "CreatedBy";
                    orderByFunc = incident => incident.CreatedBy;
                    break;
                case "assignedto":
                    sortCol = "assignedToUser.FirstName";
                    sortBy = "AssignedTo";
                    orderByFunc = incident => incident.AssignedTo;
                    break;
                case "title":
                    sortCol = "Title";
                    sortBy = "Title";
                    orderByFunc = incident => incident.Title;
                    break;
                case "description":
                    sortCol = "Description";
                    sortBy = "Description";
                    orderByFunc = incident => incident.Description;
                    break;
                case "starttime":
                    sortCol = "StartTime";
                    sortBy = "StartTime";
                    orderByFunc = incident => incident.StartTime;
                    break;
                case "duedate":
                    sortCol = "DueDate";
                    sortBy = "DueDate";
                    orderByFunc = incident => incident.DueDate;
                    break;
                case "status":
                    sortCol = "Status";
                    sortBy = "Status";
                    orderByFunc = incident => incident.Status;
                    break;
                default:
                    sortCol = "CreatedAT";
                    sortBy = "CreatedAT";
                    orderByFunc = incident => incident.CreatedAT;
                    break;
            }
            sortDirection = sortDirection.ToLower() switch
            {
                "asc" => "asc",
                "desc" => "desc",
                _ => "desc",
            };
            // var convertProperty = typeof(Incident).GetProperty(sortBy);

            /*
                        IEnumerable<Incident> incidentQuery = null;

                        if (sortDirection == "asc") {
                            incidentQuery = from incident in _incidentCollection.AsQueryable()
                                            where incident.Title.ToLower().Contains(serach.ToLower()) || incident.Description.ToLower().Contains(serach.ToLower())

                                            select incident;

                            var tt = _incidentCollection.AsQueryable()
                                .OrderBy(orderByFunc)
                                .Join(_userCollection.AsQueryable(), i => i.AssignedTo, u => u.Id, (i, u) => new { incident = i, user = u })
                                .Where(j => j.incident.Title.ToLower().Contains(serach.ToLower()) || j.incident.Description.ToLower().Contains(serach.ToLower()))
                                .Skip(pageSize * (pageNumber - 1))
                                .Take(pageSize)
                                .Select(incident => incident);

                            incidentQuery = (from data in tt
                                             select new Incident
                                             {
                                                 Id = data.incident.Id,
                                                 CreatedAT = data.incident.CreatedAT,
                                                 CreatedBy = data.incident.CreatedBy,
                                                 AssignedTo = data.user.FirstName + " Umar",
                                                 StartTime = data.incident.StartTime,
                                                 DueDate = data.incident.DueDate,
                                                 Status = data.incident.Status,
                                                 Title = data.incident.Title,
                                                 Description = data.incident.Description,
                                                 AdditionalData = data.incident.AdditionalData
                                             });
                            incidentQuery = incidentQuery.OrderBy(i => convertProperty.GetValue(i));

                        }
                        else
                        {
                            incidentQuery = _incidentCollection.AsQueryable()
                                .OrderByDescending(orderByFunc)
                                .Where(incident => incident.Title.ToLower().Contains(serach.ToLower()) || incident.Description.ToLower().Contains(serach.ToLower()))
                                .Skip(pageSize * (pageNumber - 1))
                                .Take(pageSize)
                                .Select(incident => incident);
                            incidentQuery = from incident in _incidentCollection.AsQueryable()
                                            where incident.Title.ToLower().Contains(serach.ToLower()) || incident.Description.ToLower().Contains(serach.ToLower())
                                            select incident;
                            incidentQuery = incidentQuery.OrderByDescending(i => convertProperty.GetValue(i));
                        } */

            /*     var query = _incidentCollection.AsQueryable();
                 IOrderedEnumerable<Incident> sorted = null;

                 if (sortBy != "CreatedBy" && sortBy != "AssignedTo" && sortDirection == "desc")
                     sorted = query.OrderByDescending(orderByFunc);
                 else if (sortBy != "CreatedBy" && sortBy != "AssignedTo" && sortDirection == "asc")
                     sorted = query.OrderBy(orderByFunc);


                 var join = sorted.Join(_userCollection.AsQueryable(), i => i.AssignedTo, u => u.Id, (i, u) => new { incident = i, user = u })
                     .Join(_userCollection.AsQueryable(), i => i.incident.CreatedBy, u => u.Id, (i, u) => new { incident2 = i, user2 = u });


                 if(sortBy == "CreatedBy" && sortDirection == "desc")
                 {
                     join = join.OrderByDescending(iu => iu.user2.FirstName);
                 }
                 else if (sortBy == "CreatedBy" && sortDirection == "asc")
                 {
                     join = join.OrderBy(iu => iu.user2.FirstName);
                 }
                 if (sortBy == "AssignedTo" && sortDirection == "desc")
                 {
                     join = join.OrderByDescending(iu => iu.incident2.user.FirstName);
                 }
                 else if (sortBy == "AssignedTo" && sortDirection == "asc")
                 {
                     join = join.OrderBy(iu => iu.incident2.user.FirstName);
                 }


                 var filtered = join.Where(j => j.incident2.incident.Title.ToLower().Contains(serach.ToLower()) || j.incident2.incident.Description.ToLower().Contains(serach.ToLower()));


                 long total = filtered.Count();

                 var final = filtered.Skip(pageSize * (pageNumber - 1))
                         .Take(pageSize)
                         .Select(incident => incident);

                 incidentQuery = (from data in final
                                  select new Incident
                                  {
                                      Id = data.incident2.incident.Id,
                                      CreatedAT = data.incident2.incident.CreatedAT,
                                      CreatedBy = data.user2.FirstName + " Shareef",
                                      AssignedTo = data.incident2.user.FirstName + " Umar",
                                      StartTime = data.incident2.incident.StartTime,
                                      DueDate = data.incident2.incident.DueDate,
                                      Status = data.incident2.incident.Status,
                                      Title = data.incident2.incident.Title,
                                      Description = data.incident2.incident.Description,
                                      AdditionalData = data.incident2.incident.AdditionalData
                                  });


                // long total = await _incidentCollection.Find(_=>true).CountDocumentsAsync();

                 List<Incident> incidents = incidentQuery.ToList();
            */

            List<Incident> incidents = new List<Incident>();
            int count = 0;
            try
            {

                var pResults = _incidentCollection.Aggregate()
                                .Match(incident => incident.Title.ToLower().Contains(serach.ToLower()) || incident.Description.ToLower().Contains(serach.ToLower()))
                                .Project(new BsonDocument{
                                    { "_id", 1 },
                                    { "Title", 1 },
                                    {"Description", 1  },
                                    {"Status", 1 },
                                    {"StartTime", 1 },
                                    {"DueDate", 1 },
                                    {"CreatedAT", 1 },
                                    {
                                        "CreatedBy", new BsonDocument{
                                            {

                                                "$convert", new BsonDocument{
                                                    { "input", "$CreatedBy" },
                                                    { "to", "objectId" }
                                                }

                                                }
                                            }
                                        },
                                        {
                                        "AssignedTo", new BsonDocument{
                                            {
                                                "$convert", new BsonDocument{
                                                    { "input", "$AssignedTo" },
                                                    { "to", "objectId" }
                                                }
                                                }
                                            }
                                        }
                                    })
                                .Lookup("Users", "CreatedBy", "_id", "createdByUser")
                                .Lookup("Users", "AssignedTo", "_id", "assignedToUser")
                                //.Project(new BsonDocument{
                                //     { "_id", 1 },
                                //    { "Title", 1 },
                                //    {"Description", 1  },
                                //    {"Status", 1 },
                                //    {"StartTime", 1 },
                                //    {"DueDate", 1 },
                                //    {"CreatedAT", 1 },

                                //    {

                                //        "createdByUser", new BsonDocument{
                                //            {

                                //                "$convert", new BsonDocument{
                                //                    { "input", "$Id" },
                                //                    { "to", "string" }
                                //                }

                                //                }
                                //            }
                                //        },
                                //    { "assignedToUser", new BsonDocument{
                                //        {
                                //            "$map", new BsonDocument{
                                //                { "input", "$assignedToUser" },
                                //                { "as", "user" },
                                //                {
                                //                    "in", new BsonDocument{
                                //                        {
                                //                            "$convert", new BsonDocument{
                                //                                { "input", "$$user._id" },
                                //                                { "to", "string" }
                                //                            }
                                //                        }
                                //                    }
                                //                }
                                //            }
                                //        }
                                //    }
                                //    }})


                                .Sort(new BsonDocument
                                   (sortCol, sortDirection == "asc" ? 1 : -1)
                                    );
                count = pResults.ToEnumerable().Count();
                var incidentsData = await pResults.Skip(pageSize * (pageNumber - 1)).Limit(pageSize).ToListAsync();

                foreach (var i in incidentsData)
                {
                    string id = i.GetValue("_id").RawValue.ToString();
                    string title = i.GetValue("Title").RawValue.ToString();
                    string des = i.GetValue("Description").RawValue.ToString();
                    DateTime startTime = i.GetValue("StartTime").AsDateTime;
                    DateTime duedate = i.GetValue("DueDate").AsDateTime;
                    DateTime createdat = i.GetValue("CreatedAT").AsDateTime;
                    string status = i.GetValue("Status").RawValue.ToString();
                    BsonDocument user = i.GetValue("createdByUser")[0].AsBsonDocument;
                    string createdBy = user.GetValue("FirstName").RawValue.ToString() + " " + user.GetValue("LastName").RawValue.ToString();
                    user = i.GetValue("assignedToUser")[0].AsBsonDocument;
                    string assignedTo = user.GetValue("FirstName").RawValue.ToString() + " " + user.GetValue("LastName").RawValue.ToString();

                    var incident = new Incident
                    {
                        Id = id,
                        CreatedBy = createdBy,
                        AssignedTo = assignedTo,
                        Title = title,
                        Status = status,
                        Description = des,
                        CreatedAT = createdat,
                        StartTime = startTime,
                        DueDate = duedate
                    };

                    incidents.Add(incident);
                }
            }
            catch (Exception ex)
            {
            }
            return new IncidentsWithPage
            {
                Total_Incidents = count,
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
                        NotifyAbout = await _userService.GetNameByUserId(userId) + " updated " + parameter + " for an incident.",
                        SourceUserId = userId,
                        UserId = watch.UserId,
                        IncidentId = incidentId,
                    });
                }
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateCreateDateAsync()
        {
       
            var filter = Builders<Incident>.Filter
                .Gt(i => i.CreatedAT, DateTime.UtcNow);
            var update = Builders<Incident>.Update
                .Set("CreatedAT", DateTime.UtcNow.AddDays(-10));

            var updateResult = await _incidentCollection.UpdateManyAsync(filter, update);
            if (updateResult.ModifiedCount > 0)
            {            
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
            try
            {
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
                    select new
                    {
                        UserId = data.name,
                        Name = _userService.GetNameByUserId(data.name).Result,
                        Count = data.count
                    })
                    .ToList();
        }

    } //end of class
}
