"""
Titan Engine MCP Server (stdio mode)

A Python stdio MCP Server that bridges Aone Copilot to the Titan Engine's HTTP REST API.

Architecture:
    Aone Copilot <--stdio--> This Script <--HTTP--> TitanEngine (C# Plugin, port 8818)

Usage:
    python titan_mcp_server.py [--port 8818] [--host localhost]
"""

import sys
import json
import inspect
import argparse
from typing import Optional
import httpx
from mcp.server.fastmcp import FastMCP

ENGINE_HOST = "localhost"
ENGINE_PORT = 8818


def get_engine_url():
    return f"http://{ENGINE_HOST}:{ENGINE_PORT}"


def call_engine_api(method: str, path: str, body: dict = None):
    """Send an HTTP request to the Titan Engine REST API."""
    url = f"{get_engine_url()}{path}"
    try:
        with httpx.Client(timeout=10.0) as client:
            if method == "GET":
                response = client.get(url)
            else:
                response = client.post(url, json=body)
            response.raise_for_status()
            return response.json()
    except httpx.ConnectError:
        return {"error": f"Cannot connect to Titan Engine at {url}. Is the engine running?"}
    except httpx.TimeoutException:
        return {"error": f"Request to {url} timed out"}
    except Exception as exc:
        return {"error": str(exc)}


def invoke_engine_tool(tool_name: str, arguments: dict) -> str:
    """Invoke a tool on the engine via the REST API."""
    result = call_engine_api("POST", "/call", {
        "name": tool_name,
        "arguments": arguments or {}
    })
    if isinstance(result, dict):
        if result.get("success"):
            return result.get("result", "")
        error_msg = result.get("error", "Unknown error")
        raise RuntimeError(f"Engine tool error: {error_msg}")
    return json.dumps(result)


JSON_TYPE_TO_PYTHON = {
    "string": str,
    "number": float,
    "boolean": bool,
}


def build_tool_function(tool_name: str, tool_desc: str, properties: dict, required: list):
    """
    Build a callable function with a proper signature that fastmcp can introspect.
    This avoids the **kwargs problem where fastmcp cannot determine parameter names.
    """
    param_names = list(properties.keys())

    # Build the function signature dynamically
    params = []
    param_docs = []
    for pname in param_names:
        pinfo = properties[pname]
        ptype = pinfo.get("type", "string")
        pdesc = pinfo.get("description", "")
        python_type = JSON_TYPE_TO_PYTHON.get(ptype, str)
        if pname in required:
            params.append(inspect.Parameter(pname, inspect.Parameter.POSITIONAL_OR_KEYWORD, annotation=python_type))
        else:
            default = "" if python_type == str else (0.0 if python_type == float else False)
            params.append(inspect.Parameter(pname, inspect.Parameter.POSITIONAL_OR_KEYWORD, default=default, annotation=Optional[python_type]))
        if pdesc:
            param_docs.append(f"    {pname}: {pdesc}")

    def tool_fn(**kwargs) -> str:
        return invoke_engine_tool(tool_name, kwargs)

    tool_fn.__name__ = tool_name
    docstring = tool_desc
    if param_docs:
        docstring += "\n\nArgs:\n" + "\n".join(param_docs)
    tool_fn.__doc__ = docstring
    tool_fn.__signature__ = inspect.Signature(params, return_annotation=str)

    return tool_fn


def create_server() -> FastMCP:
    """Create the MCP server and dynamically register tools from the engine."""
    server = FastMCP("TitanEngine")

    # Always register a status check tool
    @server.tool(description="Check if the Titan Engine is running and accessible")
    def check_engine_status() -> str:
        """Check if the Titan Engine is running and accessible."""
        result = call_engine_api("GET", "/")
        return json.dumps(result, indent=2)

    # Try to fetch tools from engine
    engine_tools = []
    try:
        result = call_engine_api("GET", "/tools")
        if isinstance(result, list):
            engine_tools = result
    except Exception:
        pass

    if not engine_tools:
        print("Warning: Engine not reachable. Only 'check_engine_status' registered.", file=sys.stderr)
        print("Start the engine and restart this MCP server to get all tools.", file=sys.stderr)
        return server

    print(f"Fetched {len(engine_tools)} tools from Titan Engine", file=sys.stderr)

    for tool_def in engine_tools:
        name = tool_def.get("name", "")
        desc = tool_def.get("description", "")
        return_desc = tool_def.get("returnDescription", "")
        if return_desc:
            desc = f"{desc}\n\nReturns: {return_desc}"
        params = tool_def.get("parameters", {})
        properties = params.get("properties", {})
        required = params.get("required", [])

        fn = build_tool_function(name, desc, properties, required)
        server.tool(name=name, description=desc)(fn)

    print(f"Registered {len(engine_tools) + 1} tools total", file=sys.stderr)
    return server


def main():
    global ENGINE_HOST, ENGINE_PORT

    parser = argparse.ArgumentParser(description="Titan Engine MCP Server")
    parser.add_argument("--host", default="localhost", help="Engine HTTP host (default: localhost)")
    parser.add_argument("--port", type=int, default=8818, help="Engine HTTP port (default: 8818)")
    args = parser.parse_args()

    ENGINE_HOST = args.host
    ENGINE_PORT = args.port

    print(f"Titan Engine MCP Server starting (engine at {get_engine_url()})", file=sys.stderr)

    server = create_server()
    server.run(transport="stdio")


if __name__ == "__main__":
    main()
