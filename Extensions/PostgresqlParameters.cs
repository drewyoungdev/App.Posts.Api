using System.Data;
using Dapper;

namespace PostsApi.Extensions
{
    public class PostgresqlParameters : DynamicParameters
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
