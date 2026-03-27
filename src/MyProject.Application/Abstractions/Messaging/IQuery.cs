using MediatR;
using MyProject.Domain.Abstractions;

namespace MyProject.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
