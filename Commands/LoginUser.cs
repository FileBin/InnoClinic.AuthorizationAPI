using Microsoft.AspNetCore.Identity;
using FluentValidation;
using Filebin.Common.Validation;
using Filebin.Common.Commands.Abstraction;
using Filebin.Common.Util.Exceptions;
using Filebin.Common.Models.Auth;
using Filebin.Domain.Auth.Abstraction.Services;

namespace Filebin.AuthServer.Commands;

public class LoginUserCommand : LoginDto, ICommand<LoginResultDto> {
    public LoginUserCommand(LoginDto other) : base(other) { }
}

public sealed class LoginUserValidator : AbstractValidator<LoginUserCommand> {
    public LoginUserValidator() {
        RuleFor(x => x.Login).LoginValidation();
        RuleFor(x => x.Password).PasswordValidation();
    }
}

public class LoginUserHandler(UserManager<IdentityUser> userManager,
                        SignInManager<IdentityUser> signInManager,
                        ITokenService tokenService) 
                        
                        : ICommandHandler<LoginUserCommand, LoginResultDto> {


    public async Task<LoginResultDto> Handle(LoginUserCommand request, CancellationToken cancellationToken) {
        var user = await userManager.FindByNameAsync(request.Login);
        user = user ?? await userManager.FindByEmailAsync(request.Login);

        if (user is null) {
            throw new BadRequestException("Username or password are invalid");
        }

        var password = request.Password;
        var result = await signInManager.PasswordSignInAsync(user, password, false, false);

        if (!result.Succeeded) {
            throw new BadRequestException("Username or password are invalid");
        }

        

        var pair = await tokenService.GenerateTokenAsync(user);

        return new LoginResultDto {
            AccessToken = pair.AccessToken,
            RefreshToken = pair.RefreshToken,
        };
    }
}