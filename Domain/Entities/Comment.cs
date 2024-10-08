﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Domain.Entities
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Comment is required and cannot be empty")]
        public string Content { get; set; }
        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public MyUser? User { get; set; }


        public string? PostId { get; set; }
        [ForeignKey("PostId")]
        public Post? Post { get; set; }
    }

}
