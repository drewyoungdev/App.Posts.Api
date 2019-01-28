using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PostsApi.Enums;
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

        public async Task<List<Post>> GetReplies(RepliesSortType sortType, int parentId, int? directReplyLimit, int? directReplyOffset, int startDepth, int depthLimit, int recursiveLimit)
        {
            using (var conn = this.Connection)
            {
                conn.Open();

                PostgresqlParameters param = new PostgresqlParameters();
                param.Add("@input_parent_id", parentId);
                param.Add("@direct_reply_limit", directReplyLimit); // null will retrieve all
                param.Add("@direct_reply_offset", directReplyOffset); // null will offset none
                param.Add("@start_depth", startDepth);
                param.Add("@depth_limit", depthLimit);
                param.Add("@recursive_limit", recursiveLimit);

                IEnumerable<Post> results;
                switch (sortType)
                {
                    case RepliesSortType.Top:
                        results =  await conn.QueryAsync<Post>("get_replies_top", param, null, null, CommandType.StoredProcedure);
                        break;
                    default:
                        results = await conn.QueryAsync<Post>("get_replies_best", param, null, null, CommandType.StoredProcedure);
                        break;
                }

                return results.ToList();
            }
        }
    
        public async Task<int> CreatePost(Post post)
        {
            using (var conn = this.Connection)
            {
                // To return entire object
                // string sQuery = @"INSERT INTO posts(parent_id, body, author, upvotes, downvotes, create_date) " +
                //                 "VALUES (@ParentId, @Body, @Author, @Upvotes, @Downvotes, @CreateDate) " +
                //                 "RETURNING id, parent_id as ParentId, body, author, create_date as CreateDate";
                // conn.Open();

                // return await conn.QuerySingleAsync<Post>(sQuery, post);

                string sQuery = @"INSERT INTO posts(parent_id, body, author, upvotes, downvotes, create_date) " +
                                "VALUES (@ParentId, @Body, @Author, @Upvotes, @Downvotes, @CreateDate) " +
                                "RETURNING id";   
                conn.Open();

                return await conn.ExecuteScalarAsync<int>(sQuery, post);
            }
        }
    }
}
