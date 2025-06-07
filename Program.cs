// See https://aka.ms/new-console-template for more information


using genai.Services;
using OpenAI.Chat;

Console.WriteLine("\t\t----------------GENAI----------------\n");


Console.WriteLine("Welcome to the GenAI Service. Please enter your chat prompt. Enter \"q\" or \"quit\" to exit!\n");

string chatMessage = Console.ReadLine() ?? string.Empty;
IGenAiService genAiService = new GenAiService();
IGenAiServiceStreaming genAiServiceStreaming = new GenAiServiceStreaming();
string chatGuid = string.Empty  ;
while (!chatMessage.Equals("q", StringComparison.CurrentCultureIgnoreCase) && !chatMessage.Equals("quit", StringComparison.CurrentCultureIgnoreCase))
{
    if (string.IsNullOrEmpty(chatMessage))
    {
        Console.WriteLine("Chat message cannot be empty. Please enter a valid message:");
        chatMessage = Console.ReadLine() ?? string.Empty;
        continue;
    }

    try
    {
        #region Non-Streaming Example
        // var response = await genAiService.InitiateConversation(chatMessage, chatGuid: chatGuid);
        // chatGuid = response.outChatGuid;
        // Console.WriteLine($"Response: {response.response}");
        #endregion

        #region Streaming Example
        var response = genAiServiceStreaming.GetChatResponseStream(chatMessage, chatGuid: chatGuid);
        Console.WriteLine("Chat assistant:");
        await foreach (StreamingChatCompletionUpdate updateResponse in response)
        {
            if (updateResponse.ContentUpdate.Count > 0 && updateResponse is StreamingChatCompletionUpdate update)
            {
                System.Threading.Thread.Sleep(100); // Simulate processing delay
                Console.Write(updateResponse.ContentUpdate[0].Text);
            }
        }
        #endregion
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }

    Console.WriteLine("\n\nEnter your next chat prompt or \"q/quit\" to exit:");
    chatMessage = Console.ReadLine() ?? string.Empty;
}



// {
//     Console.WriteLine("Chat message cannot be empty. Please enter a valid message:");
//     chatMessage = Console.ReadLine() ?? string.Empty;
// }
// if (string.IsNullOrEmpty(chatMessage))
// {
//     Console.WriteLine("Chat message cannot be empty. Exiting...");
//     return;
// }
// await genAiService.InitiateConversation(chatMessage);
