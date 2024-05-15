using System;
using System.Collections.Generic;
using System.Linq;
//using Microsoft.CodeAnalysis.MSBuild;

namespace EngineNS.Bricks.ProjectGen
{
    public class UBrickManager : IRootForm
    {
        public UBrickManager()
        {
            UEngine.RootFormManager.RegRootForm(this);
        }
        bool mVisible = false;
        public bool Visible 
        {
            get => mVisible;
            set => mVisible = value;
        }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            var path = UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource);
            var files = IO.TtFileManager.GetFiles(path, "*.brick", true);
            foreach(var i in files)
            {
                try
                {
                    string jsonString = IO.TtFileManager.ReadAllText(i);
                    var desc = IO.SerializerHelper.LoadFromJson<UBrickDesc>(jsonString);
                    AllBricks.Add(desc);
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            }
            AllBricks.Sort((x, y) =>
            {
                return x.FullName.CompareTo(y.FullName);
            });

#pragma warning disable CS0162
            if (false)
            {
                var testObj = new UBrickDesc();
                testObj.Name = "ProjectGen";
                testObj.Path = "CSharpCode/Bricks/ProjectGen";
                testObj.SharedProjects.Add("CSharpCode/Base/Base.brick");
                testObj.SharedProjects.Add("CSharpCode/RHI/RHI.brick");
                testObj.SharedProjects.Add("CSharpCode/Rtti/Rtti.brick");
                testObj.Nugets.Add(new UNugetDesc() { Name = "Microsoft.Build", Version = "15.1.1012" });
                testObj.Nugets.Add(new UNugetDesc() { Name = "Microsoft.Build.Framework", Version = "16.5.0" });
                testObj.DllModules.Add("3rd/csharp/System.Windows.Forms.dll");
                testObj.Configs.Add(new UCompileConfig());
                var jsonString = IO.SerializerHelper.SaveAsJson(testObj);
                var file = IO.TtFileManager.CombinePath(path, testObj.FullName + ".bbb");
                IO.TtFileManager.WriteAllText(file, jsonString);
                //string jsonString = IO.FileManager.ReadAllText(@"F:\titan2.0\CSharpCode\Base\Base.brick");
                //var testObj2 = IO.SerializerHelper.LoadFromJson<UBrickDesc>(jsonString);
            }
#pragma warning restore CS0162
            return true;
        }
        public void SaveProject(string projName)
        {
            ProjectDesc.Build();
            ProjectDesc.Name = "Engine";
            ProjectDesc.Configs.Clear();

            UCompileConfig debugConfig = new UCompileConfig();
            debugConfig.Defines.Add("TRACE");
            debugConfig.Defines.Add("PWindow");
            debugConfig.Name = "Debug";
            debugConfig.Arch = UCompileConfig.ECpuArch.AnyCPU;
            ProjectDesc.Configs.Add(debugConfig);

            UCompileConfig releaseConfig = new UCompileConfig();
            releaseConfig.Defines.Add("TRACE");
            releaseConfig.Defines.Add("PWindow");
            releaseConfig.Name = "Release";
            releaseConfig.Arch = UCompileConfig.ECpuArch.AnyCPU;
            ProjectDesc.Configs.Add(releaseConfig);

            var path = UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource);
            ProjectDesc.Build();
            ProjectDesc.SaveVSProject(IO.TtFileManager.CombinePath(path, projName));
        }
        public void Dispose() { }
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);

            Vector2 size = new Vector2(0, 0);
            var result = EGui.UIProxy.DockProxy.BeginMainForm("BrickManager", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                for (int i = 0; i < AllBricks.Count; i++)
                {
                    bool bCheck = AllBricks[i].Checked;
                    EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox(AllBricks[i].FullName, ref bCheck);
                    if (bCheck != AllBricks[i].Checked)
                    {
                        SetBrickChecked(AllBricks[i], bCheck);
                    }
                }
                var sz = new Vector2(0);
                if (ImGuiAPI.Button("Save Project", in sz))
                {
                    SaveProject("CustomEngine");
                }
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
        public void SetBrickChecked(UBrickDesc brick, bool bCheck)
        {
            brick.Checked = bCheck;
            if (bCheck)
            {
                ProjectDesc.AddBrick(brick, this);
            }
            else
            {
                ProjectDesc.RemoveBrick(brick.FullName);
                foreach (var i in brick.SharedProjects)
                {
                    if (ProjectDesc.IsReferProj(i) == false)
                    {
                        ProjectDesc.RemoveBrick(i);
                        var brx = FindBrick(i);
                        if (brx != null)
                        {
                            SetBrickChecked(brx, false);
                        }
                    }
                }
            }
            ProjectDesc.Build();
        }
        public UBrickDesc FindBrick(string fullname)
        {
            foreach (var i in AllBricks)
            {
                if (i.FullName == fullname)
                    return i;
            }
            return null;
        }
        private List<UBrickDesc> AllBricks { get; } = new List<UBrickDesc>();

        public UProjectDesc ProjectDesc = new UProjectDesc();
    }
}

