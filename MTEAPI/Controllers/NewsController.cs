using Microsoft.AspNetCore.Mvc;
using MTEAPI.Models;
using MTEAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MTEAPI.Data;


namespace MTEAPI.Controllers
{
    [Route("[controller]")]
    public class NewsController : Controller
    {
        ApplicationDbContext _context;
        AppSettings _appsettings;

        public NewsController(ApplicationDbContext context, IOptions<AppSettings> options_appsettings)
        {
            _context = context;
            _appsettings = options_appsettings.Value; 
        }

        // GET: /news
        [HttpGet]
        public JsonResult Get()
        {
            return GetNews(-1); 
        }

        // GET /news/5
        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            Services.NewsService serv = new Services.NewsService(_context,_appsettings);
            return GetNews(id);
        }

        private JsonResult GetNews(int getid)
        {
            Services.NewsService serv = new Services.NewsService(_context,_appsettings);

            int pagesize = 20;
            int page = 1;

            string ps = HttpContext.Request.Query["pagesize"].ToString().Trim();
            string p = HttpContext.Request.Query["page"].ToString().Trim();


            bool psOK = int.TryParse(ps, out pagesize);
            bool pOK = int.TryParse(p, out page);

            if (page < 1) page = -1;
            if (page == 0) page = 1;
            if (pagesize <= 0) pagesize = 20;

            return serv.GetNews(page, pagesize, getid);
        }


        [HttpDelete]
        public JsonResult Delete()
        {
            return new JsonResult("ERROR: must pass in id.");
        }

        [HttpDelete("{id}")]
        public JsonResult Delete(int id)
        {
            return new JsonResult(id.ToString() + " will be deleted when the code is completed.");
        }

        [HttpPost]
        public JsonResult Create()
        {
            return new JsonResult("This will be create a new news item in the database.");
        }

    }

}
