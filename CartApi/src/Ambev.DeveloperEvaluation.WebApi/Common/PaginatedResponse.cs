namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Represents a paginated response for API endpoints that return a collection of items.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PaginatedResponse<T> : ApiResponseWithData<IEnumerable<T>>
{
    /// <summary>
    /// Gets or sets the current page number in the paginated response.
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages available in the paginated response.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets or sets the total number of items across all pages in the paginated response.
    /// </summary>
    public int TotalCount { get; set; }
}