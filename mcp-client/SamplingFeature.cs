using ModelContextProtocol.Protocol.Types;

namespace McpClientDemo;

// Sampling is a server→client capability: the MCP server asks the client
// to perform an LLM inference and return the result.
//
// To enable it, pass a sampling handler when creating the client:
//
//   var options = new McpClientOptions
//   {
//       ClientInfo = ...,
//       Capabilities = new ClientCapabilities
//       {
//           Sampling = new SamplingCapability(),
//       },
//   };
//
// Then register a handler on the underlying server (how to do this depends
// on the SDK version — see McpClientFactory overloads or IMcpClient.SetSamplingHandler).

static class SamplingFeature
{
    // Example handler: wire this up to your LLM of choice.
    public static Task<CreateMessageResult> HandleAsync(
        CreateMessageRequestParams request,
        CancellationToken ct)
    {
        Console.WriteLine("\n[Sampling request from server]");
        Console.WriteLine($"  Max tokens : {request.MaxTokens}");

        if (request.SystemPrompt is { } sys)
            Console.WriteLine($"  System     : {sys}");

        foreach (var msg in request.Messages)
        {
            Console.WriteLine($"  [{msg.Role}] {msg.Content.Text}");
        }

        // TODO: replace with a real LLM call (e.g. Anthropic SDK, OpenAI SDK, etc.)
        var reply = new CreateMessageResult
        {
            Role = Role.Assistant,
            Content = new Content { Type = "text", Text = "<your LLM reply here>" },
            Model = "stub-model",
            StopReason = "end_turn",
        };

        return Task.FromResult(reply);
    }
}
