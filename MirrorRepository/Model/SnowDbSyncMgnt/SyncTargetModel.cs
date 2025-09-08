using System;
using MirrorRepository.Data.SnowDbSyncMgnt;
using System.Collections.Generic;
using System.Linq;

namespace MirrorRepository.Model.SnowDbSyncMgnt
{
    
    public class SyncTargetModel : BaseModel
    {
        
        public Guid Id { get; set; }

        public string TargetType { get; set; }

        public string Targetname { get; set; }

        public string Endpoint { get; set; }
        
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        
        /// <summary>
        /// get synctargets
        /// </summary>
        /// <returns></returns>
        public List<SyncTarget> GetSyncTargetSettings()
        {
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                return snowEntities.SyncTarget.OrderBy(n => n.Targetname).ToList();
            }
        }
    }
}
