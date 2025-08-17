using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

/// <summary>
/// Profile for mapping between Cart entity and ListCartsResponse
/// </summary>
public class ListCartsProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for ListCarts operation
    /// </summary>
    public ListCartsProfile()
    {
        CreateMap<Cart, ListCartsResult>();
    }
}
