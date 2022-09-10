using SDL2;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui
{
    public class UUvAnimEditor : Editor.IAssetEditor, Graphics.Pipeline.IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public UUvAnim UvAnim;
        public EGui.Controls.PropertyGrid.PropertyGrid UvAnimPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        ~UUvAnimEditor()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            UvAnimPropGrid.Target = null;
            UvAnim = null;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            return await UvAnimPropGrid.Initialize();
        }
        public Graphics.Pipeline.IRootForm GetRootForm()
        {
            return this;
        }
        public async System.Threading.Tasks.Task<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            UvAnim = await UEngine.Instance.GfxDevice.UvAnimManager.CreateUVAnim(name);
            if (UvAnim == null)
                return false;

            UvAnimPropGrid.Target = UvAnim;
            if (UvAnim.mTexture != null)
            {
                ImageSize.X = UvAnim.mTexture.PicDesc.Width;
                ImageSize.Y = UvAnim.mTexture.PicDesc.Height;
            }
            return true;
        }
        public void OnCloseEditor()
        {
            Cleanup();
        }
        public float LeftWidth = 0;
        public Vector2 WindowSize = new Vector2(800, 600);
        public Vector2 ImageSize = new Vector2(512, 512);
        public Vector2 ViewStart = new Vector2(0, 0);
        public float ScaleFactor = 1.0f;
        public unsafe void OnDraw()
        {
            if (Visible == false || UvAnim == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin(UvAnim.AssetName.Name, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
            {
                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
                DrawToolBar();
                ImGuiAPI.Separator();
                ImGuiAPI.Columns(2, null, true);
                if (LeftWidth == 0)
                {
                    ImGuiAPI.SetColumnWidth(0, 300);
                }
                LeftWidth = ImGuiAPI.GetColumnWidth(0);

                var min = ImGuiAPI.GetWindowContentRegionMin();
                var max = ImGuiAPI.GetWindowContentRegionMin();

                DrawLeft(ref min, ref max);
                ImGuiAPI.NextColumn();

                DrawRight(ref min, ref max);
                ImGuiAPI.NextColumn();

                ImGuiAPI.Columns(1, null, true);
            }
            ImGuiAPI.End();
        }
        int CurFrameIndex = 0;
        protected void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (ImGuiAPI.Button("Save", in btSize))
            {
                UvAnim.SaveAssetTo(UvAnim.AssetName);
                var unused = UEngine.Instance.GfxDevice.UvAnimManager.ReloadUVAnim(UvAnim.AssetName);
            }
            ImGuiAPI.SameLine(0, 100);
            if (ImGuiAPI.Checkbox("ShowFrames", ref mIsShowFrames))
            {

            }
            if (ImGuiAPI.Checkbox("WriteFrame", ref mIsWriteFrame))
            {

            }
            ImGuiAPI.SameLine(0, 100);
            ImGuiAPI.Text("Index");
            ImGuiAPI.SameLine(0, 0);
            ImGuiAPI.SetNextItemWidth(100);
            if (ImGuiAPI.InputInt("##FrameIndex", ref CurFrameIndex, 1, 0, ImGuiInputTextFlags_.ImGuiInputTextFlags_None))
            {
                CurFrameIndex = CurFrameIndex % UvAnim.FrameUVs.Count;
            }
            ImGuiAPI.SameLine(0, 0);
            if (ImGuiAPI.Button("AddFrame"))
            {
                UvAnim.FrameUVs.Insert(CurFrameIndex, new Vector4());
            }
            ImGuiAPI.SameLine(0, 0);
            if (ImGuiAPI.Button("RemoveFrame"))
            {
                UvAnim.FrameUVs.RemoveAt(CurFrameIndex);
            }
        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            var sz = new Vector2(-1);
            if (ImGuiAPI.BeginChild("LeftView", in sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                UvAnimPropGrid.OnDraw(true, false, false);
            }
            ImGuiAPI.EndChild();
        }
        bool IsMovingImage = false;
        bool IsSelectingRect = false;
        Vector2 MovingPivot;
        Vector2 SelectBegin;
        Vector2 SelectEnd;
        bool mIsShowFrames = false;
        bool mIsWriteFrame = false;
        public bool IsShowFrames { get => mIsShowFrames; set => mIsShowFrames = value; }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            var sz = new Vector2(-1);
            if (ImGuiAPI.BeginChild("TextureView", in sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
            {
                var winPt = ImGuiAPI.GetWindowPos();
                if (ImGuiAPI.IsWindowHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                {
                    if (ImGuiAPI.GetIO().MouseWheel != 0)
                    {
                        ScaleFactor += ImGuiAPI.GetIO().MouseWheel * 0.1f;

                        if (ScaleFactor <= 0.1f)
                            ScaleFactor = 0.1f;
                        if (ScaleFactor >= 3.0f)
                            ScaleFactor = 3.0f;
                    }
                    if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Middle))
                    {
                        if (IsMovingImage == false && ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Middle, 5.0f))
                        {
                            IsMovingImage = true;
                            var MoveStartPosition = ImGuiAPI.GetMousePos() - winPt;
                            MovingPivot = MoveStartPosition - ViewStart;
                        }
                        else if (IsMovingImage)
                        {
                            var curPos = ImGuiAPI.GetMousePos() - winPt;

                            ViewStart = curPos - MovingPivot;
                        }
                    }
                    else
                    {
                        IsMovingImage = false;

                        if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left))
                        {
                            if (IsSelectingRect == false && ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, 5.0f))
                            {
                                IsSelectingRect = true;
                                var curPos = ImGuiAPI.GetMousePos() - winPt;

                                SelectBegin = (curPos - ViewStart) / ScaleFactor;
                            }
                            else if (IsSelectingRect)
                            {
                                var curPos = ImGuiAPI.GetMousePos() - winPt;
                                SelectEnd = (curPos - ViewStart) / ScaleFactor;
                            }
                        }
                        else
                        {
                            if (IsSelectingRect)// && ImGuiAPI.IsKeyDown((int)ImGuiKey_.ImGuiKey_LeftCtrl))
                            {
                                if (mIsWriteFrame && CurFrameIndex >= 0 && CurFrameIndex < UvAnim.FrameUVs.Count)
                                {
                                    float u1 = SelectBegin.X / ImageSize.X;
                                    float v1 = SelectBegin.Y / ImageSize.Y;
                                    float u2 = SelectEnd.X / ImageSize.X;
                                    float v2 = SelectEnd.Y / ImageSize.Y;

                                    UvAnim.FrameUVs[CurFrameIndex] = new Vector4(u1, v1, u2 - u1, v2 - v1);
                                }
                            }
                            IsSelectingRect = false;
                        }
                    }
                }
                
                
                var drawlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
                var uv1 = new Vector2(0, 0);
                var uv2 = new Vector2(1, 1);
                var min1 = ImGuiAPI.GetWindowContentRegionMin() + ViewStart;
                var max1 = min1 + ImageSize * ScaleFactor;

                min1 = min1 + winPt;
                max1 = max1 + winPt;
                if (UvAnim.mTexture != null)
                {
                    drawlist.AddImage(UvAnim.mTexture.GetTextureHandle().ToPointer(), in min1, in max1, in uv1, in uv2, 0xFFFFFFFF);
                    if (IsSelectingRect)
                    {
                        var sb = SelectBegin * ScaleFactor + min1;
                        var se = SelectEnd * ScaleFactor + min1;
                        drawlist.AddRect(in sb, in se, 0xFFFFFFFF, 1, ImDrawFlags_.ImDrawFlags_None, 1);
                    }

                    if (IsShowFrames)
                    {
                        for (int i = 0; i < UvAnim.FrameUVs.Count; i++)
                        {
                            Vector2 sb, se;
                            UvAnim.GetUV(i, out sb, out se);
                            sb *= ImageSize;
                            se *= ImageSize;

                            sb = sb * ScaleFactor + min1;
                            se = se * ScaleFactor + min1;

                            drawlist.AddRect(in sb, in se, (uint)Color.Green.ToArgb(), 1, ImDrawFlags_.ImDrawFlags_None, 1);
                            drawlist.AddText(in sb, (uint)Color.White.ToArgb(), i.ToString(), null);
                        }
                    }
                }
            }
            ImGuiAPI.EndChild();
        }
        public void OnEvent(ref SDL.SDL_Event e)
        {
            
        }
    }
}
