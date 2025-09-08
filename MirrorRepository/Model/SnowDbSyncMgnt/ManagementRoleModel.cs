using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorRepository.Model.SnowDbSyncMgnt
{
    public class ManagementRoleModel
    {
        /// <summary>
        /// Get ManagementRoles
        /// </summary>
        /// <returns></returns>
        public List<ManagementRole> GetManagementRoles()
        {
            using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
            {
                List<ManagementRole> managementRoles = entities.ManagementRole.ToList();
                return managementRoles;
            }
        }
    }
}
