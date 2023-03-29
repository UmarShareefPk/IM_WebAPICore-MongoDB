using IM_DataAccess.Models;

namespace IM_DataAccess.Models
{
    public class UsersWithPage
    {
        public int Total_Users { get; set; }
        public List<User> Users { get; set; }
    }
}
