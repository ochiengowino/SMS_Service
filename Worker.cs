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
            return new System.ServiceModel.EndpointAddress("http://192.168.1.21:1002/test/WS/kssl/Codeunit/MSACCO");
        }

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);

                try
                {
                    var responses = new List<Sms>();
                    // Fetch data from Navision
                    MSACCO_PortClient service = new MSACCO_PortClient(GetBinding(), GetAddress());
                    service.ClientCredentials.Windows.ClientCredential = cd;

                    var sms = service.GetSMSList(5);

                    var messages = sms.Split(';');


                    foreach (var message in messages)
                    {
                        if (string.IsNullOrWhiteSpace(message))
                            continue;

                        var fields = message.Split('|');

                        var smsResponse = new Sms

                        {
                            PartnerID = "12345",
                            Apikey = "6565b5a73b8221",
                            Pass_Type = "plain",
                            Clientsmsid = int.Parse(fields[2]),
                            Mobile = fields[3],
                            Message = fields[4],
                            Shortcode = "Advanta"

                        };

                        responses.Add(smsResponse);

                    }

                    var ret = new SmsRequest
                    {
                        Count = 5,
                        Smslist = responses

                    };
                    this.WriteLog(MethodBase.GetCurrentMethod().Name, "Data From Nav to Provider: "+$"-- {ret}");

                    var client = _httpClientFactory.CreateClient();
                    var url = "https://quicksms.advantasms.com/api/services/sendbulk/";
                    var content = new StringContent(JsonConvert.SerializeObject(ret), Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        this.WriteLog(MethodBase.GetCurrentMethod().Name, "Success: Data Sent To Provider");
                        //  _logger.LogInformation("SMS data posted successfully.");
                        Console.WriteLine("SMS data posted successfully");
                    }
                    else
                    {
                        this.WriteLog(MethodBase.GetCurrentMethod().Name, "Error: Data Not Sent To Provider");
                        Console.WriteLine("Failed to post SMS data.");
                        //_logger.LogError("Failed to post SMS data.");
                    }

                }
                catch (Exception ex)
                {
                    this.WriteLog(MethodBase.GetCurrentMethod().Name, "Server Error: " + $"-- {ex.Message}");
                    Console.WriteLine("Error fetching or sending data: " + ex.Message);
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            /*      while (!stoppingToken.IsCancellationRequested)
                  {
                      _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                      await Task.Delay(1000, stoppingToken);
                  }*/
        }

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