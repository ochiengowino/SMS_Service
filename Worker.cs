using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using NavWS;
using Newtonsoft.Json;
using SMS_Service.Models;

namespace SMS_Service
{
    public class Worker : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Worker> _logger;
        private readonly HttpClient _httpClient;
        public static string logpath;
        private static NetworkCredential cd = new NetworkCredential("mpesactob", "$MyProtect@2018#");

        public static System.ServiceModel.Channels.Binding GetBinding()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            binding.MaxReceivedMessageSize = 7500000;
            return binding;

        }

        public static System.ServiceModel.EndpointAddress GetAddress()
        {
              return new System.ServiceModel.EndpointAddress("http://192.168.1.109:8148/MobileBanking/WS/kssl/Codeunit/MSACCO");
           // return new System.ServiceModel.EndpointAddress("http://192.168.1.21:1002/test/WS/kssl/Codeunit/MSACCO");
        }

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, HttpClient httpClient)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _httpClient = httpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
            
                this.WriteLog(MethodBase.GetCurrentMethod().Name, "---------------- The SMS Service has started running at: {time}"+ DateTimeOffset.Now+"---------------------");

                try
                {
                    // Fetch data from Navision
                    MSACCO_PortClient service = new MSACCO_PortClient(GetBinding(), GetAddress());
                    service.ClientCredentials.Windows.ClientCredential = cd;

                    var responses = new List<Sms>();
                    var logResponses = new List<SmsLogs>();

                    var sms = service.GetSMSList(5);
                    //var sms = "0|Success|2882125|254741112070|Test bulk sms - logs just ignore|InternetBanking;0|Success|2882126|254741112070|Test bulk sms|InternetBanking;";
                    var messages = sms.Split(';');
                    
                    foreach (var message in messages)
                    {
                        if (string.IsNullOrWhiteSpace(message))
                            continue;

                        var fields = message.Split('|');
                        this.WriteLog(MethodBase.GetCurrentMethod().Name, "SMS from Navision to the Provider: " + $"--- {("ClientID: "+int.Parse(fields[2]), "Phone Number: "+fields[3], "Message: "+fields[4])}");
                                           

                        //SMS details from Nav sent to the Provider
                        var smsResponse = new Sms

                        {
                            partnerID = "10735",
                            apikey = "823b0508951c8f6cde821340c5fe64db",
                            pass_type = "plain",
                            clientsmsid = int.Parse(fields[2]),
                            mobile = fields[3],
                            message = fields[4],
                            shortcode = "KIMISITU"

                        };
                       
                        responses.Add(smsResponse);
                    }

                    var smsBody = new SmsRequest
                    {
                        count = 5,
                        smslist = responses
                    };                 
                                     
                    var url = "https://quicksms.advantasms.com/api/services/sendbulk/";
                    var content = new StringContent(JsonConvert.SerializeObject(smsBody), Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {                    
                        this.WriteLog(MethodBase.GetCurrentMethod().Name, "Success: Response from the Provider" + $"--- {JsonConvert.SerializeObject(response.Content.ReadAsStringAsync().Result)}"); 
                    }
                    else
                    {
                        this.WriteLog(MethodBase.GetCurrentMethod().Name, "Error: SMS Not Sent to the Provider");
                    }
                }
                catch (Exception ex)
                {
                    this.WriteLog(MethodBase.GetCurrentMethod().Name, "Server Error: " + $"-- {ex.Message}");                 
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            this.WriteLog(MethodBase.GetCurrentMethod().Name, "------------ The SMS service has started stopping at: {time}"+ DateTimeOffset.Now+"-------------");
            await base.StopAsync(stoppingToken);
        }


        // Logging Functions
        public static void LogEntryOnFile(string clientRequest)
        {
            System.IO.File.AppendAllText(Worker.LogFileName, clientRequest + "\n");
        }

        public static string LogFileName
        {
            get
            {
                if (!Directory.Exists(Worker.logpath))
                    Directory.CreateDirectory(Worker.logpath);
                string[] strArray1 = new string[5]
                {
                  Worker.logpath,
                  null,
                  null,
                  null,
                  null
                };
                DateTime now1 = DateTime.Now;
                string[] strArray2 = strArray1;
                int num = now1.Year;
                string str1 = num.ToString();
                strArray2[1] = str1;
                DateTime now2 = DateTime.Now;
                string[] strArray3 = strArray1;
                num = now2.Month;
                string str2 = num.ToString();
                strArray3[2] = str2;
                DateTime now3 = DateTime.Now;
                string[] strArray4 = strArray1;
                num = now3.Day;
                string str3 = num.ToString();
                strArray4[3] = str3;
                strArray1[4] = ".txt";
                return string.Concat(strArray1);
            }
        }

        public void WriteLog(string FunctionName, string ErrorMessage)
        {
            try
            {
                string LogPath = @"C:\\SMSLogs";
                if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);
                string LogFileName = Path.Combine(LogPath, $"{DateTime.Now:yyyy-MMM-dd}.txt");
                System.IO.File.AppendAllText(LogFileName, $"{DateTime.Now:yyyy-MMM-dd HH:mm:ss}: {FunctionName} => {ErrorMessage} {Environment.NewLine}");
            }
            catch (Exception ex)
            {
            }
        }

        public void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                TextWriter textWriter = txtWriter;
                DateTime now = DateTime.Now;
                string longTimeString = now.ToLongTimeString();
                now = DateTime.Now;
                string longDateString = now.ToLongDateString();
                textWriter.WriteLine("{0} {1}", (object)longTimeString, (object)longDateString);
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  :{0}", (object)logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception ex)
            {
            }
        }
    }
}