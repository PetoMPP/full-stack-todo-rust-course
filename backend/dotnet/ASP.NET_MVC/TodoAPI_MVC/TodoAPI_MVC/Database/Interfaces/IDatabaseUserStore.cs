using Microsoft.AspNetCore.Identity;

namespace TodoAPI_MVC.Database.Interfaces
{
    public interface IDatabaseUserStore<TUser> :
        IUserStore<TUser>,
        IUserPasswordStore<TUser>
        where TUser : class
    {
    }
}
