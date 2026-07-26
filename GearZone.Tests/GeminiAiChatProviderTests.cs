using System.Net;
using System.Text;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.AiChat.Dtos;
using GearZone.Infrastructure.External;
using GearZone.Infrastructure.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GearZone.Tests;

public sealed class GeminiAiChatProviderTests
{
    [Fact]
    public async Task GenerateAsync_PreservesGemini3FunctionCallIdAndMergesToolMetadata()
    {
        var handler = new SequenceHandler(
            """
            {
              "candidates": [{
                "content": {
                  "role": "model",
                  "parts": [{
                    "functionCall": {
                      "name": "request_login",
                      "args": {},
                      "id": "call-gearzone-1"
                    }
                  }]
                },
                "finishReason": "STOP"
              }],
              "usageMetadata": {
                "promptTokenCount": 12,
                "candidatesTokenCount": 3
              }
            }
            """,
            """
            {
              "candidates": [{
                "content": {
                  "role": "model",
                  "parts": [{ "text": "Vui lòng đăng nhập để kiểm tra đơn hàng." }]
                },
                "finishReason": "STOP"
              }],
              "usageMetadata": {
                "promptTokenCount": 18,
                "candidatesTokenCount": 9
              }
            }
            """);
        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/")
        };
        var provider = CreateProvider(http);

        var result = await provider.GenerateAsync(
            new AiChatProviderRequest
            {
                SystemInstruction = "Only use tools.",
                UserMessage = "Đơn hàng của tôi ở đâu?",
                Actor = new AiChatActor(null, "guest-hash")
            },
            (name, _, _) =>
            {
                Assert.Equal("request_login", name);
                return Task.FromResult(new AiToolExecutionResult
                {
                    Json = """{"authenticationRequired":true}""",
                    Metadata = new AiChatMessageMetadataDto
                    {
                        Actions =
                        [
                            new AiSuggestedActionDto
                            {
                                Type = "login",
                                Label = "Đăng nhập",
                                Url = "/Auth/Login"
                            }
                        ]
                    }
                });
            });

        Assert.Equal("Vui lòng đăng nhập để kiểm tra đơn hàng.", result.Text);
        Assert.Single(result.Metadata.Actions);
        Assert.Equal(30, result.InputTokens);
        Assert.Equal(12, result.OutputTokens);
        Assert.Equal(2, handler.RequestBodies.Count);
        Assert.Contains(
            "\"id\":\"call-gearzone-1\"",
            handler.RequestBodies[1],
            StringComparison.Ordinal);
        Assert.Contains(
            "\"functionResponse\"",
            handler.RequestBodies[1],
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateAsync_ReturnsSafeBlockedResponseWithoutCallingTools()
    {
        var handler = new SequenceHandler(
            """{"promptFeedback":{"blockReason":"SAFETY"}}""");
        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/")
        };
        var provider = CreateProvider(http);
        var toolCalls = 0;

        var result = await provider.GenerateAsync(
            new AiChatProviderRequest
            {
                SystemInstruction = "Be safe.",
                UserMessage = "blocked",
                Actor = new AiChatActor(null, "guest-hash")
            },
            (_, _, _) =>
            {
                toolCalls++;
                return Task.FromResult(new AiToolExecutionResult());
            });

        Assert.True(result.WasBlocked);
        Assert.Equal(0, toolCalls);
        Assert.Contains("GearZone products", result.Text, StringComparison.OrdinalIgnoreCase);
    }

    private static GeminiAiChatProvider CreateProvider(HttpClient http) =>
        new(
            http,
            Options.Create(new AiChatSettings
            {
                Enabled = true,
                GeminiApiKey = "test-key",
                GeminiModel = "gemini-3.1-flash-lite",
                MaxOutputTokens = 700
            }),
            NullLogger<GeminiAiChatProvider>.Instance);

    private sealed class SequenceHandler(params string[] responseBodies) : HttpMessageHandler
    {
        private readonly Queue<string> _responses = new(responseBodies);
        public List<string> RequestBodies { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestBodies.Add(
                request.Content is null
                    ? string.Empty
                    : await request.Content.ReadAsStringAsync(cancellationToken));
            var body = _responses.Dequeue();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }
    }
}
