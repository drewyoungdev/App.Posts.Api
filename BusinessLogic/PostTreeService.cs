using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<Post>> LoadMainFeed()
        {
            // return top x posts (filter further by date range? e.g. last two days)
            var mainFeed = await this.postsRepository.GetMainFeed(2);

            // TODO: start task to append main post data (same functionality as LoadMainPost but for many ids)

            return mainFeed;
        }

        public async Task<Post> LoadRootPost(int rootPostId)
        {
            var rootPost = await this.postsRepository.GetRootPost(rootPostId);

            return rootPost;
        }

        public async Task<List<Post>> LoadRootPostReplies(int rootPostId)
        {
            // return top 100 comments to root-post, then 6 levels in with top 10 replies
            return await GetReplies(rootPostId, directReplyLimit: 100, depthLimit: 6, recursiveLimit: 10);
        }

        public async Task<List<Post>> LoadSubPostReplies(int parentId)
        {
            // return all replies to sub-post, then 1 level in with one additional reply
            return await GetReplies(parentId, directReplyLimit: null, depthLimit: 1, recursiveLimit: 1);
        }

        private async Task<List<Post>> GetReplies(int rootPostId, int? directReplyLimit, int depthLimit, int recursiveLimit)
        {
            var rootPost = await this.postsRepository.GetRootPost(rootPostId);

            if (rootPost == null) return null;
            
            List<Post> repliesToRootPost = new List<Post>();

            if (IsHighActivity(rootPost))
            {
                // if TOO active, then we need to begin hiding sub_replies
                repliesToRootPost = await this.postsRepository.GetRepliesHighActivity(rootPostId, directReplyLimit, depthLimit, recursiveLimit);
            }
            else
            {
                repliesToRootPost = await this.postsRepository.GetRepliesLowActivity(rootPostId, directReplyLimit, depthLimit, recursiveLimit);
            }

            var repliesTree = BuildTree(repliesToRootPost);

            return repliesTree;
        }

        // High Activity gives us the ability to determine if we confidently apply algorithm at this state
        private bool IsHighActivity(Post rootPost)
        {
            // determine based on time of post and number of replies
            return false;
        }

        // Depends on posts being ordered. This ensures lookup always contains parent before child searches for parent.
        // Foreach loop guarentees items are re-added to new tree structure in order they come from db
        private List<Post> BuildTree(List<Post> posts)
        {
            var lookup = new Dictionary<int, Post>();
            var rootPosts = new List<Post>();

            foreach (var reply in posts)
            {
                lookup.Add(reply.Id, reply);

                if (reply.Depth == 0)
                {
                    rootPosts.Add(reply);
                }
                else
                {
                    var parentPost = lookup[reply.ParentId];
                    parentPost.Replies.Add(reply);
                }
            }

            return rootPosts;
        }
    }
}
