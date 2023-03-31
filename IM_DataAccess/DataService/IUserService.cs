using IM.Models;
using IM_DataAccess.Models;

namespace IM_DataAccess.DataService
{
    public interface IUserService
    {
        Task<User> AddUserAsync(User user);
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model);
        Task CreateAsync(User newUser);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetAsync();
        Task<User?> GetAsync(string id);
        Task<UsersWithPage> GetUsersPageAsync(int pageSize, int pageNumber, string sortBy, string sortDirection, string Serach);
        Task RemoveAsync(string id);
        Task UpdateAsync(string id, User updatedUser);
        Task<bool> UpdateHubIdAsync(string userId, string hubId);

        Task CreateLoginAsync(User user);
    }
}