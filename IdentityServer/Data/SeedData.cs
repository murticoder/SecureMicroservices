using IdentityModel;
using IdentityServer.Models;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer.Data
{
    public class SeedData
    {
        public static void InitializeDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                if (!context.Clients.Any())
                {
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }
                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.IdentityResources)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiScopes.Any())
                {
                    foreach (var resource in Config.ApiScopes)
                    {
                        context.ApiScopes.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }

            string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;database=IdentityServer4.Quickstart.EntityFramework-4.0.0;trusted_connection=yes;";
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(options =>
               options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<AppDbContext>();
                    context.Database.Migrate();

                    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var murtaza = userMgr.FindByNameAsync("murtaza").Result;
                    if (murtaza == null)
                    {
                        murtaza = new ApplicationUser
                        {
                            UserName = "murtaza",
                            Email = "murtazasafarpour@email.com",
                            EmailConfirmed = true,
                        };
                        var result = userMgr.CreateAsync(murtaza, "Pass123$").Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        result = userMgr.AddClaimsAsync(murtaza, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "murtaza safarpour"),
                            new Claim(JwtClaimTypes.GivenName, "Murtaza"),
                            new Claim(JwtClaimTypes.FamilyName, "Safarpour"),
                            new Claim(JwtClaimTypes.WebSite, "http://murtaza.com"),
                            new Claim(JwtClaimTypes.Role, "admin"),
                        }).Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }
                        Log.Debug("murtaza created");
                    }
                    else
                    {
                        Log.Debug("murtaza already exists");
                    }

                    var kadir = userMgr.FindByNameAsync("kadir").Result;
                    if (kadir == null)
                    {
                        kadir = new ApplicationUser
                        {
                            UserName = "kadir",
                            Email = "kadirteke@email.com",
                            EmailConfirmed = true
                        };
                        var result = userMgr.CreateAsync(kadir, "Pass123@").Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        result = userMgr.AddClaimsAsync(kadir, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "kadir Teke"),
                            new Claim(JwtClaimTypes.GivenName, "kadir"),
                            new Claim(JwtClaimTypes.FamilyName, "teke"),
                            new Claim(JwtClaimTypes.WebSite, "http://kadir.com"),
                            new Claim("location", "somewhere"),
                            new Claim(JwtClaimTypes.Role, "user"),
                        }).Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }
                        Log.Debug("kadir created");
                    }
                    else
                    {
                        Log.Debug("kadir already exists");
                    }
                }
            }
        }
    }
}
