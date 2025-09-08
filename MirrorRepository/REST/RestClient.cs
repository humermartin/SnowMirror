using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using MirrorRepository.Base;
using Newtonsoft.Json;
using System.IO;
using log4net;
using System.Reflection;
using System.Threading;
using MirrorRepository.Model.RecordCount;
using Microsoft.Ajax.Utilities;

namespace MirrorRepository.REST
{
    /**
     * https://developer.servicenow.com/app.do#!/rest_api_doc?v=newyork&id=r_TableAPI-GET
     */

    public enum PROP { sys_class_name, sys_class_path, sys_created_by, sys_created_on, sys_domain_path, sys_id, sys_mod_count, sys_tags, sys_updated_by, sys_updated_on, name, element }
    public enum PARM { sysparm_orderby, sysparm_order_direction, sysparm_query, sysparm_limit, sysparm_offset, sysparm_fields, sysparm_count, sysparm_no_count, sysparm_suppress_pagination_header }
    public enum sort_dir { asc, desc }
    // MultiSort: <property>=true^ORDERBY<sorFieldAsc>^ORDERBYDESC<sortFieldDesc>
    public enum mult_sort_dir { ORDERBY, ORDERBYDESC }

    public enum OP { STARTSWITH, ENDSWITH, LIKE, NOTLIKE, ISEMPTY, ISNOTEMPTY, ANYTHING, EMPTYSTRING, BETWEEN, SAMEAS, IN }
    public class SnowParm
    {
        public static readonly string SNOW_DATEFORMAT = "yyyy-MM-ddTHH:mm:ss";
        public static string[] OPSTR = { "*", "!*", "%", };

        private bool _filter = false;
        public PARM parm { get; set; }
        public string op { get; set; } = "=";
        public string value { get; set; }
        public bool isFilter { get { return _filter; } }

        public SnowParm(PARM parm, string value = "")
        {
            this.parm = parm;
            this.value = value;
        }

        public SnowParm(PARM parm, string op, string value)
        {
            this.parm = parm;
            this.op = op;
            this.value = value;
        }

        public SnowParm(PARM parm, long value) : this(parm, "" + value) { }

        public static SnowParms asc(string key = null, SnowParms parms = null)
        {
            parms = parms ?? new SnowParms();
            key = key ?? Enum.GetName(typeof(PROP), PROP.sys_id);
            parms.Add(new SnowParm(PARM.sysparm_orderby, key));
            return parms;
        }

        public static SnowParms desc(string key, SnowParms parms = null)
        {
            parms = asc(key, parms);
            parms.Add(new SnowParm(PARM.sysparm_order_direction, Enum.GetName(typeof(sort_dir), sort_dir.desc)));
            return parms;
        }

        public static SnowParms asc(PROP key, SnowParms parms = null)
        {
            return asc(Enum.GetName(typeof(PROP), key), parms);
        }
        public static SnowParms desc(PROP key, SnowParms parms = null)
        {
            return desc(Enum.GetName(typeof(PROP), key), parms);
        }
        public static SnowParms limit(long limit, SnowParms parms = null)
        {
            parms = parms ?? new SnowParms();
            parms.Add(new SnowParm(PARM.sysparm_limit, limit));
            return parms;
        }

        public static SnowParms count(SnowParms parms = null)
        {
            parms = parms ?? new SnowParms();
            parms.Add(new SnowParm(PARM.sysparm_count, "true"));
            return parms;
        }

        public static SnowParms nocount(SnowParms parms = null)
        {
            parms = parms ?? new SnowParms();
            parms.Add(new SnowParm(PARM.sysparm_no_count, "true"));
            return parms;
        }

        public static SnowParms suppress_pagination_header(SnowParms parms = null)
        {
            parms = parms ?? new SnowParms();
            parms.Add(new SnowParm(PARM.sysparm_suppress_pagination_header, "true"));
            return parms;
        }

        public static SnowParms offset(long offset, SnowParms parms = null)
        {
            parms = parms ?? new SnowParms();
            parms.Add(new SnowParm(PARM.sysparm_offset, offset));
            return parms;
        }

        public static SnowParms select(string[] fields, SnowParms parms = null)
        {
            parms = parms ?? new SnowParms();
            parms.Add(new SnowParm(PARM.sysparm_fields, String.Join(",", fields)));
            return parms;
        }

        public static SnowParms columns(List<string> columns, SnowParms parms = null)
        {
            parms = parms ?? new SnowParms();
            if (columns == null || columns.Count == 0)
                return parms;
            parms.Add(new SnowParm(PARM.sysparm_fields, string.Join("%2C", columns)));
            return parms;
        }

        public static SnowParm newFilter(string prop, object value, OP op, bool greater = true, string cond = "^")
        {
            return newFilter(prop, Enum.GetName(typeof(OP), op), value, greater, cond);
        }
        public static SnowParm newFilter(PARM prop, object value, string op = "=", bool greater = true, string cond = "^")
        {
            return newFilter(Enum.GetName(typeof(PARM), prop), op, value, greater, cond);
        }
        public static SnowParm newFilter(string prop = "active", string op = "=", object value = null, bool greater = true, string cond = "^")
        {
            SnowParm filter = new SnowParm(PARM.sysparm_query);
            filter._filter = true;
            if (prop != "")
            {
                if ("active".Equals(prop) && value == null) value = "true";
                filter.filter(prop, op, value, greater, cond);
            } else
            {

            }
            return filter;
        }

        /**
         * sysparm_query=active=true^nameSTARTSWITHu_^ORDERBYDESCname
         */
        // EQUALS
        public SnowParm equals(PROP prop, object val)
        {
            return equals(Enum.GetName(typeof(PROP), prop), val);
        }
        public SnowParm equals(string prop, object val)
        {
            return filter(prop, "=", val);
        }

        public SnowParm notEquals(PROP prop, object val)
        {
            return notEquals(Enum.GetName(typeof(PROP), prop), val);
        }
        public SnowParm notEquals(string prop, object val)
        {
            return filter(prop, "!=", val);
        }

        // LIKE
        public SnowParm like(PROP prop, object val)
        {
            return like(Enum.GetName(typeof(PROP), prop), val);
        }
        public SnowParm like(string prop, object val)
        {
            return filter(prop, OP.LIKE, val);
        }

        public SnowParm notLike(PROP prop, object val)
        {
            return notLike(Enum.GetName(typeof(PROP), prop), val);
        }
        public SnowParm notLike(string prop, object val)
        {
            return filter(prop, OP.NOTLIKE, val);
        }

        // BETWEEN
        public SnowParm between(PROP prop, object lower, object upper)
        {
            return between(Enum.GetName(typeof(PROP), prop), lower, upper);
        }
        public SnowParm between(string prop, object lower, object upper)
        {
            return filter2(prop, OP.BETWEEN, lower, upper);
        }

        // IN
        public SnowParm inList(PROP prop, ICollection<String> values)
        {
            return filterOR(Enum.GetName(typeof(PROP), prop), values);
        }
        public SnowParm inList(string prop, ICollection<String> values)
        {
            return filterOR(prop, values);
        }

        public SnowParm empty(PROP prop)
        {
            return empty(Enum.GetName(typeof(PROP), prop));
        }
        public SnowParm empty(string prop)
        {
            return filter(prop, OP.ISEMPTY, "");
        }

        public SnowParm notEmpty(PROP prop)
        {
            return notEmpty(Enum.GetName(typeof(PROP), prop));
        }
        public SnowParm notEmpty(string prop)
        {
            return filter(prop, OP.ISNOTEMPTY, "");
        }

        public SnowParm orderBy(PROP prop)
        {
            return orderBy(Enum.GetName(typeof(PROP), prop));
        }

        public SnowParm orderBy(string prop)
        {
            return filter_("", prop, Enum.GetName(typeof(mult_sort_dir), mult_sort_dir.ORDERBY));
        }

        public SnowParm orderByDesc(PROP prop)
        {
            return orderByDesc(Enum.GetName(typeof(PROP), prop));
        }

        public SnowParm orderByDesc(string prop)
        {
            return filter_("", prop, Enum.GetName(typeof(mult_sort_dir), mult_sort_dir.ORDERBYDESC));
        }

        public SnowParm filter(string prop, OP op, object val, bool greater = true, string cond = "^")
        {
            return filter_(prop, val, Enum.GetName(typeof(OP), op), greater, cond);
        }
        public SnowParm filter(PARM prop, OP op, object val, bool greater = true, string cond = "^")
        {
            return filter_(Enum.GetName(typeof(PARM), prop), val, Enum.GetName(typeof(OP), op), greater, cond);
        }
        public SnowParm filter2(string prop, OP op, object val1, object val2, bool greater = true, string cond = "^")
        {
            return filter2_(prop, val1, val2, Enum.GetName(typeof(OP), op), greater, cond);
        }
        public SnowParm filter2(PARM prop, OP op, object val1, object val2, bool greater = true, string cond = "^")
        {
            return filter2_(Enum.GetName(typeof(PARM), prop), val1, val2, Enum.GetName(typeof(OP), op), greater, cond);
        }

        public SnowParm filter(string prop, OP op, ICollection<String> values, bool greater = true, string cond = "^")
        {
            return filter_(prop, values, Enum.GetName(typeof(OP), op), greater, cond);
        }

        public SnowParm equals(PARM prop, object val, string op = "=", bool greater = true, string cond = "^")
        {
            return filter_(Enum.GetName(typeof(PARM), prop), val, op, greater, cond);
        }
        public SnowParm active(bool active = true)
        {
            return filter_("active", active ? "true" : "false", "=");
        }

        public SnowParm nop()
        {
            return filter_("", "", "");
        }

        public SnowParm filter(PARM prop, string op, object val, bool greater = true, string cond = "^")
        {
            return filter_(Enum.GetName(typeof(PARM), prop), val, op, greater, cond);
        }
        public SnowParm filter(string prop, string op, object val, bool greater = true, string cond = "^")
        {
            return filter_(prop, val, op, greater, cond);
        }
        SnowParm filter_(string prop, object val, string op = "=", bool greater = true, string cond = "^")
        {
            _filter = true;
            this.value = (this.value == null || this.value.Length == 0) ? "" : value + cond;

            if (val is DateTime)
            {   // "sys_created_on" : "2019-07-15 15:10:46"
                val = ((DateTime)val).ToString(SNOW_DATEFORMAT);
                op = greater ? ">=" : "<=";
            }

            this.value += prop + op + val;
            return this;
        }

        SnowParm filter2_(string prop, object val1, object val2, string op = "=", bool greater = true, string cond = "^")
        {
            _filter = true;
            this.value = (this.value == null || this.value.Length == 0) ? "" : value + cond;

            if (val1 is DateTime)
            {   // "sys_created_on" : "2019-07-15 15:10:46"
                val1 = ((DateTime)val1).ToString(SNOW_DATEFORMAT);
            }
            if (val2 is DateTime)
            {   // "sys_created_on" : "2019-07-15 15:10:46"
                val2 = ((DateTime)val2).ToString(SNOW_DATEFORMAT);
            }

            this.value += prop + op + val1 + "@" + val2;
            return this;
        }

        SnowParm filterOR(string prop, ICollection<String> values, string op = "=", bool greater = true, string cond = "^")
        {
            if (values == null || values.Count == 0)
                return this;
            _filter = true;
            this.value = (this.value == null || this.value.Length == 0) ? "" : value + cond;

            this.value += prop + op + values.First();
            values.Skip(1).ForEach(v => this.value += "^OR" + prop + op + v);
            return this;
        }


        public string query() {
            if (value == null || value == "")
            {
                switch (parm)
                {
                    case PARM.sysparm_order_direction:
                        value = Enum.GetName(typeof(sort_dir), sort_dir.asc);
                        break;
                    default:
                        break;
                }
            }
            return Enum.GetName(typeof(PARM), parm) + op + value;
        }

        public override string ToString()
        {
            return "SParm[" + parm + ":" + value + "]";
        }
    }

    public class SnowParms : List<SnowParm> {
        public static SnowParms New { get { return new SnowParms(); } }
        public SnowParms add(SnowParm parm) { Add(parm); return this; }
        public SnowParms select(string[] fields) { AddRange(SnowParm.select(fields)); return this; }
        public SnowParms limit(long limit) { AddRange(SnowParm.limit(limit)); return this; }
        public SnowParms offset(long limit) { AddRange(SnowParm.offset(limit)); return this; }
        public SnowParms nocount() { AddRange(SnowParm.nocount()); return this; }
        public SnowParms columns(List<string> columns) { AddRange(SnowParm.columns(columns)); return this; }
        public SnowParms suppress_pagination_header() { AddRange(SnowParm.suppress_pagination_header()); return this; }
        //public SnowParms asc(string key = null) { AddRange(SnowParm.asc(key)); return this; }
        //public SnowParms desc(string key = null) { AddRange(SnowParm.desc(key)); return this; }
        //public SnowParms asc(PROP key) { AddRange(SnowParm.asc(key)); return this; }
        //public SnowParms desc(PROP key) { AddRange(SnowParm.desc(key)); return this; }
        public SnowParms empty(PROP parm) { Filter.empty(parm); return this; }
        public SnowParms empty(string parm) { Filter.empty(parm); return this; }
        public SnowParms notEmpty(PROP parm) { Filter.notEmpty(parm); return this; }
        public SnowParms notEmpty(string parm) { Filter.notEmpty(parm); return this; }

        public SnowParms orderBy(string parm) { Filter.orderBy(parm); return this; }
        public SnowParms orderBy(PROP parm) { Filter.orderBy(parm); return this; }
        public SnowParms orderByDesc(string parm) { Filter.orderByDesc(parm); return this; }
        public SnowParms orderByDesc(PROP parm) { Filter.orderByDesc(parm); return this; }

        public SnowParms equals(PROP parm, object value) { Filter.equals(parm, value); return this; }
        public SnowParms equals(string parm, object value) { Filter.equals(parm, value); return this; }
        public SnowParms notEquals(PROP parm, object value) { Filter.notEquals(parm, value); return this; }
        public SnowParms notEquals(string parm, object value) { Filter.notEquals(parm, value); return this; }
        public SnowParms between(PROP parm, object value1, object value2) { Filter.between(parm, value1, value2); return this; }
        public SnowParms between(string parm, object value1, object value2) { Filter.between(parm, value1, value2); return this; }
        public SnowParms like(PROP parm, object value) { Filter.like(parm, value); return this; }
        public SnowParms like(string parm, object value) { Filter.like(parm, value); return this; }
        public SnowParms notLike(PROP parm, object value) { Filter.notLike(parm, value); return this; }
        public SnowParms notLike(string parm, object value) { Filter.notLike(parm, value); return this; }
        public SnowParms active(bool active = true) { Filter.active(active); return this; }
        public SnowParms inList(PROP parm, ICollection<String> values) { Filter.inList(parm, values); return this; }
        public SnowParms inList(string parm, ICollection<String> values) { Filter.inList(parm, values); return this; }


        public SnowParms filter(string prop, OP op, object value, bool greater = true, string cond = "^")
        {
            return filter(prop, Enum.GetName(typeof(OP), op), value, greater, cond);
        }
        public SnowParms filter(string prop, string op, object value, bool greater = true, string cond = "^")
        {
            Filter.filter(prop, op, value, greater, cond);
            return this;
        }

        public SnowParm Filter
        {
            get
            {
                var filt = this.Where(p => p.isFilter == true).FirstOrDefault();
                if (filt == null)
                {
                    filt = SnowParm.newFilter("");
                    Add(filt);
                }
                return filt;
            }
        }
        public string join(string sep = "&")
        {
            return String.Join(sep, this.Select(sp => sp.query()));
        }
    }

    public class RestClient : IClient<RestClient>, IDisposable
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        public static String A1_PROXY { get; set; } = "http://proxy.austria.local:8080/";
        public static ICredentials ProxyCredentials { private get; set; }

        public const int MAX_TIMEOUT = 30;
        public string BaseUrl { get; private set; }
        public string Username { get; private set; }
        private string Password { get; set; }
        public int Timeout { get; set; } = MAX_TIMEOUT;

        HttpClient client { get; set; }
        WebClient webClient { get; set; }
        public bool UseWebClient { get; set; }
        public CookieContainer Cookies { get; protected set; } = new CookieContainer();
        public static JsonSerializer SnowSerializer { 
            get {
                return new JsonSerializer
                {
                    DateFormatString = SnowBase.SNOW_DATETIME_FORMAT
                };
            } 
        }

        public T Deserialize<T>(string value)
        {
            try
            {
                return SnowSerializer.Deserialize<T>(new JsonTextReader(new StringReader(value)));
            }
            catch (Exception e)
            {
                Log.Debug($"{MethodBase.GetCurrentMethod()?.Name}: cannot deserialize value: {value}. Exception: {e.Message}, {e.GetBaseException()}");
                throw;
            }
        }

        readonly Regex SnowNodeRegex = new Regex("BIGipServerpool|glide_user_route");
        public List<Cookie> SnowNodeCookies()
        {
            return GetCookies().Where(c => !string.IsNullOrEmpty(c.Name) && SnowNodeRegex.IsMatch(c.Name)).ToList();
        }
        public List<Cookie> GetCookies()
        {
            var cookies = new List<Cookie>();
            if (Cookies == null) return cookies;
            var ce = Cookies.GetCookies(client.BaseAddress).GetEnumerator();
            while (ce.MoveNext()) cookies.Add((Cookie)ce.Current);
            return cookies;
        }

        public RestClient New()
        {
            return Build(BaseUrl, Username, Password, 
                useDefaultCredentials: ProxyCredentials == null, credentials: ProxyCredentials,
                timeout: Timeout);
        }

        public static RestClient Build(string URL, string username, string password, int timeout = MAX_TIMEOUT, bool useWebClient = false, bool useDefaultCredentials = true, ICredentials credentials = null)
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

                RestClient rc = new RestClient() { BaseUrl = URL, Username = username, Password = password, UseWebClient = useWebClient};
                
                var handler = new HttpClientHandler
                {
                    Proxy = proxy, //new System.Net.WebProxy(A1_PROXY),
                    UseProxy = proxy != null,
                    Credentials = new NetworkCredential(username, password),
                    CookieContainer = rc.Cookies,
                };

                if (!URL.EndsWith("/")) URL += "/"; // must end with "/" !!!

                if (rc.client == null)
                {
                    HttpClient client = new HttpClient(handler);
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
            } catch (Exception e)
            {
                Log.Info("cannot build RestClient: " + e.GetBaseException(), e);
                throw new Exception("cannot build RestClient: " + e.GetBaseException().Message, e);
            }
        }

        readonly int MAX_RETRY = 3;
        public String Read(string request, SnowParms parms = null)
        {
            //try { Thread.ResetAbort(); } catch { } // just for fun??!!
            for (int i = 0; i < MAX_RETRY; i++)
            {
                try
                {
                    try
                    {
                        return ReadInternal(request, parms);
                    }
                    catch
                    {
                        try { Thread.ResetAbort(); } catch { } // just for fun??!!
                    }
                } 
                catch
                {
                    if (i < MAX_RETRY-1)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    throw;
                }
            }
            throw new Exception("too many aborts - cannot read " + request);
        }

        public String ReadInternal(string request, SnowParms parms = null) {

            request = FormatRequest(request, parms);

            Log.Info(String.Format("reading: {0}", request));
            try
            {
                string content;
                if (UseWebClient && webClient != null)
                {
                    //var task = webClient.DownloadStringTaskAsync(request);
                    //content = task.Result;
                    content = webClient.DownloadString(request);
                    Log.Info(String.Format("read: {0}", request));
                    return content;
                }
                else
                {
                    try
                    {
                        var task = client.GetAsync(request);
                        HttpResponseMessage response = task.Result;
                        Log.Info(String.Format("read: {0} ({1}) req={2} : cookies={3}", 
                            (int)response.StatusCode, response.ReasonPhrase, request, string.Join(",", SnowNodeCookies())));

                        if (response.IsSuccessStatusCode)
                        {
                            // Parse the response body.
                            //Make sure to add a reference to System.Net.Http.Formatting.dll
                            content = response.Content.ReadAsStringAsync().Result;
                            return content;
                        }
                        else
                        {
                            var msg = String.Format("failed to read: {0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                            throw new Exception(msg);
                        }
                    }
                    catch (AggregateException ae)
                    {
                        var msg = String.Format("cannot request: base={0} req={1} : exc={2}", client.BaseAddress, request, ae.GetBaseException().Message);
                        Log.Info(msg);
                        Log.Debug(msg, ae.GetBaseException());
                        throw ae.GetBaseException();
                    }
                }
            } 
            catch (Exception e)
            {
                try { Thread.ResetAbort(); } catch { }
                var msg = String.Format("cannot request: base={0} req={1} : exc={2}", client.BaseAddress, request, e.GetBaseException().Message);
                Log.Info(msg);
                Log.Debug(msg, e.GetBaseException());
                throw;
            }
        }

        public string FormatRequest(string request, SnowParms parms)
        {
            parms = parms ?? new SnowParms();

            if (request.StartsWith("/")) request = Regex.Replace(request, "^/+", ""); // must NOT START with "/" !!!

            request = request + "?"; // take only active values
            // List data response.
            // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (parms.Count > 0)
            {
                request = request + parms.join();
            }

            if (request.EndsWith("?"))
            {
                request = request.Remove(request.Length - 1, 1);
            }

            return request;
        }

        public Dictionary<SnowDictEntry, List<SnowDictEntry>> ReadDictionary(string name = null, string like = null)
        {
            var dict = new Dictionary<SnowDictEntry, List<SnowDictEntry>>();
            // read tables
            var filter = SnowParm.select(new string[] { "name", "sys_id", "element", "sys_name" })
                .active()
                .empty("element")
                .orderBy("name");
            if (name != null && like != null) filter.like(name, like);

            var content = Read("/api/now/table/sys_dictionary", filter);
            var snowTables = Deserialize<DictionaryResponse>(content);

            foreach (var tabEnt in snowTables.result)
            {
                // read Table entry
                content = Read("/api/now/table/sys_dictionary",
                    SnowParms.New
                    .active()
                    .equals("name", tabEnt.name));
                var entries = Deserialize<DictionaryResponse>(content);
                var table = entries.result.Where(e => String.IsNullOrEmpty(e.element)).First();
                // read fields of table entry
                var fields = entries.result.Where(e => !string.IsNullOrEmpty(e.element)).ToList();

                dict.Add(tabEnt, fields);
            }
            return dict;
        }

        public Dictionary<SnowDictEntry, List<SnowDictEntry>> ToTables(string json)
        {
            var snowTables = Deserialize<DictionaryResponse>(json);
            return ToTables(snowTables.result.ToList());
        }

        public Dictionary<SnowDictEntry, List<SnowDictEntry>> ToTables(List<SnowDictEntry> dictEntries)
        {
            var dict = new Dictionary<SnowDictEntry, List<SnowDictEntry>>();
            var tables = dictEntries.Where(e => String.IsNullOrEmpty(e.element))
                .DistinctBy(e => e.name);
            // read tables
            foreach (var table in tables)
            {
                var fields = dictEntries.Where(e => e.name == table.name && !string.IsNullOrEmpty(e.element))
                    .DistinctBy(e => e.element)
                    .ToList();
                dict.Add(table, fields);
            }
            return dict;
        }

        public StatsResultResponse Count(string table)
        {
            var content = Read("/api/now/v1/stats/"+table, SnowParm.count());
            var result = Deserialize<StatsResultResponse>(content);
            return result;
        }

        /// <summary>
        /// get delta table record count
        /// </summary>
        /// <param name="deltaStart"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public StatsResultResponse DeltaCount(DateTime deltaStart, string table)
        {
            string reqDeltaCount = $"/api/now/v1/stats/{table}?sysparm_query=sys_updated_on>javascript:gs.dateGenerate('{deltaStart.Date:yyyy-MM-dd}','{deltaStart:HH:mm:ss}')&sysparm_count=true";
            var content = Read(reqDeltaCount, null);
            var result = Deserialize<StatsResultResponse>(content);
            return result;
        }

        public ClusterStateResponse ClusterState()
        {
            var content = Read("/api/now/table/sys_cluster_state");
            var result = Deserialize<ClusterStateResponse>(content);
            return result;
        }

        public ClusterNodeResponse ClusterNodes()
        {
            var content = Read("/api/now/table/v_cluster_node");
            var result = Deserialize<ClusterNodeResponse>(content);
            return result;
        }

        public void Dispose()
        {
            //if (myclient != null)
            //{
            //    myclient.Dispose();
            //    myclient = null;
            //}
        }
        
    }

    public class RestClientAbortException : Exception
    {
        public RestClientAbortException() { }
        public RestClientAbortException(string message) : base(message) { }
        public RestClientAbortException(string message, Exception inner) : base(message, inner) { }
    }
}
