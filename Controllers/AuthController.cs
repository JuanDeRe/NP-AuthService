using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using AuthService.Exceptions;
using System.Diagnostics;
using AuthService.Observability;

namespace AuthService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuthMetrics _metrics;

    public AuthController(
        IAuthService authService,
        IAuthMetrics metrics)
    {
        _authService = authService;
        _metrics = metrics;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        _metrics.RecordRegistrationAttempt();
        try
        {
            var response = await _authService.RegisterAsync(
                request,
                cancellationToken
            );

            return StatusCode(
                StatusCodes.Status201Created,
                response
            );
        }
        catch (UsernameAlreadyExistsException exception)
        {
            _metrics.RecordRegistrationFailure();
            return Conflict(new
            {
                message = exception.Message
            });
        }
        catch (ArgumentException exception)
        {
            _metrics.RecordRegistrationFailure();
            return BadRequest(new
            {
                message = exception.Message
            });
        }
        catch
        {
            _metrics.RecordRegistrationFailure();
            throw;
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        _metrics.RecordLoginAttempt();

        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (string.IsNullOrWhiteSpace(
                    request.Username))
            {
                _metrics.RecordLoginFailure();

                return BadRequest(new
                {
                    message = "Username is required."
                });
            }

            if (string.IsNullOrWhiteSpace(
                    request.Password))
            {
                _metrics.RecordLoginFailure();

                return BadRequest(new
                {
                    message = "Password is required."
                });
            }

            var response =
                await _authService.LoginAsync(
                    request,
                    cancellationToken
                );

            if (response is null)
            {
                _metrics.RecordLoginFailure();

                return Unauthorized(new
                {
                    message =
                        "Invalid username or password."
                });
            }

            return Ok(response);
        }
        catch
        {
            _metrics.RecordLoginFailure();
            throw;
        }
        finally
        {
            stopwatch.Stop();

            _metrics.RecordAuthenticationLatency(
                stopwatch.Elapsed.TotalMilliseconds
            );
        }
    }
}