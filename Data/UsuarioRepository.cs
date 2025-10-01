using Inmobiliaria.Web.Models;
using MySqlConnector;

namespace Inmobiliaria.Web.Data
{
    public class UsuarioRepository : RepositoryBase
    {
        public UsuarioRepository(Db db) : base(db) {}

        private static Usuario Map(MySqlDataReader r) => new()
        {
            Id = r.GetInt32("id"),
            Email = r.GetString("email"),
            Nombre = r.GetString("nombre"),
            Apellido = r.GetString("apellido"),
            Rol = r.GetString("rol"),
            AvatarUrl = r.IsDBNull(r.GetOrdinal("avatar_url")) ? null : r.GetString("avatar_url"),
        };

        public async Task<List<Usuario>> GetAllAsync()
        {
            const string sql = "SELECT * FROM usuarios ORDER BY apellido, nombre";
            var list = new List<Usuario>();
            await using var cn = Db.GetConnection(); await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync()) list.Add(Map(rd));
            return list;
        }

        public async Task<Usuario?> GetAsync(int id)
        {
            const string sql = "SELECT * FROM usuarios WHERE id=@id ";
            await using var cn = Db.GetConnection(); await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        public async Task<int> InsertAsync(Usuario u, string plainPassword)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            const string sql = @"INSERT INTO usuarios(email,password_hash,nombre,apellido,rol)
                                 VALUES(@e,@p,@n,@a,@r); SELECT LAST_INSERT_ID();";
            await using var cn = Db.GetConnection(); await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@e", u.Email);
            cmd.Parameters.AddWithValue("@p", hash);
            cmd.Parameters.AddWithValue("@n", u.Nombre);
            cmd.Parameters.AddWithValue("@a", u.Apellido);
            cmd.Parameters.AddWithValue("@r", u.Rol);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public Task<int> UpdateAsync(Usuario u) => Db.ExecAsync(
            "UPDATE usuarios SET email=@e,nombre=@n,apellido=@a,rol=@r WHERE id=@id",
            c => {
                c.Parameters.AddWithValue("@e", u.Email);
                c.Parameters.AddWithValue("@n", u.Nombre);
                c.Parameters.AddWithValue("@a", u.Apellido);
                c.Parameters.AddWithValue("@r", u.Rol);
                c.Parameters.AddWithValue("@id", u.Id);
            });

        public Task<int> DeleteAsync(int id) =>
            Db.ExecAsync("DELETE FROM usuarios WHERE id=@id", c => c.Parameters.AddWithValue("@id", id));

        public Task<int> UpdatePasswordAsync(int id, string newPlainPassword)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(newPlainPassword);
            return Db.ExecAsync("UPDATE usuarios SET password_hash=@p WHERE id=@id",
                c => { c.Parameters.AddWithValue("@p", hash); c.Parameters.AddWithValue("@id", id); });
        }

        public Task<int> UpdateAvatarAsync(int id, string? avatarUrl) =>
            Db.ExecAsync("UPDATE usuarios SET avatar_url=@a WHERE id=@id",
                c => { c.Parameters.AddWithValue("@a", (object?)avatarUrl ?? DBNull.Value); c.Parameters.AddWithValue("@id", id); });
    }
}
