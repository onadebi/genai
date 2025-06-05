// using Azure.AI.OpenAI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;

namespace genai.Services;

public class GenAiService : IGenAiService
{

    private ConcurrentDictionary<string, ChatClient> _chatClients = new();
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
    }


    public async Task<(string response, string format)> InitiateConversation(string chatMessage)
    {
        string deploymentModelName = "gpt-4.1-mini";
        ChatClient chatClient = _client.GetChatClient(deploymentModelName);
        var systemMessage = new SystemChatMessage("You are a helpful assistant.");


        List<ChatMessage> messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant."),
        };
        UserChatMessage userPrompt = new UserChatMessage(chatMessage);
        messages.Add(userPrompt);

        ChatCompletion chatCompletion = await chatClient.CompleteChatAsync(messages);

        return (chatCompletion.Content.FirstOrDefault()?.Text ?? string.Empty, "text/plain");

        //OpenAI.Chat.ChatCompletionOptions completionOptions = new();

        // var chatOptions = new ChatRequest {
        //     Model = deploymentModelName,
        //     Messages = new List<Message>
        //     {
        //         new Message
        //         {
        //             Role = "user",
        //             Content = "Hello, how can you assist me today?"
        //         }
        //     }
        // };
    }

}

public interface IGenAiService
{
    Task<(string response, string format)> InitiateConversation(string chatMessage);
}