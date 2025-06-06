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


    public async Task<(string response, string outChatGuid, string format)> InitiateConversation(string chatMessage, string? chatGuid = null, string? systemChatMessage = null)
    {
        string deploymentModelName = "gpt-4.1-mini";
        ChatClient chatClient = _client.GetChatClient(deploymentModelName);
        SystemChatMessage systemMessage = new(systemChatMessage ?? "You are a helpful assistant.");
        string chatGuidKey = chatGuid ?? string.Empty;

        if (string.IsNullOrEmpty(chatGuid))
        {
            chatGuidKey = Guid.NewGuid().ToString();
            _chatRecordsData.TryAdd(chatGuidKey, new List<ChatMessage>()
            {
                new SystemChatMessage(string.IsNullOrWhiteSpace(systemChatMessage) ? "You are a helpful assistant." : systemChatMessage)
            });
        }

        if (_chatRecordsData.TryGetValue(chatGuidKey, out _))
        {
            _chatRecordsData.AddOrUpdate(chatGuidKey, [new UserChatMessage(chatMessage)], (key, existingValue) =>
            {
                existingValue.Add(new UserChatMessage(chatMessage));
                return existingValue;
            });
        }

        ChatCompletion chatCompletion = await chatClient.CompleteChatAsync(_chatRecordsData.FirstOrDefault().Value);
        _chatRecordsData.AddOrUpdate(chatGuidKey, [new AssistantChatMessage(chatCompletion.Content.FirstOrDefault()?.Text ?? string.Empty)], (key, existingValue) =>
        {
            // existingValue.Add(new UserChatMessage(chatMessage));
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
    Task<(string response, string outChatGuid, string format)> InitiateConversation(string chatMessage, string? chatGuid = null, string? systemChatMessage = null);
}