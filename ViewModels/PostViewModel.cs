using System.ComponentModel.DataAnnotations;


namespace BlogHub.ViewModels
{
    public class PostViewModel
    {

        [Required(ErrorMessage = "Title is required and cannot be empty")]
        public required string Title { get; set; } 
        [Required(ErrorMessage = "Content is required and cannot be empty")]
        public required string Content { get; set; } 
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public required string CategoryName { get; set; } 
        [Display(Name = "Tags (separate with commas)")]
        public string? Tags { get; set; }

    }
}
