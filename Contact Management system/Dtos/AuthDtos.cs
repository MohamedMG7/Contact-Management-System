namespace Contact_Management_system.Dtos
{
    public record RegisterDto(string Email, string Password);
    public record LoginDto(string Email, string Password);
    public record LoginResponse(bool success, string? token);

}
