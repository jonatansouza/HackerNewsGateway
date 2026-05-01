using HackerNewsGateway.Infrastructure.Http;
using HackerNewsGatewayApi.Cache;
using HackerNewsGatewayApi.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<StoryCache>();

builder.Services.AddHttpClient<HackerNewsClient>(client =>
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com");
});

builder.Services.AddHostedService<StorySyncWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
