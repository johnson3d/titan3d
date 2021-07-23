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
        RL_CustomOpaque,
        RL_Translucent,
        RL_CustomTranslucent,
        //for editor to use;this layer should always be the last layer to send to renderer;
        RL_Gizmos,
        RL_Shadow,
        RL_Sky,

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
            unsafe
            {
                for (ERenderLayer i = ERenderLayer.RL_Opaque; i < ERenderLayer.RL_Num; i++)
                {
                    PassBuffers[(int)i].DrawCmdList.mCoreObject.SetViewport(vp.mCoreObject);
                }
            }
        }
        public unsafe void BuildRenderPass(ref RenderPassDesc passDesc, RHI.CFrameBuffers frameBuffers)
        {
            for (ERenderLayer i = ERenderLayer.RL_Opaque; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i].DrawCmdList.PassNumber = PassBuffers[(int)i].DrawCmdList.mCoreObject.GetPassNumber();
                var cmdlist = PassBuffers[(int)i].DrawCmdList.mCoreObject;
                cmdlist.BeginCommand();
                if (i == ERenderLayer.RL_Opaque)
                {
                    cmdlist.BeginRenderPass(ref passDesc, frameBuffers.mCoreObject, i.ToString());
                    cmdlist.BuildRenderPass(0);
                    cmdlist.EndRenderPass();
                    cmdlist.EndCommand();
                }
                else
                {
                    if (PassBuffers[(int)i].DrawCmdList.PassNumber > 0)
                    {
                        cmdlist.BeginRenderPass((RenderPassDesc*)0, frameBuffers.mCoreObject, i.ToString());
                        cmdlist.BuildRenderPass(0);
                        cmdlist.EndRenderPass();
                        cmdlist.EndCommand();
                    }
                }
            }

            var num = mPipelineStat.mCoreObject.mDrawCall;
        }
        public void Commit(RHI.CRenderContext rc)
        {
            unsafe
            {
                for (ERenderLayer i = ERenderLayer.RL_Opaque; i < ERenderLayer.RL_Num; i++)
                {
                    var cmdlist = PassBuffers[(int)i].CommitCmdList.mCoreObject;
                    if (PassBuffers[(int)i].CommitCmdList.PassNumber == 0 && i != ERenderLayer.RL_Opaque)
                        continue;
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
