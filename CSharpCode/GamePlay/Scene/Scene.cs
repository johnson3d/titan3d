using EngineNS.Bricks.CodeBuilder;
using EngineNS.Graphics.Pipeline;
using EngineNS.IO;
using EngineNS.Macross;
using EngineNS.Thread;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.Scene.USceneAMeta@EngineCore" })]
    public class TtSceneAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtScene.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "Scene";
        }
        public override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.SceneBoderColor;
        }
        public override async Thread.Async.TtTask<IO.IAsset> LoadAsset(params object[] args)
        {
            return await TtEngine.Instance.SceneManager.CreateScene(args[0] as TtWorld, GetAssetName());
        }
        public override async Thread.Async.TtTask<IO.IAsset> CreateAsset(params object[] args)
        {
            return await TtEngine.Instance.SceneManager.CreateScene(args[0] as TtWorld, GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "scene", null);
        //}
    }
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.Scene.USceneData@EngineCore" })]
    public class TtSceneData : TtNodeData
    {
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.AssetExt)]
        public RName RPolicyName { get; set; }
        [Rtti.Meta("")]
        public int NumOfNodes { get; set; }
    }
    [TtScene.SceneCreateAttribute]
    [IO.AssetCreateMenu(MenuName = "Scene")]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.Scene.UScene@EngineCore" })]
    public partial class TtScene : TtNode, IO.IAsset
    {
        public const string AssetExt = ".scene";
        public string TypeExt { get => AssetExt; }

        // 编辑器删除节点时, 只记录待删除的 .node 文件路径, 等 SaveAssetTo 时
        // 再真正删除. 这样"删了不保存关闭"不会丢失节点文件.
        internal List<string> PendingDeleteNodeFiles { get; } = new List<string>();
        public override string ToString()
        {
            return this.AssetName.ToString();
        }
        public class SceneCreateAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                //mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType, new USceneData()) as IO.IAsset;
                mAsset = Rtti.TtTypeDescManager.CreateInstance(TypeSlt.SelectedType) as IO.IAsset;
                var world = new TtWorld(null);
                await world.InitWorld();
                var task = (mAsset as TtScene).InitializeNode(world, new TtSceneData(), EBoundVolumeType.Box, typeof(TtPlacement));
                PGAsset.Target = mAsset;
            }
        }
        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
            {
                data = new TtSceneData();
            }
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            SetWorld(world);
            //ParentScene = GetNearestParentScene();

            var task = mMemberTickables.InitializeMembers(this);
            return true;
        }
        public TtScene()
        {
            mMemberTickables.CollectMembers(this);
        }
        ~TtScene()
        {
            Dispose();
        }
        public override void Dispose()
        {
            ClearChildren();
            mMemberTickables.CleanupMembers(this);
            TtEngine.Instance?.SceneManager.UnloadScene(this.AssetName);
        }
        public TtSceneData SceneData
        {
            get
            {
                return NodeData as TtSceneData;
            }
        }
        [Category("Option")]
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        [RName.PGRName(FilterExts = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.AssetExt)]
        public RName RPolicyName
        {
            get => SceneData?.RPolicyName;
            set
            {
                if (SceneData == null)
                    return;
                SceneData.RPolicyName = value;
            }
        }
        [Rtti.Meta("")]
        public async Thread.Async.TtTask<TtRenderPolicy> SetRenderPolicyToViewport(TtViewportSlate slate)
        {
            if (slate.RenderPolicy==null || slate.RenderPolicy.RPolicyName != RPolicyName)
            {
                var policy = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.CreateRenderPolicy(RPolicyName, slate);
                if (policy != null)
                {
                    await policy.Initialize(null);
                    if (slate.Viewport.Width > 1 && slate.Viewport.Height > 1)
                        policy.OnResize(slate.Viewport.Width, slate.Viewport.Height);
                    slate.RenderPolicy = policy;
                    return policy;
                }
            }
            else
            {
                return slate.RenderPolicy;
            }
            return null;
        }
        WeakReference<TtWorld> mWorld;
        public void SetWorld(TtWorld world)
        {
            if (world == null)
            {
                mWorld = null;
                return;
            }
            mWorld = new WeakReference<TtWorld>(world);
        }
        public TtWorld World
        {
            get
            {
                if (mWorld == null)
                    return null;
                if (mWorld.TryGetTarget(out var result))
                    return result;
                return null;
            }
        }

        #region Macross
        Bricks.CodeBuilder.MacrossNode.TtMacrossEditor mMacrossEditor = null;
        [Browsable(false)]
        public Bricks.CodeBuilder.MacrossNode.TtMacrossEditor MacrossEditor
        {
            get
            {
                if (mMacrossEditor == null)
                {
                    mMacrossEditor = new Bricks.CodeBuilder.MacrossNode.TtMacrossEditor();
                    mMacrossEditor.AssetName = AssetName;
                    //mMacrossEditor.FolderExt = ".Macross";
                }
                return mMacrossEditor;
            }
        }
        #endregion

        #region IAsset
        public RName AssetName { get; set; }
        public void SaveAssetTo(RName name)
        {
            name.AMeta.ClearAssetFiles();
            
            UpdateNumOfNodes();
            var typeStr = Rtti.TtTypeDesc.TypeStr(GetType());
            var xndHolder = new EngineNS.IO.TtXndHolder(typeStr, 1, 0);
            var xnd = xndHolder;
            var node = xndHolder.RootNode;
            //if (SceneData != null)
            //{
            //    using (var dataAttr = xnd.NewAttribute(Rtti.TtTypeDesc.TypeStr(SceneData.GetType()), 1, (uint)ENodeFlags.IsNodeDesc))
            //    {
            //        node.AddAttribute(dataAttr);
            //        using (var ar = dataAttr.GetWriter((ulong)SceneData.GetStructSize() * 2))
            //        {
            //            this.OnBeforeSaveNodeData();
            //            ar.Write(SceneData);
            //        }
            //    }
            //}

            //SaveChildNode(this, xnd.mCoreObject, node.mCoreObject);
            node.Name = Rtti.TtTypeDesc.TypeStr(this.GetType());
            node.Version = 1;
            SaveNodeTree(this, xnd.mCoreObject, node.mCoreObject, false);
            SaveRootNodes();

            if (IO.TtFileManager.FileExists(name.Address))
                IO.TtFileManager.DeleteFile(name.Address);

            var file = name.Address + "/" + name.PureName + AssetExt;
            xndHolder.SaveXnd(file);
            name.AMeta.AddAssetFile(file);
            TtEngine.Instance.SourceControlModule.AddFile(file, true);

            // Macross
            MacrossEditor.AssetName = name;
            MacrossEditor.DefClass.ClassName = name.PureName;
            MacrossEditor.DefClass.Namespace = TtNamespaceDeclaration.GetNameSpaceFromRName(AssetName);
            MacrossEditor.DefClass.SupperClassNames.Clear();
            var baseClsName = typeof(TtSceneMacrossBase).FullName;
            if(!MacrossEditor.DefClass.SupperClassNames.Contains(baseClsName))
                MacrossEditor.DefClass.SupperClassNames.Add(baseClsName);
            MacrossEditor.SaveClassGraph(AssetName);
            MacrossEditor.GenerateCode();
            MacrossEditor.CompileCode();

            if (MacrossGetter != null)
            {
                MacrossGetter.Name = name;
                var mc = MacrossGetter.Get();
                if (mc != null)
                {
                    mc.Root = this;
                    mc.InitializeMacrossNodePropertyValues();
                }
            }

            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }

            // 保存完成后, 真正删除编辑器中标记删除的节点文件.
            if (PendingDeleteNodeFiles.Count > 0)
            {
                foreach (var pendingFile in PendingDeleteNodeFiles)
                {
                    TtFileManager.DeleteFile(pendingFile);
                }
                PendingDeleteNodeFiles.Clear();
            }
        }
        public void SaveRootNodes()
        {
            var dir = TtFileManager.CombinePath(this.AssetName.Address, "nodes");
            var t1 = Support.TtTime.HighPrecision_GetTickCount();
            SaveNodeList(dir, this, this);

            var t2 = Support.TtTime.HighPrecision_GetTickCount();
            Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Info, $"Scene({this.AssetName}): SaveRootNodes cost {(t2 - t1) / 1000} ms");
            //TtFileManager.SureDirectory(dir);
            //string rootNodes = "";
            //foreach (var child in Children)
            //{
            //    if (child.HasStyle(ENodeStyles.Transient))
            //        continue;
            //    if (child is TtHubNode)
            //    {

            //    }
            //    if (child is TtSceneActorNode== false)
            //    {
            //        Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Scene({this.AssetName}): SaveRootNodes skipped node which is not TtSceneActorNode. NodeName={child.NodeName}, NodeId={child.NodeId}");
            //        continue;
            //    }

            //    var file = TtFileManager.CombinePath(dir, child.NodeId.ToString() + TtNode.NodeExt);
            //    using (var xnd = new IO.TtXndHolder(Rtti.TtTypeDesc.TypeOf(child.GetType()).TypeString, 0, 0))
            //    {
            //        child.SaveNodeTree(this, xnd.mCoreObject, xnd.RootNode.mCoreObject, true);
            //        xnd.SaveXnd(file);
            //    }
            //    TtEngine.Instance.SourceControlModule.AddFile(file, true);
            //    rootNodes += child.NodeId.ToString()+'\n';
            //}
            //IO.TtFileManager.WriteAllText(TtFileManager.CombinePath(dir, "nodelist.txt"), rootNodes);
            //TtEngine.Instance.SourceControlModule.AddFile(TtFileManager.CombinePath(dir, "nodelist.txt"), true);
        }
        static bool IsSaveMT = true;
        static bool IsLoadMT = false;
        internal unsafe static void SaveNodeList(string dir, TtScene scene, TtNode node)
        {
            TtFileManager.SureDirectory(dir);
            string rootNodes = "";

            if (IsSaveMT)
            {
                List<TtHubNode> hubNodes = new List<TtHubNode>();
                List<TtNode> children = new List<TtNode>();
                for (int i = 0; i<node.Children.Count; i++)
                {
                    var child = node.Children[i];
                    if (child is TtSceneActorNode== false)
                    {
                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Scene({scene.AssetName}): SaveRootNodes skipped node which is not TtSceneActorNode. NodeName={child.NodeName}, NodeId={child.NodeId}");
                        continue;
                    }
                    if (child.HasStyle(ENodeStyles.Transient))
                        continue;
                    if (child is TtHubNode)
                    {
                        rootNodes += "hub:" + child.NodeName + '\n';
                        hubNodes.Add(child as TtHubNode);
                        continue;
                    }

                    children.Add(child);
                }
                string[] childrenHash = new string[children.Count];
                TtEngine.Instance.EventPoster.ParallelFor(children.Count, (i, state) =>
                {
                    var child = children[i];

                    var file = TtFileManager.CombinePath(dir, child.NodeId.ToString() + TtNode.NodeExt);
                    string hash = "";
                    using (var xnd = new IO.TtXndHolder(Rtti.TtTypeDesc.TypeOf(child.GetType()).TypeString, 0, 0))
                    {
                        child.SaveNodeTree(scene, xnd.mCoreObject, xnd.RootNode.mCoreObject, true);
                        //xnd.SaveXnd(file);
                        using (var mem = TtMemWriter.CreateInstance(1024))
                        {
                            var header = XndHolder.GetXndHead();
                            mem.WritePtr(header, 4);
                            xnd.SaveXndWithoutHead(mem);
                            hash = IO.TtFileInfo.ComputeSHA256Hash(mem.Ptr, mem.GetPosition());
                            if (hash != child.SaveHash || !IO.TtFileManager.FileExists(file))
                            {
                                using (var fileWriter = new IO.TtFileWriter(file))
                                {
                                    mem.WriteToFile(fileWriter);
                                }
                                TtRes2Memory.OnAfterWriteFile(file);
                            }
                        }
                    }
                    TtEngine.Instance.SourceControlModule.AddFile(file, true);
                    childrenHash[i] = child.NodeId.ToString() + '#' + hash + '\n';
                });
                foreach(var ch in childrenHash)
                {
                    rootNodes += ch;
                }

                foreach(var hub in hubNodes)
                {
                    SaveNodeList(dir + hub.NodeName, scene, hub);
                }
            }
            else
            {
                for (int i = 0; i<node.Children.Count; i++)
                {
                    var child = node.Children[i];
                    if (child.HasStyle(ENodeStyles.Transient))
                        continue;
                    if (child is TtHubNode)
                    {
                        SaveNodeList(dir + child.NodeName, scene, child);
                        rootNodes += "hub:" + child.NodeName + '\n';
                        continue;
                    }
                    if (child is TtSceneActorNode== false)
                    {
                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Scene({scene.AssetName}): SaveRootNodes skipped node which is not TtSceneActorNode. NodeName={child.NodeName}, NodeId={child.NodeId}");
                        continue;
                    }

                    //todo: gather as list,and parallel save
                    var file = TtFileManager.CombinePath(dir, child.NodeId.ToString() + TtNode.NodeExt);
                    string hash = "";
                    using (var xnd = new IO.TtXndHolder(Rtti.TtTypeDesc.TypeOf(child.GetType()).TypeString, 0, 0))
                    {
                        child.SaveNodeTree(scene, xnd.mCoreObject, xnd.RootNode.mCoreObject, true);
                        //xnd.SaveXnd(file);
                        using (var mem = TtMemWriter.CreateInstance(1024))
                        {
                            var header = XndHolder.GetXndHead();
                            mem.WritePtr(header, 4);
                            xnd.SaveXndWithoutHead(mem);
                            hash = IO.TtFileInfo.ComputeSHA256Hash(mem.Ptr, mem.GetPosition());
                            if (hash != child.SaveHash || !IO.TtFileManager.FileExists(file))
                            {
                                using (var fileWriter = new IO.TtFileWriter(file))
                                {
                                    mem.WriteToFile(fileWriter);
                                }
                                TtRes2Memory.OnAfterWriteFile(file);
                            }
                        }
                    }
                    TtEngine.Instance.SourceControlModule.AddFile(file, true);
                    rootNodes += child.NodeId.ToString() + '#' + hash + '\n';
                }
            }
                
            IO.TtFileManager.WriteAllText(TtFileManager.CombinePath(dir, "nodelist.txt"), rootNodes);
            TtEngine.Instance.SourceControlModule.AddFile(TtFileManager.CombinePath(dir, "nodelist.txt"), true);
        }
        public async Thread.Async.TtTask<bool> LoadRootNodes(TtWorld world)
        {
            var dir = TtFileManager.CombinePath(this.AssetName.Address, "nodes");
            if (TtFileManager.DirectoryExists(dir) == false)
                return false;
            var t1 = Support.TtTime.HighPrecision_GetTickCount();
            await LoadNodeList(world, dir, this, this);
            var t2 = Support.TtTime.HighPrecision_GetTickCount();
            Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Info, $"Scene({this.AssetName}): LoadRootNodes cost {(t2 - t1) / 1000} ms");
            return true;
        }
        internal static async Thread.Async.TtTask LoadNodeList(TtWorld world, string dir, TtScene scene, TtNode parent)
        {
            var rootStr = IO.TtFileManager.ReadAllText(TtFileManager.CombinePath(dir, "nodelist.txt"));
            if (string.IsNullOrEmpty(rootStr))
            {
                //deprecated: old assets
                //var files = TtFileManager.GetFiles(dir, "*" + TtNode.NodeExt, false);
                //foreach (var file in files)
                //{
                //    var name = IO.TtFileManager.GetPureName(file);
                //    var nodeId = Guid.Parse(name);
                //    using (var xnd = IO.TtXndHolder.LoadXnd(file))
                //    {
                //        var node = await TtNode.LoadNodeTree(world, this, this, xnd.RootNode.mCoreObject, false, null);
                //        if (node == null)
                //        {
                //            Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Scene({this.AssetName}): LoadRootNodes failed. NodeFile={file}");
                //            continue;
                //        }
                //        node.NodeId = nodeId;
                //    }
                //}
                return;
            }
            if (IsLoadMT)
            {
                List<string> hubNodes = new List<string>();
                List<string> children = new List<string>();
                var files = rootStr.Split('\n');
                for (int i = 0; i<files.Length; i++)
                {
                    var f = files[i];
                    if (string.IsNullOrEmpty(f))
                        continue;
                    if (f.StartsWith("hub:"))
                    {
                        var name = f.Substring("hub:".Length);
                        hubNodes.Add(name);
                    }
                    else
                    {
                        children.Add(f);
                    }
                }
                var smp = TtSemaphore.CreateSemaphore(children.Count);
                foreach(var f in children)
                {
                    TtEngine.Instance.EventPoster.RunOn(async (state) =>
                    {
                        var f1 = f;
                        if (f.EndsWith('\n'))
                        {
                            f1 = f.Substring(0, f.Length - 1);
                        }
                        var segs = f1.Split('#');
                        Guid nodeId;
                        if (Guid.TryParse(segs[0], out nodeId))
                        {
                            var file = TtFileManager.CombinePath(dir, segs[0] + TtNode.NodeExt);
                            using (var xnd = IO.TtXndHolder.LoadXnd(file))
                            {
                                var node = await TtNode.LoadNodeTree(world, scene, parent, xnd.RootNode.mCoreObject, false, null);
                                if (node == null)
                                {
                                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Scene({scene.AssetName}): LoadRootNodes failed. NodeFile={file}");
                                    smp.Release();
                                    return false;
                                }
                                node.NodeId = nodeId;
                                if (segs.Length>1)
                                {
                                    node.SaveHash = segs[1];
                                }
                                else
                                {
                                    node.SaveHash = null;
                                }
                                System.Diagnostics.Debug.Assert(xnd.ResRefCount <= 0);
                            }
                        }

                        smp.Release();
                        return true;
                    }, Thread.Async.EAsyncTarget.TPools);
                }
                await smp.Await();

                foreach (var name in hubNodes)
                {
                    var subDir = TtFileManager.CombinePath(dir, name);
                    if (TtFileManager.DirectoryExists(subDir))
                    {
                        var hubNode = await TtNode.SpawnNode<TtHubNode>(parent, null);
                        hubNode.Parent = parent;
                        await LoadNodeList(world, subDir, scene, hubNode);
                    }
                }
            }
            else
            {
                //if (string.IsNullOrEmpty(rootStr)==false)
                {
                    var files = rootStr.Split('\n');
                    //foreach (var f in files)
                    for (int i = 0; i<files.Length; i++)
                    {
                        var f = files[i];
                        if (string.IsNullOrEmpty(f))
                            continue;
                        if (f.StartsWith("hub:"))
                        {
                            var name = f.Substring("hub:".Length);
                            var subDir = TtFileManager.CombinePath(dir, name);
                            if (TtFileManager.DirectoryExists(subDir))
                            {
                                var hubNode = await TtNode.SpawnNode<TtHubNode>(parent, null);
                                hubNode.Parent = parent;
                                await LoadNodeList(world, subDir, scene, hubNode);
                            }
                        }
                        else
                        {
                            var f1 = f;
                            if (f.EndsWith('\n'))
                            {
                                f1 = f.Substring(0, f.Length - 1);
                            }
                            var segs = f1.Split('#');
                            Guid nodeId;
                            if (Guid.TryParse(segs[0], out nodeId))
                            {
                                var file = TtFileManager.CombinePath(dir, segs[0] + TtNode.NodeExt);
                                using (var xnd = IO.TtXndHolder.LoadXnd(file))
                                {
                                    var node = await TtNode.LoadNodeTree(world, scene, parent, xnd.RootNode.mCoreObject, false, null);
                                    if (node == null)
                                    {
                                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Scene({scene.AssetName}): LoadRootNodes failed. NodeFile={file}");
                                        continue;
                                    }
                                    node.NodeId = nodeId;
                                    if (segs.Length>1)
                                    {
                                        node.SaveHash = segs[1];
                                    }
                                    else
                                    {
                                        node.SaveHash = null;
                                    }
                                }
                            }
                        }
                    }
                }
                //else
                //{//deprecated: old assets
                //    var files = TtFileManager.GetFiles(dir, "*" + TtNode.NodeExt, false);
                //    foreach (var file in files)
                //    {
                //        var name = IO.TtFileManager.GetPureName(file);
                //        var nodeId = Guid.Parse(name);
                //        using (var xnd = IO.TtXndHolder.LoadXnd(file))
                //        {
                //            var node = await TtNode.LoadNodeTree(world, this, this, xnd.RootNode.mCoreObject, false, null);
                //            if (node == null)
                //            {
                //                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Scene({this.AssetName}): LoadRootNodes failed. NodeFile={file}");
                //                continue;
                //            }
                //            node.NodeId = nodeId;
                //        }
                //    }
                //}
            }
        }
        public int NumOfNodes 
        { 
            get
            {
                return GetNodeData<TtSceneData>().NumOfNodes;
            }
            protected set
            {
                GetNodeData<TtSceneData>().NumOfNodes = value;
            }
        }
        public int NumOfLoadedNode { get; internal set; } = 0;
        public void UpdateNumOfNodes()
        {
            NumOfNodes = 0;
            this.DFS_VisitNodeTree((TtNode inNode, object inArg) =>
            {
                if (inNode.HasStyle(ENodeStyles.Transient))
                    return false;
                ((TtScene)inArg).NumOfNodes++;
                return false;
            }, this);
        }
        internal static async Thread.Async.TtTask<TtScene> LoadScene(GamePlay.TtWorld world, RName name)
        {
            var file = name.Address + "/" + name.PureName + AssetExt;
            if (IO.TtFileManager.FileExists(name.Address))
                file = name.Address;
            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                var dir = TtFileManager.CombinePath(name.Address, "nodes");
                TtScene scene = null;
                if (TtFileManager.DirectoryExists(dir))
                {
                    scene = (await TtNode.LoadNodeTree(world, null, null, xnd.RootNode.mCoreObject, false, null, false)) as TtScene;
                    if (scene == null)
                        return null;
                    scene.AssetName = name;
                    if (false == await scene.LoadRootNodes(world))
                        return null;
                }
                else
                {
                    scene = (await TtNode.LoadNodeTree(world, null, null, xnd.RootNode.mCoreObject, false, null)) as TtScene;
                    if (scene == null)
                        return null;
                    scene.AssetName = name;
                }
                

                //var descAttr = xnd.RootNode.mCoreObject.FindFirstAttributeByFlags((uint)ENodeFlags.IsNodeDesc);
                //if (descAttr.NativePointer == IntPtr.Zero)
                //{
                //    return null;
                //}

                //TtSceneData nodeData = Rtti.TtTypeDescManager.CreateInstance(Rtti.TtTypeDesc.TypeOf(descAttr.Name)) as TtSceneData;

                ////UScene don't have construct with params
                ////UScene scene = Rtti.UTypeDescManager.CreateInstance(Rtti.TtTypeDesc.TypeOf(xnd.RootNode.Name), nodeData) as UScene;
                //TtScene scene = Rtti.TtTypeDescManager.CreateInstance(Rtti.TtTypeDesc.TypeOf(xnd.RootNode.Name)) as TtScene;
                //if (scene == null)
                //    return null;

                //scene.NumOfLoadedNode = 0;

                //using (var ar = descAttr.GetReader(scene))
                //{
                //    IO.ISerializer desc = nodeData;
                //    try
                //    {
                //        ar.ReadTo(desc, scene);
                //        if (await scene.InitializeNode(world, nodeData, EBoundVolumeType.None, null) == false)
                //        {
                //            Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, $"InitializeNode failed: NodeDataType={descAttr.Name}, NodeData={xnd.RootNode.Name}");
                //            return null;
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Profiler.Log.WriteException(ex);
                //        Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, $"SceneData({scene.AssetName}): load failed");
                //    }
                //}
                //scene.NumOfLoadedNode++;

                //scene.AssetName = name;

                //var dir = TtFileManager.CombinePath(name.Address, "nodes");
                //if (TtFileManager.DirectoryExists(dir))
                //{
                //    if (false == await scene.LoadRootNodes(world))
                //        return null;
                //}
                //else
                //{
                //    if (await scene.LoadChildNode(world, scene, xnd.RootNode.mCoreObject, false) == false)
                //        return null;
                //}

                scene.DFS_VisitNodeTree((TtNode inNode, object inArg) =>
                {
                    inNode.OnSceneLoaded();
                    return false;
                }, null);
                var notify = new FHostNotify();
                notify.Info = "OnSceneLoaded";
                scene.GetWorld()?.OnHostNotify(scene, in notify);
                if(scene.MacrossGetter != null)
                {
                    scene.MacrossGetter.Name = name;
                    var mc = scene.MacrossGetter.Get();
                    if (mc != null)
                    {
                        mc.Root = scene;
                        mc.InitializeMacrossNodePropertyValues();
                    }
                }
                return scene;
            }
        }

        TtMacrossGetter<TtSceneMacrossBase> mMacrossGetter;
        public TtMacrossGetter<TtSceneMacrossBase> MacrossGetter
        {
            get
            {
                if (mMacrossGetter == null)
                    mMacrossGetter = TtMacrossGetter<TtSceneMacrossBase>.NewInstance();
                return mMacrossGetter;
            }
        }

        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtSceneAMeta();
            return result;
        }

        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();

            UpdateNodeAssetReferences(this, ameta);
        }
        protected void UpdateNodeAssetReferences(TtNode node, IO.IAssetMeta ameta)
        {
            node.AddAssetReferences(ameta);
            foreach (var i in node.Children)
            {
                i.AddAssetReferences(ameta);
                UpdateNodeAssetReferences(i, ameta);
            }
        }
        #endregion

        TtMemberTickables mMemberTickables = new TtMemberTickables();
        public TtMemberTickables MemberTickables
        {
            get => mMemberTickables;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            mMemberTickables.TickLogic(this, TtEngine.Instance.ElapseTickCountMS);
            return true;
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtScene), nameof(TickLogic));
                return mScopeTick;
            }
        }
        public override Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtScene>.Scope;
        }
        public override unsafe bool IsTreeContain(DVector3* localStart, DVector3* dir, DBoundingBox* pBox)
        {
            return true;
        }
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            FHostNotify notify = new FHostNotify();
            notify.Info = "OnGatherVisibleMeshes";
            notify.Parameter = rp;
            mMemberTickables.SendNotify(this, in notify);
            base.OnGatherVisibleMeshes(rp);
        }
    }

    public partial class TtSceneMacrossBase : Macross.AuxMacrossObject
    {
        public TtScene Root;

        public TtNode FindSceneNode(in Guid id, bool bRecursive)
        {
            if (Root == null)
                return null;

            return Root.FindNode(in id, bRecursive);
        }

        public virtual void InitializeMacrossNodePropertyValues()
        {

        }
    }

    public class TtSceneManager : TtModule<TtEngine>
    {
        public override void Cleanup(TtEngine host)
        {
            Scenes.Clear();
        }
        public Dictionary<RName, WeakReference<TtScene>> Scenes { get; } = new Dictionary<RName, WeakReference<TtScene>>();
        private Thread.TtAwaitSessionManager<RName, TtScene> mCreatingSession = new Thread.TtAwaitSessionManager<RName, TtScene>();
        public async Thread.Async.TtTask<TtScene> GetScene(GamePlay.TtWorld world, RName name)
        {
            //return await UScene.LoadScene(world, name);
            System.GC.Collect();
            TtScene scene;
            WeakReference<TtScene> result;
            if (Scenes.TryGetValue(name, out result))
            {
                result.TryGetTarget(out scene);
                if (scene != null)
                {
                    return scene;
                }
                else
                {
                    Scenes.Remove(name);
                }
            }

            Thread.TtSemaphore smp;
            var session = mCreatingSession.GetOrNewSession(name, out smp);
            if (smp != null)
            {
                await smp.Await();
                return session.Result;
            }

            scene = await TtScene.LoadScene(world, name);
            if (scene == null)
                return null;

            Scenes.Add(name, new WeakReference<TtScene>(scene));
            session.FinishSession(name, scene);
            return scene;
        }
        public async Thread.Async.TtTask<TtScene> CreateScene(GamePlay.TtWorld world, RName name)
        {
            System.GC.Collect();
            TtScene scene;
            scene = await TtScene.LoadScene(world, name);
            if (scene == null)
                return null;

            return scene;
        }
        public void UnloadScene(RName name)
        {
            if (name == null)
                return;
            if (Scenes.ContainsKey(name))
                Scenes.Remove(name);
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public GamePlay.Scene.TtSceneManager SceneManager { get; } = new GamePlay.Scene.TtSceneManager();
    }
}
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.GamePlay.Scene
{
	partial class TtScene
	{
		public async Thread.Async.TtTask<TtRenderPolicy> macross_SetRenderPolicyToViewport (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtViewportSlate slate) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = await SetRenderPolicyToViewport(slate);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross