
using IM.Models;
using IM_DataAccess.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Data;

namespace IM_DataAccess.DataService
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<UserLogin> _userLoginCollection;
        private readonly IConfiguration _config;
       
        public UserService(IConfiguration config)
        {
            _config = config;
            MongoClient client = new MongoClient(_config.GetValue<string>("MongoDbCon"));
            IMongoDatabase database = client.GetDatabase(_config.GetValue<string>("DB"));
            _userCollection = database.GetCollection<User>("Users");
            _userLoginCollection = database.GetCollection<UserLogin>("UserLogins");
        }

        public async Task<UserLogin> LoginAsync(string username, string password)
        {
            var loginTable = new DataTable();
            var userTable = new DataTable();
            var _userLogin = from userLogin in _userLoginCollection.AsQueryable()
                  where userLogin.Username == username
                  && userLogin.Password == password
                  select userLogin;

            return _userLogin.FirstOrDefault();
            //if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            //    return null;

            //loginTable = ds.Tables[0];
            //userTable = ds.Tables[1];

            //var Users = (from rw in userTable.AsEnumerable()
            //             select new User()
            //             {
            //                 Id = rw["Id"].ToString(),
            //                 CreateDate = DateTime.Parse(rw["CreateDate"].ToString()),
            //                 FirstName = rw["FirstName"].ToString(),
            //                 LastName = rw["LastName"].ToString(),
            //                 ProfilePic = rw["ProfilePic"].ToString(),
            //                 Email = rw["Email"].ToString(),
            //                 Phone = rw["Phone"].ToString(),
            //             }).ToList();


            //var userLogin = (from rw in loginTable.AsEnumerable()
            //                 select new UserLogin()
            //                 {
            //                     Id = rw["Id"].ToString(),
            //                     user = Users.First(),
            //                     Username = rw["Username"].ToString(),
            //                     Password = rw["Password"].ToString(),
            //                     CreateDate = DateTime.Parse(rw["CreateDate"].ToString()),
            //                     LastLogin = DateTime.Now // DateTime.Parse(rw["LastLogin"].ToString())
            //                 }).ToList().First();

            //_userLogin.UnreadConversationCount = int.Parse(ds.Tables[2].Rows[0][0].ToString());

            //return _userLogin;
        }

        public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model)
        {
          //  var userLogin = await LoginAsync(model.Username, model.Password);

            var userLogin = await LoginAsync(model.Username, model.Password);
            if (userLogin is null)
                return null;
            var userDetails = await _userCollection.FindAsync(u => u.Id == userLogin.userId);

            var response = new AuthenticateResponse
            {
                Id = userLogin.Id,
                Username = userLogin.Username,
                Password = userLogin.Password,
                user = userDetails.FirstOrDefault(),
            };           
           
            return response;
        }

        public async Task<List<User>> GetAsync()
        {
            var userLogin = await LoginAsync("umar", "password");
            var user = await _userCollection.FindAsync(u => u.Id == userLogin.userId);
            
            
            
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
            

        public async Task UpdateAsync(string id, User updatedBook) =>
            await _userCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveAsync(string id) =>
            await _userCollection.DeleteOneAsync(x => x.Id == id);
    }
}
