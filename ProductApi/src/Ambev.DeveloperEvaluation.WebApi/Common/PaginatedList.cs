using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Represents a paginated list of items.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PaginatedList<T> : List<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedList{T}"/> class.
    /// </summary>
    public int CurrentPage { get; private set; }

    /// <summary>
    /// Gets the total number of pages based on the total count and page size.
    /// </summary>
    public int TotalPages { get; private set; }

    /// <summary>
    /// Gets the size of each page, which is the number of items per page.
    /// </summary>
    public int PageSize { get; private set; }

    /// <summary>
    /// Gets the total count of items across all pages.
    /// </summary>
    public int TotalCount { get; private set; }

    /// <summary>
    /// Indicates whether there are previous pages available.
    /// </summary>
    public bool HasPrevious => CurrentPage > 1;

    /// <summary>
    /// Indicates whether there are next pages available.
    /// </summary>
    public bool HasNext => CurrentPage < TotalPages;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedList{T}"/> class with the specified items, total count, page number, and page size.
    /// </summary>
    /// <param name="items"></param>
    /// <param name="count"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);

        AddRange(items);
    }

    /// <summary>
    /// Creates a new instance of <see cref="PaginatedList{T}"/> asynchronously from a queryable source.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}