using MirrorRepository.SnowTableApi;

namespace MirrorRepository.Interfaces
{
    /// <summary>
    /// Interface ITableApiClient
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITableApiClient<T>
     where T : Record
    {
        /// <summary>
        /// Method GetById
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        RestSingleResponse<T> GetById(string id);

        /// <summary>
        /// Method GetByQuery
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        RestQueryResponse<T> GetByQuery(string query);

        /// <summary>
        /// Method GetFull
        /// </summary>
        /// <returns></returns>
        RestQueryResponse<T> GetFull();
    }
}
