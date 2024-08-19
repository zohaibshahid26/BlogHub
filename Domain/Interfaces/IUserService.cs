using Domain.Entities;
namespace Domain.Interfaces
{
    public interface IUserService
    {
        MyUser? GetUserById(string id);
    }
}
