using System.Collections.Generic;
using System.Threading.Tasks;
using PostsApi.Models;

namespace PostsApi.Repositories.Interfaces
{
    public interface IPostsRepository
    {
        Task<List<Post>> GetMainFeed(int limit);
        Task<Post> GetRootPost(int rootPostId);
        Task<List<Post>> GetRepliesLowActivity(int parentId, int? directReplyLimit, int depthLimit, int recursiveLimit);
        Task<List<Post>> GetRepliesHighActivity(int parentId, int? directReplyLimit, int depthLimit, int recursiveLimit);
    }
}
