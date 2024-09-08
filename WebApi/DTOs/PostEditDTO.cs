using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class PostEditDTO
    {
        [Required(ErrorMessage = "PostId is required")]
        public string PostId { get; set; }  // Unique identifier of the post to be edited

        [Required(ErrorMessage = "Title is required and cannot be empty")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required and cannot be empty")]
        public string Content { get; set; }

        // Optional image upload for editing the post's image
        public IFormFile? Image { get; set; }

        // Category name for assigning the post to a category
        [Required(ErrorMessage = "Category is required")]
        public string CategoryName { get; set; }

        // Tags as a comma-separated string
        public string? Tags { get; set; }
    }
}