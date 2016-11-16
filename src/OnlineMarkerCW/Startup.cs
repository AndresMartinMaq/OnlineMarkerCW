using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OnlineMarkerCW.Data;
using OnlineMarkerCW.Models;
using OnlineMarkerCW.Services;
using OnlineMarkerCW.Interfaces;


namespace OnlineMarkerCW
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            //add the custom created db services
            services.AddScoped<IDbServices, DbServices>();


            //add entity framework and sqlserverrerigester  the model context as a service, connect it to the the sql server
            // Add framework services.

          //UPDATE - OnlineMarkerCWContext are not necessary any more, as one context for all models have been merged into ApplicationDbContext
          // services.AddDbContext<OnlineMarkerCWContext>(options =>
          //     options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

          // services.AddDbContext<ApplicationDbContext>(options =>
          //   options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

          //uncomment the previous two lines and comment these two for using MSSQL server rather then local sqlite DB

          //UPDATE - OnlineMarkerCWContext are not necessary any more, as one context for all models have been merged into ApplicationDbContext
          //services.AddDbContext<OnlineMarkerCWContext>(options =>
          //    options.UseSqlite(Configuration.GetConnectionString("SQLliteConnection")));

            services.AddDbContext<ApplicationDbContext>(options =>
                  options.UseSqlite(Configuration.GetConnectionString("SQLliteConnection")));


          //add support for the indentiy service which provides authenicaiton
          services.AddIdentity<ApplicationUser, ApplicationUserRole>()
              .AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();

            //indentiy service options
            services.Configure<IdentityOptions>(options =>
              {
                  // Password settings
                  options.Password.RequireDigit = true;
                  options.Password.RequiredLength = 8;
                  options.Password.RequireNonAlphanumeric = false;
                  options.Password.RequireUppercase = true;
                  options.Password.RequireLowercase = false;

                  // Lockout settings
                  options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                  options.Lockout.MaxFailedAccessAttempts = 10;

                  // Cookie settings
                  options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(150);
                  options.Cookies.ApplicationCookie.LoginPath = "/Account/Login";
                  options.Cookies.ApplicationCookie.LogoutPath = "/Account/Logout";

                  // User settings
                  options.User.RequireUniqueEmail = true;
              });

              //configure session
              services.AddMemoryCache();
              services.AddSession(options => {
                      options.IdleTimeout = TimeSpan.FromDays(1);
                      options.CookieName = ".app_session";
              });
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ApplicationDbContext context)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

          //cusstom error code message
          app.UseStatusCodePagesWithRedirects("/Error_Message/{0}");
            //init the db prepopulation
            //DbInitializer.Initialize(context);
        }
    }
}
