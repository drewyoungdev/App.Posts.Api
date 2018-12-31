using System.Collections.Generic;
using System.Threading.Tasks;
using PostsApi.Models;

namespace PostsApi.BusinessLogic.Interfaces
{
    public interface IPostTreeService
    {
        Task<List<Post>> LoadMainFeed();
        Task<List<Post>> LoadRootPostWithReplies(int rootPostId);
        Task<List<Post>> LoadReplies(int parentId);
    }
}
