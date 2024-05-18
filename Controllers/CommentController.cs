using BlogHub.Authorization;
using BlogHub.Models;
using BlogHub.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogHub.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IAuthorizationService _authorizationService;
        public CommentController(ICommentRepository commentRepository, IAuthorizationService authorizationService)
        {
            _commentRepository = commentRepository;
            _authorizationService = authorizationService;
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
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, comment, "DeleteCommentPolicy");
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
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
