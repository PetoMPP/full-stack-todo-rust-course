using Microsoft.AspNetCore.Identity;

namespace TodoAPI_MVC.Database
{
    public interface IDatabaseUserStore<TUser> :
        IUserStore<TUser>,
        IUserPasswordStore<TUser>
        where TUser : class
    {
    }
}
