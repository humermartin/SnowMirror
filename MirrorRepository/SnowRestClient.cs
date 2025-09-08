using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Snow_RestApi.Helpers;

namespace Snow_RestApi
{
    public class FetchRequest
    {
        public bool IsSucceeded { get; set; }
        public string FromId { get; set; }
        public string ToId { get; set; }
        public string LastId { get; set; }
        public int RequestId { get; set; }
    }

    public class Program
    {

        /// <summary>
        /// log4net setter
        /// </summary>
        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);


        Task InitTask(string fromId, string toId, int RequestId)
        {
            Task<FetchRequest> task = Task<FetchRequest>.Factory.StartNew(() =>
            {
                var message = $"Task '{Thread.CurrentThread.ManagedThreadId}' processing ({fromId}-{toId}>)";
                Log.Info(message);
                Console.WriteLine(message);

                SnowGetData(fromId, toId, RequestId);
                return new FetchRequest() { FromId = "", ToId = "", IsSucceeded = false, LastId = "" };
            });

            return task;
        }

        static void Main(string[] args)
        {
            new Program().Execute(args);
        }
        void Execute(string[] args)
        {

            List<Task> tasks = new List<Task>();
            List<FetchRequest> requests = new List<FetchRequest>();

            for (int i = 0; i < 16; i++)
            {
                requests.Add(new FetchRequest
                {
                    FromId = String.Format("{0}{1}", (char)(i > 9 ? 'a' + i - 10 : '0' + i), new String('0', 31)),
                    ToId = String.Format("{0}{1}", (char)(i > 9 ? 'a' + i - 10 : '0' + i), new String('f', 31)),
                    RequestId = i

                }
                );
            }

            int maxWorkers = 8;
            foreach (FetchRequest request in requests)
            {
                // List<Task> filtered = tasks.Where(t => { return (t.Status == TaskStatus.Running) });
                while (tasks.Where(t => { return (t.Status == TaskStatus.Running); }).ToList().Count() >= maxWorkers)
                {
                    Task.WaitAny(tasks.ToArray());
                }

                // conditional is useless ... but ...
                if (tasks.Where(t => { return (t.Status == TaskStatus.Running); }).ToList().Count() < maxWorkers)
                {
                    tasks.Add(InitTask(request.FromId, request.ToId, request.RequestId));
                    Thread.Sleep(1000);
                }
            }

            var logMessage = "All Tasks started. Wait to finish last workers.";
            Console.WriteLine(logMessage);
            Log.Info(logMessage);
            Task.WaitAll(tasks.ToArray());

            logMessage = "All Tasks finished.";
            Log.Info(logMessage);
            Console.WriteLine(logMessage);
            // Console.ReadLine();
        }

        public FetchRequest SnowGetData(string fromId, string toId, int requestId)
        {
            SnowClient client;

            client = new SnowClient
            {
                ApiUrl = "https://a1prod.service-now.com/api/now/table/cmdb_ci",
                Credentials = "Q396230:********",
                OutputFilesFormat = @"c:\temp\snowget.{0}-{1}.{2,4:0000.#}.json", // pid, threadid, third is for counter, starting from zero

            };

            // after some experience add additional args to GetData
            // get data used TableApi and params to define limit, columns, query to specify data to retrieve (class...)
            var getDataResult = client.GetData(client.ApiUrl, fromId, toId, requestId);
            var message = $"finished: { getDataResult.success}";
            Log.Info(message);
            Console.WriteLine(message);
            return new FetchRequest
            {
                IsSucceeded = getDataResult.success,
                RequestId = requestId,
                FromId = fromId,
                ToId = toId,
                LastId = getDataResult.lastSysId
            };
        }
    }
}

