using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Filebin.AuthServer.Commands;
using Filebin.Common.Models.Auth;

namespace Filebin.AuthServer.Controllers;

[ApiController]
[Route("api/accounts", Name = nameof(AccountsController))]
[Authorize]
public class AccountsController(IMediator mediator, IConfiguration config) : ControllerBase {

    [HttpPost]
    [Route("login", Name = nameof(Login))]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto,
                                           CancellationToken cancellationToken) {
        var command = new LoginUserCommand(dto);

        var response = await mediator.Send(command, cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    [Route("refresh_token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken) {
        var command = new RefreshTokenCommand() {
            UserDesc = new ClaimUserDescriptor() { User = User },
        };

        var response = await mediator.Send(command, cancellationToken);

        return Ok(response);
    }

    [HttpPost]
    [Route("register", Name = nameof(Register))]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto,
                                              CancellationToken cancellationToken) {
        var command = new RegisterUserCommand(dto) { };

        await mediator.Send(command, cancellationToken);

        return Ok();
    }

    [HttpGet]
    [Route("confirm", Name = nameof(ConfirmEmailAsync))]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmailAsync(string userId,
                                                       string token,
                                                       CancellationToken cancellationToken) {
        var command = new ConfirmEmailCommand() {
            UserId = userId,
            Token = token,
        };

        await mediator.Send(command, cancellationToken);

        var loginRoute = config["LoginRoute"];

        if (loginRoute is not null)
            return Redirect(loginRoute);

        return Ok();
    }

    [HttpPost]
    [Route("resend", Name = nameof(ResendEmail))]
    [AllowAnonymous]
    public async Task<IActionResult> ResendEmail([FromBody] string userEmail,
                                                 CancellationToken cancellationToken) {
        var command = new ResendEmailCommand() {
            UserEmail = userEmail,
        };

        await mediator.Send(command, cancellationToken);

        return Ok();
    }

    [HttpPost]
    [Route("forgot_password", Name = nameof(ForgotPassword))]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] string userEmail,
                                         CancellationToken cancellationToken) {
        var command = new ForgotPasswordCommand() {
            UserEmail = userEmail,
        };

        await mediator.Send(command, cancellationToken);

        return Ok();
    }

    [HttpPost]
    [Route("reset_password", Name = nameof(ResetPassword))]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto,
                                                   CancellationToken cancellationToken) {
        var command = new ResetPasswordCommand(dto);

        await mediator.Send(command, cancellationToken);

        return Ok();
    }

    [HttpGet]
    [Route("info", Name = nameof(GetInfo))]
    public async Task<IActionResult> GetInfo(CancellationToken cancellationToken) {
        var command = new GetUserInfoCommand() {
            User = User,
        };

        var response = await mediator.Send(command, cancellationToken);

        return Ok(response);
    }
}
