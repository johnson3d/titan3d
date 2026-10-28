using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EngineNS.Editor.AndroidBuild
{
    // 只负责 cache/config/android-toolchain.json：保留未知字段，AndroidTools.ps1 是唯一消费者。
    public sealed class TtAndroidToolchainDocument
    {
        private static readonly string[] RequiredEntries =
        {
            "ndk", "androidSdk", "cmake", "ninja", "dotnet", "javaHome", "abi"
        };

        private readonly JsonObject mRoot;

        private TtAndroidToolchainDocument(JsonObject root)
        {
            mRoot = root;
        }

        public static TtAndroidToolchainDocument CreateEmpty()
        {
            return new TtAndroidToolchainDocument(new JsonObject());
        }

        public static TtAndroidToolchainDocument Parse(string json)
        {
            JsonNode node;
            try
            {
                node = JsonNode.Parse(json);
            }
            catch (JsonException ex)
            {
                throw new FormatException("android-toolchain.json 不是合法 JSON：" + ex.Message, ex);
            }
            var obj = node as JsonObject;
            if (obj == null)
                throw new FormatException("android-toolchain.json 顶层必须是 JSON 对象。");
            return new TtAndroidToolchainDocument(obj);
        }

        public static TtAndroidToolchainDocument Load(string path)
        {
            if (!File.Exists(path))
                return CreateEmpty();
            return Parse(File.ReadAllText(path, Encoding.UTF8));
        }

        public string Serialize()
        {
            return mRoot.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }

        public void Save(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            // 无 BOM：AndroidTools.ps1 用 Get-Content -Encoding UTF8 读取，保持与手写文件一致，diff 更干净。
            File.WriteAllText(path, Serialize(), new UTF8Encoding(false));
        }

        public string Get(string name)
        {
            var node = mRoot[name];
            if (node == null)
                return string.Empty;
            return node.ToString();
        }

        public void Set(string name, string value)
        {
            mRoot[name] = value ?? string.Empty;
        }

        public int GetApiLevel()
        {
            var raw = Get("apiLevel");
            if (int.TryParse(raw, out var value))
                return value;
            return 0;
        }

        public void SetApiLevel(int value)
        {
            mRoot["apiLevel"] = value;
        }

        public IReadOnlyList<string> MissingRequiredEntries()
        {
            var missing = new List<string>();
            foreach (var name in RequiredEntries)
            {
                if (string.IsNullOrWhiteSpace(Get(name)))
                    missing.Add(name);
            }
            if (GetApiLevel() <= 0)
                missing.Add("apiLevel");
            return missing;
        }
    }
}
