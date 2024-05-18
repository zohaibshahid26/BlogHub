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
        [AllowAnonymous]
        public async Task<IActionResult> Add(Comment comment)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Redirect("/Identity/Account/Login" + "?ReturnUrl=%2FPost%2FDetails%2F" + comment.PostId);
            }
            await _commentRepository.AddCommentAsync(comment);
            await _commentRepository.SaveChangesAsync();
            return RedirectToAction("Details", "Post", new { id = comment.PostId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Comment comment)
        {
            await _commentRepository.DeleteCommentAsync(comment.CommentId);
            await _commentRepository.SaveChangesAsync();
            return RedirectToAction("Details", "Post", new { id = comment.PostId });
        }
       
        [HttpPost]
        public async Task<IActionResult> Edit(Comment comment)
        {
            _commentRepository.UpdateComment(comment);
            await _commentRepository.SaveChangesAsync();
            return RedirectToAction("Details", "Post", new { id = comment.PostId });
        }

    }
}
