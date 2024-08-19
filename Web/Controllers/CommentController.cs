using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<CommentController> _logger;
        private readonly ICommentService _commentService;

        public CommentController(IAuthorizationService authorizationService, ILogger<CommentController> logger, ICommentService commentService)
        {
            _authorizationService = authorizationService;
            _logger = logger;
            _commentService = commentService;
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Add(Comment comment)
        {
            if(!ModelState.IsValid)
            {
                return Json(new { redirectUrl = "/Error/400" });
            }

            if (User.Identity?.IsAuthenticated != true)
            {
                return Json( new { redirectUrl = "/Identity/Account/Login" + "?ReturnUrl=%2FPost%2FDetails%2F" + comment.PostId});
            }
            try
            {
                await _commentService.AddCommentAsync(comment);
                var savedComment = _commentService.GetCommentWithDetails(comment.CommentId, "User,User.Image");
                return PartialView("_CommentsPartial", savedComment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a new comment.");
                return Json(new { redirectUrl = "/Error/500" });
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {

            try
            {
                var comment = _commentService.GetCommentWithDetails(id, "Post,Post.User,User");
                if (comment == null)
                {
                    _logger.LogWarning("Comment with ID {CommentId} not found.", id);
                    return Json(new { redirectUrl = "/Error/404" });
                }

                var authorizationResult = await _authorizationService.AuthorizeAsync(User, comment, "DeleteCommentPolicy");
                if (!authorizationResult.Succeeded)
                {
                    return  Json(new { redirectUrl = "/Identity/Account/AccessDenied" });
                }

                await _commentService.DeleteCommentAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting comment with ID {CommentId}.", id);
                return Json(new { redirectUrl = "/Error/500" });
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(Comment comment)
        {
            try
            {
                Comment? commentToUpdate = _commentService.GetCommentWithDetails(comment.CommentId, "Post,Post.User,User");
                if (commentToUpdate == null)
                {
                    _logger.LogWarning("Comment with ID {CommentId} not found.", comment.CommentId);
                    return NotFound();
                }

                var authorizationResult = await _authorizationService.AuthorizeAsync(User, commentToUpdate, "EditCommentPolicy");
                if (!authorizationResult.Succeeded)
                {
                    return Forbid();
                }

                if (ModelState.IsValid)
                {
                    commentToUpdate.Content = comment.Content;
                    await _commentService.UpdateCommentAsync(commentToUpdate);
                }

                return RedirectToAction("Details", "Post", new { id = commentToUpdate.PostId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating comment with ID {CommentId}.", comment.CommentId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}