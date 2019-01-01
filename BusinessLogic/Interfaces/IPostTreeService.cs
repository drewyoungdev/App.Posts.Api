using System.Collections.Generic;
using System.Threading.Tasks;
using PostsApi.Models;

namespace PostsApi.BusinessLogic.Interfaces
{
    public interface IPostTreeService
    {
        Task<List<Post>> LoadMainFeed();
        Task<Post> LoadRootPost(int rootPostId);
        Task<List<Post>> LoadRootPostReplies(int rootPostId);
        Task<List<Post>> LoadSubPostReplies(int parentId);
    }
}
