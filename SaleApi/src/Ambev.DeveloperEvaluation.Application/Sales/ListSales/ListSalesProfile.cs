using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Profile for mapping between Sale entity and ListSalesResponse
/// </summary>
public class ListSalesProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for ListSales operation
    /// </summary>
    public ListSalesProfile()
    {
        CreateMap<Sale, ListSalesResult>();
    }
}
