
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcEfSample.Models;

namespace MvcEfSample.Controllers
{
    public class HomeController : Controller
    {
        private WebsiteDbContext dbContext;
        public HomeController(WebsiteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var articles = await dbContext.Articles.OrderByDescending(a => a.Id)
                .Take(10)
                .ToArrayAsync();
                
            return View(new ArticleListViewModel { Articles = articles });
        }
    }
}