namespace BlogHub.Repository
{
    public interface ILikeRepository : IDisposable
    {
        Task ToggleLikeAsync(string postId, string userId);
        Task<int> GetLikesCountForPostAsync(string postId);
    }
}
