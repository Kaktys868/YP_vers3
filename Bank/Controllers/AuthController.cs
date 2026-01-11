using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Bank.Models;
using Bank.Context;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserContext _context;

    public AuthController(UserContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel login)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == login.Email && u.IsActive);

            if (user == null)
                return Unauthorized(new AuthResponse { Message = "Пользователь не найден" });

            if (!VerifyPassword(login.Password, user.Password))
                return Unauthorized(new AuthResponse { Message = "Неверный пароль" });

            return Ok(new AuthResponse
            {
                Email = user.Email,
                Name = user.Name,
                UserId = user.Id,
                Message = "Успешная авторизация"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new AuthResponse { Message = $"Ошибка сервера: {ex.Message}" });
        }
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            var computedHash = Convert.ToBase64String(hash);

            return computedHash == storedHash;
        }
        catch
        {
            return false;
        }
    }
}