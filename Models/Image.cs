using System.ComponentModel.DataAnnotations;

namespace BlogHub.Models
{
    public class Image
    {
        [Key]
        [Required(ErrorMessage = "Image ID is required")]
        public int ImageId { get; set; }

        [Required(ErrorMessage = "Image URL is required")]
        [Url(ErrorMessage = "The Image URL must be a valid URL")]
        public required string ImageURL { get; set; }
    }

}
