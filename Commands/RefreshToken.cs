using Microsoft.AspNetCore.Identity;
using FluentValidation;
using Filebin.Common.Commands.Abstraction;
using Filebin.Common.Util.Exceptions;
using Filebin.Common.Models.Auth;
using Filebin.Domain.Auth.Abstraction;
using Filebin.Domain.Auth.Abstraction.Services;

namespace Filebin.AuthServer.Commands;


public class RefreshTokenCommand : ICommand<LoginResultDto> {
    public required IUserDescriptor UserDesc { get; set; }
}

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand> {
    public RefreshTokenValidator() {
        RuleFor(x => x.UserDesc.UserId).NotEmpty();
    }
}

public class RefreshTokenCommandHandler(UserManager<IdentityUser> userManager,
                                        ITokenService tokenService)
                                        : ICommandHandler<RefreshTokenCommand, LoginResultDto> {

    public async Task<LoginResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken) {
        var user = await userManager.FindByIdAsync(request.UserDesc.UserId);

        if (user is null) {
            throw new NotFoundException("Username not found");
        }

        var pair = await tokenService.GenerateTokenAsync(user);

        return new LoginResultDto {
            AccessToken = pair.AccessToken,
            RefreshToken = pair.RefreshToken,
        };
    }
}
