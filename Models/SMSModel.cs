using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_Service.Models
{
    public class SmsRequest
    {
        public int Count { get; set; }
        public List<Sms> Smslist { get; set; }
    }

    public class Sms
    {
        public string PartnerID { get; set; }
        public string Apikey { get; set; }
        public string Pass_Type { get; set; }
        public int Clientsmsid { get; set; }
        public string Mobile { get; set; }
        public string Message { get; set; }
        public string Shortcode { get; set; }
    }
}
