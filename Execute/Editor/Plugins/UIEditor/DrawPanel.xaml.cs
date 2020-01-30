using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UIEditor
{
    /// <summary>
    /// DrawPanel.xaml 的交互逻辑
    /// </summary>
    public partial class DrawPanel : UserControl
    {
        internal DesignPanel HostDesignPanel;

        public DrawPanel()
        {
            InitializeComponent();

            Viewport.AfterInitializedAction = InitViewport;
        }
        private void Viewport_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var delta = 1 + e.Delta * 0.001f;
            var tempScale = mUIScale * delta;
            if (tempScale > 10f)
            {
                tempScale = 10;
                delta = tempScale / mUIScale;
            }
            else if(tempScale < 0.1f)
            {
                tempScale = 0.1f;
                delta = tempScale / mUIScale;
            }
            mUIScale = tempScale;

            var center = new EngineNS.Vector2(e.Location.X, e.Location.Y);
            var deltaXY = center * delta;
            mGridDrawSize.X = (mGridDrawSize.X / center.X) * deltaXY.X + center.X - deltaXY.X;
            mGridDrawSize.Y = (mGridDrawSize.Y / center.Y) * deltaXY.Y + center.Y - deltaXY.Y;
            mGridDrawSize.Width = mGridDrawSize.Width * delta;
            mGridDrawSize.Height = mGridDrawSize.Height * delta;

            if (mGridDrawSize.Width > 200)
                mGridDrawSize.Width /= 2;
            else if (mGridDrawSize.Width < 50)
                mGridDrawSize.Width *= 2;
            if (mGridDrawSize.Height > 200)
                mGridDrawSize.Height /= 2;
            else if (mGridDrawSize.Height < 50)
                mGridDrawSize.Height *= 2;

            mWindowDrawSize.X = (mWindowDrawSize.X / center.X) * deltaXY.X + center.X - deltaXY.X;
            mWindowDrawSize.Y = (mWindowDrawSize.Y / center.Y) * deltaXY.Y + center.Y - deltaXY.Y;
            mWindowDrawSize.Width = mWindowDrawSize.Width * delta;
            mWindowDrawSize.Height = mWindowDrawSize.Height * delta;

            UpdateUIShow();
            UpdateUIAssistShow();
        }
        
        EngineNS.RectangleF mGridDrawSize = new EngineNS.RectangleF(0, 0, 100, 100);
        EngineNS.RectangleF mStartDrawSize;
        EngineNS.Vector2 mGridOffset = EngineNS.Vector2.Zero;
        float mUIScale = 1.0f;
        EngineNS.RectangleF mStartWindowDrawSize;
        EngineNS.RectangleF mWindowDrawSize = new EngineNS.RectangleF(0, 0, 1920, 1080);    // UI当前绘制分辨率
        EngineNS.RectangleF mWindowDesignSize = new EngineNS.RectangleF(0, 0, 1920, 1080);  // UI当前设计分辨率
        EngineNS.Matrix mControlRenderMat = EngineNS.Matrix.Identity;                       // UI的绘制矩阵
        private void Viewport_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateUIAssistShow();
        }
        void ShowAll()
        {
            var width = Viewport.GetViewPortWidth();
            var height = Viewport.GetViewPortHeight();
            var oldWidth = mWindowDrawSize.Width;
            var whDelta = mWindowDrawSize.Height / mWindowDrawSize.Width;
            mWindowDrawSize.Width = width * 0.8f;
            mWindowDrawSize.Height = mWindowDrawSize.Width * whDelta;
            mWindowDrawSize.X = width * 0.5f - mWindowDrawSize.Width * 0.5f;
            mWindowDrawSize.Y = height * 0.5f - mWindowDrawSize.Height * 0.5f;
            mUIScale = mWindowDrawSize.Width / oldWidth;
            if (mWindowRectUIShow != null)
            {
                mWindowRectUIShow.SetDesignRect(ref mWindowDrawSize, true);
            }
            UpdateUIShow();
        }
        void UpdateUIAssistShow()
        {
            var width = Viewport.GetViewPortWidth();
            var height = Viewport.GetViewPortHeight();
            if (mGridlineImage2D != null)
            {
                // GridLine
                mGridlineImage2D.RenderMatrix = EngineNS.Matrix.Scaling(width, height, 1);

                var widthDelta = (float)width / mGridDrawSize.Width;
                var heightDelta = (float)height / mGridDrawSize.Height;
                var tempPass = mGridlineImage2D.GetPass();
                var shaderIdx_tile = mGridlineImage2D.Mesh.FindVar(0, "GridTile");
                var shaderIdx_offset = mGridlineImage2D.Mesh.FindVar(0, "UVoffset");

                var snapTile = new EngineNS.Vector2(widthDelta, heightDelta);
                mGridlineImage2D.Mesh.MtlMeshArray[0].CBuffer.SetValue((int)shaderIdx_tile, snapTile, 0);
                var offset = new EngineNS.Vector2(-mGridDrawSize.X / width, -mGridDrawSize.Y / height);
                mGridlineImage2D.Mesh.MtlMeshArray[0].CBuffer.SetValue((int)shaderIdx_offset, offset, 0);
            }

            // WindowRect
            if(mWindowRectUIShow != null)
            {
                mWindowRectUIShow.SetDesignRect(ref mWindowDrawSize, true);
            }
            UpdatePointAtShow();
            UpdateSelectedRectShow();
        }
        void UpdateUIShow()
        {
            mControlRenderMat = EngineNS.Matrix.Scaling(mWindowDrawSize.Width / mWindowDesignSize.Width, mWindowDrawSize.Height / mWindowDesignSize.Height, 1.0f) * EngineNS.Matrix.Translate(mWindowDrawSize.X, mWindowDrawSize.Y, 0.0f); ;
        }
        async Task InitPanelRectsShow()
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            mPanelRectsShow.Clear();
            await AddPanelRectShow(rc, HostDesignPanel.CurrentUI);
        }
        internal async Task AddPanelRectShow(EngineNS.CRenderContext rc, EngineNS.UISystem.UIElement uiCtrl)
        {
            var panel = uiCtrl as EngineNS.UISystem.Controls.Containers.Panel;

            if (panel != null)
            {
                var imgInit = new EngineNS.UISystem.Controls.ImageInitializer();
                imgInit.ImageBrush.ImageName = EngineNS.RName.GetRName("uieditor/uvanim_dottedline.uvanim", EngineNS.RName.enRNameType.Editor);
                imgInit.ImageBrush.TileMode = EngineNS.UISystem.Brush.enTileMode.Both;
                imgInit.ImageBrush.Color = new EngineNS.Color4(0.8f, 1.0f, 1.0f, 1.0f);
                var show = new EngineNS.UISystem.Controls.Image();
                await show.Initialize(rc, imgInit);
                mPanelRectsShow[panel] = show;

                if(uiCtrl == HostDesignPanel.CurrentUI || !(uiCtrl is EngineNS.UISystem.Controls.UserControl))
                {
                    foreach (var child in panel.ChildrenUIElements)
                    {
                        await AddPanelRectShow(rc, child);
                    }
                }
            }
        }
        void UpdatePanelRectsShow()
        {
            if (mPanelRectsShow.Count == 0)
                return;

            foreach (var rect in mPanelRectsShow)
            {
                var uiElement = rect.Key;
                var show = rect.Value;

                var leftDelta = uiElement.DesignRect.X / mWindowDesignSize.Width;
                var topDelta = uiElement.DesignRect.Y / mWindowDesignSize.Height;
                var widthDelta = uiElement.DesignRect.Width / mWindowDesignSize.Width;
                var heightDelta = uiElement.DesignRect.Height / mWindowDesignSize.Height;
                var dRect = new EngineNS.RectangleF(mWindowDrawSize.Width * leftDelta + mWindowDrawSize.Left - 1,
                                                    mWindowDrawSize.Height * topDelta + mWindowDrawSize.Top - 1,
                                                    mWindowDrawSize.Width * widthDelta + 2,
                                                    mWindowDrawSize.Height * heightDelta + 2);
                if (!show.DesignRect.Equals(ref dRect))
                    show.SetDesignRect(ref dRect, true);
            }
        }

        public async Task WaitViewportInitComplated()
        {
            await Viewport.WaitInitComplated();
        }

        EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile mRP_EditorMobile;
        bool mViewPortInited = false;
        async Task InitViewport(EditorCommon.ViewPort.ViewPortControl vpCtrl)
        {
            if (mViewPortInited)
                return;
            mViewPortInited = true;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            mRP_EditorMobile = new EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile();
            var width = (uint)vpCtrl.GetViewPortWidth();
            var height = (uint)vpCtrl.GetViewPortHeight();
            await mRP_EditorMobile.Init(rc, width, height, Viewport.Camera, vpCtrl.DrawHandle);
            vpCtrl.RPolicy = mRP_EditorMobile;

            mRP_EditorMobile.mHitProxy.mEnabled = false;

            Viewport.SizeChanged += Viewport_SizeChanged;
            vpCtrl.TickLogicEvent = Viewport_TickLogic;
            Viewport.SetDrawPanelMouseWheelCallback(Viewport_MouseWheel);
            Viewport.SetDrawPanelMouseDownCallback(Viewport_MouseDown);
            Viewport.SetDrawPanelMouseUpCallback(Viewport_MouseUp);
            Viewport.SetDrawPanelMouseMoveCallback(Viewport_MouseMove);
            Viewport.SetDrawPanelDragEnterCallback(Viewport_DragEnter);
            Viewport.SetDrawPanelDragLeaveCallback(Viewport_DragLeave);
            Viewport.SetDrawPanelDragOverCallback(Viewport_DragOver);
            Viewport.SetDrawPanelDragDropCallback(Viewport_DragDrop);
        }
        void Viewport_TickLogic(EditorCommon.ViewPort.ViewPortControl vpc)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if (vpc.World != null)
            {
                vpc.World.CheckVisible(rc.ImmCommandList, vpc.Camera);
                vpc.World.Tick();
            }
            vpc.RPolicy.TickLogic(null, rc);

            var idMat = EngineNS.Matrix.Identity;
            mGridlineImage2D?.UpdateForEditerMode(rc);

            //EngineNS.SizeF tagDesignSize;
            //var scale = EngineNS.CEngine.Instance.UIManager.Config.GetDPIScaleAndDesignSize(mWindowDesignSize.Width, mWindowDesignSize.Height, out tagDesignSize);
            var scale = 1.0f;
            mWindowRectUIShow?.Commit(rc.ImmCommandList, ref idMat, 1.0f);

            UpdatePanelRectsShow();
            foreach (var rect in mPanelRectsShow.Values)
            {
                rect.Commit(rc.ImmCommandList, ref idMat, 1.0f);
            }

            HostDesignPanel?.CurrentUI?.Commit(rc.ImmCommandList, ref mControlRenderMat, scale);
            mMousePointAtUIRectShow?.Commit(rc.ImmCommandList, ref idMat, 1.0f);
            UpdateSelectedRectShow();
            foreach (var data in mSelectedUIDatas.Values)
            {
                data.ShowRect?.Commit(rc.ImmCommandList, ref idMat, 1.0f);
            }
            mSlotOperator?.Commit(rc.ImmCommandList);
        }

        EngineNS.Bricks.FreeTypeFont.CFontMesh mScaleFontMesh;
        EngineNS.Graphics.Mesh.CGfxImage2D mGridlineImage2D;
        EngineNS.UISystem.Controls.Image mWindowRectUIShow;
        Dictionary<EngineNS.UISystem.UIElement, EngineNS.UISystem.Controls.Image> mPanelRectsShow = new Dictionary<EngineNS.UISystem.UIElement, EngineNS.UISystem.Controls.Image>();
        public async Task SetObjectToEditor(EngineNS.CRenderContext rc, EditorCommon.Resources.ResourceEditorContext context)
        {
            await Viewport.WaitInitComplated();

            if (mWindowRectUIShow == null)
            {
                var imgInit = new EngineNS.UISystem.Controls.ImageInitializer();
                imgInit.ImageBrush.ImageName = EngineNS.RName.GetRName("uieditor/uvanim_dottedline.uvanim", EngineNS.RName.enRNameType.Editor);
                imgInit.ImageBrush.TileMode = EngineNS.UISystem.Brush.enTileMode.Both;
                mWindowRectUIShow = new EngineNS.UISystem.Controls.Image();
                await mWindowRectUIShow.Initialize(rc, imgInit);
            }
            if(mMousePointAtUIRectShow == null)
            {
                var imgInit = new EngineNS.UISystem.Controls.ImageInitializer();
                imgInit.ImageBrush.ImageName = EngineNS.RName.GetRName("uieditor/uva_pointatrect.uvanim", EngineNS.RName.enRNameType.Editor);
                imgInit.ImageBrush.TileMode = EngineNS.UISystem.Brush.enTileMode.None;
                mMousePointAtUIRectShow = new EngineNS.UISystem.Controls.Image();
                await mMousePointAtUIRectShow.Initialize(rc, imgInit);
                mMousePointAtUIRectShow.Visibility = EngineNS.UISystem.Visibility.Collapsed;
            }
            if (mGridlineImage2D == null)
            {
                mGridlineImage2D = await EngineNS.Graphics.Mesh.CGfxImage2D.CreateImage2D(rc, EngineNS.RName.GetRName("uieditor/mi_background_grid.instmtl", EngineNS.RName.enRNameType.Editor), 0, 0, 0, 1, 1);
                mGridlineImage2D.RenderMatrix = EngineNS.Matrix.Scaling(Viewport.GetViewPortWidth(), Viewport.GetViewPortHeight(), 1);
            }
            await InitPanelRectsShow();

            var font = EngineNS.CEngine.Instance.FontManager.GetFont(EngineNS.CEngine.Instance.Desc.DefaultFont, 12, 1024, 128);
            if (mScaleFontMesh == null)
            {
                var mtl = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.RName.GetRName("Material/font.instmtl"));
                mScaleFontMesh = new EngineNS.Bricks.FreeTypeFont.CFontMesh();
                await mScaleFontMesh.SetMaterial(rc, mtl, "txDiffuse");

                mScaleFontMesh.RenderMatrix = EngineNS.Matrix.Translate(20, 20, 0);
                //mScaleFontMesh.Offset = new EngineNS.Vector2(20, 20);
                //mScaleFontMesh.Scale = new EngineNS.Vector2(1, 1);
            }

            //var color = new EngineNS.FrameBufferClearColor();
            //color.r = 0.1f;
            //color.g = 0.1f;
            //color.b = 0.1f;
            //color.a = 1.0f;
            //Viewport.RPolicy.SetClearColorRT(0, ref color);
            var smp = EngineNS.Thread.ASyncSemaphore.CreateSemaphore(1);
            Viewport.RPolicy.OnDrawUI += (cmd, view) =>
            {
                var mtlmesh = mGridlineImage2D.Mesh.MtlMeshArray[0];
                var pass = mGridlineImage2D.GetPass();
                pass.ViewPort = view.Viewport;
                if (pass.RenderPipeline == null)
                {
                    var rplDesc = new EngineNS.CRenderPipelineDesc();
                    pass.RenderPipeline = rc.CreateRenderPipeline(rplDesc);
                }
                pass.RenderPipeline.RasterizerState = mtlmesh.MtlInst.CustomRasterizerState;
                pass.RenderPipeline.DepthStencilState = mtlmesh.MtlInst.CustomDepthStencilState;
                pass.RenderPipeline.BlendState = mtlmesh.MtlInst.CustomBlendState;
                //pass.ShaderSamplerBinder = mtlmesh.GetSamplerBinder(rc, pass.Effect.ShaderProgram);
                pass.BindCBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_View, view.ScreenViewCB);
                pass.ShadingEnv.BindResources(mGridlineImage2D.Mesh, pass);
                cmd.PushPass(pass);

                mWindowRectUIShow.Draw(rc, cmd, view);

                foreach(var rect in mPanelRectsShow.Values)
                {
                    rect.Draw(rc, cmd, view);
                }

                HostDesignPanel.CurrentUI.Draw(rc, cmd, view);
                mMousePointAtUIRectShow.Draw(rc, cmd, view);
                foreach(var data in mSelectedUIDatas.Values)
                {
                    data.ShowRect?.Draw(rc, cmd, view);
                }
                mSlotOperator?.Draw(rc, cmd, view);
                mScaleFontMesh.DrawText(rc, font, "缩放:" + mUIScale.ToString("F"), true);
                for (int i = 0; i < mScaleFontMesh.PassNum; i++)
                {
                    var fontPass = mScaleFontMesh.GetPass(i);
                    if (fontPass == null)
                        continue;

                    fontPass.ViewPort = view.Viewport;
                    fontPass.BindCBuffer(fontPass.Effect.ShaderProgram, fontPass.Effect.CacheData.CBID_View, view.ScreenViewCB);
                    fontPass.ShadingEnv.BindResources(mScaleFontMesh.Mesh, fontPass);
                    cmd.PushPass(fontPass);
                }

                if (smp.IsValid)
                    smp.Release();
            };
            await smp.Await();

            HostDesignPanel.CurrentUI.SetDesignRect(ref mWindowDesignSize);
            ShowAll();
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Z:
                    {
                        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                            EditorCommon.UndoRedo.UndoRedoManager.Instance.Undo(HostDesignPanel.UndoRedoKey);
                    }
                    break;
                case Key.Y:
                    {
                        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                            EditorCommon.UndoRedo.UndoRedoManager.Instance.Redo(HostDesignPanel.UndoRedoKey);
                    }
                    break;
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Delete:
                    if(mSelectedUIDatas.Count > 0)
                        HostDesignPanel.BroadcastDeleteUIs(this, mSelectedUIDatas.Keys.ToArray());
                    break;
            }
        }
        void RemoveChildPanelRectShow(EngineNS.UISystem.UIElement ui)
        {
            var panel = ui as EngineNS.UISystem.Controls.Containers.Panel;
            if(panel != null)
            {
                foreach (var child in panel.ChildrenUIElements)
                {
                    if (mSelectedUIDatas.ContainsKey(child))
                        mSelectedUIDatas.Remove(child);
                    mPanelRectsShow.Remove(child);

                    RemoveChildPanelRectShow(child);
                }
            }
        }
        internal void OnReceiveDeleteUIs(EngineNS.UISystem.UIElement[] processUIs)
        {
            foreach(var ui in processUIs)
            {
                if (mSelectedUIDatas.ContainsKey(ui))
                    mSelectedUIDatas.Remove(ui);
                mPanelRectsShow.Remove(ui);

                RemoveChildPanelRectShow(ui);
                if (ui == mMousePointAtUIElement)
                    mMousePointAtUIElement = null;
            }
            UpdateSelectedRectShow();
            UpdatePointAtShow();
        }
        internal async Task OnReceiveAddChildren(EngineNS.UISystem.UIElement parent, EngineNS.UISystem.UIElement[] children, int insertIndex)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            //var rc = EngineNS.CEngine.Instance.RenderContext;
            //foreach(var child in children)
            //{
            //    await AddPanelRectShow(rc, child);
            //}
        }

    }
}