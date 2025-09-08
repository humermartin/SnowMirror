using System;

namespace MirrorRepository.Model
{
    [Serializable]
    public class ScriptCommand
    {
        /// <summary>
        /// Gets or sets the guid value
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the synchronization
        /// </summary>
        public Data.SnowDbSyncMgnt.Synchronization Synchronization { get; set; }

        /// <summary>
        /// Gets or sets the tablename value
        /// </summary>
        public string TableName { get; set; }
        
        /// <summary>
        /// Gets or sets command value
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets the script command created value
        /// </summary>
        public string Created { get; set; }
    }
}
