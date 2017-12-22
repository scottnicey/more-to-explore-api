using Microsoft.AspNetCore.Mvc;
using MTEAPI.Models;
using MTEAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;


namespace MTEAPI.Controllers
{
    [Route("[controller]")]
    public class TracksController : Controller
    {

        private readonly IMemoryCache _MemoryCache;
        private readonly IService<CachedDataClass.cachedData> _service;
        private readonly AppSettings _appsettings;

        public TracksController(IMemoryCache memCache, IService<CachedDataClass.cachedData> serv, IOptions<AppSettings> options_appsettings)
        {
            _MemoryCache = memCache;
            _service = serv;
            _appsettings = options_appsettings.Value;
        }

        // GET: api/admin
        [HttpGet]
        public JsonResult Get()
        {
            return new JsonResult("Error! Must enter a site id.");

        }


        // GET api/values/5
        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            Services.DataClass ad = new Services.DataClass(_MemoryCache, _service, _appsettings);
            return ad.GetTrack(id);
        }

    }

}
