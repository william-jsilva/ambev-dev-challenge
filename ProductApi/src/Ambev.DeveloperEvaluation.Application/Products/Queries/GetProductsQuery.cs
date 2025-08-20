using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.Queries;

public record GetProductsQuery(QueryParameters Query) : IRequest<PaginationResult<ProductDto>>;
