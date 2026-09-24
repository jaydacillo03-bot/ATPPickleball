# Firebase match database setup

The tabulation server stores match results in a Firestore collection named `tabulation_results`.
The browser pages continue using the existing `/api/tabulation/results` endpoints, so no Firebase credentials are exposed to clients.

## 1. Create the Firebase project

1. Open the [Firebase console](https://console.firebase.google.com/).
2. Create or select a Firebase project.
3. Open **Build > Firestore Database** and create the database in production mode.
4. The project ID for this project is `atp-pickleball-f673a`.
5. The default Firestore database is configured in `asia-southeast1` (Singapore).

## 2. Create server credentials

1. In **Project settings > Service accounts**, select **Generate new private key**.
2. Save the downloaded JSON file outside this repository.
3. Set `GOOGLE_APPLICATION_CREDENTIALS` to that file's absolute path.
4. Set `FIREBASE_PROJECT_ID` to the Firebase project ID.

Example in macOS/Linux:

```sh
export FIREBASE_PROJECT_ID="atp-pickleball-f673a"
export GOOGLE_APPLICATION_CREDENTIALS="$HOME/.config/firebase/your-service-account.json"
dotnet run --project tabulation_server/TabulationServer.csproj
```

Example in PowerShell:

```powershell
$env:FIREBASE_PROJECT_ID = "atp-pickleball-f673a"
$env:GOOGLE_APPLICATION_CREDENTIALS = "C:\secure\your-service-account.json"
dotnet run --project tabulation_server/TabulationServer.csproj
```

Never commit the service-account JSON file or put it in a public web folder.

## Data shape

Each document ID is the match ID supplied by the existing tabulation page. The fields are:

- `division` (string)
- `round` (string)
- `teamA` (string)
- `teamB` (string)
- `scoreA` (number)
- `scoreB` (number)
- `court` (string)
- `createdAt` (ISO-8601 string)
- `winner` (string)

The server upserts a document when a match is saved and deletes it when a match is deleted.

## Local SQLite mode

Firestore is the default store for this project. To deliberately use the existing local `tabulation_results.sqlite` file during offline development, set `USE_SQLITE=true` before launching. Do not set it for tournament use.
