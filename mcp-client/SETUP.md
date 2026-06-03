# MCP Client C# — Project Setup

Recreate this project exactly as described below. Create all files listed, then commit and push to `main`.

## Project specs

- **Language / runtime**: C# / .NET 10
- **Project type**: Console app (single executable)
- **Package**: `ModelContextProtocol` v1.3.0 (official C# MCP SDK)
- **Transport**: HTTP + SSE (`SseClientTransport`)
- **Server URL placeholder**: `http://localhost:5000/sse`
- **Capabilities**: tool listing & calling, resource reading, prompt templates, sampling handler stub

## Files to create

### `McpClient.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>McpClientDemo</RootNamespace>
    <AssemblyName>mcp-client</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <!-- Check https://www.nuget.org/packages/ModelContextProtocol for the latest version -->
    <PackageReference Include="ModelContextProtocol" Version="1.3.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.5" />
  </ItemGroup>

</Project>
```

### `Program.cs`

```csharp
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using McpClientDemo;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

Console.WriteLine($"Connecting to {Connection.ServerUrl} ...");

await using var client = await Connection.CreateAsync(loggerFactory);

Console.WriteLine("Connected.\n");

await RunMenuAsync(client);

static async Task RunMenuAsync(IMcpClient client)
{
    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

    while (!cts.IsCancellationRequested)
    {
        PrintMenu();
        var choice = Console.ReadLine()?.Trim();

        try
        {
            switch (choice)
            {
                case "1":
                    Console.WriteLine("\n-- Tools --");
                    await ToolsFeature.ListAsync(client, cts.Token);
                    break;

                case "2":
                    Console.WriteLine("\n-- Call tool --");
                    await ToolsFeature.CallAsync(client, cts.Token);
                    break;

                case "3":
                    Console.WriteLine("\n-- Resources --");
                    await ResourcesFeature.ListAsync(client, cts.Token);
                    break;

                case "4":
                    Console.WriteLine("\n-- Read resource --");
                    await ResourcesFeature.ReadAsync(client, cts.Token);
                    break;

                case "5":
                    Console.WriteLine("\n-- Prompts --");
                    await PromptsFeature.ListAsync(client, cts.Token);
                    break;

                case "6":
                    Console.WriteLine("\n-- Get prompt --");
                    await PromptsFeature.GetAsync(client, cts.Token);
                    break;

                case "q":
                case "Q":
                    return;

                default:
                    Console.WriteLine("Unknown option.");
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine();
    }
}

static void PrintMenu()
{
    Console.WriteLine("--- MCP Client ---");
    Console.WriteLine("  1  List tools");
    Console.WriteLine("  2  Call a tool");
    Console.WriteLine("  3  List resources");
    Console.WriteLine("  4  Read a resource");
    Console.WriteLine("  5  List prompts");
    Console.WriteLine("  6  Get a prompt");
    Console.WriteLine("  q  Quit");
    Console.Write("> ");
}
```

### `Connection.cs`

```csharp
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
```

### `ToolsFeature.cs`

```csharp
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

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

        var result = await client.CallToolAsync(name, args, cancellationToken: ct);

        Console.WriteLine(result.IsError ? "[error response]" : "[success]");
        foreach (var content in result.Content)
        {
            if (content is TextContentBlock tb)
                Console.WriteLine(tb.Text);
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
```

### `ResourcesFeature.cs`

```csharp
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

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
                Console.WriteLine($"<binary blob, {blob.Blob.Length} bytes>");
        }
    }
}
```

### `PromptsFeature.cs`

```csharp
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
```

### `SamplingFeature.cs`

```csharp
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace McpClientDemo;

// Sampling is a server→client capability: the MCP server asks the client
// to perform an LLM inference and return the result.
//
// Wire it up when building McpClientOptions:
//
//   var options = new McpClientOptions
//   {
//       ClientInfo = ...,
//       Capabilities = new() { Sampling = new() },
//       Handlers = new() { SamplingHandler = SamplingFeature.HandleAsync },
//   };

static class SamplingFeature
{
    // Matches the Func<CreateMessageRequestParams?, IProgress<ProgressNotificationValue>, CancellationToken, ValueTask<CreateMessageResult>>
    // expected by McpClientHandlers.SamplingHandler.
    public static ValueTask<CreateMessageResult> HandleAsync(
        CreateMessageRequestParams? request,
        IProgress<ProgressNotificationValue> progress,
        CancellationToken ct)
    {
        Console.WriteLine("\n[Sampling request from server]");
        Console.WriteLine($"  Max tokens : {request?.MaxTokens}");

        if (request?.SystemPrompt is { } sys)
            Console.WriteLine($"  System     : {sys}");

        foreach (var msg in request?.Messages ?? [])
        {
            var text = msg.Content is TextContentBlock tb ? tb.Text : "<non-text content>";
            Console.WriteLine($"  [{msg.Role}] {text}");
        }

        // TODO: replace with a real LLM call (e.g. Anthropic SDK, OpenAI SDK, etc.)
        var reply = new CreateMessageResult
        {
            Role = Role.Assistant,
            Content = [new TextContentBlock { Text = "<your LLM reply here>" }],
            Model = "stub-model",
            StopReason = CreateMessageResult.StopReasonEndTurn,
        };

        return ValueTask.FromResult(reply);
    }
}
```

## Key SDK facts (verified against ModelContextProtocol 1.3.0)

- `SseClientTransport` and `SseClientTransportOptions` are in `ModelContextProtocol.Client`
- `TextContentBlock`, `TextResourceContents`, `BlobResourceContents`, `Role`, `CreateMessageResult`, etc. are in `ModelContextProtocol.Protocol`
- `CallToolAsync` returns `ValueTask<CallToolResult>` (not `CallToolResponse`)
- `PromptMessage.Content` is a single `ContentBlock` — cast to `TextContentBlock` to read `.Text`
- `SamplingMessage.Content` is `IList<ContentBlock>`
- `CreateMessageResult.Content` is `IList<ContentBlock>` — use `[new TextContentBlock { Text = "..." }]`
- `StopReason` correct value is `CreateMessageResult.StopReasonEndTurn` (string `"endTurn"`)
- Sampling handler signature: `Func<CreateMessageRequestParams?, IProgress<ProgressNotificationValue>, CancellationToken, ValueTask<CreateMessageResult>>`

## How to run

```bash
dotnet run --project McpClient.csproj
```

Update `Connection.ServerUrl` to point at your actual MCP server before running.
