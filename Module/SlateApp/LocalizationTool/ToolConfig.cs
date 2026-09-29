using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LocalizationTool
{
    // 工具配置。首次运行若文件缺失则写出引擎默认规则，之后可人工扩展，无需改工具代码。
    public class TtToolConfig
    {
        // 扫描根（相对引擎根目录）。默认仅引擎层 C#。
        public List<string> ScanRoots { get; set; } = new List<string>() { "CSharpCode" };
        // 排除片段：命中即跳过（对规范化后的正斜杠相对路径做 Contains 判断）。
        public List<string> ExcludeContains { get; set; } = new List<string>()
        {
            "/obj/", "/bin/", ".gen.cs", ".designer.cs", ".g.cs", "/Localization/",
        };
        // 已知 UI 文本函数名（方法名匹配，任意接收者）。命中则默认勾选并归类 UiText。
        public List<string> UiTextMethods { get; set; } = new List<string>()
        {
            "Text", "TextColored", "TextDisabled", "TextWrapped", "TextUnformatted",
            "BulletText", "LabelText", "Button", "SmallButton", "InvisibleButton",
            "MenuItem", "BeginMenu", "BeginMenuItem", "AddMenuItem", "InsertMenuItem",
            "AddMenuDraw", "AddMenuSeparator", "Selectable", "TreeNode", "TreeNodeEx",
            "CollapsingHeader", "Checkbox", "RadioButton", "SetTooltip",
        };
        // 精确的“接收者.方法”白名单，优先级高于方法名匹配（用于 UIProxy 等）。
        public List<string> UiTextQualified { get; set; } = new List<string>()
        {
            "DockProxy.BeginMainForm",
            "DockProxy.BeginPanel",
        };
        // UI 元数据 attribute 名（不含 Attribute 后缀）。
        public List<string> UiMetadataAttributes { get; set; } = new List<string>()
        {
            "DisplayName", "Description", "Category",
        };
        // 固定从引擎层加载基础配置，再按 JSON 属性用游戏层覆盖；数组作为一个属性整体覆盖。
        public static TtToolConfig LoadOrCreate(string engineContentRoot, string gameContentRoot)
        {
            var engineFile = GetConfigFile(engineContentRoot);
            var gameFile = GetConfigFile(gameContentRoot);
            var def = new TtToolConfig();

            if (File.Exists(engineFile) == false)
            {
                try
                {
                    var options = new JsonSerializerOptions() { WriteIndented = true };
                    Directory.CreateDirectory(Path.GetDirectoryName(engineFile));
                    File.WriteAllText(engineFile, JsonSerializer.Serialize(def, options));
                }
                catch (Exception)
                {
                }
            }

            try
            {
                var merged = JsonSerializer.SerializeToNode(def) as JsonObject ?? new JsonObject();
                MergeConfigFile(merged, engineFile);
                MergeConfigFile(merged, gameFile);
                return merged.Deserialize<TtToolConfig>() ?? def;
            }
            catch (Exception)
            {
                // 任一配置损坏时回退代码默认值，避免阻断工具启动。
                return def;
            }
        }

        static string GetConfigFile(string contentRoot)
        {
            return Path.Combine(contentRoot ?? string.Empty, "localization", "localizationtool.config.json").Replace('\\', '/');
        }

        static void MergeConfigFile(JsonObject target, string file)
        {
            if (File.Exists(file) == false)
                return;
            try
            {
                var overlay = JsonNode.Parse(File.ReadAllText(file)) as JsonObject;
                if (overlay != null)
                    MergeJsonObject(target, overlay);
            }
            catch (Exception)
            {
                // 损坏的单层配置不影响其它层。
            }
        }

        static void MergeJsonObject(JsonObject target, JsonObject overlay)
        {
            foreach (var property in overlay)
            {
                if (property.Value is JsonObject overlayObject && target[property.Key] is JsonObject targetObject)
                    MergeJsonObject(targetObject, overlayObject);
                else
                    target[property.Key] = property.Value?.DeepClone();
            }
        }

        public bool IsExcluded(string normalizedRelativePath)
        {
            foreach (var frag in ExcludeContains)
            {
                if (string.IsNullOrEmpty(frag))
                    continue;
                if (normalizedRelativePath.IndexOf(frag, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
    }
}
