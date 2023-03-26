using IM_WebAPICore_MongoDB.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace IM_WebAPICore_MongoDB.DataService
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IConfiguration _config;
        public UserService(IConfiguration config)
        {
            _config = config;
            MongoClient client = new MongoClient(_config.GetSection("MongoDB:ConnectionURI").Value);
            IMongoDatabase database = client.GetDatabase(_config.GetSection("MongoDB:DatabaseName").Value);
            _userCollection = database.GetCollection<User>("Users");

        }

        public async Task<List<User>> GetAsync()
        {
            return await _userCollection.Find(_ => true).ToListAsync();

        }
        public async Task<User?> GetAsync(string id) =>
            await _userCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(User newBook) =>
            await _userCollection.InsertOneAsync(newBook);

        public async Task UpdateAsync(string id, User updatedBook) =>
            await _userCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveAsync(string id) =>
            await _userCollection.DeleteOneAsync(x => x.Id == id);
    }
}
