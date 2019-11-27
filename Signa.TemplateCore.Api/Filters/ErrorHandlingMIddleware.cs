using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Signa.TemplateCore.Api.Data.Repository;
using Signa.TemplateCore.Api.Helpers;
using Signa.Library.Core.Exceptions;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Signa.Library.Core;

namespace Signa.TemplateCore.Api.Data.Filters
{
    // TODO: incluir em Signa.Library.Api
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private static IExceptionHandling _exceptionHandling;
        private readonly SignaRegraNegocioExceptionHandling _signaRegraNegocioHandling;
        private readonly SignaSqlNotFoundExceptionHandling _signaSqlNotFoundHandling;
        private readonly SqlExceptionHandling _sqlHandling;
        private readonly GenericExceptionHandling _genericHandling;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            SignaRegraNegocioExceptionHandling signaRegraNegocioHandling,
            SignaSqlNotFoundExceptionHandling signaSqlNotFoundHandling,
            SqlExceptionHandling sqlHandling,
            GenericExceptionHandling genericHandling)
        {
            this.next = next;
            _signaRegraNegocioHandling = signaRegraNegocioHandling;
            _signaSqlNotFoundHandling = signaSqlNotFoundHandling;
            _sqlHandling = sqlHandling;
            _genericHandling = genericHandling;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        // DOC: https://stackoverflow.com/questions/29664/how-to-catch-sqlserver-timeout-exceptions
        private enum SQLError
        {
            NetworkError = 11,
            OutOfMemory = 701,
            LockIssue = 1204,
            DeadLock = 1205,
            LockTimeout = 1222
        }

        public class ErrorHandlingModel
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

        public class SignaRegraNegocioExceptionHandling : IExceptionHandling
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

        public class SignaSqlNotFoundExceptionHandling : IExceptionHandling
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

        public class SqlExceptionHandling : IExceptionHandling
        {
            private readonly DatabaseLog _databaseLog;
            private readonly ILogger<SqlExceptionHandling> _logger;

            public SqlExceptionHandling(
                DatabaseLog databaseLog,
                ILogger<SqlExceptionHandling> logger)
            {
                _databaseLog = databaseLog;
                _logger = logger;
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

                _logger.LogError(ex, mensagemUsuario);

                _databaseLog.GravaLogMsg(mensagemUsuario, ex);

                return new ErrorHandlingModel(
                    (int)HttpStatusCode.InternalServerError,
                    new { Message = mensagemUsuario, stack = ex.ToString() }
                );
            }
        }

        public class GenericExceptionHandling : IExceptionHandling
        {
            private readonly HelperDAO _helperDAO;
            private readonly ILogger<GenericExceptionHandling> _logger;

            public GenericExceptionHandling(
                HelperDAO helperDAO,
                ILogger<GenericExceptionHandling> logger)
            {
                _helperDAO = helperDAO;
                _logger = logger;
            }

            public ErrorHandlingModel TratarErro(Exception ex)
            {
                var frame = new System.Diagnostics.StackTrace(ex, true).GetFrame(1);

                var result = JsonConvert.SerializeObject(new
                {
                    Message = "Problemas no sistema. Entre em contato com o administrador do sistema.",
                    Error = new { Text = ex.Message, Method = (frame.GetMethod().DeclaringType == null ? "" : frame.GetMethod().DeclaringType.Name) + "." + frame.GetMethod().Name, Linha = frame.GetFileLineNumber(), Coluna = frame.GetFileColumnNumber() }
                });

                _logger.LogError(ex, ex.Message);

                try
                {
                    _helperDAO.Gravar(new Data.Entities.LogMsg
                    {
                        UsuarioInternetId = Global.UsuarioId,
                        DataLog = DateTime.Now,
                        Msg01 = result
                    });
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }

                return new ErrorHandlingModel((int)HttpStatusCode.BadRequest, result);
            }
        }

        private IExceptionHandling VerificarTipoExcecao(Exception ex)
        {
            if (ex is SignaRegraNegocioException)
            {
                return _signaRegraNegocioHandling;
            }

            if (ex is SignaSqlNotFoundException)
            {
                return _signaSqlNotFoundHandling;
            }

            if (ex is SqlException)
            {
                return _sqlHandling;
            }

            return _genericHandling;
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            try
            {
                _exceptionHandling = VerificarTipoExcecao(exception);

                var errorHandling = _exceptionHandling.TratarErro(exception);

                context.Response.StatusCode = errorHandling.ErrorCode;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(errorHandling.ErrorObject));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}