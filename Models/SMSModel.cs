using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_Service.Models
{
    public class SmsRequest
    {
        public int count { get; set; }
        public List<Sms> smslist { get; set; }
    }

    public class Sms
    {
        public string partnerID { get; set; }
        public string apikey { get; set; }
        public string pass_type { get; set; }
        public int clientsmsid { get; set; }
        public string mobile { get; set; }
        public string message { get; set; }
        public string shortcode { get; set; }
    }

    public class SmsLogs
    {

        public int clientsmsid { get; set; }
        public string mobile { get; set; }
        public string message { get; set; }
     
    }
}
