using System.Collections.Concurrent;
using OpenAI;
using OpenAI.Chat;

namespace genai.Services;

public class GenAiServiceStreaming : IGenAiServiceStreaming
{
    private ConcurrentDictionary<string, List<ChatMessage>> _chatRecordsData;
    private readonly string _key;
    private readonly OpenAIClient _client;
    public GenAiServiceStreaming()
    {
        _key = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
        if (string.IsNullOrEmpty(_key))
        {
            throw new InvalidOperationException("OpenAI API key is not set in environment variables.");
        }
        _client = new OpenAIClient(_key);
        _chatRecordsData = new ConcurrentDictionary<string, List<ChatMessage>>();
    }

    public System.ClientModel.AsyncCollectionResult<StreamingChatCompletionUpdate> GetChatResponseStream(string chatMessage, out string guid, string? chatGuid = null, string? systemChatMessage = null)
    {
        guid = string.Empty;
        string deploymentModelName = "gpt-4.1-mini";
        ChatClient chatClient = _client.GetChatClient(deploymentModelName);
        SystemChatMessage systemMessage = new(systemChatMessage ?? "You are a helpful assistant.");
        string chatGuidKey = chatGuid ?? string.Empty;

        if (string.IsNullOrEmpty(chatGuid))
        {
            guid = Guid.NewGuid().ToString();
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

        System.ClientModel.AsyncCollectionResult<StreamingChatCompletionUpdate> chatCompletion
                = chatClient.CompleteChatStreamingAsync(_chatRecordsData.FirstOrDefault().Value);
        return chatCompletion;
    }


    public async Task<(string response, string outChatGuid, string format)> InitiateConversationStream(string chatMessage, string? chatGuid = null, string? systemChatMessage = null)
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

        System.ClientModel.AsyncCollectionResult<StreamingChatCompletionUpdate> chatCompletion
                = chatClient.CompleteChatStreamingAsync(_chatRecordsData.FirstOrDefault().Value);

        System.Text.StringBuilder sb = new();
        await foreach (StreamingChatCompletionUpdate completionUpdate in chatCompletion)
        {
            if (completionUpdate.ContentUpdate.Count > 0)
            {
                Console.Write(completionUpdate.ContentUpdate[0].Text);
                sb.Append(completionUpdate.ContentUpdate[0].Text);
            }
        }

        _chatRecordsData.AddOrUpdate(chatGuidKey, [new AssistantChatMessage(sb.ToString())], (key, existingValue) =>
        {
            if (sb.Length > 0)
            {
                existingValue.Add(new AssistantChatMessage(sb.ToString()));
            }
            return existingValue;
        });
        return (sb.ToString(), chatGuidKey, "text/plain");
    }
}



public interface IGenAiServiceStreaming
{
    Task<(string response, string outChatGuid, string format)> InitiateConversationStream(string chatMessage, string? chatGuid = null, string? systemChatMessage = null);
    System.ClientModel.AsyncCollectionResult<StreamingChatCompletionUpdate> GetChatResponseStream(string chatMessage,out string guid, string? chatGuid = null, string? systemChatMessage = null);
}