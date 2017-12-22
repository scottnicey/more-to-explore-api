using Microsoft.AspNetCore.Mvc;
using System;
using MTEAPI.Models;
using MTEAPI.Data;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System.Threading.Tasks;

namespace MTEAPI.Services
{
    public class FeedbackService : Controller
    {

        ApplicationDbContext _context;
        AppSettings _appsettings;

        public FeedbackService(ApplicationDbContext context, AppSettings appsettings)
        {
            _context = context;
            _appsettings = appsettings;
        }



        public async Task<JsonResult> createFeedbackAsync(FeedbackData fd)
        {
            try
            {
                JsonResult rv = await sendEmail(fd);
                return rv;
            }
            catch(Exception ex)
            {
                return new JsonResult("Error: " + ex.ToString());
            }
            /*
            try
            {
                var conn = _context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                using (var command = conn.CreateCommand())
                {
                    string sql = "exec spFeedbackSave @platform = '{0}', @feedbackObjectType = '{1}', @feedbackObjectId = '{2}', @comment = '{3}', @feedbackType = '{4}";
                    sql += ", @photo1 = '{5}', @photo1 = '{6}', @photo1 = '{7}'";

                    string p1 = "";
                    string p2 = "";
                    string p3 = "";

                    sql = String.Format(sql, fd.platform, fd.feedbackObjectType, fd.feedbackObjectId, fd.comment, fd.feedbackType, p1, p2, p3);


                    await sendEmail("snicey@bigpond.net.au", "test MTE", "The body of the message");

                    //command.CommandText = sql;
                    //command.CommandType = System.Data.CommandType.Text;
                    //string rv = command.ExecuteScalar().ToString();

                    //if (rv != "OK") return new JsonResult("Error");

                    return new JsonResult("OK");
                }


                return new JsonResult("OK");
            }
            catch (Exception ex)
            {
                return new JsonResult("Error");
            }
            */

        }

        public class FeedbackData
        {
            public string platform { get; set; }
            public string feedbackObjectType { get; set; }
            public string feedbackObjectId { get; set; }
            public string comment { get; set; }
            public string feedbackType { get; set; }
            public string feedbackName { get; set; }
            public string photo1 { get; set; }
            public string photo2 { get; set; }
            public string photo3 { get; set; }
        }

        public async Task<JsonResult> sendEmail(FeedbackData fd)
        {
            try
            {
                var emailMessage = new MimeMessage();

                string subject = "MTE Feedback";
                if (!String.IsNullOrEmpty(fd.feedbackName)) subject += " for " + fd.feedbackName;

                string body = "Feedback from the iOS app has been submitted.\n\n\n";

                if (!String.IsNullOrEmpty(fd.feedbackName) || !String.IsNullOrEmpty(fd.feedbackObjectId))
                {
                    body += "Feature:\n" + fd.feedbackName + " - " + fd.feedbackObjectType + " #" + fd.feedbackObjectId + "\n\n";
                }
                body += "Feedback Type:\n" + fd.feedbackType + "\n\n";
                body += "Comment:\n" + fd.comment + "\n\n";


                emailMessage.From.Add(new MailboxAddress("MTE API Application", _appsettings.feedbackMailFrom));


                string to = _appsettings.feedbackMailTo + ";";
                to = to.Replace(",", ";").Replace("|", ";");

                foreach(string s in to.Split(';'))
                {
                    if (s.Trim().Length > 0)
                    {
                        MailboxAddress mb = null;
                        if(MailboxAddress.TryParse(s, out mb)) emailMessage.To.Add(mb);
                    }
                }

                string cc = _appsettings.feedbackMailCC + ";";
                cc = cc.Replace(",", ";").Replace("|", ";");

                foreach (string s in cc.Split(';'))
                {
                    if (s.Trim().Length > 0)
                    {
                        MailboxAddress mb = null;
                        if (MailboxAddress.TryParse(s, out mb)) emailMessage.Cc.Add(mb);
                    }
                }


                emailMessage.Subject = subject;

                var builder = new BodyBuilder { TextBody = body };
                if (fd.photo1 != null) builder.Attachments.Add(fd.photo1);
                if (fd.photo2 != null) builder.Attachments.Add(fd.photo2);
                if (fd.photo3 != null) builder.Attachments.Add(fd.photo3);

                emailMessage.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    int port = 25;
                    int.TryParse(_appsettings.smtpPort, out port);
                    SecureSocketOptions secOpt = new SecureSocketOptions();
                    secOpt = SecureSocketOptions.None;
                    if (_appsettings.smtpSecurity.ToUpper() == "TLS") secOpt = SecureSocketOptions.StartTls;
                    if (_appsettings.smtpSecurity.ToUpper() == "SSL") secOpt = SecureSocketOptions.SslOnConnect;
                    if (_appsettings.smtpSecurity.ToUpper() == "AUTO") secOpt = SecureSocketOptions.Auto;

                    await client.ConnectAsync(_appsettings.smtpServer, port, secOpt).ConfigureAwait(false);
                    if (!String.IsNullOrEmpty(_appsettings.smtpUsername))
                    {
                        await client.AuthenticateAsync(_appsettings.smtpUsername, _appsettings.smtpPassword);
                    }
                    await client.SendAsync(emailMessage).ConfigureAwait(false);
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }

                return new JsonResult("OK");
            }
            catch(Exception ex)
            {
                return new JsonResult("Error: " + ex.ToString());
            }
        }
    }   

}



