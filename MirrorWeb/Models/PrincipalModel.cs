using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using MirrorRepository;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorWeb.AdAuthorizationFilter;

namespace MirrorWeb.Models
{
    public class PrincipalModel
    {
        /// <summary>
        /// Gets or sets the SamAccountName
        /// </summary>
        public string SamAccountName { get; set; }

        /// <summary>
        /// Gets or sets the FirstName
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the LastName
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets the permitted adUser principals
        /// </summary>
        /// <returns></returns>
        public List<AdUserModel> GetPrincipals()
        {
            List<AdUserModel> adUserList = new List<AdUserModel>();

            using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
            {
                var principals = entities.Principals.ToList();
                
                foreach (var principal in principals)
                {
                    AdUserModel adUserModel = new AdUserModel();
                    adUserModel.Id = principal.Id;
                    adUserModel.UserName = principal.UserName;

                    if (AdAuthenticationService.principalContext != null)
                    {
                        using (var adUserFound = UserPrincipal.FindByIdentity(AdAuthenticationService.principalContext, adUserModel.UserName))
                        {
                            if (adUserFound != null)
                            {
                                adUserModel.FullName = $"{adUserFound.GivenName} {adUserFound.Surname}";
                            }
                        }
                    }

                    var mgntRole = entities.ManagementRole.FirstOrDefault(r => r.Id == principal.RoleId);
                    adUserModel.ManagementRole = mgntRole;
                    adUserModel.Active = principal.Active;
                    adUserModel.CreatedTime = principal.CreateTime;
                    adUserList.Add(adUserModel);
                }
            }

            return adUserList;
        }
    }
}