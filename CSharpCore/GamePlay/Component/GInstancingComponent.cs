using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GInstancingComponentInitializer), "实例化组件", "Mesh", "InstancingComponent")]
    public class GInstancingComponent : GComponent
    {
        public class GInstancingComponentInitializer : GComponentInitializer
        {
            public Vector4 mF41;
            [Rtti.MetaData]
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public Vector4 F41
            {
                get { return mF41; }
                set { mF41 = value; }
            }
        }
        public GInstancingComponent()
        {
            OnlyForGame = false;
        }
        public GInstancingComponentInitializer GetInstInitializer()
        {
            return Initializer as GInstancingComponentInitializer;
        }
        public Vector4 F41
        {
            get { return GetInstInitializer().F41; }
            set { GetInstInitializer().F41 = value; }
        }
        //public U32_4 mLightIndices;
        public override void OnAdded()
        {
            Host.mComponentFlags |= Actor.GActor.EComponentFlags.HasInstancing;
            var hostMesh = HostContainer as GMeshComponent;
            if (hostMesh!=null)
            {
                hostMesh.mInstancingComp = this;
            }
            base.OnAdded();
        }
        public override void OnRemove()
        {
            Host.mComponentFlags &= (~Actor.GActor.EComponentFlags.HasInstancing);
            var hostMesh = HostContainer as GMeshComponent;
            if (hostMesh != null)
            {
                hostMesh.mInstancingComp = null;
            }
            base.OnRemove();
        }
    }
    public class CGfxInstancingModifier : Graphics.Mesh.CGfxModifier
    {
        private GInstancing mHost;
        private GInstancing mShadowHost;
        public void SetHost(GInstancing host)
        {
            mHost = host;
        }
        public void SetShadowHost(GInstancing host)
        {
            mShadowHost = host;
        }
        public CGfxInstancingModifier()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("GfxInstancingModifier");
            Name = Rtti.RttiHelper.GetTypeSaveString(typeof(CGfxInstancingModifier));
            ShaderModuleName = RName.GetRName("Shaders/Modifier/InstancingModifier");
        }
        public CGfxInstancingModifier(NativePointer self)
        {
            mCoreObject = self;
            Name = Rtti.RttiHelper.GetTypeSaveString(typeof(CGfxInstancingModifier));
            ShaderModuleName = RName.GetRName("Shaders/Modifier/InstancingModifier");
        }
        public override Graphics.Mesh.CGfxModifier CloneModifier(CRenderContext rc)
        {
            var result = new CGfxInstancingModifier();
            result.Name = Name;
            return result;
            //var ptr = SDK_GfxModifier_CloneModifier(CoreObject, rc.CoreObject);
            //var result = new CGfxInstancingModifier(ptr);
            //result.Name = Name;
            //return result;
        }
        public override string FunctionName
        {
            get { return "DoInstancingModifierVS"; }
        }
        public override void OnSetPassData(CPass pass, bool shadow)
        {
            if(shadow)
                mShadowHost.BindBuffers(pass);
            else
                mHost.BindBuffers(pass);
        }
    }
    public class GInstancing
    {
        public Actor.GActor MeshActor
        {
            get;
            protected set;
        }
        public GMeshComponent MeshComp
        {
            get;
            protected set;
        }
        const int DefaultMaxNumber = 64;
        public async System.Threading.Tasks.Task Init(RName name, bool shadow)
        {
            mMaxNumber = DefaultMaxNumber;
            mInstDataArray = new VSInstantData[mMaxNumber];
            mPosData = new Vector3[mMaxNumber];
            mScaleData = new Vector4[mMaxNumber];
            mRotateData = new Quaternion[mMaxNumber];
            mF41Data = new UInt32_4[mMaxNumber];

            unsafe
            {
                var rc = CEngine.Instance.RenderContext;
                CVertexBufferDesc desc = new CVertexBufferDesc();
                desc.CPUAccess = (UInt32)ECpuAccess.CAS_WRITE;
                desc.ByteWidth = (UInt32)(sizeof(Vector3) * mMaxNumber);
                desc.Stride = (UInt32)sizeof(Vector3);
                mPosVB = rc.CreateVertexBuffer(desc);

                desc.ByteWidth = (UInt32)(sizeof(Vector4) * mMaxNumber);
                desc.Stride = (UInt32)sizeof(Vector4);
                mScaleVB = rc.CreateVertexBuffer(desc);

                desc.ByteWidth = (UInt32)(sizeof(Quaternion) * mMaxNumber);
                desc.Stride = (UInt32)sizeof(Quaternion);
                mRotateVB = rc.CreateVertexBuffer(desc);

                desc.ByteWidth = (UInt32)(sizeof(Vector4) * mMaxNumber);
                desc.Stride = (UInt32)sizeof(Vector4);
                mF41VB = rc.CreateVertexBuffer(desc);

                var bfDesc = new CGpuBufferDesc();
                bfDesc.SetMode(true, false);
                bfDesc.ByteWidth = (uint)(mMaxNumber * sizeof(VSInstantData));
                bfDesc.StructureByteStride = (uint)sizeof(VSInstantData);
                mInstDataBuffer = rc.CreateGpuBuffer(bfDesc, IntPtr.Zero);

                var srvDesc = new ISRVDesc();
                srvDesc.ToDefault();
                srvDesc.ViewDimension = EResourceDimension.RESOURCE_DIMENSION_BUFFER;
                srvDesc.Buffer.ElementOffset = 0;
                srvDesc.Buffer.NumElements = (uint)mMaxNumber;
                mInstDataView = rc.CreateShaderResourceViewFromBuffer(mInstDataBuffer, srvDesc);
            }

            MeshActor = await Actor.GActor.NewMeshActorAsync(name);
            MeshComp = MeshActor.GetComponent<GMeshComponent>();
            var modifier = MeshComp.SceneMesh.MdfQueue.FindModifier<CGfxInstancingModifier>();
            if (modifier == null)
            {
                modifier = new CGfxInstancingModifier();
                MeshComp.SceneMesh.MdfQueue.AddModifier(modifier);
            }
            if (shadow)
                modifier.SetShadowHost(this);
            else
                modifier.SetHost(this);
            
            MeshComp.IsIdentityDrawTransform = true;
            MeshActor.Placement.Location = Vector3.Zero;
            CEngine.Instance.HitProxyManager.MapActor(MeshActor);

            mAttachVBs.BindVertexBuffer(EVertexSteamType.VST_InstPos, mPosVB);
            mAttachVBs.BindVertexBuffer(EVertexSteamType.VST_InstScale, mScaleVB);
            mAttachVBs.BindVertexBuffer(EVertexSteamType.VST_InstQuat, mRotateVB);
            mAttachVBs.BindVertexBuffer(EVertexSteamType.VST_F4_1, mF41VB);

            mAttachSRVs.VSBindTexture(13, mInstDataView);
        }

        CShaderResources mAttachSRVs = new CShaderResources();
        CVertexArray mAttachVBs = new CVertexArray();
        CGpuBuffer mInstDataBuffer;
        CShaderResourceView mInstDataView;
        CVertexBuffer mPosVB;
        CVertexBuffer mScaleVB;
        CVertexBuffer mRotateVB;
        CVertexBuffer mF41VB;

        UInt32 mCurSize;
        public UInt32 CurSize
        {
            get { return mCurSize; }
        }
        UInt32 mMaxNumber = 0;
        Vector3[] mPosData = null;
        Vector4[] mScaleData = null;
        Quaternion[] mRotateData = null;
        UInt32_4[] mF41Data = null;

        struct VSInstantData
        {
            public Matrix WorldMatrix;
            public UInt32_4 CustomData;
            public UInt32_4 PointLightIndices;
        }
        VSInstantData[] mInstDataArray;
        public void Reset()
        {
            mCurSize = 0;
        }
        public void PushInstance(ref Vector3 pos, ref Vector3 scale, ref Quaternion quat, ref UInt32_4 f41, int lightNum)
        {
            var rc = CEngine.Instance.RenderContext;
            if (mCurSize == mMaxNumber)
            {
                var savedNum = mMaxNumber;
                mMaxNumber = mCurSize * 2;
                VSInstantData[] newInstData = null;
                Vector3[] newPos = null;
                Vector4[] newScale = null;
                Quaternion[] newQuat = null;
                UInt32_4[] newF41 = null;

                if(CRenderContext.ShaderModel >= 4)
                {
                    newInstData = new VSInstantData[mMaxNumber];
                }
                else
                {
                    newPos = new Vector3[mMaxNumber];
                    newScale = new Vector4[mMaxNumber];
                    newQuat = new Quaternion[mMaxNumber];
                    newF41 = new UInt32_4[mMaxNumber];
                }

                unsafe
                {
                    if (CRenderContext.ShaderModel >= 4)
                    {
                        fixed (VSInstantData* src = &mInstDataArray[0])
                        fixed (VSInstantData* dest = &newInstData[0])
                        {
                            CoreSDK.SDK_Memory_Copy(dest, src, (UInt32)(sizeof(VSInstantData) * savedNum));
                        }

                        var bfDesc = new CGpuBufferDesc();
                        bfDesc.SetMode(true, false);
                        bfDesc.ByteWidth = (uint)(mMaxNumber * sizeof(VSInstantData));
                        bfDesc.StructureByteStride = (uint)sizeof(VSInstantData);
                        mInstDataBuffer = rc.CreateGpuBuffer(bfDesc, IntPtr.Zero);

                        var srvDesc = new ISRVDesc();
                        srvDesc.ToDefault();
                        srvDesc.ViewDimension = EResourceDimension.RESOURCE_DIMENSION_BUFFER;
                        srvDesc.Buffer.ElementOffset = 0;
                        srvDesc.Buffer.NumElements = (uint)mMaxNumber;
                        mInstDataView = rc.CreateShaderResourceViewFromBuffer(mInstDataBuffer, srvDesc);

                        mAttachSRVs.VSBindTexture(13, mInstDataView);
                    }
                    else
                    {
                        fixed (Vector3* src = &mPosData[0])
                        fixed (Vector3* dest = &newPos[0])
                        {
                            CoreSDK.SDK_Memory_Copy(dest, src, (UInt32)(sizeof(Vector3) * savedNum));
                        }

                        fixed (Vector4* src = &mScaleData[0])
                        fixed (Vector4* dest = &newScale[0])
                        {
                            CoreSDK.SDK_Memory_Copy(dest, src, (UInt32)(sizeof(Vector3) * savedNum));
                        }

                        fixed (Quaternion* src = &mRotateData[0])
                        fixed (Quaternion* dest = &newQuat[0])
                        {
                            CoreSDK.SDK_Memory_Copy(dest, src, (UInt32)(sizeof(Quaternion) * savedNum));
                        }

                        fixed (UInt32_4* src = &mF41Data[0])
                        fixed (UInt32_4* dest = &newF41[0])
                        {
                            CoreSDK.SDK_Memory_Copy(dest, src, (UInt32)(sizeof(UInt32_4) * savedNum));
                        }

                        CVertexBufferDesc desc = new CVertexBufferDesc();
                        desc.ByteWidth = (UInt32)(sizeof(Vector3) * mMaxNumber);
                        desc.Stride = (UInt32)sizeof(Vector3);
                        mPosVB = rc.CreateVertexBuffer(desc);

                        desc.ByteWidth = (UInt32)(sizeof(Vector4) * mMaxNumber);
                        desc.Stride = (UInt32)sizeof(Vector4);
                        mScaleVB = rc.CreateVertexBuffer(desc);

                        desc.ByteWidth = (UInt32)(sizeof(Quaternion) * mMaxNumber);
                        desc.Stride = (UInt32)sizeof(Quaternion);
                        mRotateVB = rc.CreateVertexBuffer(desc);

                        desc.ByteWidth = (UInt32)(sizeof(Vector4) * mMaxNumber);
                        desc.Stride = (UInt32)sizeof(Vector4);
                        mF41VB = rc.CreateVertexBuffer(desc);

                        mAttachVBs.BindVertexBuffer(EVertexSteamType.VST_InstPos, mPosVB);
                        mAttachVBs.BindVertexBuffer(EVertexSteamType.VST_InstScale, mScaleVB);
                        mAttachVBs.BindVertexBuffer(EVertexSteamType.VST_InstQuat, mRotateVB);
                        mAttachVBs.BindVertexBuffer(EVertexSteamType.VST_F4_1, mF41VB);
                    }
                }

                mInstDataArray = newInstData;
                mPosData = newPos;
                mScaleData = newScale;
                mRotateData = newQuat;
                mF41Data = newF41;
            }

            if (CRenderContext.ShaderModel >= 4)
            {
                mInstDataArray[mCurSize].WorldMatrix = Matrix.Transformation(scale, quat, pos);
                mInstDataArray[mCurSize].WorldMatrix.Transpose();
                mInstDataArray[mCurSize].CustomData.x = (uint)lightNum;
                mInstDataArray[mCurSize].PointLightIndices = f41;
            }
            else
            {
                mPosData[mCurSize] = pos;
                mScaleData[mCurSize].X = scale.X;
                mScaleData[mCurSize].Y = scale.Y;
                mScaleData[mCurSize].Z = scale.Z;
                mScaleData[mCurSize].W = lightNum;
                mRotateData[mCurSize] = quat;
                mF41Data[mCurSize] = f41;
            }
            mCurSize++;
        }
        public void Flush2VB(CCommandList cmd)
        {
            mAttachVBs.NumInstances = mCurSize;
            if (mCurSize == 0)
                return;
            
            var rc = CEngine.Instance.RenderContext;
            unsafe
            {
                if (CRenderContext.ShaderModel >= 4)
                {
                    fixed (VSInstantData* p = &mInstDataArray[0])
                    {
                        mInstDataBuffer.UpdateBufferData(cmd, (IntPtr)p, (UInt32)(sizeof(VSInstantData) * mCurSize));
                    }
                }
                else
                {
                    fixed (Vector3* p = &mPosData[0])
                    {
                        mPosVB.UpdateBuffData(cmd, (IntPtr)p, (UInt32)(sizeof(Vector3) * mCurSize));
                    }
                    fixed (Vector4* p = &mScaleData[0])
                    {
                        mScaleVB.UpdateBuffData(cmd, (IntPtr)p, (UInt32)(sizeof(Vector4) * mCurSize));
                    }
                    fixed (Quaternion* p = &mRotateData[0])
                    {
                        mRotateVB.UpdateBuffData(cmd, (IntPtr)p, (UInt32)(sizeof(Quaternion) * mCurSize));
                    }
                    fixed (UInt32_4* p = &mF41Data[0])
                    {
                        mF41VB.UpdateBuffData(cmd, (IntPtr)p, (UInt32)(sizeof(UInt32_4) * mCurSize));
                    }
                }
            }
        }

        public void BindBuffers(CPass pass)
        {
            pass.AttachSRVs = mAttachSRVs;
            pass.AttachVBs = mAttachVBs;
        }
    }
    public class GInstancingManager
    {
        public enum EPoolType
        {
            Normal,
            Shadow,
        }
        public Dictionary<RName, GInstancing> Meshes
        {
            get;
        } = new Dictionary<RName, GInstancing>(new RName.EqualityComparer());
        public Dictionary<RName, GInstancing> ShadowMeshes
        {
            get;
        } = new Dictionary<RName, GInstancing>(new RName.EqualityComparer());
        public void Reset(EPoolType poolType)
        {
            switch (poolType)
            {
                case EPoolType.Normal:
                    {
                        using (var i = Meshes.Values.GetEnumerator())
                        {
                            while (i.MoveNext())
                            {
                                i.Current.Reset();
                            }
                        }
                    }
                    break;
                case EPoolType.Shadow:
                    {
                        using (var i = ShadowMeshes.Values.GetEnumerator())
                        {
                            while (i.MoveNext())
                            {
                                i.Current.Reset();
                            }
                        }
                    }
                    break;
            }
        }
        //实测，在android上，性能分析工具调用本身开心就很大
        //public static Profiler.TimeScope ScopeFind = Profiler.TimeScopeManager.GetTimeScope(typeof(GInstancingManager), "PushInstance.Find", Profiler.TimeScope.EProfileFlag.Windows);
        //public static Profiler.TimeScope ScopePush = Profiler.TimeScopeManager.GetTimeScope(typeof(GInstancingManager), "PushInstance.Push", Profiler.TimeScope.EProfileFlag.Windows);
        public void PushInstance(GInstancingComponent inst, int lightNum, ref UInt32_4 lightIndices, Graphics.Mesh.CGfxMesh oriMesh, EPoolType poolType)
        {
            var meshComp = inst.HostContainer as GMeshComponent;
            if (meshComp == null)
                return;
            //ScopeFind.Begin();
            if(oriMesh.mInstancePool==null || oriMesh.mShadowInstancePool == null)
            {
                oriMesh.mInstancePool = new GInstancing();
                Meshes[oriMesh.Name] = oriMesh.mInstancePool;
                
                var task = oriMesh.mInstancePool.Init(oriMesh.Name, false);

                oriMesh.mShadowInstancePool = new GInstancing();
                ShadowMeshes[oriMesh.Name] = oriMesh.mShadowInstancePool;

                var task1 = oriMesh.mShadowInstancePool.Init(oriMesh.Name, true);
            }
            GInstancing instType = null;
            switch(poolType)
            {
                case EPoolType.Normal:
                    instType = oriMesh.mInstancePool;
                    break;
                case EPoolType.Shadow:
                    instType = oriMesh.mShadowInstancePool;
                    break;
            }
            //ScopeFind.End();

            //ScopePush.Begin();
            instType.PushInstance(ref meshComp.Placement.mDrawPosition,
                ref meshComp.Placement.mDrawScale,
                ref meshComp.Placement.mDrawRotation,
                ref lightIndices,
                lightNum);
            //ScopePush.End();
        }
        public void Commit(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param, EPoolType poolType)
        {
            switch (poolType)
            {
                case EPoolType.Normal:
                    {
                        using (var i = Meshes.Values.GetEnumerator())
                        {
                            while (i.MoveNext())
                            {
                                if (i.Current.CurSize == 0)
                                    continue;

                                i.Current.MeshComp.CommitVisual(cmd, camera, param);
                                i.Current.Flush2VB(cmd);
                            }
                        }
                    }
                    break;
                case EPoolType.Shadow:
                    {
                        using (var i = ShadowMeshes.Values.GetEnumerator())
                        {
                            while (i.MoveNext())
                            {
                                if (i.Current.CurSize == 0)
                                    continue;

                                i.Current.MeshComp.CommitVisual(cmd, camera, param);
                                i.Current.Flush2VB(cmd);
                            }
                        }
                    }
                    break;
            }
        }
    }

    partial class GMeshComponent
    {
        internal GInstancingComponent mInstancingComp;
    }
}

namespace EngineNS.Graphics.Mesh
{   
    partial class CGfxMesh
    {
        public GamePlay.Component.GInstancing mInstancePool;
        public GamePlay.Component.GInstancing mShadowInstancePool;
    }
    partial class CGfxMeshManager
    {
        public GamePlay.Component.GInstancingManager InstancingManager
        {
            get;
        } = new GamePlay.Component.GInstancingManager();
    }
}
