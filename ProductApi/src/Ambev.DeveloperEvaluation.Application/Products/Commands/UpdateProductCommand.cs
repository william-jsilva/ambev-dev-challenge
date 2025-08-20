using Ambev.DeveloperEvaluation.Application.Dtos;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.Commands;

public record UpdateProductCommand(string Id, ProductDto Product) : IRequest<ProductDto?>;
