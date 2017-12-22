using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MTEAPI.Models;
using MTEAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using Microsoft.Extensions.Options;


namespace MTEAPI.Services
{
    public class DataClass
    {
        private readonly IMemoryCache _MemoryCache;
        private readonly AppSettings _appsettings;

        private readonly IService<CachedDataClass.cachedData> _service;
        //1
        public DataClass(IMemoryCache memCache, IService<CachedDataClass.cachedData> serv, AppSettings appsettings)
        {
            _MemoryCache = memCache;
            _service = serv;
            _appsettings = appsettings;
        }



        private CachedDataClass.cachedData SetGetMemoryCache()
        {
            //2
            string key = "MyMemoryKey-Cache";
            CachedDataClass.cachedData cachedData;

            if (!_MemoryCache.TryGetValue(key, out cachedData))
            {
                cachedData = _service.Get(_appsettings);
                cachedData.dataFrom = "New Data";
                _MemoryCache.Set(key, cachedData, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
            }
            else
            {
                cachedData = _MemoryCache.Get(key) as CachedDataClass.cachedData;
                cachedData.dataFrom = "Cache"; 
            }
            return cachedData;
        }

        private CachedDataClass.cachedData RefreshMemoryCache()
        {
            
            string key = "MyMemoryKey-Cache";
            CachedDataClass.cachedData cachedData;

            cachedData = _service.Get();
            cachedData.dataFrom = "Forced Refresh";
     
            _MemoryCache.Set(key, cachedData, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));

            return cachedData;
        }



        class stats
        {
            public int Tracks { get; set; }
            public int Sites { get; set; }
            public int Assets { get; set; }
            public int Huts { get; set; }
            public int Relics { get; set; }
            public int Carparks { get; set; }
            public string dataFrom { get; set; }
        }


        public JsonResult GetStats()
        {
            var cd = SetGetMemoryCache();

            stats st = new Services.DataClass.stats();

            st.Tracks = cd.track.features.Count;
            st.Sites = cd.site.features.Count;
            st.Assets = cd.asset.features.Count;
            st.Huts = cd.hut.features.Count;
            st.Relics = cd.relic.features.Count;
            st.Carparks = cd.carpark.features.Count;
            st.dataFrom = cd.dataFrom;
             
            JsonResult result = new JsonResult(st);
            return result;

        }
        public JsonResult RefreshCache()
        {
            var cd = RefreshMemoryCache();

            stats st = new Services.DataClass.stats();

            st.Tracks = cd.track.features.Count;
            st.Sites = cd.site.features.Count;
            st.Assets = cd.asset.features.Count;
            st.Huts = cd.hut.features.Count;
            st.Relics = cd.relic.features.Count;
            st.Carparks = cd.carpark.features.Count;
            st.dataFrom = cd.dataFrom;

            JsonResult result = new JsonResult(st);
            return result;

        }



        //GET TRACKS
        public JsonResult GetFeatures(double neLat, double neLng, double swLat, double swLng, string fullurl, int pagesize, int page, string trackType, string difficulty, string duration, string amenities, string activities, string access, string query)
        {
            try
            {
                var cd = SetGetMemoryCache();
                PreviewItems p = new PreviewItems();

                //Filter items
                bool filtered = false;
                bool thisfilteredok = false;
                bool trackTypeFilter = false;
                bool difficultyFilter = false;
                bool durationFilter = false;
                bool amenitiesFilter = false;
                bool activitiesFilter = false;
                bool accessFilter = false;


                if (trackType != "" && trackType != "all")
                {
                    filtered = true;
                    trackTypeFilter = true;
                }
                if (difficulty != "" && difficulty != "all")
                {
                    filtered = true;
                    difficultyFilter = true;
                }
                if (duration != "" && duration != "all")
                {
                    filtered = true;
                    durationFilter = true;
                }
                if (amenities  != "" && amenities != "all")
                {
                    filtered = true;
                    amenitiesFilter = true;
                }
                if (activities != "" && activities != "all")
                {
                    filtered = true;
                    activitiesFilter = true;
                }
                if (access != "" && access != "all")
                {
                    filtered = true;
                    accessFilter = true;
                }

                foreach (CachedDataClass.Track.Feature f in cd.track.features)
                {
                    bool found = false;

                    if (query.Length > 0)
                    {
                 
                        if(ncstr(f.properties.NAME).ToLower().Contains(query.ToLower()))
                        {
                            found = true;
                        } 
                    }
                    else
                    {
                        if (f.geometry.type == "MultiLineString")
                        {
                            foreach (List<object> g in f.geometry.coordinates)
                            {
                                foreach (object o in g)
                                {
                                    Newtonsoft.Json.Linq.JArray h = (Newtonsoft.Json.Linq.JArray) o;
                                    double x = (double) h[0];
                                    double y = (double) h[1];
                                    if (((x >= neLng && x <= swLng) || (x <= neLng && x >= swLng)) && ((y >= neLat && y <= swLat) || (y <= neLat && y >= swLat)))
                                    {
                                        found = true;
                                        break;
                                    }
                                    if (found) break;
                                }
                            }
                        }
                        if (f.geometry.type == "LineString")
                        {
                            foreach(List<object> h in f.geometry.coordinates)
                            {
                                double x = (double)h[0];
                                double y = (double)h[1];
                                if (((x >= neLng && x <= swLng) || (x <= neLng && x >= swLng)) && ((y >= neLat && y <= swLat) || (y <= neLat && y >= swLat)))
                                {
                                    found = true;
                                    break;
                                }
                                if (found) break;
                            }
                        }
                    }

                    if (found)
                    {
                        PreviewItems.Result result = new PreviewItems.Result();
                        result.links = new PreviewItems.Links(); 
                        string serialno = "0000";
                        if(f.properties.SERIAL_NO != null) serialno = f.properties.SERIAL_NO;
                        result.id = serialno;
                        result.name = f.properties.NAME;
                        result.description = f.properties.COMMENTS;
                        if (f.properties.COMMENTS == null && f.properties.W_COMMENT != null) result.description = f.properties.W_COMMENT.ToString();
                        if(f.properties.COMMENTS != null && f.properties.W_COMMENT != null) result.description = f.properties.COMMENTS + ' ' + f.properties.W_COMMENT.ToString();
                        result.featureType = "track";
                        result.closed = false;
                        if (f.properties.CLOS_STAT != null) result.closed = true;
                        result.features = new List<string>();  
                        result.links.details = _appsettings.BaseURL + "/tracks/" + serialno;


                        if (f.photos.Count > 0)
                        {
                            result.image = getImageString(f.photos[0]);
                        }


                        //Get Features
                        foreach (string s in f.trackFeatures)
                        {
                            if(!result.features.Contains(s))  result.features.Add(s);  
                        }


                        if(trackTypeFilter || difficultyFilter || durationFilter || activitiesFilter || amenitiesFilter || accessFilter)
                        {
                            thisfilteredok = true;
                            bool filter1ok = true;
                            bool filter2ok = true;
                            bool filter3ok = true;
                            bool filter4ok = true;
                            bool filter5ok = true;
                            bool filter6ok = true;
                            bool debugtest = false;

                            if (trackTypeFilter)
                            {
                                if(f.properties.SERIAL_NO == "1764")
                                {
                                    debugtest = false;
                                }
                                if(!filterCompare(f.filters.trackType, trackType))
                                {
                                    filter1ok = false;
                                }
                            }

                            if (difficultyFilter && !filterCompare(f.filters.difficulty, difficulty))
                            {
                                filter2ok = false;
                            }


                            if (durationFilter)
                            {
                                if (!filterCompare(f.filters.duration, duration))
                                {
                                    filter3ok = false;
                                }
                                else
                                {
                                    debugtest = true;
                                }
                            }

                            /*
                            if (activitiesFilter)
                            {
                                if (!filterCompare(f.filters.activities, activities))
                                {
                                    filter4ok = false;
                                }
                                else
                                {
                                    debugtest = true;
                                }
                            }

                            if (amenitiesFilter) 
                            {
                                if (!filterCompare(f.filters.amenities, amenities))
                                {
                                    filter5ok = false;
                                }
                                else
                                {
                                    debugtest = true;
                                }
                            }

                            if (accessFilter && !filterCompare(f.filters.access, access))
                            {
                                filter6ok = false;
                            }
                            */

                            //if (filter1ok) thisfilteredok = true;
                            if (!filter1ok ||!filter2ok || !filter2ok || !filter3ok || !filter4ok || !filter5ok || !filter6ok) thisfilteredok = false;

                        }


                        if (filtered)
                        {
                            if (thisfilteredok)
                            {
                                p.results.Add(result);
                            }
                        }
                        else
                        {
                            p.results.Add(result);
                        }
                    }
                }




                //GET SITES
                if (/*!trackTypeFilter  && !difficultyFilter && !durationFilter ||*/ true)
                {
                    foreach (CachedDataClass.Site.Feature f in cd.site.features)
                    {
                        bool found = false;
                        bool filterFound = false;
                        double x = f.geometry.coordinates[0];
                        double y = f.geometry.coordinates[1];


                        if (query.Length > 0)
                        {
                            if (f.properties.NAME.ToLower().Contains(query.ToLower()))
                            {
                                found = true;
                            }
                        }
                        else
                        {
                            if (((x >= neLng && x <= swLng) || (x <= neLng && x >= swLng)) && ((y >= neLat && y <= swLat) || (y <= neLat && y >= swLat)))
                            {
                                found = true;
                            }
                        }

                        if (found)
                        {
                            if(f.properties.SERIAL_NO == "2118" || f.properties.SERIAL_NO == "10342" || f.properties.SERIAL_NO == "8540")
                            {
                                string debug = "test";
                            }
                            PreviewItems.Result result = new PreviewItems.Result();
                            result.links = new PreviewItems.Links();
                            result.id = f.properties.SERIAL_NO;
                            result.name = f.properties.NAME;
                            result.description = f.siteDescriptrionFromFirstActivity;

                            result.featureType = "site";
                            result.closed = false;
                            if (f.properties.CLOS_STAT != null) result.closed = true;

                            if (f.properties.PHOTO_ID_1 != null)
                            {
                                int pid = 0;
                                int.TryParse(f.properties.PHOTO_ID_1, out pid);
                                if (pid > 0) result.image = getImageString(pid);
                            }

                            result.features = new List<string>();
                            result.links.details = _appsettings.BaseURL  + "/sites/" + f.properties.SERIAL_NO;

                            //Get Features and filters
                            filterFound = false;
                            foreach (string s in f.siteFeatures)
                            {
                                if (!result.features.Contains(s)) result.features.Add(s);
                                filterFound = ff(s, trackType, filterFound);
                            }


                            if (/*trackTypeFilter || difficultyFilter || durationFilter ||*/ activitiesFilter || amenitiesFilter || accessFilter)
                            {
                                thisfilteredok = true;
                                bool filter4ok = true;
                                bool filter5ok = true;
                                bool filter6ok = true;
                                bool debugtest = false;

                                if (activitiesFilter && !filterCompare(f.filters.activities, activities))
                                {
                                    filter4ok = false;
                                }

                                if (amenitiesFilter) {
                                    if (!filterCompare(f.filters.amenities, amenities))
                                    {
                                        filter5ok = false;
                                    }
                                    else
                                    {
                                        debugtest = true;
                                    }
                                }

                                if (accessFilter && !filterCompare(f.filters.access, access))
                                {
                                    filter6ok = false;
                                }

                                if (!filter4ok || !filter5ok || !filter6ok) thisfilteredok = false;

                                if (thisfilteredok)
                                {
                                    p.results.Add(result);
                                }
                            }
                            else
                            {
                                p.results.Add(result);
                            }
                        }
                    }
                }


                //GET RELICS
                if (!filtered)
                {
                    foreach (CachedDataClass.Relic.Feature f in cd.relic.features)
                    {
                        bool found = false;
                        double x = f.geometry.coordinates[0];
                        double y = f.geometry.coordinates[1];


                        if (query.Length > 0)
                        {
                            if (f.properties.NAME.ToLower().Contains(query.ToLower()))
                            {
                                found = true;
                            }
                        }
                        else
                        {
                            if (((x >= neLng && x <= swLng) || (x <= neLng && x >= swLng)) && ((y >= neLat && y <= swLat) || (y <= neLat && y >= swLat)))
                            {
                                found = true;
                            }
                        }

                        if (found)
                        {
                            PreviewItems.Result result = new PreviewItems.Result();
                            result.links = new PreviewItems.Links();
                            result.id = f.properties.SERIAL_NO;
                            result.name = f.properties.NAME;
                            result.description = f.properties.COMMENTS;
                            result.featureType = "relic";
                            result.closed = false;
                            if (f.properties.PHOTO_ID_1 != null)
                            {
                                int pid = 0;
                                int.TryParse(f.properties.PHOTO_ID_1, out pid);
                                if (pid > 0) result.image = getImageString(pid);
                            }
                            result.features = new List<string>();
                            result.features.Add("historicRelic");

                            foreach(string rf in f.relicFeatures)
                            {
                                result.features.Add(rf); 
                            }

                            result.links.details = _appsettings.BaseURL + "/relics/" + f.properties.SERIAL_NO;
                            p.results.Add(result);
                        }
                    }
                }

                int totalcount = p.results.Count(); 

                if(pagesize > 0 && p.results.Count() > pagesize)
                {
                    PreviewItems ppaged = new PreviewItems();

                    int i = 0;
                    foreach(PreviewItems.Result r in p.results)
                    {
                        int mini = pagesize * (page -1);
                        int maxi = (pagesize * page) - 1;
                   
                        if(i >= mini && i <= maxi)
                        {
                            ppaged.results.Add(r);
                        }

                        if(maxi >= totalcount - 1)
                        {
                            ppaged.meta.links.next = null;
                        }
                        else
                        {
                            string s = fullurl.ToLower().Replace("page=" + page.ToString(), "");
                            s += "&page=" + (page + 1).ToString();
                            s = s.Replace("&&", "&");
                            ppaged.meta.links.next = s;
                        }
                        i++;
                    }


                    ppaged.meta.totalResults = totalcount;
                    ppaged.meta.links.self = fullurl.ToLower();

                    JsonResult jsonresult = new JsonResult(ppaged);
                    return jsonresult;

                }
                else
                {
                    p.meta.totalResults = totalcount;
                    p.meta.links.self = fullurl.ToLower();

                    JsonResult jsonresult = new JsonResult(p);
                    return jsonresult;
                }

            }
            catch (Exception ex)
            {
                JsonResult jsonresult = new JsonResult("Error: " + ex.ToString());
                return jsonresult;
            }
        }


        public JsonResult GetTrack(int id)
        {
            
            try
            {
                TrackResult tr = new Models.TrackResult();
                tr = GetTrackResult(id);

                /*

                var cd = SetGetMemoryCache();

                TrackResult tr = new Models.TrackResult();
                foreach (CachedDataClass.Track.Feature f in cd.track.features)
                {
                    if (f.properties.SERIAL_NO == id.ToString())
                    {
                        tr.result.id = id.ToString();
                        tr.result.name = f.properties.NAME;
                        tr.result.description = f.properties.COMMENTS;
                        foreach (string s in f.trackFeatures)
                        {
                            if(!tr.result.features.Contains(s))  tr.result.features.Add(s);
                        }


                        //get bounding box
                        double neLng = -9999999999;
                        double swLng = 9999999999;
                        double neLat = -9999999999;
                        double swLat = 9999999999;

                        foreach (List<List<double>> g in f.geometry.coordinates)
                        {
                            foreach (List<double> h in g)
                            {
                                double x = h[0];
                                double y = h[1];
                                if (x > neLng) neLng = x;
                                if (x < swLng) swLng = x;
                                if (y > neLat) neLat = y;
                                if (y < swLat) swLat = y;
                            }

                            if (neLng != 9999999999)
                            {
                                tr.result.boundingBox.neLat = neLat;
                                tr.result.boundingBox.neLng = neLng;
                                tr.result.boundingBox.swLat = swLat;
                                tr.result.boundingBox.swLng = swLng;
                            }
                        }


                        //get activities
                        foreach(CachedDataClass.Track.Activity trAct in f.activities)
                        {
                            TrackResult.Activity a = new TrackResult.Activity();
                            a.activityType = trAct.activityType;
                            a.comment = trAct.comment;
                            a.gradeComment = trAct.grade_comment;
                            a.grade = trAct.grade;   
                            a.distance = trAct.distance;
                            a.duration = trAct.duration;
                            a.durationMeasure = trAct.duration_measure;
                            tr.result.activities.Add(a);   
                        }


                        tr.result.qualities.trackClass = f.qualities.track_class;
                        tr.result.qualities.quality = f.qualities.quality;
                        tr.result.qualities.markings = f.qualities.markings;

                        foreach(int pid in f.photos)
                        {
                            string p = getImageString(pid);
                            tr.result.images.Add(p);    
                        }

                        tr.result.closure.description = f.properties.CLOS_DESC;
                        tr.result.closure.closureDate = ncdatestr(f.properties.CLOS_START);
                        tr.result.closure.openDate = ncdatestr(f.properties.CLOS_OPEN);
                        tr.result.closure.reason = f.properties.CLOS_REAS;
                        tr.result.closure.status = f.properties.CLOS_STAT;

                        tr.meta.links.self = baseurl + "/tracks/" + id.ToString();


                        if (tr.result.features.Count == 0) tr.result.features = null;
                        if (tr.result.activities.Count == 0) tr.result.activities = null;
                        if (tr.result.images.Count == 0) tr.result.images = null;
                        break;

                    }
                }
                */
                
                JsonResult jsonresult = new JsonResult(tr);
                return jsonresult;

            }
            catch (Exception ex)
            {
                JsonResult jsonresult = new JsonResult("Error: " + ex.ToString());
                return jsonresult;
            }

        }


        // ***********************************************************************************************************************
        // GET TRACKRESULT
        // ***********************************************************************************************************************
        public TrackResult GetTrackResult(int id)
        {
            try
            {
                var cd = SetGetMemoryCache();

                TrackResult tr = new Models.TrackResult();
                foreach (CachedDataClass.Track.Feature f in cd.track.features)
                {
                    if (f.properties.SERIAL_NO == id.ToString())
                    {
                        tr.result.id = id.ToString();
                        tr.result.name = f.properties.NAME;
                        tr.result.description = f.properties.originalComments;
                        foreach (string s in f.trackFeatures)
                        {
                            if (!tr.result.features.Contains(s)) tr.result.features.Add(s);
                        }
                        tr.result.accessDescription = f.properties.ACCESS_DSC;


                        //get bounding box
                        double neLng = -9999999999;
                        double swLng = 9999999999;
                        double neLat = -9999999999;
                        double swLat = 9999999999;

                        bool firstpoint = false;

                        double x = 0;
                        double y = 0;
                        if (f.geometry.type == "MultiLineString")
                        {
                            foreach (List<object> g in f.geometry.coordinates)
                            {
                                foreach (object o in g)
                                {
                                    Newtonsoft.Json.Linq.JArray h = (Newtonsoft.Json.Linq.JArray)o;
                                    x = (double)h[0];
                                    y = (double)h[1];
                                    if (x > neLng) neLng = x;
                                    if (x < swLng) swLng = x;
                                    if (y > neLat) neLat = y;
                                    if (y < swLat) swLat = y;

                                    if (!firstpoint)
                                    {
                                        tr.result.startLocation = new TrackResult.Location();
                                        tr.result.startLocation.latitude = y;
                                        tr.result.startLocation.longitude = x;
                                        firstpoint = true;
                                    }
                                }

                                tr.result.endLocation = new TrackResult.Location();
                                tr.result.endLocation.latitude = y;
                                tr.result.endLocation.longitude = x;

                            }
                        }
                        if (f.geometry.type == "LineString")
                        {
                            foreach (List<object> h in f.geometry.coordinates)
                            {
                                x = (double)h[0];
                                y = (double)h[1];
                                if (x > neLng) neLng = x;
                                if (x < swLng) swLng = x;
                                if (y > neLat) neLat = y;
                                if (y < swLat) swLat = y;

                                if (!firstpoint)
                                {
                                    tr.result.startLocation = new TrackResult.Location();
                                    tr.result.startLocation.latitude = y;
                                    tr.result.startLocation.longitude = x;
                                    firstpoint = true;
                                }
                            }

                            tr.result.endLocation = new TrackResult.Location();
                            tr.result.endLocation.latitude = y;
                            tr.result.endLocation.longitude = x;
                        }
                        if (neLng != 9999999999)
                        {
                            tr.result.boundingBox.neLat = neLat;
                            tr.result.boundingBox.neLng = neLng;
                            tr.result.boundingBox.swLat = swLat;
                            tr.result.boundingBox.swLng = swLng;
                        }
 

                    /*
                    foreach (List<List<double>> g in f.geometry.coordinates)
                    {
                        foreach (List<double> h in g)
                        {
                            double x = h[0];
                            double y = h[1];
                            if (x > neLng) neLng = x;
                            if (x < swLng) swLng = x;
                            if (y > neLat) neLat = y;
                            if (y < swLat) swLat = y;

                            if(!firstpoint)
                            {
                                tr.result.location = new TrackResult.Location();
                                tr.result.location.latitude = y;
                                tr.result.location.longitude = x;
                                firstpoint = true;
                            }
                        }

                        if (neLng != 9999999999)
                        {
                            tr.result.boundingBox.neLat = neLat;
                            tr.result.boundingBox.neLng = neLng;
                            tr.result.boundingBox.swLat = swLat;
                            tr.result.boundingBox.swLng = swLng;
                        }
                    }
                    */


                    //get activities
                    foreach (CachedDataClass.Track.Activity trAct in f.activities)
                        {
                            TrackResult.Activity a = new TrackResult.Activity();
                            a.activityType = trAct.activityType;
                            a.comment = trAct.comment;
                            a.gradeComment = trAct.grade_comment;
                            //scott 20112017
                            a.grade = trAct.grade;
                            a.distance = trAct.distance;
                            a.duration = trAct.duration;
                            a.durationMeasure = trAct.duration_measure;
                            tr.result.activities.Add(a);
                        }


                        tr.result.qualities.trackClass = f.qualities.track_class;
                        tr.result.qualities.quality = f.qualities.quality;
                        tr.result.qualities.markings = f.qualities.markings;

                        foreach (int pid in f.photos)
                        {
                            string p = getImageString(pid);
                            tr.result.images.Add(p);
                        }

                        tr.result.closure.description = f.properties.CLOS_DESC;
                        tr.result.closure.closureDate = ncdatestr(f.properties.CLOS_START);
                        tr.result.closure.openDate = ncdatestr(f.properties.CLOS_OPEN);
                        tr.result.closure.reason = f.properties.CLOS_REAS;
                        tr.result.closure.status = f.properties.CLOS_STAT;

                        tr.meta.links.self = _appsettings.BaseURL + "/tracks/" + id.ToString();


                        if (tr.result.features.Count == 0) tr.result.features = null;
                        if (tr.result.activities.Count == 0) tr.result.activities = null;
                        if (tr.result.images.Count == 0) tr.result.images = null;
                        break;

                    }
                }


                /*
                if(String.IsNullOrEmpty(tr.result.description))
                {
                    foreach(TrackResult.Activity a  in tr.result.activities)
                    {
                        if(a.comment != null && String.IsNullOrEmpty(a.comment.ToString()))
                        {
                            tr.result.description = a.comment.ToString();
                            break;
                        }
                    }
                }
                */

                return tr;
                //JsonResult jsonresult = new JsonResult(tr);
                //return jsonresult;

            }
            catch (Exception ex)
            {
               throw ex;
            }

        }






        // ***********************************************************************************************************************
        // GET SITE
        // ***********************************************************************************************************************
        public JsonResult GetSite(int id)
        {
            try
            {
                var cd = SetGetMemoryCache();

                SiteResult s = new Models.SiteResult();
                foreach (CachedDataClass.Site.Feature f in cd.site.features)
                {
                    if (f.properties.SERIAL_NO == id.ToString())
                    {
                        s.result.id = id.ToString();
                        s.result.name = f.properties.NAME;
                        s.result.description = f.properties.originalComments;
                        foreach(string ss in f.siteFeatures)
                        {
                            if (!s.result.features.Contains(ss)) s.result.features.Add(ss);
                        }
                        s.result.accessDescription = f.properties.ACCESS_DSC;
                           
                        s.result.location.longitude = f.geometry.coordinates[0];
                        s.result.location.latitude = f.geometry.coordinates[1];

                        //get activities
                        foreach (CachedDataClass.Site.Activity act in f.activities)
                        {
                            SiteResult.Activity a = new SiteResult.Activity();
                            a.activityType = act.activityType;
                            a.comment = act.comment;
                            s.result.activities.Add(a);
                        }


                        foreach (int pid in f.photos)
                        {
                            string p = getImageString(pid);
                            s.result.images.Add(p);
                        }

                        s.result.closure.description = f.properties.CLOS_DESC; 
                        s.result.closure.closureDate = ncdatestr(f.properties.CLOS_START);
                        s.result.closure.openDate = ncdatestr(f.properties.CLOS_OPEN);
                        s.result.closure.reason = f.properties.CLOS_REAS;
                        s.result.closure.status = f.properties.CLOS_STAT;


                        int track = ncint(f.properties.IS_PART_OF);
                        if (track > 0)
                        {
                            s.result.track = GetTrackResult(track).result;
                            s.result.links.track = _appsettings.BaseURL + "/tracks/" + track.ToString();
                        }

                        s.meta.links.self = _appsettings.BaseURL + "/sites/" + id.ToString();

                        if (s.result.features.Count == 0) s.result.features = null;
                        if (s.result.activities.Count == 0) s.result.activities = null;
                        if (s.result.images.Count == 0) s.result.images = null;
                        break;
                    }
                }

                /*
                if (s.result.description == null || String.IsNullOrEmpty(s.result.description.ToString()))
                {
                    foreach (SiteResult.Activity a in s.result.activities)
                    {
                        if (a.comment != null && !String.IsNullOrEmpty(a.comment.ToString()))
                        {
                            s.result.description = a.comment.ToString();
                            break;
                        }
                    }
                }
                */

                JsonResult jsonresult = new JsonResult(s);
                return jsonresult;

            }
            catch (Exception ex)
            {
                JsonResult jsonresult = new JsonResult("Error: " + ex.ToString());
                return jsonresult;
            }

        }


        // ***********************************************************************************************************************
        // GET RELIC
        // ***********************************************************************************************************************
        public JsonResult GetRelic(int id)
        {

            var cd = SetGetMemoryCache();

            try
            {
                RelicResult r = new RelicResult();
                foreach (CachedDataClass.Relic.Feature f in cd.relic.features)
                {
                    if (f.properties.SERIAL_NO == id.ToString())
                    {
                        r.result.id = id.ToString();
                        r.result.name = f.properties.NAME;
                        r.result.description = f.properties.COMMENTS;

                        r.result.location.longitude = f.geometry.coordinates[0];
                        r.result.location.latitude = f.geometry.coordinates[1];

                        //r.result.activities = new List<RelicResult.Activity>(); 
                        //RelicResult.Activity act = new Models.RelicResult.Activity();

                        r.result.features.Add("historicRelic");

                        //act.activityType = "historicRelic";
                        //r.result.activities.Add(act);
                        //get activities
                        foreach (CachedDataClass.Relic.Activity act2 in f.activities)
                        {
                            //RelicResult.Activity a = new RelicResult.Activity();
                            //a.activityType = act2.activityType;
                            //a.comment = act2.comment;
                            //r.result.activities.Add(a);

                        }

                        foreach (int pid in f.photos)
                        {
                            string p = getImageString(pid);
                            r.result.images.Add(p);
                        }

                        foreach (string ss in f.relicFeatures)
                        {
                            if (!r.result.features.Contains(ss)) r.result.features.Add(ss);
                        }



                        int track = ncint(f.properties.IS_PART_OF);
                        if (track > 0)
                        {
                            r.result.track = GetTrackResult(track).result;
                            r.result.links.track = _appsettings.BaseURL + "/tracks/" + track.ToString();
                        }



                        r.meta.links.self = _appsettings.BaseURL + "/relics/" + id.ToString();

                        //if (r.result.activities.Count == 0) r.result.activities = null;
                        if (r.result.images.Count == 0) r.result.images = null;

                        break;
                    }
                }

                JsonResult jsonresult = new JsonResult(r);
                return jsonresult;
            }
            catch (Exception ex)
            {
                JsonResult jsonresult = new JsonResult("Error: " + ex.ToString());
                return jsonresult;
            }

        }



        public bool ff(string v, string filter, bool filterFound)
        {
            if (filter.Trim() == "" || filter.ToUpper().Trim() == "ALL") return true;

            if (v == filter) return true;
            else return false;
        }

        public int ncint(object v)
        {
            if (v == null) return 0;
            int rv = 0;
            int.TryParse(v.ToString(), out rv);
            return rv;
        }

        public object ncdatestr(string d)
        {
            if (d == null) return null;
            DateTime dt = new DateTime();
            bool rv = DateTime.TryParse(d, out dt);
            if (!rv) return null;
            return dt.ToString("yyyy-MM-dd");
        }

        public string getImageString(int id)
        {
            return _appsettings.ImageLocation + "/" + id.ToString() + ".jpg?autorotate=true&mode=crop";
        }

        public string ncstr(object v)
        {
            if (v == null) return "";
            return v.ToString(); 
        }

        public bool filterCompare2(string compareList, string compareValue)
        {
            if (compareList == null || compareList == "" || compareValue == null || compareValue == "") return false;
            string s = ("," + compareList + ",").ToLower().Replace(" ", "");

            string[] sl = compareValue.Split(',');
            for (int i = 0; i < sl.Length; i++)
            {
                string si = sl[i];
                if (si.Length > 0)
                {
                    string s2 = ("," + si + ",").ToLower().Replace(" ", "");
                    if (s.Contains(s2)) return true;
                }
            }

            return false;

        }

        public bool filterCompare(List<string> features, string compareValue)
        {
            bool allFound = true;
            string[] sl = compareValue.Split(',');
            for (int i = 0; i < sl.Length; i++)
            {
                string si = sl[i];
                if (si.Length > 0)
                {
                    bool fffound = false;
                    foreach(string fff in features)
                    {
                        if(fff.ToLower() == si.ToLower())
                        {
                            fffound = true;
                            return true;
                            break;
                        }
                    }
                    if (!fffound) allFound = false;
                }
            }

            return allFound;
        }



    }
}
