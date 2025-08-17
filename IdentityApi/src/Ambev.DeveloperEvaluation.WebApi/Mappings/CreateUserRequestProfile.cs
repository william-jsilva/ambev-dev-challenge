using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Mappings;

/// <summary>
/// Profile for mapping CreateUserRequest to CreateUserCommand
/// </summary>
public class CreateUserRequestProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CreateUser operation
    /// </summary>
    public CreateUserRequestProfile()
    {
        CreateMap<CreateUserRequest, CreateUserCommand>();
    }
}