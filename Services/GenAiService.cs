// using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;

namespace genai.Services;

public class GenAiService : IGenAiService
{
    public async Task InitiateConversation()
    {
        string key = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
        var client = new OpenAIClient(key);

        string deploymentModelName = "gpt-4.1-mini";
        ChatClient chatClient = client.GetChatClient(deploymentModelName);
        var systemMessage = new SystemChatMessage("You are a helpful assistant.");



        List<ChatMessage> messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant."),
            new UserChatMessage("Hello, how can you assist me today?"),
        };

        ChatCompletion chatCompletion = await chatClient.CompleteChatAsync(messages);
        Console.WriteLine($"Chat Completion Response: {chatCompletion.Content.FirstOrDefault()?.Text}");
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
    Task InitiateConversation();
}