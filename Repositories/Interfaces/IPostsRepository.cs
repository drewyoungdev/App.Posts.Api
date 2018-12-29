using System.Collections.Generic;
using System.Threading.Tasks;
using PostsApi.Models;

namespace PostsApi.Repositories.Interfaces
{
    public interface IPostsRepository
    {
         Task<List<Post>> GetFlatPostTree(int parentId, int subLevelLimit);
    }
}
