using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.DeleteCart;

/// <summary>
/// Profile for mapping DeleteCart feature requests to commands
/// </summary>
public class DeleteCartProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for DeleteCart feature
    /// </summary>
    public DeleteCartProfile()
    {
        CreateMap<Guid, Application.Carts.DeleteCart.DeleteCartCommand>()
            .ConstructUsing(id => new Application.Carts.DeleteCart.DeleteCartCommand(id));
    }
}
