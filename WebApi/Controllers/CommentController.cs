using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
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

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Add([FromBody] CommentDTO commentDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid data" });
            }
            try
            {
                var comment = new Comment
                {
                    Content = commentDTO.Content,
                    DatePosted = DateTime.UtcNow,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    PostId = commentDTO.PostId
                };

                // Add comment to the database
                await _commentService.AddCommentAsync(comment);

                // Get the saved comment with all necessary details
                var savedComment = _commentService.GetCommentWithDetails(comment.CommentId, "User,User.Image");
                if (savedComment != null)
                {
                    var responseDTO = new CommentDTO
                    {
                        CommentId = savedComment.CommentId,
                        Content = savedComment.Content,
                        DatePosted = savedComment.DatePosted,
                        PostId = savedComment?.PostId ?? "",
                        UserId = savedComment?.UserId ?? "Unknown"
                    };

                    return Ok(responseDTO);
                }
                else
                {
                    return NotFound(new { message = "Comment not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a new comment.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] CommentDTO commentDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid data" });
            }

            try
            {
                var commentToUpdate = _commentService.GetCommentWithDetails(id, "Post,Post.User,User");
                if (commentToUpdate == null)
                {
                    _logger.LogWarning("Comment with ID {CommentId} not found.", id);
                    return NotFound(new { message = $"Comment with ID {id} not found." });
                }
                var authorizationResult = await _authorizationService.AuthorizeAsync(User, commentToUpdate, "EditCommentPolicy");
                if (!authorizationResult.Succeeded)
                {
                    return Forbid();
                }

                commentToUpdate.Content = commentDTO.Content;

                await _commentService.UpdateCommentAsync(commentToUpdate);

                return Ok(new { success = true, message = "Comment updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating comment with ID {CommentId}.", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var comment = _commentService.GetCommentWithDetails(id, "Post,Post.User,User");
                if (comment == null)
                {
                    _logger.LogWarning("Comment with ID {CommentId} not found.", id);
                    return NotFound(new { message = $"Comment with ID {id} not found." });
                }

                var authorizationResult = await _authorizationService.AuthorizeAsync(User, comment, "DeleteCommentPolicy");
                if (!authorizationResult.Succeeded)
                {
                    return Forbid();
                }

                await _commentService.DeleteCommentAsync(id);
                return Ok(new { success = true, message = "Comment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting comment with ID {CommentId}.", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }



    }
}
