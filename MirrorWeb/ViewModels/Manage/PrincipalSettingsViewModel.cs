using MirrorWeb.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MirrorWeb.ViewModels.Manage
{
    public class PrincipalSettingsViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the Modeldescription
        /// </summary>
        public string ModelDescription { get; set; }

        /// <summary>
        /// Gets or sets the CredentRoles
        /// </summary>
        public IEnumerable<SelectListItem> ManagementRoles { get; set; }

        /// <summary>
        /// Gets or sets the CredentialRole
        /// </summary>
        public string ManagementRole { get; set; }

        /// <summary>
        /// Gets or sets the Principal collection
        /// </summary>
        public List<AdUserModel> Principals { get; set; }

        /// <summary>
        /// Gets or sets the Principal total count
        /// </summary>
        public int PrincipalsTotalCount { get; set; }
    }
}