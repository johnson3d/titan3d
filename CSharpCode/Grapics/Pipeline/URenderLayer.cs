using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class UDrawBuffers
    {
        private NxRHI.UCommandList[] mCmdLists = new NxRHI.UCommandList[2];
        public void Initialize(NxRHI.UGpuDevice rc, string debugName)
        {
            mCmdLists[0] = rc.CreateCommandList();
            mCmdLists[1] = rc.CreateCommandList();
            SetDebugName(debugName);
        }
        public void SetPipeline(in NxRHI.UGpuPipeline pipeline)
        {
            if (pipeline == null)
            {
                var nullPipeline = new NxRHI.IGpuPipeline();
                mCmdLists[0].mCoreObject.SetDefaultPipeline(nullPipeline);
                mCmdLists[1].mCoreObject.SetDefaultPipeline(nullPipeline);
            }
            else
            {
                mCmdLists[0].mCoreObject.SetDefaultPipeline(pipeline.mCoreObject);
                mCmdLists[1].mCoreObject.SetDefaultPipeline(pipeline.mCoreObject);
            }
        }
        public uint DrawcallNumber { get; set; }
        public NxRHI.UCommandList DrawCmdList
        {
            get { return mCmdLists[0]; }
        }
        public NxRHI.UCommandList CommitCmdList
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
        public NxRHI.UGpuPipeline mPipeline;
        public UDrawBuffers[] PassBuffers = new UDrawBuffers[(int)ERenderLayer.RL_Num];
        public UDrawBuffers PostCmds = new UDrawBuffers();
        public void Initialize(NxRHI.UGpuDevice rc, string debugName)
        {
            for (ERenderLayer i = ERenderLayer.RL_Opaque; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i] = new UDrawBuffers();
                PassBuffers[(int)i].Initialize(rc, $"{debugName}:{i.ToString()}");
                PassBuffers[(int)i].SetPipeline(mPipeline);
            }
            PostCmds = new UDrawBuffers();
            PostCmds.Initialize(rc, $"{debugName}:Post");
        }
        public void ClearMeshDrawPassArray()
        {
            for (ERenderLayer i = ERenderLayer.RL_Opaque; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i].DrawCmdList.mCoreObject.ResetGpuDraws();
            }
        }
        public unsafe void SetViewport(in NxRHI.FViewPort vp)
        {
            fixed(NxRHI.FViewPort* p = &vp)
            {
                for (ERenderLayer i = ERenderLayer.RL_Opaque; i < ERenderLayer.RL_Num; i++)
                {
                    PassBuffers[(int)i].DrawCmdList.SetViewport(1, p);
                }
            }
        }
        public unsafe void BuildTranslucentRenderPass(URenderPolicy policy, in NxRHI.FRenderPassClears passClear, UGraphicsBuffers frameBuffers, UGraphicsBuffers gizmosFrameBuffers)
        {
            PassBuffers[(int)ERenderLayer.RL_Translucent].DrawcallNumber = PassBuffers[(int)ERenderLayer.RL_Translucent].DrawCmdList.DrawcallNumber;
            PassBuffers[(int)ERenderLayer.RL_Sky].DrawcallNumber = PassBuffers[(int)ERenderLayer.RL_Sky].DrawCmdList.DrawcallNumber;
            PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawcallNumber = PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawCmdList.DrawcallNumber;
            PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawcallNumber = PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawCmdList.DrawcallNumber;

            {
                if (PassBuffers[(int)ERenderLayer.RL_Translucent].DrawcallNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_Translucent].DrawCmdList;
                    frameBuffers.BuildFrameBuffers(policy);
                    cmdlist.BeginPass(frameBuffers.FrameBuffers, in passClear, ERenderLayer.RL_Translucent.ToString());
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();
                    cmdlist.EndCommand();
                }
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_Sky].DrawcallNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_Sky].DrawCmdList;
                    frameBuffers.BuildFrameBuffers(policy);
                    var noClear = new NxRHI.FRenderPassClears();
                    noClear.ClearFlags = 0;
                    cmdlist.BeginPass(frameBuffers.FrameBuffers, in noClear, ERenderLayer.RL_Sky.ToString());
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();
                    cmdlist.EndCommand();
                }
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawcallNumber > 0 ||
                    PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawcallNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawCmdList;
                    gizmosFrameBuffers.BuildFrameBuffers(policy);
                    cmdlist.BeginPass(gizmosFrameBuffers.FrameBuffers, in passClear, ERenderLayer.RL_Gizmos.ToString());
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();
                    cmdlist.EndCommand();
                }
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawcallNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawCmdList;
                    gizmosFrameBuffers.BuildFrameBuffers(policy);
                    var noClear = new NxRHI.FRenderPassClears();
                    noClear.ClearFlags = 0;
                    cmdlist.BeginPass(gizmosFrameBuffers.FrameBuffers, in noClear, ERenderLayer.RL_TranslucentGizmos.ToString());
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();
                    cmdlist.EndCommand();
                }
            }
        }
        public unsafe void BuildRenderPass(URenderPolicy policy, in NxRHI.FViewPort viewport, in NxRHI.FRenderPassClears passClear, UGraphicsBuffers frameBuffers, UGraphicsBuffers gizmosFrameBuffers, string debugName)
        {
            PassBuffers[(int)ERenderLayer.RL_Opaque].DrawcallNumber = PassBuffers[(int)ERenderLayer.RL_Opaque].DrawCmdList.DrawcallNumber;
            PassBuffers[(int)ERenderLayer.RL_Translucent].DrawcallNumber = PassBuffers[(int)ERenderLayer.RL_Translucent].DrawCmdList.DrawcallNumber;
            PassBuffers[(int)ERenderLayer.RL_Sky].DrawcallNumber = PassBuffers[(int)ERenderLayer.RL_Sky].DrawCmdList.DrawcallNumber;
            PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawcallNumber = PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawCmdList.DrawcallNumber;
            PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawcallNumber = PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawCmdList.DrawcallNumber;

            {
                var cmdlist = PassBuffers[(int)ERenderLayer.RL_Opaque].DrawCmdList;

                cmdlist.BeginCommand();
                cmdlist.SetViewport(in viewport);
                //frameBuffers.BuildFrameBuffers(policy);
                cmdlist.BeginPass(frameBuffers.FrameBuffers, in passClear, debugName + ERenderLayer.RL_Opaque.ToString());
                cmdlist.FlushDraws();
                cmdlist.EndPass();
                cmdlist.EndCommand();

                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_Translucent].DrawcallNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_Translucent].DrawCmdList;

                    cmdlist.BeginCommand();
                    cmdlist.SetViewport(in viewport);
                    //frameBuffers.BuildFrameBuffers(policy);
                    var noClear = new NxRHI.FRenderPassClears();
                    noClear.ClearFlags = 0;
                    cmdlist.BeginPass(frameBuffers.FrameBuffers, in noClear, debugName + ERenderLayer.RL_Translucent.ToString());
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();
                    cmdlist.EndCommand();
                    UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
                }   
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_Sky].DrawcallNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_Sky].DrawCmdList;

                    cmdlist.BeginCommand();
                    cmdlist.SetViewport(in viewport);
                    //frameBuffers.BuildFrameBuffers(policy);
                    var noClear = new NxRHI.FRenderPassClears();
                    noClear.ClearFlags = 0;
                    cmdlist.BeginPass(frameBuffers.FrameBuffers, in noClear, debugName + ERenderLayer.RL_Sky.ToString());
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();
                    cmdlist.EndCommand();
                    UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
                }
            }
            
            {
                if (PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawcallNumber > 0 || 
                    PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawcallNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawCmdList;

                    cmdlist.BeginCommand();
                    cmdlist.SetViewport(in viewport);
                    //gizmosFrameBuffers.BuildFrameBuffers(policy);
                    cmdlist.BeginPass(gizmosFrameBuffers.FrameBuffers, in passClear, debugName + ERenderLayer.RL_Gizmos.ToString());
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();
                    cmdlist.EndCommand();
                    UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
                }   
            }

            {
                if (PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawcallNumber > 0)
                {
                    var cmdlist = PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawCmdList;

                    cmdlist.BeginCommand();
                    cmdlist.SetViewport(in viewport);
                    //gizmosFrameBuffers.BuildFrameBuffers(policy);
                    var noClear = new NxRHI.FRenderPassClears();
                    noClear.ClearFlags = 0;
                    cmdlist.BeginPass(gizmosFrameBuffers.FrameBuffers, in noClear, debugName + ERenderLayer.RL_TranslucentGizmos.ToString());
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();
                    cmdlist.EndCommand();
                    UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
                }
            }
        }
        public void SwapBuffer()
        {
            for (ERenderLayer i = ERenderLayer.RL_Opaque; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i].SwapBuffer();
            }
            PostCmds.SwapBuffer();
        }
        public void PushDrawCall(ERenderLayer layer, NxRHI.UGraphicDraw drawcall)
        {
            unsafe
            {
                PassBuffers[(int)layer].DrawCmdList.PushGpuDraw(drawcall);
            }
        }
    }

}
