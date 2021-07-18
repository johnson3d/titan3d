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
        public virtual Common.UBasePassNode GetBasePassNode() { return null; }
        
        public Common.UPickedProxiableManager PickedProxiableManager { get; protected set; } = new Common.UPickedProxiableManager();
        public List<Mesh.UMesh> VisibleMeshes = new List<Mesh.UMesh>();
        
        public virtual Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom) { return null; }
        public virtual void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Mesh.UMesh mesh, int atom) 
        {
            mesh.MdfQueue.OnDrawCall(shadingType, drawcall, this, mesh);
        }
        public virtual RHI.CShaderResourceView GetFinalShowRSV() { return null; }
        public virtual async System.Threading.Tasks.Task Initialize(float x, float y)
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
        public virtual void Cleanup()
        {
            VisibleMeshes.Clear();
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
