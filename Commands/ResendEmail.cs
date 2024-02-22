using Microsoft.AspNetCore.Identity;
using FluentValidation;
using MediatR;
using Filebin.Common.Validation;
using Filebin.Common.Commands.Abstraction;
using Filebin.Common.Util.Exceptions;

namespace Filebin.AuthServer.Commands;


public class ResendEmailCommand : ICommand {
    public required string UserEmail { get; init; }
}

public sealed class ResendEmailValidator : AbstractValidator<ResendEmailCommand> {
    public ResendEmailValidator() {
        RuleFor(x => x.UserEmail).EmailValidation();
    }
}

public class ResendEmailHandler : ICommandHandler<ResendEmailCommand> {
    private readonly UserManager<IdentityUser> userManager;
    private readonly IMediator mediator;

    public ResendEmailHandler(UserManager<IdentityUser> userManager, IMediator mediator) {
        this.userManager = userManager;
        this.mediator = mediator;
    }

    public async Task Handle(ResendEmailCommand request, CancellationToken cancellationToken) {
        var user = await userManager.FindByEmailAsync(request.UserEmail);

        if (user is null) {
            throw new NotFoundException($"User with email ${request.UserEmail} not found");
        }

        _ = mediator.Send(new SendConfirmationEmailCommand {
            User = user,
        });
    }
}