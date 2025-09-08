namespace MirrorRepository.Interfaces
{
    /// <summary>
    /// Interface IRestResponse
    /// </summary>
    public interface IRestResponse
    {
        /// <summary>
        /// property RawJson
        /// </summary>
        string RawJson { get; set; }

        /// <summary>
        /// property ErrorMsg
        /// </summary>
        string ErrorMsg { get; }

        /// <summary>
        /// property IsError
        /// </summary>
        bool IsError { get; }

        /// <summary>
        /// property ResultCount
        /// </summary>
        int ResultCount { get; }
    }
}
