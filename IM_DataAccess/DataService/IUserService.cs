using IM_WebAPICore_MongoDB.Models;

namespace IM_WebAPICore_MongoDB.DataService
{
    public interface IUserService
    {
        Task CreateAsync(User newBook);
        Task<List<User>> GetAsync();
        Task<User?> GetAsync(string id);
        Task RemoveAsync(string id);
        Task UpdateAsync(string id, User updatedBook);
    }
}