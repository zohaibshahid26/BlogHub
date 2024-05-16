using BlogHub.Models;
using BlogHub.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHub.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        public CommentController(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Add(Comment comment, string PostId)
        {
            await _commentRepository.AddCommentAsync(comment);
            await _commentRepository.SaveChangesAsync();
            return RedirectToAction("Details", "Post", new { id = comment.PostId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int CommentId, string PostId)
        {
            await _commentRepository.DeleteCommentAsync(CommentId);
            await _commentRepository.SaveChangesAsync();
            
            return RedirectToAction("Details", "Post", new { id = PostId });

        }
        
    }
}
