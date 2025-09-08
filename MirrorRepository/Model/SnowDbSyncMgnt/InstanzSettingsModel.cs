using log4net;
using MirrorRepository.Data.SnowDbSyncMgnt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository.Model.SnowDbSyncMgnt
{
    public class InstanzSettingsModel : BaseModel
    {
        public string Id { get; set; }
        public string InstanceName { get; set; }
        public string UserName { get; set; }
        public string PW { get; set; }
        public string Servername { get; set; }
        public string Port { get; set; }
        public string ProxyPort { get; set; }
        public string ProxyHost { get; set; }
        public string ProxyUser { get; set; }
        public string ProxyPW { get; set; }
        
        

        /// <summary>
        /// Add a new InstanzSettings set into the DB or update a curren one
        /// </summary>
        public void InsertOrUpdateData()
        {
            InstanzSettings InstanzSettings;
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                if (String.IsNullOrEmpty(Id))
                {
                    InstanzSettings = new InstanzSettings();
                    InstanzSettings.Id = Guid.NewGuid();
                    snowEntities.InstanzSettings.Add(InstanzSettings);
                    InstanzSettings.Created = DateTime.Now;
                }
                else
                {
                    InstanzSettings = snowEntities.InstanzSettings.Where(d => d.Id.ToString().ToUpper() == Id.ToString().ToUpper()).First();
                }

                InstanzSettings.InstanzName = InstanceName;
                if (!string.IsNullOrEmpty(PW) && !PW.Equals(InstanzSettings.Password))
                {
                    InstanzSettings.Password = Encryptdata(PW);
                }
                InstanzSettings.UserName = UserName;

                InstanzSettings.Servername = Servername;
                InstanzSettings.Port = Convert.ToInt32(Port);

                InstanzSettings.ProxyServer = ProxyHost;
                InstanzSettings.ProxyUserName = ProxyUser;
                if (!string.IsNullOrEmpty(ProxyPW) && !ProxyPW.Equals(InstanzSettings.ProxyUserPassword))
                {
                    InstanzSettings.ProxyUserPassword = Encryptdata(ProxyPW);
                }
                InstanzSettings.Port = 443;
                int x = 0;
                if (Int32.TryParse(ProxyPort, out x))
                {
                    InstanzSettings.ProxyPort = x;
                }
                InstanzSettings.LastChanged = DateTime.Now;
                snowEntities.SaveChanges();
            }
        }
        

        /// <summary>
        /// Remove a InstanzSettings set from the DB
        /// </summary>
        public void RemoveData()
        {
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                InstanzSettings removeInstanzSettings = snowEntities.InstanzSettings.Where(d => d.Id.ToString().Equals(Id)).FirstOrDefault();
                snowEntities.InstanzSettings.Attach(removeInstanzSettings);
                snowEntities.InstanzSettings.Remove(removeInstanzSettings);
                snowEntities.SaveChanges();
            }
        }

        /// <summary>
        /// Return a list with all InstanzSettings sets
        /// </summary>
        /// <returns></returns>
        public IList<InstanzSettings> GetAllInstances()
        {
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                return snowEntities.InstanzSettings.ToList();
            }
        }

        /// <summary>
        /// Return a list with all values of a InstanzSettings set with a specific id
        /// </summary>
        /// <returns></returns>
        public List<string> GetInstanceInfo()
        {
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                var list = snowEntities.InstanzSettings.FirstOrDefault(m => m.Id.ToString() == Id.ToString());
                List<String> InstanceInfo = new List<string>();
                InstanceInfo.Add(list.Id.ToString());
                InstanceInfo.Add(list.InstanzName);
                InstanceInfo.Add(list.Servername);
                InstanceInfo.Add("" + list.Port);
                InstanceInfo.Add(list.UserName);
                InstanceInfo.Add(list.Password);
                InstanceInfo.Add(list.ProxyServer);
                InstanceInfo.Add(list.ProxyPort.ToString());
                InstanceInfo.Add(list.ProxyUserName);
                InstanceInfo.Add(list.ProxyUserPassword);
                return InstanceInfo;
            }
        }

    }
}