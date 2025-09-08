using System.Collections.Generic;
using MirrorRepository.Interfaces;

namespace MirrorRepository.SnowTableApi
{
    /// <summary>
    /// class REST query response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RestQueryResponse<T> : RestResponse, IRestQueryResponse<T>
    {
        /// <summary>
        /// Gets or sets the collection result value
        /// </summary>
        public ICollection<T> Result { get; set; }

        /// <summary>
        /// Gets the result count value
        /// </summary>
        public int ResultCount
        {
            get
            {
                if (Result == null) { return 0; }
                return Result.Count;
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public RestQueryResponse()
        {
            this.Result = new List<T>();
        }


        

        
    }
}
