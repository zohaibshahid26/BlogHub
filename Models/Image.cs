using System.ComponentModel.DataAnnotations;

namespace BlogHub.Models
{
    public class Image
    {
        [Key]
        [Required(ErrorMessage = "Image ID is required")]
        public required int ImageId { get; set; }

        [Required(ErrorMessage = "Image data is required")]
        public required byte[] ImageData { get; set; }

    }

}
