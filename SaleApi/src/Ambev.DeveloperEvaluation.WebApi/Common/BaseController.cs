using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Base controller for all API controllers
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the current user's ID from the claims.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    protected int GetCurrentUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new NullReferenceException());

    /// <summary>
    /// Gets the current user's email from the claims.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    protected string GetCurrentUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? throw new NullReferenceException();

    /// <summary>
    /// Returns a successful response with no data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    protected IActionResult Ok<T>(T data) =>
            base.Ok(new ApiResponseWithData<T> { Data = data, Success = true });

    /// <summary>
    /// Returns a created response with the specified route name and values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="routeName"></param>
    /// <param name="routeValues"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    protected IActionResult Created<T>(string routeName, object routeValues, T data) =>
        base.CreatedAtRoute(routeName, routeValues, new ApiResponseWithData<T> { Data = data, Success = true });

    /// <summary>
    /// Returns a bad request response with the specified message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected IActionResult BadRequest(string message) =>
        base.BadRequest(new ApiResponse { Message = message, Success = false });

    /// <summary>
    /// Returns a not found response with the specified message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected IActionResult NotFound(string message = "Resource not found") =>
        base.NotFound(new ApiResponse { Message = message, Success = false });

    /// <summary>
    /// Returns a paginated response with the specified paginated list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pagedList"></param>
    /// <returns></returns>
    protected IActionResult OkPaginated<T>(PaginatedList<T> pagedList) =>
            base.Ok(new PaginatedResponse<T>
            {
                Data = pagedList,
                CurrentPage = pagedList.CurrentPage,
                TotalPages = pagedList.TotalPages,
                TotalCount = pagedList.TotalCount,
                Success = true
            });
}
