using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PostsApi.BusinessLogic.Interfaces;
using PostsApi.Models;
using PostsApi.Repositories.Interfaces;

namespace PostsApi.BusinessLogic
{
    // or should i do the main post in a separate db call then swith procedure to handle one variation by parent_id and one variation by just id
    // TODO: need better way to limit per depth.
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

        public async Task<List<Post>> LoadMainPost(int id)
        {
            // return top x comments for main post
            var flatPostTree = await this.postsRepository.GetFlatPostTree(id, 200);

            // extract main post (no parent id)
            var mainPost = flatPostTree.Where(x => x.ParentId == 0).FirstOrDefault();

            if (mainPost == null) return null;

            // remove main post from post tree building logic (ensure order is not messed up)
            var repliesToMainPost = flatPostTree.Where(x => x.Id != mainPost.Id).ToList();

            // TODO: start task to append main post data (link_url (images), subreddit, subreddit info, up/down percentage)

            // start task to build tree. main post was depth 0 and replies start at 1.
            var repliesTree = BuildTree(repliesToMainPost, 1);

            // combine data sets
            var postTree = new List<Post>(repliesTree.Count + 1);
            postTree.Add(mainPost);
            postTree.AddRange(repliesTree);

            return postTree;
        }

        public async Task<List<Post>> LoadReplies(int id)
        {
            var flatPostTree = await this.postsRepository.GetFlatPostTree(id, 10);

            return BuildTree(flatPostTree, 0);
        }

        // Depends on posts being ordered. This ensures lookup always contains parent before child searches for parent.
        // Foreach loop guarentees items are re-added to new tree structure in order they come from db
        private List<Post> BuildTree(List<Post> posts, int baseDepth)
        {
            var lookup = new Dictionary<int, Post>();
            var rootPosts = new List<Post>();

            foreach (var reply in posts)
            {
                lookup.Add(reply.Id, reply);

                if (reply.Depth == baseDepth)
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
