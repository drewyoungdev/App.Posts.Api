using System.Collections.Generic;
using System.Threading.Tasks;
using PostsApi.BusinessLogic.Interfaces;
using PostsApi.Models;
using PostsApi.Repositories.Interfaces;

namespace PostsApi.BusinessLogic
{
    public class PostTreeService : IPostTreeService
    {
        private readonly IPostsRepository postsRepository;

        public PostTreeService(IPostsRepository postsRepository)
        {
            this.postsRepository = postsRepository;
        }

        // Depends on posts being ordered. This ensures lookup always contains parent before child searches for parent.
        // Foreach loop guarentees items are re-added to new tree structure in order they come from db
        public async Task<List<Post>> GetPostTree(int id, int subLevelLimit)
        {
            var flatPostTree = await this.postsRepository.GetFlatPostTree(id, subLevelLimit);

            var lookup = new Dictionary<int, Post>();
            var rootPost = new List<Post>();

            foreach (var post in flatPostTree)
            {
                lookup.Add(post.Id, post);

                if (post.Depth == 0)
                {
                    rootPost.Add(post);
                }
                else
                {
                    var parentPost = lookup[post.ParentId];
                    parentPost.Replies.Add(post);
                }
            }

            return rootPost;
        }
    }
}
