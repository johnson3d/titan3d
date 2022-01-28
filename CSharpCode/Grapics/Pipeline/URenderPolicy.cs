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
        public virtual RHI.CShaderResourceView GetFinalShowRSV()
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
        public virtual void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Mesh.UMesh mesh, int atom) 
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
    }
}
