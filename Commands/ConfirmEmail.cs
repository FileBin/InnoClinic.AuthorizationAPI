using Filebin.Common.Commands.Abstraction;
using Filebin.Common.Util.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Filebin.AuthServer.Commands;

public class ConfirmEmailCommand : ICommand {
    public required string UserId { get; init; }
    public required string Token { get; init; }
}

public sealed class ConfirmEmailValidator : AbstractValidator<ConfirmEmailCommand> {
    public ConfirmEmailValidator() {
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Token).NotEmpty().MaximumLength(1024);
    }
}

public class ConfirmEmailHandler : ICommandHandler<ConfirmEmailCommand> {
    private readonly UserManager<IdentityUser> userManager;

    public ConfirmEmailHandler(UserManager<IdentityUser> userManager) {
        this.userManager = userManager;
    }

    public async Task Handle(ConfirmEmailCommand request, CancellationToken cancellationToken) {
        var user = await userManager.FindByIdAsync(request.UserId);

        if (user is null) {
            throw new NotFoundException("User not found");
        }

        var result = await userManager.ConfirmEmailAsync(user, request.Token);

        if (!result.Succeeded) {
            throw new BadRequestException("Bad token");
        }
    }
}