using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class UDrawBuffers
    {
        private RHI.CCommandList[] mCmdLists = new RHI.CCommandList[2];
        public void Initialize(RHI.CRenderContext rc, string debugName)
        {
            var desc = new ICommandListDesc();
            mCmdLists[0] = rc.CreateCommandList(ref desc);
            mCmdLists[1] = rc.CreateCommandList(ref desc);
            SetDebugName(debugName);
        }
        public void SetPipelineStat(RHI.CPipelineStat stat)
        {
            unsafe
            {
                mCmdLists[0].mCoreObject.SetPipelineState(stat.mCoreObject);
                mCmdLists[1].mCoreObject.SetPipelineState(stat.mCoreObject);
            }
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

    public enum ERenderLayer : sbyte
    {
        RL_Opaque,
        RL_Translucent,
        RL_Sky,
        //for editor to use;this layer should always be the last layer to send to renderer;
        RL_Gizmos,
        RL_TranslucentGizmos,

        RL_Num,
    }

    public class UPassDrawBuffers
    {
        public RHI.CPipelineStat mPipelineStat;
        public UDrawBuffers[] PassBuffers = new UDrawBuffers[(int)ERenderLayer.RL_Num];
        public void Initialize(RHI.CRenderContext rc, string debugName)
        {
            mPipelineStat = new RHI.CPipelineStat();
            for (ERenderLayer i = ERenderLayer.RL_Opaque; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i] = new UDrawBuffers();
                PassBuffers[(int)i].Initialize(rc, $"{debugName}:{i.ToString()}");
                PassBuffers[(int)i].SetPipelineStat(mPipelineStat);
            }
        }
        public void ClearMeshDrawPassArray()
        {
            for (ERenderLayer i = ERenderLayer.RL_Opaque; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i].DrawCmdList.mCoreObject.ClearMeshDrawPassArray();
            }
            mPipelineStat.mCoreObject.Reset();
        }
        public void SetViewport(RHI.CViewPort vp)
        {
            for (ERenderLayer i = ERenderLayer.RL_Opaque; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i].DrawCmdList.mCoreObject.SetViewport(vp.mCoreObject);
            }
        }
        public unsafe void BuildRenderPass(ref RenderPassDesc passDesc, RHI.CFrameBuffers frameBuffers, ref RenderPassDesc gizmosPassDesc, RHI.CFrameBuffers gizmosFrameBuffers)
        {
            PassBuffers[(int)ERenderLayer.RL_Opaque].DrawCmdList.PassNumber = PassBuffers[(int)ERenderLayer.RL_Opaque].DrawCmdList.mCoreObject.GetPassNumber();
            PassBuffers[(int)ERenderLayer.RL_Translucent].DrawCmdList.PassNumber = PassBuffers[(int)ERenderLayer.RL_Translucent].DrawCmdList.mCoreObject.GetPassNumber();
            PassBuffers[(int)ERenderLayer.RL_Sky].DrawCmdList.PassNumber = PassBuffers[(int)ERenderLayer.RL_Sky].DrawCmdList.mCoreObject.GetPassNumber();
            PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawCmdList.PassNumber = PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawCmdList.mCoreObject.GetPassNumber();
            PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawCmdList.PassNumber = PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawCmdList.mCoreObject.GetPassNumber();
            {
                var cmdlist = PassBuffers[(int)ERenderLayer.RL_Opaque].DrawCmdList.mCoreObject;

                cmdlist.BeginRenderPass(ref passDesc, frameBuffers.mCoreObject, ERenderLayer.RL_Opaque.ToString());
                cmdlist.BuildRenderPass(0);
                cmdlist.EndRenderPass();
                cmdlist.EndCommand();
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_Translucent].DrawCmdList.PassNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_Translucent].DrawCmdList.mCoreObject;

                    cmdlist.BeginRenderPass((RenderPassDesc*)0, frameBuffers.mCoreObject, ERenderLayer.RL_Translucent.ToString());
                    cmdlist.BuildRenderPass(0);
                    cmdlist.EndRenderPass();
                    cmdlist.EndCommand();
                }   
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_Sky].DrawCmdList.PassNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_Sky].DrawCmdList.mCoreObject;

                    cmdlist.BeginRenderPass((RenderPassDesc*)0, frameBuffers.mCoreObject, ERenderLayer.RL_Sky.ToString());
                    cmdlist.BuildRenderPass(0);
                    cmdlist.EndRenderPass();
                    cmdlist.EndCommand();
                }
            }
            
            {
                if (PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawCmdList.PassNumber > 0 || 
                    PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawCmdList.PassNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawCmdList.mCoreObject;

                    cmdlist.BeginRenderPass(ref gizmosPassDesc, gizmosFrameBuffers.mCoreObject, ERenderLayer.RL_Gizmos.ToString());
                    cmdlist.BuildRenderPass(0);
                    cmdlist.EndRenderPass();
                    cmdlist.EndCommand();
                }   
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawCmdList.PassNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawCmdList.mCoreObject;

                    cmdlist.BeginRenderPass((RenderPassDesc*)0, gizmosFrameBuffers.mCoreObject, ERenderLayer.RL_TranslucentGizmos.ToString());
                    cmdlist.BuildRenderPass(0);
                    cmdlist.EndRenderPass();
                    cmdlist.EndCommand();
                }
            }
            var num = mPipelineStat.mCoreObject.mDrawCall;
        }
        public void Commit(RHI.CRenderContext rc)
        {
            {
                var cmdlist = PassBuffers[(int)ERenderLayer.RL_Opaque].CommitCmdList.mCoreObject;
                cmdlist.Commit(rc.mCoreObject);
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_Translucent].CommitCmdList.PassNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_Translucent].CommitCmdList.mCoreObject;
                    cmdlist.Commit(rc.mCoreObject);
                }
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_Sky].CommitCmdList.PassNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_Sky].CommitCmdList.mCoreObject;
                    cmdlist.Commit(rc.mCoreObject);
                }
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_Gizmos].CommitCmdList.PassNumber > 0 ||
                    PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].CommitCmdList.PassNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_Gizmos].CommitCmdList.mCoreObject;
                    cmdlist.Commit(rc.mCoreObject);
                }
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].CommitCmdList.PassNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].CommitCmdList.mCoreObject;
                    cmdlist.Commit(rc.mCoreObject);
                }
            }
        }
        public void SwapBuffer()
        {
            for (ERenderLayer i = ERenderLayer.RL_Opaque; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i].SwapBuffer();
            }
        }
        public void PushDrawCall(ERenderLayer layer, RHI.CDrawCall drawcall)
        {
            unsafe
            {
                PassBuffers[(int)layer].DrawCmdList.mCoreObject.PushDrawCall(drawcall.mCoreObject);
            }
        }
    }

}
