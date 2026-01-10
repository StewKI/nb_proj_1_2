namespace NppCore.Configuration;

public class CassandraConfiguration
{
    public string[] ContactPoints { get; set; } = ["localhost"];
    public int Port { get; set; } = 9042;
    public string Keyspace { get; set; } = "npp";
    public string? Username { get; set; }
    public string? Password { get; set; }
}
