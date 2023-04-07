
using IM.Models;
using IM_DataAccess.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Data;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace IM_DataAccess.DataService
{
    public interface IUserService
    {
        Task<User> AddUserAsync(User user);
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model);
        Task CreateAsync(User newUser);
        Task CreateLoginAsync(User user);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetAsync();
        Task<User?> GetAsync(string id);
        Task<List<string>> GetHubIdsAsync(string incidentId, string userId);
        Task<string> GetNameByUserId(string userId);
        Task<UsersWithPage> GetUsersPageAsync(int pageSize, int pageNumber, string? sortBy, string? sortDirection, string? serach);
        Task RemoveAsync(string id);
        Task UpdateAsync(string id, User updatedUser);
        Task<bool> UpdateHubIdAsync(string userId, string hubId);
    }

    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<UserLogin> _userLoginCollection;
        private readonly IMongoCollection<IncidentNotification> _incidentNotification;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _memoryCache;

        public UserService(IConfiguration config, IMemoryCache memoryCache)
        {
            _config = config;
            MongoClient client = new MongoClient(_config.GetValue<string>("MongoDbCon"));
            IMongoDatabase database = client.GetDatabase(_config.GetValue<string>("DB"));
            _userCollection = database.GetCollection<User>("Users");
            _userLoginCollection = database.GetCollection<UserLogin>("UserLogins");
            _incidentNotification = database.GetCollection<IncidentNotification>("IncidentNotifications");
            _memoryCache = memoryCache;
        }


        public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model)
        {

            var _userLogin = from userLogin in _userLoginCollection.AsQueryable()
                             where userLogin.Username == model.Username
                             && userLogin.Password == model.Password
                             select userLogin;

            var _userLoginData = _userLogin.FirstOrDefault();

            if (_userLoginData is null)
                return null;
            var userDetails = await _userCollection.FindAsync(u => u.Id == _userLoginData.userId);

            var response = new AuthenticateResponse
            {
                Id = _userLoginData.Id,
                Username = _userLoginData.Username,
                Password = _userLoginData.Password,
                user = userDetails.FirstOrDefault(),
            };

            return response;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> allUsers = _memoryCache.Get<List<User>>("allUsers");
            if (allUsers is null)
            {
                allUsers = await _userCollection.Find(_ => true).ToListAsync();
                _memoryCache.Set("allUsers", allUsers);
            }

            return allUsers;
        }

        public async Task<string> GetNameByUserId(string userId)
        {
            var allUser = await GetAllUsersAsync();
            string name = allUser.Where(u => u.Id == userId).Select(u => u.FirstName + " " + u.LastName).First();
            return name;
        }
        public async Task<User> AddUserAsync(User user)
        {
            await _userCollection.InsertOneAsync(user);
            _memoryCache.Remove("allUsers");
            return user;
        }

        public async Task<bool> UpdateHubIdAsync(string userId, string hubId)
        {
            var filter = Builders<User>.Filter
               .Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.HubId, hubId);

            var updateResult = await _userCollection.UpdateOneAsync(filter, update);
            if (updateResult.ModifiedCount > 0)
                return true;
            return false;
        }
        public async Task<UsersWithPage> GetUsersPageAsync(int pageSize, int pageNumber, string? sortBy, string? sortDirection, string? serach)
        {

            //var  usersQuery =  _userCollection.Find(u => 
            //              string.IsNullOrEmpty(serach)? true :
            //   u.FirstName.ToLower().Contains(serach.ToLower()) || u.LastName.ToLower().Contains(serach.ToLower()));
            //u.FirstName.Contains(serach) || u.LastName.Contains(serach));

            var usersQuery = from user in _userCollection.AsQueryable()
                             where user.FirstName.ToLower().Contains(serach.ToLower()) || user.LastName.ToLower().Contains(serach.ToLower())
                             orderby user.CreateDate descending
                             select user;



            int total = usersQuery.ToEnumerable().Count();

            var users = usersQuery.ToEnumerable().Skip(pageSize * (pageNumber - 1)).Take(pageSize).OrderByDescending(u => u.CreateDate).ToList();

            return new UsersWithPage
            {
                Total_Users = total,
                Users = users
            };
        }


        public async Task<List<User>> GetAsync()
        {
            return await _userCollection.Find(_ => true).ToListAsync();
        }
        public async Task<User?> GetAsync(string id) =>
            await _userCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(User newUser)
        {
            //await _userLoginCollection.InsertOneAsync(new UserLogin
            // {
            //     userId = "641f7abd387e0a505b180d98",
            //     Username = "umar",
            //     Password ="password",
            //     CreateDate = DateTime.UtcNow

            // });

            await _userCollection.InsertOneAsync(newUser);
        }

        public async Task CreateLoginAsync(User user)
        {
            await _userLoginCollection.InsertOneAsync(new UserLogin
            {
                userId = user.Id,
                Username = user.FirstName,
                Password = "password",
                CreateDate = DateTime.UtcNow

            });

            //  await _userCollection.InsertOneAsync(newUser);
        }
        public async Task<List<string>> GetHubIdsAsync(string incidentId, string userId)
        {
            var notifications = await _incidentNotification.Find(n => n.IncidentId == incidentId).ToListAsync();

            List<string> ids = notifications.Select(n => n.UserId).ToList();

            List<ObjectId> objIds = new List<ObjectId>();
            foreach (var id in ids)
            {
                objIds.Add(ObjectId.Parse(id));
            }

            var filter = Builders<User>.Filter.AnyIn("_id", objIds);
            var userQuery = await _userCollection.FindAsync(filter);

            List<string> hubIds = userQuery.ToList().Select(x => x.HubId).ToList();

            return hubIds;
        }


        public async Task UpdateAsync(string id, User updatedUser) =>
            await _userCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

        public async Task RemoveAsync(string id) =>
            await _userCollection.DeleteOneAsync(x => x.Id == id);
    }
}
