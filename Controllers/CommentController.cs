using BlogHub.Models;
using BlogHub.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHub.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<CommentController> _logger;

        public CommentController(IAuthorizationService authorizationService, IUnitOfWork unitOfWork, ILogger<CommentController> logger)
        {
            _unitOfWork = unitOfWork;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Add(Comment comment)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Redirect("/Identity/Account/Login" + "?ReturnUrl=%2FPost%2FDetails%2F" + comment.PostId);
            }

            try
            {
                await _unitOfWork.CommentRepository.AddAsync(comment);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction("Details", "Post", new { id = comment.PostId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a new comment.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var comment = _unitOfWork.CommentRepository.Get(filter: c => c.CommentId == id, includeProperties: "Post,Post.User,User").FirstOrDefault();
                if (comment == null)
                {
                    _logger.LogWarning("Comment with ID {CommentId} not found.", id);
                    return NotFound();
                }

                var authorizationResult = await _authorizationService.AuthorizeAsync(User, comment, "DeleteCommentPolicy");
                if (!authorizationResult.Succeeded)
                {
                    return Forbid();
                }

                await _unitOfWork.CommentRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction("Details", "Post", new { id = comment.PostId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting comment with ID {CommentId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Comment comment)
        {
            try
            {
                Comment? commentToUpdate = _unitOfWork.CommentRepository.Get(filter: c => c.CommentId == comment.CommentId, includeProperties: "Post,Post.User,User").FirstOrDefault();
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
                    _unitOfWork.CommentRepository.Update(commentToUpdate);
                    await _unitOfWork.SaveChangesAsync();
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
