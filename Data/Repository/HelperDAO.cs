using Dapper;
using Microsoft.Extensions.Configuration;
using Signa.TemplateCore.Api.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Signa.TemplateCore.Api.Data.Repository
{
    public class HelperDAO : RepositoryBase
    {
        public HelperDAO(IConfiguration configuration)
        {
            base.configuration = configuration;
        }

        public Dictionary<int, string> BuscarParametros(params int[] ids)
        {
            using (IDbConnection db = Connection)
            {
                return db.Query(@"
                    Select Tab_Parametro_Sistema_Id, Parametro
                    From   Tab_Parametro_Sistema
                    Where  Tab_Parametro_Sistema_Id In @Ids",
                    new { Ids = ids }
                    )
                    .ToDictionary(row => (int)row.Tab_Parametro_Sistema_Id, row => (string)row.Parametro);
            }
        }

        public void Gravar(LogMsgEntity log)
        {
            using (IDbConnection db = Connection)
            {
                db.Execute(
                    sql: "Insert Log_Msg (Tab_Tipo_Msg_Id, Usuario_Internet_Id, Data_Log, Msg01, Msg02, Msg03, Msg04, Msg05)" + Environment.NewLine +
                    "Values (@TabTipoMsgId, @UsuarioInternetId, @DataLog, @Msg01, @Msg02, @Msg03, @Msg04, @Msg05)",
                    param: log,
                    commandType: CommandType.Text
                    );
            }
        }
    }
}