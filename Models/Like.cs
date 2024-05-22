using BlogHub.Helper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogHub.Models
{
    public class Like
    {
        [Key]
        public int LikeId { get; set; }  // Primary key for the Like record

        [Required(ErrorMessage = "User is required for liking a post")]
        public required string UserId { get; set; }
        [ForeignKey("UserId")]
        public MyUser? User { get; set; }  // Navigation property to the User

        public string? PostId { get; set; }
        [ForeignKey("PostId")]
        public Post? Post { get; set; }  // Navigation property to the Post
    }
}
