using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Cryptography;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MPServer.Data;
using MPServer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MPServer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            ContentRootPath = env.ContentRootPath;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddJsonFile("appsettings.Runtime.json", true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        private string ContentRootPath { get; }

        private SigningKeyProtector SigningKeyProtector { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
            var serviceProvider = services.BuildServiceProvider();
            SigningKeyProtector = ActivatorUtilities.CreateInstance<SigningKeyProtector>(serviceProvider);

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(t =>
            {
                t.UseSqlite(connectionString);
                t.UseOpenIddict();
            });

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            // Create signing key if not exists
            if (Configuration["SigningKey"] == null)
            {
                var signingKey = new byte[2048 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(signingKey);
                }
                var signingKeyString = SigningKeyProtector.ProtectKey(signingKey);
                Configuration["SigningKey"] = signingKeyString;
                var runtimeConfiguration = new JObject { { "SigningKey", new JValue(signingKeyString) } };
                var runtimeConfigurationFile = new FileInfo(Path.Combine(ContentRootPath, "appsettings.Runtime.json"));
                if (runtimeConfigurationFile.Exists) runtimeConfigurationFile.Delete();
                using (var runtimeConfigurationFileWriter = runtimeConfigurationFile.CreateText())
                {
                    runtimeConfigurationFileWriter.Write(runtimeConfiguration.ToString(Formatting.Indented));
                }
            }

            // Register the OpenIddict services, including the default Entity Framework stores.
            services.AddOpenIddict(t =>
            {
                t.AddEntityFrameworkCoreStores<AppDbContext>();
                t.AddMvcBinders();
                // Enable the token endpoint (required to use the password flow).
                t.EnableTokenEndpoint("/connect/token");
                // Allow client applications to use the grant_type=password flow.
                t.AllowPasswordFlow();
                t.AllowRefreshTokenFlow();
                // During development, you can disable the HTTPS requirement.
                t.DisableHttpsRequirement();
                t.UseJsonWebTokens();
                t.AddSigningKey(
                    new SymmetricSecurityKey(SigningKeyProtector.UnprotectKey(Configuration["SigningKey"])));
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            services.AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    //options.Authority = "http://localhost:58795/";
                    //options.Audience = "resource_server";
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(SigningKeyProtector.UnprotectKey(Configuration["SigningKey"])),
                        ValidateIssuer = false,
                        ValidIssuer = "",
                        ValidateAudience = false,
                        ValidAudience = "",
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = OpenIdConnectConstants.Claims.Subject,
                        RoleClaimType = OpenIdConnectConstants.Claims.Role
                    };
                });

            

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();

            try
            {
                serviceProvider.SeedData(ContentRootPath).Wait();
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger(GetType());
                logger.LogError(ex, "An error occurred seeding the DB.");
            }
        }
    }
}