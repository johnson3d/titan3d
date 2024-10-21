using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui
{
    public class UUvAnimEditor : Editor.IAssetEditor, IRootForm
    {
        public RName AssetName { get; set; }
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public TtUVAnim UvAnim;
        public EGui.Controls.PropertyGrid.PropertyGrid UvAnimPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        ~UUvAnimEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            UvAnimPropGrid.Target = null;
            UvAnim = null;
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            return await UvAnimPropGrid.Initialize();
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Thread.Async.TtTask<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            UvAnim = await TtEngine.Instance.GfxDevice.UvAnimManager.CreateUVAnim(name);
            if (UvAnim == null)
                return false;

            UvAnimPropGrid.Target = UvAnim;
            if (UvAnim.Texture != null)
            {
                ImageSize.X = UvAnim.Texture.PicDesc.Width;
                ImageSize.Y = UvAnim.Texture.PicDesc.Height;
            }
            return true;
        }
        public void OnCloseEditor()
        {
            Dispose();
        }
        bool mDockInitialized = false;
        protected void ResetDockspace(bool force = false)
        {
            var pos = ImGuiAPI.GetCursorPos();
            var id = ImGuiAPI.GetID(AssetName.Name + "_Dockspace");
            mDockKeyClass.ClassId = id;
            ImGuiAPI.DockSpace(id, Vector2.Zero, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, mDockKeyClass);
            if (mDockInitialized && !force)
                return;
            ImGuiAPI.DockBuilderRemoveNode(id);
            ImGuiAPI.DockBuilderAddNode(id, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None);
            ImGuiAPI.DockBuilderSetNodePos(id, pos);
            ImGuiAPI.DockBuilderSetNodeSize(id, Vector2.One);
            mDockInitialized = true;

            var rightId = id;
            uint leftId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref rightId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Property", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("TextureView", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderFinish(id);
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
            var result = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (result)
            {
                DrawToolBar();
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(result);

            DrawProperty();
            DrawTextureView();
        }
        int CurFrameIndex = 0;
        protected void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                UvAnim.SaveAssetTo(UvAnim.AssetName);
                var unused = TtEngine.Instance.GfxDevice.UvAnimManager.ReloadUVAnim(UvAnim.AssetName);
            }
            ImGuiAPI.SameLine(0, 100);
            if (EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox("ShowFrames", ref mIsShowFrames))
            {

            }
            if (EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox("WriteFrame", ref mIsWriteFrame))
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
        bool mPropertyShow = true;
        protected unsafe void DrawProperty()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Property", ref mPropertyShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                UvAnimPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool IsMovingImage = false;
        bool IsSelectingRect = false;
        Vector2 MovingPivot;
        Vector2 SelectBegin;
        Vector2 SelectEnd;
        bool mIsShowFrames = false;
        bool mIsWriteFrame = false;
        bool mTextureViewShow = true;
        public bool IsShowFrames { get => mIsShowFrames; set => mIsShowFrames = value; }
        protected unsafe void DrawTextureView()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "TextureView", ref mTextureViewShow, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
            if (show)
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
                if (UvAnim.Texture != null)
                {
                    drawlist.AddImage((ulong)UvAnim.Texture.GetTextureHandle(), in min1, in max1, in uv1, in uv2, 0xFFFFFFFF);
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

                            drawlist.AddRect(in sb, in se, (uint)Color4b.Green.ToArgb(), 1, ImDrawFlags_.ImDrawFlags_None, 1);
                            drawlist.AddText(in sb, (uint)Color4b.White.ToArgb(), i.ToString(), null);
                        }
                    }
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        public void OnEvent(in Bricks.Input.Event e)
        {
            
        }

        public string GetWindowsName()
        {
            return UvAnim.AssetName.Name;
        }
    }
}
