using System.Collections.Generic;
using System.Threading.Tasks;
using PostsApi.Enums;
using PostsApi.Models;

namespace PostsApi.BusinessLogic.Interfaces
{
    public interface IPostTreeService
    {
        Task<List<Post>> LoadMainFeed();
        Task<Post> LoadRootPost(int rootPostId);
        Task<List<Post>> LoadRootPostReplies(RepliesSortType sortType, int rootPostId, int maxDepth);
        Task<List<Post>> LoadSubPostReplies(RepliesSortType sortType, int parentId, int startDepth, int maxDepth, int offSet);
        Task<int> CreatePost(NewPost newPost);
    }
}
