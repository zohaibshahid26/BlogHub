﻿using BlogHub.Models;
using System.ComponentModel.DataAnnotations;


namespace BlogHub.ViewModels
{
    public class PostViewModel
    {

        public string? PostId { get; set; }
        [Required(ErrorMessage = "Title is required and cannot be empty")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Content is required and cannot be empty")]
        public string Content { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public Category Category { get; set; }
        public IEnumerable<Category>? Categories { get; set; }

        [Display(Name = "Tags (separate with commas)")]
        public string? Tags { get; set; }

    }
}
