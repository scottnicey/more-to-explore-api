using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MTEAPI.Models;
using MTEAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MTEAPI.Controllers
{
    [Route("[controller]")]
    
    public class AdminController : Controller
    {

        private readonly IMemoryCache _MemoryCache;
        private readonly IService<CachedDataClass.cachedData> _service;
        private readonly AppSettings _appsettings;
        //1
        public AdminController(IMemoryCache memCache, IService<CachedDataClass.cachedData> serv, IOptions<AppSettings> options_appsettings)
        {
            _MemoryCache = memCache;
            _service = serv;
            _appsettings = options_appsettings.Value;
        }

        // GET: api/admin
        [HttpGet]
        public JsonResult Get()
        {
            Services.DataClass ad = new Services.DataClass(_MemoryCache, _service, _appsettings);
            return ad.GetStats(); 
        }

        [HttpGet("{function}")]
        public JsonResult Get(string function)
        {
            if (function == "refreshcache")
            {
                Services.DataClass ad = new Services.DataClass(_MemoryCache, _service, _appsettings);
                return ad.RefreshCache();
            }
            else
            {
                Services.DataClass ad = new Services.DataClass(_MemoryCache, _service, _appsettings);
                return ad.GetStats();
            }

        }



        /*
        // GET api/refreshcache
        [HttpGet]
        public JsonResult RefreshCache()
        {
            Services.DataClass ad = new Services.DataClass(_MemoryCache, _service);
            return ad.RefreshCache();
        }
        */


        /*
        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        */
    }
}
