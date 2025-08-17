using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Profile for mapping between Application and API CreateSale response and request models
/// </summary>
public class CreateSaleProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CreateSale feature
    /// </summary>
    public CreateSaleProfile()
    {
        CreateMap<SaleProductDto, SaleProduct>();
        CreateMap<SaleProduct, SaleProductDto>();

        CreateMap<CreateSaleRequest, CreateSaleCommand>()
            .ForCtorParam("userId", opt => opt.MapFrom(src => src.UserId))
            .ForCtorParam("date", opt => opt.MapFrom(src => src.Date))
            .ForCtorParam("products", opt => opt.MapFrom(src => src.Products));

        CreateMap<CreateSaleResult, CreateSaleResponse>();
    }
}
