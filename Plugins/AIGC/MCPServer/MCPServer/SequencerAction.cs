using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EngineNS.Plugins.MCPServer
{
    /// <summary>
    /// 序列器诊断通路: 读节点 transform + 读序列结构 + 驱动播放头采样。
    ///
    /// 为什么需要这一组: 序列器的问题几乎都是"值不对"而不是"崩了" —— 关键帧插值退化成阶梯、
    /// 绑定解析到了错节点、求值结果没落到节点上。这三类从截图和日志里都看不出来, 只能拿到
    /// 一串"tick -> 节点实际 transform"的数值才能判定。在它们之前只能靠人肉拖播放头再口头描述。
    ///
    /// 分工:
    ///   get_node_placement  - 读任意打开世界里某个节点的 Position/Scale/Quat
    ///   get_sequence_info   - 读序列的时基/绑定/轨道/每个关键帧 (含 InterpMode 与切线)
    ///   scrub_sequence      - 把播放头扫过一段区间, 每个采样点回读被绑定节点的 transform
    ///
    /// scrub_sequence 走的是编辑器的 ScrubTo, 也就是拖播放头那条路径。这一点很要紧: 若另开
    /// 一条求值路径去采样, 采出来的曲线漂亮也不能说明界面上拖着不跳。
    /// </summary>
    public partial class TtMCPServerPlugin
    {
        #region 共用: 找编辑器 / 遍历节点 / 描述 transform

        /// <summary> 取第一个打开的序列编辑器。必须在主线程上调用。 </summary>
        private static Editor.Forms.TtSequenceEditor FindSequenceEditor()
        {
            var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
            if (mainEditor == null)
                return null;
            foreach (var i in mainEditor.AssetEditorManager.OpenedEditors)
            {
                var seqEditor = i as Editor.Forms.TtSequenceEditor;
                if (seqEditor != null && seqEditor.Sequence != null)
                    return seqEditor;
            }
            return null;
        }

        /// <summary>
        /// 收集所有能拿到的世界根节点, 连带一个人能看懂的来源名。序列编辑器用的是自己的预览
        /// 世界而不是主场景, 所以这里必须把两种都覆盖, 否则查不到被序列驱动的那个节点。
        /// 必须在主线程上调用。
        /// </summary>
        private static void GatherWorldRoots(List<KeyValuePair<string, GamePlay.Scene.TtNode>> result)
        {
            var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
            if (mainEditor == null)
                return;
            foreach (var i in mainEditor.AssetEditorManager.OpenedEditors)
            {
                if (i == null)
                    continue;
                var assetName = i.AssetName == null ? "?" : i.AssetName.ToString();

                var seqEditor = i as Editor.Forms.TtSequenceEditor;
                if (seqEditor != null)
                {
                    var root = seqEditor.PreviewViewport?.World?.Root;
                    if (root != null)
                        result.Add(new KeyValuePair<string, GamePlay.Scene.TtNode>($"SequenceEditor:{assetName}", root));
                    continue;
                }
                var sceneEditor = i as Editor.Forms.TtSceneEditor;
                if (sceneEditor != null)
                {
                    var root = sceneEditor.Scene as GamePlay.Scene.TtNode;
                    if (root != null)
                        result.Add(new KeyValuePair<string, GamePlay.Scene.TtNode>($"SceneEditor:{assetName}", root));
                }
            }
        }

        /// <summary> 深度优先遍历, 把匹配 filter 的节点收进 result。filter 空则全收。 </summary>
        private static void CollectNodes(GamePlay.Scene.TtNode node, string filter, int limit,
            List<GamePlay.Scene.TtNode> result)
        {
            if (node == null || result.Count >= limit)
                return;
            var name = node.NodeName ?? "";
            if (string.IsNullOrEmpty(filter) ||
                name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result.Add(node);
            }
            for (int i = 0; i < node.Children.Count; ++i)
                CollectNodes(node.Children[i], filter, limit, result);
        }

        /// <summary>
        /// 把节点的 transform 拍成一个可序列化的匿名对象。位置留 6 位小数: 引擎里位置是
        /// double, 直接输出会拖一长串浮点尾巴, 淹掉"值有没有在变"这个真正要看的信息。
        /// </summary>
        private static object DescribePlacement(GamePlay.Scene.TtNode node)
        {
            var placement = node.Placement;
            if (placement == null)
            {
                return new
                {
                    name = node.NodeName,
                    type = node.GetType().Name,
                    error = "node has no Placement",
                };
            }
            var pos = placement.Position;
            var scale = placement.Scale;
            var quat = placement.Quat;
            return new
            {
                name = node.NodeName,
                type = node.GetType().Name,
                nodeId = node.NodeId.ToString(),
                position = new { x = Math.Round(pos.X, 6), y = Math.Round(pos.Y, 6), z = Math.Round(pos.Z, 6) },
                scale = new { x = Math.Round(scale.X, 6), y = Math.Round(scale.Y, 6), z = Math.Round(scale.Z, 6) },
                quat = new
                {
                    x = Math.Round(quat.X, 6),
                    y = Math.Round(quat.Y, 6),
                    z = Math.Round(quat.Z, 6),
                    w = Math.Round(quat.W, 6),
                },
            };
        }

        #endregion

        #region get_node_placement

        [Bricks.AIGC.TtMCPTool("get_node_placement",
            "Reads the Position / Scale / Quat of nodes in every world that is currently open, including " +
            "the sequence editor's own preview world (which is a different world from the main scene - a " +
            "node driven by a sequence lives there, not in the scene editor). Use it to check what a " +
            "value actually became, instead of inferring it from a screenshot. Matching on nodeName is a " +
            "case insensitive substring match; leave it empty to dump the whole tree.",
            returnDescription: "{nodes: [{name, type, nodeId, editor: string - which editor's world it " +
            "came from, position:{x,y,z}, scale:{x,y,z}, quat:{x,y,z,w}}], worlds: string[] - every world " +
            "that was searched, truncated: boolean, error: string - present only on failure}")]
        public static string GetNodePlacement(
            [Bricks.AIGC.TtMCPParameter("Case insensitive substring of the node name, e.g. 'oldwood'. Empty dumps every node")] string nodeName = "",
            [Bricks.AIGC.TtMCPParameter("Stop after this many matches, so an empty filter cannot flood the response")] double limit = 24)
        {
            LogToolCall("get_node_placement", $"nodeName={nodeName}, limit={limit}");

            var filter = (nodeName ?? "").Trim();
            int max = Math.Max(1, (int)limit);

            var described = new List<object>();
            var worlds = new List<string>();
            bool truncated = false;

            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var roots = new List<KeyValuePair<string, GamePlay.Scene.TtNode>>();
                GatherWorldRoots(roots);
                foreach (var pair in roots)
                {
                    worlds.Add(pair.Key);
                    var found = new List<GamePlay.Scene.TtNode>();
                    CollectNodes(pair.Value, filter, max - described.Count, found);
                    for (int i = 0; i < found.Count; ++i)
                    {
                        if (described.Count >= max)
                        {
                            truncated = true;
                            break;
                        }
                        var one = DescribePlacement(found[i]);
                        described.Add(new { editor = pair.Key, placement = one });
                    }
                }
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            if (worlds.Count == 0)
                return FailJson("no open editor exposes a world; open a .scene or a .sequence first");

            return JsonSerializer.Serialize(new { nodes = described, worlds, truncated });
        }

        #endregion

        #region get_sequence_info

        [Bricks.AIGC.TtMCPTool("get_sequence_info",
            "Dumps the structure of the .sequence currently open in a sequence editor: time base, " +
            "playback range, every binding (and whether it still resolves to a live node), every track / " +
            "section, and every keyframe with its value, InterpMode and tangents. This is the way to tell " +
            "'the keys are Constant so the motion steps' apart from 'the keys are Cubic so the data is " +
            "fine and the problem is elsewhere' - neither is visible in a screenshot.",
            returnDescription: "{asset, tickResolution, displayRate, ticksPerFrame, playbackStartTick, " +
            "playbackEndTick, currentTick, bindings: [{displayName, targetNodeId, parentNodeId, " +
            "relativePath, resolved, resolvedNode, tracks: [{trackTypeName, displayName, muted, sections: " +
            "[{type, startTick, endTick, channels: [{name, keyCount, keys: [{tick, seconds, value, " +
            "interp, tangentMode, arrive, leave}]}]}]}]}], error: string - present only on failure}")]
        public static string GetSequenceInfo()
        {
            LogToolCall("get_sequence_info", "");

            string error = null;
            object payload = null;

            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var editor = FindSequenceEditor();
                if (editor == null)
                {
                    error = "no sequence editor is open; open a .sequence with open_asset_editor first";
                    return;
                }
                var sequence = editor.Sequence;
                var bindings = new List<object>();
                for (int bi = 0; bi < sequence.Bindings.Count; ++bi)
                {
                    var binding = sequence.Bindings[bi];
                    if (binding == null)
                        continue;
                    var node = editor.Player?.Resolver?.Resolve(binding);
                    var tracks = new List<object>();
                    for (int ti = 0; ti < binding.Tracks.Count; ++ti)
                    {
                        var track = binding.Tracks[ti];
                        if (track == null)
                            continue;
                        var sections = new List<object>();
                        for (int si = 0; si < track.Sections.Count; ++si)
                        {
                            var section = track.Sections[si];
                            if (section == null)
                                continue;
                            sections.Add(new
                            {
                                type = section.GetType().Name,
                                startTick = section.StartTick,
                                endTick = section.EndTick,
                                channels = DescribeChannels(section, sequence.TickResolution),
                            });
                        }
                        tracks.Add(new
                        {
                            trackTypeName = track.TrackTypeName,
                            displayName = track.DisplayName,
                            muted = track.Muted,
                            sections,
                        });
                    }
                    bindings.Add(new
                    {
                        displayName = binding.DisplayName,
                        targetNodeId = binding.TargetNodeId.ToString(),
                        parentNodeId = binding.ParentNodeId.ToString(),
                        relativePath = binding.RelativePath,
                        resolved = node != null,
                        resolvedNode = node == null ? null : node.NodeName,
                        tracks,
                    });
                }

                payload = new
                {
                    asset = editor.AssetName == null ? null : editor.AssetName.ToString(),
                    tickResolution = sequence.TickResolution.ToString(),
                    displayRate = sequence.DisplayRate.ToString(),
                    ticksPerFrame = sequence.TickResolution.TicksPerFrame(sequence.DisplayRate),
                    playbackStartTick = sequence.PlaybackStartTick,
                    playbackEndTick = sequence.PlaybackEndTick,
                    currentTick = editor.CurrentTick,
                    bindings,
                };
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            if (error != null)
                return FailJson(error);
            return JsonSerializer.Serialize(payload);
        }

        /// <summary>
        /// 把一个 Section 的所有通道连关键帧一起描述出来。通道名走 Section 自己的顺序,
        /// Transform 段的七条顺序是固定的 (见 TtTransformSection.GatherChannels), 所以能直接
        /// 给出人能看懂的名字; 其他 Section 类型退化成下标名。
        /// </summary>
        private static List<object> DescribeChannels(Sequencer.Asset.TtSequenceSection section,
            in Sequencer.TtFrameRate tickResolution)
        {
            string[] transformNames = { "PositionX", "PositionY", "PositionZ", "ScaleX", "ScaleY", "ScaleZ", "Rotation" };
            bool isTransform = section is Sequencer.Asset.TtTransformSection;

            var channels = new List<Sequencer.ISequenceChannel>();
            section.GatherChannels(channels);

            var result = new List<object>();
            for (int ci = 0; ci < channels.Count; ++ci)
            {
                var channel = channels[ci];
                if (channel == null)
                    continue;
                var name = (isTransform && ci < transformNames.Length) ? transformNames[ci] : $"Channel{ci}";

                var keys = new List<object>();
                var scalar = channel as Sequencer.TtScalarChannel;
                for (int ki = 0; ki < channel.KeyCount; ++ki)
                {
                    var tick = channel.GetKeyTime(ki);
                    var seconds = Math.Round(tickResolution.AsSeconds(tick), 6);
                    if (scalar != null)
                    {
                        var key = scalar.GetKey(ki);
                        keys.Add(new
                        {
                            tick,
                            seconds,
                            value = Math.Round(key.Value, 6),
                            interp = key.InterpMode.ToString(),
                            tangentMode = key.TangentMode.ToString(),
                            arrive = Math.Round(key.ArriveTangent, 6),
                            leave = Math.Round(key.LeaveTangent, 6),
                        });
                    }
                    else
                    {
                        // 非标量通道 (四元数 / 资产名) 没有 InterpMode 与切线, 只报时刻
                        keys.Add(new { tick, seconds });
                    }
                }
                result.Add(new
                {
                    name,
                    channelType = channel.GetType().Name,
                    keyCount = channel.KeyCount,
                    keys,
                });
            }
            return result;
        }

        #endregion

        #region scrub_sequence

        [Bricks.AIGC.TtMCPTool("scrub_sequence",
            "Moves the sequence editor's playhead and reports what the bound nodes actually became. " +
            "With sampleCount > 1 it sweeps from startTick to endTick and returns one sample per step, " +
            "all inside a single engine frame - that gives you a clean 'tick -> transform' curve to check " +
            "whether motion interpolates smoothly or steps between keys. It drives the same ScrubTo path " +
            "the user's mouse drag uses, so a smooth curve here really does mean the UI is smooth. " +
            "Leave startTick / endTick at -1 to sweep the whole playback range.",
            returnDescription: "{tickResolution, displayRate, ticksPerFrame, playbackStartTick, " +
            "playbackEndTick, samples: [{requestedTick, tick - what ClampTick actually allowed, seconds, " +
            "nodes: [{name, type, nodeId, position:{x,y,z}, scale:{x,y,z}, quat:{x,y,z,w}}]}], " +
            "finalTick, error: string - present only on failure}")]
        public static string ScrubSequence(
            [Bricks.AIGC.TtMCPParameter("First tick to sample. -1 uses the sequence's PlaybackStartTick")] double startTick = -1,
            [Bricks.AIGC.TtMCPParameter("Last tick to sample. -1 uses the sequence's PlaybackEndTick")] double endTick = -1,
            [Bricks.AIGC.TtMCPParameter("How many evenly spaced samples to take. 1 just sets the playhead to startTick")] double sampleCount = 1,
            [Bricks.AIGC.TtMCPParameter("Put the playhead back where it was when done, instead of leaving it at the last sample")] bool restore = false)
        {
            LogToolCall("scrub_sequence",
                $"startTick={startTick}, endTick={endTick}, sampleCount={sampleCount}, restore={restore}");

            int count = Math.Max(1, (int)sampleCount);
            string error = null;
            object payload = null;

            // 整个扫描留在一个主线程回调里: 分成多次 Invoke 的话中间会插进引擎的 Tick,
            // 而 Tick 里播放中的序列会自己推进播放头, 采出来的就不是我们设的那些 tick 了。
            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var editor = FindSequenceEditor();
                if (editor == null)
                {
                    error = "no sequence editor is open; open a .sequence with open_asset_editor first";
                    return;
                }
                var sequence = editor.Sequence;
                long originalTick = editor.CurrentTick;

                long from = startTick < 0 ? sequence.PlaybackStartTick : (long)startTick;
                long to = endTick < 0 ? sequence.PlaybackEndTick : (long)endTick;
                if (count == 1)
                    to = from;

                var samples = new List<object>();
                for (int i = 0; i < count; ++i)
                {
                    // 端点必须精确落在 from / to 上, 所以用 (count-1) 做分母而不是 count
                    long requested = count == 1
                        ? from
                        : from + (long)((double)(to - from) * i / (count - 1));
                    var actual = editor.ScrubTo(requested);

                    var nodes = new List<object>();
                    for (int bi = 0; bi < sequence.Bindings.Count; ++bi)
                    {
                        var binding = sequence.Bindings[bi];
                        if (binding == null)
                            continue;
                        var node = editor.Player?.Resolver?.Resolve(binding);
                        if (node == null)
                            continue;
                        nodes.Add(DescribePlacement(node));
                    }
                    samples.Add(new
                    {
                        requestedTick = requested,
                        tick = actual,
                        seconds = Math.Round(sequence.TickResolution.AsSeconds(actual), 6),
                        nodes,
                    });
                }

                if (restore)
                    editor.ScrubTo(originalTick);

                payload = new
                {
                    tickResolution = sequence.TickResolution.ToString(),
                    displayRate = sequence.DisplayRate.ToString(),
                    ticksPerFrame = sequence.TickResolution.TicksPerFrame(sequence.DisplayRate),
                    playbackStartTick = sequence.PlaybackStartTick,
                    playbackEndTick = sequence.PlaybackEndTick,
                    samples,
                    finalTick = editor.CurrentTick,
                };
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            if (error != null)
                return FailJson(error);
            return JsonSerializer.Serialize(payload);
        }

        #endregion

        #region merge_sequence_sections

        [Bricks.AIGC.TtMCPTool("merge_sequence_sections",
            "Collapses every track of the open .sequence down to a single section, moving all keyframes " +
            "into it and recomputing Auto tangents. This repairs the one shape a track must have before it " +
            "can interpolate at all: a section holding a single lone keyframe can only evaluate to a " +
            "constant, so N sections of one key each produce a staircase no matter what InterpMode those " +
            "keys carry. Only memory is touched - nothing reaches the asset until the user saves - so it " +
            "is safe to run purely to see whether the values start moving smoothly.",
            returnDescription: "{changed: boolean, tracks: [{binding, track, sectionsBefore, " +
            "sectionsAfter, startTick, endTick, keyCounts: int[] - per channel after the merge, " +
            "skippedChannels: int - channels whose keys could not be moved}], error: string - present " +
            "only on failure}")]
        public static string MergeSequenceSections()
        {
            LogToolCall("merge_sequence_sections", "");

            string error = null;
            object payload = null;

            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var editor = FindSequenceEditor();
                if (editor == null)
                {
                    error = "no sequence editor is open; open a .sequence with open_asset_editor first";
                    return;
                }
                var sequence = editor.Sequence;
                var reports = new List<object>();
                bool changed = false;

                for (int bi = 0; bi < sequence.Bindings.Count; ++bi)
                {
                    var binding = sequence.Bindings[bi];
                    if (binding == null)
                        continue;
                    for (int ti = 0; ti < binding.Tracks.Count; ++ti)
                    {
                        var track = binding.Tracks[ti];
                        if (track == null || track.Sections.Count <= 1)
                            continue;

                        int before = track.Sections.Count;
                        int skipped;
                        var host = track.MergeSectionsIntoFirst(sequence.TickResolution, out skipped);
                        changed = true;

                        var hostChannels = new List<Sequencer.ISequenceChannel>();
                        host.GatherChannels(hostChannels);
                        var counts = new List<int>();
                        for (int ci = 0; ci < hostChannels.Count; ++ci)
                            counts.Add(hostChannels[ci] == null ? 0 : hostChannels[ci].KeyCount);

                        reports.Add(new
                        {
                            binding = binding.DisplayName,
                            track = track.DisplayName,
                            sectionsBefore = before,
                            sectionsAfter = track.Sections.Count,
                            startTick = host.StartTick,
                            endTick = host.EndTick,
                            keyCounts = counts,
                            skippedChannels = skipped,
                        });
                    }
                }

                // 合并后立刻按当前播放头重求一次, 否则节点还停在合并前那一帧的值上,
                // 紧跟着来的采样会读到过期数据。
                editor.ScrubTo(editor.CurrentTick);

                payload = new { changed, tracks = reports };
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            if (error != null)
                return FailJson(error);
            return JsonSerializer.Serialize(payload);
        }

        #endregion

        #region save_sequence

        [Bricks.AIGC.TtMCPTool("save_sequence",
            "Saves the .sequence open in a sequence editor to its asset file, the same as pressing Save in " +
            "the editor. Needed to verify anything that only shows up across a reload - binding " +
            "persistence, tick values not drifting - and to commit a repair done by " +
            "merge_sequence_sections, which otherwise only lives in memory.",
            returnDescription: "{saved: boolean, asset: string, error: string - present only on failure}")]
        public static string SaveSequence()
        {
            LogToolCall("save_sequence", "");

            string error = null;
            string asset = null;

            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                var editor = FindSequenceEditor();
                if (editor == null)
                {
                    error = "no sequence editor is open; open a .sequence with open_asset_editor first";
                    return;
                }
                var sequence = editor.Sequence;
                if (sequence.AssetName == null)
                {
                    error = "the open sequence has no AssetName; it was never saved to disk";
                    return;
                }
                asset = sequence.AssetName.ToString();
                sequence.SaveAssetTo(sequence.AssetName);
            });

            if (ok == false)
                return FailJson("timed out waiting for the engine main thread");
            if (error != null)
                return FailJson(error);
            return JsonSerializer.Serialize(new { saved = true, asset });
        }

        #endregion
    }
}
