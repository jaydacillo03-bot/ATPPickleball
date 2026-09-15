#!/usr/bin/env node
// Simple WebSocket demo server for testing the OBS overlay
// Usage:
//   npm install ws    # first-time
//   node ws_server_node.js        # run server
//   node ws_server_node.js --demo # send demo updates every 3s

const WebSocket = require('ws');
const port = process.env.PORT || 8080;
const wss = new WebSocket.Server({ port }, () => console.log(`ws server listening on ws://localhost:${port}`));

wss.on('connection', (ws) => {
  console.log('client connected');
  ws.send(JSON.stringify({ type: 'info', message: 'Welcome to demo WS server' }));
  ws.on('message', (msg) => {
    console.log('received:', msg.toString());
  });
  ws.on('close', () => console.log('client disconnected'));
});

// Demo mode: broadcast incremental scores
if (process.argv.includes('--demo')) {
  let a = 0, b = 0;
  setInterval(() => {
    // simple increment logic
    if (Math.random() > 0.5) a++; else b++;
    const payload = { type: 'update', teamAName: 'Demo A', teamBName: 'Demo B', scoreA: a, scoreB: b, serving: (a>b?1:2) };
    const text = JSON.stringify(payload);
    wss.clients.forEach((c) => { if (c.readyState === WebSocket.OPEN) c.send(text); });
    console.log('broadcast:', text);
  }, 3000);
}
