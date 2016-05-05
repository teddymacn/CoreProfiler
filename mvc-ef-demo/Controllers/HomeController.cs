
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcEfSample.Models;
using CoreProfiler;
using CoreProfiler.Web;
using System.Threading;

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
                
                // WebTimingAsync() profiles the wrapped action as a web request timing
                var url = "http://localhost:5000/home/child";
                await ProfilingSession.Current.WebTimingAsync(url, async (correlationId) =>
                {
                    using (var httpClient = new HttpClient())
                    {   
                        httpClient.DefaultRequestHeaders.Add(CoreProfilerMiddleware.XCorrelationId, correlationId);            
                                 
                        var uri = new Uri(url);
                        var result = await httpClient.GetStringAsync(uri);
                    }
                });
                    
                using (ProfilingSession.Current.Step("Render View"))      
                {         
                    return View(new ArticleListViewModel { Articles = articles });
                }
            }
        }
        
        public ContentResult Child()
        {
            using (ProfilingSession.Current.Step("Handle Request - /Child"))
            {
                Thread.Sleep(200);
                return Content("test child data");
            }
        }
    }
}