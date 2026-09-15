# ATP Pickleball — Overlay & Scoreboard

This repository contains a scoreboard web UI and an OBS-compatible transparent overlay.

Quick start

1. Serve the `overlay` folder locally:

```bash
cd ATPPickleball/overlay
python3 -m http.server 8000
```

2. In OBS add a *Browser* source and point it to:

```
http://localhost:8000/obs_scoreboard.html?overlay=1
```

3. Make sure the Browser source has a transparent background (or uncheck the background color) so the overlay is see-through.

Tools included

- `overlay/ws_server_node.js` — Node WebSocket demo server.
- `overlay/ws_server_python.py` — Python WebSocket demo server.
- `overlay/ws_cli.js` — Node CLI for sending updates.
- `overlay/obs_scoreboard.html` — lightweight overlay page for OBS.

OBS tips

- Use `?ws=ws://host:port` on the overlay URL to connect directly to a WebSocket server for live updates.
- Alternatively, open the overlay with query params to set initial values, e.g. `?overlay=1&teamAName=Owls&scoreA=5`.

Local test sender

Open the main UI (`index.html`) and click **Test Sender** in the header to send updates to a WebSocket server or open the overlay with provided query params.
