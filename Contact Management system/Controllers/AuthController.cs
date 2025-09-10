using Contact_Management_system.Dtos;
using Contact_Management_system.Managers;
using Microsoft.AspNetCore.Mvc;

namespace Contact_Management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthManager _authManager;
        private readonly Validations _validations;
        public AuthController(IAuthManager authManager, Validations validations)
        {
            _authManager = authManager;
            _validations = validations;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // validate email and password
            var email = model.Email?.Trim() ?? string.Empty;
            if (!_validations.IsValidEmail(email))
                return BadRequest(new { message = "Email format is invalid." });

            var pwdCheck = _validations.ValidatePassword(model.Password);
            if (pwdCheck != "Valid")
                return BadRequest(new { message = pwdCheck });

            var success = _authManager.RegisterUser(model);

            if (!success)
                return BadRequest(new { message = "Registration failed. Email may already exist." });

            return Ok(new { message = "User registered." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = _authManager.LoginUser(model);

            if (!login.success)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(login);
        }
    }
}
