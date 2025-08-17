using Ambev.DeveloperEvaluation.Application.Dtos;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.Queries;

public record GetProductByIdQuery(string Id) : IRequest<ProductDto?>;
