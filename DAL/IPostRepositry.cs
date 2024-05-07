using BlogHub.Models;
namespace BlogHub.DAL
{
    public interface IPostRepositry
    {
        public List<Post> GetAllPosts();
        public Post GetPostById(string id);
        public void AddPost(Post post);
        public void UpdatePost(Post post);
        public void DeletePost(string id);
    }
}
