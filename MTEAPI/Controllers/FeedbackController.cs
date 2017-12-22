using Microsoft.AspNetCore.Mvc;
using MTEAPI.Models;
using Microsoft.Extensions.Options;
using MTEAPI.Data;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

namespace MTEAPI.Controllers
{
    [Route("[controller]")]
    public class FeedbackController : Controller
    {
        ApplicationDbContext _context;
        AppSettings _appsettings;

        public FeedbackController(ApplicationDbContext context, IOptions<AppSettings> options_appsettings)
        {
            _context = context;
            _appsettings = options_appsettings.Value;
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<JsonResult> PostAsync()
        {
            try
            {
                Services.FeedbackService serv = new Services.FeedbackService(_context, _appsettings);
                Services.FeedbackService.FeedbackData fd = new Services.FeedbackService.FeedbackData();

                fd.platform = Request.Form["platform"];
                fd.feedbackObjectType = Request.Form["feedbackObjectType"];
                fd.feedbackObjectId = Request.Form["feedbackObjectId"];
                fd.comment = Request.Form["comment"];
                fd.feedbackType = Request.Form["feedbackType"];
                fd.feedbackName = Request.Form["feedbackName"];

                int cnt = 0;
                foreach (IFormFile file in Request.Form.Files)
                {
                    if (file.ContentType == "image/jpeg")
                    {
                        cnt++;
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);

                            byte[] photo = new byte[memoryStream.Length];
                            await memoryStream.ReadAsync(photo, 0, photo.Length);

                            string path = _appsettings.FeedbackImageLocation + "\\";
                            string fn = "mte-" + DateTime.Today.ToString("yyyy-MM-dd") + "-" + fd.feedbackName + "-" + fd.feedbackObjectId + "-" + cnt.ToString() + ".jpg";
                            fn = path + fn.Replace("  ", "-").Replace(" ", "-").Replace("--","-").Replace("--","-").ToLower();
                            System.IO.File.WriteAllBytes(fn, photo);

                            if (fd.photo1 == null) fd.photo1 = fn;
                            else if (fd.photo2 == null) fd.photo2 = fn;
                            else if (fd.photo3 == null) fd.photo3 = fn;
                        }
                    }
                }



                JsonResult rv = await serv.createFeedbackAsync(fd);

                return rv;
            }
            catch (JsonException ex)
            {
                string ee = ex.ToString();
                return new JsonResult("Error");
            }



        }

        [HttpGet]
        public JsonResult Get()
        {
            return new JsonResult("Must post here."); 
        }

    }

}
