using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EngineNS.Plugins.MCPServer
{
    /// <summary>
    /// 视口通路: 打开资产编辑器 + 把视口画面存成 png。
    ///
    /// 这两个凑在一起才让"改完渲染代码自己确认效果"变成闭环。在它们之前, 引擎侧改动的视觉结果
    /// 是彻底看不见的 —— 材质/shader 改完只能靠读资产文件和日志推断, 真正"画面对不对"只能让人
    /// 肉眼看一眼再口头告诉我。capture_renderdoc_frame 虽然能看 GPU 侧, 但抓帧只反映当前正在
    /// 渲染的内容, 而且 .rdc 得转手 renderdoc MCP 才能读, 拿不到"整张画面长什么样"。
    ///
    /// 分工:
    ///   open_asset_editor  - 把资产在主编辑器里打开 (.scene 会开场景编辑器并加载世界)
    ///   capture_screenshot - 存下视口渲染结果或整个编辑器窗口
    /// </summary>
    public partial class TtMCPServerPlugin
    {
        #region open_asset_editor

        [Bricks.AIGC.TtMCPTool("open_asset_editor",
            "Opens an asset in the main editor, exactly like double clicking it in the Content Browser: " +
            "the asset type's [TtAssetEditor] attribute decides which editor to open. Pass a .scene to " +
            "load a level into a scene editor viewport - that is the prerequisite for capture_screenshot " +
            "with source='viewport', and for every terrain_* tool. Waits until the editor actually " +
            "appears, so a successful return means the asset finished loading. Opening an already open " +
            "asset just brings it to the front.",
            returnDescription: "{opened: boolean, alreadyOpen: boolean, asset: string, editorType: string, " +
            "window: string - ImGui window name, sceneLoaded: boolean - Only meaningful for scenes, " +
            "openEditors: string[] - Every asset currently open in an editor, error: string - Present " +
            "only on failure}")]
        public static string OpenAssetEditor(
            [Bricks.AIGC.TtMCPParameter("Asset RName, e.g. 'survivor/maps/firstlevel/scene_firstlevel.scene'")] string asset,
            [Bricks.AIGC.TtMCPParameter("How long to wait for the editor to finish opening, in ms. Big scenes need a lot")] double timeoutMs = 120000)
        {
            if (string.IsNullOrWhiteSpace(asset))
                return FailJson("asset is empty");

            LogToolCall("open_asset_editor", $"asset={asset}, timeoutMs={timeoutMs}");

            var rn = RName.GetRName(asset.Trim());
            int timeout = Math.Max(5000, (int)timeoutMs);

            string setupError = null;
            string editorTypeName = null;
            bool alreadyOpen = false;

            var armed = TtMainThreadDispatcher.Invoke(() =>
            {
                var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
                if (mainEditor == null)
                {
                    setupError = "the running SlateApplication is not TtMainEditorApplication; " +
                        "asset editors only exist inside the main editor";
                    return;
                }

                var meta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(rn);
                if (meta == null)
                {
                    setupError = $"no .ameta registered for '{rn}'";
                    return;
                }
                // 派发逻辑和内容浏览器双击完全一致 (ContentBrowser.cs 的 IsMouseDoubleClicked 分支):
                // ameta.TypeStr -> 资产类型 -> [TtAssetEditor] -> 编辑器类型。不要在这里硬编码
                // .scene => TtSceneEditor, 否则每加一种资产类型都得回来改。
                var type = Rtti.TtTypeDesc.TypeOf(meta.TypeStr)?.SystemType;
                if (type == null)
                {
                    setupError = $"asset type '{meta.TypeStr}' is not registered in rtti";
                    return;
                }
                var attrs = type.GetCustomAttributes(typeof(Editor.TtAssetEditorAttribute), false);
                var editorType = attrs.Length > 0 ? (attrs[0] as Editor.TtAssetEditorAttribute)?.EditorType : null;
                if (editorType == null)
                {
                    setupError = $"asset type '{type.Name}' has no [TtAssetEditor] attribute, " +
                        "there is no editor to open for it";
                    return;
                }
                editorTypeName = editorType.Name;

                var existing = FindOpenedEditor(mainEditor, rn);
                if (existing != null)
                {
                    mainEditor.AssetEditorManager.CurrentActiveEditor = existing;
                    alreadyOpen = true;
                    return;
                }

                // TryOpenEditor 是 async 的, 而且内部 BlockOperation 会把主循环占住直到资产加载完。
                // 这里只发起, 结果靠下面轮询 OpenedEditors 判定 —— 在主线程回调里同步等它必然超时。
                Editor.TtAssetEditorManager.TryOpenEditor(editorType, rn, null, true).AddWaitTask();
            });

            if (armed == false)
                return FailJson("timed out waiting for the engine main thread to start opening the asset");
            if (setupError != null)
                return FailJson(setupError);

            if (alreadyOpen)
                return DescribeOpenedEditor(rn, editorTypeName, true);

            var deadline = DateTime.Now.AddMilliseconds(timeout);
            while (DateTime.Now < deadline)
            {
                System.Threading.Thread.Sleep(250);

                bool found = false;
                try
                {
                    // 加载期间主线程被 BlockOperation 占着, Invoke 大概率超时 —— 那不代表失败,
                    // 只是还没轮到我们的回调, 继续等就行。
                    TtMainThreadDispatcher.Invoke(() =>
                    {
                        var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
                        var editor = mainEditor == null ? null : FindOpenedEditor(mainEditor, rn);
                        if (editor != null)
                        {
                            mainEditor.AssetEditorManager.CurrentActiveEditor = editor;
                            found = true;
                        }
                    }, 2000);
                }
                catch (TtMainThreadInvokeException)
                {
                }
                if (found)
                    return DescribeOpenedEditor(rn, editorTypeName, false);
            }

            return FailJson($"'{rn}' did not show up in the editor within {timeout} ms. " +
                "It may still be loading (check get_recent_logs), or opening failed - " +
                "a failed OpenEditor logs 'AssetEditor ... open failed'.");
        }

        /// <summary> 在已打开的编辑器里按资产名找。必须在主线程上调用。 </summary>
        private static Editor.IAssetEditor FindOpenedEditor(Editor.TtMainEditorApplication mainEditor, RName rn)
        {
            foreach (var i in mainEditor.AssetEditorManager.OpenedEditors)
            {
                if (i != null && i.AssetName == rn)
                    return i;
            }
            return null;
        }

        private static string DescribeOpenedEditor(RName rn, string editorTypeName, bool alreadyOpen)
        {
            string window = null;
            bool sceneLoaded = false;
            var openEditors = new List<string>();

            TtMainThreadDispatcher.Invoke(() =>
            {
                var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
                if (mainEditor == null)
                    return;
                foreach (var i in mainEditor.AssetEditorManager.OpenedEditors)
                {
                    if (i?.AssetName != null)
                        openEditors.Add(i.AssetName.ToString());
                }
                var editor = FindOpenedEditor(mainEditor, rn);
                if (editor == null)
                    return;
                window = editor.GetWindowsName();
                var sceneEditor = editor as Editor.Forms.TtSceneEditor;
                if (sceneEditor != null)
                    sceneLoaded = sceneEditor.Scene != null;
            });

            return JsonSerializer.Serialize(new
            {
                opened = true,
                alreadyOpen,
                asset = rn.ToString(),
                editorType = editorTypeName,
                window,
                sceneLoaded,
                openEditors,
            });
        }

        #endregion

        #region capture_screenshot

        [Bricks.AIGC.TtMCPTool("capture_screenshot",
            "Saves what the engine is currently rendering to a png file and returns its absolute path, " +
            "which can be read back directly as an image. source='viewport' grabs a viewport's final " +
            "render target - a clean picture of the 3D scene with no editor UI on top, which is what you " +
            "want to check whether something actually renders; it needs a viewport to exist, so call " +
            "open_asset_editor on a .scene first. source='window' grabs the whole editor window " +
            "including all ImGui panels. Unlike capture_renderdoc_frame this needs no RenderDoc and " +
            "gives you a picture instead of a .rdc to disassemble.",
            returnDescription: "{captured: boolean, file: string - Absolute png path, source: string, " +
            "viewport: string - Title of the viewport that was grabbed, width: number, height: number, " +
            "availableViewports: string[] - Titles of every live viewport, handy when the filter matched " +
            "nothing, error: string - Present only on failure}")]
        public static unsafe string CaptureScreenshot(
            [Bricks.AIGC.TtMCPParameter("'viewport' for the clean 3D render target, 'window' for the whole editor window")] string source = "viewport",
            [Bricks.AIGC.TtMCPParameter("Tag appended to the file name, e.g. 'hilltree_after_migration'")] string tag = "mcp",
            [Bricks.AIGC.TtMCPParameter("With source='viewport', pick the viewport whose title contains this. Empty picks the one being drawn, e.g. 'Scene:'")] string viewportFilter = "",
            [Bricks.AIGC.TtMCPParameter("Downscale so neither side exceeds this many pixels. 0 keeps the native resolution")] double maxSize = 0,
            [Bricks.AIGC.TtMCPParameter("Let the engine run this many frames first, so freshly loaded content has time to appear")] double warmupFrames = 3)
        {
            var mode = (source ?? "viewport").Trim().ToLowerInvariant();
            if (mode != "viewport" && mode != "window")
                return FailJson($"unknown source '{source}', expected 'viewport' or 'window'");

            LogToolCall("capture_screenshot",
                $"source={mode}, tag={tag}, viewportFilter={viewportFilter}, maxSize={maxSize}");

            // tag 直接进文件名, 挡掉路径分隔符之类。
            tag = SanitizeCaptureTag(tag);

            var warmup = Math.Max(0, (int)warmupFrames);
            if (warmup > 0 && TtMainThreadDispatcher.WaitFrames(warmup, 15000) == false)
                return FailJson("the engine is not ticking (window minimized? main thread blocked?), " +
                    "nothing would be rendered to capture");

            string setupError = null;
            string viewportTitle = null;
            uint width = 0, height = 0;
            var availableViewports = new List<string>();
            NxRHI.ITexture tex = default;

            // 纹理句柄要在主线程上取: 视口列表和 RenderPolicy 都可能被主线程改。取到的 ITexture
            // 是个指针包装, 拿出来之后的回读走 RenderQueue, 不再碰这些托管结构。
            var located = TtMainThreadDispatcher.Invoke(() =>
            {
                if (mode == "window")
                {
                    // 不能直接用 SlateApplication.NativeWindow: 多窗口模式下那是个 10x10 的占位窗,
                    // 界面画在 ImGui 派生的 platform 窗口上, 抓它只会得到一张 10x10 的图。
                    var window = TtEngine.Instance.NativeWindowManager.GetLargestRenderableWindow()
                        as Graphics.Pipeline.TtPresentWindow;
                    if (window == null)
                        window = TtEngine.Instance.GfxDevice.SlateApplication?.NativeWindow;
                    var swapChain = window?.SwapChain;
                    if (swapChain == null)
                    {
                        setupError = "no renderable window with a swap chain (all windows hidden or minimized?)";
                        return;
                    }
                    viewportTitle = window.WindowName;
                    // 抓当前后台缓冲而不是固定的 0 号: FLIP_DISCARD 下 0 号大多数帧都不是刚画完那张。
                    tex = swapChain.mCoreObject.GetBackBuffer(swapChain.mCoreObject.GetCurrentBackBuffer());
                    return;
                }

                Graphics.Pipeline.TtViewportSlate picked = null;
                foreach (var weak in TtEngine.Instance.ViewportSlateManager.Viewports)
                {
                    Graphics.Pipeline.TtViewportSlate slate;
                    if (weak.TryGetTarget(out slate) == false || slate == null)
                        continue;
                    var title = slate.Title ?? "";
                    availableViewports.Add(title);
                    if (string.IsNullOrEmpty(viewportFilter) == false)
                    {
                        if (title.IndexOf(viewportFilter, StringComparison.OrdinalIgnoreCase) >= 0 && picked == null)
                            picked = slate;
                    }
                    else if (picked == null && slate.IsDrawing)
                    {
                        // 没给 filter 就取正在绘制的那个: 停靠在后台页签里的视口不会被绘制,
                        // 抓它只会拿到一张过期的图。
                        picked = slate;
                    }
                }
                if (picked == null)
                {
                    setupError = string.IsNullOrEmpty(viewportFilter)
                        ? "no viewport is currently being drawn; open an asset editor with a viewport first " +
                          "(open_asset_editor on a .scene), or pass viewportFilter to target one by title"
                        : $"no live viewport title contains '{viewportFilter}'";
                    return;
                }

                var srv = picked.RenderPolicy?.GetFinalShowRSV();
                if (srv == null)
                {
                    setupError = $"viewport '{picked.Title}' has no final render target yet " +
                        "(its RenderPolicy is still initializing)";
                    return;
                }
                viewportTitle = picked.Title;
                tex = srv.GetTexture();
            });

            if (located == false)
                return FailJson("timed out waiting for the engine main thread to locate the render target");
            if (setupError != null)
                return JsonSerializer.Serialize(new { error = setupError, availableViewports });

            width = tex.Desc.Width;
            height = tex.Desc.Height;

            var dir = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cache) + "screenshots/";
            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{mode}_{tag}.png";
            var absFile = System.IO.Path.GetFullPath(dir + fileName);

            // 回读故意留在 MCP 工作线程上: 拷贝命令排进 RenderQueue 由渲染线程按序执行, 这里只是
            // 阻塞等 fence。要是搬到主线程去等, 就是逻辑线程等渲染线程, 一旦渲染线程反过来要
            // 同步主线程就死锁了。
            bool ok;
            try
            {
                ok = Editor.TtSnapshot.SaveTextureToFile(tex, absFile, Math.Max(0, (int)maxSize));
            }
            catch (Exception ex)
            {
                return FailJson($"{ex.GetType().Name} while reading back the render target: {ex.Message}");
            }
            if (ok == false)
                return FailJson("render target readback produced no pixels");

            return JsonSerializer.Serialize(new
            {
                captured = true,
                file = absFile,
                source = mode,
                viewport = viewportTitle,
                width,
                height,
                availableViewports,
            });
        }

        #endregion
    }
}
