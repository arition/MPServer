using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("appsettings.Runtime.json", optional: true)
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
            services.AddDbContext<AppDbContext>(t => t.UseSqlite(connectionString));

            services.AddIdentity<User, IdentityRole>(t => t.Cookies.ApplicationCookie.AutomaticChallenge = false)
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

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
                var runtimeConfiguration = new JObject {{"SigningKey", new JValue(signingKeyString)}};
                var runtimeConfigurationFile = new FileInfo(Path.Combine(ContentRootPath, "appsettings.Runtime.json"));
                if (runtimeConfigurationFile.Exists) runtimeConfigurationFile.Delete();
                using (var runtimeConfigurationFileWriter = runtimeConfigurationFile.CreateText())
                {
                    using (var jsonWriter = new JsonTextWriter(runtimeConfigurationFileWriter))
                    {
                        runtimeConfiguration.WriteTo(jsonWriter);
                    }
                }
            }

            // Register the OpenIddict services, including the default Entity Framework stores.
            services.AddOpenIddict<User, AppDbContext>()
                // Enable the token endpoint (required to use the password flow).
                .EnableTokenEndpoint("/api/account/token")
                // Allow client applications to use the grant_type=password flow.
                .AllowPasswordFlow()
                .AllowRefreshTokenFlow()
                // During development, you can disable the HTTPS requirement.
                .DisableHttpsRequirement()
                .UseJsonWebTokens()
                .AddSigningKey(new SymmetricSecurityKey(SigningKeyProtector.UnprotectKey(Configuration["SigningKey"])));

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.SeedData(ContentRootPath).Wait();
            app.UseStaticFiles();
            app.UseIdentity();
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(SigningKeyProtector.UnprotectKey(Configuration["SigningKey"])),
                    ValidateIssuer = false,
                    ValidIssuer = "",
                    ValidateAudience = false,
                    ValidAudience = "",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }
            });
            app.UseOpenIddict();
            app.UseMvc();
        }
    }
}
