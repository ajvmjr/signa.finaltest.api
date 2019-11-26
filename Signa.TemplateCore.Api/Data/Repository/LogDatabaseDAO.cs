using Dapper;
using Microsoft.Extensions.Configuration;
using Signa.TemplateCore.Api.Data.Entities;

namespace Signa.TemplateCore.Api.Data.Repository
{
    public class LogDatabaseDAO : RepositoryBase
    {
        public LogDatabaseDAO(IConfiguration configuration)
        {
            base.configuration = configuration;
        }

        public void Insert(LogExecucao log)
        {
            using (var db = Connection)
            {
                db.Execute(
                    sql: @"
                    Insert Log_Execucao (Data_Execucao, Data_Fim_Execucao, Observacao, Usuario_Id, Funcao_Id, Mensagem, Mensagem2, Mensagem3, Mensagem4, Parametro_Xml_In, Parametro_Xml_Out, Tab_Tipo_Msg_Id, Mensagem_Tecnica) 
                    Values (@DataExecucao, @DataFimExecucao, @Observacao, @UsuarioId, @FuncaoId, @Mensagem, @Mensagem2, @Mensagem3, @Mensagem4, @ParametroXmlIn, @ParametroXmlOut, @TabTipoMsgId, @MensagemTecnica)",
                    param: log
                );
            }
        }
    }
}
