using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCart;
using Ambev.DeveloperEvaluation.Application.Carts.ListCarts;
using Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;
using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts.CreateCart;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts.DeleteCart;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts.GetCart;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts.ListCarts;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts.UpdateCart;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts;

/// <summary>
/// Controller for managing cart operations
/// </summary>
// [Authorize]
[ApiController]
[Route("api/[controller]")]
public class CartsController(IMediator mediator, IMapper mapper) : BaseController
{
    /// <summary>
    /// Add a new cart
    /// </summary>
    /// <param name="request">The cart creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created cart details</returns>
    /// <remarks>
    /// Return http status 422 when cart active with userId already exists
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateCartResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateCart([FromBody] CreateCartRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateCartRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = mapper.Map<CreateCartCommand>(request);
        var response = await mediator.Send(command, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<CreateCartResponse>
        {
            Success = true,
            Message = "Cart created successfully",
            Data = mapper.Map<CreateCartResponse>(response)
        });
    }

    /// <summary>
    /// Retrieve a list of all carts
    /// </summary>
    /// <param name="request">ordering and pagination settings to list cards</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated carts</returns>
    /// <remarks>
    /// Return http status 404 when cart not found
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseWithData<GetCartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListCarts(ListCartsRequest request, CancellationToken cancellationToken)
    {
        var validator = new ListCartsRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = mapper.Map<ListCartsCommand>(request);
        var response = await mediator.Send(command, cancellationToken);
        var carts = mapper.Map<List<CartDto>>(response.Carts);

        return OkPaginated(new PaginatedList<CartDto>(carts, response.TotalItems, request.Page, request.Size));
    }

    /// <summary>
    /// Update a specific cart
    /// </summary>
    /// <param name="id">The unique identifier of the cart</param>
    /// <param name="request">The cart updating request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated cart details</returns>
    /// <remarks>
    /// Return http status 404 when cart not found
    /// </remarks>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateCartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCart([FromRoute] Guid id, [FromBody] UpdateCartRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpdateCartRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = mapper.Map<UpdateCartCommand>((id, request));
        var result = await mediator.Send(command, cancellationToken);

        return Ok(new ApiResponseWithData<UpdateCartResponse>
        {
            Success = true,
            Message = "Cart updated successfully",
            Data = mapper.Map<UpdateCartResponse>(result)
        });
    }

    /// <summary>
    /// Delete a specific cart
    /// </summary>
    /// <param name="id">The unique identifier of the cart to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response if the cart was deleted</returns>
    /// <remarks>
    /// Return http status 422 when cart is completed. Can't remove a completed cart.
    /// Return http status 404 when cart not found
    /// </remarks>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCart([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new DeleteCartRequest { Id = id };
        var validator = new DeleteCartRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = mapper.Map<DeleteCartCommand>(request.Id);
        await mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse
        {
            Success = true,
            Message = "Cart deleted successfully"
        });
    }

    /// <summary>
    /// Retrieve a specific cart by ID
    /// </summary>
    /// <param name="id">The unique identifier of the cart</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cart details if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetCartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCart([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new GetCartRequest { Id = id };
        var validator = new GetCartRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = mapper.Map<GetCartCommand>(request.Id);
        var result = await mediator.Send(command, cancellationToken);
        var cart = mapper.Map<CartDto>(result);

        return Ok(cart);
    }
}