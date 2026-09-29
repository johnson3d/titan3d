using System;
using System.Collections.Generic;
using System.Linq;
using EngineNS;

namespace LocalizationTool
{
    // 多国化工具主界面（IRootForm）。工作流：扫描 → 勾选/编辑译文 → 保存目录 → 预览改写 → 应用改写。
    // 仅在用户显式点击“应用改写”时才修改源码；重复扫描/应用因 scanner 的已本地化检测而幂等。
    public class TtLocalizationEditor : IRootForm
    {
        TtToolConfig mConfig;
        TtSourceScanner mScanner;
        TtCatalogStore mStore;
        TtSourceRewriter mRewriter = new TtSourceRewriter();

        TtScanResult mScan;
        List<TtTranslationEntry> mEntries = new List<TtTranslationEntry>();
        readonly Dictionary<string, string> mFileHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 界面状态。
        string mFilterText = "";
        bool mOnlySelected = false;
        bool mOnlyMissing = false;
        bool mHideRisk = false;
        int mActiveCultureIndex = 0;
        string mNewCulture = "";
        string mStatus = "Ready. Click Scan to begin.";
        TtRewriteResult mPreview;
        bool mShowPreview = false;

        public TtLocalizationEditor()
        {
            TtEngine.RootFormManager.RegRootForm(this);
            Visible = true;
        }

        public async EngineNS.Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            var engineRoot = TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            var gameRoot = TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            // Engine 根为 enginecontent；扫描根 CSharpCode 位于其上一级仓库根，故取 EngineSource 的父目录更稳妥。
            var sourceRoot = TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.EngineSource);
            var repoRoot = ResolveRepoRoot(sourceRoot, engineRoot);
            mConfig = TtToolConfig.LoadOrCreate(engineRoot, gameRoot);
            mScanner = new TtSourceScanner(repoRoot, mConfig);
            mStore = new TtCatalogStore(repoRoot, engineRoot, gameRoot);
            return true;
        }

        // 扫描根、目录根都以“仓库根”为基准。EngineSource 通常为 <repo>/CSharpCode，取其父目录。
        static string ResolveRepoRoot(string sourceRoot, string engineRoot)
        {
            var s = (sourceRoot ?? "").Replace('\\', '/').TrimEnd('/');
            if (s.EndsWith("/CSharpCode", StringComparison.OrdinalIgnoreCase))
                return s.Substring(0, s.Length - "/CSharpCode".Length) + "/";
            // 回退：enginecontent 的父目录。
            var e = (engineRoot ?? "").Replace('\\', '/').TrimEnd('/');
            var idx = e.LastIndexOf('/');
            return idx > 0 ? e.Substring(0, idx) + "/" : e + "/";
        }

        public void Dispose()
        {
            TtEngine.RootFormManager.UnregRootForm(this);
        }

        public bool Visible { get; set; }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var size = new Vector2(1100, 780);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);

            var result = EngineNS.EGui.UIProxy.DockProxy.BeginMainForm("LocalizationTool", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                DockId = ImGuiAPI.GetWindowDockID();
                DrawToolbar();
                ImGuiAPI.Separator();
                DrawCultureBar();
                ImGuiAPI.Separator();
                DrawFilters();
                ImGuiAPI.Separator();
                DrawTable();
                if (mShowPreview)
                {
                    ImGuiAPI.Separator();
                    DrawPreviewPanel();
                }
                ImGuiAPI.Separator();
                ImGuiAPI.TextWrapped(mStatus);
            }
            EngineNS.EGui.UIProxy.DockProxy.EndMainForm(result);
        }

        #region toolbar
        unsafe void DrawToolbar()
        {
            if (ImGuiAPI.Button("Scan", in Vector2.Zero))
                DoScan();
            ImGuiAPI.SameLine(0, 8);
            if (ImGuiAPI.Button("Rescan", in Vector2.Zero))
                DoScan();
            ImGuiAPI.SameLine(0, 16);
            if (ImGuiAPI.Button("Save Catalog", in Vector2.Zero))
                DoSaveCatalog();
            ImGuiAPI.SameLine(0, 8);
            if (ImGuiAPI.Button("Preview Rewrite", in Vector2.Zero))
                DoPreview();
            ImGuiAPI.SameLine(0, 8);
            if (ImGuiAPI.Button("Apply Rewrite", in Vector2.Zero))
                DoApply();

            if (mScan != null)
            {
                ImGuiAPI.SameLine(0, 16);
                var selectedOcc = mScan.Occurrences.Count(o => o.Selected && o.Ignored == false && o.CanRewrite);
                ImGuiAPI.Text($"Entries: {mEntries.Count(e => e.Obsolete == false)}   Occurrences: {mScan.Occurrences.Count}   Selected(rewritable): {selectedOcc}   AlreadyLocalized: {mScan.AlreadyLocalizedCount}");
            }
        }
        #endregion

        #region culture bar
        unsafe void DrawCultureBar()
        {
            var cultures = mStore != null ? mStore.TranslatableCultures() : new List<string>();
            ImGuiAPI.Text("Active language:");
            ImGuiAPI.SameLine(0, 8);
            if (cultures.Count == 0)
            {
                ImGuiAPI.TextDisabled("(no translatable culture; add one)");
            }
            else
            {
                if (mActiveCultureIndex >= cultures.Count)
                    mActiveCultureIndex = 0;
                ImGuiAPI.SetNextItemWidth(160);
                ImGuiAPI.Combo("##activeCulture", ref mActiveCultureIndex, cultures, cultures.Count, 8);
            }

            ImGuiAPI.SameLine(0, 24);
            ImGuiAPI.Text("Add culture:");
            ImGuiAPI.SameLine(0, 6);
            ImGuiAPI.SetNextItemWidth(120);
            ImGuiAPI.InputText("##newCulture", ref mNewCulture);
            ImGuiAPI.SameLine(0, 6);
            if (ImGuiAPI.Button("Add", in Vector2.Zero))
            {
                if (string.IsNullOrWhiteSpace(mNewCulture) == false && mStore != null)
                {
                    mStore.AddCulture(mNewCulture.Trim());
                    mStatus = $"Added culture {mNewCulture.Trim()}.";
                    mNewCulture = "";
                    RebuildEntries();
                }
            }
            if (cultures.Count > 0)
            {
                ImGuiAPI.SameLine(0, 12);
                if (ImGuiAPI.Button("Remove Active", in Vector2.Zero) && mActiveCultureIndex < cultures.Count)
                {
                    var c = cultures[mActiveCultureIndex];
                    mStore.RemoveCulture(c);
                    mStatus = $"Removed culture {c}.";
                    RebuildEntries();
                }
            }
        }

        string ActiveCulture()
        {
            var cultures = mStore != null ? mStore.TranslatableCultures() : new List<string>();
            if (cultures.Count == 0)
                return null;
            if (mActiveCultureIndex >= cultures.Count)
                mActiveCultureIndex = 0;
            return cultures[mActiveCultureIndex];
        }
        #endregion

        #region filters
        unsafe void DrawFilters()
        {
            ImGuiAPI.Text("Filter:");
            ImGuiAPI.SameLine(0, 6);
            ImGuiAPI.SetNextItemWidth(280);
            ImGuiAPI.InputText("##locfilter", ref mFilterText);
            ImGuiAPI.SameLine(0, 16);
            ImGuiAPI.Checkbox("Selected only", ref mOnlySelected);
            ImGuiAPI.SameLine(0, 12);
            ImGuiAPI.Checkbox("Missing translation", ref mOnlyMissing);
            ImGuiAPI.SameLine(0, 12);
            ImGuiAPI.Checkbox("Hide risk", ref mHideRisk);
        }

        bool PassFilter(TtTranslationEntry entry, string activeCulture)
        {
            if (entry.Obsolete)
                return false;
            if (string.IsNullOrEmpty(mFilterText) == false)
            {
                if (entry.Source.IndexOf(mFilterText, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }
            bool anySelected = entry.Occurrences.Any(o => o.Selected && o.Ignored == false);
            if (mOnlySelected && anySelected == false)
                return false;
            if (mHideRisk && entry.Occurrences.All(o => o.IsRisk))
                return false;
            if (mOnlyMissing)
            {
                if (activeCulture == null)
                    return false;
                var has = entry.Translations.TryGetValue(activeCulture, out var t) && string.IsNullOrEmpty(t) == false;
                if (has)
                    return false;
            }
            return true;
        }
        #endregion

        #region table
        unsafe void DrawTable()
        {
            var active = ActiveCulture();
            var childSize = new Vector2(0, mShowPreview ? 360 : 520);
            if (ImGuiAPI.BeginChild("LocList", in childSize, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_HorizontalScrollbar))
            {
                ImGuiAPI.Columns(6, "LocColumns", true);
                ImGuiAPI.Text("Use");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Text("Source");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Text(active != null ? $"Translation ({active})" : "Translation");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Text("Category");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Text("Occ");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Text("Kind");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Separator();

                int rowId = 0;
                foreach (var entry in mEntries)
                {
                    if (PassFilter(entry, active) == false)
                        continue;
                    rowId++;
                    DrawEntryRow(entry, active, rowId);
                }
                ImGuiAPI.Columns(1, null, false);
            }
            ImGuiAPI.EndChild();
        }

        unsafe void DrawEntryRow(TtTranslationEntry entry, string activeCulture, int rowId)
        {
            // Use 列：聚合勾选（对全部可改写 occurrence 生效）。
            bool anySelected = entry.Occurrences.Any(o => o.Selected && o.Ignored == false);
            bool canAny = entry.Occurrences.Any(o => o.CanRewrite);
            bool sel = anySelected;
            if (canAny)
            {
                if (ImGuiAPI.Checkbox($"##use{rowId}", ref sel))
                {
                    foreach (var o in entry.Occurrences)
                    {
                        if (o.CanRewrite)
                        {
                            o.Selected = sel;
                            o.Ignored = false;
                        }
                    }
                }
            }
            else
            {
                ImGuiAPI.TextDisabled("-");
            }
            ImGuiAPI.NextColumn();

            // Source 列：展开显示各 occurrence（逐位置覆盖）。
            var sourcePreview = entry.Source.Length > 60 ? entry.Source.Substring(0, 57) + "..." : entry.Source;
            bool srcOpen = ImGuiAPI.TreeNodeEx($"{sourcePreview}##src{rowId}", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth);
            // 右键 Source 行：两个菜单选项分别用 VS / VSCode(缺失则 notepad) 跳转到第一个 occurrence。
            if (ImGuiAPI.BeginPopupContextItem($"srcctx{rowId}", ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                bool hasOcc = entry.Occurrences.Count > 0;
                var firstOcc = hasOcc ? entry.Occurrences[0] : null;
                if (ImGuiAPI.MenuItem("Goto in Visual Studio", null, false, hasOcc))
                    GotoSource(firstOcc, false);
                if (ImGuiAPI.MenuItem("Goto in VS Code / Notepad", null, false, hasOcc))
                    GotoSource(firstOcc, true);
                ImGuiAPI.EndPopup();
            }
            if (srcOpen)
            {
                int occId = 0;
                foreach (var o in entry.Occurrences)
                {
                    occId++;
                    bool osel = o.Selected;
                    if (o.CanRewrite)
                    {
                        if (ImGuiAPI.Checkbox($"##occ{rowId}_{occId}", ref osel))
                            o.Selected = osel;
                        ImGuiAPI.SameLine(0, 6);
                    }
                    if (ImGuiAPI.SmallButton($"Goto##g{rowId}_{occId}"))
                        ImGuiAPI.OpenPopup($"gotopop{rowId}_{occId}", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                    if (ImGuiAPI.BeginPopup($"gotopop{rowId}_{occId}", ImGuiWindowFlags_.ImGuiWindowFlags_None))
                    {
                        if (ImGuiAPI.MenuItem($"Goto in Visual Studio##vs{rowId}_{occId}", null, false, true))
                            GotoSource(o, false);
                        if (ImGuiAPI.MenuItem($"Goto in VS Code / Notepad##ext{rowId}_{occId}", null, false, true))
                            GotoSource(o, true);
                        ImGuiAPI.EndPopup();
                    }
                    ImGuiAPI.SameLine(0, 6);
                    var loc = $"{o.RelativeFilePath}:{o.Line}  {o.TypeName}.{o.MemberName}";
                    if (o.CanRewrite == false)
                        ImGuiAPI.TextColored(new Vector4(1.0f, 0.6f, 0.2f, 1.0f), $"[not rewritable] {loc}");
                    else if (o.IsRisk)
                        ImGuiAPI.TextColored(new Vector4(1.0f, 0.8f, 0.3f, 1.0f), $"[risk] {loc}  ({o.InvocationTarget})");
                    else
                        ImGuiAPI.Text($"{loc}  ({o.InvocationTarget})");
                }
                ImGuiAPI.TreePop();
            }
            ImGuiAPI.NextColumn();

            // Translation 列：编辑 active culture 译文。
            if (activeCulture != null)
            {
                entry.Translations.TryGetValue(activeCulture, out var text);
                text ??= "";
                ImGuiAPI.SetNextItemWidth(-1);
                if (ImGuiAPI.InputText($"##tr{rowId}", ref text))
                    entry.Translations[activeCulture] = text;

                if (entry.IsFormat && string.IsNullOrEmpty(text) == false)
                {
                    if (EngineNS.Localization.TtLocalizationSerializer.ValidatePlaceholders(entry.Source, text, out var err) == false)
                        ImGuiAPI.TextColored(new Vector4(1.0f, 0.3f, 0.3f, 1.0f), err);
                }
            }
            else
            {
                ImGuiAPI.TextDisabled("(add culture)");
            }
            ImGuiAPI.NextColumn();

            // Category 列。
            var cat = entry.Occurrences.Count > 0 ? entry.Occurrences[0].Category.ToString() : "";
            ImGuiAPI.Text(cat);
            ImGuiAPI.NextColumn();

            ImGuiAPI.Text(entry.Occurrences.Count.ToString());
            ImGuiAPI.NextColumn();

            var kind = entry.Occurrences.Count > 0 ? entry.Occurrences[0].RewriteKind.ToString() : "";
            if (entry.IsFormat)
                kind = "Format";
            ImGuiAPI.Text(kind);
            ImGuiAPI.NextColumn();
        }
        #endregion

        #region preview panel
        unsafe void DrawPreviewPanel()
        {
            ImGuiAPI.Text("Rewrite Preview");
            ImGuiAPI.SameLine(0, 12);
            if (ImGuiAPI.Button("Close Preview", in Vector2.Zero))
            {
                mShowPreview = false;
                return;
            }
            if (mPreview == null)
                return;

            ImGuiAPI.Text($"Files with changes: {mPreview.Files.Count(f => f.Changed)}   Errors: {mPreview.Errors.Count}");
            var childSize = new Vector2(0, 160);
            if (ImGuiAPI.BeginChild("PreviewList", in childSize, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                foreach (var f in mPreview.Files)
                {
                    if (string.IsNullOrEmpty(f.Error) == false)
                        ImGuiAPI.TextColored(new Vector4(1.0f, 0.3f, 0.3f, 1.0f), $"{f.RelativeFilePath}: {f.Error}");
                    else if (f.Changed)
                        ImGuiAPI.Text($"{f.RelativeFilePath}: {f.ReplacementCount} replacement(s)");
                }
            }
            ImGuiAPI.EndChild();
        }
        #endregion

        #region actions
        void DoScan()
        {
            try
            {
                mShowPreview = false;
                mPreview = null;
                mScan = mScanner.Scan();
                mStore.ApplyScanState(mScan);

                mFileHashes.Clear();
                foreach (var f in mScan.ScannedFiles)
                {
                    try { mFileHashes[f] = TtSourceScanner.ComputeFileHash(f); }
                    catch { }
                }
                RebuildEntries();
                mStatus = $"Scanned {mScan.ScannedFiles.Count} files, {mScan.Occurrences.Count} occurrences, {mEntries.Count(e => e.Obsolete == false)} unique sources. Errors: {mScan.Errors.Count}.";
            }
            catch (Exception ex)
            {
                mStatus = $"Scan failed: {ex.Message}";
            }
        }

        void RebuildEntries()
        {
            if (mScan == null)
            {
                mEntries = new List<TtTranslationEntry>();
                return;
            }
            mEntries = mStore.BuildEntries(mScan);
        }

        // 通过 VisualStudioPlugin 跳转到指定 occurrence 的源码位置（文件:行）。
        // useExternal=false 用 Visual Studio；true 用 VSCode（缺失则 notepad）。
        void GotoSource(TtStringOccurrence occ, bool useExternal)
        {
            if (occ == null)
                return;
            try
            {
                var plugin = EngineNS.Bricks.DevIDE.TtDevIDEPlugin.FindDevIDEPlugin();
                if (plugin == null)
                {
                    mStatus = "DevIDE plugin (VisualStudioPlugin) not found.";
                    return;
                }
                if (useExternal)
                {
                    bool ok = plugin.OpenFileAtLineExternal(occ.AbsoluteFilePath, occ.Line);
                    mStatus = ok
                        ? $"Goto (VS Code/Notepad) {occ.RelativeFilePath}:{occ.Line}"
                        : "Open in VS Code/Notepad failed.";
                }
                else
                {
                    bool ok = plugin.OpenFileAtLine(occ.AbsoluteFilePath, occ.Line);
                    mStatus = ok
                        ? $"Goto (Visual Studio) {occ.RelativeFilePath}:{occ.Line}"
                        : "Open in Visual Studio failed (no running instance?).";
                }
            }
            catch (Exception ex)
            {
                mStatus = $"Goto failed: {ex.Message}";
            }
        }

        void DoSaveCatalog()
        {
            if (mStore == null)
                return;
            try
            {
                var errors = mStore.SaveAll(mEntries);
                if (mScan != null)
                    mStore.SaveScanState(mScan.Occurrences);
                mStatus = errors.Count == 0
                    ? "Catalog saved."
                    : $"Catalog saved with {errors.Count} rejected entry(ies): {string.Join(" | ", errors.Take(3))}";
            }
            catch (Exception ex)
            {
                mStatus = $"Save failed: {ex.Message}";
            }
        }

        void DoPreview()
        {
            if (mScan == null)
            {
                mStatus = "Scan first.";
                return;
            }
            try
            {
                mPreview = mRewriter.BuildPreview(mScan.Occurrences, mFileHashes);
                mShowPreview = true;
                mStatus = $"Preview: {mPreview.Files.Count(f => f.Changed)} file(s) would change, {mPreview.Errors.Count} error(s).";
            }
            catch (Exception ex)
            {
                mStatus = $"Preview failed: {ex.Message}";
            }
        }

        void DoApply()
        {
            if (mScan == null)
            {
                mStatus = "Scan first.";
                return;
            }
            try
            {
                // 应用前先保存目录，保证被改写字符串在运行时有 catalog 键。
                mStore.SaveAll(mEntries);
                mStore.SaveScanState(mScan.Occurrences);

                var res = mRewriter.Apply(mScan.Occurrences, mFileHashes);
                mPreview = res;
                mShowPreview = true;
                mStatus = res.Errors.Count == 0
                    ? $"Applied {res.AppliedOccurrenceCount} rewrite(s) across {res.AppliedFileCount} file(s). Rescan recommended."
                    : $"Applied {res.AppliedFileCount} file(s) with {res.Errors.Count} error(s). See preview panel.";
            }
            catch (Exception ex)
            {
                mStatus = $"Apply failed: {ex.Message}";
            }
        }
        #endregion
    }
}
