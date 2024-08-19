using Domain.Entities;
using Domain.Interfaces;
namespace Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddCommentAsync(Comment comment)
        {
            await _unitOfWork.CommentRepository.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(int id)
        {
            await _unitOfWork.CommentRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            _unitOfWork.CommentRepository.Update(comment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _unitOfWork.CommentRepository.GetByIdAsync(id);
        }

        public Comment? GetCommentWithDetails(int id, string includeProperties)
        {
            return _unitOfWork.CommentRepository.Get(filter: c => c.CommentId == id, includeProperties: includeProperties).FirstOrDefault();
        }
    }
}
