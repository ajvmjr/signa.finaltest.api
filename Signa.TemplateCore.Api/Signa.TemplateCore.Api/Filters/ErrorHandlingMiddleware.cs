using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Signa.TemplateCore.Api.Data.Repository;
using Signa.TemplateCore.Api.Helpers;
using Signa.Library.Exceptions;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Signa.TemplateCore.Api.Data.Filters
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private static IExceptionHandling _exceptionHandling;

        public IConfiguration Configuration { get; }

        public ErrorHandlingMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.next = next;
            this.Configuration = configuration;
        }

        public async Task Invoke(HttpContext context, HelperDAO helperDAO, DatabaseLog databaseLog /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, helperDAO, databaseLog);
            }
        }

        //https://stackoverflow.com/questions/29664/how-to-catch-sqlserver-timeout-exceptions
        private enum SQLError
        {
            //Timeout = -2,
            NetworkError = 11,
            OutOfMemory = 701,
            LockIssue = 1204,
            DeadLock = 1205,
            LockTimeout = 1222
            //All = (NetworkError | OutOfMemory | LockIssue | DeadLock | LockTimeout)
        }

        class ErrorHandlingModel
        {
            public int ErrorCode { get; set; }
            public object ErrorObject { get; set; }

            public ErrorHandlingModel()
            {
                ErrorCode = (int)HttpStatusCode.InternalServerError;
                ErrorObject = null;
            }

            public ErrorHandlingModel(int errorCode, object ErrorObject)
            {
                ErrorCode = errorCode;
                this.ErrorObject = ErrorObject;
            }
        }

        interface IExceptionHandling
        {
            ErrorHandlingModel TratarErro(Exception ex);
        }

        class SignaRegraNegocioHandling : IExceptionHandling
        {
            public ErrorHandlingModel TratarErro(Exception ex)
            {
                var exception = ex as SignaRegraNegocioException;

                if (exception == null)
                {
                    return new ErrorHandlingModel();
                }

                return new ErrorHandlingModel(
                    (int)HttpStatusCode.BadRequest,
                    new { ex.Message }
                );
            }
        }

        class SignaSqlNotFoundHandling : IExceptionHandling
        {
            public ErrorHandlingModel TratarErro(Exception ex)
            {
                var exception = ex as SignaSqlNotFoundException;

                if (exception == null)
                {
                    return new ErrorHandlingModel();
                }

                return new ErrorHandlingModel(
                    (int)HttpStatusCode.NotFound,
                    new { ex.Message }
                );
            }
        }

        class SqlHandling : IExceptionHandling
        {
            private DatabaseLog _databaseLog;
            private HttpContext _context;

            public SqlHandling(DatabaseLog databaseLog, HttpContext context)
            {
                _databaseLog = databaseLog;
                _context = context;
            }

            public ErrorHandlingModel TratarErro(Exception ex)
            {
                string mensagemUsuario;
                var sqlEx = ex as SqlException;

                if (Enum.IsDefined(typeof(SQLError), sqlEx.Number) || (sqlEx.Number == -2 && Regex.IsMatch(ex.Message, @"[tT]ime\-{0,1}out")))
                {
                    mensagemUsuario = "Tivemos uma instabilidade temporária.\nTente novamente e, se não funcionar, informe ao nosso suporte";
                }
                else if (sqlEx.Number >= 50000)//Mensagem de usuário do banco (eg. triggers)
                {
                    mensagemUsuario = sqlEx.Errors[0].Message;
                }
                else if (sqlEx.Number == 4060)//Conexão com a base
                {
                    mensagemUsuario = $"Não foi possível conectar ao server {sqlEx.Server}; {sqlEx.Message}";
                }
                else
                {
                    mensagemUsuario = "Problemas na nossa base de dados. Informe o suporte.";
                }

                _databaseLog.GravaLogMsg(mensagemUsuario, ex, _context);

                return new ErrorHandlingModel(
                    (int)HttpStatusCode.InternalServerError,
                    new { Message = mensagemUsuario, stack = ex.ToString() }
                );
            }
        }

        class GenericHandling : IExceptionHandling
        {
            private HelperDAO _helperDAO;

            public GenericHandling(HelperDAO helperDAO)
            {
                _helperDAO = helperDAO;
            }

            public ErrorHandlingModel TratarErro(Exception ex)
            {
                var frame = new System.Diagnostics.StackTrace(ex, true).GetFrame(1);

                var result = JsonConvert.SerializeObject(new
                {
                    Message = "Problemas no sistema. Entre em contato com o administrador do sistema.",
                    Error = new { Text = ex.Message, Method = (frame.GetMethod().DeclaringType == null ? "" : frame.GetMethod().DeclaringType.Name) + "." + frame.GetMethod().Name, Linha = frame.GetFileLineNumber(), Coluna = frame.GetFileColumnNumber() }
                });

                _helperDAO.Gravar(new Data.Entities.LogMsg
                {
                    UsuarioInternetId = Globals.UsuarioId,
                    DataLog = DateTime.Now,
                    Msg01 = result
                });

                return new ErrorHandlingModel((int)HttpStatusCode.BadRequest, result);
            }
        }

        private static IExceptionHandling VerificarTipoExcecao(Exception ex, HelperDAO helperDAO, DatabaseLog databaseLog, HttpContext context)
        {
            if (ex is SignaRegraNegocioException)
            {
                return new SignaRegraNegocioHandling();
            }

            if (ex is SignaSqlNotFoundException)
            {
                return new SignaSqlNotFoundHandling();
            }

            if (ex is SqlException)
            {
                return new SqlHandling(databaseLog, context);
            }

            return new GenericHandling(helperDAO);
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, HelperDAO helperDAO, DatabaseLog databaseLog)
        {
            try
            {
                _exceptionHandling = VerificarTipoExcecao(exception, helperDAO, databaseLog, context);

                var errorHandling = _exceptionHandling.TratarErro(exception);

                context.Response.StatusCode = errorHandling.ErrorCode;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(errorHandling.ErrorObject));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}