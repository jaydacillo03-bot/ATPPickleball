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
