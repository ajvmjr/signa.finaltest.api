using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Signa.TemplateCore.Api.Data.Repository
{
    public abstract class RepositoryBase
    {
        protected const string _n = "\n";
        protected int _usuarioId = Globals.UserId.GetValueOrDefault();
        protected IConfiguration configuration;

        internal IDbConnection Connection
        {
            get
            {
                var conn = new SqlConnection(configuration["DATABASE_CONNECTION"] + (_usuarioId > 0 ? " U:" + _usuarioId : ""));

                conn.Open();

                conn.BeginTransaction(IsolationLevel.ReadUncommitted).Commit();

                return conn;
            }
        }
    }
}
