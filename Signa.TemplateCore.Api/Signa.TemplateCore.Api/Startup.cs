using AutoMapper;
using Dapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Signa.TemplateCore.Api.Data.Filters;
using Signa.TemplateCore.Api.Data.Repository;
using Signa.TemplateCore.Api.Filters;
using Signa.TemplateCore.Api.Helpers;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Signa.TemplateCore.Api.Filters.ValidateModel;
using Signa.TemplateCore.Api.Domain.Models;
using Signa.TemplateCore.Api.Domain.Entities;
using static Signa.TemplateCore.Api.Data.Filters.ErrorHandlingMiddleware;
using Serilog;
using Signa.Library.Core.Extensions;
using Signa.Library.Core.Exceptions;
using Signa.Library.Core;
using Signa.TemplateCore.Api.Business;
using Signa.Library.Core.Helpers;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(new Action<IMapperConfigurationExpression>(c =>
            {
            }), typeof(Startup));

            services.AddMvc(options =>
                {
                    options.Filters.Add(typeof(ValidateModelAttribute));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Formatting = Formatting.Indented;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Converters = new List<JsonConverter> { new ConfigurationsHelper.DecimalConverter() };
                }).AddFluentValidation();

            #region :: Validators ::
            services.AddTransient<IValidator<PessoaModel>, PessoaValidator>();
            #endregion

            #region :: Swagger ::
            //Necessário para a documentação do Swagger
            services.AddMvcCore().AddApiExplorer();

            services.AddResponseCompression();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "Signa consultoria e Sistemas",
                        Version = "v1",
                        Description = "API Template Signa",
                        Contact = new Contact
                        {
                            Name = "Signa",
                            Url = "http://signainfo.com.br"
                        }
                    });

                options.AddSecurityDefinition(
                    "bearer",
                    new ApiKeyScheme
                    {
                        In = "header",
                        Description = "Autenticação baseada em Json Web Token (JWT)",
                        Name = "Authorization",
                        Type = "apiKey"
                    });

                var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
                var applicationName = PlatformServices.Default.Application.ApplicationName;
                var xmlDocumentPath = Path.Combine(applicationBasePath, $"{applicationName}.xml");

                if (File.Exists(xmlDocumentPath))
                {
                    options.IncludeXmlComments(xmlDocumentPath);
                }

                options.OperationFilter<FormFileSwaggerFilter>();
            });
            #endregion

            #region :: Acesso a Dados / Dapper ::
            services.AddTransient<HelperDAO>();
            services.AddTransient<DatabaseLog>();
            services.AddTransient<LogDatabaseDAO>();
            services.AddTransient<PessoaDAO>();

            DefaultTypeMap.MatchNamesWithUnderscores = true;
            Dapper.SqlMapper.AddTypeMap(typeof(string), System.Data.DbType.AnsiString);
            #endregion

            #region :: Business ::
            services.AddTransient<PessoaBL>();
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

            #region :: AppSettings ::
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();

            if (appSettings.FuncaoId.IsZeroOrNull())
            {
                throw new SignaRegraNegocioException("Necessário incluir o id da função");
            }

            if (appSettings.NomeApi.IsNullEmptyOrWhiteSpace())
            {
                throw new SignaRegraNegocioException("Necessário incluir o nome da api");
            }

            Global.FuncaoId = appSettings.FuncaoId;
            Global.NomeProjeto = appSettings.NomeApi;
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

            #region :: Other classes ::
            services.AddTransient<SignaRegraNegocioExceptionHandling>();
            services.AddTransient<SignaSqlNotFoundExceptionHandling>();
            services.AddTransient<SqlExceptionHandling>();
            services.AddTransient<GenericExceptionHandling>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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
                c.SwaggerEndpoint("./v1/swagger.json", "Template de API .NET Core Signa");
            });

            app.UseHttpsRedirection();
            app.UseResponseCompression();
            app.UseAuthentication();

            loggerFactory.AddSerilog();

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            #region :: Middleware Claims from JWT ::
            // DOC: https://www.wellingtonjhn.com/posts/obtendo-o-usu%C3%A1rio-logado-em-apis-asp.net-core/
            app.Use(async delegate (HttpContext httpContext, Func<Task> next)
            {
                if (httpContext.User.Claims.Any())
                {
                    Globals.UsuarioId = int.Parse(httpContext.User.Claims.Where(c => c.Type == "UserId").FirstOrDefault().Value);
                    Globals.GrupoUsuarioId = int.Parse(httpContext.User.Claims.Where(c => c.Type == "UserGroupId")?.FirstOrDefault().Value);
                }

                Global.ConnectionString = Configuration["DATABASE_CONNECTION"];

                await next.Invoke();
            });
            #endregion

            app.UseCors(config =>
            {
                config.AllowAnyHeader();
                config.AllowAnyMethod();
                config.AllowAnyOrigin();
            });

            app.UseMvc();
        }
    }
}
