// See https://aka.ms/new-console-template for more information


using genai.Services;

Console.WriteLine("Hello, World!");

IGenAiService genAiService = new GenAiService();
await genAiService.InitiateConversation();
