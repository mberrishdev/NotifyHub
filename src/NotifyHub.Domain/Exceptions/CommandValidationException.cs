using NotifyHub.Domain.Validators;

namespace NotifyHub.Domain.Exceptions;

public class CommandValidationException(IEnumerable<CommandValidationError> messages)
    : DomainException(messages.Select(x => x.ErrorMessage));