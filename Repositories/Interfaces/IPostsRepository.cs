using System.Collections.Generic;
using System.Threading.Tasks;
using PostsApi.Models;

namespace PostsApi.Repositories.Interfaces
{
    public interface IPostsRepository
    {
        Task<List<Post>> GetMainFeed(int limit);
        Task<List<Post>> GetRootPostWithReplies(int rootPostId);
        Task<List<Post>> GetReplies(int parentId, int directReplyLimit, int depthLimit, int recursiveLimit);
    }
}
