using Inmobiliaria.Web.Models;

namespace Inmobiliaria.Web.Data
{
    public class PagoRepository : RepositoryBase
    {
        public PagoRepository(Db db) : base(db) {}

        public async Task<int> CrearPagoAsync(Pago p, int userId)
        {
            const string nextN = "SELECT IFNULL(MAX(numero),0)+1 FROM pagos WHERE contrato_id=@c";
            var numero = await Db.ScalarAsync<int>(nextN, c=>c.Parameters.AddWithValue("@c", p.ContratoId));
            const string sql = @"INSERT INTO pagos(contrato_id,numero,fecha,concepto,importe,creado_por)
                                 VALUES(@c,@n,@f,@k,@i,@u); SELECT LAST_INSERT_ID();";
            await using var cn = Db.GetConnection(); await cn.OpenAsync();
            await using var cmd = new MySqlConnector.MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@c", p.ContratoId);
            cmd.Parameters.AddWithValue("@n", numero);
            cmd.Parameters.AddWithValue("@f", p.Fecha.ToDateTime(TimeOnly.MinValue));
            cmd.Parameters.AddWithValue("@k", p.Concepto);
            cmd.Parameters.AddWithValue("@i", p.Importe);
            cmd.Parameters.AddWithValue("@u", userId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public Task EditarConceptoAsync(int id, string concepto) => Db.ExecAsync(
            "UPDATE pagos SET concepto=@k WHERE id=@id AND anulado=0",
            c=>{ c.Parameters.AddWithValue("@k", concepto); c.Parameters.AddWithValue("@id", id); });

        public Task AnularAsync(int id, int userId) => Db.ExecAsync(
            "UPDATE pagos SET anulado=1, anulado_por=@u WHERE id=@id",
            c=>{ c.Parameters.AddWithValue("@u", userId); c.Parameters.AddWithValue("@id", id); });

        public async Task<List<Pago>> ListarPorContratoAsync(int contratoId){
            const string sql="SELECT * FROM pagos WHERE contrato_id=@c ORDER BY numero";
            var list=new List<Pago>();
            await using var cn=Db.GetConnection(); await cn.OpenAsync();
            await using var cmd=new MySqlConnector.MySqlCommand(sql,cn);
            cmd.Parameters.AddWithValue("@c", contratoId);
            await using var rd=await cmd.ExecuteReaderAsync();
            while(await rd.ReadAsync()){
                list.Add(new Pago{
                    Id=rd.GetInt32("id"),
                    ContratoId=rd.GetInt32("contrato_id"),
                    Numero=rd.GetInt32("numero"),
                    Fecha=DateOnly.FromDateTime(rd.GetDateTime("fecha")),
                    Concepto=rd.GetString("concepto"),
                    Importe=rd.GetDecimal("importe"),
                    Anulado=rd.GetBoolean("anulado")
                });
            }
            return list;
        }
    }
}
