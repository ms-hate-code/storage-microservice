using System.Net;
using System.Net.Sockets;

namespace BuildingBlocks.Web;

public static class GlobalExtension
{
    public static string GetCurrentHost()
    {
        var iPAddress = Environment.GetEnvironmentVariable("HOST_PORT_INSTANCE");
        if (!string.IsNullOrWhiteSpace(iPAddress))
        {
            return iPAddress;
        }
        
        var name = Dns.GetHostName();
        var ip = Dns.GetHostEntry(name).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)
            ?? throw new InvalidOperationException("Cannot get IP address");
        
        var host = ip.ToString();
        var port = GetCurrentPort();
        var schema = GetCurrentScheme();
        
        return $"{schema}://{host}:{port}";

    }

    private static int GetCurrentPort()
    {
        var portVal = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS");

        var isParseSuccess = int.TryParse(portVal, out var port);
        if (!isParseSuccess)
        {
            throw new InvalidOperationException("Cannot get current port");
        }
        return port;
    }

    private static string GetCurrentScheme()
    {
        var schema = Environment.GetEnvironmentVariable("SCHEMA_INSTANCE");
        return string.IsNullOrWhiteSpace(schema) ? "http" : schema;
    }

    private static bool IsDevelopmentEnvironment()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        return env == "Development";
    }

    private static bool IsDockerEnvironment()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        return env == "Docker";
    }
}