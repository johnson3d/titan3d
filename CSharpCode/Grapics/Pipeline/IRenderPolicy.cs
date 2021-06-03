using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class UDrawBuffers
    {
        private RHI.CCommandList[] mCmdLists = new RHI.CCommandList[2];
        public void Initialize(RHI.CRenderContext rc)
        {
            var desc = new ICommandListDesc();
            mCmdLists[0] = rc.CreateCommandList(ref desc);
            mCmdLists[1] = rc.CreateCommandList(ref desc);
        }
        public RHI.CCommandList DrawCmdList
        {
            get { return mCmdLists[0]; }
        }
        public RHI.CCommandList CommitCmdList
        {
            get { return mCmdLists[1]; }
        }
        public void SwapBuffer()
        {
            var save = mCmdLists[0];
            mCmdLists[0] = mCmdLists[1];
            mCmdLists[1] = save;
        }
        public void SetDebugName(string name)
        {
            mCmdLists[0].mCoreObject.SetDebugName(name);
            mCmdLists[1].mCoreObject.SetDebugName(name);
        }
    }
    public class IRenderPolicy
    {
        public enum EShadingType
        {
            BasePass,
            DepthPass,
            HitproxyPass,
            Picked,
            Count,
        }
        //[EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public UGraphicsBuffers GBuffers { get; protected set; } = new UGraphicsBuffers();
        public List<Mesh.UMesh> VisibleMeshes = new List<Mesh.UMesh>();
        public virtual Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh) { return null; }
        public virtual void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Mesh.UMesh mesh) { }
        public virtual RHI.CShaderResourceView GetFinalShowRSV() { return null; }
        public virtual async System.Threading.Tasks.Task Initialize(float x, float y)
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
        public virtual void Cleanup()
        {
            VisibleMeshes.Clear();
            GBuffers?.Cleanup();
            GBuffers = null;
        }
        public virtual void TickLogic()
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
