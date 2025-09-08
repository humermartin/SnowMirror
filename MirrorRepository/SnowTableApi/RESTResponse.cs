using System.Collections.Generic;
using MirrorRepository.Interfaces;

namespace MirrorRepository.SnowTableApi
{
    public abstract class RestResponse
    {
        /// <summary>
        /// Gets or sets the RawJson value
        /// </summary>
        public string RawJson { get; set; }

        /// <summary>
        /// Gets or sets the RawXml value
        /// </summary>
        public string RawXml { get; set; }

        /// <summary>
        /// Gets or sets the ErrorMsg value
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// Gets the IsError value
        /// </summary>
        public bool IsError
        {
            get
            {
                if (ErrorMsg.Length > 0) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected RestResponse()
        {
            this.RawJson = "";
            this.ErrorMsg = "";
        }
    }
    
    
}
