using Microsoft.AspNetCore.Identity;
using FluentValidation;

using Filebin.Common.Commands.Abstraction;
using Filebin.Common.Util.Exceptions;
using Filebin.Common.Models.Auth;

namespace Filebin.AuthServer.Commands;


public class ResetPasswordCommand : ResetPasswordDto, ICommand {
    public ResetPasswordCommand(ResetPasswordDto other) : base(other) { }
    public ResetPasswordCommand(string userId, string token, string newPassword) : base(userId, token, newPassword) { }
}

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand> {
    public ResetPasswordValidator() {
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Token).NotEmpty().MaximumLength(1024);
    }
}

public class ResetPasswordHandler : ICommandHandler<ResetPasswordCommand> {
    private readonly UserManager<IdentityUser> userManager;

    public ResetPasswordHandler(UserManager<IdentityUser> userManager) {
        this.userManager = userManager;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken) {
        var user = await userManager.FindByIdAsync(request.UserId);

        if (user is null) {
            throw new NotFoundException("User not found");
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded) {
            throw new BadRequestException(result.Errors.Select(x => x.ToString()).Aggregate((x, y) => $"{x}\n{y}"));
        }
    }
}