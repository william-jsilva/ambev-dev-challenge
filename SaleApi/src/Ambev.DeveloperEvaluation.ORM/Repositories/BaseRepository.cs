using System.Linq.Expressions;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Base repository class providing common functionality for repositories
/// </summary>
public abstract class BaseRepository
{
    public static IQueryable<TQuery> ApplyOrder<TQuery>(string order, IQueryable<TQuery> query,
        Dictionary<string, Expression<Func<TQuery, object>>> allowedFields)
    {
        const string orderingAscending = "asc"; // ToDo: criar classe com constantes para evitar duplicação
        const string orderingDescending = "desc";
        const char commaSeparator = ',';
        const char spaceSeparator = ' ';
        const int amountWithDirection = 2;

        bool first = true;

        var orderParts = order.Split(commaSeparator, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in orderParts)
        {
            var tokens = part.Trim().Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries);
            var field = tokens[0];

            var direction = tokens.Length >= amountWithDirection
                ? tokens[1].ToLowerInvariant()
                : orderingAscending;

            if (!allowedFields.TryGetValue(field, out var keySelector))
                continue; // ignora campos não permitidos

            if (first)
            {
                query = direction == orderingDescending
                    ? query.OrderByDescending(keySelector)
                    : query.OrderBy(keySelector);
                first = false;
            }
            else
            {
                var orderedQuery = (IOrderedQueryable<TQuery>)query;
                query = direction == orderingDescending
                    ? orderedQuery.ThenByDescending(keySelector)
                    : orderedQuery.ThenBy(keySelector);
            }
        }

        return query;
    }
}
