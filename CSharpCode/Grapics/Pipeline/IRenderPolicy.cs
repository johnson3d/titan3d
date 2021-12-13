using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{   
    public class IRenderPolicy
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
        protected CCamera mCamera;
        public CCamera Camera { get => mCamera; }
        public virtual Common.UBasePassNode GetBasePassNode() { return null; }
        public virtual Common.UGpuSceneNode GetGpuSceneNode() { return null; }
        public virtual Common.URenderGraphNode QueryNode(string name)
        {
            switch(name)
            {
                case "BasePassNode":
                    return GetBasePassNode();
            }
            return null;
        }
        public virtual RHI.CShaderResourceView GetFinalShowRSV() { return null; }
        public virtual RHI.CShaderResourceView QuerySRV(string name)
        {
            return null;
        }
        public virtual IProxiable GetHitproxy(UInt32 MouseX, UInt32 MouseY)
        {
            return null;
        }
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
        public Common.UPickedProxiableManager PickedProxiableManager { get; protected set; } = new Common.UPickedProxiableManager();
        public List<Mesh.UMesh> VisibleMeshes = new List<Mesh.UMesh>();
        public List<GamePlay.Scene.UNode> VisibleNodes = new List<GamePlay.Scene.UNode>();

        public virtual Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom, Pipeline.Common.URenderGraphNode node) { return null; }
        public virtual void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Mesh.UMesh mesh, int atom) 
        {
            mesh.MdfQueue.OnDrawCall(shadingType, drawcall, this, mesh);
        }
        public virtual async System.Threading.Tasks.Task Initialize(CCamera camera, float x, float y)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            if (camera == null)
            {
                camera = new CCamera();
                camera.mCoreObject.PerspectiveFovLH(3.14f / 4f, x, y, 0.3f, 1000.0f);
                var eyePos = new DVector3(0, 0, -10);
                camera.mCoreObject.LookAtLH(in eyePos, in DVector3.Zero, in Vector3.Up);
            }
            else
            {
                camera.mCoreObject.PerspectiveFovLH(3.14f / 4f, x, y, 0.3f, 1000.0f);
            }
            mCamera = camera;
        }
        public virtual void Cleanup()
        {
            VisibleMeshes.Clear();
            VisibleNodes.Clear();
        }
        public virtual void TickLogic(GamePlay.UWorld world)
        {

        }
        public virtual void TickRender()
        {

        }
        public virtual void TickSync()
        {
            
        }
        public virtual void OnResize(float x, float y)
        {
        }
    }
}
