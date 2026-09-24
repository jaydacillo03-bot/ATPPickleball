using System.Text.Json;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
var projectFileProvider = new PhysicalFileProvider(projectDir);
app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = projectFileProvider });
app.UseStaticFiles(new StaticFileOptions { FileProvider = projectFileProvider });

var store = MatchResultStore.Create();

app.MapGet("/api/tabulation/results", async () =>
{
    return Results.Json(await store.GetAllAsync());
});

app.MapPost("/api/tabulation/results", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var match = JsonSerializer.Deserialize<MatchResult>(body, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (match is null) return Results.BadRequest();

    match.Id = string.IsNullOrWhiteSpace(match.Id) ? Guid.NewGuid().ToString() : match.Id;
    match.Division = string.IsNullOrWhiteSpace(match.Division) ? "Open Division" : match.Division;
    match.Round = string.IsNullOrWhiteSpace(match.Round) ? "-" : match.Round;
    match.Court = string.IsNullOrWhiteSpace(match.Court) ? "-" : match.Court;
    match.CreatedAt = string.IsNullOrWhiteSpace(match.CreatedAt) ? DateTimeOffset.UtcNow.ToString("O") : match.CreatedAt;
    match.Winner = string.IsNullOrWhiteSpace(match.Winner) ? (match.ScoreA > match.ScoreB ? match.TeamA : match.TeamB) : match.Winner;

    await store.SaveAsync(match);
    return Results.Ok(new { id = match.Id });
});

app.MapDelete("/api/tabulation/results/{id}", async (string id) =>
{
    await store.DeleteAsync(id);
    return Results.Ok(new { deleted = true });
});

app.Run();
