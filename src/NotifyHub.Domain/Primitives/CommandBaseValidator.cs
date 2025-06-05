using NotifyHub.Domain.Validators;

namespace NotifyHub.Domain.Primitives;

public class CommandBaseValidator : ICommandBase
{
    public void Validate()
    {
        if (GetType()
                .GetCustomAttributes(typeof(CommandValidationAttribute), true)
                .FirstOrDefault() is not CommandValidationAttribute commandValidationAttribute)
            return;

        var type = commandValidationAttribute.ValidatorType;
        CommandValidator.Validate(type, this);
    }
}