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
