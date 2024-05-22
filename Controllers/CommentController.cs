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
        public CommentController(IAuthorizationService authorizationService, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            await _unitOfWork.CommentRepository.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Details", "Post", new { id = comment.PostId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = _unitOfWork.CommentRepository.Get(filter: c => c.CommentId == id, includeProperties: "Post,Post.User,User").FirstOrDefault();
            if (comment == null)
            {
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

        [HttpPost]
        public IActionResult Edit(Comment comment)
        {
            var commentToUpdate = _unitOfWork.CommentRepository.Get(filter: c => c.CommentId == comment.CommentId, includeProperties: "Post,Post.User,User").FirstOrDefault();
            if (commentToUpdate == null)
            {
                return NotFound();
            }

            var authorizationResult = _authorizationService.AuthorizeAsync(User, commentToUpdate, "EditCommentPolicy").Result;
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            commentToUpdate.Content = comment.Content;
            _unitOfWork.CommentRepository.Update(commentToUpdate);
            _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Details", "Post", new { id = commentToUpdate.PostId });
        }

    }
}
