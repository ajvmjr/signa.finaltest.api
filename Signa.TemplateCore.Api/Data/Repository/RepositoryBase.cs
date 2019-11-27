using Microsoft.Extensions.Configuration;
using Signa.Library.Core;
using System.Data;
using System.Data.SqlClient;

namespace Signa.TemplateCore.Api.Data.Repository
{
    // TODO: usar de Signa.Library
    public abstract class RepositoryBase
    {
        protected int _usuarioId = Global.UsuarioId;
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
