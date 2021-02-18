using System;
using System.Text;
using Arcmage.Configuration;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Arcmage.Server.Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // setup default json serializers
            JsonConvert.DefaultSettings = () => {
                var jsonSetting = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Include
                };
                jsonSetting.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
                return jsonSetting;
            };

            // setup json serializers for api controllers
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
            } );

            
            // setup Hangfire services.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Settings.Current.ArcmageConnectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true,
                    SchemaName = "hfo"
                }));

            // add the processing server as IHostedService
            services.AddHangfireServer();


            // setup http context access in the middleware pipeline 
            var httpContextAccessor = new HttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

            // setup authentication validation
            var key = Encoding.ASCII.GetBytes(Settings.Current.TokenEncryptionKey);
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            // setup API controller
            services.AddMvcCore()
                    .AddApiExplorer();

            // setup gzip compression response
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();

            // setup cors access control
            services.AddCors();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // use static files to serve files under wwwroot
            // Remark:
            // - Uncomment/Comment the next lines to host the static (wwwroot) files using Kerstel instead of IIS / IIS Express
            // - Also change the web.config to remove hosting the static (wwwroot) files form IIS / IIS Express 
            // app.UseDefaultFiles();
            // app.UseStaticFiles();

            // use url routing
            app.UseRouting();

            // use authentication            
            app.UseAuthentication();

            // use authorization
            app.UseAuthorization();

            // use compression
            app.UseResponseCompression();

            // use https redirection
            //app.UseHttpsRedirection();

            // use cors to allow other web based origins
            app.UseCors(o => {
                o.AllowAnyHeader().WithExposedHeaders(
                        "Content-Disposition",
                        "Set-Cookie",
                        ".AspNetCore.Identity.Application",
                        ".AspNetCore.Session",
                        "Date",
                        "X-ImportId")
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .SetIsOriginAllowed(x => true)
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowCredentials();

                o.WithOrigins(Settings.Current.PortalUrl, Settings.Current.GameApiUrl);
            });

            // use api endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
      
    }
}
