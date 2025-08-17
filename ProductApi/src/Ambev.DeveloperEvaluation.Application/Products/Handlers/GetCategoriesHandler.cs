using Ambev.DeveloperEvaluation.Application.Products.Queries;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.Handlers;

public class GetCategoriesHandler(IProductRepository repo) 
    : IRequestHandler<GetCategoriesQuery, IEnumerable<string>>
{
    public async Task<IEnumerable<string>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        return await repo.GetCategoriesAsync(ct);
    }
}
