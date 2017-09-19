using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MPServer.Data;
using MPServer.Models;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MPServer
{
    public static class DataSeeder
    {
        // TODO: Move this code when seed data is implemented in EF 7

        /// <summary>
        /// This is a workaround for missing seed data functionality in EF 7.0-rc1
        /// More info: https://github.com/aspnet/EntityFramework/issues/629
        /// </summary>
        /// <param name="service">
        /// An instance that provides the mechanisms to get instance of the database context.
        /// </param>
        /// <param name="contentRootPath">Content Path</param>
        public static async Task SeedData(this IServiceProvider service, string contentRootPath)
        {
            var database = service.GetService<AppDbContext>();
            var userManager = service.GetService<UserManager<User>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            JArray userList = null;

            if (!await roleManager.RoleExistsAsync("SendHeartBeat"))
                await roleManager.CreateAsync(new IdentityRole {Name = "SendHeartBeat" });

            if (!await roleManager.RoleExistsAsync("ViewHeartBeat"))
                await roleManager.CreateAsync(new IdentityRole { Name = "ViewHeartBeat" });

            if (!await roleManager.RoleExistsAsync("SendMessage"))
                await roleManager.CreateAsync(new IdentityRole { Name = "SendMessage" });

            if (!await roleManager.RoleExistsAsync("ViewMessage"))
                await roleManager.CreateAsync(new IdentityRole { Name = "ViewMessage" });

            using (var rng = RandomNumberGenerator.Create())
            {
                var signingKey = new byte[256 / 8];
                string password;
                if (!database.Users.Any(t => t.UserName == "admin"))
                {
                    rng.GetBytes(signingKey);
                    password = Convert.ToBase64String(signingKey).Replace("+", "&").Replace("/", "#").Replace("=", "");

                    var user = new User {UserName = "admin"};
                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "ViewMessage");
                        await userManager.AddToRoleAsync(user, "ViewHeartBeat");
                        userList = new JArray {new JObject {{"username", "admin"}, {"password", password}}};
                    }
                }

                if (!database.Users.Any(t => t.UserName == "heartbeat"))
                {
                    rng.GetBytes(signingKey);
                    password = Convert.ToBase64String(signingKey).Replace("+", "&").Replace("/", "#").Replace("=", "");

                    var user = new User {UserName = "heartbeat"};
                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "SendHeartBeat");
                        if (userList == null) userList = new JArray();
                        userList.Add(new JObject { { "username", "heartbeat" }, { "password", password } });
                    }
                }

                if (!database.Users.Any(t => t.UserName == "message"))
                {
                    rng.GetBytes(signingKey);
                    password = Convert.ToBase64String(signingKey).Replace("+", "&").Replace("/", "#").Replace("=", "");

                    var user = new User { UserName = "message" };
                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "SendMessage");
                        if (userList == null) userList = new JArray();
                        userList.Add(new JObject { { "username", "message" }, { "password", password } });
                    }
                }
            }

            if (userList != null)
            {
                var userListFile = new FileInfo(Path.Combine(contentRootPath, "_default-user.json"));
                if (userListFile.Exists) userListFile.Delete();
                using (var userListFileWriter = userListFile.CreateText())
                {
                    using (var jsonWriter = new JsonTextWriter(userListFileWriter))
                    {
                        userList.WriteTo(jsonWriter);
                    }
                }
            }
        }
    }
}
