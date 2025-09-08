using Inmobiliaria.Web.Models;
using MySqlConnector;

namespace Inmobiliaria.Web.Data
{
    public class InmuebleRepository : RepositoryBase
    {
        public InmuebleRepository(Db db) : base(db) {}

        private static Inmueble Map(MySqlDataReader r)=>new(){
            Id=r.GetInt32("id"),
            PropietarioId=r.GetInt32("propietario_id"),
            Direccion=r.GetString("direccion"),
            Uso=r.GetString("uso"),
            TipoId=r.GetInt32("tipo_id"),
            Ambientes=r.GetInt32("ambientes"),
            Lat = r.IsDBNull(r.GetOrdinal("lat")) ? (decimal?)null : r.GetDecimal("lat"),
            Lng = r.IsDBNull(r.GetOrdinal("lng")) ? (decimal?)null : r.GetDecimal("lng"),
            Precio=r.GetDecimal("precio"),
            Disponible=r.GetBoolean("disponible")
        };

        public async Task<List<Inmueble>> GetAllAsync()
        {
            const string sql="SELECT * FROM inmuebles ORDER BY id DESC";
            var list=new List<Inmueble>();
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlCommand(sql,cn);
            await using var rd=await cmd.ExecuteReaderAsync();
            while(await rd.ReadAsync()) list.Add(Map(rd));
            return list;
        }

        public async Task<Inmueble?> GetAsync(int id){
            const string sql="SELECT * FROM inmuebles WHERE id=@id";
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlCommand(sql,cn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var rd=await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync()? Map(rd):null;
        }

        public async Task<int> InsertAsync(Inmueble i, int userId){
            const string sql=@"INSERT INTO inmuebles(propietario_id,direccion,uso,tipo_id,ambientes,lat,lng,precio,disponible,created_by)
                               VALUES(@p,@d,@u,@t,@a,@lat,@lng,@pr,@disp,@uId); SELECT LAST_INSERT_ID();";
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlCommand(sql,cn);
            cmd.Parameters.AddWithValue("@p", i.PropietarioId);
            cmd.Parameters.AddWithValue("@d", i.Direccion);
            cmd.Parameters.AddWithValue("@u", i.Uso);
            cmd.Parameters.AddWithValue("@t", i.TipoId);
            cmd.Parameters.AddWithValue("@a", i.Ambientes);
            cmd.Parameters.AddWithValue("@lat", (object?)i.Lat ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lng", (object?)i.Lng ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pr", i.Precio);
            cmd.Parameters.AddWithValue("@disp", i.Disponible);
            cmd.Parameters.AddWithValue("@uId", userId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public Task<int> UpdateAsync(Inmueble i)=>Db.ExecAsync(
            @"UPDATE inmuebles SET propietario_id=@p,direccion=@d,uso=@u,tipo_id=@t,ambientes=@a,lat=@lat,lng=@lng,precio=@pr,disponible=@disp WHERE id=@id",
            c=>{
                c.Parameters.AddWithValue("@p", i.PropietarioId);
                c.Parameters.AddWithValue("@d", i.Direccion);
                c.Parameters.AddWithValue("@u", i.Uso);
                c.Parameters.AddWithValue("@t", i.TipoId);
                c.Parameters.AddWithValue("@a", i.Ambientes);
                c.Parameters.AddWithValue("@lat", (object?)i.Lat ?? DBNull.Value);
                c.Parameters.AddWithValue("@lng", (object?)i.Lng ?? DBNull.Value);
                c.Parameters.AddWithValue("@pr", i.Precio);
                c.Parameters.AddWithValue("@disp", i.Disponible);
                c.Parameters.AddWithValue("@id", i.Id);
            });

        public Task<int> SetDisponibleAsync(int id, bool disponible)=>Db.ExecAsync(
            "UPDATE inmuebles SET disponible=@d WHERE id=@id",
            c=>{ c.Parameters.AddWithValue("@d", disponible); c.Parameters.AddWithValue("@id", id);} );

        public async Task<List<Inmueble>> DisponiblesAsync(){
            const string sql="SELECT * FROM inmuebles WHERE disponible=1 ORDER BY precio";
            var list=new List<Inmueble>();
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlCommand(sql,cn);
            await using var rd=await cmd.ExecuteReaderAsync();
            while(await rd.ReadAsync()) list.Add(Map(rd));
            return list;
        }

        public async Task<List<Inmueble>> LibresEntreAsync(DateOnly desde, DateOnly hasta){
            const string sql = @"SELECT i.* FROM inmuebles i
                WHERE i.disponible=1 AND NOT EXISTS (
                   SELECT 1 FROM contratos c
                   WHERE c.inmueble_id=i.id
                     AND c.fecha_inicio <= @hasta AND c.fecha_fin >= @desde
                ) ORDER BY i.precio";
            var list=new List<Inmueble>();
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlCommand(sql,cn);
            cmd.Parameters.AddWithValue("@desde", desde.ToDateTime(TimeOnly.MinValue));
            cmd.Parameters.AddWithValue("@hasta", hasta.ToDateTime(TimeOnly.MinValue));
            await using var rd=await cmd.ExecuteReaderAsync();
            while(await rd.ReadAsync()) list.Add(Map(rd));
            return list;
        }
    }
}
