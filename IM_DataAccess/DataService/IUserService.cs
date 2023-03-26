using IM.Models;
using IM_DataAccess.Models;

namespace IM_DataAccess.DataService
{
    public interface IUserService
    {
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model);
        Task CreateAsync(User newBook);
        Task<List<User>> GetAsync();
        Task<User?> GetAsync(string id);
        Task RemoveAsync(string id);
        Task UpdateAsync(string id, User updatedBook);
    }
}