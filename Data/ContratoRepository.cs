using Inmobiliaria.Web.Models;
using MySqlConnector;

namespace Inmobiliaria.Web.Data
{
    public class ContratoRepository : RepositoryBase
    {
        public ContratoRepository(Db db) : base(db) {}

        private static Contrato Map(MySqlDataReader r)=>new(){
            Id=r.GetInt32("id"),
            InmuebleId=r.GetInt32("inmueble_id"),
            InquilinoId=r.GetInt32("inquilino_id"),
            Monto=r.GetDecimal("monto"),
            FechaInicio=DateOnly.FromDateTime(r.GetDateTime("fecha_inicio")),
            FechaFin=DateOnly.FromDateTime(r.GetDateTime("fecha_fin")),
            FinalizadoAnticipadoEn = r["finalizado_anticipado_en"] is DBNull ? null : DateOnly.FromDateTime((DateTime)r["finalizado_anticipado_en"]),
            Multa = r["multa"] is DBNull ? null : (decimal?)r.GetDecimal("multa")
        };

        public async Task<List<Contrato>> GetAllAsync(){
            const string sql="SELECT * FROM contratos ORDER BY id DESC";
            var list=new List<Contrato>();
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlCommand(sql,cn);
            await using var rd=await cmd.ExecuteReaderAsync();
            while(await rd.ReadAsync()) list.Add(Map(rd));
            return list;
        }

        public async Task<Contrato?> GetAsync(int id){
            const string sql="SELECT * FROM contratos WHERE id=@id";
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlCommand(sql,cn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var rd=await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync()? Map(rd):null;
        }

        public async Task<bool> HaySolapamientoAsync(int inmuebleId, DateOnly desde, DateOnly hasta, int? excluirId=null){
            const string sql=@"SELECT COUNT(1) FROM contratos
                WHERE inmueble_id=@iid AND fecha_inicio <= @hasta AND fecha_fin >= @desde
                  AND (@excluirId IS NULL OR id<>@excluirId)";
            return (await Db.ScalarAsync<int>(sql, c=>{
                c.Parameters.AddWithValue("@iid", inmuebleId);
                c.Parameters.AddWithValue("@desde", desde.ToDateTime(TimeOnly.MinValue));
                c.Parameters.AddWithValue("@hasta", hasta.ToDateTime(TimeOnly.MinValue));
                c.Parameters.AddWithValue("@excluirId", (object?)excluirId ?? DBNull.Value);
            })) > 0;
        }

        public async Task<int> InsertAsync(Contrato cto, int userId){
            const string sql=@"INSERT INTO contratos(inmueble_id,inquilino_id,monto,fecha_inicio,fecha_fin,creado_por)
                               VALUES(@i,@inq,@m,@fi,@ff,@u); SELECT LAST_INSERT_ID();";
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlCommand(sql,cn);
            cmd.Parameters.AddWithValue("@i", cto.InmuebleId);
            cmd.Parameters.AddWithValue("@inq", cto.InquilinoId);
            cmd.Parameters.AddWithValue("@m", cto.Monto);
            cmd.Parameters.AddWithValue("@fi", cto.FechaInicio.ToDateTime(TimeOnly.MinValue));
            cmd.Parameters.AddWithValue("@ff", cto.FechaFin.ToDateTime(TimeOnly.MinValue));
            cmd.Parameters.AddWithValue("@u", userId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public Task<int> UpdateAsync(Contrato c)=>Db.ExecAsync(
            @"UPDATE contratos SET inmueble_id=@i,inquilino_id=@inq,monto=@m,fecha_inicio=@fi,fecha_fin=@ff WHERE id=@id",
            p=>{
                p.Parameters.AddWithValue("@i", c.InmuebleId);
                p.Parameters.AddWithValue("@inq", c.InquilinoId);
                p.Parameters.AddWithValue("@m", c.Monto);
                p.Parameters.AddWithValue("@fi", c.FechaInicio.ToDateTime(TimeOnly.MinValue));
                p.Parameters.AddWithValue("@ff", c.FechaFin.ToDateTime(TimeOnly.MinValue));
                p.Parameters.AddWithValue("@id", c.Id);
            });

        public Task<int> DeleteAsync(int id)=>Db.ExecAsync("DELETE FROM contratos WHERE id=@id", c=> c.Parameters.AddWithValue("@id", id));

        public async Task TerminarAnticipadoAsync(int id, DateOnly fechaEfectiva, decimal multa, int terminadoPor){
            const string sql=@"UPDATE contratos SET finalizado_anticipado_en=@f, multa=@m, terminado_por=@u WHERE id=@id";
            await Db.ExecAsync(sql, c=>{
                c.Parameters.AddWithValue("@f", fechaEfectiva.ToDateTime(TimeOnly.MinValue));
                c.Parameters.AddWithValue("@m", multa);
                c.Parameters.AddWithValue("@u", terminadoPor);
                c.Parameters.AddWithValue("@id", id);
            });
        }

        public async Task<List<Contrato>> VencenAntesDeAsync(DateOnly hasta){
            const string sql="SELECT * FROM contratos WHERE fecha_fin BETWEEN CURDATE() AND @hasta ORDER BY fecha_fin";
            var list=new List<Contrato>();
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlCommand(sql,cn);
            cmd.Parameters.AddWithValue("@hasta", hasta.ToDateTime(TimeOnly.MinValue));
            await using var rd=await cmd.ExecuteReaderAsync();
            while(await rd.ReadAsync()) list.Add(Map(rd));
            return list;
        }
    }
}
