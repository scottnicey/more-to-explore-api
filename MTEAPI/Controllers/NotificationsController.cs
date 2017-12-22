using Microsoft.AspNetCore.Mvc;
using MTEAPI.Models;
using MTEAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MTEAPI.Data;
using Newtonsoft.Json;


namespace MTEAPI.Controllers
{
    [Route("[controller]/register")]
    public class NotificationsController : Controller
    {
        ApplicationDbContext _context;
        AppSettings _appsettings;

        public NotificationsController(ApplicationDbContext context, IOptions<AppSettings> options_appsettings)
        {
            _context = context;
            _appsettings = options_appsettings.Value;
        }

        [HttpPost]
        public JsonResult Register()
        {
            try
            {
                Services.NotificationsService serv = new Services.NotificationsService(_context, _appsettings);

                System.IO.StreamReader r = new System.IO.StreamReader(Request.Body);
                string data = r.ReadToEnd();

                object o = JsonConvert.DeserializeObject<Services.NotificationsService.RegistrationObject>(data);

                Services.NotificationsService.RegistrationObject obj = JsonConvert.DeserializeObject<Services.NotificationsService.RegistrationObject>(data);


                return serv.register(obj);
            }
            catch(JsonException ex)
            {
                return new JsonResult("Error: " + ex.Message); 
            }
        }

        [HttpGet]
        public JsonResult Info()
        {
            return new JsonResult("Must post data here.");
        }

    }

}
