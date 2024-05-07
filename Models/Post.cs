using Azure;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BlogHub.Models
{
    public class Post
    {
        [Key]
        [Required(ErrorMessage = "Post ID is required and cannot be empty")]
        public required string Id { get; set; }

        [Required(ErrorMessage = "Title is required and cannot be empty")]
        public required string Title { get; set; }

        private  string _content;
        [Required(ErrorMessage = "Content is required and cannot be empty")]
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                TimeToRead = CalculateReadTime(_content);
            }
        }
        public DateOnly DatePosted { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [Required(ErrorMessage = "Category is required")]
        public required string CategoryName { get; set; }
        public required Category Category { get; set; }

        public List<Tag> ?Tags { get; set; }

        public int? ImageId { get; set; }
        [ForeignKey("ImageId")]
        public Image ?Image { get; set; }

        public required string UserId { get; set; }
        [ForeignKey("UserId")]
        [Required(ErrorMessage = "User is required")]
        public required IdentityUser User { get; set; }

        public List<Comment> ?Comments { get; set; }

        public int TimeToRead { get; private set; }
        public string Content1 { get => _content; set => _content = value; }

        private int CalculateReadTime(string content)
        {
            int wordsPerMinute = 200; // Average reading speed
            int numberOfWords = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return (int)Math.Ceiling((double)numberOfWords / wordsPerMinute);
        }
    }
}
