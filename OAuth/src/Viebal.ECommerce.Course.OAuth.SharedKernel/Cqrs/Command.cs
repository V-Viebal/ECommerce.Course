using MediatR;

namespace Viebal.ECommerce.Course.OAuth.SharedKernel.Cqrs;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

public interface ICommand : IRequest<Unit>
{
}

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Unit>
    where TCommand : ICommand
{
}
