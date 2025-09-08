using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using log4net;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using MirrorRepository.Model.ESB;
using MirrorRepository.Model.Kafka;

namespace MirrorRepository.REST
{
    public class EsbClient : IClient<EsbClient>, IDisposable
    {

        public enum Esb_Service_Paths { IncidentUpdated };

        public static readonly string SNOW_DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ss";
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        public static String A1_PROXY { get; set; } = "http://proxy.austria.local:8080/";
        public static ICredentials ProxyCredentials { private get; set; }

        public const int MAX_TIMEOUT = 30;
        public string BaseUrl { get; private set; } = "https://esb-e.a1telekom.inside:8463/eai-event-facade/event/ServicenowCMDBEvents/a1ta-dev/1.0";
        public string Username { get; private set; }
        private string Password { get; set; }
        public int Timeout { get; set; } = MAX_TIMEOUT;

        HttpClient client { get; set; }
        WebClient webClient { get; set; }
        public bool UseWebClient { get; set; }
        public CookieContainer Cookies { get; protected set; } = new CookieContainer();
        public static JsonSerializer JsonSerializer
        {
            get
            {
                return new JsonSerializer
                {
                    DateFormatString = SNOW_DATETIME_FORMAT
                };
            }
        }

        public T Deserialize<T>(string value)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(new JsonTextReader(new StringReader(value)));
            }
            catch (Exception e)
            {
                Log.Debug($"{MethodBase.GetCurrentMethod()?.Name}: cannot deserialize value: {value}. Exception: {e.Message}, {e.GetBaseException()}");
                throw;
            }
        }

        public String Serialize<T>(T value)
        {
            try
            {
                return Kafka.AsString(value);
            }
            catch (Exception e)
            {
                Log.Debug($"{MethodBase.GetCurrentMethod()?.Name}: cannot deserialize value: {value}. Exception: {e.Message}, {e.GetBaseException()}");
                throw;
            }
        }

        public List<Cookie> SnowNodeCookies()
        {
            return GetCookies().Where(c => !string.IsNullOrEmpty(c.Name)).ToList();
        }
        public List<Cookie> GetCookies()
        {
            var cookies = new List<Cookie>();
            if (Cookies == null) return cookies;
            var ce = Cookies.GetCookies(client.BaseAddress).GetEnumerator();
            while (ce.MoveNext()) cookies.Add((Cookie)ce.Current);
            return cookies;
        }

        public EsbClient New()
        {
            return Build(BaseUrl, Username, Password,
                useDefaultCredentials: ProxyCredentials == null, credentials: ProxyCredentials,
                timeout: Timeout);
        }

        public static EsbClient Build(string URL, string username, string password, int timeout = MAX_TIMEOUT, bool useWebClient = false, bool useDefaultCredentials = true, ICredentials credentials = null)
        {
            try
            {
                if (credentials == null)
                {
                    credentials = ProxyCredentials;
                }

                WebProxy proxy = new WebProxy(A1_PROXY, true)
                {
                    Credentials = credentials
                };

                if (!URL.EndsWith("/")) URL += "/"; // must end with "/" !!!

                EsbClient rc = new EsbClient() { BaseUrl = URL, Username = username, Password = password, UseWebClient = useWebClient };

                var handler = new HttpClientHandler
                {
                    //Proxy = proxy, //new System.Net.WebProxy(A1_PROXY),
                    UseProxy = false,//proxy != null,
                    Credentials = new NetworkCredential(username, password),
                    CookieContainer = rc.Cookies,
                    ClientCertificateOptions = ClientCertificateOption.Manual
                };
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };


                if (rc.client == null)
                {
                    HttpClient client = new HttpClient(handler);
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;
                    client.Timeout = TimeSpan.FromSeconds(timeout);
                    client.BaseAddress = new Uri(URL);
                    var byteArray = new UTF8Encoding().GetBytes(username + ":" + password);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    // Add an Accept header for JSON format.
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    rc.client = client;

                    // while working with current user:
                    rc.webClient = new WebClient
                    {
                        BaseAddress = URL,
                        Credentials = new NetworkCredential(username, password),
                    };
                }
                return rc;
            }
            catch (Exception e)
            {
                Log.Info("cannot build EsbClient: " + e.GetBaseException(), e);
                throw new Exception("cannot build EsbClient: " + e.GetBaseException().Message, e);
            }
        }

        readonly int MAX_RETRY = 3;
        public IncidentUpdatedResponse Write(Esb_Service_Paths path, IncidentUpdatedRequest request, string sysId, int blockCount = 0)
        {
            //try { Thread.ResetAbort(); } catch { } // just for fun??!!
            for (int i = 0; i < MAX_RETRY; i++)
            {
                try
                {
                    try
                    {
                        HttpStatusCode responseStatus = WriteInternal(path.ToString(), Serialize(request), sysId, blockCount);
                        if (responseStatus != HttpStatusCode.OK)
                        {
                            throw new Exception("ESB failed: " + responseStatus);
                        }
                        return new IncidentUpdatedResponse() { status = (int)responseStatus};
                    }
                    catch
                    {
                        try { Thread.ResetAbort(); } catch { } // just for fun??!!
                    }
                }
                catch
                {
                    if (i < MAX_RETRY - 1)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    throw;
                }
            }
            throw new Exception("too many aborts - cannot read " + request);
        }

        /// <summary>
        /// Write data
        /// </summary>
        /// <param name="path"></param>
        /// <param name="payload"></param>
        /// <param name="sysId"></param>
        /// <param name="blockCount"></param>
        /// <returns></returns>
        public IncidentUpdatedResponse WriteData(Esb_Service_Paths path, string payload, string sysId, int blockCount = 0)
        {
            //try { Thread.ResetAbort(); } catch { } // just for fun??!!
            for (int i = 0; i < MAX_RETRY; i++)
            {
                try
                {
                    try
                    {
                        HttpStatusCode responseStatus = WriteInternal(path.ToString(), payload, sysId, blockCount);
                        if (responseStatus != HttpStatusCode.OK)
                        {
                            throw new Exception("ESB failed: " + responseStatus);
                        }
                        return new IncidentUpdatedResponse() { status = (int)responseStatus };
                    }
                    catch
                    {
                        try { Thread.ResetAbort(); } catch { } // just for fun??!!
                    }
                }
                catch
                {
                    if (i < MAX_RETRY - 1)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    throw;
                }
            }
            throw new Exception("too many aborts - cannot read " + payload);
        }

        public HttpStatusCode WriteInternal(string path, string request, string sysId, int blockCount = 0)
        {

            Log.Debug($"writing: {request} for sysId=" + sysId);
            try
            {
                string content;
                if (UseWebClient && webClient != null)
                {
                    content = webClient.UploadString(BaseUrl + path, request);
                    Log.Debug($"read: {request}");
                    return HttpStatusCode.OK; // HTTP.OK
                }
                else
                {
                    try
                    {
                        var task = client.PostAsync(BaseUrl + path, new StringContent(request, UTF8Encoding.UTF8, "application/json"));
                        HttpResponseMessage response = task.Result;
                        Log.Debug($"read: {(int) response.StatusCode} ({response.ReasonPhrase}) from Url: {BaseUrl + path}, req={request} for sysId=" + sysId);                            

                        if (response.IsSuccessStatusCode)
                        {
                            // Parse the response body.
                            //Make sure to add a reference to System.Net.Http.Formatting.dll
                            Log.Info($"successfully sent {blockCount} records to Kafka. response: {(int)response.StatusCode} ({response.ReasonPhrase}) from Url: {BaseUrl + path}");
                            return response.StatusCode;
                        }
                        
                        var msg = $"failed to read: {(int)response.StatusCode} ({response.ReasonPhrase})";
                        throw new Exception(msg);
                        
                    }
                    catch (AggregateException ae)
                    {
                        var msg = $"cannot request: base={client.BaseAddress} req={request} : exc={ae.GetBaseException().Message}";
                        Log.Info(msg);
                        Log.Debug(msg, ae.GetBaseException());
                        throw ae.GetBaseException();
                    }
                }
            }
            catch (Exception e)
            {
                try { Thread.ResetAbort(); } catch { }
                var msg = $"cannot request: base={client.BaseAddress} req={request} : exc={e.GetBaseException().Message}";
                Log.Info(msg);
                Log.Debug(msg, e.GetBaseException());
                throw;
            }
        }


        public void Dispose()
        {
        }

    }

}
