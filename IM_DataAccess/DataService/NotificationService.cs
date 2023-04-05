using IM_DataAccess.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IM_DataAccess.DataService
{
    public interface INotificationService
    {
        Task AddNotification(IncidentNotification incidentNotification);
        Task AddToWatchList(string incidentId, string userId);
        Task<List<IncidentNotification>> GetUserNotificationsAsync(string userId);
        Task<List<WatchList>> GetWatchListByIncident(string incidentId);
        Task<bool> UpdateIsReadAsync(string notificationId, bool isRead);
    }

    public class NotificationService : INotificationService
    {
        private readonly IMongoCollection<IncidentNotification> _incidentNotification;
        private readonly IMongoCollection<WatchList> _watchList;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<UserLogin> _userLoginCollection;
        private readonly IConfiguration _config;

        public NotificationService(IConfiguration config)
        {
            _config = config;
            MongoClient client = new MongoClient(_config.GetValue<string>("MongoDbCon"));
            IMongoDatabase database = client.GetDatabase(_config.GetValue<string>("DB"));
            _watchList = database.GetCollection<WatchList>("WatchList");
            _incidentNotification = database.GetCollection<IncidentNotification>("IncidentNotifications");
            _userCollection = database.GetCollection<User>("Users");
            _userLoginCollection = database.GetCollection<UserLogin>("UserLogins");
        }

        public async Task AddToWatchList(string incidentId, string userId)
        {
            var check =  _watchList.Find(wl => wl.IncidentId == incidentId && wl.UserId == userId);

            if (check.ToList().Count() == 0)
                await _watchList.InsertOneAsync(new WatchList()
                {
                    IncidentId = incidentId,
                    UserId = userId
                });
        }

        public async Task<List<WatchList>> GetWatchListByIncident(string incidentId)
        {
            return await _watchList.Find(w => w.IncidentId == incidentId).ToListAsync();
        }

        public async Task AddNotification(IncidentNotification incidentNotification)
        {
            await _incidentNotification.InsertOneAsync(incidentNotification);
        }

        public async Task<List<IncidentNotification>> GetUserNotificationsAsync(string userId)
        {
            return await _incidentNotification.Find(noti => noti.UserId == userId).ToListAsync();
        }

        public async Task<bool> UpdateIsReadAsync(string notificationId, bool isRead)
        {
            var filter = Builders<IncidentNotification>.Filter
               .Eq(n => n.Id, notificationId);
            var update = Builders<IncidentNotification>.Update
                .Set(n => n.IsRead, isRead);

            var updateResult = await _incidentNotification.UpdateOneAsync(filter, update);
            if (updateResult.ModifiedCount > 0)
                return true;
            return false;

        }

    }// end of class
}
