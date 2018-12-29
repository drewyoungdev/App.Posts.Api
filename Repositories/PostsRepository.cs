using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
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
 
        public async Task<List<Post>> GetFlatPostTree(int parentId, int topLevelLimit = 10, int subLevelLimit = 10)
        {
            using (var conn = this.Connection)
            {
                conn.Open();

                Parameters param = new Parameters();
                        param.Add("@input_parent_id", parentId);
                        param.Add("@top_level_limit", topLevelLimit);
                        param.Add("@sub_level_limit", subLevelLimit);

                var results = await conn.QueryAsync<Post>("get_posts_tree_by_parent_id", param, null, null, CommandType.StoredProcedure);

                return results.ToList();
            }
        }
    }

    public class Parameters : DynamicParameters
    {
        public new void Add(string name, object value = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null)
        {
            if (dbType == null && value is string)
            {
                if (size == null)
                {
                    dbType = DbType.AnsiString;
                }
                else
                {
                    dbType = DbType.AnsiStringFixedLength;
                }
            }
            base.Add(name, value, dbType, direction, size);
        }
    }
}
