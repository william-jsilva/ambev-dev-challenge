using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Application.Products.Commands;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.Handlers;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public UpdateProductHandler(IProductRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<Product>(request.Product);
        var updated = await _repo.UpdateAsync(request.Id, entity, ct);
        return updated is null ? null : _mapper.Map<ProductDto>(updated);
    }
}
