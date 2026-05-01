using HackerNewsGateway.Domain.Options;
using HackerNewsGateway.Infrastructure.Http;
using HackerNewsGatewayApi.Cache;
using HackerNewsGatewayApi.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<HackerNewsOptions>(
    builder.Configuration.GetSection("HackerNews"));

builder.Services.AddSingleton<IStoryCache, StoryCache>();

builder.Services.AddHttpClient<HackerNewsClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<HackerNewsOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
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
