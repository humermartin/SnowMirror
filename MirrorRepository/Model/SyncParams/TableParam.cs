using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorRepository.Model.SyncParams
{
    public class TableParam
    {
        /// <summary>
        /// Gets or sets the Instance ID
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the Instance Name
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets the Snow selected table columns
        /// </summary>
        public List<string> SnowColummns{ get; set; }
        
        public List<SynchronizationType> SynchronizationTypes{ get; set; }

        public string Init()
        {
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                List<TableParam> tblParams = new List<TableParam>();

                var instances = snowEntities.InstanzSettings.ToList();
                var syncTypes = snowEntities.SyncType.ToList();

                foreach (var instance in instances)
                {
                    TableParam tblParam = new TableParam();

                    //instance Name
                    tblParam.InstanceName = instance.InstanzName;

                    //instance Id
                    tblParam.InstanceId = instance.Id;
                    
                    List<SynchronizationType> lstSynchronizationTypes = new List<SynchronizationType>();
                    //Full
                    lstSynchronizationTypes.Add(GetSynchronizationType(syncTypes.Single(i => i.TypeName == "Full")));
                    //Delta
                    lstSynchronizationTypes.Add(GetSynchronizationType(syncTypes.Single(i => i.TypeName == "Delta")));
                    //Consistency
                    lstSynchronizationTypes.Add(GetSynchronizationType(syncTypes.Single(i => i.TypeName == "Consistency")));

                    tblParam.SynchronizationTypes = lstSynchronizationTypes;
                    tblParams.Add(tblParam);
                }
                
                //return serialized json
                return JsonConvert.SerializeObject(tblParams);
            }
        }

        /// <summary>
        /// Get synchronizationtype object
        /// </summary>
        /// <param name="syncType"></param>
        /// <returns></returns>
        private SynchronizationType GetSynchronizationType(SyncType syncType)
        {
            SynchronizationType synchronizationType= new SynchronizationType();
            synchronizationType.SyncTypeId = syncType.Id;
            synchronizationType.SyncTypeName = syncType.TypeName;

            SyncParameter syncParam = new SyncParameter();
            synchronizationType.SyncParameter = syncParam;
            return synchronizationType;

        }
    }
}
