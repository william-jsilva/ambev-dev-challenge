using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Application.Products.Queries;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.Handlers;

public class GetProductsByCategoryHandler : IRequestHandler<GetProductsByCategoryQuery, PaginationResult<ProductDto>>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public GetProductsByCategoryHandler(IProductRepository repo, IMapper mapper)
    {
        _repo = repo; _mapper = mapper;
    }

    public async Task<PaginationResult<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken ct)
    {
        return null;

        //var (items, total) = await _repo.GetByCategoryAsync(request.Category, request.Query, ct);
        //var data = _mapper.Map<IEnumerable<ProductDto>>(items).ToList();
        //var pages = (int)Math.Ceiling((double)total / request.Query.Size);

        //return new PaginationResult<ProductDto>
        //{
        //    Data = data,
        //    TotalItems = total,
        //    CurrentPage = request.Query.Page,
        //    TotalPages = pages
        //};
    }
}
