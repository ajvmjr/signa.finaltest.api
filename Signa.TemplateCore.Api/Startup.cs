using System;
using System.Collections.Generic;
using System.IO;
using AutoMapper;
using Dapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Signa.Library.Core;
using Signa.Library.Core.Exceptions;
using Signa.Library.Core.Extensions;
using Signa.TemplateCore.Api.Helpers;
using Swashbuckle.AspNetCore.Swagger;
using static Signa.TemplateCore.Api.Filters.ValidateModel;

namespace Signa.TemplateCore.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private string applicationBasePath { get; }
        private string applicationName { get; }


        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            applicationBasePath = env.ContentRootPath;
            applicationName = env.ApplicationName;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(new Action<IMapperConfigurationExpression>(c =>
            {
            }), typeof(Startup));

            services.AddMvc(options =>
                {
                    options.Filters.Add(typeof(ValidateModelAttribute));
                })
                .AddFluentValidation();

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Formatting = Formatting.Indented;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Converters = new List<JsonConverter> { new ConfigurationsHelper.DecimalConverter() };
                });

            #region :: Validators ::
            // services.AddTransient<IValidator<PessoaModel>, PessoaValidator>();
            #endregion

            #region :: Swagger ::
            //Necessário para a documentação do Swagger
            services.AddMvcCore().AddApiExplorer();

            services.AddResponseCompression();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Signa consultoria e Sistemas",
                        Version = "v1",
                        Description = "API Template Signa",
                        Contact = new OpenApiContact
                        {
                            Name = "Signa",
                            Url = new Uri("http://signainfo.com.br")
                        }
                    });

                // options.AddSecurityDefinition(
                //     "Bearer",
                //     new OpenApiSecurityScheme
                //     {
                //         In = ParameterLocation.Header,
                //         Description = "Autenticação baseada em Json Web Token (JWT)",
                //         Name = "Authorization",
                //         Type = SecuritySchemeType.ApiKey
                //     });

                var xmlDocumentPath = Path.Combine(applicationBasePath, $"{applicationName}.xml");

                if (File.Exists(xmlDocumentPath))
                {
                    options.IncludeXmlComments(xmlDocumentPath);
                }

                // options.OperationFilter<FormFileSwaggerFilter>();
            });
            #endregion

            #region :: Acesso a Dados / Dapper ::
            // services.AddTransient<HelperDAO>();
            // services.AddTransient<DatabaseLog>();
            // services.AddTransient<LogDatabaseDAO>();
            // services.AddTransient<PessoaDAO>();

            DefaultTypeMap.MatchNamesWithUnderscores = true;
            Dapper.SqlMapper.AddTypeMap(typeof(string), System.Data.DbType.AnsiString);
            #endregion

            #region :: Business ::
            // services.AddTransient<PessoaBL>();
            #endregion

            #region :: Other classes ::
            // services.AddTransient<SignaRegraNegocioExceptionHandling>();
            // services.AddTransient<SignaSqlNotFoundExceptionHandling>();
            // services.AddTransient<SqlExceptionHandling>();
            // services.AddTransient<GenericExceptionHandling>();
            #endregion

            #region :: AutoMapper ::
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                // cfg.CreateMap<PessoaEntity, PessoaModel>()
                //     .ForMember(d => d.CnpjCpf, s => s.MapFrom(x => x.IndicativoPfPj == "PF" ? x.PfCpf : x.PjCnpj))
                //     .ForMember(d => d.DataNascimentoFormatada, s => s.MapFrom(x => x.DataNascimento.ToString("dd/MM/yyyy HH:mm")))
                //     .ReverseMap();
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
            // var signingConfigurations = new SigningConfigurations(appSettings.Secret);
            // services.AddSingleton(signingConfigurations);

            // var tokenConfigurations = new TokenConfigurations();

            // new ConfigureFromConfigurationOptions<TokenConfigurations>(
            //     Configuration.GetSection("TokenConfigurations"))
            //         .Configure(tokenConfigurations);

            // services.AddSingleton(tokenConfigurations);

            // services.AddAuthentication(authOptions =>
            // {
            //     authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //     authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            // }).AddJwtBearer(bearerOptions =>
            // {
            //     bearerOptions.SaveToken = true;

            //     var paramsValidation = bearerOptions.TokenValidationParameters;

            //     paramsValidation.IssuerSigningKey = signingConfigurations.Key;

            //     // Valida a assinatura de um token recebido
            //     paramsValidation.ValidateIssuerSigningKey = true;
            //     paramsValidation.ValidateIssuer = false;
            //     paramsValidation.ValidateAudience = false;

            //     // Verifica se um token recebido ainda é válido
            //     paramsValidation.ValidateLifetime = true;

            //     // Tempo de tolerância para a expiração de um token (utilizado
            //     // caso haja problemas de sincronismo de horário entre diferentes
            //     // computadores envolvidos no processo de comunicação)
            //     paramsValidation.ClockSkew = TimeSpan.Zero;
            // });

            // // Ativa o uso do token como forma de autorizar o acesso
            // // a recursos deste projeto
            // services.AddAuthorization(auth =>
            // {
            //     auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
            //         .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
            //         .RequireAuthenticatedUser().Build());
            // });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = string.Empty;
                c.SwaggerEndpoint("./v1/swagger.json", "Template de API .NET Core Signa");
            });

            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseResponseCompression();
            app.UseAuthentication();

            app.UseCors(config =>
            {
                config.AllowAnyHeader();
                config.AllowAnyMethod();
                config.AllowAnyOrigin();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // TODO: add serilog
            // TODO: add middleware claims
        }
    }
}
