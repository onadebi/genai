// using Azure.AI.OpenAI;
using System.Collections.Concurrent;
using OpenAI;
using OpenAI.Chat;

namespace genai.Services;

public class GenAiService : IGenAiService
{
    private ConcurrentDictionary<string, List<ChatMessage>> _chatRecordsData;
    private readonly string _key;
    private readonly OpenAIClient _client;
    public GenAiService()
    {
        _key = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
        if (string.IsNullOrEmpty(_key))
        {
            throw new InvalidOperationException("OpenAI API key is not set in environment variables.");
        }
        _client = new OpenAIClient(_key);
        _chatRecordsData = new ConcurrentDictionary<string, List<ChatMessage>>();
    }


    public async Task<(string response, string outChatGuid, string format)> InitiateConversation(string chatMessage, string? chatGuid = null, string? SystemChatMessage = null)
    {
        string deploymentModelName = "gpt-4.1-mini";
        ChatClient chatClient = _client.GetChatClient(deploymentModelName);
        SystemChatMessage systemMessage = new("You are a helpful assistant.");
        string chatGuidKey = chatGuid ?? string.Empty;

        List<ChatMessage> chatHistory = [];
        if (!string.IsNullOrEmpty(chatGuid))
        {
            _chatRecordsData.TryGetValue(chatGuid ?? deploymentModelName, out List<ChatMessage>? outChatHistory);
            chatHistory.AddRange(outChatHistory ?? Enumerable.Empty<ChatMessage>());
        }
        else
        {
            chatGuidKey = Guid.NewGuid().ToString();
            chatHistory =
            [
                new SystemChatMessage(string.IsNullOrWhiteSpace(SystemChatMessage) ? "You are a helpful assistant." : SystemChatMessage),
            ];
        }
        chatHistory.Add(new UserChatMessage(chatMessage));

        ChatCompletion chatCompletion = await chatClient.CompleteChatAsync(chatHistory);
        _chatRecordsData.AddOrUpdate(chatGuidKey, chatHistory, (key, existingValue) =>
        {
            existingValue.Add(new UserChatMessage(chatMessage));
            if (chatCompletion.Content.Any())
            {
                existingValue.Add(new AssistantChatMessage(chatCompletion.Content.FirstOrDefault()?.Text ?? string.Empty));
            }
            return existingValue;
        });
        return (chatCompletion.Content.FirstOrDefault()?.Text ?? string.Empty, chatGuidKey, "text/plain");
    }
}

public interface IGenAiService
{
    Task<(string response, string outChatGuid, string format)> InitiateConversation(string chatMessage, string? chatGuid = null, string? SystemChatMessage = null);
}