using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTEAPI.Models
{
    public class AppSettings
    {
        public string DataLocation { get; set; }
        public string FeedbackImageLocation { get; set; }
        public string ImageLocation { get; set; }
        public string ImageLocationNews { get; set; }
        public string BaseURL { get; set; }
        public string ImageFolder { get; set; }
        public string feedbackMailFrom { get; set; }
        public string feedbackMailTo { get; set; }
        public string feedbackMailCC { get; set; }
        public string smtpServer { get; set; }
        public string smtpUsername { get; set; }
        public string smtpPassword { get; set; }
        public string smtpPort { get; set; }
        public string smtpSecurity { get; set; }

    }
}
