using FluentValidation;
using Microsoft.AspNetCore.Identity;

using Filebin.Common.Util;
using Filebin.Common.Util.Exceptions;
using Filebin.Common.Util.LinkGenerator;
using Filebin.Common.Validation;
using Filebin.Common.Commands.Abstraction;

using Filebin.Domain.Auth.Abstraction.Services;

namespace Filebin.AuthServer.Commands;

public class ForgotPasswordCommand : ICommand {
    public required string UserEmail { get; init; }
}

public sealed class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand> {
    public ForgotPasswordValidator() {
        RuleFor(x => x.UserEmail).EmailValidation();
    }
}

public class ForgotPasswordHandler : ICommandHandler<ForgotPasswordCommand> {
    private readonly UserManager<IdentityUser> userManager;

    private readonly IPasswordResetMailService mailService;

    private readonly IConfiguration config;



    public ForgotPasswordHandler(UserManager<IdentityUser> userManager,
                                 IPasswordResetMailService mailService,
                                 IConfiguration config) {
        this.userManager = userManager;
        this.mailService = mailService;
        this.config = config;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken) {
        var user = await userManager.FindByEmailAsync(request.UserEmail);

        if (user is null) {
            throw new NotFoundException($"User with email {request.UserEmail} not found");
        }

        if (!user.EmailConfirmed) {
            throw new BadRequestException("User email is not confirmed!");
        }

        var url = new RouteBasedLinkGenerator() {
            Route = config.GetOrThrow("PasswordResetRoute"),
        };

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        var PasswordResetLink = url.GenerateLink(new { userId = user.Id, token });

        ArgumentNullException.ThrowIfNull(PasswordResetLink);
        ArgumentNullException.ThrowIfNull(user.Email);

        await mailService.SendPasswordResetEmailAsync(user.Email, user.Id, PasswordResetLink);
    }
}