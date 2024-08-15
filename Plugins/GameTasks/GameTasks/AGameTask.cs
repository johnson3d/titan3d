using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class AGameTaskAssemblyDesc : UAssemblyDesc
        {
            public AGameTaskAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:GameTasks AssemblyDesc Created");
            }
            ~AGameTaskAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:GameTasks AssemblyDesc Destroyed");
            }
            public override string Name { get => "GameTasks"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static AGameTaskAssemblyDesc AssmblyDesc = new AGameTaskAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS.Plugins.GameTasks
{
    public class UPluginLoader
    {
        public static Plugins.GameTasks.UGameTaskPlugin mPluginObject = new Plugins.GameTasks.UGameTaskPlugin();
        public static Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }

    public class UGameTaskPlugin : Bricks.AssemblyLoader.IPlugin
    {
        public AGameTaskDescriptorManager TaskDescriptorManager = new AGameTaskDescriptorManager();
        public void OnLoadedPlugin()
        {
            var editor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
            if (editor != null)
            {
                editor.ContentBrowser.OnTypeChanged();
            }
        }
        public void OnUnloadPlugin()
        {
            var editor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
            if (editor != null)
            {
                editor.ContentBrowser.OnTypeChanged();
            }
        }
    }
    [Rtti.Meta]
    [AGameTaskDescriptor.ADescriptor]
    [IO.CommonCreate]
    [IO.AssetCreateMenu(MenuName = "Game/Task")]
    public class AGameTaskDescriptorAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return AGameTaskDescriptor.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "Item";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            return ((UGameTaskPlugin)UPluginLoader.mPluginObject).TaskDescriptorManager.FindDescriptor(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return false;
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "vms", null);
        //}
        protected override Color4b GetBorderColor()
        {
            return Color4b.LightYellow;
        }
    }

    [Rtti.Meta]
    public class AGameTaskDescriptor : IO.IAsset
    {
        #region IAsset
        public const string AssetExt = ".task";
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new AGameTaskDescriptorAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
            IO.TtFileManager.SaveObjectToXml(name.Address, this);
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion
        public class ADescriptorAttribute : URNameTagObjectAttribute
        {
            public override object GetTagObject(RName rn)
            {
                return ((UGameTaskPlugin)UPluginLoader.mPluginObject).TaskDescriptorManager.FindDescriptor(rn);
            }
        }
        [Rtti.Meta]
        public RName Uid { get; set; }
        [Rtti.Meta]
        public string Name { get; set; }
        [Rtti.Meta]
        public List<RName> RequireTasks { get; set; }
        [Rtti.Meta]
        public List<GameItems.AItemRequirement> AcceptRequireItems { get; set; }
        [Rtti.Meta]
        public List<GameItems.AItemRequirement> FinishRequireItems { get; set; }
        [Rtti.Meta]
        public List<GameItems.AItemMakeSuccess> RewardItems { get; set; }

        public virtual AGameTask CreateTask()
        {
            var result = new AGameTask();
            result.DescriptorUid = Uid;
            result.Uid = Guid.NewGuid();
            return result;
        }
        public virtual bool CanAccept(AGameTaskBoard board)
        {
            if (board.IsFinishedTask(Uid))
            {
                return false;
            }
            foreach (var i in RequireTasks)
            {
                if (board.IsFinishedTask(i) == false)
                {
                    return false;
                }
            }
            foreach (var i in AcceptRequireItems)
            {
                if (board.HasRequireItems(i) == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
    
    public class AGameTaskDescriptorManager
    {
        public Dictionary<RName, AGameTaskDescriptor> TaskDescriptors { get; } = new Dictionary<RName, AGameTaskDescriptor>();
        public AGameTaskDescriptor FindDescriptor(RName id)
        {
            AGameTaskDescriptor result;
            if (TaskDescriptors.TryGetValue(id, out result))
                return result;
            return null;
        }
        public bool RegDescriptor(AGameTaskDescriptor desc)
        {
            AGameTaskDescriptor result;
            if (TaskDescriptors.TryGetValue(desc.Uid, out result))
                return false;
            TaskDescriptors.Add(desc.Uid, desc);
            return true;
        }

        public bool Initialize(RName location)
        {
            var files = IO.TtFileManager.GetFiles(location.Address, "*.task", false);
            foreach (var i in files)
            {
                var descriptor = IO.TtFileManager.LoadXmlToObject(i) as AGameTaskDescriptor;
                if (descriptor == null)
                    continue;
                RegDescriptor(descriptor);
            }
            return true;
        }
    }

    [Rtti.Meta]
    public class AGameTask
    {
        public AGameTaskDescriptor Descriptor
        {
            get
            {
                if (DescriptorUid == null)
                    return null;
                return DescriptorUid.GetTagObject<AGameTaskDescriptor>();
            }
        }
        [Rtti.Meta]
        public RName DescriptorUid { get; set; }
        [Rtti.Meta]
        public Guid Uid { get; set; }

        public virtual bool Abandom(AGameTaskBoard board)
        {
            return true;
        }
        public virtual bool Finish(AGameTaskBoard board)
        {
            return true;
        }
    }

    public class AGameTaskBoard
    {
        [Rtti.Meta]
        public List<AGameTask> FinishedTasks { get; set; }
        [Rtti.Meta]
        public List<AGameTask> ContinueTasks { get; set; }
        public virtual bool IsFinishedTask(RName tsk)
        {
            foreach(var i in FinishedTasks)
            {
                if (i.DescriptorUid == tsk)
                    return true;
            }
            return false;
        }
        public virtual bool HasRequireItems(GameItems.AItemRequirement item)
        {
            return false;
        }
        public bool AcceptTask(AGameTaskDescriptor tsk)
        {
            if (tsk.CanAccept(this) == false)
                return false;

            var tmp = tsk.CreateTask();
            ContinueTasks.Add(tmp);
            return true;
        }
        public bool AbandonTask(AGameTask tsk)
        {
            foreach (var i in ContinueTasks)
            {
                if (i.Uid == tsk.Uid)
                {
                    if (i.Abandom(this))
                    {
                        ContinueTasks.Remove(i);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        public bool TryFinishTask(AGameTask tsk)
        {
            foreach (var i in ContinueTasks)
            {
                if (i.Uid == tsk.Uid)
                {
                    if (i.Finish(this))
                    {
                        ContinueTasks.Remove(i);
                        FinishedTasks.Add(i);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}
