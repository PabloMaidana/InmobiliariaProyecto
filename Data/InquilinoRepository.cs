using Inmobiliaria.Web.Models;
using MySqlConnector;

namespace Inmobiliaria.Web.Data
{
    public class InquilinoRepository : RepositoryBase
    {
        public InquilinoRepository(Db db) : base(db) {}

        private static Inquilino Map(MySqlDataReader r) => new()
        {
            Id       = r.GetInt32("id"),
            Dni      = r.GetString("dni"),
            Nombre   = r.GetString("nombre"),
            Telefono = r.IsDBNull(r.GetOrdinal("telefono")) ? null : r.GetString("telefono"),
            Email    = r.IsDBNull(r.GetOrdinal("email"))    ? null : r.GetString("email"),
        };

        public async Task<List<Inquilino>> GetAllAsync()
        {
            const string sql = "SELECT * FROM inquilinos ORDER BY nombre";
            var list = new List<Inquilino>();
            await using var cn = Db.GetConnection(); await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            await using var rd  = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync()) list.Add(Map(rd));
            return list;
        }

        public async Task<Inquilino?> GetAsync(int id)
        {
            const string sql = "SELECT * FROM inquilinos WHERE id=@id";
            await using var cn = Db.GetConnection(); await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        public async Task<int> InsertAsync(Inquilino p, int userId)
        {
            const string sql = @"INSERT INTO inquilinos(dni,nombre,telefono,email,created_by)
                                 VALUES(@dni,@nom,@tel,@em,@u); SELECT LAST_INSERT_ID();";
            await using var cn = Db.GetConnection(); await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@dni", p.Dni);
            cmd.Parameters.AddWithValue("@nom", p.Nombre);
            cmd.Parameters.AddWithValue("@tel", (object?)p.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@em", (object?)p.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@u", userId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public Task<int> UpdateAsync(Inquilino p) => Db.ExecAsync(
            "UPDATE inquilinos SET dni=@dni,nombre=@nom,telefono=@tel,email=@em WHERE id=@id",
            c => {
                c.Parameters.AddWithValue("@dni", p.Dni);
                c.Parameters.AddWithValue("@nom", p.Nombre);
                c.Parameters.AddWithValue("@tel", (object?)p.Telefono ?? DBNull.Value);
                c.Parameters.AddWithValue("@em", (object?)p.Email ?? DBNull.Value);
                c.Parameters.AddWithValue("@id", p.Id);
            });

        public Task<int> DeleteAsync(int id) =>
            Db.ExecAsync("DELETE FROM inquilinos WHERE id=@id", c => c.Parameters.AddWithValue("@id", id));
    }
}
