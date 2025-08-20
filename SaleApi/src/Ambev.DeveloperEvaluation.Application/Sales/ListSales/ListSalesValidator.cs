using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Validator for ListSalesCommand
/// </summary>
public class ListSalesValidator : AbstractValidator<ListSalesCommand>
{
    private static readonly string[] AllowedFields = ["id", "userId", "date", "minDate", "maxDate"];

    /// <summary>
    /// Initializes validation rules for ListSalesRequest
    /// </summary>
    public ListSalesValidator()
    {
        RuleFor(x => x.Page)
            .Must(page => page >= 1).WithMessage("Page number must be greater than or equal to 1");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100).WithMessage("Size number must be between 1 and 100");

        RuleFor(x => x.Order!)
            .Must(BeValidOrder)
            .WithMessage("Order must be in the format: 'field [asc|desc], field2 [asc|desc]' and fields must be one of: id, userId, date, minDate, maxDate.")
            .When(x => !string.IsNullOrWhiteSpace(x.Order));
    }

    /// <summary>
    /// Validates the order string format and fields
    /// </summary>
    /// <remarks>The span approach is used to optimize performance, despite increasing maintainability and complexity.</remarks>
    /// <param name="order"></param>
    /// <returns></returns>
    private bool BeValidOrder(string order)
    {
        const string orderingAscending = "asc";
        const string orderingDescending = "desc";
        const char commaSeparator = ',';
        const char spaceSeparator = ' ';
        const int indexNotFound = -1;

        // ToDo: dividir em mais m�todos para melhorar a legibilidade, complexidade e manutenibilidade
        // ToDo: unificar os m�todos ListSalesRequestValidator.BeValidOrder e ListSalesValidator.BeValidOrder para evitar duplica��o de c�digo

        var remaining = order.AsSpan();

        while (!remaining.IsEmpty)
        {
            var commaIndex = remaining.IndexOf(commaSeparator);
            ReadOnlySpan<char> part;

            if (commaIndex == indexNotFound)
            {
                part = remaining;
                remaining = [];
            }
            else
            {
                part = remaining[..commaIndex];
                remaining = remaining[(commaIndex + 1)..];
            }

            part = part.Trim();

            if (part.IsEmpty)
                return false;

            var spaceIndex = part.IndexOf(spaceSeparator);
            ReadOnlySpan<char> field;
            ReadOnlySpan<char> direction;

            if (spaceIndex == indexNotFound)
            {
                field = part;
                direction = orderingAscending.AsSpan();
            }
            else
            {
                field = part[..spaceIndex].Trim();
                direction = part[(spaceIndex + 1)..].Trim();
            }

            bool fieldIsValid = false;
            foreach (var allowed in AllowedFields)
            {
                if (field.Equals(allowed.AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    fieldIsValid = true;
                    break;
                }
            }
            if (!fieldIsValid)
                return false;

            if (!direction.Equals(orderingAscending.AsSpan(), StringComparison.OrdinalIgnoreCase) &&
                !direction.Equals(orderingDescending.AsSpan(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }
}
