using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Signa.TemplateCore.Api.Data.Entities;
using Signa.TemplateCore.Api.Data.Repository;
using System;
using System.Diagnostics;
using System.Linq;

namespace Signa.TemplateCore.Api.Helpers
{
    public class DatabaseLog
    {
        private readonly IConfiguration configuration;
        private readonly HelperDAO helperDAO;

        public DatabaseLog(IConfiguration configuration, HelperDAO helperDAO)
        {
            this.configuration = configuration;
            this.helperDAO = helperDAO;
        }

        public string GravaLogMsg(string mensagemUsuario, Exception ex, HttpContext context = null)
        {
            var mensagemRetorno = mensagemUsuario;

            try
            {
                var st = new StackTrace(ex, true);
                StackFrame frame = st.GetFrame(st.FrameCount - 1);
                string msg03 = null, msg04 = null;

                if (ex is System.Data.SqlClient.SqlException)
                {
                    var sqlEx = ex as System.Data.SqlClient.SqlException;

                    if (!sqlEx.Message.StartsWith("Timeout expired"))
                        msg03 = string.Concat(sqlEx.Errors.Cast<System.Data.SqlClient.SqlError>().Select(e => $"{e.Procedure}; Linha: {e.LineNumber}; Mensagem: {e.Message} |\n"));
                }
                else
                {
                    msg03 = frame.GetMethod().DeclaringType.Name + "." + frame.GetMethod().Name;
                    msg04 = ex.StackTrace;
                }

                var log = new LogMsg
                {
                    TabTipoMsgId = 95,
                    Msg01 = mensagemUsuario,
                    Msg02 = ex.Message,
                    Msg03 = msg03,
                    Msg04 = msg04,
                    Msg05 = context.Request.Path,
                    UsuarioInternetId = Globals.UsuarioId,
                    DataLog = DateTime.Now
                };

                if (Globals.UsuarioId == 1)
                {
                    mensagemRetorno += log.Msg03 + " " + log.Msg02;
                }

                helperDAO.Gravar(log);
            }
            catch (Exception)
            {
                // TODO: estourar em console e em file
            }

            return mensagemRetorno;
        }
    }
}
