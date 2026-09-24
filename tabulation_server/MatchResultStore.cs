using Google.Cloud.Firestore;
using Microsoft.Data.Sqlite;

public sealed class MatchResult
{
    [FirestoreDocumentId]
    public string Id { get; set; } = "";

    [FirestoreProperty] public string Division { get; set; } = "";
    [FirestoreProperty] public string Round { get; set; } = "";
    [FirestoreProperty] public string TeamA { get; set; } = "";
    [FirestoreProperty] public string TeamB { get; set; } = "";
    [FirestoreProperty] public int ScoreA { get; set; }
    [FirestoreProperty] public int ScoreB { get; set; }
    [FirestoreProperty] public string Court { get; set; } = "";
    [FirestoreProperty] public string CreatedAt { get; set; } = "";
    [FirestoreProperty] public string Winner { get; set; } = "";
}

public interface IMatchResultStore
{
    Task<IReadOnlyList<MatchResult>> GetAllAsync();
    Task SaveAsync(MatchResult match);
    Task DeleteAsync(string id);
}

public static class MatchResultStore
{
    public static IMatchResultStore Create()
    {
        var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
        return string.IsNullOrWhiteSpace(projectId)
            ? new SqliteMatchResultStore()
            : new FirestoreMatchResultStore(FirestoreDb.Create(projectId));
    }
}

public sealed class FirestoreMatchResultStore(FirestoreDb database) : IMatchResultStore
{
    private CollectionReference Matches => database.Collection("tabulation_results");

    public async Task<IReadOnlyList<MatchResult>> GetAllAsync()
    {
        var results = new List<MatchResult>();
        await foreach (var snapshot in Matches.OrderBy("createdAt").StreamAsync())
        {
            results.Add(snapshot.ConvertTo<MatchResult>());
        }

        return results;
    }

    public Task SaveAsync(MatchResult match) => Matches.Document(match.Id).SetAsync(match);

    public Task DeleteAsync(string id) => Matches.Document(id).DeleteAsync();
}

public sealed class SqliteMatchResultStore : IMatchResultStore
{
    private readonly string dbPath;

    public SqliteMatchResultStore()
    {
        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        dbPath = Path.Combine(projectDir, "tabulation_results.sqlite");
        Directory.CreateDirectory(projectDir);

        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS tabulation_results (
                id TEXT PRIMARY KEY, division TEXT NOT NULL, round TEXT NOT NULL,
                team_a TEXT NOT NULL, team_b TEXT NOT NULL, score_a INTEGER NOT NULL,
                score_b INTEGER NOT NULL, court TEXT NOT NULL, created_at TEXT NOT NULL,
                winner TEXT NOT NULL
            );";
        command.ExecuteNonQuery();
    }

    public async Task<IReadOnlyList<MatchResult>> GetAllAsync()
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id, division, round, team_a, team_b, score_a, score_b, court, created_at, winner
            FROM tabulation_results ORDER BY created_at ASC;";

        using var reader = await command.ExecuteReaderAsync();
        var results = new List<MatchResult>();
        while (await reader.ReadAsync())
        {
            results.Add(new MatchResult
            {
                Id = reader.GetString(0), Division = reader.GetString(1), Round = reader.GetString(2),
                TeamA = reader.GetString(3), TeamB = reader.GetString(4), ScoreA = reader.GetInt32(5),
                ScoreB = reader.GetInt32(6), Court = reader.GetString(7), CreatedAt = reader.GetString(8),
                Winner = reader.GetString(9)
            });
        }

        return results;
    }

    public async Task SaveAsync(MatchResult match)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO tabulation_results (id, division, round, team_a, team_b, score_a, score_b, court, created_at, winner)
            VALUES ($id, $division, $round, $teamA, $teamB, $scoreA, $scoreB, $court, $createdAt, $winner)
            ON CONFLICT(id) DO UPDATE SET division = excluded.division, round = excluded.round,
                team_a = excluded.team_a, team_b = excluded.team_b, score_a = excluded.score_a,
                score_b = excluded.score_b, court = excluded.court, created_at = excluded.created_at,
                winner = excluded.winner;";
        command.Parameters.AddWithValue("$id", match.Id);
        command.Parameters.AddWithValue("$division", match.Division);
        command.Parameters.AddWithValue("$round", match.Round);
        command.Parameters.AddWithValue("$teamA", match.TeamA);
        command.Parameters.AddWithValue("$teamB", match.TeamB);
        command.Parameters.AddWithValue("$scoreA", match.ScoreA);
        command.Parameters.AddWithValue("$scoreB", match.ScoreB);
        command.Parameters.AddWithValue("$court", match.Court);
        command.Parameters.AddWithValue("$createdAt", match.CreatedAt);
        command.Parameters.AddWithValue("$winner", match.Winner);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(string id)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM tabulation_results WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id);
        await command.ExecuteNonQueryAsync();
    }

    private SqliteConnection CreateConnection() => new($"Data Source={dbPath}");
}