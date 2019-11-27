using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Signa.TemplateCore.Api.Data.Repository;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Signa.TemplateCore.Api.Data.Filters
{
    // TODO: incluir em Signa.Library.Api
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly LogDatabaseDAO _logDatabase;

        public RequestResponseLoggingMiddleware(RequestDelegate next,
                                                ILoggerFactory loggerFactory,
                                                IConfiguration configuration,
                                                LogDatabaseDAO logDatabase)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<RequestResponseLoggingMiddleware>();
            _configuration = configuration;
            _logDatabase = logDatabase;
        }

        public async Task Invoke(HttpContext context)
        {
            var xmlIn = new XmlDocument();

            try
            {
                xmlIn.InnerXml = await FormatRequest(context.Request);
            }
            catch (Exception)
            {
            }

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                try
                {
                    var xmlOut = await FormatResponse(context.Response);

                    _logDatabase.Insert(new Data.Entities.LogExecucao
                    {
                        DataExecucao = DateTime.Now,
                        FuncaoId = 80966,
                        ParametroXmlIn = xmlIn,
                        ParametroXmlOut = JsonConvert.DeserializeXmlNode(xmlOut, "Root"),
                        TabTipoMsgId = 3
                    });
                }
                catch (Exception) { }
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            var body = request.Body;
            request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body = body;

            try
            {
                bodyAsText = JsonConvert.DeserializeXmlNode(bodyAsText, "Body").InnerXml;
            }
            catch (Exception)
            {
                bodyAsText = $"<body>{bodyAsText}</body>";
            }

            return $"<xml><scheme>{request.Scheme}</scheme><host>{request.Host}</host><path>{request.Path}</path><queryString>{request.QueryString}</queryString>{bodyAsText}</xml>";
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = response.ContentLength > 0 ? await new StreamReader(response.Body).ReadToEndAsync() : "null";
            response.Body.Seek(0, SeekOrigin.Begin);

            return $"{{'Response': {text}}}";
        }
    }

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder) => builder.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
