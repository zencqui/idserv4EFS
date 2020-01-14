// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Data;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // configures IIS out-of-proc settings (see https://github.com/aspnet/AspNetCore/issues/14882)
            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            // configures IIS in-proc settings
            services.Configure<IISServerOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            
            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                //.AddInMemoryIdentityResources(Config.Ids)
                //.AddInMemoryApiResources(Config.Apis)
                //.AddInMemoryClients(Config.Clients)
                .AddInMemoryIdentityResources(Configuration.GetSection("IdentityResources"))
                .AddInMemoryApiResources(Configuration.GetSection("ApiResources"))
                .AddInMemoryClients(Configuration.GetSection("Clients"))
                .AddAspNetIdentity<ApplicationUser>();

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    // register your IdentityServer with Google at https://console.developers.google.com
                    // enable the Google+ API
                    // set the redirect URI to http://localhost:5000/signin-google
                    options.ClientId = "copy client ID from Google here";
                    options.ClientSecret = "copy client secret from Google here";
                })
                .AddOpenIdConnect("aad", "Sign-in with Azure AD", options =>
                {
                    options.Authority = "https://login.windows.net/applicationid"; //directory client id make configurable
                    options.ClientId = "client id";  //application client id make configurable

                    //options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    //options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                    options.ResponseType = "id_token";
                    options.CallbackPath = "/signin-oidc";
                    //options.SignedOutCallbackPath = "/signout-callback-aad";
                    //options.RemoteSignOutPath = "/signout-aad";

                    options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = false };

                    //options.TokenValidationParameters = new TokenValidationParameters
                    //{
                    //    //ValidateIssuer = false,
                    //    //ValidAudience = "165b99fd-195f-4d93-a111-3e679246e6a9",

                    //    NameClaimType = "name",
                    //    RoleClaimType = "role"
                    //};
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}