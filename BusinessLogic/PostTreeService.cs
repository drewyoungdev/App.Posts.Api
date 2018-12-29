using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PostsApi.BusinessLogic.Interfaces;
using PostsApi.Models;
using PostsApi.Repositories.Interfaces;

namespace PostsApi.BusinessLogic
{
    // (need better way to limit per depth)
    public class PostTreeService : IPostTreeService
    {
        private readonly IPostsRepository postsRepository;

        public PostTreeService(IPostsRepository postsRepository)
        {
            this.postsRepository = postsRepository;
        }

        public Task<List<Post>> LoadMainFeed()
        {
            // update postgres fn to accept "NULL" as input and set subLevelLimit = 0
            // or have a separate fn for main feed?
            throw new System.NotImplementedException();
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

            // start task to build tree
            var repliesTree = BuildTree(repliesToMainPost, 1);

            // combine data sets
            var postTree = new List<Post>(repliesTree.Count + 1);
            postTree.Add(mainPost);
            postTree.AddRange(repliesTree);

            return postTree;
        }

        public async Task<List<Post>> LoadMoreReplies(int id)
        {
            // return top x comments for reply
            var flatPostTree = await this.postsRepository.GetFlatPostTree(id, 10);

            // build tree of replies
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
