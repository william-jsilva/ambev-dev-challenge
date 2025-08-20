using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// Profile for mapping between Application and API UpdateSale response and request models
/// </summary>
public class UpdateSaleProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for UpdateSale feature
    /// </summary>
    public UpdateSaleProfile()
    {
        CreateMap<SaleProductDto, SaleProduct>().ReverseMap();
        CreateMap<UpdateSaleRequest, UpdateSaleResponse>().ReverseMap();
        CreateMap<UpdateSaleResult, UpdateSaleResponse>().ReverseMap();

        CreateMap<(Guid id, UpdateSaleRequest request), UpdateSaleCommand>()
            .ForCtorParam("id", opt => opt.MapFrom(src => src.id))
            .ForCtorParam("userId", opt => opt.MapFrom(src => src.request.UserId))
            .ForCtorParam("date", opt => opt.MapFrom(src => src.request.Date))
            .ForCtorParam("products", opt => opt.MapFrom(src => src.request.Products));
    }
}
