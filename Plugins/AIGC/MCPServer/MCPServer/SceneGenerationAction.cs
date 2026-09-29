using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace EngineNS.Plugins.MCPServer
{
    /// <summary>
    /// 为场景生成 Agent 提供可创建节点类型、场景占用范围和碰撞命中的只读感知能力。
    /// </summary>
    public partial class TtMCPServerPlugin
    {
        private sealed class FSceneRayHit
        {
            public string World;
            public GamePlay.Scene.TtNode Node;
            public VHitResult Result;
            public double Distance;
        }

        [Bricks.AIGC.TtMCPTool("list_scene_node_types",
            "Lists loaded, concrete TtNode subclasses that have a valid TtNodeAttribute and can therefore be " +
            "passed to add_scene_node. Returns stable type metadata and public member schemas without constructing " +
            "nodes or invoking property getters. keyword is matched case-insensitively against type names, the " +
            "default name prefix, and NodeData type.",
            returnDescription: "{items:[{typeName,shortName,defaultNamePrefix,nodeDataType,properties:[{path," +
            "memberKind,type,canRead,canWrite,declaredOn}]}],total:int,returned:int,truncated:boolean,warnings:string[]}")]
        public static string ListSceneNodeTypes(
            [Bricks.AIGC.TtMCPParameter("Optional case-insensitive keyword matched against node type metadata")] string keyword = "",
            [Bricks.AIGC.TtMCPParameter("Maximum number of node types to return, clamped to 1..500")] double maxCount = 100)
        {
            LogToolCall("list_scene_node_types", $"keyword={keyword}, maxCount={maxCount}");

            try
            {
                var keywordText = (keyword ?? "").Trim();
                var limitValue = double.IsNaN(maxCount) ? 100.0 : Math.Clamp(maxCount, 1.0, 500.0);
                var limit = (int)limitValue;
                var warnings = new List<string>();
                var types = new List<Type>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                for (int i = 0; i < assemblies.Length; ++i)
                {
                    var assembly = assemblies[i];
                    Type[] assemblyTypes;
                    try
                    {
                        assemblyTypes = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        var loadedTypes = new List<Type>();
                        if (ex.Types != null)
                        {
                            for (int j = 0; j < ex.Types.Length; ++j)
                            {
                                if (ex.Types[j] != null)
                                    loadedTypes.Add(ex.Types[j]);
                            }
                        }
                        assemblyTypes = loadedTypes.ToArray();
                        warnings.Add($"assembly '{assembly.GetName().Name}' loaded only {assemblyTypes.Length} types: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        warnings.Add($"assembly '{assembly.GetName().Name}' was skipped: {UnwrapException(ex).Message}");
                        continue;
                    }

                    for (int j = 0; j < assemblyTypes.Length; ++j)
                    {
                        var type = assemblyTypes[j];
                        if (type == null || type.IsAbstract || type.ContainsGenericParameters ||
                            type.IsSubclassOf(typeof(GamePlay.Scene.TtNode)) == false)
                            continue;

                        try
                        {
                            var attribute = GamePlay.Scene.TtNode.GetNodeAttribute(type);
                            var nodeDataType = attribute?.NodeDataType;
                            if (nodeDataType == null || nodeDataType.IsAbstract ||
                                typeof(GamePlay.Scene.TtNodeData).IsAssignableFrom(nodeDataType) == false)
                                continue;

                            var searchable = $"{type.FullName} {type.Name} {attribute.DefaultNamePrefix} " +
                                             $"{nodeDataType.FullName} {nodeDataType.Name}";
                            if (keywordText.Length > 0 &&
                                searchable.IndexOf(keywordText, StringComparison.OrdinalIgnoreCase) < 0)
                                continue;
                            types.Add(type);
                        }
                        catch (Exception ex)
                        {
                            warnings.Add($"type '{type.FullName}' was skipped: {UnwrapException(ex).Message}");
                        }
                    }
                }

                types.Sort((left, right) => string.Compare(
                    left.FullName ?? left.Name, right.FullName ?? right.Name, StringComparison.OrdinalIgnoreCase));

                var items = new List<object>();
                var returned = Math.Min(types.Count, limit);
                for (int i = 0; i < returned; ++i)
                {
                    var type = types[i];
                    try
                    {
                        var attribute = GamePlay.Scene.TtNode.GetNodeAttribute(type);
                        var properties = new List<object>();
                        CollectPublicMemberSchema(type, "", properties);
                        CollectPublicMemberSchema(attribute.NodeDataType, "NodeData.", properties);
                        properties.Sort(CompareMemberSchema);
                        items.Add(new
                        {
                            typeName = type.FullName ?? type.Name,
                            shortName = type.Name,
                            defaultNamePrefix = attribute.DefaultNamePrefix ?? "",
                            nodeDataType = FriendlyTypeName(attribute.NodeDataType),
                            properties,
                        });
                    }
                    catch (Exception ex)
                    {
                        warnings.Add($"type '{type.FullName}' schema was skipped: {UnwrapException(ex).Message}");
                    }
                }

                return JsonSerializer.Serialize(new
                {
                    items,
                    total = types.Count,
                    returned = items.Count,
                    truncated = returned < types.Count,
                    warnings,
                }, NodePropertyJsonOptions);
            }
            catch (Exception ex)
            {
                return FailJson($"listing scene node types failed: {UnwrapException(ex).Message}");
            }
        }

        [Bricks.AIGC.TtMCPTool("get_scene_bounds",
            "Returns world-space transforms and AbsAABB bounds for matching nodes in worlds exposed by currently " +
            "open scene and sequence editors. Filters are case-insensitive substrings. Nodes without a valid " +
            "BoundVolume are skipped and counted. Coordinates are left-handed, Y-up, and measured in meters.",
            returnDescription: "{worlds:string[],nodes:[{nodeId,name,type,world,transform:{position,scale," +
            "rotation},absAabb:{minimum,maximum,center,size}}],matched:int,returned:int,truncated:boolean," +
            "skippedWithoutBounds:int,skippedRepeatedNodes:int,aggregateBounds:{minimum,maximum,center,size}|null," +
            "coordinateSystem:string,error:string}")]
        public static string GetSceneBounds(
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of the world source, such as SceneEditor:")] string worldFilter = "",
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of NodeName; empty matches all nodes")] string nodeName = "",
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of the runtime type full name; empty matches all types")] string typeFilter = "",
            [Bricks.AIGC.TtMCPParameter("Maximum number of node details to return; aggregate bounds and counts still cover every match")] double maxCount = 100)
        {
            LogToolCall("get_scene_bounds",
                $"worldFilter={worldFilter}, nodeName={nodeName}, typeFilter={typeFilter}, maxCount={maxCount}");

            var limitValue = double.IsNaN(maxCount) ? 100.0 : Math.Clamp(maxCount, 1.0, 1000.0);
            var limit = (int)limitValue;
            string result = null;
            object payload = null;
            try
            {
                var dispatched = TtMainThreadDispatcher.Invoke(() =>
                {
                    var roots = new List<KeyValuePair<string, GamePlay.Scene.TtNode>>();
                    GatherWorldRoots(roots);
                    if (roots.Count == 0)
                    {
                        result = FailJson("no open scene or sequence world is available; open a .scene or sequence editor first");
                        return;
                    }

                    var worldText = (worldFilter ?? "").Trim();
                    var nodeText = (nodeName ?? "").Trim();
                    var typeText = (typeFilter ?? "").Trim();
                    var matchedRoots = new List<KeyValuePair<string, GamePlay.Scene.TtNode>>();
                    var availableWorlds = new List<string>();
                    for (int i = 0; i < roots.Count; ++i)
                    {
                        availableWorlds.Add(roots[i].Key);
                        if (worldText.Length == 0 ||
                            roots[i].Key.IndexOf(worldText, StringComparison.OrdinalIgnoreCase) >= 0)
                            matchedRoots.Add(roots[i]);
                    }
                    if (matchedRoots.Count == 0)
                    {
                        result = JsonSerializer.Serialize(new
                        {
                            error = $"worldFilter '{worldFilter}' matched no open world",
                            availableWorlds,
                        });
                        return;
                    }

                    var nodes = new List<object>();
                    var matchedCount = 0;
                    var skippedWithoutBounds = 0;
                    var skippedRepeatedNodes = 0;
                    var aggregate = DBoundingBox.EmptyBox();
                    var hasAggregate = false;
                    var worlds = new List<string>();
                    for (int i = 0; i < matchedRoots.Count; ++i)
                    {
                        worlds.Add(matchedRoots[i].Key);
                        CollectSceneBounds(matchedRoots[i].Value, matchedRoots[i].Key, nodeText, typeText,
                            limit, nodes, ref matchedCount, ref skippedWithoutBounds, ref skippedRepeatedNodes,
                            ref aggregate, ref hasAggregate);
                    }

                    var validBoundsCount = matchedCount - skippedWithoutBounds;
                    payload = new
                    {
                        worlds,
                        nodes,
                        matched = matchedCount,
                        returned = nodes.Count,
                        truncated = nodes.Count < validBoundsCount,
                        skippedWithoutBounds,
                        skippedRepeatedNodes,
                        aggregateBounds = hasAggregate ? DescribeBounds(in aggregate) : null,
                        coordinateSystem = "left-handed, Y-up, meters",
                    };
                });

                if (dispatched == false)
                    return FailJson("timed out waiting for the engine main thread");
                if (result != null)
                    return result;
                return JsonSerializer.Serialize(payload, NodePropertyJsonOptions);
            }
            catch (Exception ex)
            {
                return FailJson($"reading scene bounds failed: {UnwrapException(ex).Message}");
            }
        }

        [Bricks.AIGC.TtMCPTool("inspect_bezier_splines",
            "Inspects TtBezierSplineNode instances in worlds exposed by currently open scene and sequence editors. " +
            "Returns local and world-space anchors/control handles and validates each spline with the same " +
            "TtSplineRegionSnapshot.Create path used by PGC Spline Region. Name and world filters are " +
            "case-insensitive substrings. Coordinates are left-handed, Y-up, and measured in meters.",
            returnDescription: "{worlds:string[],splines:[{nodeId,name,parentName,world,isClosed,pointCount," +
            "segmentCount,length,revision,segments,transform,absAabb,points:[{index,local:{anchor,leftControl," +
            "rightControl},world:{anchor,leftControl,rightControl}}],regionValidation:{isValid,invalidReason," +
            "boundaryPointCount,minimumXZ,maximumXZ,minimumY,maximumY,averageY}}],matched:int,returned:int," +
            "truncated:boolean,warnings:string[],coordinateSystem:string,error:string}")]
        public static string InspectBezierSplines(
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of NodeName; empty matches all Bezier splines")] string nodeName = "",
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of the world source; empty checks all open worlds")] string worldFilter = "",
            [Bricks.AIGC.TtMCPParameter("PGC region border sample spacing in meters; values <= 0 use 0.25")] double borderSampleSpacing = 0.25,
            [Bricks.AIGC.TtMCPParameter("Maximum number of spline details to return; values <= 0 use 20, clamped to 1..100")] double maxCount = 20)
        {
            LogToolCall("inspect_bezier_splines",
                $"nodeName={nodeName}, worldFilter={worldFilter}, borderSampleSpacing={borderSampleSpacing}, maxCount={maxCount}");

            var spacing = double.IsFinite(borderSampleSpacing) && borderSampleSpacing > 0.0 ?
                (float)Math.Clamp(borderSampleSpacing, 1e-5, 100000.0) : 0.25f;
            var limitValue = double.IsFinite(maxCount) && maxCount > 0.0 ?
                Math.Clamp(maxCount, 1.0, 100.0) : 20.0;
            var limit = (int)limitValue;
            string result = null;
            object payload = null;
            try
            {
                var dispatched = TtMainThreadDispatcher.Invoke(() =>
                {
                    var roots = new List<KeyValuePair<string, GamePlay.Scene.TtNode>>();
                    GatherWorldRoots(roots);
                    if (roots.Count == 0)
                    {
                        result = FailJson("no open scene or sequence world is available; open a .scene or sequence editor first");
                        return;
                    }

                    var worldText = (worldFilter ?? "").Trim();
                    var nodeText = (nodeName ?? "").Trim();
                    var worlds = new List<string>();
                    var splines = new List<object>();
                    var warnings = new List<string>();
                    var matchedCount = 0;
                    var visited = new HashSet<GamePlay.Scene.TtNode>();
                    for (int rootIndex = 0; rootIndex < roots.Count; ++rootIndex)
                    {
                        var worldName = roots[rootIndex].Key;
                        if (worldText.Length > 0 &&
                            worldName.IndexOf(worldText, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;
                        worlds.Add(worldName);

                        var pending = new Stack<GamePlay.Scene.TtNode>();
                        pending.Push(roots[rootIndex].Value);
                        while (pending.Count > 0)
                        {
                            var node = pending.Pop();
                            if (node == null || visited.Add(node) == false)
                                continue;
                            for (int childIndex = node.Children.Count - 1; childIndex >= 0; --childIndex)
                                pending.Push(node.Children[childIndex]);

                            if (node is not GamePlay.Scene.TtBezierSplineNode splineNode)
                                continue;
                            var splineName = splineNode.NodeName ?? "";
                            if (nodeText.Length > 0 &&
                                splineName.IndexOf(nodeText, StringComparison.OrdinalIgnoreCase) < 0)
                                continue;
                            matchedCount++;
                            if (splines.Count >= limit)
                                continue;

                            try
                            {
                                splines.Add(DescribeBezierSpline(splineNode, worldName, spacing));
                            }
                            catch (Exception ex)
                            {
                                warnings.Add($"spline '{splineName}' could not be inspected: {UnwrapException(ex).Message}");
                            }
                        }
                    }

                    payload = new
                    {
                        worlds,
                        splines,
                        matched = matchedCount,
                        returned = splines.Count,
                        truncated = splines.Count < matchedCount,
                        warnings,
                        coordinateSystem = "left-handed, Y-up, meters",
                    };
                });

                if (dispatched == false)
                    return FailJson("timed out waiting for the engine main thread");
                if (result != null)
                    return result;
                return JsonSerializer.Serialize(payload, NodePropertyJsonOptions);
            }
            catch (Exception ex)
            {
                return FailJson($"inspecting Bezier splines failed: {UnwrapException(ex).Message}");
            }
        }

        [Bricks.AIGC.TtMCPTool("raycast_scene",
            "Casts a finite world-space segment through collision octrees of matching open scene or sequence worlds, " +
            "then performs exact per-node LineCheck tests and returns the nearest hit. Coordinates are left-handed, " +
            "Y-up, and measured in meters.",
            returnDescription: "{hit:boolean,node:{nodeId,name,type,world},position:{x,y,z},normal:{x,y,z}," +
            "distance:double,candidateCount:int,worlds:string[],coordinateSystem:string,error:string}")]
        public static string RaycastScene(
            [Bricks.AIGC.TtMCPParameter("Segment start X in world meters")] double startX,
            [Bricks.AIGC.TtMCPParameter("Segment start Y in world meters")] double startY,
            [Bricks.AIGC.TtMCPParameter("Segment start Z in world meters")] double startZ,
            [Bricks.AIGC.TtMCPParameter("Segment end X in world meters")] double endX,
            [Bricks.AIGC.TtMCPParameter("Segment end Y in world meters")] double endY,
            [Bricks.AIGC.TtMCPParameter("Segment end Z in world meters")] double endZ,
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of the world source; empty checks all open worlds")] string worldFilter = "")
        {
            LogToolCall("raycast_scene",
                $"start=({startX},{startY},{startZ}), end=({endX},{endY},{endZ}), worldFilter={worldFilter}");

            var start = new DVector3(startX, startY, startZ);
            var end = new DVector3(endX, endY, endZ);
            var segment = end - start;
            var segmentLength = segment.Length();
            if (segmentLength < 1e-6)
                return FailJson("ray segment has zero length; start and end must be different world-space points");

            string result = null;
            try
            {
                var dispatched = TtMainThreadDispatcher.Invoke(() =>
                {
                    var roots = new List<KeyValuePair<string, GamePlay.Scene.TtNode>>();
                    GatherWorldRoots(roots);
                    if (roots.Count == 0)
                    {
                        result = FailJson("no open scene or sequence world is available; open a .scene or sequence editor first");
                        return;
                    }

                    var filter = (worldFilter ?? "").Trim();
                    var availableWorlds = new List<string>();
                    var worlds = new List<string>();
                    var initializedOctreeCount = 0;
                    var collidableNodeCount = 0;
                    var candidateCount = 0;
                    FSceneRayHit closest = null;
                    var direction = segment / segmentLength;
                    var ray = new DRay
                    {
                        Position = start,
                        Direction = new Vector3((float)direction.X, (float)direction.Y, (float)direction.Z),
                    };

                    for (int i = 0; i < roots.Count; ++i)
                    {
                        var pair = roots[i];
                        availableWorlds.Add(pair.Key);
                        if (filter.Length > 0 &&
                            pair.Key.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;

                        worlds.Add(pair.Key);
                        var world = pair.Value.GetWorld();
                        var octree = world?.CollideOctree;
                        if (octree?.mOctree == null)
                            continue;

                        initializedOctreeCount++;
                        collidableNodeCount += octree.mOctree.Count;
                        var candidates = new List<GamePlay.Scene.TtNode>();
                        octree.GetColliding(candidates, in ray, segmentLength);
                        candidateCount += candidates.Count;
                        for (int j = 0; j < candidates.Count; ++j)
                        {
                            var node = candidates[j];
                            if (node == null || node.Placement == null)
                                continue;

                            var hitResult = new VHitResult();
                            if (node.LineCheck(in start, in end, ref hitResult) == false)
                                continue;
                            var distance = (hitResult.Position - start).Length();
                            if (distance < -1e-6 || distance > segmentLength + 1e-6)
                                continue;
                            if (closest == null || distance < closest.Distance)
                            {
                                closest = new FSceneRayHit
                                {
                                    World = pair.Key,
                                    Node = node,
                                    Result = hitResult,
                                    Distance = distance,
                                };
                            }
                        }
                    }

                    if (worlds.Count == 0)
                    {
                        result = JsonSerializer.Serialize(new
                        {
                            error = $"worldFilter '{worldFilter}' matched no open world",
                            availableWorlds,
                        });
                        return;
                    }
                    if (initializedOctreeCount == 0)
                    {
                        result = FailJson("matching worlds have no initialized collision octree; wait for world initialization or reopen the scene");
                        return;
                    }
                    if (collidableNodeCount == 0)
                    {
                        result = FailJson("matching worlds contain no collidable nodes; ensure nodes have valid BoundVolume data");
                        return;
                    }
                    if (closest == null)
                    {
                        result = JsonSerializer.Serialize(new
                        {
                            hit = false,
                            candidateCount,
                            worlds,
                            coordinateSystem = "left-handed, Y-up, meters",
                        });
                        return;
                    }

                    var hitPosition = closest.Result.Position;
                    var hitNormal = closest.Result.Normal;
                    result = JsonSerializer.Serialize(new
                    {
                        hit = true,
                        node = new
                        {
                            nodeId = closest.Node.NodeId.ToString(),
                            name = closest.Node.NodeName,
                            type = closest.Node.GetType().FullName,
                            world = closest.World,
                        },
                        position = DescribeVector(in hitPosition),
                        normal = DescribeVector(in hitNormal),
                        distance = Math.Round(closest.Distance, 6),
                        candidateCount,
                        worlds,
                        coordinateSystem = "left-handed, Y-up, meters",
                    }, NodePropertyJsonOptions);
                });

                if (dispatched == false)
                    return FailJson("timed out waiting for the engine main thread");
                return result;
            }
            catch (Exception ex)
            {
                return FailJson($"scene raycast failed: {UnwrapException(ex).Message}");
            }
        }

        private static void CollectPublicMemberSchema(Type type, string prefix, List<object> result)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            var properties = type.GetProperties(flags);
            for (int i = 0; i < properties.Length; ++i)
            {
                var property = properties[i];
                if (property.GetIndexParameters().Length != 0)
                    continue;
                var getter = property.GetGetMethod(false);
                var setter = property.GetSetMethod(false);
                if (getter == null && setter == null)
                    continue;
                result.Add(new Dictionary<string, object>
                {
                    ["path"] = prefix + property.Name,
                    ["memberKind"] = "property",
                    ["type"] = FriendlyTypeName(property.PropertyType),
                    ["canRead"] = getter != null,
                    ["canWrite"] = setter != null,
                    ["declaredOn"] = FriendlyTypeName(property.DeclaringType),
                });
            }

            var fields = type.GetFields(flags);
            for (int i = 0; i < fields.Length; ++i)
            {
                var field = fields[i];
                result.Add(new Dictionary<string, object>
                {
                    ["path"] = prefix + field.Name,
                    ["memberKind"] = "field",
                    ["type"] = FriendlyTypeName(field.FieldType),
                    ["canRead"] = true,
                    ["canWrite"] = field.IsInitOnly == false && field.IsLiteral == false,
                    ["declaredOn"] = FriendlyTypeName(field.DeclaringType),
                });
            }
        }

        private static int CompareMemberSchema(object left, object right)
        {
            var leftItem = left as Dictionary<string, object>;
            var rightItem = right as Dictionary<string, object>;
            var leftPath = leftItem?["path"] as string ?? "";
            var rightPath = rightItem?["path"] as string ?? "";
            return string.Compare(leftPath, rightPath, StringComparison.OrdinalIgnoreCase);
        }

        private static void CollectSceneBounds(GamePlay.Scene.TtNode root, string world, string nodeFilter,
            string typeFilter, int limit, List<object> result, ref int matchedCount,
            ref int skippedWithoutBounds, ref int skippedRepeatedNodes, ref DBoundingBox aggregate,
            ref bool hasAggregate)
        {
            if (root == null)
                return;

            var pending = new Stack<GamePlay.Scene.TtNode>();
            var visited = new HashSet<GamePlay.Scene.TtNode>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var node = pending.Pop();
                if (node == null)
                    continue;
                if (visited.Add(node) == false)
                {
                    skippedRepeatedNodes++;
                    continue;
                }

                var name = node.NodeName ?? "";
                var typeName = node.GetType().FullName ?? node.GetType().Name;
                var nameMatched = nodeFilter.Length == 0 ||
                                  name.IndexOf(nodeFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                var typeMatched = typeFilter.Length == 0 ||
                                  typeName.IndexOf(typeFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                if (nameMatched && typeMatched)
                {
                    matchedCount++;
                    var boundVolume = node.BoundVolume;
                    var bounds = node.AbsAABB;
                    if (boundVolume == null || bounds.IsEmpty())
                    {
                        skippedWithoutBounds++;
                    }
                    else
                    {
                        if (hasAggregate)
                            aggregate.Merge(in bounds);
                        else
                        {
                            aggregate = bounds;
                            hasAggregate = true;
                        }
                        if (result.Count < limit)
                        {
                            result.Add(new
                            {
                                nodeId = node.NodeId.ToString(),
                                name = node.NodeName,
                                type = typeName,
                                world,
                                transform = DescribeWorldTransform(node),
                                absAabb = DescribeBounds(in bounds),
                            });
                        }
                    }
                }

                for (int i = node.Children.Count - 1; i >= 0; --i)
                    pending.Push(node.Children[i]);
            }
        }

        private static object DescribeBezierSpline(GamePlay.Scene.TtBezierSplineNode node, string world,
            float borderSampleSpacing)
        {
            var spline = node.Spline;
            if (spline == null)
            {
                return new
                {
                    nodeId = node.NodeId.ToString(),
                    name = node.NodeName,
                    parentName = node.Parent?.NodeName,
                    world,
                    error = "Spline data is null",
                };
            }

            var transform = node.Placement.AbsTransform;
            var snapshot = spline.CreateSnapshot();
            var region = TtSplineRegionSnapshot.Create(snapshot, in transform, borderSampleSpacing);
            var points = new List<object>(snapshot.Points.Count);
            for (int i = 0; i < snapshot.Points.Count; ++i)
            {
                var point = snapshot.Points[i];
                var localAnchor = point.Position;
                var localLeft = point.LeftControl;
                var localRight = point.RightControl;
                var localAnchorD = localAnchor.AsDVector();
                var localLeftD = localLeft.AsDVector();
                var localRightD = localRight.AsDVector();
                var worldAnchor = transform.TransformPosition(in localAnchorD);
                var worldLeft = transform.TransformPosition(in localLeftD);
                var worldRight = transform.TransformPosition(in localRightD);
                points.Add(new
                {
                    index = i,
                    local = new
                    {
                        anchor = DescribeVector(in localAnchor),
                        leftControl = DescribeVector(in localLeft),
                        rightControl = DescribeVector(in localRight),
                    },
                    world = new
                    {
                        anchor = DescribeVector(in worldAnchor),
                        leftControl = DescribeVector(in worldLeft),
                        rightControl = DescribeVector(in worldRight),
                    },
                });
            }

            var bounds = node.AbsAABB;
            var minimumXZ = region.MinimumXZ;
            var maximumXZ = region.MaximumXZ;
            return new
            {
                nodeId = node.NodeId.ToString(),
                name = node.NodeName,
                parentName = node.Parent?.NodeName,
                world,
                isClosed = snapshot.IsClosed,
                pointCount = snapshot.Points.Count,
                segmentCount = spline.SegmentCount,
                length = Math.Round(spline.Length, 6),
                revision = snapshot.Revision,
                segments = snapshot.Segments,
                transform = DescribeWorldTransform(node),
                absAabb = bounds.IsEmpty() ? null : DescribeBounds(in bounds),
                points,
                regionValidation = new
                {
                    isValid = region.IsValid,
                    invalidReason = region.InvalidReason,
                    boundaryPointCount = region.BoundaryPoints.Count,
                    minimumXZ = DescribeVector(in minimumXZ),
                    maximumXZ = DescribeVector(in maximumXZ),
                    minimumY = Math.Round(region.MinimumY, 6),
                    maximumY = Math.Round(region.MaximumY, 6),
                    averageY = Math.Round(region.AverageY, 6),
                    borderSampleSpacing = Math.Round(borderSampleSpacing, 6),
                },
            };
        }

        private static object DescribeWorldTransform(GamePlay.Scene.TtNode node)
        {
            var placement = node.Placement;
            if (placement == null)
                return null;
            var transform = placement.AbsTransform;
            var position = transform.Position;
            var scale = transform.Scale;
            var rotation = transform.Quat;
            return new
            {
                position = DescribeVector(in position),
                scale = DescribeVector(in scale),
                rotation = new
                {
                    x = Math.Round(rotation.X, 6),
                    y = Math.Round(rotation.Y, 6),
                    z = Math.Round(rotation.Z, 6),
                    w = Math.Round(rotation.W, 6),
                },
            };
        }

        private static object DescribeBounds(in DBoundingBox bounds)
        {
            var center = bounds.GetCenter();
            var size = bounds.Maximum - bounds.Minimum;
            return new
            {
                minimum = DescribeVector(in bounds.Minimum),
                maximum = DescribeVector(in bounds.Maximum),
                center = DescribeVector(in center),
                size = DescribeVector(in size),
            };
        }

        private static object DescribeVector(in Vector2 value)
        {
            return new
            {
                x = Math.Round(value.X, 6),
                y = Math.Round(value.Y, 6),
            };
        }

        private static object DescribeVector(in DVector3 value)
        {
            return new
            {
                x = Math.Round(value.X, 6),
                y = Math.Round(value.Y, 6),
                z = Math.Round(value.Z, 6),
            };
        }

        private static object DescribeVector(in Vector3 value)
        {
            return new
            {
                x = Math.Round(value.X, 6),
                y = Math.Round(value.Y, 6),
                z = Math.Round(value.Z, 6),
            };
        }
    }
}
