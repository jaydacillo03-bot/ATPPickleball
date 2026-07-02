$projectDir = Resolve-Path '..'
$dbPath = Join-Path $projectDir 'tabulation_results.sqlite'
Write-Host "DB_PATH=$dbPath"
$connectionString = "Data Source=$dbPath"
$connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
$connection.Open()
$command = $connection.CreateCommand()
$command.CommandText = @"
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
"@
$command.ExecuteNonQuery() | Out-Null
Write-Host 'TABLE_READY'
$connection.Close()
