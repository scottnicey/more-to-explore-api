using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using System.Collections.Generic;
using System;
using MTEAPI.Models;


using MTEAPI.Data;

namespace MTEAPI.Services
{
    public class NewsService : Controller

    {

        ApplicationDbContext _context;
        AppSettings _appsettings;

        public NewsService(ApplicationDbContext context, AppSettings appsettings)
        {
            _context = context;
            _appsettings = appsettings;
        }


        public JsonResult GetNews(int page, int page_size, int getid)
        {
            //var options =  GetService<DbContextOptions<BloggingContext>>();
            try
            {
                bool noPaging = false;
                if (page_size == -1) noPaging = true;
                if (page < 0) page = 1;
                int rowcount = 0;

                var conn = _context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open) conn.Open(); 
                using (var command = conn.CreateCommand())
                {
                    string query = "SELECT * from News where isnull(published,1) = 1";
                    if (getid > 0) query += " and id = " + getid.ToString();
                    query += " order by id desc";
                    command.CommandText = query;
                    DbDataReader reader = command.ExecuteReader();

                    News n = new News();

                    int i = 0;
                    int ifrom = (page-1) * page_size;
                    int ito = ifrom + page_size - 1;

                    bool nextPageExists = false;
                    string self = _appsettings.BaseURL + "/news";

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (noPaging || (i >= ifrom && i <= ito))
                            {
                                int id = (int)reader["id"];
                                string newsId = reader["newsId"].ToString();
                                string title = reader["title"].ToString();
                                string body = reader["body"].ToString().Trim();
                                if(body.Length > 200)
                                {
                                    //body = body.Substring(0, 200) + "..."; 
                                }
                                string image = reader["image"].ToString();
                                string publishedDateStr = reader["publishedDate"].ToString();
                                DateTime publishedDate = DateTime.MinValue;
                                DateTime.TryParse(publishedDateStr, out publishedDate);
                                string link = reader["link"].ToString();
                                string linkTitle = reader["linkTitle"].ToString();


                                News.Result ni = new News.Result();

                                ni.id = id.ToString();
                                if (!String.IsNullOrEmpty(title)) ni.title = title;
                                if (!String.IsNullOrEmpty(body)) ni.body = body;
                                if (!String.IsNullOrEmpty(image)) ni.image = _appsettings.ImageLocationNews + "/" + image;

                                if (publishedDate.Year > 2000) ni.publishedDate = publishedDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

                                if (!String.IsNullOrEmpty(link)) ni.link = link;
                                if (!String.IsNullOrEmpty(linkTitle)) ni.linkTitle = linkTitle;


                                if (getid > 0)
                                {
                                    NewsItem nii = new NewsItem();
                                    nii.news.result.id = ni.id;
                                    nii.news.result.title = ni.title;
                                    nii.news.result.body = ni.body;
                                    nii.news.result.image = ni.image;
                                    nii.news.result.publishedDate = ni.publishedDate;
                                    nii.news.result.link = ni.link;
                                    nii.news.result.linkTitle = ni.linkTitle;


                                    self += "/" + getid.ToString();

                                    nii.news.meta.self = self; 
                                    return new JsonResult(nii.news);
                                }


                                n.news.results.Add(ni);
                            }
                            i++;

                            if (!noPaging && i > ito) nextPageExists = true;
                        }
                    }
                    reader.Dispose();
                    rowcount = i;

                    n.news.meta.links = new News.Links2();
                    
                    string next = _appsettings.BaseURL + "/news";
                    n.news.meta.links.next = null;

                    if (getid > 0)
                    {
                        self += "/" + getid.ToString();
                    }
                    else
                    {
                        if(!noPaging)
                        {
                            self += "?page_size=" + page_size.ToString() + "&page=" + page.ToString();
                            if (nextPageExists)
                            {
                                next += "?page_size=" + page_size.ToString() + "&page=" + (page + 1).ToString();
                                n.news.meta.links.next = next;
                            }
                        }
                    }
                    
                    n.news.meta.totalResults = rowcount;
                    n.news.meta.links.self = self;
                   

                    return new JsonResult(n.news); 
                }
            }
            catch(DbException ex)
            {
                return new JsonResult(ex.ToString());
            }
            




        }



    }


    public class News
    {
        public RootObject news { get; set; }

        public News()
        {
            news = new RootObject();
            news.results = new List<Result>();
            news.meta = new Meta(); 
        }

        public class Result
        {
            public string id { get; set; }
            public object title { get; set; }
            public object body { get; set; }
            public object image { get; set; }
            public object publishedDate { get; set; }
            public object link { get; set; }
            public object linkTitle { get; set; }
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

        public class RootObject
        {
            public List<Result> results { get; set; }
            public Meta meta { get; set; }
        }
    }
}

public class NewsItem
{
    public RootObject news { get; set; }

    public NewsItem()
    {
        news = new RootObject();
        news.result = new Result();
        news.meta = new Meta();
    }

    public class Result
    {
        public string id { get; set; }
        public object title { get; set; }
        public object body { get; set; }
        public object image { get; set; }
        public object publishedDate { get; set; }
        public object link { get; set; }
        public object linkTitle { get; set; }
    }

    public class Meta
    {
        public string self { get; set; }
    }

    public class RootObject
    {
        public Result result { get; set; }
        public Meta meta { get; set; }
    }

}