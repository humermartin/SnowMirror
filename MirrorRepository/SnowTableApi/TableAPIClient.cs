using Newtonsoft.Json;
using log4net;
using MirrorRepository.Interfaces;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.SnowTableApi.TableDefinitions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using System.Xml.Serialization;
using MirrorRepository.Model;
using MirrorRepository.Model.RecordCount;
using MirrorRepository.Model.SnowDbSyncMgnt;

namespace MirrorRepository.SnowTableApi
{
    /// <summary>
    /// Client for the ServiceNow REST TableAPI.
    /// </summary>
    /// <typeparam name="T">Data type to retrieve for each row retrieved from the ServiceNow table.<para />
    /// Property names in the supplied type must match ServiceNow field names to be retrieved in a record from the specified table.</typeparam>
    public class TableApiClient<T> : ApiClient, ITableApiClient<T> where T : Record
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        

        /// <summary>
        /// Gets the Service Now InstanceTableApiUrl
        /// </summary>
        protected string InstanceTableApiUrl => $"https://{InstanceName}.service-now.com/api/now/table/{TableName}";

        /// <summary>
        /// Gets or sets the InstanceUrl
        /// </summary>
        protected override string InstanceUrl => "https://" + InstanceName + ".service-now.com/";

        public TableApiClient(string tableName, InstanzSettings settings) : base(tableName, settings)
        { 
        }
        
        /// <summary>
        /// build url table field list
        /// </summary>
        /// <returns></returns>
        protected string BuildFieldList()
        {
            //Build the field list from the record type that will be retrieved
            string fieldList = "";
            Type i = typeof(T);
            foreach (var prop in i.GetProperties())
            {
                // We need to build the field list using the JsonProperty attributes since those strings can contain our dot notation.
                var field = prop.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "JsonPropertyAttribute");
                if (field != null)
                {
                    CustomAttributeTypedArgument fieldName = field.ConstructorArguments.FirstOrDefault(x => x.ArgumentType.Name == "String");
                    if (fieldList.Length > 0)
                    {
                        fieldList += ",";
                    }
                    fieldList += fieldName.Value;
                }
            }

            return fieldList;
        }

        /// <summary>
        /// Respone parse exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string ParseWebException(WebException ex)
        {
            string message = ex.Message + "\n\n";

            if (ex.Response != null)
            {
                var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                dynamic obj = JsonConvert.DeserializeObject(resp);

                message = "status: " + obj.status + "\n";
                message += ex.Message + "\n\n";
                message += "message: " + obj.error.message + "\n";
                message += "detail: " + obj.error.detail + "\n";
            }
            
            return message;
        }

        /// <summary>
        /// Retrieves a single record contained in the RESTSingleResponse of the type T defined for the table client.<para />
        /// Any errors will be fully captured and returned in the ErrorMsg property of the response.
        /// </summary>
        /// <param name="id">sys_id of the record to be retrieved.</param>
        /// <returns>A RestResponse containing a single result of T (if successful) along with any error messages (if any).</returns>
        public RestSingleResponse<T> GetById(string id)
        {
            var response = new RestSingleResponse<T>();

            try
            {
                var fieldList = BuildFieldList();
                response.RawJson = ServiceNowClient.DownloadString(InstanceTableApiUrl + "/" + id + "?&sysparm_fields=" + fieldList);
            }
            catch (WebException ex)
            {
                response.ErrorMsg = ParseWebException(ex);
            }
            catch (Exception ex)
            {
                response.ErrorMsg = "An error occured retrieving the REST response: " + ex.Message;
            }

            RestSingleResponse<T> tmp = JsonConvert.DeserializeObject<RestSingleResponse<T>>(response.RawJson);
            if (tmp != null) { response.Result = tmp.Result; }

            return response;
        }

        /// <summary>
        /// Retrieves a record set in the response (as a list) based on the query result.
        /// </summary>
        /// <param name="query">A standard service-now table query.</param>
        /// <returns>A RestResponse containing a result list of T (if successful) along with any error messages (if any).</returns>
        public RestQueryResponse<T> GetByQuery(string query)
        {
            var response = new RestQueryResponse<T>();

            try
            {
                var fieldList = BuildFieldList(); 
                response.RawJson = ServiceNowClient.DownloadString(InstanceTableApiUrl + "?&sysparm_fields=" + fieldList + "&sysparm_query=" + query);
            }
            catch (WebException ex)
            {
                response.ErrorMsg = ParseWebException(ex);
            }
            catch (Exception ex)
            {
                response.ErrorMsg = "An error occured retrieving the REST response: " + ex.Message;
            }

            RestQueryResponse<T> tmp = JsonConvert.DeserializeObject<RestQueryResponse<T>>(response.RawJson);
            if (tmp != null) { response.Result = tmp.Result; }

            return response;
        }

        /// <summary>
        /// Retrieves a record set in the response (as a list) based on the query result.
        /// </summary>
        /// <param name="url">A standard service-now table query.</param>
        /// <param name="addHeader"></param>
        /// <param name="instanzSettings"></param>
        /// <returns>A RestResponse containing a result list of T (if successful) along with any error messages (if any).</returns>
        public RestQueryResponse<T> GetByFinalUrl(string url, bool addHeader = false, InstanzSettings instanzSettings = null)
        {
            var response = new RestQueryResponse<T>();

            try
            {
                if (addHeader)
                {
                    string authenticationString = $"{instanzSettings.UserName}:{BaseModel.Decryptdata(instanzSettings.Password)}";
                    var base64EncodedAuthenticationString = $"Basic {Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString))}";
                    ServiceNowClient.Headers.Add("Authorization", base64EncodedAuthenticationString);
                }
                response.RawJson = ServiceNowClient.DownloadString(url);
            }
            catch (WebException ex)
            {
                response.ErrorMsg = ParseWebException(ex);
            }
            catch (Exception ex)
            {
                response.ErrorMsg = "An error occured retrieving the REST response: " + ex.Message;
            }

            RestQueryResponse<T> tmp = JsonConvert.DeserializeObject<RestQueryResponse<T>>(response.RawJson);
            if (tmp != null) { response.Result = tmp.Result; }

            return response;
        }

        public XmlStats GetNodeStatFromUrl(string url, bool addHeader = false, InstanzSettings instanzSettings = null)
        {
            var response = new RestQueryResponse<T>();

            try
            {
                if (addHeader)
                {
                    string authenticationString = $"{instanzSettings.UserName}:{BaseModel.Decryptdata(instanzSettings.Password)}";
                    var base64EncodedAuthenticationString = $"Basic {Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString))}";
                    ServiceNowClient.Headers.Add("Authorization", base64EncodedAuthenticationString);
                }
                response.RawXml = ServiceNowClient.DownloadString(url);
            }
            catch (WebException ex)
            {
                response.ErrorMsg = ParseWebException(ex);
            }
            catch (Exception ex)
            {
                response.ErrorMsg = "An error occured retrieving the REST response: " + ex.Message;
            }
            
            XmlSerializer serializer = new XmlSerializer(typeof(XmlStats));
            using (StringReader reader = new StringReader(response.RawXml))
            {
                return (XmlStats)serializer.Deserialize(reader);
            }

        }

        /// <summary>
        /// Retrieves a record set in the response (as a list) based on the query result.
        /// </summary>
        /// <returns>A RestResponse containing a result list of T (if successful) along with any error messages (if any).</returns>
        public RestQueryResponse<T> GetFull()
        {
            var response = new RestQueryResponse<T>();

            try
            {
                var fieldList = BuildFieldList();
                response.RawJson = ServiceNowClient.DownloadString(InstanceTableApiUrl + "?&sysparm_fields=" + fieldList);
            }
            catch (WebException ex)
            {
                response.ErrorMsg = ParseWebException(ex);
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {response.ErrorMsg}");
            }
            catch (Exception ex)
            {
                response.ErrorMsg = "An error occured retrieving the REST response: " + ex.Message;
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {response.ErrorMsg}");
            }

            RestQueryResponse<T> tmp = JsonConvert.DeserializeObject<RestQueryResponse<T>>(response.RawJson);
            if (tmp != null) { response.Result = tmp.Result; }

            return response;
        }

        /// <summary>
        /// Get rowcount in tableObject
        /// </summary>
        /// <param name="snowTableList"></param>
        /// <returns></returns>
        public List<SnowTables> GetRowCount(List<SnowTables> snowTableList)
        {
            try
            {
                foreach (var table in snowTableList)
                {
                    string query = $"api/now/v1/stats/{table.Name}?sysparm_limit=1&sysparm_count=true";

                    try
                    {
                        var rawJson = ServiceNowClient.DownloadString($"{InstanceUrl}{query}");
                        RootCountResult recordObject = JsonConvert.DeserializeObject<RootCountResult>(rawJson);

                        table.RowCount = recordObject.Result.Stats.Count;
                    }
                    catch (WebException ex)
                    {
                        Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: { ((HttpWebResponse)ex.Response).StatusCode }, {((HttpWebResponse)ex.Response).ResponseUri.AbsoluteUri}");
                    }   
                    
                    
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");
                
            }

            return snowTableList;
        }

        /// <summary>
        /// update single rowcount in tableObject
        /// </summary>
        /// <param name="snowTable"></param>
        /// <returns></returns>
        public async Task<int> GetRowCount(SnowTables snowTable)
        {
            try
            {
                
                    string query = $"api/now/v1/stats/{snowTable.Name}?sysparm_limit=1&sysparm_count=true";

                    try
                    {
                        var rawJson = ServiceNowClient.DownloadString($"{InstanceUrl}{query}");
                        RootCountResult recordObject = JsonConvert.DeserializeObject<RootCountResult>(rawJson);

                        snowTable.RowCount = recordObject.Result.Stats.Count;

                    }
                    catch (WebException ex)
                    {
                        Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: { ((HttpWebResponse)ex.Response).StatusCode }, {((HttpWebResponse)ex.Response).ResponseUri.AbsoluteUri}");
                    }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");

            }

            return snowTable.RowCount;
        }
        /// <summary>
        /// Retrieves a record set in the response (as a list) based on the query result.
        /// </summary>
        /// <returns>A RestResponse containing a result list of T (if successful) along with any error messages (if any).</returns>
        public ColumnResponse GetColumns(string tableName)
        {
            try
            {
                var result = ServiceNowClient.DownloadString($"{InstanceTableApiUrl}?sysparm_fields=sys_id,sys_name,name,element,max_length,internal_type,column_label&name={tableName}&sysparm_display_value=all");
                
                if (!string.IsNullOrWhiteSpace(result))
                {
                    ColumnResponse columnResponse = JsonConvert.DeserializeObject<ColumnResponse>(result);

                    var removeEmptyValues = columnResponse.SnowColumns.Where(c => string.IsNullOrEmpty(c.Element.Value) && string.IsNullOrEmpty(c.Element.DisplayValue)).ToList();
                    if (removeEmptyValues.Any())
                    {
                        var snowTableColumns = columnResponse.SnowColumns.Except(removeEmptyValues).ToList();
                        columnResponse.SnowColumns = snowTableColumns;
                    }
                    return columnResponse;
                }

                return null;
            }
            catch (WebException ex)
            {
                var errorMsg = ParseWebException(ex);
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {errorMsg}");
            }
            catch (Exception ex)
            {
                var errorMsg = "An error occured retrieving the REST response: " + ex.Message;
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {errorMsg}");
            }

            return null;
        }
    }
}
