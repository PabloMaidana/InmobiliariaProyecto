using Inmobiliaria.Web.Models;
using MySqlConnector;

namespace Inmobiliaria.Web.Data
{
    public class PropietarioRepository : RepositoryBase
    {
        public PropietarioRepository(Db db) : base(db) {}

        private static Propietario Map(MySqlDataReader r) => new()
        {
            Id = r.GetInt32("id"),
            Dni = r.GetString("dni"),
            Nombre = r.GetString("nombre"),
            Apellido = r.GetString("apellido"),
            Telefono = r.IsDBNull(r.GetOrdinal("telefono")) ? null : r.GetString("telefono"),
            Email    = r.IsDBNull(r.GetOrdinal("email"))    ? null : r.GetString("email"),
        };

        public async Task<List<Propietario>> GetAllAsync()
        {
            const string sql = "SELECT * FROM propietarios ORDER BY apellido,nombre";
            var list = new List<Propietario>();
            await using var cn = Db.GetConnection(); await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync()) list.Add(Map(rd));
            return list;
        }

        public async Task<Propietario?> GetAsync(int id)
        {
            const string sql = "SELECT * FROM propietarios WHERE id=@id";
            await using var cn = Db.GetConnection(); await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync()? Map(rd) : null;
        }

        public async Task<int> InsertAsync(Propietario p, int userId)
        {
            const string sql = @"INSERT INTO propietarios(dni,nombre,apellido,telefono,email,created_by)
                                 VALUES(@dni,@nom,@ape,@tel,@em,@u); SELECT LAST_INSERT_ID();";
            await using var cn = Db.GetConnection(); await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@dni", p.Dni);
            cmd.Parameters.AddWithValue("@nom", p.Nombre);
            cmd.Parameters.AddWithValue("@ape", p.Apellido);
            cmd.Parameters.AddWithValue("@tel", (object?)p.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@em", (object?)p.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@u", userId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public Task<int> UpdateAsync(Propietario p) => Db.ExecAsync(
            "UPDATE propietarios SET dni=@dni,nombre=@nom,apellido=@ape,telefono=@tel,email=@em WHERE id=@id",
            c => {
                c.Parameters.AddWithValue("@dni", p.Dni);
                c.Parameters.AddWithValue("@nom", p.Nombre);
                c.Parameters.AddWithValue("@ape", p.Apellido);
                c.Parameters.AddWithValue("@tel", (object?)p.Telefono ?? DBNull.Value);
                c.Parameters.AddWithValue("@em", (object?)p.Email ?? DBNull.Value);
                c.Parameters.AddWithValue("@id", p.Id);
            });

        public Task<int> DeleteAsync(int id) => Db.ExecAsync("DELETE FROM propietarios WHERE id=@id",
            c => c.Parameters.AddWithValue("@id", id));

        public async Task<List<Propietario>> BuscarPorNombreAsync(string q)
        {
            const string sql = "SELECT * FROM propietarios WHERE nombre LIKE @q OR apellido LIKE @q ORDER BY apellido,nombre";
            var list = new List<Propietario>();
            await using var cn = Db.GetConnection(); await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@q", $"%{q}%");
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync()) list.Add(Map(rd));
            return list;
        }
    }
}
