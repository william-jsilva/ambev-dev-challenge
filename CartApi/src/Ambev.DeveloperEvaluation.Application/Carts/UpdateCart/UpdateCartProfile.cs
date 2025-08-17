using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;

/// <summary>
/// Profile for mapping between Cart entity and UpdateCartResponse
/// </summary>
public class UpdateCartProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for UpdateCart operation
    /// </summary>
    public UpdateCartProfile()
    {
        CreateMap<UpdateCartCommand, Cart>();
    }
}
