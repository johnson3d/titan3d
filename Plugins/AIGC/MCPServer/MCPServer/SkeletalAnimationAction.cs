using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using EngineNS;
using EngineNS.Animation;
using EngineNS.Animation.Asset;
using EngineNS.Animation.BlendTree;
using EngineNS.Animation.BlendTree.Node;
using EngineNS.Animation.Player;
using EngineNS.Animation.SceneNode;
using EngineNS.Animation.SkeletonAnimation;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Animation.SkeletonAnimation.Skeleton;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.Animation.KawaiiPhysics;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.Editor;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.IO;

namespace EngineNS.Plugins.MCPServer
{
    /// <summary>
    /// Skeleton-animation debug MCP tools. All tools return JSON strings with a
    /// consistent shape so the LLM-side agent can iterate fields safely.
    /// </summary>
    public partial class TtMCPServerPlugin
    {
        #region Shared Helpers

        // ----------------------------------------------------------------
        //  world / scene traversal
        // ----------------------------------------------------------------

        /// <summary>
        /// Locate an active TtWorld. Order of preference:
        ///   1) PIE world (TtEngine.Instance.GameInstance.GameWorld)
        ///   2) The currently active asset editor's PreviewViewport.World
        /// Returns null when neither is available.
        /// </summary>
        private static GamePlay.TtWorld GetActiveWorld()
        {
            try
            {
                var gameInstance = TtEngine.Instance.GameInstance;
                if (gameInstance != null)
                {
                    var pieWorld = gameInstance.GameWorld;
                    if (pieWorld != null && pieWorld.Root != null)
                        return pieWorld;
                }
            }
            catch { /* fall through */ }

            try
            {
                var mainEditor = TtEngine.Instance.GfxDevice?.SlateApplication as Editor.TtMainEditorApplication;
                var mgr = mainEditor?.AssetEditorManager;
                if (mgr != null)
                {
                    // 2) the currently focused editor
                    if (mgr.CurrentActiveEditor != null &&
                        TryGetEditorWorld(mgr.CurrentActiveEditor, out var w0) && w0 != null && w0.Root != null)
                        return w0;

                    // 3) fallback: scan all opened editors. Focus may be elsewhere (e.g. the
                    //    Qoder panel) so CurrentActiveEditor can be null. Prefer a world whose
                    //    Root actually has nodes; otherwise keep the first non-empty world.
                    GamePlay.TtWorld fallback = null;
                    if (mgr.OpenedEditors != null)
                    {
                        foreach (var ed in mgr.OpenedEditors)
                        {
                            if (TryGetEditorWorld(ed, out var w) && w != null && w.Root != null)
                            {
                                if (WorldHasNodes(w)) return w;
                                fallback ??= w;
                            }
                        }
                    }
                    if (fallback != null) return fallback;
                }
            }
            catch { /* fall through */ }

            return null;
        }

        /// <summary> True if the world's Root has at least one child node. </summary>
        private static bool WorldHasNodes(GamePlay.TtWorld world)
        {
            if (world?.Root == null) return false;
            int count = 0;
            try
            {
                world.Root.IterateNodes((n, _) =>
                {
                    count++;
                    return count < 3; // early-out once we know there are children
                }, null);
            }
            catch { }
            return count > 1;
        }

        private static bool TryGetEditorWorld(IAssetEditor editor, out GamePlay.TtWorld world)
        {
            world = null;
            try
            {
                if (editor == null) return false;
                var type = editor.GetType();
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

                // 1) Field named "PreviewViewport" (most editors use this)
                var field = type.GetField("PreviewViewport", flags);
                if (field != null)
                {
                    var pv = field.GetValue(editor) as Graphics.Pipeline.TtViewportSlate;
                    if (pv != null && pv.World != null) { world = pv.World; return true; }
                }

                // 2) Property named "PreviewViewport"
                var prop = type.GetProperty("PreviewViewport", flags);
                if (prop != null && prop.PropertyType != typeof(void))
                {
                    var pv = prop.GetValue(editor) as Graphics.Pipeline.TtViewportSlate;
                    if (pv != null && pv.World != null) { world = pv.World; return true; }
                }

                // 3) Field/property named "Viewport" or "World"
                foreach (var name in new[] { "Viewport", "ViewportSlate", "World", "MainViewport" })
                {
                    var f = type.GetField(name, flags);
                    if (f != null)
                    {
                        var pv = f.GetValue(editor) as Graphics.Pipeline.TtViewportSlate;
                        if (pv != null && pv.World != null) { world = pv.World; return true; }
                        var w = f.GetValue(editor) as GamePlay.TtWorld;
                        if (w != null) { world = w; return true; }
                    }
                }
            }
            catch { }
            return false;
        }

        private class PlayerCollector
        {
            public List<TtSkeletonAnimPlayNode> SkeletonPlayers = new List<TtSkeletonAnimPlayNode>();
            public List<TtAnimStateMachinePlayNode> StateMachinePlayers = new List<TtAnimStateMachinePlayNode>();
        }

        private static bool CollectPlayerVisitor(TtNode node, object arg)
        {
            var collector = arg as PlayerCollector;
            if (collector == null) return true;
            if (node is TtSkeletonAnimPlayNode sap)
                collector.SkeletonPlayers.Add(sap);
            else if (node is TtAnimStateMachinePlayNode smp)
                collector.StateMachinePlayers.Add(smp);
            return true;
        }

        private static PlayerCollector CollectAllPlayers(GamePlay.TtWorld world)
        {
            var collector = new PlayerCollector();
            if (world?.Root != null)
            {
                world.Root.IterateNodes(CollectPlayerVisitor, collector);
            }
            return collector;
        }

        // ----------------------------------------------------------------
        //  bone-node matching
        // ----------------------------------------------------------------

        /// <summary>
        /// Walk the parent chain of a TtNode to find the TtMeshNode hosting it
        /// (e.g. TtSkeletonAnimPlayNode sits under a mesh). Falls back to walking
        /// descendants if we started from the mesh itself.
        /// </summary>
        private static TtMeshNode FindParentMeshNode(TtNode node)
        {
            var cur = node;
            while (cur != null)
            {
                if (cur is TtMeshNode mn) return mn;
                cur = cur.Parent;
            }
            return null;
        }

        /// <summary>
        /// Find first descendant of the given node matching predicate.
        /// </summary>
        private static T FindFirstDescendant<T>(TtNode root, Func<T, bool> pred) where T : TtNode
        {
            T found = null;
            root?.IterateNodes((n, _) =>
            {
                if (n is T t && pred(t))
                {
                    found = t;
                    return false;
                }
                return true;
            }, null);
            return found;
        }

        private static TtSkeletonAnimPlayNode FindSkeletonPlayerByMesh(GamePlay.TtWorld world, string meshName)
        {
            if (world?.Root == null) return null;
            var players = CollectAllPlayers(world).SkeletonPlayers;
            foreach (var p in players)
            {
                var mesh = FindParentMeshNode(p);
                if (mesh == null) continue;
                if (string.IsNullOrEmpty(meshName) ||
                    (mesh.NodeName != null && mesh.NodeName.IndexOf(meshName, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return p;
                }
            }
            return null;
        }

        private static TtAnimStateMachinePlayNode FindStateMachinePlayerByMesh(GamePlay.TtWorld world, string meshName)
        {
            if (world?.Root == null) return null;
            var players = CollectAllPlayers(world).StateMachinePlayers;
            foreach (var p in players)
            {
                var mesh = FindParentMeshNode(p);
                if (mesh == null) continue;
                if (string.IsNullOrEmpty(meshName) ||
                    (mesh.NodeName != null && mesh.NodeName.IndexOf(meshName, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return p;
                }
            }
            return null;
        }

        private static TtMeshNode FindMeshNodeByName(GamePlay.TtWorld world, string meshName)
        {
            if (world?.Root == null || string.IsNullOrEmpty(meshName)) return null;
            TtMeshNode found = null;
            world.Root.IterateNodes((n, _) =>
            {
                if (n is TtMeshNode mn &&
                    (mn.NodeName?.IndexOf(meshName, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    found = mn;
                    return false;
                }
                return true;
            }, null);
            return found;
        }

        // ----------------------------------------------------------------
        //  FTransform / pose serialization helpers
        // ----------------------------------------------------------------

        private static Dictionary<string, object> SerializeTransform(FTransform t)
        {
            return new Dictionary<string, object>
            {
                ["pos"] = new[] { t.Position.X, t.Position.Y, t.Position.Z },
                ["quat"] = new[] { t.Quat.X, t.Quat.Y, t.Quat.Z, t.Quat.W },
                ["scale"] = new[] { t.Scale.X, t.Scale.Y, t.Scale.Z },
                ["isIdentity"] = t.IsIdentity,
            };
        }

        private static Dictionary<string, object> SerializeBone(ILimbDesc desc, FTransform t)
        {
            var entry = new Dictionary<string, object>
            {
                ["name"] = desc?.Name ?? "",
                ["index"] = desc != null ? (int)desc.NameHash : 0,
                ["parent"] = desc?.ParentName ?? "",
                ["pos"] = new[] { t.Position.X, t.Position.Y, t.Position.Z },
                ["quat"] = new[] { t.Quat.X, t.Quat.Y, t.Quat.Z, t.Quat.W },
                ["scale"] = new[] { t.Scale.X, t.Scale.Y, t.Scale.Z },
            };
            return entry;
        }

        private static bool HasNaNOrInf(FTransform t)
        {
            return double.IsNaN(t.Position.X) || double.IsInfinity(t.Position.X) ||
                   double.IsNaN(t.Position.Y) || double.IsInfinity(t.Position.Y) ||
                   double.IsNaN(t.Position.Z) || double.IsInfinity(t.Position.Z) ||
                   float.IsNaN(t.Quat.X)     || float.IsInfinity(t.Quat.X)     ||
                   float.IsNaN(t.Quat.Y)     || float.IsInfinity(t.Quat.Y)     ||
                   float.IsNaN(t.Quat.Z)     || float.IsInfinity(t.Quat.Z)     ||
                   float.IsNaN(t.Quat.W)     || float.IsInfinity(t.Quat.W);
        }

        // ----------------------------------------------------------------
        //  async-to-sync bridge
        // ----------------------------------------------------------------

        /// <summary>
        /// Run an async TtTask on the main thread and block until it completes.
        /// Uses TtTask.GetResultUntilCompleted() which routes the wait through
        /// TtContextThread.CurrentContext.WaitTask, so the game thread drives
        /// the task forward instead of parking a thread-pool thread with
        /// GetAwaiter().GetResult() (which can deadlock on the engine's
        /// custom sync context).
        /// Used by tools that need to load assets (e.g. animation clip) synchronously.
        /// </summary>
        private static T RunSync<T>(System.Func<Thread.Async.TtTask<T>> task)
        {
            try
            {
                return task().GetResultUntilCompleted();
            }
            catch (System.AggregateException ae)
            {
                throw ae.InnerException ?? ae;
            }
        }

        // ----------------------------------------------------------------
        //  reflection helpers for state machines & blend trees
        // ----------------------------------------------------------------

        private static object SafeGetProperty(object target, string name)
        {
            if (target == null) return null;
            try
            {
                var p = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null && p.CanRead) return p.GetValue(target);
                var f = target.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null) return f.GetValue(target);
            }
            catch { }
            return null;
        }

        private static object SafeGetField(object target, string name)
        {
            if (target == null) return null;
            try
            {
                var f = target.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null) return f.GetValue(target);
                var p = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null && p.CanRead) return p.GetValue(target);
            }
            catch { }
            return null;
        }

        private static object SafeInvoke(object target, string method, params object[] args)
        {
            if (target == null) return null;
            try
            {
                var m = target.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (m != null) return m.Invoke(target, args);
            }
            catch { }
            return null;
        }

        // ----------------------------------------------------------------
        //  snapshot persistence helpers
        // ----------------------------------------------------------------

        private static string GetSnapshotDir()
        {
            try
            {
                var dir = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cache);
                dir = Path.Combine(dir, "pose_snapshots");
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                return dir;
            }
            catch
            {
                var dir = Path.Combine(Path.GetTempPath(), "titanengine_pose_snapshots");
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                return dir;
            }
        }

        private static string GetSnapshotIndexPath()
        {
            return Path.Combine(GetSnapshotDir(), "pose_snapshots_index.json");
        }

        private static Dictionary<string, object> ReadSnapshotIndex()
        {
            try
            {
                var path = GetSnapshotIndexPath();
                if (!File.Exists(path)) return new Dictionary<string, object>();
                var text = File.ReadAllText(path);
                if (string.IsNullOrEmpty(text)) return new Dictionary<string, object>();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(text) ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        private static void WriteSnapshotIndex(Dictionary<string, object> index)
        {
            try
            {
                var path = GetSnapshotIndexPath();
                var text = JsonSerializer.Serialize(index,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, text);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Warning,
                    $"Failed to write snapshot index: {ex.Message}");
            }
        }

        // ----------------------------------------------------------------
        //  shared logging
        // ----------------------------------------------------------------

        private static void LogToolCall(string toolName, string args)
        {
            Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Info,
                $"[anim][{toolName}] {args}");
        }

        #endregion

        // =================================================================
        //  Section 1: Player discovery
        // =================================================================

        [Bricks.AIGC.TtMCPTool("list_skeleton_anim_players",
            "List all skeleton animation / state machine player nodes in the active world. Useful as an entry point for further queries.",
            returnDescription: "{world: string, total: number, skeletonPlayers: [{nodeId, name, parentMeshName, clipName, hasPlayer, time, duration}], stateMachinePlayers: [{nodeId, name, parentMeshName}]}")]
        public static string ListSkeletonAnimPlayers(
            [Bricks.AIGC.TtMCPParameter("Reserved for future multi-world support; currently 0 only")] double worldIndex = 0,
            [Bricks.AIGC.TtMCPParameter("Include TtAnimStateMachinePlayNode in the result")] bool includeStateMachine = true)
        {
            try
            {
                LogToolCall("list_skeleton_anim_players", $"worldIndex={worldIndex}, includeStateMachine={includeStateMachine}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { error = "No active world available. Start a PIE session or open an asset editor that owns a PreviewViewport." });

                var collector = CollectAllPlayers(world);
                var skList = new List<Dictionary<string, object>>();
                foreach (var p in collector.SkeletonPlayers)
                {
                    var mesh = FindParentMeshNode(p);
                    var player = p.Player;
                    var entry = new Dictionary<string, object>
                    {
                        ["nodeId"] = p.NodeId.ToString(),
                        ["name"] = p.NodeName ?? "",
                        ["parentMeshName"] = mesh?.NodeName ?? "",
                        ["hasPlayer"] = player != null,
                        ["clipName"] = player?.SkeletonAnimClip?.AssetName?.Name ?? "",
                        ["time"] = player?.Time ?? 0f,
                        ["duration"] = player?.Duration ?? 0f,
                    };
                    skList.Add(entry);
                }
                var smList = new List<Dictionary<string, object>>();
                if (includeStateMachine)
                {
                    foreach (var p in collector.StateMachinePlayers)
                    {
                        var mesh = FindParentMeshNode(p);
                        var player = p.Player;
                        smList.Add(new Dictionary<string, object>
                        {
                            ["nodeId"] = p.NodeId.ToString(),
                            ["name"] = p.NodeName ?? "",
                            ["parentMeshName"] = mesh?.NodeName ?? "",
                            ["hasPlayer"] = player != null,
                            ["hasStateMachine"] = player?.StateMachine != null,
                            ["stateMachineType"] = player?.StateMachine?.GetType().Name ?? "",
                        });
                    }
                }
                return JsonSerializer.Serialize(new
                {
                    world = "active",
                    total = skList.Count + smList.Count,
                    skeletonPlayerCount = skList.Count,
                    stateMachinePlayerCount = smList.Count,
                    skeletonPlayers = skList,
                    stateMachinePlayers = smList,
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("find_anim_player_by_mesh",
            "Locate the skeleton / state machine player node attached to a mesh node by partial-name match.",
            returnDescription: "{found: bool, kind: 'skeleton'|'stateMachine'|null, nodeName, parentMeshName, hasPlayer, clipName, time, duration}")]
        public static string FindAnimPlayerByMesh(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name to search for (case-insensitive). Empty to pick the first one.")] string meshName = "")
        {
            try
            {
                LogToolCall("find_anim_player_by_mesh", $"meshName={meshName}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world" });

                var sk = FindSkeletonPlayerByMesh(world, meshName);
                if (sk != null)
                {
                    var mesh = FindParentMeshNode(sk);
                    return JsonSerializer.Serialize(new
                    {
                        found = true,
                        kind = "skeleton",
                        nodeName = sk.NodeName ?? "",
                        parentMeshName = mesh?.NodeName ?? "",
                        hasPlayer = sk.Player != null,
                        clipName = sk.Player?.SkeletonAnimClip?.AssetName?.Name ?? "",
                        time = sk.Player?.Time ?? 0f,
                        duration = sk.Player?.Duration ?? 0f,
                    });
                }
                var sm = FindStateMachinePlayerByMesh(world, meshName);
                if (sm != null)
                {
                    var mesh = FindParentMeshNode(sm);
                    return JsonSerializer.Serialize(new
                    {
                        found = true,
                        kind = "stateMachine",
                        nodeName = sm.NodeName ?? "",
                        parentMeshName = mesh?.NodeName ?? "",
                        hasPlayer = sm.Player != null,
                        stateMachineType = sm.Player?.StateMachine?.GetType().Name ?? "",
                    });
                }
                return JsonSerializer.Serialize(new { found = false, error = $"No anim player found for mesh '{meshName}'" });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("get_anim_state_machine_players",
            "List all TtAnimStateMachinePlayNode instances and the state machine they own.",
            returnDescription: "{total, players: [{nodeName, parentMeshName, hasPlayer, hasStateMachine, stateMachineType, currentStateName, isInitialized}]}")]
        public static string GetAnimStateMachinePlayers(
            [Bricks.AIGC.TtMCPParameter("Reserved for future multi-world support; currently 0 only")] double worldIndex = 0)
        {
            try
            {
                LogToolCall("get_anim_state_machine_players", $"worldIndex={worldIndex}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { error = "No active world" });

                var list = new List<Dictionary<string, object>>();
                foreach (var p in CollectAllPlayers(world).StateMachinePlayers)
                {
                    var mesh = FindParentMeshNode(p);
                    var sm = p.Player?.StateMachine;
                    var cur = SafeGetProperty(sm, "CurrentState");
                    list.Add(new Dictionary<string, object>
                    {
                        ["nodeName"] = p.NodeName ?? "",
                        ["parentMeshName"] = mesh?.NodeName ?? "",
                        ["hasPlayer"] = p.Player != null,
                        ["hasStateMachine"] = sm != null,
                        ["stateMachineType"] = sm?.GetType().Name ?? "",
                        ["currentStateName"] = cur?.GetType().Name ?? "",
                        ["isInitialized"] = sm != null,
                    });
                }
                return JsonSerializer.Serialize(new { total = list.Count, players = list });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        // =================================================================
        //  Section 2: Final Pose retrieval
        // =================================================================

        [Bricks.AIGC.TtMCPTool("get_final_local_pose",
            "Dump the final local-space pose produced by the skeleton animation player attached to the named mesh.",
            returnDescription: "{found, space: 'local', boneCount, bones: [{name, index, parent, pos, quat, scale}]}")]
        public static string GetFinalLocalPose(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name (case-insensitive). Empty selects the first available player.")] string meshName = "",
            [Bricks.AIGC.TtMCPParameter("Optional bone name filter; empty returns all bones")] string boneNameFilter = "")
        {
            try
            {
                LogToolCall("get_final_local_pose", $"meshName={meshName}, filter={boneNameFilter}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world" });

                var player = FindSkeletonPlayerByMesh(world, meshName);
                if (player?.Player == null)
                {
                    // also accept state-machine players (their OutPose is local)
                    var sm = FindStateMachinePlayerByMesh(world, meshName);
                    if (sm?.Player?.OutPose == null)
                        return JsonSerializer.Serialize(new { found = false, error = $"No skeleton anim player / OutPose for mesh '{meshName}'" });
                    return DumpLocalPose(sm.Player.OutPose, boneNameFilter, "local");
                }
                var pose = player.Player.OutPose;
                if (pose == null)
                    return JsonSerializer.Serialize(new { found = false, error = "Player OutPose is null (player has not been ticked yet)" });
                return DumpLocalPose(pose, boneNameFilter, "local");
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("get_final_mesh_space_pose",
            "Dump the final mesh-space (absolute) pose from the mesh node's MeshSpaceRuntimePose.",
            returnDescription: "{found, space: 'mesh', boneCount, bones: [{name, index, parent, pos, quat, scale}]}")]
        public static string GetFinalMeshSpacePose(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name")] string meshName = "",
            [Bricks.AIGC.TtMCPParameter("Optional bone name filter; empty returns all bones")] string boneNameFilter = "")
        {
            try
            {
                LogToolCall("get_final_mesh_space_pose", $"meshName={meshName}, filter={boneNameFilter}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world" });

                var mesh = FindMeshNodeByName(world, meshName);
                if (mesh == null)
                    return JsonSerializer.Serialize(new { found = false, error = $"Mesh node '{meshName}' not found" });

                var pose = mesh.MeshSpaceRuntimePose;
                if (pose == null)
                    return JsonSerializer.Serialize(new { found = false, error = "MeshSpaceRuntimePose is null — mesh node has not been ticked yet" });

                return DumpLocalPose(pose, boneNameFilter, "mesh");
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private static string DumpLocalPose(IRuntimePose pose, string boneNameFilter, string space)
        {
            var bones = new List<Dictionary<string, object>>();
            int nanInfCount = 0;
            for (int i = 0; i < pose.Descs.Count; i++)
            {
                var desc = pose.Descs[i];
                var t = pose.Transforms[i];
                if (!string.IsNullOrEmpty(boneNameFilter) &&
                    (desc?.Name == null || desc.Name.IndexOf(boneNameFilter, StringComparison.OrdinalIgnoreCase) < 0))
                {
                    continue;
                }
                if (HasNaNOrInf(t)) nanInfCount++;
                bones.Add(SerializeBone(desc, t));
            }
            return JsonSerializer.Serialize(new
            {
                found = true,
                space,
                boneCount = bones.Count,
                nanInfCount,
                bones,
            });
        }

        [Bricks.AIGC.TtMCPTool("get_bone_transform",
            "Get a single bone's transform (position / rotation / scale) by name, in either local or mesh space.",
            returnDescription: "{found, meshName, boneName, space, transform: {pos, quat, scale, isIdentity}} or {found:false, error}")]
        public static string GetBoneTransform(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name")] string meshName = "",
            [Bricks.AIGC.TtMCPParameter("Bone name (exact match preferred, partial supported)")] string boneName = "",
            [Bricks.AIGC.TtMCPParameter("'local' (from the player OutPose) or 'mesh' (mesh-space). Defaults to local.")] string space = "local")
        {
            try
            {
                if (string.IsNullOrEmpty(boneName))
                    return JsonSerializer.Serialize(new { found = false, error = "boneName is required" });

                LogToolCall("get_bone_transform", $"meshName={meshName}, boneName={boneName}, space={space}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world" });

                IRuntimePose pose = null;
                if (string.Equals(space, "mesh", StringComparison.OrdinalIgnoreCase))
                {
                    var mesh = FindMeshNodeByName(world, meshName);
                    if (mesh == null) return JsonSerializer.Serialize(new { found = false, error = $"Mesh '{meshName}' not found" });
                    pose = mesh.MeshSpaceRuntimePose;
                    if (pose == null) return JsonSerializer.Serialize(new { found = false, error = "MeshSpaceRuntimePose is null" });
                }
                else
                {
                    var player = FindSkeletonPlayerByMesh(world, meshName);
                    if (player == null)
                    {
                        var sm = FindStateMachinePlayerByMesh(world, meshName);
                        pose = sm?.Player?.OutPose;
                    }
                    else
                    {
                        pose = player.Player?.OutPose;
                    }
                    if (pose == null) return JsonSerializer.Serialize(new { found = false, error = "OutPose is null" });
                }

                int foundIdx = -1;
                ILimbDesc foundDesc = null;
                FTransform foundTr = FTransform.Identity;
                for (int i = 0; i < pose.Descs.Count; i++)
                {
                    var d = pose.Descs[i];
                    if (d == null) continue;
                    if (string.Equals(d.Name, boneName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundIdx = i;
                        foundDesc = d;
                        foundTr = pose.Transforms[i];
                        break;
                    }
                }
                if (foundIdx < 0)
                {
                    // partial match fallback
                    for (int i = 0; i < pose.Descs.Count; i++)
                    {
                        var d = pose.Descs[i];
                        if (d?.Name == null) continue;
                        if (d.Name.IndexOf(boneName, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            foundIdx = i;
                            foundDesc = d;
                            foundTr = pose.Transforms[i];
                            break;
                        }
                    }
                }
                if (foundIdx < 0)
                    return JsonSerializer.Serialize(new { found = false, error = $"Bone '{boneName}' not found" });

                return JsonSerializer.Serialize(new
                {
                    found = true,
                    meshName,
                    boneName = foundDesc.Name,
                    index = foundIdx,
                    space,
                    transform = SerializeTransform(foundTr),
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("get_pose_summary",
            "Compute aggregate statistics on a pose: bone count, non-identity count, bounding box, max single-axis displacement, max quaternion angle vs Identity, NaN/Inf count, root bone name.",
            returnDescription: "{found, space, boneCount, nonIdentityCount, bbox:{min,max}, maxAxisDisplacement, maxQuatAngleDeg, hasNaNOrInf, rootBoneName}")]
        public static string GetPoseSummary(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name")] string meshName = "",
            [Bricks.AIGC.TtMCPParameter("'local' or 'mesh' (defaults to local)")] string space = "local")
        {
            try
            {
                LogToolCall("get_pose_summary", $"meshName={meshName}, space={space}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world" });

                IRuntimePose pose = null;
                if (string.Equals(space, "mesh", StringComparison.OrdinalIgnoreCase))
                {
                    var mesh = FindMeshNodeByName(world, meshName);
                    if (mesh == null) return JsonSerializer.Serialize(new { found = false, error = $"Mesh '{meshName}' not found" });
                    pose = mesh.MeshSpaceRuntimePose;
                    if (pose == null) return JsonSerializer.Serialize(new { found = false, error = "MeshSpaceRuntimePose is null" });
                }
                else
                {
                    var player = FindSkeletonPlayerByMesh(world, meshName);
                    if (player == null)
                    {
                        var sm = FindStateMachinePlayerByMesh(world, meshName);
                        pose = sm?.Player?.OutPose;
                    }
                    else
                    {
                        pose = player.Player?.OutPose;
                    }
                    if (pose == null) return JsonSerializer.Serialize(new { found = false, error = "OutPose is null" });
                }

                int nonId = 0;
                int nanInf = 0;
                double maxAxisDisp = 0;
                double maxQuatAngleDeg = 0;
                double minX = double.PositiveInfinity, minY = double.PositiveInfinity, minZ = double.PositiveInfinity;
                double maxX = double.NegativeInfinity, maxY = double.NegativeInfinity, maxZ = double.NegativeInfinity;
                string rootName = "";
                for (int i = 0; i < pose.Descs.Count; i++)
                {
                    var d = pose.Descs[i];
                    var t = pose.Transforms[i];
                    if (HasNaNOrInf(t)) { nanInf++; continue; }
                    if (!t.IsIdentity) nonId++;
                    var px = t.Position.X; var py = t.Position.Y; var pz = t.Position.Z;
                    if (px < minX) minX = px; if (px > maxX) maxX = px;
                    if (py < minY) minY = py; if (py > maxY) maxY = py;
                    if (pz < minZ) minZ = pz; if (pz > maxZ) maxZ = pz;
                    var disp = System.Math.Max(System.Math.Abs(px), System.Math.Max(System.Math.Abs(py), System.Math.Abs(pz)));
                    if (disp > maxAxisDisp) maxAxisDisp = disp;
                    var ang = System.Math.Abs(t.Quat.Angle) * 180.0 / System.Math.PI;
                    if (ang > maxQuatAngleDeg) maxQuatAngleDeg = ang;
                    if (string.IsNullOrEmpty(d?.ParentName)) rootName = d?.Name ?? "";
                }
                return JsonSerializer.Serialize(new
                {
                    found = true,
                    space,
                    boneCount = pose.Descs.Count,
                    nonIdentityCount = nonId,
                    bbox = new
                    {
                        min = new[] { double.IsInfinity(minX) ? 0.0 : minX, double.IsInfinity(minY) ? 0.0 : minY, double.IsInfinity(minZ) ? 0.0 : minZ },
                        max = new[] { double.IsInfinity(maxX) ? 0.0 : maxX, double.IsInfinity(maxY) ? 0.0 : maxY, double.IsInfinity(maxZ) ? 0.0 : maxZ },
                    },
                    maxAxisDisplacement = maxAxisDisp,
                    maxQuatAngleDeg,
                    hasNaNOrInf = nanInf > 0,
                    nanInfCount = nanInf,
                    rootBoneName = rootName,
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        // =================================================================
        //  Section 3: AnimNode intermediate state
        // =================================================================

        [Bricks.AIGC.TtMCPTool("get_player_state",
            "Return runtime statistics for the TtSkeletonAnimationPlayer on a mesh: time, duration, clip name, sample rate, OutPose availability, command time, notify count.",
            returnDescription: "{found, clipName, assetName, sampleRate, time, duration, progress, hasOutPose, evaluateTime, notifyCount, looping}")]
        public static string GetPlayerState(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name")] string meshName = "")
        {
            try
            {
                LogToolCall("get_player_state", $"meshName={meshName}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world" });

                var playerNode = FindSkeletonPlayerByMesh(world, meshName);
                if (playerNode == null)
                    return JsonSerializer.Serialize(new { found = false, error = $"No TtSkeletonAnimPlayNode for mesh '{meshName}'" });
                var player = playerNode.Player;
                if (player == null)
                    return JsonSerializer.Serialize(new { found = false, error = "Player is null" });

                var cmdTime = SafeGetField(player, "mAnimEvaluateCommand") is object cmd
                    ? (float?)SafeGetProperty(cmd, "Time") ?? null
                    : null;

                var clip = player.SkeletonAnimClip;
                return JsonSerializer.Serialize(new
                {
                    found = true,
                    clipName = clip?.AssetName?.Name ?? "",
                    assetName = clip?.AssetName?.ToString() ?? "",
                    sampleRate = clip?.SampleRate ?? 0f,
                    time = player.Time,
                    duration = player.Duration,
                    progress = player.Duration > 0 ? (double)player.Time / player.Duration : 0.0,
                    hasOutPose = player.OutPose != null,
                    evaluateTime = cmdTime,
                    notifyCount = clip?.Notifies?.Count ?? 0,
                    looping = true, // player wraps via modulo, always treated as looped
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("get_anim_state_machine_state",
            "Return state-machine specific information for the player attached to a mesh: current state, blend tree type, OutPose availability.",
            returnDescription: "{found, hasPlayer, stateMachineType, currentStateName, blendTreeType, hasOutPose}")]
        public static string GetAnimStateMachineState(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name")] string meshName = "")
        {
            try
            {
                LogToolCall("get_anim_state_machine_state", $"meshName={meshName}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world" });

                var playerNode = FindStateMachinePlayerByMesh(world, meshName);
                if (playerNode == null)
                    return JsonSerializer.Serialize(new { found = false, error = $"No TtAnimStateMachinePlayNode for mesh '{meshName}'" });
                var player = playerNode.Player;
                if (player == null)
                    return JsonSerializer.Serialize(new { found = false, error = "Player is null" });

                var sm = player.StateMachine;
                var cur = sm != null ? SafeGetProperty(sm, "CurrentState") : null;
                var bt = sm != null ? SafeGetField(sm, "BlendTree") : null;

                return JsonSerializer.Serialize(new
                {
                    found = true,
                    hasPlayer = true,
                    stateMachineType = sm?.GetType().Name ?? "",
                    currentStateName = cur?.GetType().Name ?? "",
                    blendTreeType = bt?.GetType().Name ?? "",
                    hasOutPose = player.OutPose != null,
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("get_blend_tree_dump",
            "Recursively walk a state machine's blend tree, returning the type and key fields (FromNode / ToNode / Clip / IsLoop / Time / Weight / BlendTime) for each node.",
            returnDescription: "{found, maxDepth, depth, type, fields:{...}, children: [...]}")]
        public static string GetBlendTreeDump(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name")] string meshName = "",
            [Bricks.AIGC.TtMCPParameter("Maximum recursion depth (safety cap, default 4)")] double maxDepth = 4)
        {
            try
            {
                LogToolCall("get_blend_tree_dump", $"meshName={meshName}, maxDepth={maxDepth}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world" });

                object root = null;
                var sm = FindStateMachinePlayerByMesh(world, meshName);
                if (sm != null)
                {
                    root = SafeGetField(sm.Player?.StateMachine, "BlendTree");
                }
                if (root == null)
                {
                    // also accept a skeleton-player whose Player has a BlendTree (uncommon)
                    return JsonSerializer.Serialize(new { found = false, error = "No blend tree on the chosen player" });
                }

                int depthCap = (int)System.Math.Max(1, maxDepth);
                var node = DescribeBlendTreeNode(root, depthCap, 0);
                return JsonSerializer.Serialize(new
                {
                    found = true,
                    type = root.GetType().Name,
                    maxDepth = depthCap,
                    tree = node,
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private static Dictionary<string, object> DescribeBlendTreeNode(object node, int depthCap, int depth)
        {
            var result = new Dictionary<string, object>
            {
                ["type"] = node?.GetType().Name ?? "null",
                ["depth"] = depth,
            };
            if (node == null || depth >= depthCap) return result;

            // Read well-known fields
            var fields = new Dictionary<string, object>();
            AddFieldIfPresent(fields, node, "FromNode", v => v?.GetType().Name ?? "null");
            AddFieldIfPresent(fields, node, "ToNode", v => v?.GetType().Name ?? "null");
            AddFieldIfPresent(fields, node, "BlendTime", v => v);
            AddFieldIfPresent(fields, node, "IsLoop", v => v);
            AddFieldIfPresent(fields, node, "Time", v => v);
            AddFieldIfPresent(fields, node, "Weight", v => v);
            AddFieldIfPresent(fields, node, "BlendCompelete", v => v);
            AddFieldIfPresent(fields, node, "Clip", v => v is EngineNS.Animation.Asset.TtAnimationClip clip ? (object)clip.AssetName?.Name : v);
            // KawaiiPhysics specific
            AddFieldIfPresent(fields, node, "Chains", v => v is System.Collections.ICollection c ? c.Count : (object)0);
            AddFieldIfPresent(fields, node, "Rods", v => v is System.Collections.ICollection c ? c.Count : (object)0);
            AddFieldIfPresent(fields, node, "Alpha", v => v);
            AddFieldIfPresent(fields, node, "ElapseSecond", v => v);
            result["fields"] = fields;

            // Recurse into FromNode/ToNode
            var children = new List<Dictionary<string, object>>();
            var fromNode = SafeGetField(node, "FromNode");
            if (fromNode != null)
                children.Add(DescribeBlendTreeNode(fromNode, depthCap, depth + 1));
            var toNode = SafeGetField(node, "ToNode");
            if (toNode != null)
                children.Add(DescribeBlendTreeNode(toNode, depthCap, depth + 1));
            // Kawaii node: KawaiiComponent.Chains
            var kawaii = SafeGetProperty(node, "KawaiiComponent");
            if (kawaii != null)
            {
                var chains = SafeGetProperty(kawaii, "Chains") as System.Collections.ICollection;
                var rods = SafeGetProperty(kawaii, "Rods") as System.Collections.ICollection;
                result["kawaiiComponent"] = new
                {
                    chains = chains?.Count ?? 0,
                    rods = rods?.Count ?? 0,
                };
            }
            if (children.Count > 0) result["children"] = children;
            return result;
        }

        private static void AddFieldIfPresent(Dictionary<string, object> dict, object target, string name, Func<object, object> formatter)
        {
            try
            {
                var value = SafeGetField(target, name);
                if (value != null || IsAlwaysIncludeField(name))
                {
                    dict[name] = formatter(value);
                }
            }
            catch { }
        }

        private static bool IsAlwaysIncludeField(string name)
        {
            return name == "BlendTime" || name == "IsLoop" || name == "Time" || name == "Weight" || name == "BlendCompelete" || name == "Alpha" || name == "ElapseSecond";
        }

        [Bricks.AIGC.TtMCPTool("get_crossfade_state",
            "Locate the TtBlendTree_CrossfadePose in the mesh's state machine and report its current transition state.",
            returnDescription: "{found, blendTime, mCurrentTime, weight, blendComplete, fromType, toType, fromClipName, toClipName}")]
        public static string GetCrossfadeState(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name")] string meshName = "")
        {
            try
            {
                LogToolCall("get_crossfade_state", $"meshName={meshName}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world" });

                var sm = FindStateMachinePlayerByMesh(world, meshName);
                if (sm?.Player?.StateMachine == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No state machine for mesh" });

                var crossfade = FindCrossfadeRecursive(sm.Player.StateMachine.BlendTree, 6);
                if (crossfade == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No crossfade blend tree found" });

                var bt = crossfade;
                var fromNode = SafeGetField(bt, "FromNode");
                var toNode = SafeGetField(bt, "ToNode");
                var blendTime = SafeGetProperty(bt, "BlendTime");
                var curTime = SafeGetField(bt, "mCurrentTime");
                var weight = SafeGetProperty(bt, "BlendTime") != null
                    ? SafeGetProperty(GetDesc(bt), "Weight")
                    : null;
                // Try to read weight from animation command
                float weightVal = 0f;
                var cmd = SafeGetField(bt, "mAnimationCommand");
                if (cmd != null)
                {
                    var desc = SafeGetField(cmd, "Desc");
                    if (desc != null)
                    {
                        var w = SafeGetProperty(desc, "Weight");
                        if (w is float fw) weightVal = fw;
                    }
                }
                var complete = SafeGetProperty(bt, "BlendCompelete");
                return JsonSerializer.Serialize(new
                {
                    found = true,
                    blendTime = ToFloat(blendTime),
                    mCurrentTime = ToFloat(curTime),
                    weight = weightVal,
                    blendComplete = complete is bool b ? b : false,
                    fromType = fromNode?.GetType().Name ?? "null",
                    toType = toNode?.GetType().Name ?? "null",
                    fromClipName = TryGetClipName(fromNode),
                    toClipName = TryGetClipName(toNode),
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private static object GetDesc(object bt)
        {
            // unused; placeholder
            return null;
        }

        private static object FindCrossfadeRecursive(object node, int depthCap)
        {
            if (node == null || depthCap <= 0) return null;
            var t = node.GetType();
            if (t.Name.StartsWith("TtBlendTree_CrossfadePose", StringComparison.Ordinal))
                return node;
            var fromNode = SafeGetField(node, "FromNode");
            if (fromNode != null)
            {
                var r = FindCrossfadeRecursive(fromNode, depthCap - 1);
                if (r != null) return r;
            }
            var toNode = SafeGetField(node, "ToNode");
            if (toNode != null)
            {
                var r = FindCrossfadeRecursive(toNode, depthCap - 1);
                if (r != null) return r;
            }
            return null;
        }

        private static string TryGetClipName(object node)
        {
            if (node == null) return "";
            var clip = SafeGetField(node, "Clip") ?? SafeGetProperty(node, "Clip");
            if (clip is EngineNS.Animation.Asset.TtAnimationClip ac) return ac.AssetName?.Name ?? "";
            return "";
        }

        [Bricks.AIGC.TtMCPTool("get_kawaii_physics_state",
            "Locate Kawaii Physics simulation state on the mesh's state machine blend tree and report chain / rod counts, alpha, elapseSecond.",
            returnDescription: "{found, componentType, chains, rods, alpha, elapseSecond, gravityScale, windEnabled}")]
        public static string GetKawaiiPhysicsState(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name")] string meshName = "")
        {
            try
            {
                LogToolCall("get_kawaii_physics_state", $"meshName={meshName}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world" });

                object sm = null;
                var smNode = FindStateMachinePlayerByMesh(world, meshName);
                if (smNode != null) sm = smNode.Player?.StateMachine;
                if (sm == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No state machine" });

                var kawaii = FindKawaiiRecursive(sm, 6);
                if (kawaii == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No KawaiiPhysics node found" });

                var component = SafeGetField(kawaii, "KawaiiComponent") ?? SafeGetProperty(kawaii, "KawaiiComponent");
                int chainCount = 0, rodCount = 0;
                float alpha = 0f, elapse = 0f;
                string componentType = "";
                if (component != null)
                {
                    componentType = component.GetType().Name;
                    var chains = SafeGetProperty(component, "Chains") as System.Collections.ICollection;
                    if (chains != null) chainCount = chains.Count;
                    var rods = SafeGetProperty(component, "Rods") as System.Collections.ICollection;
                    if (rods != null) rodCount = rods.Count;
                }
                var alphaV = SafeGetField(kawaii, "Alpha");
                if (alphaV is float fa) alpha = fa;
                var elapseV = SafeGetField(kawaii, "ElapseSecond");
                if (elapseV is float fe) elapse = fe;
                return JsonSerializer.Serialize(new
                {
                    found = true,
                    componentType,
                    chains = chainCount,
                    rods = rodCount,
                    alpha,
                    elapseSecond = elapse,
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private static object FindKawaiiRecursive(object node, int depthCap)
        {
            if (node == null || depthCap <= 0) return null;
            var t = node.GetType();
            if (t.Name.IndexOf("Kawaii", StringComparison.OrdinalIgnoreCase) >= 0)
                return node;
            foreach (var name in new[] { "FromNode", "ToNode" })
            {
                var child = SafeGetField(node, name);
                if (child != null)
                {
                    var r = FindKawaiiRecursive(child, depthCap - 1);
                    if (r != null) return r;
                }
            }
            return null;
        }

        private static float ToFloat(object o)
        {
            if (o == null) return 0f;
            if (o is float f) return f;
            if (o is double d) return (float)d;
            if (o is int i) return i;
            return 0f;
        }

        // =================================================================
        //  Section 4: Pose snapshots and comparison
        // =================================================================

        [Bricks.AIGC.TtMCPTool("capture_pose_snapshot",
            "Serialize the current local / mesh-space pose of a mesh to a JSON file under <cache>/pose_snapshots/. Returns the file path and metadata for later comparison.",
            returnDescription: "{success, path, label, meshName, space, boneCount, timestamp}")]
        public static string CapturePoseSnapshot(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name")] string meshName = "",
            [Bricks.AIGC.TtMCPParameter("A free-form label to identify the snapshot (e.g. 'idle_t0', 'walk_t1')")] string label = "",
            [Bricks.AIGC.TtMCPParameter("'local', 'mesh', or 'both' (defaults to local)")] string space = "local")
        {
            try
            {
                if (string.IsNullOrEmpty(label))
                    return JsonSerializer.Serialize(new { success = false, error = "label is required" });

                LogToolCall("capture_pose_snapshot", $"meshName={meshName}, label={label}, space={space}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { success = false, error = "No active world" });

                var mesh = FindMeshNodeByName(world, meshName);
                if (mesh == null)
                    return JsonSerializer.Serialize(new { success = false, error = $"Mesh '{meshName}' not found" });

                var localPose = mesh.RuntimePose;
                var meshPose = mesh.MeshSpaceRuntimePose;
                if (localPose == null && meshPose == null)
                    return JsonSerializer.Serialize(new { success = false, error = "Both Local and Mesh poses are null" });

                var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var safeLabel = MakeSafeFileName(label);
                var safeMesh = MakeSafeFileName(mesh.NodeName ?? "mesh");
                var fname = $"{safeLabel}__{safeMesh}__{ts}.json";
                var fullPath = Path.Combine(GetSnapshotDir(), fname);

                var payload = new Dictionary<string, object>
                {
                    ["label"] = label,
                    ["meshName"] = mesh.NodeName ?? "",
                    ["nodeId"] = mesh.NodeId.ToString(),
                    ["space"] = space,
                    ["timestamp"] = DateTime.Now.ToString("o"),
                };

                if (string.Equals(space, "both", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(space, "local", StringComparison.OrdinalIgnoreCase))
                {
                    if (localPose != null) payload["local"] = SerializePoseForSnapshot(localPose);
                    else payload["localError"] = "LocalSpace pose is null";
                }
                if (string.Equals(space, "both", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(space, "mesh", StringComparison.OrdinalIgnoreCase))
                {
                    if (meshPose != null) payload["mesh"] = SerializePoseForSnapshot(meshPose);
                    else payload["meshError"] = "MeshSpace pose is null";
                }

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(fullPath, json);

                // Update index
                var idx = ReadSnapshotIndex();
                if (!idx.TryGetValue("snapshots", out var rawList) || rawList is not List<object> list)
                {
                    list = new List<object>();
                    idx["snapshots"] = list;
                }
                list.Add(new
                {
                    path = fullPath,
                    label,
                    meshName = mesh.NodeName ?? "",
                    space,
                    timestamp = DateTime.Now.ToString("o"),
                });
                WriteSnapshotIndex(idx);

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    path = fullPath,
                    label,
                    meshName = mesh.NodeName ?? "",
                    space,
                    boneCount = localPose?.Transforms.Count ?? meshPose?.Transforms.Count ?? 0,
                    timestamp = DateTime.Now.ToString("o"),
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message });
            }
        }

        private static string MakeSafeFileName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "x";
            var arr = s.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                if (char.IsLetterOrDigit(arr[i]) || arr[i] == '_' || arr[i] == '-') continue;
                arr[i] = '_';
            }
            var r = new string(arr);
            if (r.Length > 80) r = r.Substring(0, 80);
            return r;
        }

        private static List<Dictionary<string, object>> SerializePoseForSnapshot(IRuntimePose pose)
        {
            var list = new List<Dictionary<string, object>>(pose.Descs.Count);
            for (int i = 0; i < pose.Descs.Count; i++)
            {
                var desc = pose.Descs[i];
                var t = pose.Transforms[i];
                list.Add(SerializeBone(desc, t));
            }
            return list;
        }

        [Bricks.AIGC.TtMCPTool("list_pose_snapshots",
            "List all pose snapshots recorded so far, optionally filtered by mesh name.",
            returnDescription: "{total, snapshots: [{path, label, meshName, space, timestamp}]}")]
        public static string ListPoseSnapshots(
            [Bricks.AIGC.TtMCPParameter("Optional mesh name filter (case-insensitive substring)")] string meshNameFilter = "")
        {
            try
            {
                LogToolCall("list_pose_snapshots", $"meshNameFilter={meshNameFilter}");
                var idx = ReadSnapshotIndex();
                if (!idx.TryGetValue("snapshots", out var rawList) || rawList is not List<object> list)
                {
                    return JsonSerializer.Serialize(new { total = 0, snapshots = Array.Empty<object>() });
                }
                var result = new List<Dictionary<string, object>>();
                foreach (var entry in list)
                {
                    if (entry is not Dictionary<string, object> dict) continue;
                    var mn = dict.TryGetValue("meshName", out var mnObj) ? mnObj?.ToString() ?? "" : "";
                    if (!string.IsNullOrEmpty(meshNameFilter) &&
                        mn.IndexOf(meshNameFilter, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }
                    result.Add(dict);
                }
                return JsonSerializer.Serialize(new { total = result.Count, snapshots = result });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("compare_pose_snapshots",
            "Compare two pose snapshot JSON files bone-by-bone. Reports matched count, missing bones, max position delta, max quaternion angle, and per-bone deltas that exceed the threshold.",
            returnDescription: "{matched, missingInA, missingInB, maxPosDelta, maxQuatAngleDeg, perBone: [{name, posDelta, quatAngle, exceedsThreshold}]}")]
        public static string ComparePoseSnapshots(
            [Bricks.AIGC.TtMCPParameter("Absolute path to snapshot A (from list_pose_snapshots)")] string snapshotPathA = "",
            [Bricks.AIGC.TtMCPParameter("Absolute path to snapshot B (from list_pose_snapshots)")] string snapshotPathB = "",
            [Bricks.AIGC.TtMCPParameter("Position delta threshold; bones with larger deltas are reported. Default 0.001")] double threshold = 0.001)
        {
            try
            {
                if (string.IsNullOrEmpty(snapshotPathA) || string.IsNullOrEmpty(snapshotPathB))
                    return JsonSerializer.Serialize(new { error = "snapshotPathA and snapshotPathB are required" });

                LogToolCall("compare_pose_snapshots", $"A={snapshotPathA}, B={snapshotPathB}, threshold={threshold}");
                var poseA = LoadPoseFromSnapshotFile(snapshotPathA, out var errA);
                var poseB = LoadPoseFromSnapshotFile(snapshotPathB, out var errB);
                if (poseA == null)
                    return JsonSerializer.Serialize(new { error = $"Failed to load A: {errA}" });
                if (poseB == null)
                    return JsonSerializer.Serialize(new { error = $"Failed to load B: {errB}" });

                var mapA = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
                foreach (var b in poseA) mapA[b.TryGetValue("name", out var n) ? n?.ToString() ?? "" : ""] = b;
                var mapB = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
                foreach (var b in poseB) mapB[b.TryGetValue("name", out var n) ? n?.ToString() ?? "" : ""] = b;

                var allNames = new HashSet<string>(mapA.Keys, StringComparer.OrdinalIgnoreCase);
                allNames.UnionWith(mapB.Keys);
                var matched = 0;
                var missingInA = new List<string>();
                var missingInB = new List<string>();
                var perBone = new List<Dictionary<string, object>>();
                double maxPosDelta = 0;
                double maxQuatAngleDeg = 0;
                foreach (var name in allNames)
                {
                    var hasA = mapA.TryGetValue(name, out var bA);
                    var hasB = mapB.TryGetValue(name, out var bB);
                    if (!hasA) { missingInA.Add(name); continue; }
                    if (!hasB) { missingInB.Add(name); continue; }
                    matched++;
                    var posA = ToVec3(bA["pos"]);
                    var posB = ToVec3(bB["pos"]);
                    var quatA = ToVec4(bA["quat"]);
                    var quatB = ToVec4(bB["quat"]);
                    var dPos = new Vector3((float)(posA[0] - posB[0]), (float)(posA[1] - posB[1]), (float)(posA[2] - posB[2]));
                    var posDelta = dPos.Length();
                    if (posDelta > maxPosDelta) maxPosDelta = posDelta;
                    var angle = QuaternionAngleDegrees(quatA, quatB);
                    if (angle > maxQuatAngleDeg) maxQuatAngleDeg = angle;
                    if (posDelta > threshold || angle > threshold * 90.0) // 0.001 → 0.09 deg, treat quat in deg for user readability
                    {
                        perBone.Add(new Dictionary<string, object>
                        {
                            ["name"] = name,
                            ["posDelta"] = posDelta,
                            ["quatAngle"] = angle,
                            ["exceedsThreshold"] = true,
                        });
                    }
                }
                return JsonSerializer.Serialize(new
                {
                    matched,
                    missingInA,
                    missingInB,
                    maxPosDelta,
                    maxQuatAngleDeg,
                    perBoneCount = perBone.Count,
                    perBone,
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private static List<Dictionary<string, object>> LoadPoseFromSnapshotFile(string path, out string error)
        {
            error = null;
            try
            {
                if (!File.Exists(path)) { error = "file not found"; return null; }
                var text = File.ReadAllText(path);
                var doc = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(text);
                if (doc == null) { error = "empty document"; return null; }
                if (doc.TryGetValue("local", out var localEl) && localEl.ValueKind == JsonValueKind.Array)
                    return JsonElementToList(localEl);
                if (doc.TryGetValue("mesh", out var meshEl) && meshEl.ValueKind == JsonValueKind.Array)
                    return JsonElementToList(meshEl);
                error = "no 'local' or 'mesh' array in snapshot";
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        private static List<Dictionary<string, object>> JsonElementToList(JsonElement arr)
        {
            var result = new List<Dictionary<string, object>>();
            foreach (var el in arr.EnumerateArray())
            {
                if (el.ValueKind != JsonValueKind.Object) continue;
                var dict = new Dictionary<string, object>();
                foreach (var prop in el.EnumerateObject())
                {
                    dict[prop.Name] = JsonElementToObject(prop.Value);
                }
                result.Add(dict);
            }
            return result;
        }

        private static object JsonElementToObject(JsonElement el)
        {
            switch (el.ValueKind)
            {
                case JsonValueKind.String: return el.GetString();
                case JsonValueKind.Number: return el.GetDouble();
                case JsonValueKind.True: return true;
                case JsonValueKind.False: return false;
                case JsonValueKind.Null: return null;
                case JsonValueKind.Array:
                    var arr = new List<object>();
                    foreach (var x in el.EnumerateArray()) arr.Add(JsonElementToObject(x));
                    return arr;
                case JsonValueKind.Object:
                    var d = new Dictionary<string, object>();
                    foreach (var p in el.EnumerateObject()) d[p.Name] = JsonElementToObject(p.Value);
                    return d;
                default: return el.GetRawText();
            }
        }

        private static double[] ToVec3(object o)
        {
            if (o is object[] arr && arr.Length >= 3)
            {
                return new[] { Convert.ToDouble(arr[0]), Convert.ToDouble(arr[1]), Convert.ToDouble(arr[2]) };
            }
            if (o is JsonElement je && je.ValueKind == JsonValueKind.Array)
            {
                var en = je.EnumerateArray();
                var v0 = en.MoveNext() ? en.Current.GetDouble() : 0;
                var v1 = en.MoveNext() ? en.Current.GetDouble() : 0;
                var v2 = en.MoveNext() ? en.Current.GetDouble() : 0;
                return new[] { v0, v1, v2 };
            }
            if (o is List<object> list && list.Count >= 3)
            {
                return new[] { Convert.ToDouble(list[0]), Convert.ToDouble(list[1]), Convert.ToDouble(list[2]) };
            }
            return new[] { 0.0, 0.0, 0.0 };
        }

        private static double[] ToVec4(object o)
        {
            if (o is object[] arr && arr.Length >= 4)
            {
                return new[] { Convert.ToDouble(arr[0]), Convert.ToDouble(arr[1]), Convert.ToDouble(arr[2]), Convert.ToDouble(arr[3]) };
            }
            if (o is JsonElement je && je.ValueKind == JsonValueKind.Array)
            {
                var en = je.EnumerateArray();
                var v0 = en.MoveNext() ? en.Current.GetDouble() : 0;
                var v1 = en.MoveNext() ? en.Current.GetDouble() : 0;
                var v2 = en.MoveNext() ? en.Current.GetDouble() : 0;
                var v3 = en.MoveNext() ? en.Current.GetDouble() : 0;
                return new[] { v0, v1, v2, v3 };
            }
            if (o is List<object> list && list.Count >= 4)
            {
                return new[] { Convert.ToDouble(list[0]), Convert.ToDouble(list[1]), Convert.ToDouble(list[2]), Convert.ToDouble(list[3]) };
            }
            return new[] { 0.0, 0.0, 0.0, 1.0 };
        }

        private static double QuaternionAngleDegrees(double[] a, double[] b)
        {
            // Compute angle between two quaternions via dot product.
            // qA·qB = cos(theta/2) for unit quaternions
            var dot = a[0] * b[0] + a[1] * b[1] + a[2] * b[2] + a[3] * b[3];
            dot = System.Math.Min(1.0, System.Math.Max(-1.0, System.Math.Abs(dot)));
            return 2.0 * System.Math.Acos(dot) * 180.0 / System.Math.PI;
        }

        // =================================================================
        //  Section 5: Animation asset status
        // =================================================================

        [Bricks.AIGC.TtMCPTool("list_skeleton_assets",
            "List all .skt skeleton assets in the engine's asset manager.",
            returnDescription: "{total, returned, assets: [string]}")]
        public static string ListSkeletonAssets(
            [Bricks.AIGC.TtMCPParameter("Substring filter on asset name")] string filter = "",
            [Bricks.AIGC.TtMCPParameter("Maximum number of assets to return")] double maxCount = 200)
        {
            try
            {
                int limit = System.Math.Max(1, (int)maxCount);
                LogToolCall("list_skeleton_assets", $"filter={filter}, maxCount={limit}");

                var result = new List<string>();
                int total = 0;
                foreach (var a in TtEngine.Instance.AssetMetaManager.Assets.Values)
                {
                    if (a == null) continue;
                    var ext = a.TypeExt;
                    if (string.IsNullOrEmpty(ext)) continue;
                    if (!ext.Equals(".skt", StringComparison.OrdinalIgnoreCase)) continue;

                    var name = a.AssetName?.Name ?? a.GetAssetName().Name;
                    if (!string.IsNullOrEmpty(filter) &&
                        name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }
                    total++;
                    if (result.Count < limit)
                        result.Add(a.AssetName?.ToString() ?? name);
                }
                return JsonSerializer.Serialize(new { total, returned = result.Count, assets = result });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("get_skeleton_info",
            "Inspect a skeleton asset: bone count, root bone name, list of limbs with name/parentName/index/init translation. If the engine has no standalone .skeleton assets, the tool tries to fall back to the skeleton embedded in the named mesh.",
            returnDescription: "{found, source: 'skeletonAsset'|'meshFallback'|null, boneCount, rootBoneName, limbs: [{name, parent, index, initPos}]}")]
        public static string GetSkeletonInfo(
            [Bricks.AIGC.TtMCPParameter("Skeleton asset name (e.g. 'xxx.skeleton') OR a mesh node name used as fallback when no standalone skeleton exists.")] string skeletonAssetName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(skeletonAssetName))
                    return JsonSerializer.Serialize(new { found = false, error = "skeletonAssetName is required" });

                LogToolCall("get_skeleton_info", $"name={skeletonAssetName}");

                TtSkinSkeleton sk = null;
                string source = null;

                // 1) Try as .skt skeleton asset (TtSkeletonAsset.AssetExt)
                try
                {
                    var rn = RName.GetRName(skeletonAssetName);
                    var meta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(rn);
                    if (meta != null && meta.TypeExt != null &&
                        meta.TypeExt.Equals(".skt", StringComparison.OrdinalIgnoreCase))
                    {
                        var task = TtEngine.Instance.AnimationModule.SkeletonAssetManager.GetSkeletonAsset(rn);
                        var asset = task.GetResultUntilCompleted();
                        if (asset != null) { sk = asset.Skeleton; source = "skeletonAsset"; }
                    }
                }
                catch { }

                // 2) Fallback: read skeleton from the named mesh's MaterialMesh
                if (sk == null)
                {
                    var world = GetActiveWorld();
                    if (world != null)
                    {
                        var mesh = FindMeshNodeByName(world, skeletonAssetName);
                        if (mesh?.RenderMesh?.MaterialMesh != null)
                        {
                            try
                            {
                                var skAssetTask = mesh.RenderMesh.MaterialMesh.GetSkeletonAsset();
                                var sklAsset = skAssetTask.GetResultUntilCompleted();
                                sk = sklAsset?.Skeleton;
                                if (sk != null) source = "meshFallback";
                            }
                            catch { }
                        }
                    }
                }

                if (sk == null)
                    return JsonSerializer.Serialize(new { found = false, error = $"Could not find skeleton '{skeletonAssetName}' as .skeleton asset or as fallback from a mesh" });

                var limbs = new List<Dictionary<string, object>>();
                for (int i = 0; i < sk.Limbs.Count; i++)
                {
                    var limb = sk.Limbs[i];
                    if (limb?.Desc == null) continue;
                    var desc = limb.Desc;
                    double tx = 0, ty = 0, tz = 0;
                    try
                    {
                        var im = desc.InitMatrix;
                        tx = im.M41; ty = im.M42; tz = im.M43;
                    }
                    catch { }
                    limbs.Add(new Dictionary<string, object>
                    {
                        ["name"] = desc.Name ?? "",
                        ["parent"] = desc.ParentName ?? "",
                        ["index"] = (int)desc.NameHash,
                        ["initPos"] = new[] { tx, ty, tz },
                    });
                }
                return JsonSerializer.Serialize(new
                {
                    found = true,
                    source,
                    boneCount = limbs.Count,
                    rootBoneName = sk.Root?.Desc?.Name ?? "",
                    limbs,
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("get_animation_clip_info",
            "Load a .animclip asset and report its name, sample rate, duration, notify count, embedded chunk name, and preview mesh name.",
            returnDescription: "{found, assetName, sampleRate, duration, notifyCount, loop, chunkName, previewMeshName}")]
        public static string GetAnimationClipInfo(
            [Bricks.AIGC.TtMCPParameter("Animation clip asset name (e.g. 'xxx.animclip')")] string clipAssetName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(clipAssetName))
                    return JsonSerializer.Serialize(new { found = false, error = "clipAssetName is required" });

                LogToolCall("get_animation_clip_info", $"clip={clipAssetName}");

                TtAnimationClip clip = null;
                try
                {
                    var rn = RName.GetRName(clipAssetName);
                    var task = TtEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(rn);
                    clip = task.GetResultUntilCompleted();
                }
                catch (Exception ex)
                {
                    return JsonSerializer.Serialize(new { found = false, error = $"Failed to load clip: {ex.Message}" });
                }
                if (clip == null)
                    return JsonSerializer.Serialize(new { found = false, error = "Clip not found" });

                return JsonSerializer.Serialize(new
                {
                    found = true,
                    assetName = clip.AssetName?.ToString() ?? "",
                    sampleRate = clip.SampleRate,
                    duration = clip.Duration,
                    notifyCount = clip.Notifies?.Count ?? 0,
                    loop = true, // player always loops via modulo
                    chunkName = clip.AnimationChunkName?.ToString() ?? "",
                    previewMeshName = clip.PreviewMeshName?.ToString() ?? "",
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        // =================================================================
        //  Section 6: Macross-driven animation (TtDesignMacrossNode) + Kawaii
        //  These handle scenes where animation is driven by a DesignMacross
        //  BlendTree instead of standalone TtSkeletonAnimPlayNode nodes.
        // =================================================================

        #region Macross helpers

        private static List<TtNode> CollectDesignMacrossNodes(GamePlay.TtWorld world)
        {
            var list = new List<TtNode>();
            world?.Root?.IterateNodes((n, _) =>
            {
                if (n != null && n.GetType().Name == "TtDesignMacrossNode")
                    list.Add(n);
                return true;
            }, null);
            return list;
        }

        /// <summary> Get the TtDesignMacrossBase object hosted by a TtDesignMacrossNode. </summary>
        private static object GetDesignMacrossObject(TtNode dmNode)
        {
            var getter = SafeGetProperty(dmNode, "MacrossGetter");
            if (getter == null) return null;
            return SafeInvoke(getter, "Get");
        }

        private static bool IsKawaiiBlendTreeNode(object o)
        {
            var t = o?.GetType();
            while (t != null)
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition().Name.StartsWith("TtLocalSpaceBlendTree_KawaiiPhysics"))
                    return true;
                t = t.BaseType;
            }
            return false;
        }

        /// <summary> Find the FinalBlendTree property value on a DesignMacross object. </summary>
        private static object FindFinalBlendTree(object macross)
        {
            if (macross == null) return null;
            foreach (var p in macross.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.CanRead) continue;
                var bt = p.PropertyType;
                while (bt != null)
                {
                    if (bt.Name.Contains("FinalBlendTree"))
                    {
                        object v = null;
                        try { v = p.GetValue(macross); } catch { }
                        if (v != null) return v;
                        break;
                    }
                    bt = bt.BaseType;
                }
            }
            return null;
        }

        /// <summary> Walk the FromNode chain from a blend-tree node (root first). </summary>
        private static List<object> WalkFromNodeChain(object start, int maxDepth)
        {
            var list = new List<object>();
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            var cur = start;
            int d = 0;
            while (cur != null && d < maxDepth)
            {
                if (!visited.Add(cur)) break;
                list.Add(cur);
                cur = SafeGetProperty(cur, "FromNode");
                d++;
            }
            return list;
        }

        /// <summary> Locate the Kawaii blend-tree node within a DesignMacross object. </summary>
        private static object FindKawaiiNode(object macross, int maxDepth = 16)
        {
            var final = FindFinalBlendTree(macross);
            if (final != null)
            {
                foreach (var node in WalkFromNodeChain(final, maxDepth))
                    if (IsKawaiiBlendTreeNode(node)) return node;
            }
            return null;
        }

        private static float[] Vec3ToArray(Vector3 v) => new float[] { v.X, v.Y, v.Z };

        /// <summary> Serialize an FKawaiiPhySettings struct (read via reflection, fields or properties). </summary>
        private static Dictionary<string, object> SerializePhySettings(object phy)
        {
            return new Dictionary<string, object>
            {
                ["stiffness"] = SafeGetField(phy, "Stiffness") as float? ?? 0f,
                ["damping"] = SafeGetField(phy, "Damping") as float? ?? 0f,
                ["worldDampingLocation"] = SafeGetField(phy, "WorldDampingLocation") as float? ?? 0f,
                ["worldDampingRotation"] = SafeGetField(phy, "WorldDampingRotation") as float? ?? 0f,
                ["limitAngle"] = SafeGetField(phy, "LimitAngle") as float? ?? 0f,
                ["radius"] = SafeGetField(phy, "Radius") as float? ?? 0f,
                ["windCoefficient"] = SafeGetField(phy, "WindCoefficient") as float? ?? 0f,
                ["dragCoefficient"] = SafeGetField(phy, "DragCoefficient") as float? ?? 0f,
                ["maxFrameDisplacement"] = SafeGetField(phy, "MaxFrameDisplacement") as float? ?? 0f,
            };
        }

        /// <summary> Locate the DesignMacross node + object whose parent mesh (or itself) matches meshName. </summary>
        private static bool TryMatchMacross(GamePlay.TtWorld world, string meshName, out TtNode matched, out object macross)
        {
            matched = null; macross = null;
            foreach (var dmNode in CollectDesignMacrossNodes(world))
            {
                var mesh = FindParentMeshNode(dmNode);
                bool nameOk = string.IsNullOrEmpty(meshName)
                    || (mesh?.NodeName?.IndexOf(meshName, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (dmNode.NodeName?.IndexOf(meshName, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!nameOk) continue;
                var mc = GetDesignMacrossObject(dmNode);
                if (mc != null) { matched = dmNode; macross = mc; return true; }
            }
            return false;
        }

        #endregion

        [Bricks.AIGC.TtMCPTool("list_macross_anim_nodes",
            "List all DesignMacross-driven animation nodes (TtDesignMacrossNode) in the active world. Use this for scenes where animation runs through a DesignMacross BlendTree (e.g. Kawaii physics) instead of TtSkeletonAnimPlayNode.",
            returnDescription: "{total, nodes: [{nodeName, parentMeshName, designMacross, macrossType, isInitialized, hasFinalBlendTree, hasKawaii, hasRuntimePose}]}")]
        public static string ListMacrossAnimNodes()
        {
            try
            {
                LogToolCall("list_macross_anim_nodes", "");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { error = "No active world available." });

                var nodes = CollectDesignMacrossNodes(world);
                var outList = new List<Dictionary<string, object>>();
                foreach (var dmNode in nodes)
                {
                    var mesh = FindParentMeshNode(dmNode);
                    var macross = GetDesignMacrossObject(dmNode);
                    var final = macross != null ? FindFinalBlendTree(macross) : null;
                    var kawaii = macross != null ? FindKawaiiNode(macross) : null;
                    outList.Add(new Dictionary<string, object>
                    {
                        ["nodeName"] = dmNode.NodeName ?? "",
                        ["parentMeshName"] = mesh?.NodeName ?? "",
                        ["designMacross"] = (SafeGetProperty(dmNode, "DesignMacross"))?.ToString() ?? "",
                        ["macrossType"] = macross?.GetType().Name ?? "",
                        ["isInitialized"] = macross != null && (SafeGetProperty(macross, "IsInitialized") as bool? ?? false),
                        ["hasFinalBlendTree"] = final != null,
                        ["hasKawaii"] = kawaii != null,
                        ["hasRuntimePose"] = mesh != null && SafeGetProperty(mesh, "RuntimePose") != null,
                    });
                }
                return JsonSerializer.Serialize(new { total = outList.Count, nodes = outList });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("get_macross_kawaii_state",
            "Inspect the Kawaii physics blend-tree node inside a DesignMacross-driven mesh. Reports chain setups (bones + physics settings), live simulation particle positions, alpha, elapseSecond and NaN/static diagnostics. This is the primary tool for debugging hair/bone-chain physics.",
            returnDescription: "{found, nodeName, macrossType, isInitialized, alpha, elapseSecond, chainSetups:[{name,rootBone,rootIndex,endBone,endIndex,stiffness,damping,radius,limitAngle,gravity,wind,drag}], runtime:{chainCount,rodCount,chains:[{index,particleCount,particles:[{boneIndex,pos}]}]}, diagnostics:{anyNaN,allZero,note}}")]
        public static string GetMacrossKawaiiState(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name (or DesignMacross node name). Empty picks the first one found.")] string meshName = "")
        {
            try
            {
                LogToolCall("get_macross_kawaii_state", $"meshName={meshName}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world available." });

                // locate DesignMacross node whose parent mesh (or itself) matches meshName
                if (!TryMatchMacross(world, meshName, out var matched, out var macross))
                    return JsonSerializer.Serialize(new { found = false, error = "No DesignMacross-driven node matched. Try list_macross_anim_nodes." });

                var kawaii = FindKawaiiNode(macross);
                if (kawaii == null)
                    return JsonSerializer.Serialize(new { found = false, macrossType = macross.GetType().Name, error = "No Kawaii physics node in this macross blend tree." });

                // command desc (alpha / elapseSecond)
                var cmdDesc = SafeGetProperty(kawaii, "CommandDesc");
                float alpha = cmdDesc != null ? (SafeGetProperty(cmdDesc, "Alpha") as float? ?? 0f) : 0f;
                float elapse = cmdDesc != null ? (SafeGetProperty(cmdDesc, "ElapseSecond") as float? ?? 0f) : 0f;

                var comp = SafeGetProperty(kawaii, "KawaiiComponent") as EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiPhysicsComponent;
                bool isInit = comp?.IsInitialized ?? false;

                // component-level override settings (these are what actually take effect
                // at runtime when ApplyComponentPhysicsSettings=true, overriding per-chain values)
                bool applyComponentSettings = SafeGetProperty(comp, "ApplyComponentPhysicsSettings") as bool? ?? false;
                var componentPhy = SafeGetProperty(comp, "PhysicsSettings");
                var componentPhySettings = componentPhy != null ? SerializePhySettings(componentPhy) : null;

                // chain setups (config). NOTE: when applyComponentSettings is true the engine
                // overwrites each setup's PhysicsSettings with the component defaults at Initialize.
                var setupList = new List<Dictionary<string, object>>();
                if (SafeGetProperty(kawaii, "ChainSetups") is System.Collections.IEnumerable setups)
                {
                    foreach (var s in setups)
                    {
                        var root = SafeGetProperty(s, "RootBoneIndex");
                        var end = SafeGetProperty(s, "EndBoneIndex");
                        var phy = SafeGetProperty(s, "PhysicsSettings");
                        var entry = new Dictionary<string, object>
                        {
                            ["name"] = SafeGetProperty(s, "Name")?.ToString() ?? "",
                            ["rootBone"] = SafeGetProperty(root, "Name")?.ToString() ?? "",
                            ["rootIndex"] = SafeGetProperty(root, "Index") as int? ?? -1,
                            ["endBone"] = SafeGetProperty(end, "Name")?.ToString() ?? "",
                            ["endIndex"] = SafeGetProperty(end, "Index") as int? ?? -1,
                            ["tailBoneLength"] = SafeGetProperty(s, "TailBoneLength") as float? ?? 0f,
                            ["constrainBoneLength"] = SafeGetProperty(s, "ConstrainBoneLength") as bool? ?? false,
                            ["rootCollision"] = SafeGetProperty(s, "RootCollision") as bool? ?? false,
                            ["lodThreshold"] = SafeGetProperty(s, "LODThreshold") as int? ?? -1,
                            ["physicsSettings"] = SerializePhySettings(phy),
                        };
                        setupList.Add(entry);
                    }
                }

                // rod setups
                var rodList = new List<Dictionary<string, object>>();
                if (SafeGetProperty(kawaii, "RodSetups") is System.Collections.IEnumerable rods)
                {
                    foreach (var r in rods)
                    {
                        var root = SafeGetProperty(r, "RootBoneIndex");
                        var end = SafeGetProperty(r, "EndBoneIndex");
                        rodList.Add(new Dictionary<string, object>
                        {
                            ["name"] = SafeGetProperty(r, "Name")?.ToString() ?? "",
                            ["rootBone"] = SafeGetProperty(root, "Name")?.ToString() ?? "",
                            ["endBone"] = SafeGetProperty(end, "Name")?.ToString() ?? "",
                            ["physicsSettings"] = SerializePhySettings(SafeGetProperty(r, "PhysicsSettings")),
                        });
                    }
                }

                // live simulation particles
                var chains = new List<Dictionary<string, object>>();
                int chainCount = 0, rodCount = 0;
                bool anyNaN = false, allZero = true;
                var ctx = comp?.Context;
                if (ctx != null)
                {
                    try { chainCount = ctx.ChainCount; } catch { }
                    try { rodCount = ctx.RodCount; } catch { }
                    for (int c = 0; c < chainCount; c++)
                    {
                        int pc = 0;
                        try { pc = ctx.GetChainParticleCount(c); } catch { }
                        var parts = new List<Dictionary<string, object>>();
                        for (int p = 0; p < pc; p++)
                        {
                            var pos = ctx.GetChainParticlePosition(c, p);
                            int boneIdx = ctx.GetChainParticleBoneIndex(c, p);
                            if (float.IsNaN(pos.X) || float.IsNaN(pos.Y) || float.IsNaN(pos.Z) ||
                                float.IsInfinity(pos.X) || float.IsInfinity(pos.Y) || float.IsInfinity(pos.Z))
                                anyNaN = true;
                            if (pos.X != 0 || pos.Y != 0 || pos.Z != 0) allZero = false;
                            parts.Add(new Dictionary<string, object>
                            {
                                ["boneIndex"] = boneIdx,
                                ["pos"] = Vec3ToArray(pos),
                            });
                        }
                        chains.Add(new Dictionary<string, object>
                        {
                            ["index"] = c,
                            ["particleCount"] = pc,
                            ["particles"] = parts,
                        });
                    }
                }

                string note = "";
                if (!isInit) note = "KawaiiComponent not initialized.";
                else if (chainCount == 0 && rodCount == 0) note = "No chains/rods built.";
                else if (anyNaN) note = "NaN/Inf detected in particle positions.";
                else if (allZero) note = "All particle positions are zero (simulation likely not running or not built).";
                else if (alpha <= 0f) note = "Alpha<=0: Kawaii result is not blended into the output pose.";
                else if (applyComponentSettings) note = "ApplyComponentPhysicsSettings=true: per-chain physicsSettings are OVERRIDDEN by componentPhysicsSettings at runtime; edit component settings (or disable this flag) to change behaviour.";

                return JsonSerializer.Serialize(new
                {
                    found = true,
                    nodeName = matched?.NodeName ?? "",
                    macrossType = macross.GetType().Name,
                    kawaiiType = kawaii.GetType().Name,
                    isInitialized = isInit,
                    alpha,
                    elapseSecond = elapse,
                    applyComponentPhysicsSettings = applyComponentSettings,
                    componentPhysicsSettings = componentPhySettings,
                    chainSetupCount = setupList.Count,
                    chainSetups = setupList,
                    rodSetupCount = rodList.Count,
                    rodSetups = rodList,
                    runtime = new { chainCount, rodCount, chains },
                    diagnostics = new { anyNaN, allZero, note },
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { found = false, error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("get_macross_blend_tree_dump",
            "Dump the DesignMacross FinalBlendTree FromNode chain for a mesh, listing each blend-tree node type in evaluation order (root -> leaf). Useful to confirm where the Kawaii node sits relative to the state machine / pose output.",
            returnDescription: "{found, nodeName, macrossType, chain:[{depth, type, isKawaii}]}")]
        public static string GetMacrossBlendTreeDump(
            [Bricks.AIGC.TtMCPParameter("Partial mesh / DesignMacross node name. Empty picks the first.")] string meshName = "",
            [Bricks.AIGC.TtMCPParameter("Max chain depth to walk")] double maxDepth = 16)
        {
            try
            {
                LogToolCall("get_macross_blend_tree_dump", $"meshName={meshName}, maxDepth={maxDepth}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world available." });

                TtNode matched = null; object macross = null;
                foreach (var dmNode in CollectDesignMacrossNodes(world))
                {
                    var mesh = FindParentMeshNode(dmNode);
                    bool nameOk = string.IsNullOrEmpty(meshName)
                        || (mesh?.NodeName?.IndexOf(meshName, StringComparison.OrdinalIgnoreCase) >= 0)
                        || (dmNode.NodeName?.IndexOf(meshName, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!nameOk) continue;
                    var mc = GetDesignMacrossObject(dmNode);
                    if (mc != null) { matched = dmNode; macross = mc; break; }
                }
                if (macross == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No DesignMacross-driven node matched." });

                var final = FindFinalBlendTree(macross);
                if (final == null)
                    return JsonSerializer.Serialize(new { found = false, macrossType = macross.GetType().Name, error = "No FinalBlendTree found." });

                var chain = new List<Dictionary<string, object>>();
                int depth = 0;
                foreach (var node in WalkFromNodeChain(final, (int)maxDepth))
                {
                    chain.Add(new Dictionary<string, object>
                    {
                        ["depth"] = depth++,
                        ["type"] = node.GetType().Name,
                        ["isKawaii"] = IsKawaiiBlendTreeNode(node),
                    });
                }
                return JsonSerializer.Serialize(new
                {
                    found = true,
                    nodeName = matched?.NodeName ?? "",
                    macrossType = macross.GetType().Name,
                    chain,
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { found = false, error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("get_mesh_runtime_pose",
            "Read the final runtime pose directly from a TtMeshNode (works for BOTH Macross-driven and player-driven meshes, since both write into MeshNode.RuntimePose). Space 'local' reads RuntimePose, 'mesh' reads MeshSpaceRuntimePose.",
            returnDescription: "{found, meshName, space, boneCount, hasNaN, bones:[{name,parent,index,pos,quat,scale}]}")]
        public static string GetMeshRuntimePose(
            [Bricks.AIGC.TtMCPParameter("Partial mesh node name")] string meshName = "",
            [Bricks.AIGC.TtMCPParameter("'local' or 'mesh'")] string space = "local",
            [Bricks.AIGC.TtMCPParameter("Optional case-insensitive bone name substring filter")] string boneNameFilter = "")
        {
            try
            {
                LogToolCall("get_mesh_runtime_pose", $"meshName={meshName}, space={space}");
                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world available." });

                var mesh = FindMeshNodeByName(world, meshName);
                if (mesh == null)
                    return JsonSerializer.Serialize(new { found = false, error = $"Mesh node '{meshName}' not found." });

                bool meshSpace = space != null && space.Equals("mesh", StringComparison.OrdinalIgnoreCase);
                var pose = meshSpace ? SafeGetProperty(mesh, "MeshSpaceRuntimePose") : SafeGetProperty(mesh, "RuntimePose");
                if (pose == null)
                    return JsonSerializer.Serialize(new { found = false, meshName = mesh.NodeName, space, error = meshSpace ? "MeshSpaceRuntimePose is null (mesh may not have ticked yet)." : "RuntimePose is null (mesh not animated / not skinned)." });

                var descs = SafeGetProperty(pose, "Descs") as System.Collections.IList;
                var transforms = SafeGetProperty(pose, "Transforms") as System.Collections.IList;
                if (descs == null || transforms == null)
                    return JsonSerializer.Serialize(new { found = false, meshName = mesh.NodeName, space, error = "Pose has no Descs/Transforms." });

                int count = Math.Min(descs.Count, transforms.Count);
                bool hasNaN = false;
                var bones = new List<Dictionary<string, object>>();
                for (int i = 0; i < count; i++)
                {
                    var desc = descs[i] as EngineNS.Animation.SkeletonAnimation.Skeleton.Limb.ILimbDesc;
                    if (!string.IsNullOrEmpty(boneNameFilter) &&
                        (desc?.Name?.IndexOf(boneNameFilter, StringComparison.OrdinalIgnoreCase) ?? -1) < 0)
                        continue;
                    var t = (FTransform)transforms[i];
                    if (HasNaNOrInf(t)) hasNaN = true;
                    bones.Add(SerializeBone(desc, t));
                }
                return JsonSerializer.Serialize(new
                {
                    found = true,
                    meshName = mesh.NodeName,
                    space = meshSpace ? "mesh" : "local",
                    boneCount = count,
                    returned = bones.Count,
                    hasNaN,
                    bones,
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { found = false, error = ex.Message });
            }
        }

        [Bricks.AIGC.TtMCPTool("sample_kawaii_motion",
            "Sample the Kawaii chain particle positions multiple times over a short window and report per-particle max displacement. Definitively answers whether the simulation is actually moving (dynamic) or frozen/settled. Runs on a background thread so the engine keeps ticking between samples.",
            returnDescription: "{found, meshName, samples, intervalMs, chainCount, isMoving, maxDisplacement, perParticle:[{chain,particle,boneIndex,maxDelta,first,last}], note}")]
        public static string SampleKawaiiMotion(
            [Bricks.AIGC.TtMCPParameter("Partial mesh / DesignMacross node name. Empty picks the first.")] string meshName = "",
            [Bricks.AIGC.TtMCPParameter("Number of samples to take (2-30)")] double samples = 6,
            [Bricks.AIGC.TtMCPParameter("Milliseconds between samples (10-500)")] double intervalMs = 50,
            [Bricks.AIGC.TtMCPParameter("Displacement magnitude above which the chain is considered 'moving'")] double movingThreshold = 0.0001)
        {
            try
            {
                LogToolCall("sample_kawaii_motion", $"meshName={meshName}, samples={samples}, intervalMs={intervalMs}");
                int sampleCount = Math.Max(2, Math.Min(30, (int)samples));
                int interval = Math.Max(10, Math.Min(500, (int)intervalMs));

                var world = GetActiveWorld();
                if (world == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No active world available." });
                if (!TryMatchMacross(world, meshName, out var matched, out var macross))
                    return JsonSerializer.Serialize(new { found = false, error = "No DesignMacross-driven node matched." });
                var kawaii = FindKawaiiNode(macross);
                var comp = kawaii != null ? SafeGetProperty(kawaii, "KawaiiComponent") as EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiPhysicsComponent : null;
                var ctx = comp?.Context;
                if (ctx == null)
                    return JsonSerializer.Serialize(new { found = false, error = "No Kawaii context (node not found or not initialized)." });

                int chainCount = 0;
                try { chainCount = ctx.ChainCount; } catch { }
                if (chainCount == 0)
                    return JsonSerializer.Serialize(new { found = true, meshName = matched?.NodeName ?? "", chainCount = 0, isMoving = false, note = "No chains built; nothing to sample." });

                // build particle key list
                var keys = new List<(int chain, int particle, int boneIndex)>();
                for (int c = 0; c < chainCount; c++)
                {
                    int pc = 0; try { pc = ctx.GetChainParticleCount(c); } catch { }
                    for (int p = 0; p < pc; p++)
                    {
                        int bi = -1; try { bi = ctx.GetChainParticleBoneIndex(c, p); } catch { }
                        keys.Add((c, p, bi));
                    }
                }

                var first = new Vector3[keys.Count];
                var last = new Vector3[keys.Count];
                var maxDelta = new float[keys.Count];
                var prev = new Vector3[keys.Count];
                for (int s = 0; s < sampleCount; s++)
                {
                    for (int k = 0; k < keys.Count; k++)
                    {
                        Vector3 pos;
                        try { pos = ctx.GetChainParticlePosition(keys[k].chain, keys[k].particle); }
                        catch { pos = Vector3.Zero; }
                        if (s == 0) { first[k] = pos; prev[k] = pos; }
                        else
                        {
                            var d = pos - prev[k];
                            float mag = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z);
                            if (mag > maxDelta[k]) maxDelta[k] = mag;
                            prev[k] = pos;
                        }
                        last[k] = pos;
                    }
                    if (s < sampleCount - 1)
                        System.Threading.Thread.Sleep(interval);
                }

                float globalMax = 0f;
                var per = new List<Dictionary<string, object>>();
                for (int k = 0; k < keys.Count; k++)
                {
                    if (maxDelta[k] > globalMax) globalMax = maxDelta[k];
                    per.Add(new Dictionary<string, object>
                    {
                        ["chain"] = keys[k].chain,
                        ["particle"] = keys[k].particle,
                        ["boneIndex"] = keys[k].boneIndex,
                        ["maxDelta"] = maxDelta[k],
                        ["first"] = Vec3ToArray(first[k]),
                        ["last"] = Vec3ToArray(last[k]),
                    });
                }

                bool isMoving = globalMax > (float)movingThreshold;
                string note = isMoving
                    ? "Chain particles are moving between frames (dynamic simulation active)."
                    : "Chain particles are frozen across all samples (settled at rest, or gravity/excitation absent, or sim not integrating).";

                return JsonSerializer.Serialize(new
                {
                    found = true,
                    meshName = matched?.NodeName ?? "",
                    samples = sampleCount,
                    intervalMs = interval,
                    chainCount,
                    isMoving,
                    maxDisplacement = globalMax,
                    perParticle = per,
                    note,
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { found = false, error = ex.Message });
            }
        }
    }
}
