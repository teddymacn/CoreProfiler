using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mvc_ef_demo.Models;
using CoreProfiler;
using CoreProfiler.Web;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Threading;

namespace mvc_ef_demo.Controllers
{
    public class HomeController : Controller
    {
        private WebsiteDbContext dbContext;
        public HomeController(WebsiteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ActionResult> Index()
        {
            ProfilingSession.Current.AddTag("index");

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
                var url = this.Request.Scheme + "://" + this.Request.Host + "/home/child";
                await ProfilingSession.Current.WebTimingAsync(url, async (correlationId) =>
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add(CoreProfilerMiddleware.XCorrelationId, correlationId);

                        var uri = new Uri(url);
                        var result = await httpClient.GetStringAsync(uri);
                    }
                });

                // test profiling execute command asyncrhonously
                await dbContext.Database.ExecuteSqlCommandAsync("select * from Articles");

                var task1 = dbContext.FindAsync<Article>(100);
                var task2 = dbContext.FindAsync<Article>(101);

                await task1;
                await task2;

                using (ProfilingSession.Current.Step("Render View"))
                {
                    return View();
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

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
