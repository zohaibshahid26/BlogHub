using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class PostAddDTO
    {
        [Required(ErrorMessage = "Title is required and cannot be empty")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required and cannot be empty")]
        public string Content { get; set; }

        // For image upload during post creation
        public IFormFile? Image { get; set; }

        // Category name for assigning the post to a category
        [Required(ErrorMessage = "Category is required")]
        public string CategoryName { get; set; }
        public string? Tags { get; set; }
    }
}