using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.CreateCart;

/// <summary>
/// Profile for mapping between Application and API CreateCart response and request models
/// </summary>
public class CreateCartProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CreateCart feature
    /// </summary>
    public CreateCartProfile()
    {
        CreateMap<CartProductDto, CartProduct>();
        CreateMap<CartProduct, CartProductDto>();

        CreateMap<CreateCartRequest, CreateCartCommand>()
            .ForCtorParam("userId", opt => opt.MapFrom(src => src.UserId))
            .ForCtorParam("date", opt => opt.MapFrom(src => src.Date))
            .ForCtorParam("products", opt => opt.MapFrom(src => src.Products));

        CreateMap<CreateCartResult, CreateCartResponse>();
    }
}
