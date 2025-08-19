using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Profile for mapping between Sale entity and CreateSaleResponse
/// </summary>
public class CreateSaleProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CreateSale operation
    /// </summary>
    public CreateSaleProfile()
    {
        CreateMap<CreateSaleCommand, Sale>();
        CreateMap<Sale, CreateSaleResult>();
        CreateMap<SaleProduct, SaleProductDto>();
        CreateMap<CartProduct, SaleProduct>();
        CreateMap<(CreateSaleCommand, Cart), Sale>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Item2.UserId))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Item1.Date))
            .ForMember(dest => dest.Branch, opt => opt.MapFrom(src => src.Item1.Branch))
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Item2.Products));

        ;
    }
}
