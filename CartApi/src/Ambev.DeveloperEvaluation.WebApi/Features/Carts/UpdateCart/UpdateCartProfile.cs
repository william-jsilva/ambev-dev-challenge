using Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;
using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.UpdateCart;

/// <summary>
/// Profile for mapping between Application and API UpdateCart response and request models
/// </summary>
public class UpdateCartProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for UpdateCart feature
    /// </summary>
    public UpdateCartProfile()
    {
        CreateMap<CartProductDto, CartProduct>().ReverseMap();
        CreateMap<UpdateCartRequest, UpdateCartResponse>().ReverseMap();
        CreateMap<UpdateCartResult, UpdateCartResponse>().ReverseMap();

        CreateMap<(Guid id, UpdateCartRequest request), UpdateCartCommand>()
            .ForCtorParam("id", opt => opt.MapFrom(src => src.id))
            .ForCtorParam("userId", opt => opt.MapFrom(src => src.request.UserId))
            .ForCtorParam("date", opt => opt.MapFrom(src => src.request.Date))
            .ForCtorParam("products", opt => opt.MapFrom(src => src.request.Products));
    }
}
