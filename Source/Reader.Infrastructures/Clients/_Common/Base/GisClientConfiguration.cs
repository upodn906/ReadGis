namespace Reader.Infrastructures.Clients._Common.Base;

public record GisClientConfiguration
{
    public string Address { get; set; } = "localhost";
    public int MaxRetires { get; set; } = 5;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);
    public string LayersIdJsonKey { get; set; } = "layers";
    public string TablesIdJsonKey { get; set; } = "tables";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? AuthenticationAddress { get; set; }
    public string? Token { get; set; }
}