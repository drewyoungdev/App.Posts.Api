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
 
        public async Task<List<Post>> GetFlatPostTree(int id, int subLevelLimit)
        {
            using (var conn = this.Connection)
            {
                conn.Open();

                PostgresqlParameters param = new PostgresqlParameters();
                        param.Add("@input_id", id);
                        param.Add("@sub_level_limit", subLevelLimit);

                var results = await conn.QueryAsync<Post>("get_posts_tree_by_parent_id", param, null, null, CommandType.StoredProcedure);

                return results.ToList();
            }
        }
    }
}
