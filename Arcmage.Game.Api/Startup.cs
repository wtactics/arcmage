using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Arcmage.Configuration;
using Arcmage.Game.Api.GameRuntime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Arcmage.Game.Api
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
            });

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

            // setup API controllers
            services.AddMvcCore()
                    .AddApiExplorer();

            // setup game runtime
            services.AddSingleton<IGameRepository, GameRepository>();

            // setup gzip compression response
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();

            // setup cors access control
            services.AddCors();

            // setup signalR and payload json serialization
            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.IgnoreNullValues = true;
                options.PayloadSerializerOptions.Converters
                    .Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            }); ;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

                o.WithOrigins(Settings.Current.PortalUrl, Settings.Current.GameApiUrl, "http://localhost:4200");
            });

            // use api endpoints and signalR endpoint
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapHub<GamesHub>("/signalr/games", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                });
            });
        }
    }
}
