using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PostsApi.Extensions;
using PostsApi.Models;
using PostsApi.Repositories.Interfaces;

namespace PostsApi.Repositories
{
    public class PostsRepository : IPostsRepository
    {
        private string connectionString;

        public PostsRepository(IConfiguration configuration)
        {
            this.connectionString = configuration.GetValue<string>("ConnectionStrings:Post_Db");
        }

        internal IDbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(connectionString);
            }
        }
 
        public async Task<List<Post>> GetMainFeed(int limit)
        {
            using (var conn = this.Connection)
            {
                string sQuery = "SELECT *, (SELECT COUNT(p.parent_id) FROM posts p WHERE p.parent_id = mp.id) AS num_of_replies " +
                                "FROM posts mp WHERE mp.parent_id IS NULL " + 
                                "ORDER BY mp.score DESC, mp.id LIMIT @limit";
                
                conn.Open();
                
                var results = await conn.QueryAsync<Post>(sQuery, new { limit = limit });

                return results.ToList();
            }
        }

        public async Task<Post> GetRootPost(int rootPostId)
        {
            using (var conn = this.Connection)
            {
                string sQuery = "SELECT *, (SELECT COUNT(p.parent_id) FROM posts p WHERE p.parent_id = root.id) AS num_of_replies " +
                                "FROM posts root WHERE root.id = @rootPostId";
                
                conn.Open();
                
                var result = await conn.QuerySingleAsync<Post>(sQuery, new { rootPostId = rootPostId });

                return result;
            }
        }

        // Method to get replies for post using low-activity algorithm
        // While root post is in a state of low-activity, we give posts more of a chance to make it to the top
        // Sorts replies based on create_date and upvotes only initially
        public async Task<List<Post>> GetRepliesLowActivity(int parentId, int? directReplyLimit, int depthLimit, int recursiveLimit)
        {
            using (var conn = this.Connection)
            {
                conn.Open();

                PostgresqlParameters param = new PostgresqlParameters();
                param.Add("@input_parent_id", parentId);
                param.Add("@direct_reply_limit", directReplyLimit); // null will retrieve all
                param.Add("@depth_limit", depthLimit);
                param.Add("@recursive_limit", recursiveLimit);

                var results = await conn.QueryAsync<Post>("get_replies_low_activity", param, null, null, CommandType.StoredProcedure);

                return results.ToList();
            }
        }

        // Method to get replies for post using high-activity algorithm
        // Once root post reaches a state of high-activity, then confidence interval check begins.
        // Sorts replies based on highest upvote confidence interval then non-voted replies.
        // Negative comments are sorted after non-voted replies and in ascending order by highest downvote confidence interval.
        public async Task<List<Post>> GetRepliesHighActivity(int parentId, int? directReplyLimit, int depthLimit, int recursiveLimit)
        {
            using (var conn = this.Connection)
            {
                conn.Open();

                PostgresqlParameters param = new PostgresqlParameters();
                param.Add("@input_parent_id", parentId);
                param.Add("@direct_reply_limit", directReplyLimit); // null will retrieve all
                param.Add("@depth_limit", depthLimit);
                param.Add("@recursive_limit", recursiveLimit);

                var results = await conn.QueryAsync<Post>("get_replies_high_activity", param, null, null, CommandType.StoredProcedure);

                return results.ToList();
            }
        }
    }
}
