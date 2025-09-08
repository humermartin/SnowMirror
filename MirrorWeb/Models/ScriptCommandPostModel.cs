using System;

namespace MirrorWeb.Models
{
    public class ScriptCommandPostModel
    {
        /// <summary>
        /// Gets or sets the synchronizationId value
        /// </summary>
        public Guid? SynchronizationId { get; set; }

        /// <summary>
        /// Gets or sets the tablename value
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or set the command value
        /// </summary>
        public string Command { get; set; }
        
        
    }
}