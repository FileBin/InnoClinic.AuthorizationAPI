using FluentValidation;
using Filebin.Common.Commands.Abstraction;
using Microsoft.AspNetCore.Identity;
using Filebin.Common.Util;
using Filebin.Common.Util.Exceptions;
using Filebin.Common.Util.LinkGenerator;
using Filebin.Domain.Auth.Abstraction.Services;

namespace Filebin.AuthServer.Commands;

public class SendConfirmationEmailCommand : ICommand {
    public required IdentityUser User { get; init; }
}

public sealed class SendConfirmationEmailValidator : AbstractValidator<SendConfirmationEmailCommand> {
    public SendConfirmationEmailValidator() {
        RuleFor(x => x.User).NotNull();
    }
}

public class SendConfirmationEmailHandler : ICommandHandler<SendConfirmationEmailCommand> {
    private readonly UserManager<IdentityUser> userManager;
    private readonly IConfirmationMailService mailService;
    private readonly IConfiguration config;

    public SendConfirmationEmailHandler(UserManager<IdentityUser> userManager,
                                        IConfirmationMailService mailService,
                                        IConfiguration config) {
        this.userManager = userManager;
        this.mailService = mailService;
        this.config = config;
    }

    public async Task Handle(SendConfirmationEmailCommand request, CancellationToken cancellationToken) {
        var user = request.User;

        if (user.EmailConfirmed) {
            throw new BadRequestException("Already confirmed!");
        }

        var url = new RouteBasedLinkGenerator() {
            Route = config.GetOrThrow("ConfirmEmailRoute"),
        };

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink = url.GenerateLink(new { userId = user.Id, token });

        ArgumentNullException.ThrowIfNull(confirmationLink);
        ArgumentNullException.ThrowIfNull(user.Email);

        await mailService.SendConfirmationEmailAsync(user.Email, user.Id, confirmationLink);
    }
}