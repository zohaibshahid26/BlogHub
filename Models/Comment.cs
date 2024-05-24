using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogHub.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Comment is required and cannot be empty")]
        public required string Content { get; set; }
        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "User is required for commenting")]
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public MyUser? User { get; set; }


        public string? PostId { get; set; }
        [ForeignKey("PostId")]
        public Post? Post { get; set; }
    }

}
