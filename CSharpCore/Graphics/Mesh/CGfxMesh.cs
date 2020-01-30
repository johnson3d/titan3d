using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.Graphics.View;
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;
using System.ComponentModel;

namespace EngineNS.Graphics.Mesh
{
    
    [Rtti.MetaClass]
    public partial class CGfxMesh : AuxCoreObject<CGfxMesh.NativePointer>, IO.IResourceFile, IO.Serializer.ISerializer
    {
        #region ISerializer
        public void ReadObjectXML(XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObjectXML(this, node);
        }

        public void WriteObjectXML(XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObjectXML(this, node);
        }

        public void ReadObject(IReader pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg);
        }

        public void ReadObject(IReader pkg, MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg, metaData);
        }

        public void WriteObject(IWriter pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg);
        }

        public void WriteObject(IWriter pkg, MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg, metaData);
        }
        public EngineNS.IO.Serializer.ISerializer CloneObject()
        {
            return EngineNS.IO.Serializer.SerializerHelper.CloneObject(this);
        }
        #endregion

        EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task<Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        {
            return await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        }
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }

        
        public class MeshVars
        {
            public MeshVars(CGfxMesh This)
            {
                mHost = This;
            }
            CGfxMesh mHost;
            public UInt32 Version;
            Matrix mWorldMtx = Matrix.Identity;
            public Matrix WorldMtx
            {
                get { return mWorldMtx; }
                set
                {
                    mWorldMtx = value;
                    mWorldInverseMtx = Matrix.Invert(ref mWorldMtx);
                    var skinModifier = mHost.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                    if (skinModifier != null && skinModifier.AnimationPose!= null)
                    {
                        skinModifier.AnimationPose.WorldMatrix = value;
                    }
                    if (mHost.CBuffer != null)
                    {
                        mHost.CBuffer.SetValue(mWorldMatrixId, WorldMtx, 0);
                        mHost.CBuffer.SetValue(mWorldMatrixInverseID, mWorldInverseMtx, 0);
                    }
                }
            }
            public Matrix mWorldInverseMtx;
            Vector4 mHitProxyColor;
            public Vector4 HitProxyColor
            {
                get { return mHitProxyColor; }
                set
                {
                    mHitProxyColor = value;

                    mHost.CBuffer.SetValue(mID_HitProxyId, mHitProxyColor, 0);
                }
            }
            public UInt32 mHitProxyId;

            private float mPickedID;
            public float PickedID
            {
                get { return mPickedID; }
                set
                {
                    mPickedID = value;
                    mHost.CBuffer.SetValue(mID_PickedID, mPickedID, 0);
                }
            }

            Vector4 mActorIdColor;
            public Vector4 ActorIdColor
            {
                get => mActorIdColor;
                set
                {
                    mActorIdColor = value;
                    mHost.CBuffer.SetValue(mID_ActorId, mActorIdColor, 0);
                }
            }
            public UInt32 mActorId;

            public static Vector4 GetActorIdColorFromActorId(UInt32 actorId)
            {
                return new Vector4(((actorId >> 24) & 0x000000ff) / 255.0f, ((actorId >> 16) & 0x000000ff) / 255.0f, ((actorId >> 8) & 0x000000ff) / 255.0f, ((actorId >> 0) & 0x000000ff) / 255.0f);
            }
            public static UInt32 GetActorIdFromActorIdColor(byte[] color)
            {
                return (UInt32)(((int)(color[0]) << 24) + ((int)(color[1]) << 16) + ((int)(color[2]) << 8) + (color[3]));
            }
            //public static Profiler.TimeScope ScopeSetPointLights = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxMesh), nameof(SetPointLights));
            public void SetPointLights(List<GamePlay.SceneGraph.GSceneGraph.AffectLight> affectLights)
            {
                //using(new Profiler.TimeScopeHelper(ScopeSetPointLights))
                {
                    mHost.CBuffer.SetValue(mID_PointLightNum, affectLights.Count, 0);

                    if (affectLights.Count == 0)
                        return;

                    Vector4 indices = new Vector4();
                    for (int i = 0; i < affectLights.Count; i++)
                    {
                        indices[i] = affectLights[i].Light.IndexInScene;
                    }
                    mHost.CBuffer.SetValue(mID_PointLightIndices, indices, 0);
                }
            }
        }

        public GamePlay.Actor.GActor HostActor;

        
        public MeshVars mMeshVars;
        private CConstantBuffer mCBuffer;
        public CConstantBuffer CBuffer
        {
            get
            {
                if (mCBuffer == null)
                {
                    var rc = CEngine.Instance.RenderContext;
                    mCBuffer = rc.CreateConstantBuffer(CEngine.Instance.EffectManager.DefaultEffect.ShaderProgram,
                            CEngine.Instance.EffectManager.DefaultEffect.CacheData.CBID_Mesh);
                    if (mWorldMatrixId == -1)
                        mWorldMatrixId = mCBuffer.FindVar("WorldMatrix");
                    if (mWorldMatrixInverseID == -1)
                        mWorldMatrixInverseID = mCBuffer.FindVar("WorldMatrixInverse");
                    if (mID_HitProxyId == -1)
                        mID_HitProxyId = mCBuffer.FindVar("HitProxyId");
                    if (mID_ActorId == -1)
                        mID_ActorId = mCBuffer.FindVar("ActorId");
                    if (mID_PickedID == -1)
                    {
                        mID_PickedID = mCBuffer.FindVar("PickedID");
                    }
                    if (mID_PointLightNum==-1)
                    {
                        mID_PointLightNum = mCBuffer.FindVar("PointLightNum");
                    }
                    if (mID_PointLightIndices == -1)
                    {
                        mID_PointLightIndices = mCBuffer.FindVar("PointLightIndices");
                    }
                }
                return mCBuffer;
            }
        }
        private static int mWorldMatrixId = -1;
        private static int mWorldMatrixInverseID = -1;
        private static int mID_HitProxyId = -1;
        private static int mID_ActorId = -1;
        private static int mID_PickedID = -1;
        private static int mID_PointLightNum = -1;
        private static int mID_PointLightIndices = -1;

        public object Tag
        {
            get;
            set;
        }
        public CGfxMesh OrigionMesh
        {
            get;
            internal set;
        }
        public int SaveSerialId
        {
            get;
            protected set;
        } = 1;

        public bool mSelected = false;

        public CGfxMesh()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxMesh");
            mMeshVars = new MeshVars(this);
        }
        RName mName;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Editor.Editor_PackData()]
        [Rtti.MetaData]
        [Browsable(false)]
        public RName Name
        {
            get => mName;
            private set => mName = value;
        }

        RName mGeomName;
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.MeshSource)]
        [Editor.Editor_PackData()]
        [Rtti.MetaData]
        public RName MeshPrimitiveName
        {
            get => mGeomName;
            protected set => mGeomName = value;
        }

        List<RName> mMaterialNames = new List<RName>();
        [Editor.Editor_PackData()]
        [Rtti.MetaData]
        public List<RName> MaterialNames
        {
            get => mMaterialNames;
            protected set => mMaterialNames = value;
        }
        internal RName GetMaterialName(UInt32 index)
        {
            if (index > (UInt32)mMaterialNames.Count)
                return RName.EmptyName;
            return mMaterialNames[(int)index];
        }
        private bool SetMaterial(UInt32 index, CGfxMtlMesh material)
        {
            var atomNumber = SDK_GfxMesh_GetAtomNumber(CoreObject);
            if (index > atomNumber)
                return false;

            if (index >= mMaterialNames.Count)
            {
                for (UInt32 i = 0; i < index - mMaterialNames.Count + 1; i++)
                {
                    mMaterialNames.Add(RName.EmptyName);
                }
            }
            if (material.MtlInst != null)
                mMaterialNames[(int)index] = material.MtlInst.Name;
            else
                mMaterialNames[(int)index] = EngineNS.RName.EmptyName;
            return true;
        }

        void SetMeshPrimitives()
        {
            SDK_GfxMesh_SetMeshPrimitives(CoreObject, mMeshPrimitives.CoreObject);
            mGeomName = mMeshPrimitives.Name;
        }

        public bool Init(CRenderContext rc, RName name, CGfxMeshPrimitives meshPrimitive/*, CGfxShadingEnv senv*/)
        {
            mMeshPrimitives = meshPrimitive;
            //mShadingEnv = senv;
            if (mMeshPrimitives == null)
            {
                EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
                return false;
            }
            SetMeshPrimitives();

            mName = name;
            mGeomName = MeshPrimitives.Name;
            //mMdfQueue = new CGfxMdfQueue();
            MdfQueue = mMeshPrimitives.MdfQueue.CloneMdfQueue(rc, mMdfQueue);
            if (false == SDK_GfxMesh_Init(CoreObject, name != null ? name.Name : "", mMeshPrimitives.CoreObject, mMdfQueue.CoreObject))
            {
                EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
                return false;
            }
            SDK_GfxMesh_SetGfxMdfQueue(CoreObject, mMdfQueue.CoreObject);

            mMaterialNames.Clear();
            if (mMtlMeshArray == null || mMtlMeshArray.Length != mMeshPrimitives.AtomNumber)
            {
                mMtlMeshArray = new CGfxMtlMesh[mMeshPrimitives.AtomNumber];
            }

            //if (mCBuffer == null)
            //{
            //    mCBuffer = rc.CreateConstantBuffer(CEngine.Instance.EffectManager.DefaultEffect.ShaderProgram,
            //                        CEngine.Instance.EffectManager.DefaultEffect.CBID_Mesh);
            //}

            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);
            return true;
        }

        //通常用在生成缩略图什么的保证一切资源都加载后使用，自己别乱调用
        public void PreUse(bool isSync = false)
        {
            mMeshPrimitives.PreUse(isSync);
            for (int i = 0; i < mMtlMeshArray.Length; i++)
            {
                if (mMtlMeshArray[i] == null)
                    continue;

                mMtlMeshArray[i].MtlInst.PreUse(isSync);
                foreach (var j in mMtlMeshArray[i].Textures)
                {
                    j.Value?.PreUse(isSync);
                }
            }
        }

        static System.Random rand = new Random();
        public void UpdatePerSceneMeshVars(CCommandList cmd, UInt32 HitProxyID, UInt32 actorId, Graphics.CGfxCamera ViewerCamera)
        {
            if (mMeshVars.mHitProxyId != HitProxyID)
            {
                mMeshVars.mHitProxyId = HitProxyID;
                mMeshVars.HitProxyColor = CEngine.Instance.HitProxyManager.ConvertHitProxyIdToVector4(HitProxyID);

                mMeshVars.PickedID = (float)HitProxyID;
            }
            if (mMeshVars.mActorId != actorId)
            {
                mMeshVars.mActorId = actorId;
                mMeshVars.ActorIdColor = MeshVars.GetActorIdColorFromActorId(actorId);
            }
        }
        public int FindVar(UInt32 mtlIndex, string name, bool fixMtlName = true)
        {
            if (mtlIndex >= mMtlMeshArray.Length)
                return -1;
            if (mMtlMeshArray[mtlIndex] == null)
                return -1;
            var cb = mMtlMeshArray[mtlIndex].CBuffer;
            if (cb == null)
                return -1;

            if (fixMtlName)
            {
                name = Graphics.CGfxMaterialManager.GetValidShaderVarName(name,
                    mMtlMeshArray[mtlIndex].MtlInst.Material.GetHash64().ToString());
            }

            return cb.FindVar(name);
        }
        public bool SetVar(UInt32 mtlIndex, int varIndex, float value, UInt32 elementIndex)
        {
            if (mtlIndex >= mMtlMeshArray.Length)
                return false;
            if (mMtlMeshArray[mtlIndex] == null)
                return false;
            var cb = mMtlMeshArray[mtlIndex].CBuffer;
            if (cb == null)
                return false;

            return cb.SetValue(varIndex, value, elementIndex);
        }
        public bool SetVar(UInt32 mtlIndex, int varIndex, Vector2 value, UInt32 elementIndex)
        {
            if (mtlIndex >= mMtlMeshArray.Length)
                return false;
            if (mMtlMeshArray[mtlIndex] == null)
                return false;
            var cb = mMtlMeshArray[mtlIndex].CBuffer;
            if (cb == null)
                return false;
            return cb.SetValue(varIndex, value, elementIndex);
        }
        public bool SetVar(UInt32 mtlIndex, int varIndex, Vector3 value, UInt32 elementIndex)
        {
            if (mtlIndex >= mMtlMeshArray.Length)
                return false;
            if (mMtlMeshArray[mtlIndex] == null)
                return false;
            var cb = mMtlMeshArray[mtlIndex].CBuffer;
            if (cb == null)
                return false;
            return cb.SetValue(varIndex, value, elementIndex);
        }
        public bool SetVar(UInt32 mtlIndex, int varIndex, Quaternion value, UInt32 elementIndex)
        {
            if (mtlIndex >= mMtlMeshArray.Length)
                return false;
            if (mMtlMeshArray[mtlIndex] == null)
                return false;
            var cb = mMtlMeshArray[mtlIndex].CBuffer;
            if (cb == null)
                return false;
            return cb.SetValue(varIndex, value, elementIndex);
        }
        public bool SetVar(UInt32 mtlIndex, int varIndex, Color4 value, UInt32 elementIndex)
        {
            if (mtlIndex >= mMtlMeshArray.Length)
                return false;
            if (mMtlMeshArray[mtlIndex] == null)
                return false;
            var cb = mMtlMeshArray[mtlIndex].CBuffer;
            if (cb == null)
                return false;
            return cb.SetValue(varIndex, value, elementIndex);
        }
        public bool SetVar(UInt32 mtlIndex, int varIndex, Matrix value, UInt32 elementIndex)
        {
            if (mtlIndex >= mMtlMeshArray.Length)
                return false;
            if (mMtlMeshArray[mtlIndex] == null)
                return false;
            var cb = mMtlMeshArray[mtlIndex].CBuffer;
            if (cb == null)
                return false;
            return cb.SetValue(varIndex, value, elementIndex);
        }
        public unsafe bool SetVarValue(UInt32 mtlIndex, int index, void* data, int len, UInt32 elementIndex)
        {
            if (mtlIndex >= mMtlMeshArray.Length)
                return false;
            if (mMtlMeshArray[mtlIndex] == null)
                return false;
            var cb = mMtlMeshArray[mtlIndex].CBuffer;
            if (cb == null)
                return false;
            return cb.SetVarValue(index, (byte*)data, len, elementIndex);
        }

        //protected CGfxShadingEnv mShadingEnv;
        //public CGfxShadingEnv ShadingEnv
        //{
        //    get { return mShadingEnv; }
        //}

        public void MdfQueueChanged()
        {
            if (mMtlMeshArray == null)
                return;
            var rc = CEngine.Instance.RenderContext;
            for (int i = 0; i < mMtlMeshArray.Length; i++)
            {
                CGfxMtlMesh MtlMesh = null;
                if (mMtlMeshArray[i] != null)
                {
                    MtlMesh = mMtlMeshArray[i];
                }
                else
                {
                    MtlMesh = new CGfxMtlMesh();
                }

                if (MtlMesh.Init(rc, this, (UInt32)i, MtlMesh.MtlInst, null, false) == false)
                    continue;
            }
        }
        public bool SetMaterialWithEffects(CRenderContext rc, UInt32 index, CGfxMaterialInstance MtlInst, CGfxEffect[] effects, CGfxShadingEnv[] EnvShaderArray)
        {
            if (MtlInst == null)
                return false;

            if (mMtlMeshArray == null)
            {
                return false;
            }

            if (index >= mMtlMeshArray.Length)
            {
                return false;
            }

            CGfxMtlMesh MtlMesh = null;
            if (mMtlMeshArray[index] != null)
            {
                MtlMesh = mMtlMeshArray[index];
            }
            else
            {
                MtlMesh = new CGfxMtlMesh();
            }

            if (MtlMesh.InitByEffects(rc, this, index, MtlInst, effects, EnvShaderArray, true) == false)
                return false;

            mMtlMeshArray[index] = MtlMesh;
            return SetMaterial(index, MtlMesh);
        }
        public async System.Threading.Tasks.Task<bool> SetMaterialInstanceAsync(CRenderContext rc, UInt32 index,
            CGfxMaterialInstance MtlInst, CGfxShadingEnv[] EnvShaderArray, bool preUseEffect = false)
        {
            var ret = SetMaterialInstance(rc, index, MtlInst, EnvShaderArray, preUseEffect);
            await this.AwaitEffects();
            return ret;
        }
        public bool SetMaterialInstance(CRenderContext rc, UInt32 index,
            CGfxMaterialInstance MtlInst, CGfxShadingEnv[] EnvShaderArray,
            bool preUseEffect = false)
        {
            if (MtlInst == null)
                return false;
            if (EnvShaderArray == null)
            {
                EnvShaderArray = CEngine.Instance.PrebuildPassData.DefaultShadingEnvs;
            }

            if (mMtlMeshArray == null)
            {
                return false;
            }

            if (index >= mMtlMeshArray.Length)
            {
                return false;
            }

            CGfxMtlMesh MtlMesh = null;
            if (mMtlMeshArray[index] != null)
            {
                MtlMesh = mMtlMeshArray[index];
            }
            else
            {
                MtlMesh = new CGfxMtlMesh();
            }

            if (MtlMesh.Init(rc, this, index, MtlInst, EnvShaderArray, preUseEffect) == false)
                return false;


            mMtlMeshArray[index] = MtlMesh;
            return SetMaterial(index, MtlMesh);
        }
        public async System.Threading.Tasks.Task AwaitEffects()
        {
            for (int j = 0; j < mMtlMeshArray.Length; j++)
            {
                var MtlMesh = mMtlMeshArray[j];
                if (MtlMesh == null)
                    continue;
                for (int i = 0; i < MtlMesh.PrebuildPassArray.Length; i++)
                {
                    if (MtlMesh.PrebuildPassArray[i] == null)
                        continue;
                    await MtlMesh.PrebuildPassArray[i].Effect.AwaitLoad();
                }
            }
        }
        public async System.Threading.Tasks.Task<bool> LoadMeshAsync(CRenderContext rc, RName name, bool forceLoad = false)
        {
            using (IO.XndHolder xnd = new XndHolder())
            {
                var loadOk = await CEngine.Instance.EventPoster.Post(() =>
                {
                    if (xnd.LoadFromFile(name.Address) == false)
                        return false;

                    var verAtt = xnd.Node.FindAttrib("saveVer");
                    if (verAtt == null)
                    {
                        // 旧版本读取
                        var attr = xnd.Node.FindAttrib("Desc");
                        if (attr != null)
                        {
                            attr.BeginRead();

                            string meshName;
                            attr.Read(out meshName);
                            Name = EngineNS.RName.GetRName(meshName);
                            string geomName;
                            attr.Read(out geomName);
                            MeshPrimitiveName = EngineNS.RName.GetRName(geomName);
                            UInt32 mtlsCount;
                            attr.Read(out mtlsCount);
                            mMaterialNames.Clear();
                            for (UInt32 i = 0; i < mtlsCount; i++)
                            {
                                string mtlName;
                                attr.Read(out mtlName);
                                mMaterialNames.Add(EngineNS.RName.GetRName(mtlName));
                            }

                            attr.EndRead();
                        }
                        return true;
                    }
                    else
                    {
                        verAtt.BeginRead();
                        int ver;
                        verAtt.Read(out ver);
                        verAtt.EndRead();

                        switch (ver)
                        {
                            case 0:
                                {
                                    var att = xnd.Node.FindAttrib("_data");
                                    if (att != null)
                                    {
                                        att.BeginRead();
                                        att.ReadMetaObject(this);
                                        att.EndRead();
                                    }
                                }
                                break;
                        }
                        return true;
                    }
                });

                if (false == loadOk)
                {
                    EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
                    EngineNS.Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Mesh", $"LoadMeshAsync {name.Address} Failed");
                    return false;
                }

                //mShadingEnv = senv;

                mMeshPrimitives = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(rc, MeshPrimitiveName, forceLoad);
                if (mMeshPrimitives == null)
                {
                    EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
                    EngineNS.Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Mesh", $"LoadMeshAsync GetMeshPrimitives {MeshPrimitiveName.Address} Failed");
                    return false;
                }
                SetMeshPrimitives();

                var mdfNode = xnd.Node.FindNode("MdfQueue");
                if (mdfNode != null)
                {
                    MdfQueue = new CGfxMdfQueue();
                    var nodes = mdfNode.GetNodes();
                    foreach (var i in nodes)
                    {
                        var mdfName = i.GetName();
                        var type = Rtti.RttiHelper.GetTypeFromSaveString(mdfName);
                        if (type == null)
                            continue;
                        var modifier = System.Activator.CreateInstance(type) as CGfxModifier;
                        if (modifier == null)
                            continue;
                        if (await modifier.LoadXndAsync(i) == false)
                        {
                            continue;
                        }
                        mMdfQueue.AddModifier(modifier);
                    }
                }
                else
                {
                    MdfQueue = mMeshPrimitives.MdfQueue.CloneMdfQueue(rc, mMdfQueue);
                }
                SDK_GfxMesh_SetGfxMdfQueue(CoreObject, mMdfQueue.CoreObject);
                xnd.Node.TryReleaseHolder();

                var count = SDK_GfxMesh_GetAtomNumber(CoreObject);
                mMtlMeshArray = new CGfxMtlMesh[count];
                for (UInt32 i = 0; i < count; i++)
                {
                    var mtlName = GetMaterialName(i);
                    var mtlInst = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, mtlName);
                    if (mtlInst == null)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Mesh", $"Mesh({name})'s Material({mtlName.Name}) is not found");
                        mtlInst = CEngine.Instance.MaterialInstanceManager.DefaultMaterialInstance;
                    }
                    var mtl = new CGfxMtlMesh();
                    if (mtl.Init(rc, this, i, mtlInst, null, false) == false)
                        continue;
                    mMtlMeshArray[i] = mtl;
                }
            }
            //xnd.Node.TryReleaseHolder();
            //xnd.Dispose();

            if (CEngineDesc.ForceSaveResource)
            {
                this.SaveMesh();
            }
            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);
            return true;
        }
        public void SaveMesh()
        {
            SaveMesh(Name.Address);
        }
        public void SaveMesh(string absPath)
        {
            if (OrigionMesh != null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Mesh", $"CGfxMesh is a cloned mesh, Engine Can't SaveMesh!");
                return;
            }
            var xnd = IO.XndHolder.NewXNDHolder();
            var verAtt = xnd.Node.AddAttrib("saveVer");
            verAtt.BeginWrite();
            verAtt.Write((int)0);
            verAtt.EndWrite();

            var attr = xnd.Node.AddAttrib("_data");
            attr.BeginWrite();
            attr.WriteMetaObject(this);
            attr.EndWrite();

            if (mMdfQueue.MdfNumber > 0)
            {
                var mdfNode = xnd.Node.AddNode("MdfQueue", 0, 0);
                for (int i = 0; i < mMdfQueue.MdfNumber; i++)
                {
                    var mod = mMdfQueue.Modifiers[i];
                    var typeStr = Rtti.RttiHelper.GetTypeSaveString(mod.GetType());
                    var cnode = mdfNode.AddNode(typeStr, 0, 0);
                    mod.Save2Xnd(cnode);
                }
            }

            IO.XndHolder.SaveXND(absPath, xnd);
            SaveSerialId++;
        }

        protected CGfxMeshPrimitives mMeshPrimitives;
        public CGfxMeshPrimitives MeshPrimitives
        {
            get { return mMeshPrimitives; }
        }
        protected CGfxMdfQueue mMdfQueue;
       
        public CGfxMdfQueue MdfQueue
        {
            get { return mMdfQueue; }
            protected set
            {
                mMdfQueue = value;
                if (mMdfQueue != null)
                {
                    mMdfQueue.HostMesh = this;
                }
            }
        }
        protected CGfxMtlMesh[] mMtlMeshArray;
        
        public CGfxMtlMesh[] MtlMeshArray
        {
            get { return mMtlMeshArray; }
        }

        protected CShaderResources mShaderResources;
        public CShaderResources ShaderResources
        {//update after shading env,materalinstance,mdfqueue modified
            get { return mShaderResources; }
        }

        public async System.Threading.Tasks.Task<CGfxMesh> CloneMesh(CRenderContext rc/*, CGfxShadingEnv senv*/)
        {
            var newObject = new CGfxMesh();
            newObject.OrigionMesh = this;
            //newObject.mShadingEnv = senv;
            newObject.SaveSerialId = 0;//SaveSerialId;

            await newObject.UpdatedOrigionMesh(rc);
            return newObject;
        }
        public bool Editor_NeedUpdateOrigionMesh()
        {
            if (OrigionMesh == null)
                return false;
            if (SaveSerialId == OrigionMesh.SaveSerialId)
                return false;
            return true;
        }
        public int Editor_GetSnapshortFrameNumber()
        {
            var ptcModifier = this.MdfQueue.FindModifier<Bricks.Particle.CGfxParticleModifier>();
            if (ptcModifier != null)
                return 16;

            var skinModifier = this.MdfQueue.FindModifier<CGfxSkinModifier>();
            if (skinModifier!=null)
                return 8;

            return 1;
        }
        public async System.Threading.Tasks.Task<bool> UpdatedOrigionMesh(CRenderContext rc, List<GamePlay.Component.GMeshComponent.GMeshComponentInitializer.MtlInst> privateMtls=null)
        {
            if (Editor_NeedUpdateOrigionMesh() == false)
                return false;

            SaveSerialId = OrigionMesh.SaveSerialId;
            mMeshPrimitives = OrigionMesh.mMeshPrimitives;
            SetMeshPrimitives();

            mName = OrigionMesh.Name;

            MdfQueue = OrigionMesh.mMdfQueue.CloneMdfQueue(rc, mMdfQueue);
            SDK_GfxMesh_SetGfxMdfQueue(CoreObject, mMdfQueue.CoreObject);

            var count = (UInt32)OrigionMesh.mMtlMeshArray.Length;
            mMtlMeshArray = new CGfxMtlMesh[count];
            for (UInt32 i = 0; i < count; i++)
            {
                if (OrigionMesh.mMtlMeshArray[i] == null)
                    continue;
                var mtl = new CGfxMtlMesh();
                var mtlInst = OrigionMesh.mMtlMeshArray[i].MtlInst;
                if(privateMtls!=null && i <= privateMtls.Count &&
                     privateMtls[(int)i].IsSetName())
                {
                    mtlInst = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, privateMtls[(int)i].Name);
                }
                if (mtl.Init(rc, this, i, mtlInst, null, false) == false)
                    continue;
                mMtlMeshArray[i] = mtl;
                SetMaterial(i, mtl);
            }
            return true;
        }
        
        public void SetPassUserFlags(UInt32 flags)
        {
            if (mMtlMeshArray == null)
                return;
            for (int i = 0; i < mMtlMeshArray.Length; i++)
            {
                if (mMtlMeshArray[i] == null)
                    continue;
                for (int j = 0; j < mMtlMeshArray[i].PrebuildPassArray.Length; j++)
                {
                    if (mMtlMeshArray[i].PrebuildPassArray[j] == null)
                        continue;
                    mMtlMeshArray[i].PrebuildPassArray[j].UserFlags = flags;
                }
            }
        }

        public void SetLodLevel(float lod)
        {
            if (mMtlMeshArray == null)
                return;
            for (int i = 0; i < mMtlMeshArray.Length; i++)
            {
                if (mMtlMeshArray[i] == null)
                    continue;
                for (int j = 0; j < mMtlMeshArray[i].PrebuildPassArray.Length; j++)
                {
                    if (mMtlMeshArray[i].PrebuildPassArray[j] == null)
                        continue;
                    mMtlMeshArray[i].PrebuildPassArray[j].LodLevel = lod;
                }
            }
        }

        #region Macross
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public async System.Threading.Tasks.Task<bool> McSetMaterial(UInt32 index,
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.MaterialInstance)]
            RName name)
        {
            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(CEngine.Instance.RenderContext, name);
            if (mtl == null)
                return false;
            var ret = SetMaterialInstance(CEngine.Instance.RenderContext, index, mtl, CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
            await AwaitEffects();
            return ret;
        }

        //初始化的时候用，不能经常调用，性能不好
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable, "为模型的某个材质的指定纹理变量设置纹理，通常初始化的时候用，不能经常调用，性能不好")]
        public void McSetTexture(UInt32 mtlIndex, string shaderName,
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Texture)]
            RName name)
        {
            if (mtlIndex >= this.MtlMeshArray.Length)
                return;
            var texture = CEngine.Instance.TextureManager.GetShaderRView(CEngine.Instance.RenderContext, name);
            var mtlMesh = this.MtlMeshArray[mtlIndex];
            for (int i = 0; i < mtlMesh.PrebuildPassArray.Length; i++)
            {
                var pass = mtlMesh.PrebuildPassArray[i];
                if (pass == null)
                    continue;
                pass.Effect.PreUse((successed) =>
                {
                    if (successed == false)
                        return;
                    var slot = pass.Effect.ShaderProgram.FindTextureIndexPS(mtlMesh.MtlInst, shaderName);
                    if (slot <= 16)
                    {
                        pass.ShaderResources.SetUserControlTexture(slot, true);
                        pass.ShaderResources.PSBindTexture(slot, texture);
                    }
                });
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int McFindVar(UInt32 mtlIndex, string name)
        {
            return FindVar(mtlIndex, name, true);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool McSetVarFloat(UInt32 mtlIndex, int varIndex, float value, UInt32 elementIndex)
        {
            return SetVar(mtlIndex, varIndex, value, elementIndex);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool McSetVarVector2(UInt32 mtlIndex, int varIndex, Vector2 value, UInt32 elementIndex)
        {
            return SetVar(mtlIndex, varIndex, value, elementIndex);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool McSetVarVector3(UInt32 mtlIndex, int varIndex, Vector3 value, UInt32 elementIndex)
        {
            return SetVar(mtlIndex, varIndex, value, elementIndex);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool McSetVarQuaternion(UInt32 mtlIndex, int varIndex, Quaternion value, UInt32 elementIndex)
        {
            return SetVar(mtlIndex, varIndex, value, elementIndex);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool McSetVarColor4(UInt32 mtlIndex, int varIndex, Color4 value, UInt32 elementIndex)
        {
            return SetVar(mtlIndex, varIndex, value, elementIndex);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool McSetVarMatrix(UInt32 mtlIndex, int varIndex, Matrix value, UInt32 elementIndex)
        {
            return SetVar(mtlIndex, varIndex, value, elementIndex);
        }
        #endregion

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMesh_Init(NativePointer self, string name, CGfxMeshPrimitives.NativePointer geom, CGfxMdfQueue.NativePointer mdf);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxMesh_SetMaterial(NativePointer self, UInt32 index, CGfxMtlMesh.NativePointer material);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMesh_SetMeshPrimitives(NativePointer self, CGfxMeshPrimitives.NativePointer geom);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMesh_SetGfxMdfQueue(NativePointer self, CGfxMdfQueue.NativePointer mdf);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxMesh_Save2Xnd(NativePointer self, IO.XndNode.NativePointer node);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxMesh_LoadXnd(NativePointer self, IO.XndNode.NativePointer node);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        //public extern static string SDK_GfxMesh_GetGeomName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxMesh_GetAtomNumber(NativePointer self);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        //public extern static string SDK_GfxMesh_GetMaterailName(NativePointer self, UInt32 index);

        #endregion
    }

    public partial class CGfxMeshManager
    {
        public Dictionary<RName, CGfxMesh> FirstMeshes
        {
            get;
        } = new Dictionary<RName, CGfxMesh>(new RName.EqualityComparer());
        public void Cleanup()
        {
            FirstMeshes.Clear();
        }
        public void RemoveMesh(RName name)
        {
            lock (FirstMeshes)
            {
                if (FirstMeshes.ContainsKey(name))
                {
                    FirstMeshes.Remove(name);
                }
            }
        }
        public async System.Threading.Tasks.Task<CGfxMesh> GetMeshOrigion(CRenderContext rc, RName name/*, CGfxShadingEnv senv*/)
        {
            if (name.IsExtension(CEngineDesc.MeshExtension) == false)
                return null;
            CGfxMesh first;
            bool finded = false;
            lock (FirstMeshes)
            {
                if (FirstMeshes.TryGetValue(name, out first) == false)
                {
                    first = new CGfxMesh();
                    FirstMeshes.Add(name, first);
                }
                else
                    finded = true;
            }
            if (finded)
            {
                var context = await first.AwaitLoad();
                if (context != null && context.Result == null)
                    return null;
                return first;
            }

            if (false == await first.LoadMeshAsync(rc, name/*, senv*/))
                return null;
            return first;
        }
        public async System.Threading.Tasks.Task<CGfxMesh> CreateMeshAsync(CRenderContext rc, RName name, bool useClone = true, bool forceLoad = false)
        {
            if (name.IsExtension(CEngineDesc.MeshExtension) == false)
                return null;
            CGfxMesh first;
            bool finded = false;
            lock (FirstMeshes)
            {
                if (FirstMeshes.TryGetValue(name, out first) == false)
                {
                    first = new CGfxMesh();
                    FirstMeshes.Add(name, first);
                }
                else
                    finded = true;
            }
            if (finded)
            {
                var context = await first.AwaitLoad();
                if (context != null && context.Result == null)
                    return null;
                if (useClone)
                    return await first.CloneMesh(rc);
                else
                    return first;
            }

            if (false == await first.LoadMeshAsync(rc, name, forceLoad))
                return null;
            if (useClone)
                return await first.CloneMesh(rc/*, senv*/);
            else
                return first;
        }
        public CGfxMesh CreateMesh(CRenderContext rc, CGfxMeshPrimitives mesh/*, CGfxShadingEnv senv*/)
        {
            if (mesh == null)
                return null;

            var first = new CGfxMesh();
            if (first.Init(rc, RName.GetRName(null), mesh/*, senv*/) == false)
                return null;
            return first;
        }
        public CGfxMesh CreateMesh(CRenderContext rc, RName name, CGfxMeshPrimitives mesh/*, CGfxShadingEnv senv*/)
        {
            if (mesh == null)
                return null;

            var first = new CGfxMesh();
            if (first.Init(rc, name, mesh) == false)
                return null;
            return first;
        }
        public CGfxMesh NewMesh(CRenderContext rc, RName name, RName vmsFile/*, CGfxShadingEnv senv*/)
        {
            if (name.IsExtension(CEngineDesc.MeshExtension) == false)
                return null;
            CGfxMesh first;
            if (FirstMeshes.TryGetValue(name, out first) == true)
            {
                return null;
            }

            var meshPrimitives = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(rc, vmsFile);
            if (meshPrimitives == null)
                return null;

            first = new CGfxMesh();
            if (first.Init(rc, name, meshPrimitives/*, senv*/) == false)
                return null;

            first.SaveMesh();
            FirstMeshes.Add(name, first);

            return first;
        }
        public void GetReferences(CGfxMeshPrimitives template, List<CGfxMesh> outMeshes, Dictionary<RName, RName> outSRVs = null)
        {
            foreach (var i in FirstMeshes)
            {
                if (i.Value.MeshPrimitives == template)
                {
                    outMeshes.Add(i.Value);
                }
            }

            if (outSRVs != null)
            {
                foreach (var i in outMeshes)
                {
                    foreach (var j in i.MtlMeshArray)
                    {
                        for (UInt32 k = 0; k < j.MtlInst.SRVNumber; k++)
                        {
                            var srvName = j.MtlInst.GetSRVName(k);
                            outSRVs[srvName] = srvName;
                        }
                    }
                }
            }
        }
    }
}
