using Dapper;
using Microsoft.Extensions.Configuration;
using Signa.TemplateCore.Api.Data.Entities;

namespace Signa.TemplateCore.Api.Data.Repository
{
    public class HelperDAO : RepositoryBase
    {
        public HelperDAO(IConfiguration configuration)
        {
            base.configuration = configuration;
        }

        // TODO: incluir em Signa.Library
        public void Gravar(LogMsg log)
        {
            using (var db = Connection)
            {
                db.Execute(
                    sql: @"
                    Insert Log_Msg (Tab_Tipo_Msg_Id, Usuario_Internet_Id, Data_Log, Msg01, Msg02, Msg03, Msg04, Msg05)
                    Values (@TabTipoMsgId, @UsuarioInternetId, @DataLog, @Msg01, @Msg02, @Msg03, @Msg04, @Msg05)",
                    param: log
                );
            }
        }
    }
}