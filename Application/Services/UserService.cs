using Domain.Interfaces;
using Domain.Entities;
namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public MyUser? GetUserById(string id)
        {
            return _unitOfWork.UserRepository.Get(filter: u => u.Id == id, includeProperties: "Image").FirstOrDefault();
        }

    }
}
