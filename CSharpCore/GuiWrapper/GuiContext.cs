using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;

namespace EngineNS.GuiWrapper
{
    public class GuiContext
    {
        public GuiContext()
        {
            mContext = ImGui.CreateContext();
        }
        ~GuiContext()
        {
            if (mContext != IntPtr.Zero)
            {
                ImGui.DestroyContext(mContext);
                mContext = IntPtr.Zero;
            }
        }
        IntPtr mContext;
        public IntPtr Context
        {
            get { return mContext; }
        }
        public void SetFont(RName fontname)
        {
            var fonts = ImGui.GetIO().Fonts;
            fonts.AddFontDefault();

            FontTexId = GuiManager.Instance.GetFontMaterial(fontname, this).Ptr;
            fonts.SetTexID(FontTexId);
        }
        public IntPtr FontTexId
        {
            get;
            private set;
        }
        public void TickLogic(float deltaSeconds)
        {
            ImGui.SetCurrentContext(mContext);

            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData());

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput(ImGui.GetDrawData());

            SubmitUI();
        }
        public void SubmitUI()
        {
            ImGui.NewFrame();

            //
        }
        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {

        }
        public Vector2 WindowSize
        {
            get;
            private set;
        }

        public Vector2 ScaleFactor
        {
            get;
            set;
        }
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                WindowSize.X / ScaleFactor.X,
                WindowSize.Y / ScaleFactor.Y);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(ScaleFactor.X, ScaleFactor.Y);
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }
        Support.NativeList<ImDrawVert> mVertexData = new Support.NativeList<ImDrawVert>();
        Support.NativeList<ushort> mIndexData = new Support.NativeList<ushort>();
        private unsafe void UpdateImGuiInput(ImDrawDataPtr draw_data)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            mVertexData.Clear(false);
            mIndexData.Clear(false);
            if (draw_data.TotalVtxCount > mVertexData.Count)
            {
                mVertexData.SetCapacity(draw_data.TotalVtxCount);
            }
            if (draw_data.TotalIdxCount > mIndexData.Count)
            {
                mIndexData.SetCapacity(draw_data.TotalIdxCount);
            }

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);
            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];
                var vbData = cmd_list.VtxBuffer.Data;
                var ibData = cmd_list.IdxBuffer.Data;
                mVertexData.Append((ImDrawVert*)vbData.ToPointer(), cmd_list.VtxBuffer.Size);
                mIndexData.Append((ushort*)ibData.ToPointer(), cmd_list.IdxBuffer.Size);
            }

            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];
                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        if (pcmd.TextureId != IntPtr.Zero)
                        {
                            //if (pcmd.TextureId == _fontAtlasID)
                            //{
                            //    cl.SetGraphicsResourceSet(1, _fontTextureResourceSet);
                            //}
                            //else
                            //{
                            //    cl.SetGraphicsResourceSet(1, GetImageResourceSet(pcmd.TextureId));
                            //}
                        }

                        //cl.SetScissorRect(
                        //    0,
                        //    (uint)pcmd.ClipRect.X,
                        //    (uint)pcmd.ClipRect.Y,
                        //    (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                        //    (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y));

                        //cl.DrawIndexed(pcmd.ElemCount, 1, (uint)idx_offset, vtx_offset, 0);
                    }
                    idx_offset += (int)pcmd.ElemCount;
                }
                vtx_offset += cmd_list.VtxBuffer.Size;
            }
        }
    }
}
