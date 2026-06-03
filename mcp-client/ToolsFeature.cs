using ModelContextProtocol.Client;

namespace McpClientDemo;

static class ToolsFeature
{
    public static async Task ListAsync(IMcpClient client, CancellationToken ct = default)
    {
        var tools = await client.ListToolsAsync(ct);

        if (tools.Count == 0)
        {
            Console.WriteLine("  (no tools registered)");
            return;
        }

        foreach (var tool in tools)
        {
            Console.WriteLine($"  {tool.Name}");
            if (!string.IsNullOrWhiteSpace(tool.Description))
                Console.WriteLine($"    {tool.Description}");
        }
    }

    public static async Task CallAsync(IMcpClient client, CancellationToken ct = default)
    {
        Console.Write("Tool name: ");
        var name = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(name)) return;

        var args = ReadArguments();

        var response = await client.CallToolAsync(name, args, ct);

        Console.WriteLine(response.IsError ? "[error response]" : "[success]");
        foreach (var content in response.Content)
        {
            if (content.Text is { } text)
                Console.WriteLine(text);
        }
    }

    static Dictionary<string, object?> ReadArguments()
    {
        var args = new Dictionary<string, object?>();
        Console.WriteLine("Enter arguments as  key=value  (blank line to finish):");

        while (true)
        {
            Console.Write("  ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) break;

            var sep = line.IndexOf('=');
            if (sep < 1) continue;

            var key = line[..sep].Trim();
            var val = line[(sep + 1)..].Trim();
            args[key] = val;
        }

        return args;
    }
}
