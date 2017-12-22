using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MTEAPI.Models;
using MTEAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options; 
using System.Net.Http;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MTEAPI.Controllers
{
    [Route("[controller]")]
    public class RelicsController : Controller
    {

        private readonly IMemoryCache _MemoryCache;
        private readonly IService<CachedDataClass.cachedData> _service;
        private readonly AppSettings _appsettings;

        public RelicsController(IMemoryCache memCache, IService<CachedDataClass.cachedData> serv, IOptions<AppSettings> options_appsettings)
        {
            _MemoryCache = memCache;
            _service = serv;
            _appsettings = options_appsettings.Value; 
        }


        // GET: api/values
        [HttpGet]
        public JsonResult Get()
        {
            return new JsonResult("Error! Must enter a relic id.");
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            Services.DataClass ad = new Services.DataClass(_MemoryCache, _service, _appsettings);
            return ad.GetRelic(id);
        }
    }
}
