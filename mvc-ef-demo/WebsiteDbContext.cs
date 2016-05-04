
using Microsoft.EntityFrameworkCore;
using MvcEfSample.Models;

namespace MvcEfSample
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