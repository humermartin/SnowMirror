using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using log4net;
using System.Data.Entity.Validation;

namespace MirrorRepository.Base
{
    public partial class SnowBase : DynamicObject
    {

        public static readonly string SYS_ID = "sys_id";
        public static readonly string SNOWDBSYNC_CREATED = "snowdbsync_created";
        public static readonly string SNOWDBSYNC_UPDATED = "snowdbsync_updated";
        public static readonly string KAFKA_SYNCHRONIZED = "kafka_synchronized";

        public static readonly string SNOWDBSYNC_DATEONLY = "dd.MM.yyyy";
        public static readonly string SNOWDBSYNC_DATEFORMAT = "dd.MM.yyyy HH:mm";
        public static readonly string SNOWDBSYNC_DATEFORMAT_FULL = "dd.MM.yyyy HH:mm:ss";
        public static readonly string SNOWDBSYNC_TIMEFORMAT = "HH:mm";
        public static readonly string SNOW_DATE_FORMAT = "yyyy-MM-dd"; // "2019-07-15 15:10:46"
        public static readonly string SNOW_DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss"; // "2019-07-15 15:10:46"

        /// <summary>
        /// format Date/Time to accepted format
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString(SNOWDBSYNC_DATEFORMAT);
        }

        /// <summary>
        /// parse Date/Time: "31.12.2020 23:59:59"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ParseDateTime(string dateTime)
        {
            DateTime result;
            foreach (var format in new string[] { SNOWDBSYNC_DATEONLY, SNOWDBSYNC_DATEFORMAT, SNOWDBSYNC_DATEFORMAT_FULL })
            {
                if (DateTime.TryParseExact(dateTime, format, null, DateTimeStyles.AllowInnerWhite | DateTimeStyles.AssumeLocal, out result))
                    return result;
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// format to time only
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string FormatTime(DateTime dateTime)
        {
            return dateTime.ToString(SNOWDBSYNC_TIMEFORMAT);
        }

        /// <summary>
        /// parse time only: "23:45"
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime ParseTime(string time)
        {
            DateTime result;
            if (DateTime.TryParseExact(time, SnowBase.SNOWDBSYNC_TIMEFORMAT, null, DateTimeStyles.AllowInnerWhite | DateTimeStyles.AssumeLocal, out result))
            {
                return result;
            }
            return DateTime.Today;
        }

        public static void LogEntityException(ILog log, Exception e, object o = null) 
        {
            LogEntityException(log, null, e, o);
        }
        public static void LogEntityException(ILog log, string msg, Exception e, object o=null)
        {
            if (e is DbEntityValidationException)
            {
                log.Info((string.IsNullOrEmpty(msg)?"": msg + ": ") + "ValidationFailed: " + o + ": " +
                    string.Join("\n", ((DbEntityValidationException)e).EntityValidationErrors
                        .Select(ve => ve.Entry.GetType() + ":" + string.Join(",", ve.ValidationErrors.Select(err => err.ErrorMessage))))
                    );
            }
            else 
                log.Info((string.IsNullOrEmpty(msg)?"": msg + ": ") + "Exception for: " + o + " : " + e?.GetBaseException(), e);
        }

        /*
         "sys_class_name" : "u_cmdb_ci_service_cluster",
         "sys_class_path" : "/!!/$1",
         "sys_created_by" : "h.lehner@softpoint.at",
         "sys_created_on" : "2019-07-15 15:10:46",
         "sys_domain" : {
            "link" : "https://a1int.service-now.com/api/now/table/sys_user_group/global",
            "value" : "global"
         },
         "sys_domain_path" : "/",
         "sys_id" : "0004db1f1beebf0464f77449cd4bcbc1",
         "sys_mod_count" : "0",
         "sys_tags" : "",
         "sys_updated_by" : "h.lehner@softpoint.at",
         "sys_updated_on" : "2019-07-15 15:10:46",
         */
        [JsonProperty(PropertyName = "_sys_id")] // to 
        [Key]
        public Guid sys_id { get; set; }  // : "0004db1f1beebf0464f77449cd4bcbc1",
        protected string _sys_id_str;
        [MaxLength(32)]
        [JsonProperty(PropertyName = "sys_id")]
        public string sys_id_str { get => _sys_id_str;
            set
            {   // : "0004db1f1beebf0464f77449cd4bcbc1",
                _sys_id_str = value;
                try { this.sys_id = new Guid(value); } catch (Exception e) { throw; }
            }
        }
        public string sys_name { get; set; }
        [NotMapped]
        public SnowLink sys_package { get; set; }
        public string sys_policy { get; set; }
        [NotMapped]
        public SnowLink sys_scope { get; set; }
        public string sys_update_name { get; set; }
        public string sys_class_code { get; set; }
        public string sys_class_name { get; set; }
        public string sys_class_path { get; set; }
        public string sys_created_by { get; set; }

        public DateTime? sys_created_on { get; set; } // " : "2019-07-15 15:10:46",
        string _sys_created_on_str { get; set; } // " : "2019-07-15 15:10:46",
        public string sys_created_on_str
        {
            get
            {
                return _sys_created_on_str;
            }
            set // " : "2019-07-15 15:10:46",
            {
                _sys_created_on_str = value;
                sys_created_on = ToDate(value);
            }
        }
        /*
        sys_domain" : {
           link" : "https://a1int.service-now.com/api/now/table/sys_user_group/global",
           value" : "global"
                 },
        */
        public string sys_domain_path { get; set; }
        public int sys_mod_count { get; set; }
        public string sys_tags { get; set; }
        public string sys_updated_by { get; set; }

        public DateTime? sys_updated_on { get; set; }
        string _sys_updated_on_str;
        public string sys_updated_on_str
        {
            get
            {
                return _sys_updated_on_str;
            }
            set // " : "2019-07-15 15:10:46",
            {
                _sys_updated_on_str = value;
                sys_updated_on = ToDate(value);
            }
        }

        public static DateTime? ToDate(string value)
        {
            DateTime dt;
            if (value == null) 
                return null;
            if (DateTime.TryParseExact(value, SNOW_DATETIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                return dt;
            }
            if (DateTime.TryParseExact(value, SNOW_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                return dt;
            }
            throw new Exception("Invalid date: " + value);
        }

        public static readonly Regex GuidPattern = new Regex("(?i)[0-9a-f]{32}");
        public static readonly Regex GuidLinkPattern = new Regex("^http.*/(?i)[0-9a-f]{32}$");
        public static bool IsGuid(string val)
        {
            return GuidPattern.IsMatch(val);
        }
        public static Guid? ToGuid(string val)
        {
            if (GuidPattern.IsMatch(val))
            {
                return Guid.Parse(val);
            }
            return null;
        }

        Dictionary<string, object> properties = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (properties.ContainsKey(binder.Name))
            {
                result = properties[binder.Name];
                return true;
            }
            else
            {
                properties[binder.Name] = null;
                result = null;
                return true;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            properties[binder.Name] = value;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            dynamic method = properties[binder.Name];
            result = method(args[0].ToString(), args[1].ToString());
            return true;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[name=" + sys_class_name + ", id=" + _sys_id_str + "]";
        }
    }

    public class SnowLink
    {
        string _value;
        public Guid? Guid { get; set; }
        //[JsonProperty(PropertyName = "link")]
        public string link { get; set; } // : "https://a1int.service-now.com/api/now/table/sys_package/global",
        public string value { 
            get { 
                return _value; 
            } 
            set { 
                this._value = value; 
                Guid = SnowBase.ToGuid(value); 
            } 
        } // : "global"
        public bool IsGuid { get { return Guid != null; } }
        public override string ToString()
        {
            return this.GetType().Name + "[url=" + link+"]";
        }
    }

    public class QueryResponse
    {
        public bool ContinueOnEmptyResponse { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string status { get; set; }
        [JsonProperty(PropertyName = "error")]
        public SnowMessage error { get; set; }
        [JsonProperty(PropertyName = "result")]
        public JObject[] result { get; set; }
    }

    public class DictionaryResponse
    {
        [JsonProperty(PropertyName = "status")]
        public string status { get; set; }
        [JsonProperty(PropertyName = "error")]
        public SnowMessage error { get; set; }
        [JsonProperty(PropertyName = "result")]
        public SnowDictEntry[] result { get; set; }
    }

    public class SchemaResponse
    {
        [JsonProperty(PropertyName = "status")]
        public string status { get; set; }
        [JsonProperty(PropertyName = "error")]
        public SnowMessage error { get; set; }
        [JsonProperty(PropertyName = "result")]
        public SnowTable[] result { get; set; }
    }

    public class StatsResultResponse // {"result":{"stats":{"count":"0"}}}
    {
        [JsonProperty]
        public StatsResult result { get; set; }
    }

    public class ClusterState
    {
        public string allow_inbound { get; set; } //"true",
        public string iostats { get; set; } //""
        public string most_recent_keys { get; set; } //"9eb04e5adb1f8c10ffafb4f339961921,d6b04e5adb1f8c10ffafb4f33996191a,
        public string most_recent_message { get; set; } //"2020-03-31 13:01:04",
        public string node_id { get; set; } //"1dc94b70660ab43f8b5fb5226d77ecf1",
        public string pause_count { get; set; } //"0",
        public string ready_to_failover { get; set; } //"false",
        public string schedulers { get; set; } //"any",
        public string stats { get; set; } //"<?xml ver"
        public string status { get; set; } //"online",
        public string sys_created_by { get; set; } //"guest",
        public string sys_created_on { get; set; } //"2020-02-26 09:40:58",
        public string sys_id { get; set; } //"1dc94b70660ab43f8b5fb5226d77ecf1",
        public string sys_mod_count { get; set; } //"196485",
        public string sys_updated_by { get; set; } //"guest",
        public string sys_updated_on { get; set; } //"2020-03-31 13:01:04",
        public string system_id { get; set; } //"app130016.ams7.service-now.com:a1int004"
    }
    public class ClusterStateResponse
    {
        public ClusterState[] result;
    }

    public class ClusterNodeResponse
    {
        public ClusterNode[] result { get; set; }
    }

    public class ClusterNode 
    {
        public string errors { get; set; } // "14731",
        public string long_semaphore { get; set; } // "0",
        public string name { get; set; } // "app130016.ams7.service-now.com:16065",
        public SnowLink node { get; set; }
        //      "link" : "https://a1int.service-now.com/api/now/table/sys_cluster_state/1dc94b70660ab43f8b5fb5226d77ecf1",
        //      "value" : "1dc94b70660ab43f8b5fb5226d77ecf1"
        public string total_memory { get; set; } // "1980",
        public string transactions { get; set; } // "1184925",
        public string uptime { get; set; } // "7 days 12 hours 17 minutes",
        public string used_memory { get; set; } // "594"
    }

    public class StatsResult // {"result":{"stats":{"count":"0"}}}
    {
        [JsonProperty]
        public Stats stats { get; set; }
    }


    public class Stats
    {
        private string _count;
        [JsonProperty(PropertyName = "count")]
        public string count_str { get { return _count; } set { _count = value; count = Convert.ToInt32(value); } }
        [JsonProperty(PropertyName = "count_int")]
        public int count { get; set; }
    }

    public class SnowMessage
    {
        [JsonProperty(PropertyName = "detail")]
        public string detail { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string message { get; set; }
    }

    public class SnowDictEntry : SnowBase
    {
        public Boolean active { get; set; }
        public Boolean array { get; set; }
        public Boolean array_denormalized { get; set; }
        public string attributes { get; set; } // : "edge_encryption_enabled=true",
        public Boolean audit { get; set; }
        public string calculation { get; set; }
        public Nullable<int> choice { get; set; }
        public string choice_field { get; set; }
        public string choice_table { get; set; }
        public string column_label { get; set; }
        public string comments { get; set; }
        public string create_roles { get; set; }
        public string default_value { get; set; }
        public string defaultsort { get; set; }
        public string delete_roles { get; set; }
        public string dependent { get; set; }
        public string dependent_on_field { get; set; }
        public Boolean display { get; set; }
        public Boolean dynamic_creation { get; set; }
        public string dynamic_creation_script { get; set; }
        [NotMapped]
        public SnowLink dynamic_default_value { get; set; }
        [NotMapped]
        public SnowLink dynamic_ref_qual { get; set; }
        public string element { get; set; }
        public Boolean element_reference { get; set; }
        public string foreign_database { get; set; }
        public string function_definition { get; set; }
        public Boolean function_field { get; set; }
        [NotMapped]
        public SnowLink internal_type { get; set; }
        public Boolean mandatory { get; set; }
        public Nullable<int> max_length { get; set; }
        public string name { get; set; }
        public string next_element { get; set; }
        public Boolean primary { get; set; }
        public Boolean read_only { get; set; }
        public string read_roles { get; set; }
        [NotMapped]
        public SnowLink reference { get; set; }
        public string reference_cascade_rule { get; set; }
        public Boolean reference_floats { get; set; }
        public string reference_key { get; set; }
        public string reference_qual { get; set; }
        public string reference_qual_condition { get; set; }
        public string reference_type { get; set; }
        public string sizeclass { get; set; }
        public Boolean spell_check { get; set; }
        public Boolean staged { get; set; }
        public Boolean table_reference { get; set; }
        public Boolean text_index { get; set; }
        public Boolean unique { get; set; }
        public Boolean use_dependent_field { get; set; }
        public Boolean use_dynamic_default { get; set; }
        public string use_reference_qualifier { get; set; }
        public Boolean is_virtual {get; set;}
        public string widget { get; set; }
        public string write_roles { get; set; }
        public Boolean xml_view { get; set; }

        public override string ToString()
        {
            return this.GetType().Name + "[sys="+sys_name+",name=" + name + ",elem=" + element + ",id=" + _sys_id_str + "]";
        }

        [NotMapped]
        public bool IsReference { get { return internal_type != null
                        && (max_length.HasValue && max_length.Value == 32)
                        && (
                            "reference".Equals(internal_type.value)
                            || (reference as SnowLink != null && !string.IsNullOrEmpty(reference.value))
                            || (!string.IsNullOrEmpty(internal_type.value) && internal_type.value.EndsWith("_id"))
                        ); 
            } 
        }
        /*
        public string sys_class_name { get; set; }
        public string sys_created_by { get; set; } // : "f.bergauer@softpoint.at",
        public string sys_created_on { get; set; } // : "2020-01-15 14:48:43",
        public string sys_id { get; set; }
        public Nullable<int> sys_mod_count { get; set; }
        public string sys_name { get; set; }
        public SnowLink sys_package { get; set; }
        public string sys_policy { get; set; }
        public SnowLink sys_scope { get; set; }
        public string sys_update_name { get; set; }
        public string sys_updated_by { get; set; } // : "f.bergauer@softpoint.at",
        public string sys_updated_on { get; set; } // : "2020-01-27 14:35:28",
        */
    }

    public class SnowTable : SnowBase
    {
        public string access { get; set; }
        public Boolean actions_access { get; set; }
        public Boolean alter_access { get; set; }
        public string caller_access { get; set; }
        public Boolean client_scripts_access { get; set; }
        public Boolean configuration_access { get; set; }
        public Boolean create_access { get; set; }
        public Boolean create_access_controls { get; set; }
        public Boolean delete_access { get; set; }
        public string extension_model { get; set; }
        public Boolean is_extendable { get; set; }
        public string label { get; set; }
        public Boolean live_feed_enabled { get; set; }
        public string name { get; set; }
        public string number_ref { get; set; }
        public Boolean read_access { get; set; }
        public string super_class { get; set; }
        public Boolean update_access { get; set; }
        public string user_role { get; set; }
        public Boolean ws_access { get; set; }

        public override string ToString()
        {
            return this.GetType().Name + "[sys=" + sys_name + ",name=" + name + ",label=" + label + ",id=" + _sys_id_str + "]";
        }
    }
}
