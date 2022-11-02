using EngineNS.Thread;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{   
    public class URenderPolicy : Common.URenderGraph
    {
        public enum EShadingType
        {
            BasePass,
            DepthPass,
            HitproxyPass,//Mesh绘制HitproxyID
            Picked,//Mesh绘制选择高亮
            Count,
        }
        //TagObject通常用来处理ShadingEnv.OnDrawCall的特殊参数设置
        //public Common.URenderGraphNode TagObject;
        public object TagObject;
        protected CCamera mDefaultCamera;
        public CCamera DefaultCamera { get => mDefaultCamera; }
        public Dictionary<string, CCamera> CameraAttachments { get; } = new Dictionary<string, CCamera>();
        public bool AddCamera(string name, CCamera camera)
        {
            if (CameraAttachments.ContainsKey(name))
                return false;
            CameraAttachments.Add(name, camera);
            return true;
        }
        public CCamera FindCamera(string name)
        {
            CCamera result;
            if (CameraAttachments.TryGetValue(name, out result))
                return result;
            return null;
        }
        public void SetDefaultCamera(string name)
        {
            mDefaultCamera = FindCamera(name);
        }
        public virtual Common.UGpuSceneNode GetGpuSceneNode() 
        {
            return FindFirstNode<Common.UGpuSceneNode>();
        }
        public virtual NxRHI.USrView GetFinalShowRSV()
        {
            var attachBuffer = AttachmentCache.FindAttachement(in RootNode.GetOutput(0).Attachement.AttachmentName);
            if (attachBuffer == null)
                return null;
            return attachBuffer.Srv;
        }        
        public virtual IProxiable GetHitproxy(UInt32 MouseX, UInt32 MouseY)
        {
            var hitproxyNode = FindFirstNode<Common.UHitproxyNode>();
            if (hitproxyNode == null)
                return null;
            return hitproxyNode.GetHitproxy(MouseX, MouseY);
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTickSync = Profiler.TimeScopeManager.GetTimeScope(typeof(URenderPolicy), nameof(TickSync));
        public override void TickSync()
        {
            using (new Profiler.TimeScopeHelper(ScopeTickSync))
            {
                base.TickSync();
                foreach (var i in CameraAttachments)
                {
                    i.Value.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext);
                }
            }   
        }
        #region Turn On/Off
        protected bool mDisableShadow;
        public virtual bool DisableShadow
        {
            get => mDisableShadow;
            set => mDisableShadow = value;
        }
        protected bool mDisableAO;
        public virtual bool DisableAO
        {
            get => mDisableAO;
            set
            {
                mDisableAO = value;
            }
        }
        protected bool mDisablePointLight;
        public virtual bool DisablePointLight
        {
            get => mDisableAO;
            set
            {
                mDisableAO = value;
            }
        }
        protected bool mDisableHDR;
        public virtual bool DisableHDR
        {
            get => mDisableHDR;
            set
            {
                mDisableHDR = value;
            }
        }
        #endregion
        public Common.UPickedProxiableManager PickedProxiableManager { get; protected set; } = new Common.UPickedProxiableManager();
        public List<Mesh.UMesh> VisibleMeshes = new List<Mesh.UMesh>();
        public List<GamePlay.Scene.UNode> VisibleNodes = new List<GamePlay.Scene.UNode>();

        public virtual Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom, Pipeline.Common.URenderGraphNode node)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    {
                        var BasePassNode = FindFirstNode<Deferred.UDeferredBasePassNode>();
                        if (node == BasePassNode)
                        {
                            return BasePassNode.mOpaqueShading;
                        }
                        else
                        {
                            var ForwordNode = FindFirstNode<Deferred.UForwordNode>();
                            if (node == ForwordNode)
                            {
                                switch (mesh.Atoms[atom].Material.RenderLayer)
                                {
                                    case ERenderLayer.RL_Translucent:
                                        return ForwordNode.mTranslucentShading;
                                    case ERenderLayer.RL_Sky:
                                        return ForwordNode.mTranslucentShading;
                                    default:
                                        return ForwordNode.mOpaqueShading;
                                }
                            }   
                        }
                    }
                    break;
                case EShadingType.DepthPass:
                    {
                        var ShadowMapNode = FindFirstNode<Shadow.UShadowMapNode>();
                        return ShadowMapNode.mShadowShading;
                    }
                case EShadingType.HitproxyPass:
                    {
                        var HitproxyNode = FindFirstNode<Common.UHitproxyNode>();
                        return HitproxyNode.mHitproxyShading;
                    }
                case EShadingType.Picked:
                    {
                        var PickedNode = FindFirstNode<Common.UPickedNode>();
                        return PickedNode.PickedShading;
                    }
                default:
                    break;
            }
            return null;
        }
        public virtual void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Mesh.UMesh mesh, int atom) 
        {
            mesh.MdfQueue.OnDrawCall(shadingType, drawcall, this, mesh);
        }
        public virtual async System.Threading.Tasks.Task Initialize(CCamera camera)
        {
            if (camera == null)
            {
                camera = new CCamera();
                camera.mCoreObject.PerspectiveFovLH(3.14f / 4f, 1, 1, 0.3f, 1000.0f);
                var eyePos = new DVector3(0, 0, -10);
                camera.mCoreObject.LookAtLH(in eyePos, in DVector3.Zero, in Vector3.Up);
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
                await i.Value.Initialize((URenderPolicy)this, i.Value.Name);
            }
        }
        public override void Cleanup()
        {
            VisibleMeshes.Clear();
            VisibleNodes.Clear();

            base.Cleanup();
        }
        public override void OnResize(float x, float y)
        {
            if (DefaultCamera != null)
                DefaultCamera.mCoreObject.PerspectiveFovLH(3.14f / 4f, x, y, DefaultCamera.ZNear, DefaultCamera.ZFar);

            base.OnResize(x, y);
        }

        #region CommonState
        NxRHI.USampler mClampState;
        public NxRHI.USampler ClampState
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
                    mClampState = UEngine.Instance.GfxDevice.SamplerStateManager.GetPipelineState(
                        UEngine.Instance.GfxDevice.RenderContext, in desc);
                }
                return mClampState;
            }
        }
        NxRHI.USampler mClampPointState;
        public NxRHI.USampler ClampPointState
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
                    mClampPointState = UEngine.Instance.GfxDevice.SamplerStateManager.GetPipelineState(
                        UEngine.Instance.GfxDevice.RenderContext, in desc);
                }
                return mClampPointState;
            }
        }
        #endregion
    }

    public class UDeferredPolicyBase : URenderPolicy
    {
        #region Feature On/Off
        public override bool DisableShadow
        {
            get => mDisableShadow;
            set
            {
                mDisableShadow = value;
                var shading = this.FindFirstNode<Deferred.UDeferredDirLightingNode>()?.ScreenDrawPolicy.mBasePassShading as Deferred.UDeferredDirLightingShading;
                //var shading = DirLightingNode.ScreenDrawPolicy.mBasePassShading as UDeferredDirLightingShading;
                shading?.SetDisableShadow(value);
            }
        }
        public override bool DisablePointLight
        {
            get
            {
                return mDisablePointLight;
            }
            set
            {
                mDisablePointLight = value;
                var shading = this.FindFirstNode<Deferred.UDeferredDirLightingNode>()?.ScreenDrawPolicy.mBasePassShading as Deferred.UDeferredDirLightingShading;
                //var shading = DirLightingNode.ScreenDrawPolicy.mBasePassShading as UDeferredDirLightingShading;
                shading?.SetDisablePointLights(value);
            }
        }
        public override bool DisableHDR
        {
            get
            {
                return mDisableHDR;
            }
            set
            {
                mDisableHDR = value;
                var shading = this.FindFirstNode<Deferred.UDeferredDirLightingNode>()?.ScreenDrawPolicy.mBasePassShading as Deferred.UDeferredDirLightingShading;
                //var shading = DirLightingNode.ScreenDrawPolicy.mBasePassShading as UDeferredDirLightingShading;
                shading?.SetDisableHDR(value);
            }
        }
        #endregion
        Deferred.UDeferredBasePassNode mBasePassNode;
        Deferred.UDeferredBasePassNode BasePassNode
        {
            get
            {
                if (mBasePassNode == null)
                {
                    mBasePassNode = FindFirstNode<Deferred.UDeferredBasePassNode>();
                }
                return mBasePassNode;
            }
        }
        Deferred.UForwordNode mForwordNode;
        Deferred.UForwordNode ForwordNode
        {
            get
            {
                if (mForwordNode == null)
                {
                    mForwordNode = FindFirstNode<Deferred.UForwordNode>();
                }
                return mForwordNode;
            }
        }
        Shadow.UShadowMapNode mShadowMapNode;
        Shadow.UShadowMapNode ShadowMapNode
        {
            get
            {
                if (mShadowMapNode == null)
                {
                    mShadowMapNode = FindFirstNode<Shadow.UShadowMapNode>();
                }
                return mShadowMapNode;
            }
        }
        Common.UHitproxyNode mHitproxyNode;
        Common.UHitproxyNode HitproxyNode
        {
            get
            {
                if (mHitproxyNode == null)
                {
                    mHitproxyNode = FindFirstNode<Common.UHitproxyNode>();
                }
                return mHitproxyNode;
            }
        }
        Common.UPickedNode mPickedNode;
        Common.UPickedNode PickedNode
        {
            get
            {
                if (mPickedNode == null)
                {
                    mPickedNode = FindFirstNode<Common.UPickedNode>();
                }
                return mPickedNode;
            }
        }
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom, Pipeline.Common.URenderGraphNode node)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    {
                        //var BasePassNode = FindFirstNode<Deferred.UDeferredBasePassNode>();
                        if (node == BasePassNode)
                        {
                            return BasePassNode.mOpaqueShading;
                        }
                        else
                        {
                            //var ForwordNode = FindFirstNode<Deferred.UForwordNode>();
                            if (node == ForwordNode)
                            {
                                switch (mesh.Atoms[atom].Material.RenderLayer)
                                {
                                    case ERenderLayer.RL_Translucent:
                                        return ForwordNode.mTranslucentShading;
                                    case ERenderLayer.RL_Sky:
                                        return ForwordNode.mTranslucentShading;
                                    default:
                                        return ForwordNode.mOpaqueShading;
                                }
                            }
                        }
                    }
                    break;
                case EShadingType.DepthPass:
                    {
                        //var ShadowMapNode = FindFirstNode<Shadow.UShadowMapNode>();
                        return ShadowMapNode.mShadowShading;
                    }
                case EShadingType.HitproxyPass:
                    {
                        //var HitproxyNode = FindFirstNode<Common.UHitproxyNode>();
                        return HitproxyNode.mHitproxyShading;
                    }
                case EShadingType.Picked:
                    {
                        //var PickedNode = FindFirstNode<Common.UPickedNode>();
                        return PickedNode.PickedShading;
                    }
                default:
                    break;
            }
            return null;
        }
    }
    public class UForwordPolicyBase : URenderPolicy
    {
        public override bool DisableAO
        {
            get => mDisableAO;
            set
            {
                mDisableAO = value;
                var finalShading = FindFirstNode<Mobile.UFinalCopyNode>()?.ScreenDrawPolicy.mBasePassShading as Mobile.UFinalCopyShading;
                if (finalShading != null)
                {
                    finalShading.SetDisableAO(value);
                }
            }
        }
        public override bool DisableHDR
        {
            get
            {
                return mDisableHDR;
            }
            set
            {
                mDisableHDR = value;
                var shading = FindFirstNode<Mobile.UFinalCopyNode>().ScreenDrawPolicy.mBasePassShading as Mobile.UFinalCopyShading;
                shading?.SetDisableHDR(value);
            }
        }
        Mobile.UMobileOpaqueNode mBasePassNode;
        Mobile.UMobileOpaqueNode BasePassNode
        {
            get
            {
                if (mBasePassNode == null)
                {
                    mBasePassNode = FindFirstNode<Mobile.UMobileOpaqueNode>();
                }
                return mBasePassNode;
            }
        }
        Mobile.UMobileTranslucentNode mTranslucentNode;
        Mobile.UMobileTranslucentNode TranslucentNode
        {
            get
            {
                if (mTranslucentNode == null)
                {
                    mTranslucentNode = FindFirstNode<Mobile.UMobileTranslucentNode>();
                }
                return mTranslucentNode;
            }
        }
        Shadow.UShadowMapNode mShadowMapNode;
        Shadow.UShadowMapNode ShadowMapNode
        {
            get
            {
                if (mShadowMapNode == null)
                {
                    mShadowMapNode = FindFirstNode<Shadow.UShadowMapNode>();
                }
                return mShadowMapNode;
            }
        }
        Common.UHitproxyNode mHitproxyNode;
        Common.UHitproxyNode HitproxyNode
        {
            get
            {
                if (mHitproxyNode == null)
                {
                    mHitproxyNode = FindFirstNode<Common.UHitproxyNode>();
                }
                return mHitproxyNode;
            }
        }
        Common.UPickedNode mPickedNode;
        Common.UPickedNode PickedNode
        {
            get
            {
                if (mPickedNode == null)
                {
                    mPickedNode = FindFirstNode<Common.UPickedNode>();
                }
                return mPickedNode;
            }
        }
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom, Pipeline.Common.URenderGraphNode node)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    {
                        //var BasePassNode = FindFirstNode<Mobile.UMobileOpaqueNode>();
                        //var TranslucentNode = FindFirstNode<Mobile.UMobileTranslucentNode>();
                        if (node == BasePassNode)
                        {
                            return BasePassNode.mOpaqueShading;
                        }
                        else if (node == TranslucentNode)
                        {
                            switch (mesh.Atoms[atom].Material.RenderLayer)
                            {
                                case ERenderLayer.RL_Translucent:
                                    return TranslucentNode.mTranslucentShading;
                                case ERenderLayer.RL_Sky:
                                    return TranslucentNode.mTranslucentShading;
                                default:
                                    return BasePassNode.mOpaqueShading;
                            }
                        }
                    }
                    break;
                case EShadingType.DepthPass:
                    {
                        //var ShadowMapNode = FindFirstNode<Shadow.UShadowMapNode>();
                        return ShadowMapNode.mShadowShading;
                    }
                case EShadingType.HitproxyPass:
                    {
                        //var HitproxyNode = FindFirstNode<Common.UHitproxyNode>();
                        return HitproxyNode.mHitproxyShading;
                    }
                case EShadingType.Picked:
                    {
                        //var PickedNode = FindFirstNode<Common.UPickedNode>();
                        return PickedNode.PickedShading;
                    }
                default:
                    break;
            }
            return null;
        }
    }
}
