using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.Graphics.Pipeline.Deferred;
using EngineNS.NxRHI;
using EngineNS.Thread;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public struct FVisibleMesh
    {
        public enum EDrawMode
        {
            Normal,
            Instance,
        }
        public EDrawMode DrawMode;
        public Mesh.TtRenderMesh Mesh;
    }
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "EShadowMode")]
    public enum EShadowMode : uint
    {
        None = 0,
        Csm,
        Advance,
        Num,
    }
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtRenderPolicy : TtRenderGraph, IO.ISerializer
    {
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }
        public void OnPropertyRead(object tagObject, string prop, bool fromXml)
        {

        }
        public void OnPostRead(object tagObject, object hostObject, bool fromXml) { }
        public void OnPropertyWrite(string prop, bool fromXml)
        {

        }
        #endregion
        #region Feature On/Off
        [Rtti.Meta("")]
        [Category("Feature")]
        public bool EnableLocalLights
        {
            get;
            set;
        } = true;
        [Rtti.Meta("")]
        [Category("Feature")]
        public bool EnableAO
        {
            get;
            set;
        } = true;
        [Rtti.Meta("")]
        [Category("Feature")]
        public bool EnableGI
        {
            get;
            set;
        } = true;
        /// <summary>
        /// 延迟贴花总开关。关掉时 TtDecalPassNode 不 dispatch, DBuffer 用黑图占位
        /// (alpha = 覆盖权重 = 0, 等价于场景里没有贴花)。
        /// </summary>
        [Rtti.Meta("")]
        [Category("Feature")]
        public bool EnableDecal
        {
            get;
            set;
        } = true;
        /// <summary>
        /// 屏幕空间反射总开关。关掉时 TtSSRNode 不 dispatch, 反射输出用黑图占位。
        /// </summary>
        [Rtti.Meta("")]
        [Category("Feature")]
        public bool EnableSSR
        {
            get;
            set;
        } = true;
        [Rtti.Meta("")]
        [Category("Feature")]
        public EShadowMode ShadowMode
        {
            get;
            set;
        } = EShadowMode.Advance;
        [Rtti.Meta("")]
        [Category("Feature")]
        public Deferred.EContactShadowMode ContactShadowMode
        {
            get;
            set;
        } = Deferred.EContactShadowMode.InputNode;
        [Category("Feature")]
        public TtAntiAliasingNode.ETypeAA TypeAA
        {
            get;
            set;
        } = TtAntiAliasingNode.ETypeAA.Taa;
        [Category("Feature")]
        [ReadOnly(true)]
        public bool EnableSeparatedSSS
        {
            get
            {
                var aa = this.FindNode<TtSSSBlurNode>();
                if (aa != null)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion
        public TtRenderPolicy()
        {
            NodeList.Host = this;
        }
        public RName RPolicyName { get; set; }
        public static async Thread.Async.TtTask<TtRenderPolicy> CreatRenderPolicy(RName name)
        {
            var policy = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.CreateRenderPolicy(name, null);
            await policy.Initialize(null);
            return policy;
        }
        public override void Dispose()
        {
            //foreach(var i in VisibleMeshes)
            //{
            //    i.Dispose();
            //}
            foreach (var i in Fences)
            {
                i.Value.Dispose();
            }
            Fences.Clear();
            base.Dispose();
        }
        [Category("Option")]
        public bool IsSyncBuildDrawcall { get; set; } = false;
        [Category("Option")]
        public bool IsImmediateFlushCBuffer { get; set; } = false;
        public class TtNodeListDefine
        {
            internal TtRenderPolicy Host;
            public class TtValueEditorAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
            {
                public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
                {
                    newValue = info.Value;
                    var nodeDef = newValue as TtNodeListDefine;
                    var Host = nodeDef.Host;
                    if (Host == null)
                        return false;

                    if (Host.NodeLayers != null)
                    {
                        foreach (var i in Host.NodeLayers)
                        {
                            foreach (var j in i)
                            {
                                ImGuiAPI.Text(j.Name);
                            }
                        }
                    }
                    return false;
                }
            }
        }
        [TtNodeListDefine.TtValueEditor]
        [Category("Option")]
        public TtNodeListDefine NodeList
        {
            get;
        } = new TtNodeListDefine();
        [Category("Option")]
        public List<TtRenderGraphNode> GraphNodeList
        {
            get => GraphNodes.Values.ToList();
        }
        //TagObject通常用来处理ShadingEnv.OnDrawCall的特殊参数设置
        //public TtRenderGraphNode TagObject;
        public object TagObject;
        protected TtCamera mDefaultCamera;
        public IRenderViewport RenderViewport
        {
            get;
            internal set;
        }
        [Rtti.Meta("")]
        public TtWorld GetWorld()
        {
            return RenderViewport?.World;
        }
        public bool IsInitialized { get; set; } = false;
        public TtCamera DefaultCamera { get => mDefaultCamera; }
        public Dictionary<string, TtCamera> CameraAttachments { get; } = new Dictionary<string, TtCamera>();
        public Dictionary<string, TtFence> Fences { get; } = new Dictionary<string, TtFence>();
        public TtFence FindFence(string name)
        {
            TtFence result;
            if (Fences.TryGetValue(name, out result))
                return result;
            return null;
        }
        public TtFence FindOrCreateFence(string name)
        {
            TtFence result;
            if (Fences.TryGetValue(name, out result))
                return result;
            FFenceDesc desc = new FFenceDesc();
            result = TtEngine.Instance.GfxDevice.RenderContext.CreateFence(in desc, name);
            Fences.Add(name, result);
            return result;
        }
        public bool AddCamera(string name, TtCamera camera)
        {
            if (CameraAttachments.ContainsKey(name))
                return false;
            camera.Name = name;
            CameraAttachments.Add(name, camera);
            return true;
        }
        public TtCamera FindCamera(string name)
        {
            TtCamera result;
            if (CameraAttachments.TryGetValue(name, out result))
                return result;
            return null;
        }
        public void SetDefaultCamera(string name)
        {
            mDefaultCamera = FindCamera(name);
        }
        public virtual Common.TtGpuSceneNode GetGpuSceneNode() 
        {
            return FindFirstNode<Common.TtGpuSceneNode>();
        }
        public virtual NxRHI.TtSrView GetFinalShowRSV()
        {
            if (RootNode.ColorAttachement == null)
                return null;
            return RootNode.ColorAttachement.Srv;
            //var attachBuffer = AttachmentCache.FindAttachement(in RootNode.GetOutput(0).Attachement.AttachmentName);
            //if (attachBuffer == null)
            //    return null;
            //return attachBuffer.Srv;
        }
        public virtual IProxiable GetHitproxy(UInt32 MouseX, UInt32 MouseY)
        {
            var hitproxyNode = FindFirstNode<Common.TtHitproxyNode>();
            if (hitproxyNode == null)
                return null;
            return hitproxyNode.GetHitproxy(MouseX, MouseY);
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTickSync;
        private static Profiler.TimeScope ScopeTickSync
        {
            get
            {
                if (mScopeTickSync == null)
                    mScopeTickSync = new Profiler.TimeScope(typeof(TtRenderPolicy), nameof(TickSync));
                return mScopeTickSync;
            }
        }
        public void UpdateCameraAttachements(NxRHI.TtCbView.EUpdateMode mode = NxRHI.TtCbView.EUpdateMode.Auto)
        {
            foreach (var i in CameraAttachments)
            {
                i.Value.UpdateConstBufferData(TtEngine.Instance.GfxDevice.RenderContext, mode);
            }
        }
        public override void TickSync()
        {
            using (new Profiler.TimeScopeHelper(ScopeTickSync))
            {
                UpdateCameraAttachements(NxRHI.TtCbView.EUpdateMode.Auto);
                base.TickSync();
            }
            PolicyOptionData.Clear();
        }

        struct FActionData
        {
            public Action<TtRenderPolicy, object> Action;
            public object Arg;
        }
        List<FActionData> mAfterPolicyFinishedCallbacks = new List<FActionData>();
        public void RegisterAfterPolicyFinishedCallback(Action<TtRenderPolicy, object> callback, object arg)
        {
            mAfterPolicyFinishedCallbacks.Add(new FActionData { Action = callback, Arg = arg });
        }
        public void AfterPolicyFinished()
        {
            foreach (var callback in mAfterPolicyFinishedCallbacks)
            {
                callback.Action(this, callback.Arg);
            }
            mAfterPolicyFinishedCallbacks.Clear();
        }

        #region Turn On/Off
        [Category("Option")]
        public TtCpuCullingNode CpuCullingNode
        {
            get
            {
                return this.FindFirstNode<TtCpuCullingNode>();
            }
        }
        public string mLookNodeName;
        [Category("Option")]
        public string LookNodeName
        {
            get
            {
                if (mLookNode != null)
                    return mLookNode.Name;
                return null;
            }
            set
            {
                mLookNode = this.FindNode<TtRenderGraphNode>(value, true);
            }
        }
        protected TtRenderGraphNode mLookNode;
        [Category("Option")]
        public TtRenderGraphNode LookNode
        {
            get => mLookNode;
        }
        //public enum ETypeAA
        //{
        //    None = 0,
        //    Fsaa,
        //    Taa,
            
        //    TypeCount,
        //}
        //[Category("Option")]
        //[Rtti.Meta("")]
        //public virtual ETypeAA TypeAA { get; set; } = ETypeAA.Taa;

        public enum ETypeFog
        {
            None = 0,
            ExpHeight,
            TypeCount,
        }
        [Category("Option")]
        [Rtti.Meta("")]
        public virtual ETypeFog TypeFog { get; set; } = ETypeFog.None;

        public Dictionary<string, object> PolicyOptionData { get; } = new Dictionary<string, object>();
        public void SetOptionData(string name, object data)
        {
            PolicyOptionData[name] = data;
        }
        public object GetOptionData(string name)
        {
            if (PolicyOptionData.TryGetValue(name, out object result))
            {
                return result;
            }
            return null;
        }
        #endregion
        public Common.TtPickedProxiableManager PickedProxiableManager { get; protected set; } = new Common.TtPickedProxiableManager();
        
        public virtual void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Mesh.TtRenderMesh.TtAtom atom)
        {
            atom.MdfQueue.OnDrawCall(cmd, drawcall, this, atom);
        }
        public virtual async Thread.Async.TtTask Initialize(TtCamera camera)
        {
            IsInitialized = false;
            if (camera == null)
            {
                camera = new TtCamera();
                camera.mCoreObject.PerspectiveFovLH(3.14f / 4f, 1, 1, 0.3f, 1000.0f);
                var eyePos = new DVector3(0, 0, -10);
                camera.mCoreObject.LookAtLH(in eyePos, in DVector3.Zero, in Vector3.Up);
            }
            // todo, support ortho
            else if( camera.IsOrtho == true)
            {

            }
            else
            {
                camera.mCoreObject.PerspectiveFovLH(3.14f / 4f, 1, 1, camera.ZNear, camera.ZFar);
            }
            AddCamera("MainCamera", camera);
            SetDefaultCamera("MainCamera");

            bool hasInputError = false;
            BuildGraph(ref hasInputError);
            if (hasInputError == false)
            {

            }

            foreach (var i in GraphNodes)
            {
                await i.Value.Initialize((TtRenderPolicy)this, i.Value.Name);
            }

            this.OnResize(1, 1);

            IsInitialized = true;
        }
        public override void OnResize(float x, float y)
        {
            if (DefaultCamera != null)
            {
                if (DefaultCamera.IsOrtho == false)
                    DefaultCamera.PerspectiveFovLH(3.14f / 4f, x, y, DefaultCamera.ZNear, DefaultCamera.ZFar);
            }

            base.OnResize(x, y);
        }

        #region CommonState
        NxRHI.TtSampler mClampState;
        public NxRHI.TtSampler ClampState
        {
            get
            {
                if (mClampState == null)
                {
                    var desc = new NxRHI.FSamplerDesc();
                    desc.SetDefault();
                    desc.Filter = NxRHI.ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
                    desc.CmpMode = NxRHI.EComparisionMode.CMP_NEVER;
                    desc.AddressU = NxRHI.EAddressMode.ADM_CLAMP;
                    desc.AddressV = NxRHI.EAddressMode.ADM_CLAMP;
                    desc.AddressW = NxRHI.EAddressMode.ADM_CLAMP;
                    desc.MaxAnisotropy = 0;
                    desc.MipLODBias = 0;
                    desc.MinLOD = 0;
                    desc.MaxLOD = 3.402823466e+38f;
                    mClampState = TtEngine.Instance.GfxDevice.SamplerStateManager.GetPipelineState(
                        TtEngine.Instance.GfxDevice.RenderContext, in desc);
                }
                return mClampState;
            }
        }
        NxRHI.TtSampler mClampPointState;
        public NxRHI.TtSampler ClampPointState
        {
            get
            {
                if (mClampPointState == null)
                {
                    var desc = new NxRHI.FSamplerDesc();
                    desc.SetDefault();
                    desc.Filter = NxRHI.ESamplerFilter.SPF_MIN_MAG_MIP_POINT;
                    desc.CmpMode = NxRHI.EComparisionMode.CMP_NEVER;
                    desc.AddressU = NxRHI.EAddressMode.ADM_CLAMP;
                    desc.AddressV = NxRHI.EAddressMode.ADM_CLAMP;
                    desc.AddressW = NxRHI.EAddressMode.ADM_CLAMP;
                    desc.MaxAnisotropy = 0;
                    desc.MipLODBias = 0;
                    desc.MinLOD = 0;
                    desc.MaxLOD = 3.402823466e+38f;
                    mClampPointState = TtEngine.Instance.GfxDevice.SamplerStateManager.GetPipelineState(
                        TtEngine.Instance.GfxDevice.RenderContext, in desc);
                }
                return mClampPointState;
            }
        }
        #endregion

        [Category("Option")]
        public Common.TtFogNode FogNode
        {
            get
            {
                return FindFirstNode<Common.TtFogNode>();
            }
        }

        public NxRHI.TtRCmdQueue CmdQueue = null;
        public void ExecuteCmdQueue(bool bFlushGPU)
        {
            if (CmdQueue == null)
                return;

            CmdQueue.FlushExecute(bFlushGPU);
        }
        public void QueueCmd(NxRHI.FRenderCmd cmd, string name, object tag = null,
            NxRHI.EQueueType qType = NxRHI.EQueueType.QU_Default,
            NxRHI.ERCmdType type = NxRHI.ERCmdType.Cmd,
            bool bImm = false)
        {
            if (CmdQueue != null)
            {
                CmdQueue.QueueCmd(cmd, name, tag, qType, type, bImm);
            }
            else
            {
                TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd(cmd, name, tag, qType, type, bImm);
            }
        }
        public void CommitCommandList(NxRHI.TtCommandList cmd, string name = null, NxRHI.EQueueType qType = NxRHI.EQueueType.QU_Default)
        {
            if (name==null)
            {
                var stackTrace = new StackTrace();
                var method = stackTrace.GetFrame(1)?.GetMethod();
                if (method != null)
                {
                    name = method.DeclaringType.FullName + "." + method.Name;
                }
            }
            if (CmdQueue != null)
            {
                //TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(cmd, qType);
                CmdQueue.QueueCmdlist(cmd, name, qType);
            }
            else
            {
                TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(cmd, name, qType);
            }
        }
    }

    //[Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.UDeferredPolicyBase@EngineCore" })]
    public class TtDeferredPolicyBase : TtRenderPolicy
    {
        #region Feature On/Off
        
        #endregion
        
        Shadow.TtShadowMapNode mShadowMapNode;
        Shadow.TtShadowMapNode ShadowMapNode
        {
            get
            {
                if (mShadowMapNode == null)
                {
                    mShadowMapNode = FindFirstNode<Shadow.TtShadowMapNode>();
                }
                return mShadowMapNode;
            }
        }
        Common.TtHitproxyNode mHitproxyNode;
        Common.TtHitproxyNode HitproxyNode
        {
            get
            {
                if (mHitproxyNode == null)
                {
                    mHitproxyNode = FindFirstNode<Common.TtHitproxyNode>();
                }
                return mHitproxyNode;
            }
        }
        Common.TtPickedNode mPickedNode;
        Common.TtPickedNode PickedNode
        {
            get
            {
                if (mPickedNode == null)
                {
                    mPickedNode = FindFirstNode<Common.TtPickedNode>();
                }
                return mPickedNode;
            }
        }
    }
    //[Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.UForwordPolicyBase@EngineCore" })]
    public class TtForwordPolicyBase : TtRenderPolicy
    {
        #region Feature On/Off
        
        #endregion

        Mobile.TtMobileOpaqueNode mBasePassNode;
        Mobile.TtMobileOpaqueNode BasePassNode
        {
            get
            {
                if (mBasePassNode == null)
                {
                    mBasePassNode = FindFirstNode<Mobile.TtMobileOpaqueNode>();
                }
                return mBasePassNode;
            }
        }
        Mobile.TtMobileTranslucentNode mTranslucentNode;
        Mobile.TtMobileTranslucentNode TranslucentNode
        {
            get
            {
                if (mTranslucentNode == null)
                {
                    mTranslucentNode = FindFirstNode<Mobile.TtMobileTranslucentNode>();
                }
                return mTranslucentNode;
            }
        }
        Shadow.TtShadowMapNode mShadowMapNode;
        Shadow.TtShadowMapNode ShadowMapNode
        {
            get
            {
                if (mShadowMapNode == null)
                {
                    mShadowMapNode = FindFirstNode<Shadow.TtShadowMapNode>();
                }
                return mShadowMapNode;
            }
        }
        Common.TtHitproxyNode mHitproxyNode;
        Common.TtHitproxyNode HitproxyNode
        {
            get
            {
                if (mHitproxyNode == null)
                {
                    mHitproxyNode = FindFirstNode<Common.TtHitproxyNode>();
                }
                return mHitproxyNode;
            }
        }
        Common.TtPickedNode mPickedNode;
        Common.TtPickedNode PickedNode
        {
            get
            {
                if (mPickedNode == null)
                {
                    mPickedNode = FindFirstNode<Common.TtPickedNode>();
                }
                return mPickedNode;
            }
        }
    }
}
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Graphics.Pipeline
{
	partial class TtRenderPolicy
	{
		public unsafe TtWorld macross_GetWorld (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = GetWorld();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross