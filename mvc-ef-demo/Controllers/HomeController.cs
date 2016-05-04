
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcEfSample.Models;
using CoreProfiler;

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
            using (ProfilingSession.Current.Step("Handle Request - /"))
            {
                Article[] articles;
                
                using (ProfilingSession.Current.Step(() => "Load Data"))
                {
                    articles = await dbContext.Articles.OrderByDescending(a => a.Id)
                        .Take(10)
                        .ToArrayAsync();
                }
                    
                using (ProfilingSession.Current.Step("Render View"))      
                {         
                    return View(new ArticleListViewModel { Articles = articles });
                }
            }
        }
    }
}