using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Application.Dtos;

namespace Ambev.DeveloperEvaluation.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<Rating, RatingDto>().ReverseMap();
    }
}
