using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using log4net;


namespace Snow_RestApi.Helpers
{
    public class SnowClient
    {
        public string ApiUrl { get; set; }
        public string OutputFilesFormat { get; set; }
        public int Counter { get; set; }
        public string Credentials { get; set; }

        private HttpClient client;

        /// <summary>
        /// log4net setter
        /// </summary>
        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private bool StoreUrlToFile(string url, string filename)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.ToString());
            request.Method = "GET";
            request.ContentType = "application/json";

            client = new HttpClient();
            client.BaseAddress = new System.Uri(url.ToString());

            byte[] cred = UTF8Encoding.UTF8.GetBytes(Credentials);
            client.DefaultRequestHeaders
                .Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(cred));
            client.DefaultRequestHeaders
                .Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage message = client.GetAsync(url.ToString()).Result;

            if (message.IsSuccessStatusCode)
            {
                // string result = message.Content.ReadAsStringAsync().Result;

                // store to file
                using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    /* await */ // in async code use await in sync code use Wait
                    Task task = message.Content.CopyToAsync(fs);
                    task.Wait();
                }
            }
            else if (message.StatusCode == HttpStatusCode.NotFound) // API return 404 in case of no records found
            {
                /*                var logMessage = $"{result}";
                                Log.Warn(logMessage);*/
            }
            else // another error occured
            {
                // add addition error handling
                var logMessage = $"HTTP error: {message.StatusCode} {message.ReasonPhrase}";
                Log.Error(logMessage);
            }

            return message.IsSuccessStatusCode;
        }

        private string GetUrlForSysIdIteration(string apiUrl, int perRequest, string fromId, string toId)
        {
            StringBuilder urlFor = new StringBuilder();
            urlFor.Append(apiUrl);
            urlFor.Append(String.Format("?sysparm_query=sys_id>{0}^sys_id<{1}^ORDERBYsys_id", fromId, toId));
            urlFor.Append(String.Format("&sysparm_limit={0}",
                perRequest));

            return urlFor.ToString();
        }

        public (bool success, int count, string lastSysId) GetData(string apiUrl, string fromId, string toId, int requestId)
        {
            bool result = false;
            Counter = 0;
            int perRequest = 5000;
            int maxRequests = 500;
            int attemptsCount = 3;

            // iterate over the data until retrieve count is same as items pre request (problem is if more than items_per_request is on the same second updated
            // maxRequests limits bulk numbers
            // perRequest limits items per bulk
            // repeatCount limits repeating downloading same bulk. after reaching limits stops requesting whole GetData thread.
            // if repeatCount limit is reached, try to decrease perRequest param, or number of paralel threads.

            int rows = 0;
            var parseResult = (count: 0, updateOn: "", sysId: fromId);

            // success or error will be set after success: retrieving last bulk, error: spend all retry
            bool success = false;
            bool error = false;

            for (int i = 0; !success && !error && i < maxRequests; i++) // weird/unexpected for loop ... extended condition for better reading only.
            {

                string filename = String.Format(OutputFilesFormat, Process.GetCurrentProcess().Id, requestId, i);
                // this is tasks, not separate threads ... do not use Thread.CurrentThread.ManagedThreadId,
                // requestId is ok.
                result = true;
                string url;
                string logMessage = $"";

                url = GetUrlForSysIdIteration(apiUrl, perRequest, parseResult.sysId, toId);

                logMessage = $"Thread '{Thread.CurrentThread.ManagedThreadId}' url: {url.ToString()}";
                Log.Info(logMessage);

                // repeating requests attempts
                int attemptNo = 0;
                do
                {
                    if (attemptNo > 0)
                    {
                        logMessage = $"Retry data attempt: {attemptNo} from url: {url}";
                        Console.WriteLine(logMessage);
                        Log.Warn(logMessage);
                    }

                    result = StoreUrlToFile(url.ToString(), filename);

                    // after fetching data continue parsing
                    if (result)
                    {
                        var newParseResult = ParseFile(filename);
                        result = newParseResult.success;
                        if (newParseResult.success)
                        {
                            logMessage = $"Parsing: {filename} produce records: {newParseResult.count}";
                            Log.Info(logMessage);

                            if (newParseResult.count > 0)
                            {
                                rows += newParseResult.count;
                                parseResult.count = newParseResult.count;
                                parseResult.sysId = newParseResult.sysId;
                                parseResult.updateOn = newParseResult.updateOn;
                            }
                            else
                            {
                                parseResult.count = 0;
                                logMessage = $"Parsing: {filename} produce records: {newParseResult.count} possibly last data was retrieved.";
                                Log.Info(logMessage);
                                success = true; // stop loop and finish with success
                            }

                        }
                        else
                        {
                            // continue with retrieving new data
                            Log.Warn("we need correct data");
                        }

                    }
                    attemptNo++;
                } while (!result && attemptNo < attemptsCount);

                if (!result)
                {
                    logMessage = $"Failed download data, repeatCount: {attemptsCount}. pls, change bulk size.";
                    Log.Error(logMessage);
                    error = true; // stop loop and finish with error
                }

            }
            var logmessage = $"Thread '{Thread.CurrentThread.ManagedThreadId}' fetched rows count: {rows}";
            Log.Info(logmessage);
            return (success, rows, parseResult.sysId);
        }

        public (bool success, int count, string updateOn, string sysId) ParseFile(string filename)
        {
            var result = (success: false, count: 0, updateOn: "", sysId: "");
            JObject o1 = new JObject();
            var logMessage = "";

            try
            {
                o1 = JObject.Parse(File.ReadAllText(filename));
            }
            catch (JsonReaderException e)
            {
                // handle JsonReader exception, solution requires new data (except some memory issues)
                // TODO: analyze possible error, and proposed solution retry parsing, or retry retrieving
                logMessage = $"error parse file: {filename} JSONReader: {e.Message}";
                Log.Error(logMessage);
                Console.WriteLine(logMessage);
                return result;
            }

            var error = o1["error"];
            if (error != null && error.HasValues)
            {
                // result with error requires new data
                result.success = false;
                result.count = 0;

                logMessage = $"error in result file: {error["detail"]}";
                Log.Error(logMessage);

                return result;
            }

            var results = o1["result"].Children().ToArray();
            if (results.Length > 0)
            {
                // regular result parsed
                var lastRow = o1["result"].Children().Last();

                result.success = true;
                result.count = results.Length;
                result.updateOn = lastRow["sys_updated_on"].ToString();
                result.sysId = lastRow["sys_id"].ToString();
            }
            else
            {
                // empty result parsed
                result.success = true;
                result.count = 0;
            }

            logMessage = $"Thread '{Thread.CurrentThread.ManagedThreadId}' bulk fetched... count: {result.count,4:.#} lastSysId: {result.sysId}";
            Console.WriteLine(logMessage);
            Log.Info(logMessage);
            return result;
        }
    }
}

