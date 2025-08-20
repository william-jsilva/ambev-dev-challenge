using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Application.Products.Queries;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.Handlers;

public class GetProductsHandler(IProductRepository productRepository, IMapper mapper) 
    : IRequestHandler<GetProductsQuery, PaginationResult<ProductDto>>
{
    public async Task<PaginationResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var (items, total) = await productRepository.GetAsync(request.Query, ct);
        var data = mapper.Map<IEnumerable<ProductDto>>(items).ToList();
        var pages = (int)Math.Ceiling((double)total / request.Query.Size);

        return new PaginationResult<ProductDto>
        {
            Data = data,
            TotalItems = total,
            CurrentPage = request.Query.Page,
            TotalPages = pages
        };
    }
}
