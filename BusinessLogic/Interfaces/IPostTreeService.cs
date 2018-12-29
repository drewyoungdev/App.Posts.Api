using System.Collections.Generic;
using System.Threading.Tasks;
using PostsApi.Models;

namespace PostsApi.BusinessLogic.Interfaces
{
    public interface IPostTreeService
    {
         Task<List<Post>> GetPostTree(int parentId, int subLevelLimit);
    }
}
