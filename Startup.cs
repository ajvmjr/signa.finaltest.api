using System;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static Signa.TemplateCore.Api.Filters.ValidateModelFilter;
using Newtonsoft.Json;
using System.Collections.Generic;
using FluentValidation.AspNetCore;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Signa.TemplateCore.Api.Filters;
using Dapper;
using Microsoft.Extensions.Configuration;
using Signa.TemplateCore.Api.Helpers;
using Signa.TemplateCore.Api.Security;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using Signa.TemplateCore.Api.Domain.Entities;
using Signa.TemplateCore.Api.Domain.Models;
using Signa.TemplateCore.Api.Data.Repository;
using Signa.Library.Extensions;
using Signa.Library.Exceptions;
using Signa.Library;
using Microsoft.OpenApi.Models;
using FluentValidation;

namespace Signa.TemplateCore.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .AddFluentValidation();

            services.AddAutoMapper(new Action<IMapperConfigurationExpression>(c =>
            {
            }), typeof(Startup));

            #region :: Validators ::
            services.AddTransient<IValidator<PessoaModel>, PessoaValidator>();
            #endregion

            #region :: Swagger ::
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Signa consultoria e Sistemas",
                        Version = "v1",
                        Description = "API Login Ecargo",
                        Contact = new OpenApiContact
                        {
                            Name = "Signa",
                            Url = new Uri("http://signainfo.com.br")
                        }
                    });

                options.AddSecurityDefinition(
                    "bearer",
                    new OpenApiSecurityScheme
                    {
                        // In = "header",
                        Description = "Autenticação baseada em Json Web Token (JWT)",
                        Name = "Authorization",
                        // Type = "apiKey"
                    });

                var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
                var applicationName = PlatformServices.Default.Application.ApplicationName;
                var xmlDocumentPath = Path.Combine(applicationBasePath, $"{applicationName}.xml");

                if (File.Exists(xmlDocumentPath))
                {
                    options.IncludeXmlComments(xmlDocumentPath);
                }
            });
            #endregion

            #region :: Acesso a Dados / Dapper ::
            services.AddTransient<HelperDAO>();
            services.AddTransient<LogDatabaseDAO>();
            services.AddTransient<DatabaseLog>();
            services.AddTransient<PessoaDAO>();

            DefaultTypeMap.MatchNamesWithUnderscores = true;
            #endregion

            #region :: AppSettings ::
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();

            if (appSettings.FunctionId.IsZeroOrNull())
            {
                throw new SignaRegraNegocioException("Necessário incluir o id da função");
            }

            if (appSettings.ApiName.IsNullEmptyOrWhiteSpace())
            {
                throw new SignaRegraNegocioException("Necessário incluir o nome da api");
            }

            Global.FuncaoId = appSettings.FunctionId;
            Global.NomeApi = appSettings.ApiName;
            #endregion

            #region :: JWT / Token / Auth ::
            var signingConfigurations = new SigningConfigurations(appSettings.Secret);
            services.AddSingleton(signingConfigurations);

            var tokenConfigurations = new TokenConfigurations();

            new ConfigureFromConfigurationOptions<TokenConfigurations>(
                Configuration.GetSection("TokenConfigurations"))
                    .Configure(tokenConfigurations);

            services.AddSingleton(tokenConfigurations);

            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(bearerOptions =>
            {
                bearerOptions.SaveToken = true;

                var paramsValidation = bearerOptions.TokenValidationParameters;

                paramsValidation.IssuerSigningKey = signingConfigurations.Key;

                // Valida a assinatura de um token recebido
                paramsValidation.ValidateIssuerSigningKey = true;
                paramsValidation.ValidateIssuer = false;
                paramsValidation.ValidateAudience = false;

                // Verifica se um token recebido ainda é válido
                paramsValidation.ValidateLifetime = true;

                // Tempo de tolerância para a expiração de um token (utilizado
                // caso haja problemas de sincronismo de horário entre diferentes
                // computadores envolvidos no processo de comunicação)
                paramsValidation.ClockSkew = TimeSpan.Zero;
            });

            // Ativa o uso do token como forma de autorizar o acesso
            // a recursos deste projeto
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });
            #endregion

            #region :: AutoMapper ::
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PessoaEntity, PessoaModel>()
                    .ForMember(d => d.CnpjCpf, s => s.MapFrom(x => x.IndicativoPfPj == "PF" ? x.PfCpf : x.PjCnpj))
                    .ForMember(d => d.DataNascimentoFormatada, s => s.MapFrom(x => x.DataNascimento.ToString("dd/MM/yyyy HH:mm")))
                    .ReverseMap();
            });

            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);
            #endregion

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                app.UseHsts();
            }

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "docs/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "docs";
                c.SwaggerEndpoint("./v1/swagger.json", "API Login Ecargo");
            });

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors(config =>
            {
                config.AllowAnyHeader();
                config.AllowAnyMethod();
                config.AllowAnyOrigin();
            });

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            #region :: Middleware Claims from JWT ::
            //https://www.wellingtonjhn.com/posts/obtendo-o-usu%C3%A1rio-logado-em-apis-asp.net-core/
            // TODO: não está pegando o Claims do token, verificar - talvez criar middleware disso
            app.Use(async delegate (HttpContext httpContext, Func<Task> next)
            {
                if (httpContext.User.Claims.Any())
                {
                    AuthenticatedUser.UserId = int.Parse(httpContext.User.Claims.Where(c => c.Type == "UserId").FirstOrDefault().Value);
                    AuthenticatedUser.UserGroupId = int.Parse(httpContext.User.Claims.Where(c => c.Type == "UserGroupId")?.FirstOrDefault().Value);
                }

                // TODO: valorizar connection string, usuarioId e grupo de usuario a partir do headers - criar middleare disso

                await next.Invoke();
            });
            #endregion

            app.UseAuthentication();
            app.UseAuthorization();

            // TODO: verificar para incluir compressão
            // app.UseResponseCompression();

            app.UseEndpoints(endpoints =>
            {
                // TODO: verificar para abrir no swagger direto
                // TODO: verificar endpoints para ter certeza que estão funcionando - testar no máximo de servidores que puder
                endpoints.MapControllers();
            });
        }
    }
}
