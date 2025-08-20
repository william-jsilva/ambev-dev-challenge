using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

/// <summary>
/// Profile for mapping GetSale feature requests to commands
/// </summary>
public class ListSalesProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetSale feature
    /// </summary>
    public ListSalesProfile()
    {
        CreateMap<ListSalesRequest, ListSalesCommand>();
        CreateMap<ListSalesResult, ListSalesResponse>();

        CreateMap<Sale, SaleDto>().ReverseMap();
    }
}
