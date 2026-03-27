using MediatR;
using MyProject.Domain.Abstractions;

namespace MyProject.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
