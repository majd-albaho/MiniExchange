using AuthService.Application.Dto;
using AuthService.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) {
            _authService = authService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken) {
            try {
                var response = await _authService.LoginAsync(request, cancellationToken);
                return Ok(response);
            } catch (UnauthorizedAccessException ex) {
                return Unauthorized(new { message = ex.Message });
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken) {
            try {
                var response = await _authService.RegisterAsync(request, cancellationToken);
                return CreatedAtAction(nameof(Login), response);
            } catch (InvalidOperationException ex) {
                return Conflict(new { message = ex.Message });
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken) {
            try {
                var response = await _authService.RefreshTokenAsync(request, cancellationToken);
                return Ok(response);
            } catch (UnauthorizedAccessException ex) {
                return Unauthorized(new { message = ex.Message });
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken) {
            try {
                await _authService.LogoutAsync(request, cancellationToken);
                return NoContent();
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

