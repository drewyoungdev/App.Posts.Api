using System.Collections.Generic;
using System.Threading.Tasks;
using PostsApi.Enums;
using PostsApi.Models;

namespace PostsApi.Repositories.Interfaces
{
    public interface IPostsRepository
    {
        Task<List<Post>> GetMainFeed(int limit);
        Task<Post> GetRootPost(int rootPostId);
        Task<List<Post>> GetReplies(RepliesSortType sortType, int parentId, int? directReplyLimit, int? directReplyOffset, int startDepth, int depthLimit, int recursiveLimit);
    }
}
