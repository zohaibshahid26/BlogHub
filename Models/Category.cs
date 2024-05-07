using System.ComponentModel.DataAnnotations;

namespace BlogHub.Models
{
    public class Category
    {
        [Key]
        [Required(ErrorMessage = "Category name is required and cannot be empty")]
        public required string CategoryName { get; set; }

        public List<Post> ?Posts { get; set; } 
    }

}
