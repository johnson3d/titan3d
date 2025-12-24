using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public class TtBehaviorAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtBehavior.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "Behavior";
        }
        public override async Thread.Async.TtTask<IO.IAsset> LoadAsset(params object[] args)
        {
            //return await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
            return null;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return false;
        }
    }
    [TtBehavior.Import]
    [IO.AssetCreateMenu(MenuName = "Script/Behavior")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtBehavior : IO.BaseSerializer, IO.IAsset
    {
        public const string AssetExt = ".cs";
        public string TypeExt { get => AssetExt; }
        public class ImportAttribute : IO.IAssetCreateAttribute
        {
            bool bPopOpen = false;
            RName mDir;
            string mName;
            string mSourceFile;
            ImGui.ImGuiFileDialog mFileDialog = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                mDir = dir;
            }
            public override unsafe bool OnDraw(EGui.Controls.TtContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Select CS", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                bool retValue = false;
                var visible = true;
                ImGuiAPI.SetNextWindowSize(new Vector2(200, 500), ImGuiCond_.ImGuiCond_FirstUseEver);
                if (ImGuiAPI.BeginPopupModal($"Select CS", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    if (string.IsNullOrEmpty(ContentBrowser.CurrentImporterFile))
                    {
                        var sz = new Vector2(-1, 0);
                        if (ImGuiAPI.Button("Select CS", in sz))
                        {
                            mFileDialog.OpenModal("ChooseFileDlgKey", "Choose File", ".cs", ".");
                        }
                        // display
                        if (mFileDialog.DisplayDialog("ChooseFileDlgKey"))
                        {
                            // action if OK
                            if (mFileDialog.IsOk() == true)
                            {
                                mSourceFile = mFileDialog.GetFilePathName();
                                mName = IO.TtFileManager.GetPureName(mSourceFile);
                            }
                            // close
                            mFileDialog.CloseDialog();
                        }
                    }
                    else if (string.IsNullOrEmpty(mSourceFile))
                    {
                        mSourceFile = ContentBrowser.CurrentImporterFile;
                        mName = IO.TtFileManager.GetPureName(mSourceFile);
                    }

                    ImGuiAPI.Separator();
                    ImGuiAPI.Text(mSourceFile);

                    var btSz = Vector2.Zero;
                    //if (ImGuiAPI.Button("Import", in btSz))
                    //{
                    //    if (ImportCS())
                    //    {
                    //        ImGuiAPI.CloseCurrentPopup();
                    //        retValue = true;
                    //    }
                    //}
                    //ImGuiAPI.SameLine(0, 20);
                    if (ImGuiAPI.Button("Cancel", in btSz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        retValue = true;
                    }
                    using (var buffer = BigStackBuffer.CreateInstance(128))
                    {
                        buffer.SetTextUtf8(mName);
                        ImGuiAPI.InputText("##in_rname", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                        var name = buffer.AsTextUtf8();
                        if (mName != name)
                        {
                            mName = name;
                        }
                    }
                    if (ImGuiAPI.Button("Create", in btSz))
                    {
                        var rn = RName.GetRName(mDir.Name + mName + TtBehavior.AssetExt, mDir.RNameType);
                        CreateCS(rn);
                        ImGuiAPI.CloseCurrentPopup();
                        retValue = true;
                    }
                    ImGuiAPI.EndPopup();
                }
                if (!visible)
                    retValue = true;
                return retValue;
            }
            private unsafe bool ImportCS()
            {
                if (IO.TtFileManager.FileExists(mSourceFile) == false)
                    return false;
                var rn = RName.GetRName(mDir.Name + mName + TtBehavior.AssetExt, mDir.RNameType);
                IO.TtFileManager.CopyFile(mSourceFile, rn.Address);
                return true;
            }
            public class TtCodeWriter : Bricks.CodeBuilder.TtCodeCreator
            {
                public TtCodeWriter()
                {
                    mSegmentStartStr = "{";
                    mSegmentEndStr = "}";
                    mIndentStr = "\t";
                }
            }
            private unsafe void CreateCS(RName assetName)
            {
                var ameta = new TtBehaviorAMeta();
                ameta.SetAssetName(assetName);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtBehavior));
                ameta.Description = $"This is a behavior\n";
                ameta.SaveAMeta((IO.IAsset)null);
                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);

                string code = "";
                var creator = new TtCodeWriter();
                creator.AddLine($"//This is a flag, Please keep it!", ref code);
                creator.AddLine($"using System;", ref code);
                creator.AddLine($"System.Collections.Generic;", ref code);
                creator.AddLine($"using EngineNS;", ref code);
                creator.AddLine($"using EngineNS.GamePlay;", ref code);
                creator.AddLine($"using EngineNS.GamePlay.Scene;", ref code);
                creator.AddLine($"using EngineNS.Thread.Async;", ref code);

                creator.AddLine($"namespace TtBehavior_{ameta.AssetId.ToString().Replace('-', '_')}", ref code);
                creator.PushSegment(ref code);
                {
                    creator.AddLine($"public class {mName} : EngineNS.GamePlay.Scene.TtBehavior", ref code);
                    creator.PushSegment(ref code);
                    {
                        creator.AddLine($"public override async TtTask BeginPlay(EngineNS.GamePlay.Scene.TtNode node)", ref code);
                        creator.PushSegment(ref code);
                        {

                        }
                        creator.PopSegment(ref code);

                        creator.AddLine($"public override void Tick(EngineNS.GamePlay.Scene.TtNode node)", ref code);
                        creator.PushSegment(ref code);
                        {

                        }
                        creator.PopSegment(ref code);

                        creator.AddLine($"public override void DestroyNode(EngineNS.GamePlay.Scene.TtNode node)", ref code);
                        creator.PushSegment(ref code);
                        {

                        }
                        creator.PopSegment(ref code);
                    }
                    creator.PopSegment(ref code);
                }
                creator.PopSegment(ref code);

                IO.TtFileManager.WriteAllText(assetName.Address, code);
            }
        }
        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtBehaviorAMeta();
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
        }
        [Rtti.Meta("")]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion
        public virtual async TtTask BeginPlay(TtNode node)
        {
            return;
        }
        public virtual void Tick(TtNode node)
        {

        }
        public virtual void DestroyNode(TtNode node)
        {

        }
        public virtual void OnContact(TtNode selfNode, TtNode otherNode)
        {

        }
        public virtual void OnBeginTrigger(TtNode selfNode, TtNode otherNode)
        {

        }
        public virtual void OnEndTrigger(TtNode selfNode, TtNode otherNode)
        {

        }
    }

    partial class TtNodeData
    {
        [Rtti.Meta("")]
        public RName BehaviorName { get; set; }
    }
    partial class TtNode
    {
        public const string NodeExt = ".node";
        public class TtBehaviorGetter
        {
            public uint Version = 0;
            public TtBehavior mBehavior;
            public RName BehaviorName;
            public TtBehavior Get(TtNode node)
            {
                if (TtEngine.Instance.MacrossModule.Version != Version || BehaviorName != node.BehaviorName)
                {
                    var save = mBehavior;
                    mBehavior = null;
                    var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(node.BehaviorName);
                    if (ameta == null)
                    {
                        return null;
                    }
                    var assm = TtEngine.Instance.MacrossModule.TryGetAssembly();
                    if (assm != null)
                    {
                        var ns = "TtBehavior_" + ameta.AssetId.ToString().Replace('-', '_');
                        var n = node.BehaviorName.PureName;
                        foreach (var i in assm.GetTypes())
                        {
                            if (i.Namespace == ns && i.Name == n)
                            {
                                mBehavior = Rtti.TtTypeDescManager.CreateInstance(i) as TtBehavior;
                                if (save != null)
                                {
                                    var typeStr = Rtti.TtTypeDesc.TypeStr(mBehavior.GetType());
                                    var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeStr);
                                    if (meta != null)
                                    {
                                        meta.CopyObjectMetaField(mBehavior, save);
                                    }
                                }
                                BehaviorName = node.BehaviorName;
                                Version = TtEngine.Instance.MacrossModule.Version;
                                break;
                            }
                        }
                    }
                }
                return mBehavior;
            }
        }
        protected TtBehaviorGetter mBehaviorGetter;
        [Category("User")]
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.NoSerializable)]
        public TtBehavior Behavior
        {
            get
            {
                if (mBehaviorGetter == null)
                {
                    return null;
                }
                return mBehaviorGetter.Get(this);
            }
        }
        [Category("Option")]
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = TtBehavior.AssetExt)]
        public RName BehaviorName
        {
            get
            {
                return NodeData?.BehaviorName;
            }
            set
            {
                if (NodeData != null)
                {
                    NodeData.BehaviorName = value;
                    if (mBehaviorGetter == null  && value != null)
                    {
                        mBehaviorGetter = new TtBehaviorGetter();
                    }
                    else if(value == null)
                    {
                        mBehaviorGetter = null;
                    }
                }
            }
        }
    }
}
