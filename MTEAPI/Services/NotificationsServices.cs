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
    public class NotificationsService : Controller
    {

        ApplicationDbContext _context;
        AppSettings _appsettings;

        public NotificationsService(ApplicationDbContext context, AppSettings appsettings)
        {
            _context = context;
            _appsettings = appsettings;
        }



        public JsonResult register(RegistrationObject obj)
        {
            try
            {

                var conn = _context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                using (var command = conn.CreateCommand())
                {
                    string sql = "exec spDeviceRegister @tokenType = '{0}', @token = '{1}'";
                    sql = String.Format(sql, obj.tokenType, obj.token);

                    command.CommandText = sql;
                    command.CommandType = System.Data.CommandType.Text;
                    string rv = command.ExecuteScalar().ToString();

                    if(rv != "OK") return new JsonResult("Error");

                    return new JsonResult("OK");
                }

            }
            catch (Exception ex)
            {
                return new JsonResult("Error: " +  ex.Message);
            }
            
        }


        public class RegistrationObject
        {
                public string tokenType { get; set; }
                public string token { get; set; }
        }


    }

}

