// See https://aka.ms/new-console-template for more information


using genai.Services;

Console.WriteLine("\t\t----------------GENAI----------------\n");


Console.WriteLine("Welcome to the GenAI Service. Please enter your chat prompt. Enter \"q\" or \"quit\" to exit!\n");

string chatMessage = Console.ReadLine() ?? string.Empty;
IGenAiService genAiService = new GenAiService();
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
        var response = await genAiService.InitiateConversation(chatMessage, chatGuid: chatGuid);
        chatGuid = response.outChatGuid;
        Console.WriteLine($"Response: {response.response}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }

    Console.WriteLine("\nPlease enter your next chat prompt or \"q\" to exit:");
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
