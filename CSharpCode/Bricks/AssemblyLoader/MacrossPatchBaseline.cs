using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace EngineNS.Macross
{
    /// <summary>
    /// 发版基线清单：记录 GameProject 编译时生成的所有可补丁方法的 MethodId 与签名，
    /// 以及 GameProject.dll MVID 和镜像程序集哈希。补丁工具据此校验补丁的合法性。
    /// </summary>
    public sealed class TtMacrossPatchBaseline
    {
        public sealed class FMethod
        {
            public ulong MethodId;
            public string Signature;
        }

        public Guid GameMvid;
        public string MirrorAssembly;
        public string MirrorSha256;
        public readonly List<FMethod> Methods = new();

        public void Save(string path)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            writer.WriteStartObject();

            writer.WriteStartObject("BaselineId");
            writer.WriteString("GameMvid", GameMvid.ToString());
            writer.WriteString("MirrorAssembly", MirrorAssembly);
            writer.WriteString("MirrorSha256", MirrorSha256);
            writer.WriteEndObject();

            writer.WriteStartArray("Methods");
            foreach (var m in Methods)
            {
                writer.WriteStartObject();
                writer.WriteString("MethodId", $"0x{m.MethodId:X16}");
                writer.WriteString("Signature", m.Signature);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
            writer.Flush();

            File.WriteAllBytes(path, stream.ToArray());
        }

        public static TtMacrossPatchBaseline Load(string path)
        {
            var bytes = File.ReadAllBytes(path);
            using var doc = JsonDocument.Parse(bytes);
            var root = doc.RootElement;

            var baseline = root.GetProperty("BaselineId");
            var result = new TtMacrossPatchBaseline
            {
                GameMvid = Guid.Parse(baseline.GetProperty("GameMvid").GetString()),
                MirrorAssembly = baseline.GetProperty("MirrorAssembly").GetString(),
                MirrorSha256 = baseline.GetProperty("MirrorSha256").GetString(),
            };

            foreach (var m in root.GetProperty("Methods").EnumerateArray())
            {
                var idStr = m.GetProperty("MethodId").GetString();
                if (idStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    idStr = idStr.Substring(2);
                result.Methods.Add(new FMethod
                {
                    MethodId = ulong.Parse(idStr, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture),
                    Signature = m.GetProperty("Signature").GetString(),
                });
            }
            return result;
        }
    }
}
