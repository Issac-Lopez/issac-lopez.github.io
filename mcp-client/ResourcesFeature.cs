using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Types;

namespace McpClientDemo;

static class ResourcesFeature
{
    public static async Task ListAsync(IMcpClient client, CancellationToken ct = default)
    {
        var resources = await client.ListResourcesAsync(ct);

        if (resources.Count == 0)
        {
            Console.WriteLine("  (no resources)");
            return;
        }

        foreach (var r in resources)
        {
            Console.WriteLine($"  {r.Uri}");
            if (!string.IsNullOrWhiteSpace(r.Name))
                Console.WriteLine($"    name: {r.Name}");
            if (!string.IsNullOrWhiteSpace(r.Description))
                Console.WriteLine($"    {r.Description}");
            if (!string.IsNullOrWhiteSpace(r.MimeType))
                Console.WriteLine($"    mime: {r.MimeType}");
        }
    }

    public static async Task ReadAsync(IMcpClient client, CancellationToken ct = default)
    {
        Console.Write("Resource URI: ");
        var uri = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(uri)) return;

        var result = await client.ReadResourceAsync(uri, ct);

        foreach (var content in result.Contents)
        {
            Console.WriteLine($"--- {content.Uri} ({content.MimeType ?? "unknown"}) ---");

            if (content is TextResourceContents text)
                Console.WriteLine(text.Text);
            else if (content is BlobResourceContents blob)
                Console.WriteLine($"<binary blob, {blob.Blob.Length} base64 chars>");
        }
    }
}
