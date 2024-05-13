using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BlogHub.Models
{
    public class Post
    {
        [Key]
        [Required(ErrorMessage = "Post ID is required and cannot be empty")]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Title is required and cannot be empty")]
        public required string Title { get; set; }

       
        [Required(ErrorMessage = "Content is required and cannot be empty")]
        public required string Content{ get; set;}

        public DateOnly DatePosted { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [Required(ErrorMessage = "Category is required")]
        public string CategoryName { get; set; }
        public Category Category { get; set; }

        public List<Tag>? Tags { get; set; }

        public int? ImageId { get; set; }
        [ForeignKey("ImageId")]
        public Image? Image { get; set; }

        public  string UserId { get; set; }
        [ForeignKey("UserId")]
        [Required(ErrorMessage = "User is required")]
        public  IdentityUser User { get; set; }

        public List<Comment>? Comments { get; set; }

        public int Likes { get; set; } = 0;

        public int TimeToRead => CalculateReadTime(Content);

        private int CalculateReadTime(string content)
        {
            int wordsPerMinute = 200; // Average reading speed
            int numberOfWords = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return (int)Math.Ceiling((double)numberOfWords / wordsPerMinute);
        }
    }
}
