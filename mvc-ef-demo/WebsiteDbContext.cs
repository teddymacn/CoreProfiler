
using Microsoft.EntityFrameworkCore;
using mvc_ef_demo.Models;

namespace mvc_ef_demo
{
    public class WebsiteDbContext : DbContext
    {
        public WebsiteDbContext(DbContextOptions<WebsiteDbContext> options)
            : base(options)
        {

        }

        public DbSet<Article> Articles { get; set; }
    }
}