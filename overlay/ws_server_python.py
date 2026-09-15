#!/usr/bin/env python3
# Simple WebSocket demo server for testing the OBS overlay
# Usage:
#   pip install websockets
#   python3 ws_server_python.py        # run server
#   python3 ws_server_python.py --demo # send demo updates every 3s

import asyncio
import json
import sys
from websockets import serve

CLIENTS = set()

async def handler(websocket):
    CLIENTS.add(websocket)
    try:
        await websocket.send(json.dumps({"type":"info","message":"Welcome to demo WS server"}))
        async for msg in websocket:
            try:
                print('received:', msg)
            except Exception:
                pass
    finally:
        CLIENTS.remove(websocket)

async def demo_broadcast():
    a = 0
    b = 0
    while True:
        await asyncio.sleep(3)
        if asyncio.get_event_loop().time() % 2 > 1:
            a += 1
        else:
            b += 1
        payload = {"type":"update","teamAName":"Demo A","teamBName":"Demo B","scoreA":a,"scoreB":b,"serving": 1 if a>b else 2}
        text = json.dumps(payload)
        coros = [ws.send(text) for ws in CLIENTS if not ws.closed]
        if coros:
            await asyncio.gather(*coros, return_exceptions=True)
            print('broadcast:', text)

async def main():
    port = int(sys.argv[1]) if len(sys.argv) > 1 and sys.argv[1].isdigit() else 8081
    demo = '--demo' in sys.argv
    async with serve(handler, '0.0.0.0', port):
        print(f'ws server listening on ws://localhost:{port}')
        if demo:
            await demo_broadcast()
        else:
            await asyncio.Future()  # run forever

if __name__ == '__main__':
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print('\nserver stopped')
