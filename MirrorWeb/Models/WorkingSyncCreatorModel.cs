using System;
using System.Collections.Generic;
using MirrorRepository.Model;
using MirrorRepository.SnowTableApi;

namespace MirrorWeb.Models
{
    public class WorkingSyncCreatorModel
    {
        /// <summary>
        /// Gets or sets the synchronizationId value
        /// </summary>
        public Guid? SynchronizationId { get; set; }
        
        /// <summary>
        /// Gets or sets the AutoSchemaUpdate value
        /// </summary>
        public bool AutoSchemaUpdate { get; set; }

        /// <summary>
        /// Gets or set the SnowTable list value
        /// </summary>
        public List<SnowTables> SnowTables { get; set; }

        /// <summary>
        /// Gets or sets the SnowColumns list value
        /// </summary>
        public List<string> SnowColumns { get; set; }

        /// <summary>
        /// Gets or sets the tablename value
        /// </summary>
        public string TableName { get; set; }

    }
}