using System.ComponentModel.DataAnnotations;
namespace BlogHub.Models
{
    public class Post
    {
        [Key]
        [Required(ErrorMessage = "Id is required")]
        public string Id { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }
        public Guid ImageId { get; set; } // Reference to the Image table
        public DateTime DatePosted { get; set; }
        public string AuthorId { get; set; }
        public string Category { get; set; }
        public int TimeToRead { get; set; }
    }
}
