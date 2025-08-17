using Ambev.DeveloperEvaluation.Application.Dtos;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.Commands;

public record CreateProductCommand(ProductDto Product) : IRequest<ProductDto>;
