"""
TitanEngine MCP stdio bridge.

Bridges the MCP (Model Context Protocol) stdio transport used by IDE agents to the
engine-embedded tool server (TtEngineMCPServer, TCP 127.0.0.1:8818, line-JSON protocol).

No third-party dependencies: implements the minimal MCP handshake and
tools/list / tools/call over stdio, forwarding calls to the engine.

Usage: python titan_mcp_server.py [--port 8818]
"""
import argparse
import json
import socket
import sys

SERVER_VERSION = "1.0.0"


def engine_call(port, tool, args):
    req = json.dumps({"tool": tool, "args": args or {}})
    with socket.create_connection(("127.0.0.1", port), timeout=180) as sock:
        sock.sendall(req.encode("utf-8") + b"\n")
        buf = b""
        while b"\n" not in buf:
            chunk = sock.recv(65536)
            if not chunk:
                break
            buf += chunk
    if not buf:
        raise RuntimeError("engine closed connection without response")
    return json.loads(buf.decode("utf-8").splitlines()[0])


def list_tools(port):
    # engine tools are dynamic; expose a fixed catalog entry that can list itself
    return {
        "tools": [
            {
                "name": "list_tools",
                "description": "List all tools registered in the engine MCP server",
                "inputSchema": {"type": "object", "properties": {}},
            },
            {
                "name": "ping",
                "description": "Check the engine tool server is alive",
                "inputSchema": {"type": "object", "properties": {}},
            },
            {
                "name": "get_logs",
                "description": "Get recent editor logs",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "count": {"type": "integer", "description": "max lines (1-2000)"},
                        "filter": {"type": "string", "description": "substring filter"},
                    },
                },
            },
            {
                "name": "open_scene",
                "description": "Open a scene asset in the editor (args: name, e.g. 'tutorials/terrain/terrain_scene.scene')",
                "inputSchema": {
                    "type": "object",
                    "properties": {"name": {"type": "string"}},
                    "required": ["name"],
                },
            },
            {
                "name": "goto_level",
                "description": "Goto a terrain level via the level grid editor (args: levelX, levelZ, optional scene)",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "levelX": {"type": "integer"},
                        "levelZ": {"type": "integer"},
                        "scene": {"type": "string"},
                    },
                    "required": ["levelX", "levelZ"],
                },
            },
            {
                "name": "get_level_states",
                "description": "Get terrain level data states of the open scene (optional arg: scene)",
                "inputSchema": {
                    "type": "object",
                    "properties": {"scene": {"type": "string"}},
                },
            },
        ]
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--port", type=int, default=8818)
    args = parser.parse_args()
    port = args.port

    for raw in sys.stdin:
        raw = raw.strip()
        if not raw:
            continue
        try:
            msg = json.loads(raw)
        except json.JSONDecodeError:
            continue

        method = msg.get("method")
        msg_id = msg.get("id")
        is_request = msg_id is not None

        def send(result=None, error=None):
            if not is_request:
                return
            resp = {"jsonrpc": "2.0", "id": msg_id}
            if error is not None:
                resp["error"] = error
            else:
                resp["result"] = result
            sys.stdout.write(json.dumps(resp) + "\n")
            sys.stdout.flush()

        if method == "initialize":
            send({
                "protocolVersion": msg.get("params", {}).get("protocolVersion", "2024-11-05"),
                "capabilities": {"tools": {}},
                "serverInfo": {"name": "titan-engine-mcp", "version": SERVER_VERSION},
            })
        elif method == "notifications/initialized":
            pass
        elif method == "tools/list":
            send(list_tools(port))
        elif method == "tools/call":
            params = msg.get("params", {})
            tool = params.get("name")
            tool_args = params.get("arguments") or {}
            try:
                ret = engine_call(port, tool, tool_args)
                if ret.get("success"):
                    text = ret.get("content", "")
                    send({
                        "content": [{"type": "text", "text": str(text)}],
                        "isError": False,
                    })
                else:
                    send({
                        "content": [{"type": "text", "text": ret.get("error", "unknown error")}],
                        "isError": True,
                    })
            except Exception as exc:
                send({
                    "content": [{"type": "text", "text": f"engine call failed: {exc}"}],
                    "isError": True,
                })
        elif is_request:
            send(error={"code": -32601, "message": f"method not found: {method}"})


if __name__ == "__main__":
    main()
