using MySqlConnector;
using System.Data;

namespace Inmobiliaria.Web.Data
{
    public class Db
    {
        private readonly string _cs;
        public Db(string connectionString) => _cs = connectionString;
        public MySqlConnection GetConnection() => new MySqlConnection(_cs);

        public async Task<int> ExecAsync(string sql, Action<MySqlCommand>? param = null)
        {
            await using var cn = GetConnection();
            await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            param?.Invoke(cmd);
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<T?> ScalarAsync<T>(string sql, Action<MySqlCommand>? param = null)
        {
            await using var cn = GetConnection();
            await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            param?.Invoke(cmd);
            var o = await cmd.ExecuteScalarAsync();
            return o == null || o is DBNull ? default : (T)Convert.ChangeType(o, typeof(T));
        }

        public async Task SeedAdminAsync(string email, string nombre, string apellido, string plainPass)
        {
            var exists = await ScalarAsync<int>("SELECT COUNT(1) FROM usuarios WHERE email=@e", c => c.Parameters.AddWithValue("@e", email));
            if (exists > 0) return;
            var hash = BCrypt.Net.BCrypt.HashPassword(plainPass);
            const string sql = @"INSERT INTO usuarios(email,password_hash,nombre,apellido,rol) VALUES(@e,@p,@n,@a,'Admin')";
            await ExecAsync(sql, c => {
                c.Parameters.AddWithValue("@e", email);
                c.Parameters.AddWithValue("@p", hash);
                c.Parameters.AddWithValue("@n", nombre);
                c.Parameters.AddWithValue("@a", apellido);
            });
        }
    }
}
