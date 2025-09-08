using MirrorRepository.Data.SnowDbSyncMgnt;
using System.Net;
using System.Reflection;
using log4net;

namespace MirrorRepository.SnowTableApi
{
    public class ApiClient
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        /// <summary>
        /// Gets or sets the InstanceName value
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets the TableName value
        /// </summary>
        protected string TableName { get; set; }
        
        /// <summary>
        /// Gets or sets the WebClient values
        /// </summary>
        protected WebClient ServiceNowClient { get; set; }

        /// <summary>
        /// Gets the Service Now InstanceUrl
        /// </summary>
        protected virtual string InstanceUrl => $"https://{InstanceName}.service-now.com/{TableName}.do";

        /// <summary>
        /// setup instance connection
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="instanceSettings"></param>
        public ApiClient(string tableName, InstanzSettings instanceSettings)
        {
            string proxyUrl = null;
            var credentials = new NetworkCredential(instanceSettings.UserName, instanceSettings.Password );
            
            ICredentials proxyCredentials = null;
            if (!string.IsNullOrEmpty(instanceSettings.ProxyServer))
            {
                proxyUrl = $"http://{instanceSettings.ProxyServer}:{(instanceSettings.ProxyPort != null && instanceSettings.ProxyPort > 16 ? instanceSettings.ProxyPort : 8080)}";
            }
            if (!string.IsNullOrEmpty(instanceSettings.ProxyUserName))
            {
                proxyCredentials = new NetworkCredential(instanceSettings.ProxyUserName, instanceSettings.ProxyUserPassword);
            }

            //Initialize the Web Client with proxy
            WebProxy proxy = new WebProxy(proxyUrl, true)
            {
                Credentials = proxyCredentials
            };

            TableName = tableName;
            InstanceName = instanceSettings.InstanzName;

            ServiceNowClient = new WebClient { Credentials = credentials, Proxy = proxy };

        }

        
    }
}
