using Inmobiliaria.Web.Models;
using MySqlConnector;

namespace Inmobiliaria.Web.Data
{
    public class TipoInmuebleRepository : RepositoryBase
    {
        public TipoInmuebleRepository(Db db) : base(db) {}

        private static TipoInmueble Map(MySqlDataReader r)=>new(){ Id=r.GetInt32("id"), Nombre=r.GetString("nombre") };

        public async Task<List<TipoInmueble>> GetAllAsync() {
            const string sql="SELECT * FROM tipos_inmueble ORDER BY nombre";
            var list=new List<TipoInmueble>();
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlCommand(sql,cn);
            await using var rd=await cmd.ExecuteReaderAsync();
            while(await rd.ReadAsync()) list.Add(Map(rd));
            return list;
        }

        public async Task<TipoInmueble?> GetAsync(int id){
            const string sql="SELECT * FROM tipos_inmueble WHERE id=@id";
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlCommand(sql,cn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var rd=await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync()? Map(rd):null;
        }

        public async Task<int> InsertAsync(TipoInmueble t){
            const string sql=@"INSERT INTO tipos_inmueble(nombre) VALUES(@n); SELECT LAST_INSERT_ID();";
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlCommand(sql,cn);
            cmd.Parameters.AddWithValue("@n", t.Nombre);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public Task<int> UpdateAsync(TipoInmueble t)=>Db.ExecAsync("UPDATE tipos_inmueble SET nombre=@n WHERE id=@id",
            c=>{ c.Parameters.AddWithValue("@n", t.Nombre); c.Parameters.AddWithValue("@id", t.Id);} );

        public Task<int> DeleteAsync(int id)=>Db.ExecAsync("DELETE FROM tipos_inmueble WHERE id=@id",
            c=> c.Parameters.AddWithValue("@id", id));
    }
}
