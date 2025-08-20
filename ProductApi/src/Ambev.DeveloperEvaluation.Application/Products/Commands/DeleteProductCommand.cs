using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.Commands;

public record DeleteProductCommand(string Id) : IRequest;
