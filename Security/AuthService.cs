using System.Security.Claims;
using Inmobiliaria.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;

namespace Inmobiliaria.Web.Security
{
    public class AuthService
    {
        private readonly Db _db;
        public AuthService(Db db) => _db = db;

        public async Task<bool> SignInAsync(HttpContext http, string email, string password)
        {
            const string sql = "SELECT id,email,password_hash,rol,nombre,apellido FROM usuarios WHERE email=@e ";
            using var cn = _db.GetConnection(); await cn.OpenAsync();
            using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@e", email);
            using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return false;
            var hash = rd.GetString("password_hash");
            if (!BCrypt.Net.BCrypt.Verify(password, hash)) return false;

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, rd.GetInt32("id").ToString()),
                new Claim(ClaimTypes.Name, rd.GetString("email")),
                new Claim(ClaimTypes.GivenName, rd.GetString("nombre")),
                new Claim(ClaimTypes.Surname, rd.GetString("apellido")),
                new Claim(ClaimTypes.Role, rd.GetString("rol"))
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return true;
        }

        public Task SignOutAsync(HttpContext http) => http.SignOutAsync();
        public static int UserId(ClaimsPrincipal u) => int.Parse(u.FindFirstValue(ClaimTypes.NameIdentifier)!);
        public static bool IsAdmin(ClaimsPrincipal u) => u.IsInRole("Admin");
    }
}
