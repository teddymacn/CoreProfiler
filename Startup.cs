using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Threading;
using EF.Diagnostics.Profiling.Data;
using EF.Diagnostics.Profiling.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EF.Diagnostics.Profiling
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {            
            services.AddEntityFramework()
                    .AddEntityFrameworkSqlite()
                    .AddDbContext<WebsiteDbContext>(
                        options => options.UseSqlite(GetConnection()));
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseCoreProfiler();
            
            app.Run(async context =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.ContentType = "text/html";
                    
                    using (ProfilingSession.Current.Step("Handle Request: /"))
                    {
                        Thread.Sleep(200);
                        
                        await context.Response.WriteAsync("List Countries via ADO.NET:");
                        
                        using (ProfilingSession.Current.Step("Load countries from DB via ADO.NET"))
                        {
                            Thread.Sleep(100);
                            
                            using (var conn = GetConnection())
                            using (var cmd = conn.CreateCommand())
                            {
                                await conn.OpenAsync();

                                cmd.CommandText = "select * from country";
                                using (var rdr = await cmd.ExecuteReaderAsync())
                                {
                                    while (await rdr.ReadAsync())
                                    {
                                        await context.Response.WriteAsync("<br />" + rdr.GetString(0));
                                    }
                                }
                            }
                        }
                        
                        await context.Response.WriteAsync("<br />List Countries via EntityFramework Core:");
                        
                        using (ProfilingSession.Current.Step("Load countries from DB via EntityFramework Core"))
                        {
                            Thread.Sleep(100);
                            
                            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                            {
                                var db = serviceScope.ServiceProvider.GetService<WebsiteDbContext>();
                                foreach (var country in db.Countries)
                                {
                                    await context.Response.WriteAsync("<br />" + country.Name);
                                }
                            }
                        }
                    }
                }
            });
        }
        
        private static DbConnection GetConnection()
        {
            return new ProfiledDbConnection(new SqliteConnection(@"Data Source=../../../demo.sqlite;"), new DbProfiler(ProfilingSession.Current.Profiler));
        }
    }
    
    [Table("country")]
    public class Country
    {
        [Key]
        public string Name {get; set;}
    }
    
    public class WebsiteDbContext : DbContext
    {
        public WebsiteDbContext(DbContextOptions<WebsiteDbContext> options)
            : base(options)
        {

        }

        public DbSet<Country> Countries { get; set; }
    }
}