using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EngineNS.Plugins.MCPServer
{
    /// <summary>
    /// 当前编辑器 World 中场景节点的通用属性诊断与运行时修复通路。
    /// 写操作只修改内存，不保存场景；节点匹配不唯一时拒绝写入。
    /// </summary>
    public partial class TtMCPServerPlugin
    {
        private sealed class FNodePropertyMatch
        {
            public string World;
            public GamePlay.Scene.TtNode Node;
        }

        private sealed class FPropertyStep
        {
            public object Owner;
            public MemberInfo Member;
            public object Value;
            public Type ValueType;
        }

        private sealed class FPropertyWrite
        {
            public string Path;
            public object Value;
            public Type ValueType;
        }

        private sealed class FAppliedPropertyWrite
        {
            public string Path;
            public object OldValue;
        }

        private static readonly JsonSerializerOptions NodePropertyJsonOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            Converters = { new JsonStringEnumConverter() },
        };

        [Bricks.AIGC.TtMCPTool("get_node_properties",
            "Finds one scene node by NodeName in the worlds exposed by currently open scene and sequence " +
            "editors, then reads public properties or fields. Matching is case insensitive. propertyPaths is " +
            "a comma-separated list and supports nested paths such as NodeData.ShadingStruct.PlanetRadius. " +
            "When propertyPaths is empty, the tool lists the node's public top-level members instead of " +
            "invoking every getter. Use worldFilter when the same NodeName exists in multiple open worlds.",
            returnDescription: "{node:{name,type,nodeId,world}, properties:[{path,type,value,canWrite}], " +
            "members:[{name,type,canRead,canWrite}], candidates:[{name,type,nodeId,world}], " +
            "error:string - present on failure}")]
        public static string GetNodeProperties(
            [Bricks.AIGC.TtMCPParameter("NodeName to find. Exact matching is used by default")] string nodeName,
            [Bricks.AIGC.TtMCPParameter("Comma-separated property paths. Empty lists available top-level members")] string propertyPaths = "",
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of the world source, for example 'SceneEditor:'")] string worldFilter = "",
            [Bricks.AIGC.TtMCPParameter("True for exact NodeName matching; false for substring matching")] bool exactMatch = true)
        {
            LogToolCall("get_node_properties",
                $"nodeName={nodeName}, propertyPaths={propertyPaths}, worldFilter={worldFilter}, exactMatch={exactMatch}");

            if (string.IsNullOrWhiteSpace(nodeName))
                return FailJson("nodeName is empty");

            string result = null;
            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var matches = FindNodePropertyMatches(nodeName.Trim(), worldFilter, exactMatch);
                if (matches.Count != 1)
                {
                    result = BuildNodeMatchError(nodeName, matches);
                    return;
                }

                var match = matches[0];
                var paths = SplitPropertyPaths(propertyPaths);
                if (paths.Count == 0)
                {
                    result = JsonSerializer.Serialize(new
                    {
                        node = DescribeNodePropertyMatch(match),
                        members = DescribePublicMembers(match.Node),
                    }, NodePropertyJsonOptions);
                    return;
                }

                var properties = new List<object>();
                for (int i = 0; i < paths.Count; ++i)
                {
                    if (TryResolvePropertyPath(match.Node, paths[i], out var steps, out var error) == false)
                    {
                        properties.Add(new { path = paths[i], error });
                        continue;
                    }

                    var leaf = steps[steps.Count - 1];
                    properties.Add(new
                    {
                        path = paths[i],
                        type = FriendlyTypeName(leaf.ValueType),
                        value = MakeJsonSafeValue(leaf.Value),
                        canWrite = CanWritePath(steps, out _),
                    });
                }

                result = JsonSerializer.Serialize(new
                {
                    node = DescribeNodePropertyMatch(match),
                    properties,
                }, NodePropertyJsonOptions);
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            return result;
        }

        [Bricks.AIGC.TtMCPTool("set_node_properties",
            "Finds exactly one scene node by NodeName in the worlds exposed by currently open scene and " +
            "sequence editors, then atomically changes public properties or fields in memory. propertiesJson " +
            "must be a JSON object whose keys are property paths, for example " +
            "{\"CloudDensity\":0.8,\"NodeData.ShadingStruct.PlanetRadius\":6360000}. Nested value-type " +
            "members are written back through their owning properties. Matching is case insensitive. The " +
            "tool does not save the scene; a failed batch is rolled back.",
            returnDescription: "{changed:boolean,node:{name,type,nodeId,world},changes:[{path,type,oldValue," +
            "newValue}],error:string - present on failure,candidates:[{name,type,nodeId,world}]}")]
        public static string SetNodeProperties(
            [Bricks.AIGC.TtMCPParameter("Exact NodeName to find unless exactMatch is false")] string nodeName,
            [Bricks.AIGC.TtMCPParameter("JSON object mapping property paths to new values")] string propertiesJson,
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of the world source, for example 'SceneEditor:'")] string worldFilter = "",
            [Bricks.AIGC.TtMCPParameter("True for exact NodeName matching; false for substring matching")] bool exactMatch = true)
        {
            LogToolCall("set_node_properties",
                $"nodeName={nodeName}, propertiesJson={propertiesJson}, worldFilter={worldFilter}, exactMatch={exactMatch}");

            if (string.IsNullOrWhiteSpace(nodeName))
                return FailJson("nodeName is empty");
            if (string.IsNullOrWhiteSpace(propertiesJson))
                return FailJson("propertiesJson is empty");

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(propertiesJson);
            }
            catch (Exception ex)
            {
                return FailJson($"propertiesJson is not valid JSON: {ex.Message}");
            }

            using (document)
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    return FailJson("propertiesJson must be a JSON object");
                if (GetJsonPropertyCount(document.RootElement) == 0)
                    return FailJson("propertiesJson contains no properties");

                string result = null;
                var ok = TtMainThreadDispatcher.Invoke(() =>
                {
                    var matches = FindNodePropertyMatches(nodeName.Trim(), worldFilter, exactMatch);
                    if (matches.Count != 1)
                    {
                        result = BuildNodeMatchError(nodeName, matches);
                        return;
                    }

                    var match = matches[0];
                    var writes = new List<FPropertyWrite>();
                    foreach (var item in document.RootElement.EnumerateObject())
                    {
                        var path = item.Name.Trim();
                        if (TryResolvePropertyPath(match.Node, path, out var steps, out var error) == false)
                        {
                            result = FailJson($"property '{path}': {error}");
                            return;
                        }
                        if (CanWritePath(steps, out error) == false)
                        {
                            result = FailJson($"property '{path}': {error}");
                            return;
                        }

                        var valueType = steps[steps.Count - 1].ValueType;
                        if (TryConvertJsonValue(item.Value, valueType, out var converted, out error) == false)
                        {
                            result = FailJson($"property '{path}': {error}");
                            return;
                        }
                        writes.Add(new FPropertyWrite { Path = path, Value = converted, ValueType = valueType });
                    }

                    var applied = new List<FAppliedPropertyWrite>();
                    var changes = new List<object>();
                    for (int i = 0; i < writes.Count; ++i)
                    {
                        var write = writes[i];
                        if (TryResolvePropertyPath(match.Node, write.Path, out var steps, out var error) == false)
                        {
                            RollbackPropertyWrites(match.Node, applied);
                            result = FailJson($"property '{write.Path}' changed before it could be written: {error}");
                            return;
                        }

                        var oldValue = steps[steps.Count - 1].Value;
                        try
                        {
                            SetResolvedPropertyPath(steps, write.Value);
                        }
                        catch (Exception ex)
                        {
                            RollbackPropertyWrites(match.Node, applied);
                            result = FailJson($"property '{write.Path}' could not be written: {UnwrapException(ex).Message}");
                            return;
                        }

                        applied.Add(new FAppliedPropertyWrite { Path = write.Path, OldValue = oldValue });
                        changes.Add(new
                        {
                            path = write.Path,
                            type = FriendlyTypeName(write.ValueType),
                            oldValue = MakeJsonSafeValue(oldValue),
                            newValue = MakeJsonSafeValue(write.Value),
                        });
                    }

                    result = JsonSerializer.Serialize(new
                    {
                        changed = true,
                        node = DescribeNodePropertyMatch(match),
                        changes,
                        persisted = false,
                    }, NodePropertyJsonOptions);
                });

                if (ok == false)
                    return FailJson("timed out waiting for the engine main thread");
                return result;
            }
        }

        [Bricks.AIGC.TtMCPTool("add_scene_node",
            "Adds one TtNode-derived scene node to exactly one currently open scene editor. typeName must be the " +
            "full runtime type name, for example EngineNS.GamePlay.Scene.TtHeightFogNode. nodeName must be unique " +
            "inside that scene. When parentName is empty the scene root is used. The operation is added to the " +
            "scene editor's undo/redo history but is not saved automatically.",
            returnDescription: "{added:boolean,node:{name,type,nodeId,world},parent:string,persisted:boolean," +
            "error:string - present on failure,candidates:string[]}")]
        public static string AddSceneNode(
            [Bricks.AIGC.TtMCPParameter("Full runtime type name of a non-abstract TtNode subclass")] string typeName,
            [Bricks.AIGC.TtMCPParameter("Unique node name. Empty uses the type's DefaultNamePrefix")] string nodeName = "",
            [Bricks.AIGC.TtMCPParameter("Exact parent NodeName. Empty uses the scene root")] string parentName = "",
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of the open scene asset name")] string assetFilter = "")
        {
            LogToolCall("add_scene_node",
                $"typeName={typeName}, nodeName={nodeName}, parentName={parentName}, assetFilter={assetFilter}");

            if (string.IsNullOrWhiteSpace(typeName))
                return FailJson("typeName is empty");

            string result = null;
            try
            {
                var ok = TtMainThreadDispatcher.Invoke(() =>
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
                    if (mainEditor == null)
                    {
                        result = FailJson("main editor is not available");
                        return;
                    }

                    var filter = (assetFilter ?? "").Trim();
                    var editors = new List<Editor.Forms.TtSceneEditor>();
                    var candidates = new List<string>();
                    foreach (var opened in mainEditor.AssetEditorManager.OpenedEditors)
                    {
                        var sceneEditor = opened as Editor.Forms.TtSceneEditor;
                        if (sceneEditor == null || sceneEditor.Scene == null || sceneEditor.AssetName == null)
                            continue;
                        var asset = sceneEditor.AssetName.ToString();
                        if (filter.Length > 0 && asset.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;
                        editors.Add(sceneEditor);
                        candidates.Add(asset);
                    }

                    if (editors.Count != 1)
                    {
                        var error = editors.Count == 0
                            ? $"no open scene matches '{assetFilter}'"
                            : $"assetFilter matched {editors.Count} open scenes; make it more specific";
                        result = JsonSerializer.Serialize(new { error, candidates });
                        return;
                    }

                    var typeDesc = Rtti.TtTypeDesc.TypeOfFullName(typeName.Trim());
                    var nodeType = typeDesc?.SystemType;
                    if (nodeType == null || nodeType.IsSubclassOf(typeof(GamePlay.Scene.TtNode)) == false || nodeType.IsAbstract)
                    {
                        result = FailJson($"typeName '{typeName}' is not a non-abstract TtNode subclass");
                        return;
                    }

                    var nodeAttribute = GamePlay.Scene.TtNode.GetNodeAttribute(nodeType);
                    if (nodeAttribute?.NodeDataType == null)
                    {
                        result = FailJson($"typeName '{typeName}' has no valid TtNodeAttribute NodeDataType");
                        return;
                    }

                    var editor = editors[0];
                    GamePlay.Scene.TtNode parent = editor.Scene;
                    var parentText = (parentName ?? "").Trim();
                    if (parentText.Length > 0)
                    {
                        var parentMatches = new List<FNodePropertyMatch>();
                        CollectNodePropertyMatches(editor.Scene, editor.AssetName.ToString(), parentText, true, parentMatches);
                        if (parentMatches.Count != 1)
                        {
                            result = BuildNodeMatchError(parentText, parentMatches);
                            return;
                        }
                        parent = parentMatches[0].Node;
                    }

                    var finalName = (nodeName ?? "").Trim();
                    if (finalName.Length == 0)
                        finalName = string.IsNullOrWhiteSpace(nodeAttribute.DefaultNamePrefix)
                            ? nodeType.Name
                            : nodeAttribute.DefaultNamePrefix;

                    var duplicateMatches = new List<FNodePropertyMatch>();
                    CollectNodePropertyMatches(editor.Scene, editor.AssetName.ToString(), finalName, true, duplicateMatches);
                    if (duplicateMatches.Count > 0)
                    {
                        result = FailJson($"nodeName '{finalName}' already exists in scene '{editor.AssetName}'");
                        return;
                    }

                    var spawnTask = GamePlay.Scene.TtNode.SpawnNode(parent, nodeType, null, null,
                        GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.TtPlacement));
                    var newNode = spawnTask.GetResultUntilCompleted();
                    if (newNode == null)
                    {
                        result = FailJson($"failed to spawn node type '{typeName}'");
                        return;
                    }

                    newNode.NodeData.Name = finalName;
                    Editor.Forms.TtSceneEditor.PushNodeCreateCommand(editor.EditorHistory, newNode, "Add Node");
                    result = JsonSerializer.Serialize(new
                    {
                        added = true,
                        node = new
                        {
                            name = newNode.NodeName,
                            type = newNode.GetType().FullName,
                            nodeId = newNode.NodeId.ToString(),
                            world = $"SceneEditor:{editor.AssetName}",
                        },
                        parent = parent.NodeName,
                        persisted = false,
                    }, NodePropertyJsonOptions);
                });

                if (ok == false)
                    return FailJson("timed out waiting for the engine main thread");
                return result;
            }
            catch (Exception ex)
            {
                return FailJson($"adding scene node failed: {UnwrapException(ex).Message}");
            }
        }

        [Bricks.AIGC.TtMCPTool("save_open_scene",
            "Saves one currently open scene editor to its asset file, equivalent to pressing the scene editor's Save button. " +
            "Use assetFilter when more than one scene editor is open.",
            returnDescription: "{saved:boolean,asset:string,error:string - present on failure,candidates:string[]}")]
        public static string SaveOpenScene(
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of the open scene asset name. Empty is allowed only when exactly one scene is open")] string assetFilter = "")
        {
            LogToolCall("save_open_scene", $"assetFilter={assetFilter}");

            string result = null;
            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
                if (mainEditor == null)
                {
                    result = FailJson("main editor is not available");
                    return;
                }

                var filter = (assetFilter ?? "").Trim();
                var editors = new List<Editor.Forms.TtSceneEditor>();
                var candidates = new List<string>();
                foreach (var opened in mainEditor.AssetEditorManager.OpenedEditors)
                {
                    var sceneEditor = opened as Editor.Forms.TtSceneEditor;
                    if (sceneEditor == null || sceneEditor.Scene == null || sceneEditor.AssetName == null)
                        continue;
                    var asset = sceneEditor.AssetName.ToString();
                    if (filter.Length > 0 && asset.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    editors.Add(sceneEditor);
                    candidates.Add(asset);
                }

                if (editors.Count != 1)
                {
                    var error = editors.Count == 0
                        ? $"no open scene matches '{assetFilter}'"
                        : $"assetFilter matched {editors.Count} open scenes; make it more specific";
                    result = JsonSerializer.Serialize(new { error, candidates });
                    return;
                }

                var editor = editors[0];
                editor.GetAsset().SaveAssetTo(editor.AssetName);
                result = JsonSerializer.Serialize(new
                {
                    saved = true,
                    asset = editor.AssetName.ToString(),
                });
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            return result;
        }

        [Bricks.AIGC.TtMCPTool("find_scene_nodes",
            "Lists scene nodes across every world exposed by the currently open scene and sequence editors, " +
            "filtered by node type and/or name. typeFilter is a case-insensitive substring matched against each " +
            "node's runtime type full name, for example 'VolumetricCloud' matches " +
            "EngineNS.Bricks.FX.Weather.TtVolumetricCloudSceneNode. nameFilter is a case-insensitive substring of " +
            "NodeName. worldFilter is a case-insensitive substring of the world source such as 'SceneEditor:'. All " +
            "filters are optional and an empty filter matches everything. Use this to locate a node by type when " +
            "its exact NodeName is unknown, then pass the returned name and world to get_node_properties.",
            returnDescription: "{count:int,nodes:[{name,type,nodeId,world}],error:string - present on failure}")]
        public static string FindSceneNodes(
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of the runtime type full name. Empty matches any type")] string typeFilter = "",
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of NodeName. Empty matches any name")] string nameFilter = "",
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of the world source such as 'SceneEditor:'")] string worldFilter = "")
        {
            LogToolCall("find_scene_nodes",
                $"typeFilter={typeFilter}, nameFilter={nameFilter}, worldFilter={worldFilter}");

            string result = null;
            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var roots = new List<KeyValuePair<string, GamePlay.Scene.TtNode>>();
                GatherWorldRoots(roots);
                var worldText = (worldFilter ?? "").Trim();
                var typeText = (typeFilter ?? "").Trim();
                var nameText = (nameFilter ?? "").Trim();

                var nodes = new List<object>();
                for (int i = 0; i < roots.Count; ++i)
                {
                    var pair = roots[i];
                    if (worldText.Length > 0 &&
                        pair.Key.IndexOf(worldText, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    CollectNodesByTypeAndName(pair.Value, pair.Key, typeText, nameText, nodes);
                }

                result = JsonSerializer.Serialize(new
                {
                    count = nodes.Count,
                    nodes,
                }, NodePropertyJsonOptions);
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            return result;
        }

        [Bricks.AIGC.TtMCPTool("regen_volumetric_cloud_noise",
            "Regenerates the 3D noise volume and weather map textures of exactly one volumetric cloud scene node " +
            "after its NoiseGen settings were changed with set_node_properties. Changing NoiseGen fields only " +
            "updates CPU-side settings; the cloud keeps rendering with the previously baked textures until they are " +
            "regenerated, so parameters such as Frequency or Seed appear to have no effect until this tool is " +
            "called. The node is located by NodeName exactly like get_node_properties and must be a " +
            "TtVolumetricCloudSceneNode. This runs on the engine main thread and can take a few seconds.",
            returnDescription: "{regenerated:boolean,node:{name,type,nodeId,world},error:string - present on " +
            "failure,candidates:[{name,type,nodeId,world}]}")]
        public static string RegenVolumetricCloudNoise(
            [Bricks.AIGC.TtMCPParameter("Exact NodeName to find unless exactMatch is false")] string nodeName,
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of the world source, for example 'SceneEditor:'")] string worldFilter = "",
            [Bricks.AIGC.TtMCPParameter("True for exact NodeName matching; false for substring matching")] bool exactMatch = true)
        {
            LogToolCall("regen_volumetric_cloud_noise",
                $"nodeName={nodeName}, worldFilter={worldFilter}, exactMatch={exactMatch}");

            if (string.IsNullOrWhiteSpace(nodeName))
                return FailJson("nodeName is empty");

            string result = null;
            try
            {
                // 噪声重生成要新建 3D 纹理 + 天气图, 比普通属性读写慢, 给足超时(60s)。
                var ok = TtMainThreadDispatcher.Invoke(() =>
                {
                    var matches = FindNodePropertyMatches(nodeName.Trim(), worldFilter, exactMatch);
                    if (matches.Count != 1)
                    {
                        result = BuildNodeMatchError(nodeName, matches);
                        return;
                    }

                    var match = matches[0];
                    var cloud = match.Node as Bricks.FX.Weather.TtVolumetricCloudSceneNode;
                    if (cloud == null)
                    {
                        result = FailJson($"node '{match.Node.NodeName}' is {match.Node.GetType().FullName}, not a TtVolumetricCloudSceneNode");
                        return;
                    }
                    if (cloud.NoiseGen == null)
                    {
                        result = FailJson($"node '{match.Node.NodeName}' has no NoiseGen");
                        return;
                    }

                    // set_node_properties 只改了 NoiseGen 的 CPU 端字段, 烘焙纹理还是旧的;
                    // 这里同步重建, 让 Frequency/Seed 等改动真正体现到渲染上。
                    // ReGenRenderResources 返回非泛型 TtTask(void), 只有 WaitCompleted, 没有 GetResultUntilCompleted。
                    cloud.NoiseGen.ReGenRenderResources().WaitCompleted();

                    result = JsonSerializer.Serialize(new
                    {
                        regenerated = true,
                        node = DescribeNodePropertyMatch(match),
                    }, NodePropertyJsonOptions);
                }, 60000);

                if (ok == false)
                    return FailJson("timed out waiting for the engine main thread");
                return result;
            }
            catch (Exception ex)
            {
                return FailJson($"regenerating volumetric cloud noise failed: {UnwrapException(ex).Message}");
            }
        }

        private static void CollectNodesByTypeAndName(GamePlay.Scene.TtNode node, string world,
            string typeFilter, string nameFilter, List<object> result)
        {
            if (node == null)
                return;

            var typeName = node.GetType().FullName ?? "";
            var nodeName = node.NodeName ?? "";
            var typeMatched = typeFilter.Length == 0 ||
                typeName.IndexOf(typeFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            var nameMatched = nameFilter.Length == 0 ||
                nodeName.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            if (typeMatched && nameMatched)
            {
                result.Add(new
                {
                    name = node.NodeName,
                    type = typeName,
                    nodeId = node.NodeId.ToString(),
                    world,
                });
            }

            for (int i = 0; i < node.Children.Count; ++i)
                CollectNodesByTypeAndName(node.Children[i], world, typeFilter, nameFilter, result);
        }

        private static List<FNodePropertyMatch> FindNodePropertyMatches(string nodeName, string worldFilter, bool exactMatch)
        {
            var result = new List<FNodePropertyMatch>();
            var roots = new List<KeyValuePair<string, GamePlay.Scene.TtNode>>();
            GatherWorldRoots(roots);
            var worldText = (worldFilter ?? "").Trim();
            for (int i = 0; i < roots.Count; ++i)
            {
                var pair = roots[i];
                if (worldText.Length > 0 &&
                    pair.Key.IndexOf(worldText, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                CollectNodePropertyMatches(pair.Value, pair.Key, nodeName, exactMatch, result);
            }
            return result;
        }

        private static void CollectNodePropertyMatches(GamePlay.Scene.TtNode node, string world, string nodeName,
            bool exactMatch, List<FNodePropertyMatch> result)
        {
            if (node == null)
                return;

            var currentName = node.NodeName ?? "";
            var matched = exactMatch
                ? string.Equals(currentName, nodeName, StringComparison.OrdinalIgnoreCase)
                : currentName.IndexOf(nodeName, StringComparison.OrdinalIgnoreCase) >= 0;
            if (matched)
                result.Add(new FNodePropertyMatch { World = world, Node = node });

            for (int i = 0; i < node.Children.Count; ++i)
                CollectNodePropertyMatches(node.Children[i], world, nodeName, exactMatch, result);
        }

        private static object DescribeNodePropertyMatch(FNodePropertyMatch match)
        {
            return new
            {
                name = match.Node.NodeName,
                type = match.Node.GetType().FullName,
                nodeId = match.Node.NodeId.ToString(),
                world = match.World,
            };
        }

        private static string BuildNodeMatchError(string nodeName, List<FNodePropertyMatch> matches)
        {
            var candidates = new List<object>();
            for (int i = 0; i < matches.Count; ++i)
                candidates.Add(DescribeNodePropertyMatch(matches[i]));
            var error = matches.Count == 0
                ? $"node '{nodeName}' was not found in any matching open world"
                : $"node '{nodeName}' matched {matches.Count} nodes; use an exact name or worldFilter";
            return JsonSerializer.Serialize(new { error, candidates }, NodePropertyJsonOptions);
        }

        private static List<string> SplitPropertyPaths(string propertyPaths)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(propertyPaths))
                return result;
            var paths = propertyPaths.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < paths.Length; ++i)
            {
                if (paths[i].Length > 0)
                    result.Add(paths[i]);
            }
            return result;
        }

        private static List<object> DescribePublicMembers(object target)
        {
            var result = new List<object>();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            var properties = target.GetType().GetProperties(flags);
            for (int i = 0; i < properties.Length; ++i)
            {
                var property = properties[i];
                if (property.GetIndexParameters().Length != 0)
                    continue;
                result.Add(new
                {
                    name = property.Name,
                    type = FriendlyTypeName(property.PropertyType),
                    canRead = property.GetGetMethod(false) != null,
                    canWrite = property.GetSetMethod(false) != null,
                });
            }
            var fields = target.GetType().GetFields(flags);
            for (int i = 0; i < fields.Length; ++i)
            {
                var field = fields[i];
                result.Add(new
                {
                    name = field.Name,
                    type = FriendlyTypeName(field.FieldType),
                    canRead = true,
                    canWrite = field.IsInitOnly == false && field.IsLiteral == false,
                });
            }
            return result;
        }

        private static bool TryResolvePropertyPath(object root, string path, out List<FPropertyStep> steps,
            out string error)
        {
            steps = new List<FPropertyStep>();
            error = null;
            if (root == null)
            {
                error = "root object is null";
                return false;
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                error = "property path is empty";
                return false;
            }

            object owner = root;
            var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < segments.Length; ++i)
            {
                if (owner == null)
                {
                    error = $"'{string.Join('.', segments, 0, i)}' is null";
                    return false;
                }

                var member = FindPublicMember(owner.GetType(), segments[i]);
                if (member == null)
                {
                    error = $"member '{segments[i]}' was not found on {owner.GetType().FullName}";
                    return false;
                }

                try
                {
                    var value = GetMemberValue(owner, member);
                    var valueType = GetMemberValueType(member);
                    steps.Add(new FPropertyStep
                    {
                        Owner = owner,
                        Member = member,
                        Value = value,
                        ValueType = valueType,
                    });
                    owner = value;
                }
                catch (Exception ex)
                {
                    error = $"reading '{segments[i]}' failed: {UnwrapException(ex).Message}";
                    return false;
                }
            }
            return true;
        }

        private static MemberInfo FindPublicMember(Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;
            var property = type.GetProperty(name, flags);
            if (property != null && property.GetIndexParameters().Length == 0 && property.GetGetMethod(false) != null)
                return property;
            return type.GetField(name, flags);
        }

        private static object GetMemberValue(object owner, MemberInfo member)
        {
            if (member is PropertyInfo property)
                return property.GetValue(owner);
            return ((FieldInfo)member).GetValue(owner);
        }

        private static Type GetMemberValueType(MemberInfo member)
        {
            var type = member is PropertyInfo property ? property.PropertyType : ((FieldInfo)member).FieldType;
            return type.IsByRef ? type.GetElementType() : type;
        }

        private static bool CanWritePath(List<FPropertyStep> steps, out string error)
        {
            error = null;
            var leaf = steps[steps.Count - 1];
            if (CanWriteMember(leaf.Member) == false)
            {
                error = $"leaf member '{leaf.Member.Name}' is read-only";
                return false;
            }

            for (int i = steps.Count - 2; i >= 0; --i)
            {
                var step = steps[i];
                var memberType = step.Member is PropertyInfo property
                    ? property.PropertyType
                    : ((FieldInfo)step.Member).FieldType;
                if (memberType.IsByRef)
                {
                    error = $"intermediate member '{step.Member.Name}' returns by reference; use a writable owner path such as NodeData instead";
                    return false;
                }
                if (memberType.IsValueType && CanWriteMember(step.Member) == false)
                {
                    error = $"value-type member '{step.Member.Name}' is read-only and cannot be written back";
                    return false;
                }
            }
            return true;
        }

        private static bool CanWriteMember(MemberInfo member)
        {
            if (member is PropertyInfo property)
                return property.GetSetMethod(false) != null;
            var field = (FieldInfo)member;
            return field.IsInitOnly == false && field.IsLiteral == false;
        }

        private static void SetResolvedPropertyPath(List<FPropertyStep> steps, object value)
        {
            var leaf = steps[steps.Count - 1];
            SetMemberValue(leaf.Owner, leaf.Member, value);
            object changedOwner = leaf.Owner;

            for (int i = steps.Count - 2; i >= 0; --i)
            {
                var step = steps[i];
                var memberType = step.Member is PropertyInfo property
                    ? property.PropertyType
                    : ((FieldInfo)step.Member).FieldType;
                if (memberType.IsByRef)
                    throw new InvalidOperationException($"'{step.Member.Name}' returns by reference and cannot be written back safely");
                if (memberType.IsValueType == false)
                    break;
                SetMemberValue(step.Owner, step.Member, changedOwner);
                changedOwner = step.Owner;
            }
        }

        private static void SetMemberValue(object owner, MemberInfo member, object value)
        {
            if (member is PropertyInfo property)
                property.SetValue(owner, value);
            else
                ((FieldInfo)member).SetValue(owner, value);
        }

        private static bool TryConvertJsonValue(JsonElement element, Type targetType, out object value,
            out string error)
        {
            value = null;
            error = null;
            var nullableType = Nullable.GetUnderlyingType(targetType);
            var actualType = nullableType ?? targetType;
            if (element.ValueKind == JsonValueKind.Null)
            {
                if (targetType.IsValueType && nullableType == null)
                {
                    error = $"null cannot be assigned to {FriendlyTypeName(targetType)}";
                    return false;
                }
                return true;
            }

            try
            {
                if (actualType == typeof(string))
                {
                    value = element.ValueKind == JsonValueKind.String ? element.GetString() : element.GetRawText();
                    return true;
                }
                if (actualType.IsEnum && element.ValueKind == JsonValueKind.String)
                {
                    value = Enum.Parse(actualType, element.GetString(), true);
                    return true;
                }
                if (element.ValueKind == JsonValueKind.String)
                {
                    var text = element.GetString();
                    var converter = TypeDescriptor.GetConverter(actualType);
                    if (converter != null && converter.CanConvertFrom(typeof(string)))
                    {
                        value = converter.ConvertFrom(null, CultureInfo.InvariantCulture, text);
                        return true;
                    }
                }

                value = JsonSerializer.Deserialize(element.GetRawText(), actualType, NodePropertyJsonOptions);
                return true;
            }
            catch (Exception ex)
            {
                error = $"value {element.GetRawText()} cannot be converted to {FriendlyTypeName(targetType)}: {UnwrapException(ex).Message}";
                return false;
            }
        }

        private static void RollbackPropertyWrites(GamePlay.Scene.TtNode node, List<FAppliedPropertyWrite> applied)
        {
            for (int i = applied.Count - 1; i >= 0; --i)
            {
                try
                {
                    if (TryResolvePropertyPath(node, applied[i].Path, out var steps, out _))
                        SetResolvedPropertyPath(steps, applied[i].OldValue);
                }
                catch
                {
                    Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Error,
                        $"Failed to roll back node property '{applied[i].Path}'");
                }
            }
        }

        private static int GetJsonPropertyCount(JsonElement element)
        {
            int count = 0;
            foreach (var _ in element.EnumerateObject())
                count++;
            return count;
        }

        /// <summary>
        /// 把任意反射值收敛成有界 JSON 数据。不能把引擎对象直接交给 JsonSerializer：
        /// 某些数学结构和资源类型暴露了递归计算属性，会让一次诊断请求卡死主线程。
        /// 值类型只展开 public field，引用类型仅输出 ToString。
        /// </summary>
        private static object MakeJsonSafeValue(object value, int depth = 0)
        {
            if (value == null)
                return null;

            var type = value.GetType();
            if (type.IsEnum)
                return value.ToString();
            if (type.IsPrimitive || value is decimal || value is string || value is DateTime || value is Guid)
                return value;
            if (depth >= 4)
                return value.ToString();

            if (type.IsValueType)
            {
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                if (fields.Length == 0)
                    return value.ToString();

                var result = new Dictionary<string, object>();
                for (int i = 0; i < fields.Length; ++i)
                    result[fields[i].Name] = MakeJsonSafeValue(fields[i].GetValue(value), depth + 1);
                return result;
            }

            return value.ToString();
        }

        private static string FriendlyTypeName(Type type)
        {
            if (type == null)
                return "unknown";
            if (type.IsByRef)
                type = type.GetElementType();
            return type.FullName ?? type.Name;
        }

        private static Exception UnwrapException(Exception exception)
        {
            while (exception is TargetInvocationException && exception.InnerException != null)
                exception = exception.InnerException;
            return exception;
        }
    }
}
