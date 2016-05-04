using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MvcEfSample.Models;
using System.Data.Common;
using CoreProfiler.Data;
using Microsoft.Data.Sqlite;
using CoreProfiler;
using CoreProfiler.Web;

namespace MvcEfSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFramework()
                    .AddEntityFrameworkSqlite()
                    .AddDbContext<WebsiteDbContext>(
                        options => options.UseSqlite(GetConnection()));
                        
            services.AddMvc();
        }
        
        private static DbConnection GetConnection()
        {
            return new ProfiledDbConnection(new SqliteConnection("Data Source=./mvcefsample.sqlite"), () =>
            {
                if (ProfilingSession.Current == null)
                    return null;

                return new DbProfiler(ProfilingSession.Current.Profiler);
            });
        }        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);
            
            app.UseStaticFiles();
            app.UseCoreProfiler();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            InitializeDatabase(app);
        }
        
        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<WebsiteDbContext>();

                if (db.Database.EnsureCreated())
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var article = new Article {
                            Title = string.Format("Article {0}",  i + 1),
                            Content = string.Format("Article {0} content blabla blabla",  i + 1),
                            CreatedTime = DateTime.Now,
                            UpdatedTime = DateTime.Now
                        };
                        
                        db.Articles.Add(article);
                    }
                    db.SaveChanges();
                }
            }
        }
    }
}