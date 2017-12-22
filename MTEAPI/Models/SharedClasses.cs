using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTEAPI.Models
{
    //general classes are here
    public class PreviewItems
    {
        public class Links
        {
            public string details { get; set; }
        }

        public class Result
        {
            public string id { get; set; }
            public string name { get; set; }
            public object description { get; set; }
            public string featureType { get; set; }
            public bool closed { get; set; }
            public string image { get; set; }
            public List<string> features { get; set; }
            public Links links { get; set; }
        }

        public class Links2
        {
            public string self { get; set; }
            public string next { get; set; }
        }

        public class Meta
        {
            public Links2 links { get; set; }
            public int totalResults { get; set; }
        }

        public List<Result> results { get; set; }
        public Meta meta { get; set; }


        public PreviewItems()
        {
            results = new List<Result>();
            meta = new Meta();
            meta.links = new Links2(); 
        }
    }


    public class TrackResult
    {
        public class Location
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class BoundingBox
        {
            public double neLat { get; set; }
            public double neLng { get; set; }
            public double swLat { get; set; }
            public double swLng { get; set; }
        }

        public class Qualities
        {
            public object trackClass { get; set; }
            public object quality { get; set; }
            public object markings { get; set; }
        }

        public class Activity
        {
            public string activityType { get; set; }
            public object comment { get; set; }
            public object grade { get; set; }
            public object gradeComment { get; set; }
            public object distance { get; set; }
            public object duration { get; set; }
            public object durationMeasure { get; set; }
        }

        public class Result
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public object accessDescription { get; set; }
            public List<string> images { get; set; }
            public BoundingBox boundingBox { get; set; }
            public Location startLocation { get; set; }
            public Location endLocation { get; set; }
            public List<string> features { get; set; }
            public Qualities qualities { get; set; }
            public List<Activity> activities { get; set; }
            public Closure closure { get; set; }
        }


        public class Links2
        {
            public string self { get; set; }
        }

        public class Meta
        {
            public Links2 links { get; set; }
        }
        public class Closure
        {
            public object description { get; set; }
            public object openDate { get; set; }
            public object closureDate { get; set; }
            public object reason { get; set; }
            public object status { get; set; }
        }


        public Result result { get; set; }
        public Meta meta { get; set; }

        public TrackResult() {
            result = new Result();
            result.boundingBox = new BoundingBox();
            result.qualities = new Qualities();
            result.activities = new List<Activity>();
            result.images = new List<string>();
            result.features = new List<string>();
            meta = new Meta();
            meta.links = new Links2();
            result.closure = new Closure();
        }
    }


    public class SiteResult
    {
        public class Location
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class Activity
        {
            public string activityType { get; set; }
            public object comment { get; set; }
        }

        public class Links
        {
            public string track { get; set; }
        }

        public class Result
        {
            public string id { get; set; }
            public object name { get; set; }
            public object description { get; set; }
            public object accessDescription { get; set; }
            public Location location { get; set; }
            public List<string> images { get; set; }
            //public List<TrackResult.Result> track { get; set; }
            public TrackResult.Result track { get; set; } 
            public List<string> features { get; set; }
            public List<Activity> activities { get; set; }
            public Links links { get; set; }
            public Closure closure { get; set; }
        }

        public class Links2
        {
            public string self { get; set; }
        }

        public class Meta
        {
            public Links2 links { get; set; }
        }


        public Result result { get; set; }
        public Meta meta { get; set; }


        public class Closure
        {
            public object description { get; set; }
            public object openDate { get; set; }
            public object closureDate { get; set; }
            public object reason { get; set; }
            public object status { get; set; }
        }

        public SiteResult()
        {
            result = new Result();
            meta = new Meta();
            result.links = new Links();
            result.location = new Location();
            result.images = new List<string>();
            //result.track = new List<string>();
            result.features = new List<string>();
            result.activities = new List<Activity>();
            meta.links = new Links2();
            result.closure = new Closure();

        }

    }


    public class RelicResult
    {
        public class Location
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class Activity
        {
            public string activityType { get; set; }
            public object comment { get; set; }
        }

        public class Links
        {
            public string track { get; set; }
        }

        public class Result
        {
            public string id { get; set; }
            public object name { get; set; }
            public object description { get; set; }
            public Location location { get; set; }
            public List<string> features { get; set; }
            //public List<Activity> activities { get; set; }
            public List<string> images { get; set; }
            public TrackResult.Result track { get; set; }
            public Links links { get; set; }
        }

        public class Links2
        {
            public string self { get; set; }
        }

        public class Meta
        {
            public Links2 links { get; set; }
        }


        public Result result { get; set; }
        public Meta meta { get; set; }


        public RelicResult()
        {
            result = new Result();
            meta = new Meta();
            result.links = new Links();
            result.location = new Location();
            result.features = new List<string>();
            result.images = new List<string>();
            meta.links = new Links2();
        }



    }


}
