using System.Collections.Generic;

namespace MirrorRepository.Interfaces
{
    /// <summary>
    /// Interface IRestQueryResponse
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRestQueryResponse<T> : IRestResponse
    {
        /// <summary>
        /// Property Result
        /// </summary>
        ICollection<T> Result { get; set; }
    }
}
