using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nelibur.ObjectMapper;
using OpenGameListWebApp.Classes;
using OpenGameListWebApp.Data;
using OpenGameListWebApp.Data.Items;
using OpenGameListWebApp.Data.Users;
using OpenGameListWebApp.ViewModels;

namespace OpenGameListWebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add a reference to the Configuration object for DI
            services.AddSingleton<IConfiguration>(c => { return Configuration; });

            // Add framework services.
            services.AddMvc();

            // Add EntityFramework's Identity support.
            services.AddEntityFramework();

            // Add Identity Services & Stores
            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
                config.Password.RequireNonAlphanumeric = false;
                config.Cookies.ApplicationCookie.AutomaticChallenge = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Add ApplicationDbContext.
            services.AddDbContext<DbContext>(options =>
            {
                options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]);
                //options.UseOpenIddict();
            });

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));


            services.AddDbContext<DbContext>(options =>
            {
                // Configure the context to use an in-memory store.
                options.UseInMemoryDatabase(nameof(DbContext));
                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });
            services.AddOpenIddict(options =>
            {
                // Register the Entity Framework stores.
                options.AddEntityFrameworkCoreStores<DbContext>();
                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                options.AddMvcBinders();
                // Enable the token endpoint.
                options.EnableTokenEndpoint("/connect/token");
                // Enable the password flow.
                options.AllowPasswordFlow();
                // During development, you can disable the HTTPS requirement.
                options.DisableHttpsRequirement();
            });
            // Register the validation handler, that is used to decrypt the tokens.
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = OAuthValidationDefaults.AuthenticationScheme;
            })
            .AddOAuthValidation();



            // Register the OpenIddict services, including the default Entity Framework stores.
            services.AddOpenIddict<ApplicationUser, ApplicationDbContext>()

            // Integrate with EFCore
            .AddEntityFramework<ApplicationDbContext>()
            // Use Json Web Tokens (JWT)
            .UseJsonWebTokens()
            // Set a custom token endpoint (default is /connect/token)
            .EnableTokenEndpoint(Configuration["Authentication:OpenIddict:TokenEndPoint"])
            // Set a custom auth endpoint (default is /connect/authorize)
            .EnableAuthorizationEndpoint("/api/connect/authorize")
            // Allow client applications to use the grant_type=password flow.
            .AllowPasswordFlow()
            // Enable support for both authorization & implicit flows
            .AllowAuthorizationCodeFlow()
            .AllowImplicitFlow()
            // Allow the client to refresh tokens.
            .AllowRefreshTokenFlow()
            // Disable the HTTPS requirement (not recommended in production)
            .DisableHttpsRequirement()
            // Register a new ephemeral key for development.
            // We will register a X.509 certificate in production.
            .AddEphemeralSigningKey();


            // Add ApplicationDbContext's DbSeeder
            services.AddSingleton<DbSeeder>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, DbSeeder dbSeeder)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Configure a rewrite rule to auto-lookup for standard default files such as index.html.
            app.UseDefaultFiles();

            // Serve static files (html, css, js, images & more). See also the following URL:
            // https://docs.asp.net/en/latest/fundamentals/static-files.html for further reference.
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    // Disable caching for all static files.
                    context.Context.Response.Headers["Cache-Control"] =
                    Configuration["StaticFiles:Headers:Cache-Control"];
                    context.Context.Response.Headers["Pragma"] =
                    Configuration["StaticFiles:Headers:Pragma"];
                    context.Context.Response.Headers["Expires"] =
                    Configuration["StaticFiles:Headers:Expires"];
                }
            });

            // Add a custom Jwt Provider to generate Tokens
            app.UseJwtProvider();

            // Add the Jwt Bearer Header Authentication to validate Tokens
            app.UseJwtBearerAuthentication(new JwtBearerOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                RequireHttpsMetadata = false,
                TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = JwtProvider.SecurityKey,
                    ValidIssuer = JwtProvider.Issuer,
                    ValidateIssuer = false,
                    ValidateAudience = false
                },
            });

            // Add MVC to the pipeline
            app.UseMvc();


            // TinyMapper Binding Configuration
            TinyMapper.Bind<Item, ItemViewModel>();

            // Seed the Database (if needed)
            try
            {
                dbSeeder.SeedAsync().Wait();
            }
            catch (AggregateException e)
            {
                throw new Exception(e.ToString());
            }
        }
    }
}
