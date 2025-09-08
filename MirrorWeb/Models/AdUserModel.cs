using System;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorWeb.Models
{
    public class AdUserModel
    {
        /// <summary>
        /// Gets or sets the Username
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the Fullname
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the Role
        /// </summary>
        public ManagementRole ManagementRole { get; set; }

        /// <summary>
        /// Gets or sets the active/inactive value
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the created time
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the aduser Id
        /// </summary>
        public Guid Id { get; set; }

        
        /// <summary>
        /// Gets or sets the html title
        /// </summary>
        public string Title { get; set; }
    }
}