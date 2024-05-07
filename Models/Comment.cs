using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlogHub.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        [Required(ErrorMessage = "Comment content is required and cannot be empty")]
        public required string Content { get; set; }
        public DateTime DatePosted { get; set; }
        public required string UserId { get; set; }
        [Required(ErrorMessage = "User is required for commenting")]
        public required IdentityUser User { get; set; }
        public required string PostId { get; set; }
        [Required(ErrorMessage = "Post is required for commenting")]
        public required Post Post { get; set; }
    }

}
