using Microsoft.Data.Sqlite;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
var dbPath = Path.Combine(projectDir, "tabulation_results.sqlite");
Directory.CreateDirectory(projectDir);

using (var connection = new SqliteConnection($"Data Source={dbPath}"))
{
    connection.Open();
    var command = connection.CreateCommand();
    command.CommandText = @"
        CREATE TABLE IF NOT EXISTS tabulation_results (
            id TEXT PRIMARY KEY,
            division TEXT NOT NULL,
            round TEXT NOT NULL,
            team_a TEXT NOT NULL,
            team_b TEXT NOT NULL,
            score_a INTEGER NOT NULL,
            score_b INTEGER NOT NULL,
            court TEXT NOT NULL,
            created_at TEXT NOT NULL,
            winner TEXT NOT NULL
        );
    ";
    command.ExecuteNonQuery();
}

app.MapGet("/api/tabulation/results", async () =>
{
    using var connection = new SqliteConnection($"Data Source={dbPath}");
    await connection.OpenAsync();
    var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT id, division, round, team_a, team_b, score_a, score_b, court, created_at, winner
        FROM tabulation_results
        ORDER BY created_at ASC;
    ";

    using var reader = await command.ExecuteReaderAsync();
    var results = new List<object>();
    while (await reader.ReadAsync())
    {
        results.Add(new
        {
            id = reader.GetString(0),
            division = reader.GetString(1),
            round = reader.GetString(2),
            teamA = reader.GetString(3),
            teamB = reader.GetString(4),
            scoreA = reader.GetInt32(5),
            scoreB = reader.GetInt32(6),
            court = reader.GetString(7),
            createdAt = reader.GetString(8),
            winner = reader.GetString(9)
        });
    }

    return Results.Json(results);
});

app.MapPost("/api/tabulation/results", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var match = JsonSerializer.Deserialize<Dictionary<string, object>>(body, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (match is null) return Results.BadRequest();

    var id = match.GetValueOrDefault("id")?.ToString() ?? Guid.NewGuid().ToString();
    var division = match.GetValueOrDefault("division")?.ToString() ?? "Open Division";
    var round = match.GetValueOrDefault("round")?.ToString() ?? "-";
    var teamA = match.GetValueOrDefault("teamA")?.ToString() ?? "";
    var teamB = match.GetValueOrDefault("teamB")?.ToString() ?? "";
    var scoreA = Convert.ToInt32(match.GetValueOrDefault("scoreA"));
    var scoreB = Convert.ToInt32(match.GetValueOrDefault("scoreB"));
    var court = match.GetValueOrDefault("court")?.ToString() ?? "-";
    var createdAt = match.GetValueOrDefault("createdAt")?.ToString() ?? DateTimeOffset.UtcNow.ToString("O");
    var winner = match.GetValueOrDefault("winner")?.ToString() ?? (scoreA > scoreB ? teamA : teamB);

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    await connection.OpenAsync();
    var command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO tabulation_results (id, division, round, team_a, team_b, score_a, score_b, court, created_at, winner)
        VALUES ($id, $division, $round, $teamA, $teamB, $scoreA, $scoreB, $court, $createdAt, $winner)
        ON CONFLICT(id) DO UPDATE SET
            division = excluded.division,
            round = excluded.round,
            team_a = excluded.team_a,
            team_b = excluded.team_b,
            score_a = excluded.score_a,
            score_b = excluded.score_b,
            court = excluded.court,
            created_at = excluded.created_at,
            winner = excluded.winner;
    ";
    command.Parameters.AddWithValue("$id", id);
    command.Parameters.AddWithValue("$division", division);
    command.Parameters.AddWithValue("$round", round);
    command.Parameters.AddWithValue("$teamA", teamA);
    command.Parameters.AddWithValue("$teamB", teamB);
    command.Parameters.AddWithValue("$scoreA", scoreA);
    command.Parameters.AddWithValue("$scoreB", scoreB);
    command.Parameters.AddWithValue("$court", court);
    command.Parameters.AddWithValue("$createdAt", createdAt);
    command.Parameters.AddWithValue("$winner", winner);
    await command.ExecuteNonQueryAsync();

    return Results.Ok(new { id });
});

app.MapDelete("/api/tabulation/results/{id}", async (string id) =>
{
    using var connection = new SqliteConnection($"Data Source={dbPath}");
    await connection.OpenAsync();
    var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM tabulation_results WHERE id = $id;";
    command.Parameters.AddWithValue("$id", id);
    await command.ExecuteNonQueryAsync();
    return Results.Ok(new { deleted = true });
});

app.Run();
