using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Psychology.Domain.Entities;
using Psychology.Dtos;
using System.ComponentModel.DataAnnotations;

namespace Psychology.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;

        public AuthController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
        {
            _users = users; _signIn = signIn;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                IsTherapist = dto.IsTherapist,
            };

            var result = await _users.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // Optional: add default roles
            if (dto.IsTherapist)
                await _users.AddToRoleAsync(user, "Therapist");
            else
                await _users.AddToRoleAsync(user, "Client");

            await _signIn.SignInAsync(user, isPersistent: false);
            return Ok(new { user.Id, user.FullName, user.IsTherapist });
        }

        public class LoginDto
        {
            [Required, EmailAddress] public string Email { get; set; } = default!;
            [Required] public string Password { get; set; } = default!;
            public bool RememberMe { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _signIn.PasswordSignInAsync(dto.Email, dto.Password, dto.RememberMe, lockoutOnFailure: true);
            if (!result.Succeeded) return Unauthorized("ایمیل یا رمز عبور اشتباه است.");
            return NoContent();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return NoContent();
        }
    }
}
