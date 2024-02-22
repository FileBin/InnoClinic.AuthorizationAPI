using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using FluentValidation;
using MediatR;

using Filebin.Common.Commands.Abstraction;
using Filebin.Common.Util.Exceptions;
using Filebin.Common.Util;
using Filebin.Common.Models.Auth;

namespace Filebin.AuthServer.Commands;

public class GetUserInfoCommand : ICommand<UserInfoDto> {
    public required ClaimsPrincipal User { get; init; }
}

public sealed class GetUserInfoValidator : AbstractValidator<GetUserInfoCommand> {
    public GetUserInfoValidator() {
        RuleFor(x => x.User).NotNull();
    }
}

public class GetUserInfoHandler(UserManager<IdentityUser> userManager) 
: ICommandHandler<GetUserInfoCommand, UserInfoDto> {

    public async Task<UserInfoDto> Handle(GetUserInfoCommand request,
                                                  CancellationToken cancellationToken) {
        var user = await userManager.GetUserAsync(request.User);

        if (user is null) {
            throw new NotFoundException("User not found!");
        }

        return new UserInfoDto {
            Email = user.Email ?? Misc.NullMarker,
            Username = user.UserName ?? Misc.NullMarker,
        };
    }
}