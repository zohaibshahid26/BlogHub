namespace WebApi.DTOs
{
    public class PostDTO
    {
        public string PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateOnly DatePosted { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<TagDTO>? Tags { get; set; }
        public ImageDTO? Image { get; set; }
        public List<CommentDTO>? Comments { get; set; }
        public List<LikeDTO>? Likes { get; set; }
        public int TimeToRead { get; set; }
        public int ViewCount { get; set; }
    }
}
