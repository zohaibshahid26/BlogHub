using BlogHub.Helper;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BlogHub.Models
{
    public class Post
    {
        [Key]
        [Required(ErrorMessage = "Post ID is required and cannot be empty")]
        public  string PostId { get; set; }

        [Required(ErrorMessage = "Title is required and cannot be empty")]
        public required string Title { get; set; }

       
        [Required(ErrorMessage = "Content is required and cannot be empty")]
        public required string  Content{ get; set;}

        public DateOnly DatePosted { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [Required(ErrorMessage = "Category is required")]
        public  int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public  Category Category { get; set; }

        public List<Tag>? Tags { get; set; }

        public int? ImageId { get; set; }
        [ForeignKey("ImageId")]
        public Image? Image { get; set; }

        [Required(ErrorMessage = "User is required")]
        public  string UserId { get; set; }
        [ForeignKey("UserId")]
        public MyUser User { get; set; }

        public List<Comment>? Comments { get; set; }

        public List<Like>? Likes { get; set; }

        public int TimeToRead => CalculateReadTime(Content);

        private int CalculateReadTime(string content)
        {
            int wordsPerMinute = 200; // Average reading speed
            int numberOfWords = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return (int)Math.Ceiling((double)numberOfWords / wordsPerMinute);
        }
    }
}
