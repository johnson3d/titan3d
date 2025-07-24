using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
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
        public TtCmdRecorder[] PassBuffers = new TtCmdRecorder[(int)ERenderLayer.RL_Num];
        public TtCmdRecorder PostCmds = new TtCmdRecorder();
        public TtCmdRecorder GetCmdRecorder(ERenderLayer index)
        {
            return PassBuffers[(int)index];
        }
        public void Initialize(NxRHI.TtGpuDevice rc, string debugName)
        {
            for (ERenderLayer i = ERenderLayer.RL_Begin; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i] = new TtCmdRecorder();
            }
        }
        public void BeginCommands()
        {
            for (ERenderLayer i = ERenderLayer.RL_Begin; i < ERenderLayer.RL_Num; i++)
            {
                if (PassBuffers[(int)i]==null)
                    PassBuffers[(int)i] = new TtCmdRecorder();
                PassBuffers[(int)i].ResetGpuDraws();
            }
        }
        public void EndCommands()
        {
            for (ERenderLayer i = ERenderLayer.RL_Begin; i < ERenderLayer.RL_Num; i++)
            {
                PassBuffers[(int)i].ResetGpuDraws();
            }
        }
        public unsafe TtCommandList BuildTranslucentRenderPass(TtCommandList cmdlist, TtRenderPolicy policy, in NxRHI.FRenderPassClears passClear, TtGraphicsBuffers frameBuffers, TtGraphicsBuffers gizmosFrameBuffers)
        {
            if (PassBuffers[(int)ERenderLayer.RL_Translucent].DrawcallNumber > 0)
            {
                frameBuffers.BuildFrameBuffers(policy);
                cmdlist.BeginPass(frameBuffers.FrameBuffers, in passClear, ERenderLayer.RL_Translucent.ToString());
                cmdlist.AppendDraws(PassBuffers[(int)ERenderLayer.RL_Translucent]);
                cmdlist.FlushDraws();
                cmdlist.EndPass();
            }
            if (PassBuffers[(int)ERenderLayer.RL_Sky].DrawcallNumber > 0)
            {
                frameBuffers.BuildFrameBuffers(policy);
                var noClear = new NxRHI.FRenderPassClears();
                noClear.ClearFlags = 0;
                cmdlist.BeginPass(frameBuffers.FrameBuffers, in noClear, ERenderLayer.RL_Sky.ToString());
                cmdlist.AppendDraws(PassBuffers[(int)ERenderLayer.RL_Sky]);
                cmdlist.FlushDraws();
                cmdlist.EndPass();
            }
            if (PassBuffers[(int)ERenderLayer.RL_Gizmos].DrawcallNumber > 0 ||
                PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawcallNumber > 0)
            {
                gizmosFrameBuffers.BuildFrameBuffers(policy);
                cmdlist.BeginPass(gizmosFrameBuffers.FrameBuffers, in passClear, ERenderLayer.RL_Gizmos.ToString());
                cmdlist.AppendDraws(PassBuffers[(int)ERenderLayer.RL_Gizmos]);
                cmdlist.FlushDraws();
                cmdlist.EndPass();
            }
            if (PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos].DrawcallNumber > 0)
            {
                gizmosFrameBuffers.BuildFrameBuffers(policy);
                var noClear = new NxRHI.FRenderPassClears();
                noClear.ClearFlags = 0;
                cmdlist.BeginPass(gizmosFrameBuffers.FrameBuffers, in noClear, ERenderLayer.RL_TranslucentGizmos.ToString());
                cmdlist.AppendDraws(PassBuffers[(int)ERenderLayer.RL_TranslucentGizmos]);
                cmdlist.FlushDraws();
                cmdlist.EndPass();
            }
            return cmdlist;
        }
        public unsafe TtCommandList BuildRenderPass(TtCommandList cmdlist, TtRenderPolicy policy, in NxRHI.FViewPort viewport, NxRHI.FRenderPassClears* pLayerClear, int numOfClears, TtGraphicsBuffers frameBuffers, TtGraphicsBuffers gizmosFrameBuffers, string debugName)
        {
            fixed(NxRHI.FViewPort* p = &viewport)
            {
                return BuildRenderPass(cmdlist, policy, 1, p, pLayerClear, numOfClears, frameBuffers, gizmosFrameBuffers, debugName);
            }
        }
        public unsafe TtCommandList BuildRenderPass(TtCommandList cmdlist, TtRenderPolicy policy, uint numOfViewport, NxRHI.FViewPort* viewport, NxRHI.FRenderPassClears* pLayerClear, int numOfClears, TtGraphicsBuffers frameBuffers, TtGraphicsBuffers gizmosFrameBuffers, string debugName)
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

                cmdlist.AppendDraws(PassBuffers[index]);

                cmdlist.FlushDraws();
                cmdlist.EndPass();
            }
            return cmdlist;
        }
    }

}
