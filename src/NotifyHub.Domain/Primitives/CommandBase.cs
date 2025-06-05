using MediatR;

namespace NotifyHub.Domain.Primitives;

public class CommandBase<TResponse> : CommandBaseValidator, IRequest<TResponse>
{
}

public class CommandBase : CommandBaseValidator, IRequest
{
}