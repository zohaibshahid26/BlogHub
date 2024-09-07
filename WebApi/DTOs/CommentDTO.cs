namespace WebApi.DTOs
{
    public class CommentDTO
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public DateTime DatePosted { get; set; }
        public string UserName { get; set; }
    }
}
