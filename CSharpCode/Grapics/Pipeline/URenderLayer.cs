using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class UDrawBuffers : IDisposable
    {
        public void Dispose() 
        {
            //don't do it, sth maybe executing
            //CoreSDK.DisposeObject(ref mCmdLists[0]);
            //CoreSDK.DisposeObject(ref mCmdLists[1]);
        }
        private NxRHI.UCommandList[] mCmdLists = new NxRHI.UCommandList[2];
        public void Initialize(NxRHI.TtGpuDevice rc, string debugName)
        {
            mCmdLists[0] = rc.CreateCommandList();
            mCmdLists[1] = rc.CreateCommandList();
            SetDebugName(debugName);
        }
        public void SetPipeline(in NxRHI.TtGpuPipeline pipeline)
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
        RL_Begin = 0,
        RL_Background = 0,
        RL_Opaque,
        RL_Translucent,
        RL_Sky,
        //for editor to use;this layer should always be the last layer to send to renderer;
        RL_PostOpaque,
        RL_PostTranslucent,
        RL_Gizmos,
        RL_TranslucentGizmos,

        RL_Num,
    }

    public class TtLayerDrawBuffers
    {
        public struct TtLayerDrawBuffersScope : IDisposable
        {
            TtLayerDrawBuffers DrawBuffers;
            public TtLayerDrawBuffersScope(TtLayerDrawBuffers drawBuffers)
            {
                DrawBuffers = drawBuffers;
                DrawBuffers.BeginCommands();
            }
            public void Dispose()
            {
                DrawBuffers.EndCommands();
                DrawBuffers = null;
            }
        }
        public NxRHI.TtGpuPipeline mPipeline;
        public UDrawBuffers[] PassBuffers = new UDrawBuffers[(int)ERenderLayer.RL_Num];
        public UDrawBuffers PostCmds = new UDrawBuffers();
        public void Initialize(NxRHI.TtGpuDevice rc, string debugName)
        {
            for (ERenderLayer i = ERenderLayer.RL_Begin; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i] = new UDrawBuffers();
                PassBuffers[(int)i].Initialize(rc, $"{debugName}:{i.ToString()}");
                PassBuffers[(int)i].SetPipeline(mPipeline);
            }
            PostCmds = new UDrawBuffers();
            PostCmds.Initialize(rc, $"{debugName}:Post");
        }
        public void BeginCommands()
        {
            for (ERenderLayer i = ERenderLayer.RL_Begin; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i].DrawCmdList.BeginCommand();
            }
        }
        public void EndCommands()
        {
            for (ERenderLayer i = ERenderLayer.RL_Begin; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i].DrawCmdList.EndCommand();
            }
        }
        public void ExecuteCommands(TtRenderPolicy policy)
        {
            for (ERenderLayer i = ERenderLayer.RL_Begin; i < ERenderLayer.RL_Num; i++)
            {
                policy.CommitCommandList(PassBuffers[(int)i].DrawCmdList);
            }
        }
        public unsafe void SetViewport(in NxRHI.FViewPort vp)
        {
            for (ERenderLayer i = ERenderLayer.RL_Begin; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i].DrawCmdList.SetViewport(in vp);
                PassBuffers[(int)i].DrawCmdList.SetScissor(0, (NxRHI.FScissorRect*)0);
            }
        }
        public unsafe void BuildTranslucentRenderPass(TtRenderPolicy policy, in NxRHI.FRenderPassClears passClear, TtGraphicsBuffers frameBuffers, TtGraphicsBuffers gizmosFrameBuffers)
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
        public unsafe void BuildRenderPass(TtRenderPolicy policy, in NxRHI.FViewPort viewport, NxRHI.FRenderPassClears* pLayerClear, int numOfClears, TtGraphicsBuffers frameBuffers, TtGraphicsBuffers gizmosFrameBuffers, string debugName)
        {
            fixed(NxRHI.FViewPort* p = &viewport)
            {
                BuildRenderPass(policy, 1, p, pLayerClear, numOfClears, frameBuffers, gizmosFrameBuffers, debugName);
            }
        }
        public unsafe void BuildRenderPass(TtRenderPolicy policy, uint numOfViewport, NxRHI.FViewPort* viewport, NxRHI.FRenderPassClears* pLayerClear, int numOfClears, TtGraphicsBuffers frameBuffers, TtGraphicsBuffers gizmosFrameBuffers, string debugName)
        {
            NxRHI.FScissorRect* sr = stackalloc NxRHI.FScissorRect[(int)numOfViewport];
            for (int j = 0; j < numOfViewport; j++)
            {
                sr[j].m_MinX = (int)viewport[j].TopLeftX;
                sr[j].m_MinY = (int)viewport[j].TopLeftY;
                sr[j].m_MaxX = (int)(viewport[j].TopLeftX + viewport[j].Width);
                sr[j].m_MaxY = (int)(viewport[j].TopLeftY + viewport[j].Height);
            }

            for (ERenderLayer i = ERenderLayer.RL_Begin; i< ERenderLayer.RL_Num; i++)
            {
                var index = (int)i;
                var cmdlist = PassBuffers[index].DrawCmdList;
                PassBuffers[index].DrawcallNumber = cmdlist.DrawcallNumber;
                var bClear = pLayerClear[index].ClearFlags != 0;
                if (bClear == false && PassBuffers[index].DrawcallNumber == 0)
                {
                    //cmdlist.SetScissor(0, (NxRHI.FScissorRect*)0);
                    //cmdlist.SetScissor(numOfViewport, sr);
                    continue;
                }

                //frameBuffers.BuildFrameBuffers(policy);
                NxRHI.TtFrameBuffers fb = null;
                if (i == ERenderLayer.RL_Gizmos || i == ERenderLayer.RL_TranslucentGizmos)
                {
                    fb = gizmosFrameBuffers.FrameBuffers;
                }
                else
                {
                    fb = frameBuffers.FrameBuffers;
                }
                cmdlist.BeginPass(fb, pLayerClear[index], debugName + i.ToString());
                cmdlist.SetViewport(numOfViewport, viewport);
                cmdlist.SetScissor(numOfViewport, sr);

                cmdlist.FlushDraws();
                cmdlist.EndPass();
            }
        }
        public void SwapBuffer()
        {
            for (ERenderLayer i = ERenderLayer.RL_Begin; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i].SwapBuffer();
            }
            PostCmds.SwapBuffer();
        }
        public void PushDrawCall(ERenderLayer layer, NxRHI.TtGraphicDraw drawcall)
        {
            unsafe
            {
                PassBuffers[(int)layer].DrawCmdList.PushGpuDraw(drawcall);
            }
        }
        public NxRHI.UCommandList GetCmdList(ERenderLayer layer)
        {
            return PassBuffers[(int)layer].DrawCmdList;
        }
    }

}
