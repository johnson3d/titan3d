#!/usr/bin/env python3
"""直连引擎内置 MCP server (TCP 8818) 的最小客户端。
用法: python engine_call.py <tool> [json_args]
例如: python engine_call.py get_logs '{"count":100,"filter":"TerrainLevelGrid"}'
"""
import json
import socket
import sys

HOST = "127.0.0.1"
PORT = 18118


def call(tool: str, args: dict | None = None) -> str:
    req = json.dumps({"tool": tool, "args": args or {}})
    with socket.create_connection((HOST, PORT), timeout=180) as s:
        s.sendall((req + "\n").encode("utf-8"))
        buf = b""
        while True:
            chunk = s.recv(65536)
            if not chunk:
                break
            buf += chunk
            if b"\n" in buf:
                break
    line = buf.split(b"\n", 1)[0].decode("utf-8")
    resp = json.loads(line)
    if resp.get("success"):
        return resp.get("content", "")
    return f"[ERROR] {resp.get('error')}"


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    tool = sys.argv[1]
    args = {}
    if len(sys.argv) > 2:
        raw = " ".join(sys.argv[2:])
        try:
            args = json.loads(raw)
        except json.JSONDecodeError:
            # fallback: key=value 形式, 避开 PowerShell 引号转义问题
            for tok in sys.argv[2:]:
                if "=" in tok:
                    k, v = tok.split("=", 1)
                    try:
                        args[k] = json.loads(v)
                    except json.JSONDecodeError:
                        args[k] = v
    print(call(tool, args))
