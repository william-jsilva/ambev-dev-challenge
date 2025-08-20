using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.Queries;

public record GetCategoriesQuery() : IRequest<IEnumerable<string>>;
