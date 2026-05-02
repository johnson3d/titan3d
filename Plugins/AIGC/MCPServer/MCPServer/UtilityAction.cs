using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EngineNS.Plugins.MCPServer
{
    public partial class TtMCPServerPlugin
    {
        [Bricks.AIGC.TtMCPTool("echo", "Echoes back the input message, useful for testing agent connectivity",
            returnDescription: "{echo: string - The echoed message, " +
            "timestamp: string - Server timestamp when processed}")]
        public static string Echo(
            [Bricks.AIGC.TtMCPParameter("The message to echo back")] string message)
        {
            return JsonSerializer.Serialize(new
            {
                echo = message,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        [Bricks.AIGC.TtMCPTool("calculate",
            "Performs a basic arithmetic calculation. Supported operators: add, sub, mul, div",
            returnDescription: "{operation: string - The operator used, " +
            "a: number - First operand, " +
            "b: number - Second operand, " +
            "result: number - Calculation result}")]
        public static string Calculate(
            [Bricks.AIGC.TtMCPParameter("First operand")] double valueA,
            [Bricks.AIGC.TtMCPParameter("Second operand")] double valueB,
            [Bricks.AIGC.TtMCPParameter("Operator: add, sub, mul, div")] string operation = "add")
        {
            double calcResult = operation.ToLower() switch
            {
                "add" => valueA + valueB,
                "sub" => valueA - valueB,
                "mul" => valueA * valueB,
                "div" => valueB != 0 ? valueA / valueB : double.NaN,
                _ => double.NaN
            };
            return JsonSerializer.Serialize(new
            {
                operation,
                a = valueA,
                b = valueB,
                result = calcResult
            });
        }

        [Bricks.AIGC.TtMCPTool("get_recent_logs",
            "Returns recent engine log entries. Use tagFilter to filter by tag like 'Info','Warning','Error','MCP'",
            returnDescription: "{logs: string[] - Log entries in format [timestamp][tag] message, " +
            "count: number - Number of entries returned}")]
        public static string GetRecentLogs(
            [Bricks.AIGC.TtMCPParameter("Number of log entries to return")] double count = 50,
            [Bricks.AIGC.TtMCPParameter("Filter logs by keyword, e.g. 'Info','Warning','Error'")] string tagFilter = "")
        {
            var logs = TtLogCollector.GetRecentLogs((int)count, tagFilter);
            return JsonSerializer.Serialize(new
            {
                logs,
                count = logs.Count
            });
        }
    }
}
