using HackerNewsGateway.Infrastructure.Http;
using HackerNewsGatewayApi.Cache;
using HackerNewsGatewayApi.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IStoryCache, StoryCache>();

builder.Services.AddHttpClient<HackerNewsClient>(client =>
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHostedService<StorySyncWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
