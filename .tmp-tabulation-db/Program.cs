using Microsoft.Data.Sqlite;
using System.IO;

var dbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tabulation_results.sqlite"));
var connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();

using var connection = new SqliteConnection(connectionString);
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

var check = connection.CreateCommand();
check.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='tabulation_results';";

using var reader = check.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine($"TABLE:{reader.GetString(0)}");
}
