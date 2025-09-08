using System.Collections.Generic;

namespace MirrorRepository.Interfaces
{
    /// <summary>
    /// Interface IRestSingleResponse
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRestSingleResponse<T> : IRestResponse
    {
        /// <summary>
        /// Property Result
        /// </summary>
        T Result { get; set; }
    }
}
