using NotifyHub.Domain.Validators;

namespace NotifyHub.Domain.Exceptions;

public class CommandValidationException : DomainException
{
    public CommandValidationException(IEnumerable<CommandValidationError> messages) : base(
        messages.Select(x => x.ErrorMessage))
    {
    }
}