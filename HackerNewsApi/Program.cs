using HackerNewsApi.Caching;
using HackerNewsApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IStoryCacheService, StoryCacheService>();

builder.Services.AddHttpClient<IHackerNewsService, HackerNewsService>(
    client =>
    {
        client.BaseAddress = new Uri(
            "https://hacker-news.firebaseio.com/v0/");

        client.Timeout = TimeSpan.FromSeconds(10);

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Santander-HackerNews-API/1.0");
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();