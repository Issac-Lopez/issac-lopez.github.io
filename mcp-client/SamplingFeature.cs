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
