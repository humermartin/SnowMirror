using MirrorRepository.Interfaces;

namespace MirrorRepository.SnowTableApi
{
    /// <summary>
    /// class RestSingleResponse
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RestSingleResponse<T> : RestResponse, IRestSingleResponse<T>
    {
        /// <summary>
        /// Gets or sets the singel result value
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Gets or sets the singel result count value
        /// </summary>
        public int ResultCount
        {
            get
            {
                if (Result == null) { return 0; }
                return 1;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RestSingleResponse() { }

        
    }

}
