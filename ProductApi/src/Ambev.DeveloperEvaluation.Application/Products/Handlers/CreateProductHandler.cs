using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Application.Products.Commands;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.Handlers;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public CreateProductHandler(IProductRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<Product>(request.Product);
        var created = await _repo.CreateAsync(entity, ct);
        return _mapper.Map<ProductDto>(created);
    }
}

