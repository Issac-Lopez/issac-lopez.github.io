using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace McpClientDemo;

static class Connection
{
    // Swap in your server's SSE endpoint
    public const string ServerUrl = "http://localhost:5000/sse";

    public static async Task<IMcpClient> CreateAsync(
        ILoggerFactory? loggerFactory = null,
        CancellationToken ct = default)
    {
        var transport = new SseClientTransport(new SseClientTransportOptions
        {
            Endpoint = new Uri(ServerUrl),
            Name = "mcp-client-demo",
        });

        var options = new McpClientOptions
        {
            ClientInfo = new() { Name = "mcp-client-demo", Version = "1.0.0" },
        };

        return await McpClientFactory.CreateAsync(transport, options, loggerFactory, ct);
    }
}
