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
            // 默认编码器会把 '+' 转义成 \u002B（c++_static 会变成 cu002bu002b_static），可读性差；
            // 文件是 UTF-8 无 BOM 落盘，用宽松编码器只影响转义策略，不改变字符内容。
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            return mRoot.ToJsonString(options);
        }

        public void Save(string path)
        {
            EnsureBuildScriptContract();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            // 无 BOM：AndroidTools.ps1 用 Get-Content -Encoding UTF8 读取，保持与手写文件一致，diff 更干净。
            File.WriteAllText(path, Serialize(), new UTF8Encoding(false));
        }

        // AndroidTools.ps1 对 abi/generator/stl 等字段是“缺失即报错”，不会静默取默认值。
        // 落盘前把构建契约字段补齐（只补缺，不覆盖用户已填的值），与 AndroidToolchain.example.json 保持同构。
        private void EnsureBuildScriptContract()
        {
            if (string.IsNullOrWhiteSpace(Get("schemaVersion")))
                mRoot["schemaVersion"] = 1;
            if (string.IsNullOrWhiteSpace(Get("abi")))
                Set("abi", "arm64-v8a");
            if (string.IsNullOrWhiteSpace(Get("generator")))
                Set("generator", "Ninja");
            if (string.IsNullOrWhiteSpace(Get("stl")))
                Set("stl", "c++_static");
            // adb 仅 Device 阶段需要；SDK 存在时默认指向 platform-tools\adb.exe。
            if (string.IsNullOrWhiteSpace(Get("adb")))
            {
                var sdk = Get("androidSdk");
                if (!string.IsNullOrWhiteSpace(sdk))
                    Set("adb", Path.Combine(sdk, "platform-tools", "adb.exe"));
            }
            if (string.IsNullOrWhiteSpace(GetRequirementString("minimumCMakeVersion")))
                SetRequirementString("minimumCMakeVersion", "3.24");
            if (GetRequirementInt("dotnetSdkMajor", 0) <= 0)
                SetRequirementInt("dotnetSdkMajor", 10);
            if (GetRequirementInt("ndkMajor", 0) <= 0)
                SetRequirementInt("ndkMajor", TtAndroidToolchainLocator.DefaultNdkMajor);
        }

        private string GetRequirementString(string name)
        {
            var node = mRoot["requirements"] as JsonObject;
            return node?[name]?.ToString() ?? string.Empty;
        }

        public void SetRequirementString(string name, string value)
        {
            if (mRoot["requirements"] is not JsonObject node)
            {
                node = new JsonObject();
                mRoot["requirements"] = node;
            }
            node[name] = value;
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

        // 读 requirements.<name>（ndkMajor 等）；缺省或缺字段时返回 fallback。脚本侧 AndroidTools.ps1
        // 用同一字段做门禁，这里只是编辑器预读同一份契约。
        public int GetRequirementInt(string name, int fallback)
        {
            var node = mRoot["requirements"] as JsonObject;
            if (node == null)
                return fallback;
            if (int.TryParse(node[name]?.ToString(), out var value))
                return value;
            return fallback;
        }

        public void SetRequirementInt(string name, int value)
        {
            if (mRoot["requirements"] is not JsonObject node)
            {
                node = new JsonObject();
                mRoot["requirements"] = node;
            }
            node[name] = value;
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
