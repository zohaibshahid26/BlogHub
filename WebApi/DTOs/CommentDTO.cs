using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class CommentDTO
    {
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Comment is required and cannot be empty")]
        public string Content { get; set; }
        public DateTime DatePosted { get; set; }

        [Required(ErrorMessage = "PostId is required")]
        public string PostId { get; set; }

        public string UserId { get; set; }
    }
}
