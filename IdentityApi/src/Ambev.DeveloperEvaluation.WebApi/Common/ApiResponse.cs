using Ambev.DeveloperEvaluation.Common.Validation;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// General API response model used for standardizing responses across the application.
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Indicates whether the API call was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// A message providing additional information about the API call result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// A collection of validation errors that occurred during the API call, if any.
    /// </summary>
    public IEnumerable<ValidationErrorDetail> Errors { get; set; } = [];
}
