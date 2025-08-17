namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Generic API response model that includes data
/// </summary>
/// <typeparam name="T"></typeparam>
public class ApiResponseWithData<T> : ApiResponse
{
    /// <summary>
    /// The data returned in the response
    /// </summary>
    public T? Data { get; set; }
}
