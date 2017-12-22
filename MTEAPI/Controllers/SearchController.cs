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

namespace MTEAPI.Controllers
{
    [Route("[controller]/map")]
    public class SearchController : Controller
    {
        private readonly IMemoryCache _MemoryCache;
        private readonly IService<CachedDataClass.cachedData> _service;
        private readonly AppSettings _appsettings;

        public SearchController(IMemoryCache memCache, IService<CachedDataClass.cachedData> serv, IOptions<AppSettings> options_appsettings)
        {
            _MemoryCache = memCache;
            _service = serv;
            _appsettings = options_appsettings.Value;
        }


        [HttpGet]
        public JsonResult Get()
        {
            double neLat = 9999, neLng = 9999, swLat = 9999, swLng = 9999;
            int pagesize = 50;
            int page = 1;

            bool neLatOK = double.TryParse(HttpContext.Request.Query["neLat"], out neLat);
            bool neLngOK = double.TryParse(HttpContext.Request.Query["neLng"], out neLng);
            bool swLatOK = double.TryParse(HttpContext.Request.Query["swLat"], out swLat);
            bool swLngOK = double.TryParse(HttpContext.Request.Query["swLng"], out swLng);
            bool psOK = int.TryParse(HttpContext.Request.Query["pagesize"], out pagesize);
            bool pOK = int.TryParse(HttpContext.Request.Query["page"], out page);


            string trackType = HttpContext.Request.Query["trackType"].ToString().Trim();
            string difficulty = HttpContext.Request.Query["difficulty"].ToString().Trim();
            string duration = HttpContext.Request.Query["duration"].ToString().Trim();
            string amenities = HttpContext.Request.Query["amenities"].ToString().Trim();
            string activities = HttpContext.Request.Query["activities"].ToString().Trim();
            string access = HttpContext.Request.Query["access"].ToString().Trim();


            if (page < 1) page = 1;
            if (pagesize == 0) pagesize = 50;

            string fullurl = _appsettings.BaseURL + "/" + HttpContext.Request.Path;
            if (HttpContext.Request.QueryString.HasValue) fullurl += HttpContext.Request.QueryString;

            if (neLat != 9999 && neLng != 9999 && swLat != 9999 && swLng != 9999 && neLatOK && neLngOK && swLatOK && swLngOK)
            {
                Services.DataClass ad = new Services.DataClass(_MemoryCache, _service, _appsettings);
                return ad.GetFeatures(neLat, neLng, swLat, swLng, fullurl, pagesize, page, trackType, difficulty, duration, amenities, activities, access, "");
            }
            else
            {
                return new JsonResult("Error");
            }
        }
    }


    [Route("search/query")]
    public class SearchQueryController : Controller
    {
        private readonly IMemoryCache _MemoryCache;
        private readonly IService<CachedDataClass.cachedData> _service;
        private readonly AppSettings _appsettings;

        public SearchQueryController(IMemoryCache memCache, IService<CachedDataClass.cachedData> serv, IOptions<AppSettings> options_appsettings)
        {
            _MemoryCache = memCache;
            _service = serv;
            _appsettings = options_appsettings.Value;
        }


        [HttpGet]
        public JsonResult Get()
        {
            double neLat = 9999, neLng = 9999, swLat = 9999, swLng = 9999;
            int pagesize = 50;
            int page = 1;
            
            string query = HttpContext.Request.Query["query"].ToString().Trim();
            bool psOK = int.TryParse(HttpContext.Request.Query["pagesize"], out pagesize);
            bool pOK = int.TryParse(HttpContext.Request.Query["page"], out page);


            string trackType = HttpContext.Request.Query["trackType"].ToString().Trim();
            string difficulty = HttpContext.Request.Query["difficulty"].ToString().Trim();
            string duration = HttpContext.Request.Query["duration"].ToString().Trim();
            string amenities = HttpContext.Request.Query["amenities"].ToString().Trim();
            string activities = HttpContext.Request.Query["activities"].ToString().Trim();
            string access = HttpContext.Request.Query["access"].ToString().Trim();


            if (page < 1) page = 1;
            if (pagesize == 0) pagesize = 50;


            string fullurl = _appsettings.BaseURL + HttpContext.Request.Path;
            if (HttpContext.Request.QueryString.HasValue) fullurl += HttpContext.Request.QueryString;

            if (query.Length > 0)
            {
                Services.DataClass ad = new Services.DataClass(_MemoryCache, _service, _appsettings);
                return ad.GetFeatures(neLat, neLng, swLat, swLng, fullurl, pagesize, page, trackType, difficulty, duration, amenities, activities, access, query);
            }
            else
            {
                return new JsonResult("Error! No query parameter.");
            }
        }
    }
}
