using MirrorRepository.Data.SnowDbSyncMgnt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using MirrorRepository.Helpers;

namespace MirrorRepository.Model.SnowDbSyncMgnt
{
    
    public class DatabaseSettingsModel : BaseModel
    {
        
        public string Id { get; set; }
        public string Servername { get; set; }
        public string Port { get; set; }
        public string Instancename { get; set; }
        public string Databasename { get; set; }
        public string Schemaname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        

        

        /// <summary>
        /// Add a new DatabaseSettings set into the DB
        /// </summary>
        public void InsertOrUpdateData()
        {
            try
            {
                using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
                {
                    DatabaseSettings dbSettings;
                    if (String.IsNullOrEmpty(Id))
                    {
                        dbSettings = new DatabaseSettings();
                        dbSettings.Id = Guid.NewGuid();
                        snowEntities.DatabaseSettings.Add(dbSettings);
                        dbSettings.Created = DateTime.Now;
                    }
                    else
                    {
                        dbSettings = snowEntities.DatabaseSettings.First(d => d.Id.ToString().ToUpper() == Id.ToString().ToUpper());
                    }

                    dbSettings.Servername = Servername;
                    dbSettings.Instancename = Instancename;
                    dbSettings.Databasename = Databasename;
                    dbSettings.Schemaname = Schemaname;
                    dbSettings.Username = Username;
                    if (!string.IsNullOrEmpty(Password) && !Password.Equals(dbSettings.Password))
                    {
                        dbSettings.Password = Encryptdata(Password);
                    }
                    int x = 0;

                    if (Int32.TryParse(Port, out x))
                    {
                        dbSettings.Port = x;
                    }
                    dbSettings.LastChanged = DateTime.Now;

                    snowEntities.SaveChanges();
                }
            } catch (Exception e)
            {
                Log.Info("cannot save/update: " + Id + " : " + e.Message, e);
                LogHelp.Info(Log, e, this);
            }
        }

        /// <summary>
        /// Remove a DatabaseSettings set from the DB
        /// </summary>
        public void RemoveData()
        {
            try
            {
                using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
                {
                    DatabaseSettings dbSettings = snowEntities.DatabaseSettings.FirstOrDefault(d => d.Id.ToString().Equals(Id));
                    if (dbSettings != null)
                    {
                        snowEntities.DatabaseSettings.Attach(dbSettings);
                        snowEntities.DatabaseSettings.Remove(dbSettings);
                        snowEntities.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}, {e.InnerException}");
            }
            
        }

        /// <summary>
        /// Return a list with all values of a DatabaseSettings set with a specific id
        /// </summary>
        /// <returns></returns>
        public List<string> GetDatabaseInfo()
        {
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                var list = snowEntities.DatabaseSettings.FirstOrDefault(m => m.Id.ToString().Equals(Id));
                List<String> DatabaseInfo = new List<string>();
                DatabaseInfo.Add(list.Id.ToString());
                DatabaseInfo.Add(list.Servername);
                DatabaseInfo.Add(list.Port.ToString());
                DatabaseInfo.Add(list.Instancename);
                DatabaseInfo.Add(list.Databasename);
                DatabaseInfo.Add(list.Schemaname);
                DatabaseInfo.Add(list.Username);
                DatabaseInfo.Add(list.Password);
                return DatabaseInfo;
            }
        }
    }
}
