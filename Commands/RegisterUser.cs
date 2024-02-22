using Microsoft.AspNetCore.Identity;
using FluentValidation;
using MediatR;

using Filebin.Common.Validation;
using Filebin.Common.Commands.Abstraction;
using Filebin.Common.Util.Exceptions;
using Filebin.Common.Models.Auth;

namespace Filebin.AuthServer.Commands;

public class RegisterUserCommand : RegisterDto, ICommand {
    public RegisterUserCommand(RegisterDto other) : base(other) {}
}

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand> {
    public RegisterUserValidator() {
        RuleFor(x => x.Email).EmailValidation();
        RuleFor(x => x.Username).UsernameValidation();
        RuleFor(x => x.Password).PasswordValidation();
    }
}

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand> {
    private readonly IMediator mediator;
    private readonly UserManager<IdentityUser> userManager;

    public RegisterUserHandler(UserManager<IdentityUser> userManager, IMediator mediator) {
        this.userManager = userManager;
        this.mediator = mediator;
    }

    public async Task Handle(RegisterUserCommand request, CancellationToken cancellationToken) {
        var newUser = new IdentityUser { UserName = request.Username, Email = request.Email };

        var result = await userManager.CreateAsync(newUser, request.Password);

        if (!result.Succeeded) {
            var errors = result.Errors.Select(x => x.Description).Aggregate((x, y) => $"{x}\n{y}");
            throw new BadRequestException(errors);
        }

        newUser = await userManager.FindByNameAsync(newUser.UserName);
        ArgumentNullException.ThrowIfNull(newUser);

        _ = mediator.Send(new SendConfirmationEmailCommand{
            User = newUser,
        });
    }
}