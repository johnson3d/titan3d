using System;
using System.Collections.Generic;
using System.Linq;
using EngineNS;
using EngineNS.EGui;
using EngineNS.EGui.Controls;

namespace BrickManager
{
    public class TtBrickManagerEditor : IRootForm
    {
        private List<BrickInfo> allBricks = new List<BrickInfo>();
        private List<BrickInfo> resolvedDependencies = new List<BrickInfo>();
        private BrickDependencyResolver resolver;
        private bool hasResolved = false;
        private string filterText = string.Empty;

        public TtBrickManagerEditor()
        {
            TtEngine.RootFormManager.RegRootForm(this);
            Visible = true;
        }

        ~TtBrickManagerEditor()
        {
        }

        public async EngineNS.Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();

            var engineRoot = TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.EngineSource);
            resolver = new BrickDependencyResolver(engineRoot);
            allBricks = resolver.ScanAllBricks();

            return true;
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
            var size = new Vector2(900, 700);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);

            var result = EngineNS.EGui.UIProxy.DockProxy.BeginMainForm("BrickManager", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                DockId = ImGuiAPI.GetWindowDockID();
                DrawBrickSelector();
                ImGuiAPI.Separator();
                DrawResolvedDependencies();
            }
            EngineNS.EGui.UIProxy.DockProxy.EndMainForm(result);
        }

        private unsafe void DrawBrickSelector()
        {
            ImGuiAPI.Text("Brick Selector");
            ImGuiAPI.Separator();

            // Filter input
            ImGuiAPI.Text("Filter:");
            ImGuiAPI.SameLine(0, 5);
            var filterBuffer = new EngineNS.Support.TtAnyPointer();
            ImGuiAPI.InputText("##filter", ref filterText);

            ImGuiAPI.Spacing();

            // Select All / Deselect All buttons
            if (ImGuiAPI.Button("Select All", in Vector2.Zero))
            {
                foreach (var brick in allBricks)
                {
                    brick.IsSelected = true;
                }
                hasResolved = false;
            }
            ImGuiAPI.SameLine(0, 10);
            if (ImGuiAPI.Button("Deselect All", in Vector2.Zero))
            {
                foreach (var brick in allBricks)
                {
                    brick.IsSelected = false;
                }
                hasResolved = false;
                resolvedDependencies.Clear();
            }
            ImGuiAPI.SameLine(0, 10);
            if (ImGuiAPI.Button("Resolve Dependencies", in Vector2.Zero))
            {
                ResolveDependencies();
            }

            ImGuiAPI.Spacing();

            // Brick list with checkboxes
            var childSize = new Vector2(0, 350);
            if (ImGuiAPI.BeginChild("BrickList", in childSize, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                // Table header
                ImGuiAPI.Columns(3, "BrickColumns", true);
                ImGuiAPI.Text("Select");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Text("Name");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Text("Path");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Separator();

                foreach (var brick in allBricks)
                {
                    // Apply filter
                    if (!string.IsNullOrEmpty(filterText))
                    {
                        if (!brick.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase) &&
                            !brick.FullName.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    var isSelected = brick.IsSelected;
                    if (ImGuiAPI.Checkbox($"##{brick.FullName}", ref isSelected))
                    {
                        brick.IsSelected = isSelected;
                        hasResolved = false;
                    }
                    ImGuiAPI.NextColumn();

                    ImGuiAPI.Text(brick.Name);
                    ImGuiAPI.NextColumn();

                    ImGuiAPI.Text(brick.FullName);
                    ImGuiAPI.NextColumn();
                }

                ImGuiAPI.Columns(1, null, false);
            }
            ImGuiAPI.EndChild();

            // Show selected count
            var selectedCount = allBricks.Count(b => b.IsSelected);
            ImGuiAPI.Text($"Selected: {selectedCount} / {allBricks.Count}");
        }

        private void ResolveDependencies()
        {
            var selectedBricks = allBricks.Where(b => b.IsSelected).ToList();
            if (selectedBricks.Count == 0)
            {
                resolvedDependencies.Clear();
                hasResolved = true;
                return;
            }

            resolvedDependencies = resolver.ResolveAllDependencies(selectedBricks);
            hasResolved = true;
        }

        private unsafe void DrawResolvedDependencies()
        {
            ImGuiAPI.Text("Resolved Dependencies");
            ImGuiAPI.Separator();

            if (!hasResolved)
            {
                ImGuiAPI.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), "Click 'Resolve Dependencies' to analyze.");
                return;
            }

            if (resolvedDependencies.Count == 0)
            {
                ImGuiAPI.TextColored(new Vector4(1.0f, 0.5f, 0.0f, 1.0f), "No bricks selected.");
                return;
            }

            // Summary
            var directCount = allBricks.Count(b => b.IsSelected);
            var transitiveCount = resolvedDependencies.Count - directCount;
            ImGuiAPI.Text($"Direct: {directCount}, Transitive: {transitiveCount}, Total: {resolvedDependencies.Count}");
            ImGuiAPI.Spacing();

            // Collect all nugets
            var allNugets = new Dictionary<string, string>();
            foreach (var brick in resolvedDependencies)
            {
                foreach (var nuget in brick.Nugets)
                {
                    allNugets[nuget.Name] = nuget.Version;
                }
            }

            if (allNugets.Count > 0)
            {
                if (ImGuiAPI.TreeNode("Required NuGet Packages"))
                {
                    foreach (var kvp in allNugets)
                    {
                        ImGuiAPI.BulletText($"{kvp.Key} ({kvp.Value})");
                    }
                    ImGuiAPI.TreePop();
                }
                ImGuiAPI.Spacing();
            }

            // Dependency list
            var childSize = new Vector2(0, 200);
            if (ImGuiAPI.BeginChild("ResolvedList", in childSize, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiAPI.Columns(3, "ResolvedColumns", true);
                ImGuiAPI.Text("Name");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Text("Path");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Text("Type");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Separator();

                foreach (var brick in resolvedDependencies)
                {
                    var isDirect = brick.IsSelected;

                    if (isDirect)
                    {
                        ImGuiAPI.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), brick.Name);
                    }
                    else
                    {
                        ImGuiAPI.Text(brick.Name);
                    }
                    ImGuiAPI.NextColumn();

                    ImGuiAPI.Text(brick.FullName);
                    ImGuiAPI.NextColumn();

                    if (isDirect)
                    {
                        ImGuiAPI.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "Direct");
                    }
                    else
                    {
                        ImGuiAPI.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), "Transitive");
                    }
                    ImGuiAPI.NextColumn();
                }

                ImGuiAPI.Columns(1, null, false);
            }
            ImGuiAPI.EndChild();
        }
    }
}
