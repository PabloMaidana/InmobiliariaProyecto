using MySql.Data.MySqlClient;
using Inmobiliaria.Models;
using System.Data;

public class PropietarioRepositorio
{
    private readonly IConfiguration _cfg;
    public PropietarioRepositorio(IConfiguration cfg) => _cfg = cfg;

    private MySqlConnection NewConn() =>
        new MySqlConnection(_cfg.GetConnectionString("Default"));

    public List<Propietario> GetAll()
    {
        var list = new List<Propietario>();
        using var conn = NewConn();
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM propietarios", conn);
        using var rd = cmd.ExecuteReader();
        while (rd.Read()) list.Add(Map(rd));
        return list;
    }

    private static Propietario Map(IDataRecord r) => new()
    {
        Id = r.GetInt32(r.GetOrdinal("id")),
        Dni = r.GetInt32(r.GetOrdinal("dni")),
        Nombre = r.GetString(r.GetOrdinal("nombre")),
        Apellido = r.GetString(r.GetOrdinal("apellido")),
        Telefono = r.GetString(r.GetOrdinal("telefono")),
        Email = r.GetString(r.GetOrdinal("email"))
    };
}