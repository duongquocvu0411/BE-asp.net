using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using webapi.Model; // Đảm bảo đúng namespace của model Admin
using BCrypt.Net;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Admin/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Tìm kiếm user trong cơ sở dữ liệu dựa trên username
            var admin = await _context.Admins.SingleOrDefaultAsync(a => a.Username == request.Username);

            // Nếu user tồn tại và mật khẩu hợp lệ, tiến hành tạo token
            if (admin != null && BCrypt.Net.BCrypt.Verify(request.Password, admin.Password))
            {
                // Tạo JWT token
                var token = GenerateJwtToken(admin.Username);

                return Ok(new
                {
                    status = "Đăng nhập thành công",
                    token = token
                });
            }

            // Nếu không thành công, trả về lỗi xác thực
            return Unauthorized(new { status = "error", message = "Thông tin không hợp lệ, vui lòng kiểm tra lại" });
        }

        // POST: api/Admin/logout
        [HttpPost("logout")]
        [Authorize] // Bảo vệ endpoint, yêu cầu JWT token
        public IActionResult Logout()
        {
            // Server không thực sự xoá token vì JWT là stateless.
            // Client sẽ chịu trách nhiệm xoá token khỏi bộ nhớ sau khi nhận phản hồi thành công.
            return Ok(new { status = "success", message = "Đăng xuất thành công" });
        }

        private string GenerateJwtToken(string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
