using System.ComponentModel.DataAnnotations;

namespace BlogHub.Models
{
    public class Tag
    {
        [Key]
        [Required(ErrorMessage = "Tag ID is required")]
        public int TagId { get; set; }

        [Required(ErrorMessage = "Tag name is required and cannot be empty")]
        public  string TagName { get; set; }

        public List<Post>? Posts { get; set; }
    }

}
