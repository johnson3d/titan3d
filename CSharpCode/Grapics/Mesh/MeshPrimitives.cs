using EngineNS.IO;
using EngineNS.Thread.Async;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Mesh.UMeshPrimitivesAMeta@EngineCore" })]
    public class TtMeshPrimitivesAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtMeshPrimitives.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "VMS";
        }
        public override async Thread.Async.TtTask<IO.IAsset> GetAsset(params object[] args)
        {
            return await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(GetAssetName());
        }
        public override async TtTask<IAsset> CreateAsset(params object[] args)
        {
            return await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.CreateMeshPrimitive(GetAssetName());
        }
        public override async Thread.Async.TtTask CopyTo(string name, RName.ERNameType type)
        {
            if (mAssetName.Name == name && mAssetName.RNameType == type)
                return;
            var tarName = RName.GetRName(name, type);
            var ameta = TtEngine.Instance.AssetMetaManager.NewAMeta(tarName, typeof(TtMeshPrimitivesAMeta));
            ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtMeshPrimitives)).TypeString;
            foreach (var i in this.RefAssetRNames)
            {
                ameta.AddReferenceAsset(i);
            }
            ameta.SaveAMeta((IO.IAsset)null);

            var targetSnapName = TtEngine.Instance.FileManager.GetRoot2(type) + name;
            IO.TtFileManager.CopyFile(mAssetName.Address, targetSnapName);
            if (IO.TtFileManager.FileExists(targetSnapName))
            {
                mAssetName.AMeta.AddAssetFile(mAssetName.Address);
                TtEngine.Instance.SourceControlModule.AddFile(targetSnapName, true);
            }
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
        public override Color4b GetBorderColor()
        {
            return TtEngine.Instance.ConfigManager.GetConfig<Editor.TtEditorConfig>().MeshPrimitivesBoderColor;
        }

        [Rtti.Meta("")]
        public bool IsClustered { get; set; } = false;

        /// <summary>
        /// 编辑器中最近关联的 PhysicsAsset 路径，打开编辑器时自动加载。cook 后可丢弃。
        /// </summary>
        [Rtti.Meta("")]
        public RName PhysicsAssetRName { get; set; }

        /// <summary>
        /// 纹理流送用的 texel factor，表示该 mesh 在世界单位下每个纹理 texel 所覆盖的尺度，
        /// 供 TtTextureManager 的 streaming 调度根据 mesh 屏幕尺寸推算期望 mip。
        /// 参考 UE 的 FStreamingTextureBuildInfo.TexelFactor。
        /// TODO: 后续由 TtMeshPrimitiveEditor 在构建期根据三角形世界面积与 UV 面积之比统计填充，
        /// 当前先给缺省值。
        /// </summary>
        [Rtti.Meta("")]
        public float StreamingTexelFactor { get; set; } = 1.0f;

        /// <summary>
        /// 标识该 Mesh 是否需要构建 BLAS (Bottom Level Acceleration Structure).
        /// 为 true 时, 加载 Mesh 后会自动在 cache/cookedassets/blas/ 下查找或构建 BLAS.
        /// </summary>
        [Rtti.Meta("")]
        public bool HasBLAS { get; set; } = false;

        /// <summary>
        /// 导入该 mesh 的源文件路径 (FBX/OBJ/glTF 等), 空串表示不是导入来的
        /// (程序化生成的 Box/Sphere 等, 或导入于本字段存在之前)。
        ///
        /// 对应 TtSrViewAMeta.OriginImageAddress: 资产必须能回答"我是从哪个文件来的",
        /// 否则出了问题只能靠猜 —— 例如 blendshape 法线缺失时, 无法分清是源文件本身
        /// 没带法线还是导入器丢了。AssetFiles 记的是资产自己的文件, 语义不同, 不能兼用。
        ///
        /// 与 OriginImageAddress 的一处刻意差异: 不做"探测同名同目录文件"的 getter 回退。
        /// 纹理的源 png/hdr 常与资产同目录, 而 FBX 通常在 content 之外的美术目录, 探测
        /// 只会编出一个不存在的路径, 比返回空串更难排查。
        ///
        /// 路径是当时导入机器上的绝对路径, 换人/换机器后可能失效 —— 它是溯源线索而不是
        /// 可信依赖, 不要拿它做自动重导入之类依赖它必然存在的事。
        /// </summary>
        [Rtti.Meta("")]
        public string OriginSourceAddress { get; set; } = null;
    }

    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Mesh.UMeshPrimitives@EngineCore" })]
    [TtMeshPrimitives.Import]
    [IO.AssetCreateMenu(MenuName = "Mesh/Mesh")]
    public partial class TtMeshPrimitives : AuxPtrType<NxRHI.FMeshPrimitives>, IO.IAsset
    {
        public const string AssetExt = ".vms";
        public string TypeExt { get => AssetExt; }
        public partial class ImportAttribute : IO.CommonCreateAttribute
        {
            ~ImportAttribute()
            {
                //mFileDialog.Dispose();
            }
            string mSourceFile;
            ImGui.ImGuiFileDialog mFileDialog = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            //EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                mDir = dir;
                MeshImportSettings.Clear();
                mImportSourceQueue.Clear();
                mPendingImportTask = null;
                mPendingImportSource = "";
                mPendingImportMessage = "";
                mImportError = "";
                MeshType = "FromFile";
                await PGAsset.Initialize();
                //mDesc.Desc.SetDefault();
                //PGAsset.SingleTarget = mDesc;
            }
            public override unsafe bool OnDraw(EGui.Controls.TtContentBrowser ContentBrowser)
            {
                //we also can import from other types
                //return FBXCreateCreateDraw(ContentBrowser);
                return AssimpCreateCreateDraw(ContentBrowser);
            }

            public unsafe partial bool FBXCreateCreateDraw(EGui.Controls.TtContentBrowser ContentBrowser);
            public unsafe partial bool AssimpCreateCreateDraw(EGui.Controls.TtContentBrowser ContentBrowser);
        }
        public TtMeshPrimitives()
        {
            mCoreObject = NxRHI.FMeshPrimitives.CreateInstance();
        }
        public TtMeshPrimitives(string name, uint atom)
        {
            mCoreObject = NxRHI.FMeshPrimitives.CreateInstance();
            mCoreObject.Init(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, name, atom);
        }
        public TtMeshPrimitives(NxRHI.FMeshPrimitives iMeshPrimitives)
        {
            mCoreObject = iMeshPrimitives;
            System.Diagnostics.Debug.Assert(mCoreObject.IsValidPointer);
        }
        public bool Init(string name, uint atom)
        {
            return mCoreObject.Init(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, name, atom);
        }
        public uint PrimitiveNumber
        {
            get
            {
                return mCoreObject.GetPrimitiveNumber();
            }
        }
        public uint VertexNumber
        {
            get
            {
                return mCoreObject.GetVertexNumber();
            }
        }
        public NxRHI.IBuffer GetIndexBuffer()
        {
            return mCoreObject.GetGeomtryMesh().IndexBuffer.Buffer;
        }
        public NxRHI.IBuffer GetVertexBuffer(NxRHI.EVertexStreamType type)
        {
            return mCoreObject.GetGeomtryMesh().GetVertexArray().GetVB(type).Buffer;
        }
        public void SetTransientVertexBuffer(NxRHI.TtTransientBuffer buffer)
        {
            mCoreObject.SetTransientVertexBuffer(buffer.mCoreObject);
        }
        public void SetTransientIndexBuffer(NxRHI.TtTransientBuffer buffer)
        {
            mCoreObject.SetTransientIndexBuffer(buffer.mCoreObject);
        }
        public void PushAtom(uint index, in EngineNS.NxRHI.FMeshAtomDesc desc)
        {
            mCoreObject.PushAtom(index, in desc);
        }
        public uint NumAtom
        {
            get
            {
                return mCoreObject.GetAtomNumber();
            }
        }
        #region IAsset
        public override void Dispose()
        {
            base.Dispose();
        }
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtMeshPrimitivesAMeta();
            return result;
        }
        IO.IAssetMeta mAMeta = null;
        public IO.IAssetMeta GetAMeta()
        {
            if (mAMeta == null)
            {
                mAMeta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
            }
            return mAMeta;
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();

            var meshMeta = ameta as TtMeshPrimitivesAMeta;
            if (meshMeta != null && meshMeta.IsClustered)
            {
                ameta.AddReferenceAsset(RName.GetRName(AssetName.Name + ".clustermesh", AssetName.RNameType));
            }
        }
        /// <summary>
        /// 参考 UE FUVDensityAccumulator：遍历 mesh 所有三角形，按「世界面积 / UV 面积」统计加权平均
        /// 得到 StreamingTexelFactor（1 UV 单位对应多少世界单位），写入 ameta 并保存。
        /// </summary>
        public unsafe void ComputeAndApplyTexelFactor()
        {
            var mdp = new TtMeshDataProvider();
            if (!mdp.InitFrom(this))
                return;

            var builder = mdp.mCoreObject;
            int vertexCount = (int)builder.VertexNumber;
            if (vertexCount == 0)
                return;

            var pPos = (Vector3*)builder.GetStream(NxRHI.EVertexStreamType.VST_Position).GetData();
            var pUV = (Vector2*)builder.GetStream(NxRHI.EVertexStreamType.VST_UV).GetData();
            if (pPos == null || pUV == null)
                return;

            bool isIndex32 = mdp.IsIndex32;
            byte* pIndices = (byte*)builder.GetIndices().GetData();
            if (pIndices == null)
                return;

            // 收集所有三角形的 (weight, uvDensity)
            var elements = new List<(float Weight, float UVDensity)>();

            uint numAtoms = builder.GetAtomNumber();
            for (uint a = 0; a < numAtoms; a++)
            {
                var atom = builder.GetAtom(a, 0);
                uint startIdx = atom->m_StartIndex;
                uint numTri = atom->m_NumPrimitives;

                for (uint t = 0; t < numTri; t++)
                {
                    int i0, i1, i2;
                    if (isIndex32)
                    {
                        int* pi = (int*)pIndices;
                        i0 = pi[startIdx + t * 3 + 0];
                        i1 = pi[startIdx + t * 3 + 1];
                        i2 = pi[startIdx + t * 3 + 2];
                    }
                    else
                    {
                        ushort* pi = (ushort*)pIndices;
                        i0 = pi[startIdx + t * 3 + 0];
                        i1 = pi[startIdx + t * 3 + 1];
                        i2 = pi[startIdx + t * 3 + 2];
                    }

                    // 世界面积（叉积长度 = 2x 三角形面积）
                    Vector3 e1 = pPos[i1] - pPos[i0];
                    Vector3 e2 = pPos[i2] - pPos[i0];
                    float worldArea = Vector3.Cross(e1, e2).Length();

                    if (worldArea <= 1e-8f)
                        continue;

                    // UV 面积（2D 叉积绝对值）
                    Vector2 uv1 = pUV[i1] - pUV[i0];
                    Vector2 uv2 = pUV[i2] - pUV[i0];
                    float uvArea = MathF.Abs(uv1.X * uv2.Y - uv1.Y * uv2.X);

                    if (uvArea <= 1e-8f)
                        continue;

                    float weight = MathF.Sqrt(worldArea);
                    float density = MathF.Sqrt(worldArea / uvArea);
                    elements.Add((weight, density));
                }
            }

            if (elements.Count == 0)
            {
                Profiler.Log.WriteLine<Profiler.TtLogCategory>(
                    Profiler.ELogTag.Warning, "BuildTexelFactor", "No valid triangles found.");
                return;
            }

            // 排序 + 去头尾各 10% 极端值 + 面积加权平均（与 UE FUVDensityAccumulator 一致）
            elements.Sort((x, y) => x.UVDensity.CompareTo(y.UVDensity));
            int discard = (int)(elements.Count * 0.1f);
            float sumWeighted = 0, sumWeight = 0;
            for (int i = discard; i < elements.Count - discard; i++)
            {
                sumWeighted += elements[i].UVDensity * elements[i].Weight;
                sumWeight += elements[i].Weight;
            }

            float texelFactor = (sumWeight > 1e-8f) ? (sumWeighted / sumWeight) : 1.0f;

            // 写入 ameta
            var meshMeta = GetAMeta() as TtMeshPrimitivesAMeta;
            if (meshMeta != null)
            {
                meshMeta.StreamingTexelFactor = texelFactor;
                meshMeta.SaveAMeta((IO.IAsset)null);
                Profiler.Log.WriteLine<Profiler.TtLogCategory>(
                    Profiler.ELogTag.Info, "BuildTexelFactor",
                    $"StreamingTexelFactor = {texelFactor:F4} for {AssetName}");
            }
        }

        public void SaveAssetTo(RName name)
        {
            ComputeAndApplyTexelFactor();

            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
            //这里需要存盘的情况很少，正常来说vms是fbx导入的时候生成的，不是保存出来的
            var rc = TtEngine.Instance?.GfxDevice.RenderContext;
            var xnd = new IO.TtXndHolder("TtMeshPrimitives", 0, 0);
            unsafe
            {
                mCoreObject.Save2Xnd(rc.mCoreObject, xnd.RootNode.mCoreObject);
            }
            var attr = xnd.RootNode.GetOrAddAttribute("PartialSkeleton",0,0, true);
            using (var ar = attr.GetWriter(512))
            {
                ar.Write(PartialSkeleton);
            }
            if (MorphTargets != null && MorphTargets.IsValid)
            {
                var morphAttr = xnd.RootNode.GetOrAddAttribute(TtMorphTargetSet.AttributeName, 0, 0, true);
                using (var ar = morphAttr.GetWriter(1024))
                {
                    MorphTargets.Save(ar);
                }
            }
            if (Meshlets != null)
            {
                var meshlets = xnd.RootNode.GetOrAddNode("Meshlets", 0, 0, true);
                Meshlets.SaveXnd(meshlets);
            }
            xnd.SaveXnd(name.Address);
            name.AMeta.AddAssetFile(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address, true);
        }
        [Rtti.Meta("")]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion

        [Rtti.Meta("")]
        public Animation.SkeletonAnimation.Skeleton.TtSkinSkeleton PartialSkeleton
        {
            get;
            set;
        }

        /// <summary>
        /// Morph target (BlendShape) 数据, 由 FBX 导入期填充, 随 mesh 资产一起存取。
        /// 为 null 表示该 mesh 没有 morph —— 绝大多数 mesh 都是这种情况, 不要假设非空。
        ///
        /// 存储走 XND attribute (与 PartialSkeleton 同一模式), 旧资产没有该 attribute 时
        /// 加载后保持 null, 行为与加 morph 之前完全一致。
        /// </summary>
        public TtMorphTargetSet MorphTargets
        {
            get;
            set;
        }

        /// <summary>
        /// 预构建的 BLAS (Bottom Level Acceleration Structure), 供光线追踪 TLAS 组装使用.
        /// 由 TryLoadOrBuildBLAS 在 mesh 加载后自动填充 (当 AMeta.HasBLAS == true 时).
        /// </summary>
        public NxRHI.TtAccelerationStructure BLAS { get; private set; }

        /// <summary>
        /// 编辑器代理属性: 读写 AMeta.HasBLAS, 标记该 Mesh 是否需要构建 BLAS.
        /// </summary>
        [Category("RayTracing")]
        public bool HasBLAS
        {
            get
            {
                var meta = GetAMeta() as TtMeshPrimitivesAMeta;
                return meta?.HasBLAS ?? false;
            }
            set
            {
                var meta = GetAMeta() as TtMeshPrimitivesAMeta;
                if (meta != null)
                {
                    meta.HasBLAS = value;
                    meta.SaveAMeta(this);
                    if (meta.HasBLAS)
                    {
                        // 编辑器中勾选后立即触发构建
                        TryLoadOrBuildBLAS().AddWaitTask();
                    }
                }
            }
        }
        public unsafe static TtMeshPrimitives LoadXnd(RName name, TtMeshPrimitiveManager manager, IO.TtXndHolder xnd, bool bTryLoadMeshlets)
        {
            var result = new TtMeshPrimitives();
            
            try
            {
                var ret = result.mCoreObject.LoadXnd(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, name.ToString(), xnd.mCoreObject, true);
                if (ret == false)
                    return null;
                var attr = xnd.RootNode.TryGetAttribute("PartialSkeleton");
                if (attr.IsValidPointer)
                {
                    IO.ISerializer partialSkeleton = null;
                    using (var ar = attr.GetReader(manager))
                    {
                        try
                        {
                            ar.Read(out partialSkeleton, manager);
                        }
                        catch (Exception exp)
                        {
                            Profiler.Log.WriteException(exp);
                        }
                    }
                    if(partialSkeleton is Animation.SkeletonAnimation.Skeleton.TtSkinSkeleton)
                    {
                        result.PartialSkeleton = partialSkeleton as Animation.SkeletonAnimation.Skeleton.TtSkinSkeleton;
                    }
                }
                var morphAttr = xnd.RootNode.TryGetAttribute(TtMorphTargetSet.AttributeName);
                if (morphAttr.IsValidPointer)
                {
                    using (var ar = morphAttr.GetReader(manager))
                    {
                        try
                        {
                            result.MorphTargets = TtMorphTargetSet.Load(ar);
                        }
                        catch (Exception exp)
                        {
                            // morph 数据损坏不应该打断整个 mesh 的加载, 退化为"该 mesh 无 morph"
                            result.MorphTargets = null;
                            Profiler.Log.WriteException(exp);
                        }
                    }
                }
                if (bTryLoadMeshlets)
                {
                    var meshlets = xnd.RootNode.TryGetChildNode("Meshlets");
                    if (meshlets.IsValidPointer)
                    {
                        result.LoadMeshlets(meshlets);
                    }
                }
                return result;
            }
            catch (Exception exp)
            {
                Profiler.Log.WriteException(exp);
                return null;
            }
        }

        private TtMeshDataProvider mMeshDataProvider;
        public async Thread.Async.TtTask LoadMeshDataProvider()
        {
            if (mMeshDataProvider != null || AssetName == null)
                return;

            if (mMeshDataProvider == null)
            {
                var result = await TtEngine.Instance.EventPoster.Post((Thread.Async.FPostEvent<bool>)((state) =>
                {
                    using (var xnd = IO.TtXndHolder.LoadXnd(AssetName.Address))
                    {
                        if (xnd != null)
                        {
                            var tmp = new TtMeshDataProvider();

                            var ok = tmp.mCoreObject.LoadFromMeshPrimitive(xnd.RootNode.mCoreObject, NxRHI.EVertexStreamType.VST_FullMask);
                            if (ok == false)
                                return false;

                            mMeshDataProvider = tmp;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }), Thread.Async.EAsyncTarget.AsyncIO);
            }
        }
        public void FreeMeshDataProvider()
        {
            mMeshDataProvider = null;
        }
        public TtMeshDataProvider MeshDataProvider
        {
            get
            {
                return mMeshDataProvider;
            }
        }
        public unsafe Support.TtBlobObject BuildFaceDataWithMaterialIds()
        {
            var result = new Support.TtBlobObject();
            result.ReSize(0);
            for (int i = 0; i< NumAtom; i++)
            {
                var atom = mCoreObject.GetAtom((uint)i, 0);
                for (int j = 0; j< atom->NumPrimitives; j++)
                {
                    result.PushValue(i);
                }
            }
            //mMeshDataProvider.mCoreObject.SetUserBuffer(result.mCoreObject);
            return result;
        }
    }
    public class TtMeshPrimitiveManager
    {
        ~TtMeshPrimitiveManager()
        {
            mUnitSphere?.Dispose();
            mUnitSphere = null;
            //foreach (var i in Meshes)
            //{
            //    int n = i.Value.Core_UnsafeGetRefCount();
            //    if (n != 1)
            //    {

            //    }
            //    else
            //    {
            //        i.Value.Dispose();
            //    }
            //}
            Meshes.Clear();
        }
        TtMeshPrimitives mUnitSphere;
        public TtMeshPrimitives UnitSphere
        {
            get
            {
                if (mUnitSphere == null)
                {
                    mUnitSphere = Graphics.Mesh.TtMeshDataProvider.MakeSphere(1.0f, 15, 15, 0xfffffff).ToMesh();
                }
                return mUnitSphere;
            }
        }
        TtMeshPrimitives mUnitBox;
        public TtMeshPrimitives UnitBox
        {
            get
            {
                if (mUnitBox == null)
                {
                    mUnitBox = Graphics.Mesh.TtMeshDataProvider.MakeBox(0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 0xfffffff).ToMesh();
                }
                return mUnitBox;
            }
        }
        public Dictionary<RName, TtMeshPrimitives> Meshes { get; } = new Dictionary<RName, TtMeshPrimitives>();
        public async System.Threading.Tasks.Task Initialize()
        {
            var t = GetMeshPrimitive(RName.GetRName("axis/movex.vms", RName.ERNameType.Engine));
            await t;
        }
        public TtMeshPrimitives FindMeshPrimitive(RName name)
        {
            TtMeshPrimitives result;
            if (Meshes.TryGetValue(name, out result))
                return result;
            return null;
        }
        internal TtMeshPrimitives UnsafeRemove(RName name)
        {
            lock (Meshes)
            {
                if (Meshes.TryGetValue(name, out var result))
                {
                    return result;
                }
                return null;
            }
        }
        internal void UnsafeAdd(RName name, TtMeshPrimitives obj)
        {
            lock (Meshes)
            {
                Meshes.Add(name, obj);
            }
        }
        //public async System.Threading.Tasks.Task<UMeshPrimitives> GetMeshPrimitive(RName name)
        public async Thread.Async.TtTask<TtMeshPrimitives> GetMeshPrimitive(RName name, bool bTryLoadMeshlets = true)
        {
            if (name == null)
                return null;
            TtMeshPrimitives result;
            if (Meshes.TryGetValue(name, out result))
                return result;

            result = await CreateMeshPrimitive(name, bTryLoadMeshlets);

            if (result != null)
            {
                Meshes[name] = result;
                return result;
            }

            return null;
        }
        public async Thread.Async.TtTask<TtMeshPrimitives> CreateMeshPrimitive(RName name, bool bTryLoadMeshlets = true)
        {
            TtMeshPrimitives result;
            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
                {
                    if (xnd != null)
                    {
                        var mesh = TtMeshPrimitives.LoadXnd(name, this, xnd, bTryLoadMeshlets);
                        if (mesh == null)
                            return null;

                        mesh.AssetName = name;
                        return mesh;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result != null)
            {
                await result.TryLoadClusteredMesh();
                await result.TryLoadOrBuildBLAS();
                return result;
            }
            return null;
        }
        public void UnsafeRenameForCook(RName name, RName newName)
        {
            TtMeshPrimitives result;
            if (Meshes.TryGetValue(name, out result) == false)
                return;

            Meshes.Remove(name);
            result.GetAMeta().SetAssetName(newName);
            result.AssetName = newName;
            Meshes.Add(newName, result);
        }
    }
}

namespace EngineNS.Graphics.Pipeline
{
    public partial class TtGfxDevice
    {
        public Mesh.TtMeshPrimitiveManager MeshPrimitiveManager { get; } = new Mesh.TtMeshPrimitiveManager();
    }
}
