using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace McpClientDemo;

static class PromptsFeature
{
    public static async Task ListAsync(IMcpClient client, CancellationToken ct = default)
    {
        var prompts = await client.ListPromptsAsync(ct);

        if (prompts.Count == 0)
        {
            Console.WriteLine("  (no prompts)");
            return;
        }

        foreach (var p in prompts)
        {
            Console.WriteLine($"  {p.Name}");
            if (!string.IsNullOrWhiteSpace(p.Description))
                Console.WriteLine($"    {p.Description}");

            if (p.Arguments is { Count: > 0 } args)
            {
                foreach (var arg in args)
                {
                    var required = arg.Required == true ? "*" : " ";
                    Console.WriteLine($"    [{required}] {arg.Name}: {arg.Description}");
                }
            }
        }
    }

    public static async Task GetAsync(IMcpClient client, CancellationToken ct = default)
    {
        Console.Write("Prompt name: ");
        var name = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(name)) return;

        var args = ReadArguments();

        var result = await client.GetPromptAsync(name, args, ct);

        if (!string.IsNullOrWhiteSpace(result.Description))
            Console.WriteLine($"Description: {result.Description}");

        foreach (var msg in result.Messages)
        {
            Console.WriteLine($"\n[{msg.Role}]");
            if (msg.Content is TextContentBlock tb)
                Console.WriteLine(tb.Text);
        }
    }

    static Dictionary<string, string> ReadArguments()
    {
        var args = new Dictionary<string, string>();
        Console.WriteLine("Enter arguments as  key=value  (blank line to finish):");

        while (true)
        {
            Console.Write("  ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) break;

            var sep = line.IndexOf('=');
            if (sep < 1) continue;

            args[line[..sep].Trim()] = line[(sep + 1)..].Trim();
        }

        return args;
    }
}
