using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// const string modelId = "gpt-4o";
const string modelId = "gpt-4o-mini";
var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
builder.Services.AddOpenAIChatCompletion(modelId, apiKey: openAiApiKey);

var openWeatherApiKey = Environment.GetEnvironmentVariable("OPENAI_WEATHER_API_KEY")!;
var openWeatherApiSpec = File.ReadAllText("OpenAPI.FixedSK.yaml").Replace("YOUR_API_KEY", openWeatherApiKey);
var kernelSingleton = Kernel.CreateBuilder().Build();
await kernelSingleton.ImportPluginFromOpenApiAsync(
    "OpenWeatherAPI",
    new MemoryStream(Encoding.UTF8.GetBytes(openWeatherApiSpec)),
#pragma warning disable SKEXP0040
    new OpenApiFunctionExecutionParameters(),
#pragma warning restore SKEXP0040
    CancellationToken.None);
builder.Services.AddSingleton(kernelSingleton);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapGet("/weather", async (string text, IChatCompletionService completionService, Kernel kernel,  CancellationToken token) =>
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("""
                                     This assistant provides real-time weather information for cities mentioned in the user's input. It fetches data from OpenWeatherMap and returns the current temperature for each requested city. The assistant supports multiple locations in a single query and ensures clear, concise responses.
                                     
                                     If a city cannot be found or the API request fails, the assistant will politely apologize and inform the user that the requested information is unavailable. It does not provide additional weather details like humidity or wind speed unless specifically requested.
                                     
                                     The assistant prioritizes accuracy, ensuring that it correctly identifies cities and provides relevant temperature data based on OpenWeatherMap's API.
                                     """);
        chatHistory.AddUserMessage(text);
        var response = await completionService.GetChatMessageContentsAsync(chatHistory, new OpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        }, kernel, token);
        return response[0].Content;
    })
    .WithName("GetWeather")
    .WithOpenApi();

app.Run();