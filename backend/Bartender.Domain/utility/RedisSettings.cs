
namespace Bartender.Domain.Utility;

public class RedisSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 6379;
    public string Password { get; set; } = string.Empty;
    public bool Ssl { get; set; } = true;
    public bool AbortOnConnectFail { get; set; } = false;
}

