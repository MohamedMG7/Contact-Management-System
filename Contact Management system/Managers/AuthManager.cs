using Contact_Management_system.DbHelper;
using Contact_Management_system.Dtos;
using Contact_Management_system.Models;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.Eventing.Reader;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Contact_Management_system.Managers
{
    public interface IAuthManager {
        bool RegisterUser(RegisterDto data); 
        LoginResponse LoginUser (LoginDto data);        
    }
    public class AuthManager : IAuthManager
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        public AuthManager(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public bool RegisterUser(RegisterDto data)
        {
            if (data is null || string.IsNullOrWhiteSpace(data.Email) || string.IsNullOrWhiteSpace(data.Password))
                return false;

            // check if the email already exist
            if(_context.Set<ApplicationUser>().Any(u => u.Email == data.Email))
            {
                return false;   
            }

            var hash = HashPassword(data.Password);
            var user = new ApplicationUser
            {
                Email = data.Email,
                Password = hash
            };

           
            _context.Add(user);
            var saved = _context.SaveChanges() > 0;
            return saved;

        }

        public LoginResponse LoginUser(LoginDto data)
        {
            if (data is null || string.IsNullOrWhiteSpace(data.Email) || string.IsNullOrWhiteSpace(data.Password))
                return new LoginResponse(false, null);

            var email = data.Email.Trim().ToLowerInvariant();
            var user = _context.Set<ApplicationUser>().FirstOrDefault(u => u.Email == email);
            if (user is null)
                return new LoginResponse(false, null);

            var ok = VerifyPassword(data.Password, user.Password);
            if (!ok)
                return new LoginResponse(false, null);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!)
            };

            var token = GenerateToken(claims, RememberMe:true);
            return new LoginResponse(true, token);
        }

        private string HashPassword(string password)
        {
            string HashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(password);
            return HashedPassword;
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
        }

        private string GenerateToken(IList<Claim> claims, bool RememberMe)
        {
            var SecretKeyString = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var SecretKeyByte = Encoding.ASCII.GetBytes(SecretKeyString!);
            SecurityKey securityKey = new SymmetricSecurityKey(SecretKeyByte);

            
            SigningCredentials signingCredential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            DateTime tokenExpiration = RememberMe ? DateTime.Now.AddDays(1) : DateTime.Now.AddHours(1);
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken
            (
                claims: claims,
                issuer: issuer,
                audience: audience,
                signingCredentials: signingCredential,
                expires: tokenExpiration
            );

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            string token = handler.WriteToken(jwtSecurityToken);
            return token;
        }
    }
}
