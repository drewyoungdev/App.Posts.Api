using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PostsApi.BusinessLogic.Interfaces;
using PostsApi.Enums;
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

        public async Task<List<Post>> LoadRootPostReplies(RepliesSortType sortType, int rootPostId, int maxDepth)
        {
            // return top 100 comments to root-post, then go until max depth in with top 10 replies
            return await GetReplies(sortType, rootPostId, directReplyLimit: 100, directReplyOffset: null, startDepth: 0, depthLimit: maxDepth, recursiveLimit: 10);
        }

        public async Task<List<Post>> LoadSubPostReplies(RepliesSortType sortType, int parentId, int startDepth, int maxDepth, int offset)
        {
            if (startDepth > maxDepth)
            {
                // return error saying you cannot start past MaxDepth
                throw new ArgumentException($"Start Depth of {startDepth} must be less than the Max-Depth of {maxDepth}");
            }

            // return all replies to sub-post, off-set to start after the last comment returned. startDepth ensures we start at the same depth as we left off
            return await GetReplies(sortType, parentId, directReplyLimit: null, directReplyOffset: offset, startDepth: startDepth, depthLimit: maxDepth, recursiveLimit: 1);
        }

        private async Task<List<Post>> GetReplies(RepliesSortType sortType, int rootPostId, int? directReplyLimit, int? directReplyOffset, int startDepth, int depthLimit, int recursiveLimit)
        {
            var rootPost = await this.postsRepository.GetRootPost(rootPostId);

            if (rootPost == null) return null;
            
            List<Post> repliesToRootPost = new List<Post>();

            repliesToRootPost = await this.postsRepository.GetReplies(sortType, rootPostId, directReplyLimit, directReplyOffset, startDepth, depthLimit, recursiveLimit);

            var repliesTree = BuildTree(repliesToRootPost, startDepth, depthLimit);

            return repliesTree;
        }

        // Depends on posts being ordered. This ensures lookup always contains parent before child searches for parent.
        // Foreach loop guarentees items are re-added to new tree structure in order they come from db
        private List<Post> BuildTree(List<Post> posts, int startDepth, int depthLimit)
        {
            var lookup = new Dictionary<int, Post>();
            var rootPosts = new List<Post>();

            foreach (var reply in posts)
            {
                if (reply.Depth == depthLimit)
                {
                    reply.MustContinueInNewThread = true;
                }
                
                lookup.Add(reply.Id, reply);

                // preventative logic is place to never have startDepth >= MaxDepth. startDepth must be less than MaxDepth
                // (startDepth will always start at least one level above the MaxDepth allowing it to be a rootPost)
                if (reply.Depth == startDepth)
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
