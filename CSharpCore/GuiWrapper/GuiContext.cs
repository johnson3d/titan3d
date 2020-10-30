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
        private CCommandList[] CmdList = new CCommandList[2];
        public CCommandList DrawList
        {
            get { return CmdList[0]; }
        }
        public CCommandList CommitList
        {
            get { return CmdList[1]; }
        }
        public void TickSync()
        {
            var saved = CmdList[0];
            CmdList[0] = CmdList[1];
            CmdList[1] = saved;
        }
        public async System.Threading.Tasks.Task<bool> Initialize(UIntPtr winHandle, RName fontname)
        {
            ImGui.SetCurrentContext(mContext);
            var rc = CEngine.Instance.RenderContext;
            var desc = new CCommandListDesc();
            CmdList[0] = rc.CreateCommandList(desc);
            CmdList[1] = rc.CreateCommandList(desc);

            await SetFont(fontname);

            ImGui.NewFrame();
            mFrameBegun = true;
            return true;
        }
        public async System.Threading.Tasks.Task SetFont(RName fontname)
        {
            var fonts = ImGui.GetIO().Fonts;
            fonts.AddFontDefault();

            var handle = await GuiManager.Instance.GetFontMaterial(fontname, this);
            FontTexId = handle.Ptr;
            fonts.SetTexID(FontTexId);
        }
        public IntPtr FontTexId
        {
            get;
            private set;
        }
        private bool mFrameBegun = false;
        public void TickLogic(float deltaSeconds, FSubmitUI drawFunc)
        {
            ImGui.SetCurrentContext(mContext);

            if (mFrameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput();

            mFrameBegun = true;
            ImGui.NewFrame();
            if (drawFunc!=null)
                drawFunc();

            if (mFrameBegun)
            {
                mFrameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }
        public delegate void FSubmitUI();
        CVertexBuffer VertexBuffer = null;
        CIndexBuffer IndexBuffer = null;
        private unsafe void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            var rc = CEngine.Instance.RenderContext;

            ImGuiIOPtr io = ImGui.GetIO();
            mVertexData.Clear(false);
            mIndexData.Clear(false);
            if (draw_data.TotalVtxCount > mVertexData.Count)
            {
                mVertexData.SetCapacity(draw_data.TotalVtxCount);
                var vbDesc = new CVertexBufferDesc();
                vbDesc.InitData = IntPtr.Zero;
                vbDesc.Stride = (UInt32)sizeof(ImDrawVert);
                vbDesc.ByteWidth = (UInt32)(sizeof(ImDrawVert) * draw_data.TotalVtxCount);
                VertexBuffer = rc.CreateVertexBuffer(vbDesc);
            }
            if (draw_data.TotalIdxCount > mIndexData.Count)
            {
                mIndexData.SetCapacity(draw_data.TotalIdxCount);
                var ibDesc = new CIndexBufferDesc();
                ibDesc.ToDefault();
                ibDesc.InitData = IntPtr.Zero;
                ibDesc.ByteWidth = (UInt32)(sizeof(ushort) * draw_data.TotalIdxCount);
                IndexBuffer = rc.CreateIndexBuffer(ibDesc);
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

            VertexBuffer.UpdateBuffData(DrawList, mVertexData.GetBufferPtr(), (uint)(mVertexData.Count * sizeof(ImDrawVert)));
            IndexBuffer.UpdateBuffData(DrawList, mIndexData.GetBufferPtr(), (uint)(mIndexData.Count * sizeof(ushort)));

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
                            var mtlHandle = new THandle<Graphics.CGfxMaterialInstance>(pcmd.TextureId);
                            var mtl = mtlHandle.Get();
                            mtl.FindSRVIndex("txDiffuse");
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
        public Vector2 WindowSize
        {
            get;
            private set;
        }
        public void WindowResized(int width, int height)
        {
            WindowSize = new Vector2((float)width, (float)height);
        }

        public Vector2 ScaleFactor
        {
            get;
            set;
        } = new Vector2(1, 1);
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
        private void UpdateImGuiInput()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            
        }
    }

    public class UnitTest_GuiContext : BrickDescriptor
    {
        public override async System.Threading.Tasks.Task DoTest()
        {
            var context = new GuiContext();
            await context.Initialize(UIntPtr.Zero, RName.GetRName("font/msyh.ttf"));
            context.WindowResized(1024, 1024);

            for (int i = 0; i < 10; i++)
            {
                context.TickLogic(0.1f, () =>
                {
                    ImGui.Text("Hello, world!");
                });
            }
        }
    }
}
