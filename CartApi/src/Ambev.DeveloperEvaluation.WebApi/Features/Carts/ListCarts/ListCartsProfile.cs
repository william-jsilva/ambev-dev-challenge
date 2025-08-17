using Ambev.DeveloperEvaluation.Application.Carts.ListCarts;
using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.ListCarts;

/// <summary>
/// Profile for mapping GetCart feature requests to commands
/// </summary>
public class ListCartsProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetCart feature
    /// </summary>
    public ListCartsProfile()
    {
        CreateMap<ListCartsRequest, ListCartsCommand>();
        CreateMap<ListCartsResult, ListCartsResponse>();

        CreateMap<Cart, CartDto>().ReverseMap();
    }
}
