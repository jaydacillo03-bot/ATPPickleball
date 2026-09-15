# OBS Scoreboard Overlay

Simple browser-source overlay compatible with OBS. Place the `overlay` folder on a web server (or serve locally).

Usage:

- Serve locally from the `overlay` folder, e.g.:

```bash
cd ATPPickleball/overlay
python3 -m http.server 8000
```

- In OBS add a *Browser* source and set the URL to:

```
http://localhost:8000/obs_scoreboard.html?teamAName=Owls&teamBName=Hawks&scoreA=5&scoreB=3
```

- Enable `Control audio via OBS` as needed and check **Shutdown source when not visible** if desired. Enable **Use custom frame rate** only if required. In the source properties enable **Transparent** (or uncheck background color) so the overlay is see-through.

Dynamic updates:

- WebSocket: provide `ws` query param with a ws:// URL. Send JSON like `{"type":"update","scoreA":6}` from any WS server.
- postMessage: the page listens for `window.postMessage({type:'update', scoreA:7})`.
- Browser console: call `OBSScoreboard.update({scoreA:8, scoreB:4, teamAName:'A'})`.

Test WebSocket servers included
--------------------------------

Two small example servers are provided for testing live updates:

- Node (requires `ws`): `ws_server_node.js`
	- Install: `npm install ws`
	- Run server: `node ws_server_node.js`
	- Demo mode (auto updates): `node ws_server_node.js --demo`

- Python (requires `websockets`): `ws_server_python.py`
	- Install: `pip install websockets`
	- Run server: `python3 ws_server_python.py` (default port 8081)
	- Demo mode: `python3 ws_server_python.py --demo`

Use the server URL as the `ws` query parameter in the OBS browser source, for example:

```
http://localhost:8000/obs_scoreboard.html?overlay=1&ws=ws://localhost:8080
```

Node CLI
--------

There is a small Node CLI `ws_cli.js` to send updates to a WebSocket server.

Install dependencies and run from the `overlay` folder:

```bash
npm install
# send a one-off update
node ws_cli.js ws://localhost:8080 --teamA "Owls" --teamB "Hawks" --scoreA 5 --scoreB 3

# demo mode
node ws_cli.js ws://localhost:8080 --demo --teamA "Owls" --teamB "Hawks"
```


