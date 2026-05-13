using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace CSharpRAGKeywordSearch
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            string openAIEndpoint = config["AZURE_OPENAI_ENDPOINT"];
            string openAIDeploymentName = config["AZURE_OPENAI_GPT_NAME"];
            TokenCredential credential = new DefaultAzureCredential();
            AzureOpenAIClient openAIClient = new AzureOpenAIClient(new Uri(openAIEndpoint), credential);

            string searchEndpoint = config["AZURE_SEARCH_ENDPOINT"];
            string searchIndex = config["AZURE_SEARCH_INDEX"];
            // Make sure you enable RBAC on the Keys blade of the Azure Search service
            SearchClient searchClient = new SearchClient(new Uri(searchEndpoint), searchIndex, credential);

            ChatClient chatClient = openAIClient.GetChatClient(openAIDeploymentName);

            List<ChatMessage> chatMessages = new List<ChatMessage>();
            SystemChatMessage systemChatMessage = new SystemChatMessage("You are an AI assistant that extracts technical keyword phrases from natural language text to be used to perform a keyword search against a technical document library.");
            chatMessages.Add(systemChatMessage);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Welcome to Nick's tech document search. Please enter a technical topic you would like to search for.");
            
            string message = string.Empty;
            Console.ForegroundColor = ConsoleColor.Yellow;
            message = Console.ReadLine();
            while (message != "bye")
            {
                string prompt = $"Extract the technical keyword phrases from the following text. Do not make any interpretations just extract the keyword phrases as is from the text.{Environment.NewLine}###{Environment.NewLine}{message}{Environment.NewLine}###";
                UserChatMessage userChatMessage = new UserChatMessage(prompt);
                chatMessages.Add(userChatMessage);
                var completionResponse = chatClient.CompleteChat(chatMessages);

                if (completionResponse.Value.FinishReason == ChatFinishReason.ContentFilter)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The response was filtered due to your question triggering Azure OpenAI's content filtering system. Please rephrase your question and try again.");
                }
                else if (completionResponse.Value.FinishReason == ChatFinishReason.Length)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Your request exceeded the maximum number of tokens.");
                }
                else
                {
                    string searchText = completionResponse.Value.Content[0].Text;
                    chatMessages.Add(new AssistantChatMessage(searchText));
                    Pageable<SearchResult<SearchDocument>> results = Search(searchClient, searchText);
                    Console.ForegroundColor = ConsoleColor.White;
                    foreach (SearchResult<SearchDocument> result in results)
                    {
                        if (result.Document.TryGetValue("metadata_title", out object title))
                        {
                            Console.WriteLine($"Title: {title}");
                        }
                        if (result.Document.TryGetValue("metadata_author", out object author))
                        {
                            Console.WriteLine($"Author: {author}");
                        }
                        if (result.Document.TryGetValue("Category", out object category))
                        {
                            Console.WriteLine($"Category: {category}");
                        }
                        if (result.Document.TryGetValue("SubCategory", out object subCategory))
                        {
                            Console.WriteLine($"SubCategory: {subCategory}");
                        }
                        if (result.Document.TryGetValue("metadata_storage_name", out object storageName))
                        {
                            Console.WriteLine($"File Name: {storageName}");
                        }
                        Console.WriteLine(Environment.NewLine);
                        foreach (string highlight in result.Highlights["content"])
                        {
                            Console.WriteLine(highlight);
                        }
                    }
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                message = Console.ReadLine();
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Goodbye");
        }

        private static Pageable<SearchResult<SearchDocument>> Search(SearchClient searchClient, string searchText)
        {
            SearchOptions searchOptions = new SearchOptions();
            searchOptions.SearchMode = SearchMode.All;
            searchOptions.IncludeTotalCount = true;
            searchOptions.Size = 5;
            searchOptions.HighlightFields.Add("content");
            SearchResults<SearchDocument> searchResults = searchClient.Search<SearchDocument>(searchText, searchOptions);
            Pageable<SearchResult<SearchDocument>> results = searchResults.GetResults();

            return results;
        }
    }
}
