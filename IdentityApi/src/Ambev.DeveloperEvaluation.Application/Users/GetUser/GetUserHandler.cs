using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUser;

/// <summary>
/// Handler for processing GetUserCommand requests
/// </summary>
public class GetUserHandler(IUserRepository userRepository, IMapper mapper)
    : IRequestHandler<GetUserCommand, GetUserResult>
{
    /// <summary>
    /// Handles the GetUserCommand request
    /// </summary>
    /// <param name="request">The GetUser command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user details if found</returns>
    public async Task<GetUserResult> Handle(GetUserCommand request, CancellationToken cancellationToken)
    {
        var validator = new GetUserValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new KeyNotFoundException($"User with ID {request.Id} not found");

        return mapper.Map<GetUserResult>(user);
    }
}
