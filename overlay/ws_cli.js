#!/usr/bin/env node
// Simple CLI to send an update to a WebSocket overlay server
// Usage examples:
//   node ws_cli.js ws://localhost:8080 --scoreA 5 --scoreB 3 --teamA "Owls" --teamB "Hawks"
//   node ws_cli.js ws://localhost:8080 --demo

const WebSocket = require('ws');
const argv = require('minimist')(process.argv.slice(2));

function showHelp() {
  console.log('Usage: node ws_cli.js <ws_url> [--scoreA N] [--scoreB N] [--teamA NAME] [--teamB NAME] [--serving 1|2] [--demo]');
}

if (argv._.length < 1) {
  showHelp();
  process.exit(1);
}

const url = argv._[0];

const payload = { type: 'update' };
if (argv.scoreA !== undefined) payload.scoreA = Number(argv.scoreA);
if (argv.scoreB !== undefined) payload.scoreB = Number(argv.scoreB);
if (argv.teamA !== undefined) payload.teamAName = String(argv.teamA);
if (argv.teamB !== undefined) payload.teamBName = String(argv.teamB);
if (argv.serving !== undefined) payload.serving = Number(argv.serving);

const ws = new WebSocket(url);
ws.on('open', () => {
  if (argv.demo) {
    let a = 0, b = 0;
    setInterval(() => {
      if (Math.random() > 0.5) a++; else b++;
      const p = { type: 'update', teamAName: argv.teamA || 'CLI A', teamBName: argv.teamB || 'CLI B', scoreA: a, scoreB: b, serving: a>b?1:2 };
      ws.send(JSON.stringify(p));
      console.log('sent', p);
    }, 2000);
  } else {
    ws.send(JSON.stringify(payload));
    console.log('sent', payload);
    ws.close();
  }
});

ws.on('error', (err) => { console.error('ws error', err); process.exit(1); });
