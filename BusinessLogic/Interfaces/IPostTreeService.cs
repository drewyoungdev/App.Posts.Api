using System.Collections.Generic;
using System.Threading.Tasks;
using PostsApi.Models;

namespace PostsApi.BusinessLogic.Interfaces
{
    public interface IPostTreeService
    {
        Task<List<Post>> LoadMainFeed();
        Task<List<Post>> LoadMainPost(int id);
        Task<List<Post>> LoadMoreReplies(int id);
    }
}
