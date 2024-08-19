using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required and cannot be empty")]
        public string CategoryName { get; set; }
        public List<Post>? Posts { get; set; }
    }
}
