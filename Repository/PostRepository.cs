﻿using BlogHub.Data;
using BlogHub.Models;
using Microsoft.EntityFrameworkCore;
namespace BlogHub.Repository
{
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }   
        public async Task<bool> ToggleLikeAsync(string postId, string userId)
        {
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (existingLike == null)
            {
                _context.Likes.Add(new Like { PostId = postId, UserId = userId });
                return true;
            }
            else
            {
                _context.Likes.Remove(existingLike);
                return false;
            }
        }

    }
}