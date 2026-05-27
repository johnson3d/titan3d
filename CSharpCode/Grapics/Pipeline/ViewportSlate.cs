using EngineNS.Editor;
using EngineNS.GamePlay.Scene;
using EngineNS.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline
{
    public interface IRenderViewport
    {
        public Graphics.Pipeline.TtRenderPolicy RenderPolicy { get; }
        public GamePlay.TtWorld World { get; }
        void SetCameraOffset(in DVector3 offset);
        public UI.TtUIHost HUD { get; }
        void PushHUD(UI.Controls.TtUIElement hud);
        void PopHUD();
        public Bricks.Particle.TtParticleGraphNode ParticleNode { get; }
    }
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtViewportSlate : IRenderViewport, IEventProcessor, IDisposable
    {
        public TtViewportSlate()
        {
            World = new GamePlay.TtWorld(this);
            TtEngine.Instance.ViewportSlateManager.AddViewport(this);
        }
        ~TtViewportSlate()
        {
            PresentWindow?.UnregEventProcessor(this);
            TtEngine.Instance?.ViewportSlateManager.RemoveViewport(this);
            Dispose();
        }
        public virtual void Dispose()
        {
            DisposeInteractiveModes();
            DisposeParticlePolicy();
            ClearHUDs();
            CoreSDK.DisposeObject(ref mWorld);
            if (RenderPolicy != null)
            {
                RenderPolicy.Dispose();
                RenderPolicy = null;
            }
        }
        protected NxRHI.FViewPort mViewport = new NxRHI.FViewPort();
        public NxRHI.FViewPort Viewport { get => mViewport; }
        NxRHI.FScissorRect mScissorRect = new NxRHI.FScissorRect();
        public NxRHI.FScissorRect ScissorRect { get => mScissorRect; }
        GamePlay.TtWorld mWorld;
        [Rtti.Meta("")]
        [Category("Option")]
        public GamePlay.TtWorld World { get => mWorld; protected set => mWorld = value; }
        public void SetCameraOffset(in DVector3 offset)
        {
            World.CameraOffset = offset;
            this.RenderPolicy.DefaultCamera.mCoreObject.SetMatrixStartPosition(in offset);
        }
        public virtual string Title { get; set; } = "Game";
        protected bool mVisible = true;
        public bool Visible
        {
            get => mVisible;
            set
            {
                if (value == mVisible)
                    return;
                mVisible = value;
                if (value == false)
                    OnViewportClosed();
            }
        }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public Vector2 ViewportPos { get; protected set; }
        public Vector2 WindowPos { get; protected set; }
        public Vector2 ClientMin { get; protected set; }
        public Vector2 ClientMax { get; protected set; }
        public Vector2 ClientSize
        {
            get
            {
                return ClientMax - ClientMin;
            }
        }
        protected Graphics.Pipeline.TtRenderPolicy mRenderPolicy;
        [EGui.Controls.PropertyGrid.TtPGCustomValueEditor(ReadOnly = true, UserDraw = false)]
        [Rtti.Meta("")]
        public Graphics.Pipeline.TtRenderPolicy RenderPolicy 
        { 
            get => mRenderPolicy; 
            set
            {
                mRenderPolicy = value;
            }
        }
        public Vector2 Window2Viewport(Vector2 pos)
        {//pos为真实窗口的坐标，返回ViewportSlate坐标
            Vector2 tmp;
            tmp.X = pos.X - (int)(WindowPos.X + ClientMin.X - ViewportPos.X);
            tmp.Y = pos.Y - (int)(WindowPos.Y + ClientMin.Y - ViewportPos.Y);
            return tmp;
        }
        protected bool mClientChanged = false;
        protected bool mSizeChanged = false;
        protected bool mHasPendingRenderPolicyResize = false;
        protected float mPendingRenderPolicyWidth = 0;
        protected float mPendingRenderPolicyHeight = 0;
        public bool IsValidClientArea()
        {
            return (ClientSize.X >= 1 && ClientSize.Y >= 1);
        }
        protected void RequestRenderPolicyResize(float width, float height)
        {
            if (width < 1 || height < 1)
                return;

            mPendingRenderPolicyWidth = width;
            mPendingRenderPolicyHeight = height;
            mHasPendingRenderPolicyResize = true;
        }
        protected void ApplyPendingRenderPolicyResize()
        {
            if (mHasPendingRenderPolicyResize == false)
                return;

            var width = mPendingRenderPolicyWidth;
            var height = mPendingRenderPolicyHeight;
            mHasPendingRenderPolicyResize = false;

            RenderPolicy?.OnResize(width, height);
        }
        public bool IsFocused { get; protected set; }
        public bool IsDrawing { get; protected set; }
        public bool IsMouseIn = false;

        protected Graphics.Pipeline.TtPresentWindow mPresentWindow;
        public Graphics.Pipeline.TtPresentWindow PresentWindow { get => mPresentWindow; }
        public enum EViewportType
        {
            Window,
            WindowWithClose,            
            ChildWindow,
        }
        public EViewportType ViewportType { get; set; } = EViewportType.Window;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public bool IsViewportSlateFocused { get; private set; }
        // 控制是否在视口内置 UI 区画 InteractiveMode 切换 Combo。
        // 默认 true 保持原有行为; 当宿主 (例如 SceneEditor) 已经把切换 UI
        // 自己画到工具栏上时, 把它设为 false 可避免视口内出现重复的下拉框。
        public bool ShowInteractiveModeCombo { get; set; } = true;
        public virtual Vector2 OnDrawViewportUI(in Vector2 startDrawPos) 
        {
            var usedSize = Vector2.Zero;
            if (ShowInteractiveModeCombo && InteractiveModes != null && InteractiveModes.Count > 1)
            {
                var currentModeName = CurrentIntercativeMode != null ? CurrentIntercativeMode.GetType().Name : "None";
                var comboWidth = 150.0f;
                ImGuiAPI.SetNextItemWidth(comboWidth);
                if (ImGuiAPI.BeginCombo("##InteractiveMode", currentModeName, ImGuiComboFlags_.ImGuiComboFlags_None))
                {
                    for (int i = 0; i < InteractiveModes.Count; i++)
                    {
                        var mode = InteractiveModes[i];
                        var modeName = mode.GetType().Name;
                        var isSelected = (mode == CurrentIntercativeMode);
                        if (ImGuiAPI.Selectable(modeName, isSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            CurrentIntercativeMode = mode;
                        }
                    }
                    ImGuiAPI.EndCombo();
                }
                usedSize.X += comboWidth;
                usedSize.Y = ImGuiAPI.GetFrameHeight();
                ImGuiAPI.SameLine(0, -1);
            }
            return usedSize;
        }
        public bool IsHoverGuiItem { get; set; }
        public Vector2 WindowSize = Vector2.Zero;

        public virtual unsafe void OnDrawShowTexture()
        {
            var showTexture = GetShowTexture();
            if (showTexture.m__TexID != 0)
            {
                var drawlist = ImGuiAPI.GetWindowDrawList();
                var uv1 = Vector2.Zero;
                var uv2 = Vector2.One;
                unsafe
                {
                    var min = ImGuiAPI.GetWindowContentRegionMin();
                    var max = ImGuiAPI.GetWindowContentRegionMax();
                    min = min + WindowPos;
                    max = max + WindowPos;
                    drawlist.AddImage(showTexture, in min, in max, in uv1, in uv2, 0xFFFFFFFF);
                }
            }
        }
        public virtual unsafe void OnDraw()
        {
            if (mVisible == false)
                return;
            //ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var sz = WindowSize;
            //ImGuiAPI.SetNextWindowSize(ref sz, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = false;
            bool bShow = false;
            switch(ViewportType)
            {
                case EViewportType.Window:
                    //bShow = ImGuiAPI.Begin(Title, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground | ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
                    {
                        bool vis = true;
                        bShow = ImGuiAPI.Begin(Title, ref vis, ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground | ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings | ImGuiWindowFlags_.ImGuiWindowFlags_NoNavFocus);
                    }
                    break;
                case EViewportType.WindowWithClose:
                    bShow = ImGuiAPI.Begin(Title, (bool*)0, ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground | ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings | ImGuiWindowFlags_.ImGuiWindowFlags_NoNavFocus);
                    break;
                case EViewportType.ChildWindow:
                    //if(sz == Vector2.Zero)
                    //    sz = ImGuiAPI.GetWindowSize();
                    bShow = ImGuiAPI.BeginChild(Title, in sz, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground | ImGuiWindowFlags_.ImGuiWindowFlags_NoNavFocus |
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollWithMouse);
                    break;
            }
            //if (ImGuiAPI.IsWindowDocked())
            //{
            //    DockId = ImGuiAPI.GetWindowDockID();
            //}
            if (bShow)
            {
                var idStr = Title + "_Window";
                var id = ImGuiAPI.GetID(idStr);
                ImGuiAPI.PushID((int)id);
                ImGuiAPI.SetKeyOwner(ImGuiKey.ImGuiMod_Alt, id, 0);
                sz = ImGuiAPI.GetWindowSize();
                var imViewport = ImGuiAPI.GetWindowViewport();
                if ((IntPtr)imViewport->PlatformUserData != IntPtr.Zero)
                {
                    var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)imViewport->PlatformUserData);
                    var myWindow = gcHandle.Target as Graphics.Pipeline.TtPresentWindow;
                    if (myWindow != mPresentWindow)
                    {
                        mPresentWindow?.UnregEventProcessor(this);
                        mPresentWindow = myWindow;
                        mPresentWindow.RegEventProcessor(this);
                    }
                }
                IsDrawing = true;
                IsFocused = ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_ChildWindows);
                var pos = ImGuiAPI.GetWindowPos();
                ViewportPos = ImGuiAPI.GetWindowViewport()->Pos;
                if (pos != WindowPos)
                {
                    mClientChanged = true;
                    WindowPos = pos;
                }
                var min = ImGuiAPI.GetWindowContentRegionMin();
                var max = ImGuiAPI.GetWindowContentRegionMax();
                sz = max - min;
                if (sz.X == 0 || sz.Y == 0)
                {
                    sz.X++;
                    sz.Y++;
                }
                var curPos = ImGuiAPI.GetCursorScreenPos();
                if (min != ClientMin || max != ClientMax)
                {
                    mClientChanged = true;

                    var saved = ClientSize;
                    ClientMin = min;
                    ClientMax = max;

                    if (Math.Abs(ClientSize.X - saved.X) > 0.1f || Math.Abs(ClientSize.Y - saved.Y) > 0.1f)
                    {
                        mSizeChanged = true;
                    }
                }
                OnDrawShowTexture();
                //var showTexture = GetShowTexture();
                //if (showTexture != IntPtr.Zero)
                //{
                //    var drawlist = ImGuiAPI.GetWindowDrawList();
                //    var uv1 = Vector2.Zero;
                //    var uv2 = Vector2.One;
                //    unsafe
                //    {
                //        min = min + pos;
                //        max = max + pos;
                //        drawlist.AddImage(showTexture.ToPointer(), in min, in max, in uv1, in uv2, 0xFFFFFFFF);
                //    }
                //}

                IsMouseIn = ImGuiAPI.IsMouseHoveringRect(pos + min, pos + max, true);

                if(ImGuiAPI.BeginChild("ViewportClient", in sz, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove| ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground))
                {
                    IsViewportSlateFocused = ImGuiAPI.IsHoverCurrentWindow() && ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_ChildWindows);
                    var viewportUISize = OnDrawViewportUI(in curPos);                    
                }
                ImGuiAPI.EndChild();

                ImGuiAPI.PopID();
            }
            else
            {
                IsFocused = false;
                mPresentWindow?.UnregEventProcessor(this);
                mPresentWindow = null;
            }
            switch (ViewportType)
            {
                case EViewportType.Window:
                case EViewportType.WindowWithClose:
                    ImGuiAPI.End();
                    break;
                case EViewportType.ChildWindow:
                    ImGuiAPI.EndChild();
                    break;
            }
            
            if (mClientChanged && IsValidClientArea())
            {
                OnClientChanged(mSizeChanged);
                mClientChanged = false;
                mSizeChanged = false;
            }
            if (mVisible == false)
            {
                OnViewportClosed();
            }
        }
        public Vector2 WorldAxis { get; set; } = new Vector2(80, 50);
        public bool ShowWorldAxis { get; set; } = true;
        public void DrawWorldAxis(TtCamera camera)
        {
            var cmdlst = ImGuiAPI.GetWindowDrawList();
            //var camera = this.CameraController.Camera;

            var winPos = ImGuiAPI.GetWindowPos();
            var vpMin = ImGuiAPI.GetWindowContentRegionMin();
            var vpMax = ImGuiAPI.GetWindowContentRegionMax();
            var DrawOffset = new Vector2();
            DrawOffset.SetValue(winPos.X + vpMin.X, winPos.Y + vpMin.Y);

            //var viewMtx = camera.mCoreObject.GetViewProjection();
            var xCamera = camera.mCoreObject.GetRight();
            var yCamera = camera.mCoreObject.GetUp();
            var zCamera = camera.mCoreObject.GetDirection();
            var viewMtx = Matrix.MakeMatrixAxis(in xCamera, in yCamera, in zCamera, in Vector3.Zero);
            var xVec = Vector3.TransformNormal(in Vector3.Right, in viewMtx);
            var yVec = Vector3.TransformNormal(in Vector3.Up, in viewMtx);
            var zVec = Vector3.TransformNormal(in Vector3.Forward, in viewMtx);
            xVec.Normalize();
            yVec.Normalize();
            zVec.Normalize();

            var v2X = xVec.XY;
            var v2Y = yVec.XY;
            var v2Z = zVec.XY;

            v2X *= 60;
            v2Y *= 60;
            v2Z *= 60;

            v2X.Y *= -1;
            v2Y.Y *= -1;
            v2Z.Y *= -1;

            var v2Center = new Vector2(WorldAxis.X, this.ClientSize.Y - WorldAxis.Y) + DrawOffset;
            cmdlst.AddLine(in v2Center, v2Center + v2X, (uint)Color4b.Red.ToR8G8B8A8(), 1);
            cmdlst.AddLine(in v2Center, v2Center + v2Y, (uint)Color4b.Green.ToR8G8B8A8(), 1);
            cmdlst.AddLine(in v2Center, v2Center + v2Z, (uint)Color4b.Blue.ToR8G8B8A8(), 1);

            cmdlst.AddText(v2Center + v2X, (uint)Color4b.Red.ToR8G8B8A8(), "x", null);
            cmdlst.AddText(v2Center + v2Y, (uint)Color4b.Green.ToR8G8B8A8(), "y", null);
            cmdlst.AddText(v2Center + v2Z, (uint)Color4b.Blue.ToR8G8B8A8(), "z", null);

            cmdlst.AddText(v2Center, Color4b.LightPink.ToR8G8B8A8(), string.Format("fps={0:F2}", TtEngine.Instance.FPS), null);
        }
        protected virtual void OnViewportClosed()
        {
            //mPresentWindow.UnregEventProcessor(this);
        }
        protected virtual void OnClientChanged(bool bSizeChanged)
        {
            var vpSize = this.ClientSize;

            mViewport.TopLeftX = WindowPos.X + ClientMin.X;
            mViewport.TopLeftY = WindowPos.Y + ClientMin.Y;
            mViewport.Width = vpSize.X;
            mViewport.Height = vpSize.Y;

            mScissorRect.MinX = (int)mViewport.TopLeftX;
            mScissorRect.MinY = (int)mViewport.TopLeftY;
            mScissorRect.MaxX = (int)(mViewport.TopLeftX + mViewport.Width);
            mScissorRect.MaxY = (int)(mViewport.TopLeftY + mViewport.Height);

            if (bSizeChanged)
            {
                RequestRenderPolicyResize(vpSize.X, vpSize.Y);
            }

            //if (mDefaultHUD != null)
            //    mDefaultHUD.WindowSize = new SizeF(this.ClientSize.X, this.ClientSize.Y);
            foreach (var i in mHUDStack)
            {
                i.WindowSize = new SizeF(this.ClientSize.X, this.ClientSize.Y);
            }

            if (CurrentIntercativeMode != null)
            {
                CurrentIntercativeMode.OnClientChanged(bSizeChanged);
            }
        }
        public void ProcessHitproxySelected(float mouseX, float mouseY)
        {
            var edtorPolicy = this.RenderPolicy as Graphics.Pipeline.TtRenderPolicy;
            if (edtorPolicy != null)
            {
                var pos = Window2Viewport(new Vector2(mouseX, mouseY));
                var hitObj = edtorPolicy.GetHitproxy((uint)pos.X, (uint)pos.Y);
                OnHitproxySelected(hitObj);
            }
        }
        public unsafe virtual bool OnEvent(in Bricks.Input.Event e)
        {
            if (CurrentIntercativeMode != null)
            {
                return CurrentIntercativeMode.OnEvent(in e);
            }
            return true;
        }
        protected virtual ImTextureRef GetShowTexture()
        {
            var result = new ImTextureRef();
            if (RenderPolicy == null)
                return result;
            var srv = RenderPolicy.GetFinalShowRSV();
            if (srv == null)
                return result;
            result.m__TexID = (ulong)srv.GetTextureHandle();
            return result;
        }
        public virtual void OnHitproxySelected(Graphics.Pipeline.IProxiable proxy)
        {
            var edtorPolicy = this.RenderPolicy as Graphics.Pipeline.TtRenderPolicy;
            if (edtorPolicy != null)
            {
                if (proxy == null)
                {
                    //if (this.IsHoverGuiItem == false)
                    edtorPolicy.PickedProxiableManager.ClearSelected();
                }
                else
                {
                    edtorPolicy.PickedProxiableManager.Selected(proxy);
                }
            }
        }
        public virtual void OnHitproxySelectedMulti(bool clearPre, params Graphics.Pipeline.IProxiable[] proxies)
        {
            var edtorPolicy = this.RenderPolicy as Graphics.Pipeline.TtRenderPolicy;
            if (edtorPolicy != null)
            {
                if (proxies == null || proxies.Length == 0)
                    edtorPolicy.PickedProxiableManager.ClearSelected();
                else
                {
                    if(clearPre)
                        edtorPolicy.PickedProxiableManager.ClearSelected();
                    for (int i=0; i<proxies.Length; i++)
                    {
                        if (proxies[i] == null)
                            continue;
                        edtorPolicy.PickedProxiableManager.Selected(proxies[i]);
                    }
                }
            }
        }
        public virtual void OnHitproxyUnSelectedMulti(params Graphics.Pipeline.IProxiable[] proxies)
        {
            var editorPolicy = this.RenderPolicy as Graphics.Pipeline.TtRenderPolicy;
            if(editorPolicy != null)
            {
                for(int i=0; i<proxies.Length; i++)
                {
                    if (proxies[i] == null)
                        continue;
                    editorPolicy.PickedProxiableManager.Unselected(proxies[i]);
                }
            }
        }
        public delegate Thread.Async.TtTask<bool> FOnInitialize(TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax);
        public FOnInitialize OnInitialize = null;
        [Rtti.Meta("")]
        public virtual async Thread.Async.TtTask<bool> Initialize(TtSlateApplication application, RName policyName, float zMin, float zMax)
        {
            var policy = policyName.GetAsset<Bricks.RenderPolicyEditor.TtRenderPolicyAsset>().GetResultUntilCompleted().CreateRenderPolicy(policyName, this);
            if (OnInitialize != null)
            {
                await OnInitialize(this, application, policy, zMin, zMax);
            }
            await this.World.InitWorld();
            SetCameraOffset(in DVector3.Zero);

            //mDefaultHUD.RenderCamera = this.RenderPolicy.DefaultCamera;
            //PushHUD(mDefaultHUD);

            await ReCreateInteractiveModes();

            await InitParticlePolicy();

            IsInlitialized = true;
            return true;
        }

        #region HUD
        protected Stack<UI.TtUIHost> mHUDStack = new Stack<UI.TtUIHost>();
        [Rtti.Meta("")]
        public void SetHUD(UI.Controls.TtUIElement hud)
        {
            ClearHUDs();
            PushHUD(hud);
        }
        [Rtti.Meta("")]
        public void PushHUD(UI.Controls.TtUIElement hud)
        {
            TtUIHost tempHost = null;
            if (hud is UI.TtUIHost)
            {
                tempHost = hud as UI.TtUIHost;
            }
            else
            {
                tempHost = new UI.TtUIHost();
                tempHost.Children.Add(hud);
            }

            tempHost.WriteFlag(UI.Controls.TtUIElement.ECoreFlags.IsScreenSpace, true);
            tempHost.ViewportSlate = this;
            tempHost.RenderCamera = this.RenderPolicy.DefaultCamera;
            tempHost.WindowSize = new SizeF(this.ClientSize.X, this.ClientSize.Y);
            mHUDStack.Push(tempHost);

            TtEngine.Instance.UIManager.AddActivedUI(tempHost);
        }
        [Rtti.Meta("")]
        public void PopHUD()
        {
            var hud = mHUDStack.Peek();
            hud.WriteFlag(UI.Controls.TtUIElement.ECoreFlags.IsScreenSpace, false);
            hud.ViewportSlate = null;
            mHUDStack.Pop();

            TtEngine.Instance.UIManager.RemoveActivedUI(hud);
        }
        [Rtti.Meta("")]
        public void ClearHUDs()
        {
            while (mHUDStack.Count > 0)
            {
                PopHUD();
            }
        }
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly | Rtti.MetaAttribute.EMetaFlags.NoSerializable)]
        [Category("Option")]
        public UI.TtUIHost HUD
        {
            get
            {
                if (mHUDStack == null || mHUDStack.Count == 0)
                    return null;
                return mHUDStack.Peek();
            } 
        }
        #endregion
        public bool IsInlitialized { get; set; } = false;
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtViewportSlate), nameof(TickLogic));
                return mScopeTick;
            }
        }
        System.Threading.AutoResetEvent mRenderFinishedEvent = new System.Threading.AutoResetEvent(false);
        public unsafe void TickLogic(float ellapse)
        {
            if (IsInlitialized == false)
                return;

            foreach (var i in mHUDStack)
            {
                TtEngine.Instance.TaskCollector.AddWaitTask(i.BuildMesh());
            }

            if (CurrentIntercativeMode != null)
            {
                CurrentIntercativeMode.Tick(ellapse);
            }

            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (IsDrawing)
                {
                    if (mPresentWindow != null && mPresentWindow.CanRender == false)
                    {
                        IsDrawing = false;
                        return;
                    }
                    if (this.IsFocused)
                    {
                        TickOnFocus();
                    }

                    TtEngine.Instance.ThreadRender.QueueRenderAction("RenderPolicy.Tick", static (in Thread.TtThreadRender.FRenderAction RAct) =>
                    {
                        var This = (RAct.Arg as TtViewportSlate);
                        This.ApplyPendingRenderPolicyResize();
                        This.RenderPolicy?.BeginTick(This.World);
                        This.RenderPolicy?.Tick(This.World, null);
                        This.RenderPolicy?.EndTick(This.World);
                    }, this);
                    TickParticleUpdate();

                    World.TickLogic(this.RenderPolicy, ellapse);

                    TtEngine.Instance.ThreadRender.WaitFinishRenderAction(mRenderFinishedEvent);

                    RenderPolicy?.AfterPolicyFinished();

                    IsDrawing = false;
                }
            }
        }
        public virtual void TickSync(float ellapse)
        {
            RenderPolicy?.TickSync();
        }
        protected Dictionary<string, RectangleF> mOverlappedAreas = new Dictionary<string, RectangleF>();
        public void RegisterOverlappedArea(string name, in RectangleF rect)
        {
            mOverlappedAreas[name] = rect;
        }
        public void UnRegisterOverlappedArea(string name)
        {
            mOverlappedAreas.Remove(name);
        }

        public bool PointInOverlappedArea(in Vector2 point)
        {
            foreach(var rect in mOverlappedAreas.Values)
            {
                if (rect.Contains(point.X, point.Y))
                    return true;
            }
            return false;
        }

        #region InteractiveMode
        public async Thread.Async.TtTask ReCreateInteractiveModes()
        {
            DisposeInteractiveModes();

            var modeInfo = GetIntercativeModeInfo();
            if (modeInfo != null && modeInfo.ModeTypes.Count > 0)
            {
                InteractiveModes = new List<TtInteractiveMode>();
                foreach (var type in modeInfo.ModeTypes)
                {
                    var mode = Rtti.TtTypeDescManager.CreateInstance(type) as TtInteractiveMode;
                    await mode.Initialize(this);
                    InteractiveModes.Add(mode);
                }
                if (InteractiveModes.Count > 0)
                {
                    CurrentIntercativeMode = InteractiveModes[InteractiveModes.Count - 1];
                }
            }
        }
        public void DisposeInteractiveModes()
        {
            if (InteractiveModes != null)
            {
                foreach (var i in InteractiveModes)
                {
                    i.Dispose();
                }
                InteractiveModes.Clear();
                InteractiveModes = null;
            }
        }
        TtInteractiveMode mCurrentIntercativeMode;
        public virtual TtInteractiveMode CurrentIntercativeMode 
        {
            get => mCurrentIntercativeMode;
            set
            {
                if (value == mCurrentIntercativeMode)
                    return;
                if (mCurrentIntercativeMode != null)
                {
                    mCurrentIntercativeMode.OnLeaveMode();
                }
                mCurrentIntercativeMode = value;
                if (mCurrentIntercativeMode != null)
                {
                    mCurrentIntercativeMode.OnEnterMode();
                }
            }
        }
        public List<TtInteractiveMode> InteractiveModes { get; private set; }
        public TtInteractiveModeManager.TtInteractiveModeInfo GetIntercativeModeInfo()
        {
            return TtEngine.Instance.InteractiveModeManager.QueryModeInfo(Rtti.TtTypeDesc.TypeOf(this.GetType()));
        }
        // 在已经初始化好的 InteractiveModes 列表里, 把 CurrentIntercativeMode 切换为
        // 指定类型的 mode 实例。常见用法是在 viewport.Initialize 末尾 (await
        // ReCreateInteractiveModes 之后) 由 viewport 自己调用; 也允许宿主编辑器
        // (例如 TtSceneEditor) 在 OpenEditor 末尾从外部调用, 用于覆盖
        // ReCreateInteractiveModes 默认"取列表最后一个"的不可控行为, 让每个
        // viewport / 宿主编辑器自己决定缺省 mode。
        //
        // 如果列表里没有该类型 (例如未注册), 保持当前默认值不变并返回 false,
        // 避免误把 CurrentIntercativeMode 置 null 导致输入无响应。
        public bool SetDefaultInteractiveMode<TMode>() where TMode : TtInteractiveMode
        {
            if (InteractiveModes == null)
                return false;
            for (int i = 0; i < InteractiveModes.Count; i++)
            {
                if (InteractiveModes[i] is TMode)
                {
                    CurrentIntercativeMode = InteractiveModes[i];
                    return true;
                }
            }
            return false;
        }
        protected virtual void TickOnFocus()
        {
            if (CurrentIntercativeMode != null)
            {
                CurrentIntercativeMode.TickOnFocus();
            }
        }
        #endregion
    }
    public class TtViewportSlateManager
    {
        public List<WeakReference<TtViewportSlate>> Viewports { get; } = new List<WeakReference<TtViewportSlate>>();
        public TtViewportSlate GetPressedViewport()
        {
            foreach (var i in Viewports)
            {
                TtViewportSlate t;
                if (i.TryGetTarget(out t) == false)
                {
                    continue;
                }
                else
                {
                    if (t.IsDrawing && t.IsMouseIn)
                    {
                        return t;
                    }
                }
            }
            return null;
        }
        public void AddViewport(TtViewportSlate slate)
        {
            var rmv = new List<WeakReference<TtViewportSlate>>();
            foreach (var i in Viewports)
            {
                TtViewportSlate t;
                if (i.TryGetTarget(out t) == false)
                {
                    rmv.Add(i);
                    continue;
                }
                else if (t == slate)
                    return;
            }
            Viewports.Add(new WeakReference<TtViewportSlate>(slate));
            foreach (var i in rmv)
            {
                Viewports.Remove(i);
            }
        }
        public void RemoveViewport(TtViewportSlate slate)
        {
            var rmv = new List<WeakReference<TtViewportSlate>>();
            foreach (var i in Viewports)
            {
                TtViewportSlate t;
                if (i.TryGetTarget(out t) == false)
                {
                    rmv.Add(i);
                    continue;
                }
                else if (t == slate)
                {
                    rmv.Add(i);
                }
            }
            foreach (var i in rmv)
            {
                Viewports.Remove(i);
            }
        }
    }

    public partial class TtInteractiveMode : IDisposable
    {
        public virtual void Dispose()
        {
        }
        public TtViewportSlate Viewport { get; set; }
        public virtual async Thread.Async.TtTask<bool> Initialize(TtViewportSlate viewportSlate)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            Viewport = viewportSlate;
            return true;
        }
        public virtual void OnLeaveMode()
        {

        }
        public virtual void OnEnterMode()
        {

        }
        public virtual void OnClientChanged(bool bSizeChanged)
        {

        }
        public virtual bool OnEvent(in Bricks.Input.Event e)
        {
            if (e.Type == Bricks.Input.EventType.MOUSEBUTTONUP)
            {
                OnMouseUp(in e);
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEBUTTONDOWN)
            {
                OnMouseDown(in e);
            }
            return true;
        }
        public virtual void OnMouseUp(in Bricks.Input.Event e)
        {

        }
        public virtual void OnMouseDown(in Bricks.Input.Event e)
        {

        }
        public virtual void Tick(float ellapse)
        {

        }
        public virtual void TickOnFocus()
        {

        }
    }
    
    public partial class TtInteractiveModeManager
    {
        public class TtInteractiveModeInfo
        {
            public List<Rtti.TtTypeDesc> ModeTypes = new List<Rtti.TtTypeDesc>();
        }
        private Dictionary<Rtti.TtTypeDesc, TtInteractiveModeInfo> mModeInfos = new Dictionary<Rtti.TtTypeDesc, TtInteractiveModeInfo>();

        public void RegisterMode(Rtti.TtTypeDesc viewportTypeDesc, Rtti.TtTypeDesc modeTypeDesc)
        {
            if (!mModeInfos.TryGetValue(viewportTypeDesc, out var info))
            {
                info = new TtInteractiveModeInfo();
                mModeInfos[viewportTypeDesc] = info;
            }
            if (!info.ModeTypes.Contains(modeTypeDesc))
            {
                info.ModeTypes.Add(modeTypeDesc);
            }
        }
        public void RegisterMode<TViewport, TMode>()
            where TViewport : TtViewportSlate
            where TMode : TtInteractiveMode
        {
            RegisterMode(Rtti.TtTypeDescGetter<TViewport>.TypeDesc, Rtti.TtTypeDescGetter<TMode>.TypeDesc);
        }
        public void UnregisterMode(Rtti.TtTypeDesc viewportTypeDesc, Rtti.TtTypeDesc modeTypeDesc)
        {
            if (mModeInfos.TryGetValue(viewportTypeDesc, out var info))
            {
                info.ModeTypes.Remove(modeTypeDesc);
                if (info.ModeTypes.Count == 0)
                {
                    mModeInfos.Remove(viewportTypeDesc);
                }
            }
        }
        public TtInteractiveModeInfo QueryModeInfo(Rtti.TtTypeDesc viewportTypeDesc)
        {
            var resultInfo = new TtInteractiveModeInfo();
            var currentTypeDesc = viewportTypeDesc;
            while (currentTypeDesc != null)
            {
                if (mModeInfos.TryGetValue(currentTypeDesc, out var info))
                {
                    foreach (var modeTypeDesc in info.ModeTypes)
                    {
                        if (!resultInfo.ModeTypes.Contains(modeTypeDesc))
                        {
                            resultInfo.ModeTypes.Add(modeTypeDesc);
                        }
                    }
                }
                currentTypeDesc = currentTypeDesc.BaseType;
            }
            return resultInfo.ModeTypes.Count > 0 ? resultInfo : null;
        }
    }
    public partial class TtOffscreenRenderer : IRenderViewport, IDisposable
    {
        TtRenderPolicy mParticlePolicy;
        Bricks.Particle.TtParticleGraphNode mParticleNode;
        NxRHI.TtRCmdQueue mParticleCmdQueue;
        public Bricks.Particle.TtParticleGraphNode ParticleNode => mParticleNode;

        public async Thread.Async.TtTask InitParticlePolicy()
        {
            mParticlePolicy = new TtRenderPolicy();
            mParticleCmdQueue = new NxRHI.TtRCmdQueue();
            mParticlePolicy.CmdQueue = mParticleCmdQueue;

            mParticleNode = new Bricks.Particle.TtParticleGraphNode();
            mParticleNode.InitNodePins();

            var endingNode = new Common.TtAssitRootNode();
            endingNode.InitNodePins();
            endingNode.Name = "ParticleEnding";

            mParticlePolicy.RegRenderNode2("ParticleCompute", mParticleNode);
            mParticlePolicy.RegRenderNode2("ParticleEnding", endingNode);

            mParticlePolicy.AddLinker(mParticleNode.ResultPinOut, endingNode.SrcPinIn);
            mParticlePolicy.RootNode = endingNode;

            bool hasInputError = false;
            mParticlePolicy.BuildGraph(ref hasInputError);

            foreach (var kvp in mParticlePolicy.GraphNodes)
            {
                await kvp.Value.Initialize(mParticlePolicy, kvp.Value.Name);
            }
        }

        void TickParticleUpdate()
        {
            if (mParticlePolicy == null || mParticleNode == null)
                return;
            if (mParticleNode.CurEmitters.Count == 0 && mParticleNode.PrevEmitters.Count == 0)
                return;

            //mParticlePolicy.DefaultCamera = RenderPolicy?.DefaultCamera;

            mParticlePolicy.BeginTick(World);
            mParticlePolicy.Tick(World, null);
            mParticlePolicy.EndTick(World);
            mParticlePolicy.ExecuteCmdQueue(true);

            CoreSDK.Swap(ref mParticleNode.CurEmitters, ref mParticleNode.PrevEmitters);
        }

        void DisposeParticlePolicy()
        {
            if (mParticlePolicy != null)
            {
                mParticlePolicy.Dispose();
                mParticlePolicy = null;
            }
            mParticleNode = null;
            mParticleCmdQueue = null;
        }

        public bool DisposeRenderPolicy { get; set; } = false;
        public virtual void Dispose()
        {
            DisposeParticlePolicy();
            ClearHUDs();
            CoreSDK.DisposeObject(ref mWorld);

            //TtSnapshot.Save must call by bAsync = false
            if(DisposeRenderPolicy)
            {
                if (RenderPolicy != null)
                {
                    RenderPolicy.Dispose();
                    RenderPolicy = null;
                }
            }
        }
        public Graphics.Pipeline.TtRenderPolicy RenderPolicy { get; set; }
        GamePlay.TtWorld mWorld;
        public GamePlay.TtWorld World { get=> mWorld; set=> mWorld = value; }
        protected Stack<UI.TtUIHost> mHUDStack = new Stack<UI.TtUIHost>();
        public UI.TtUIHost HUD
        {
            get
            {
                if (mHUDStack == null || mHUDStack.Count == 0)
                    return null;
                return mHUDStack.Peek();
            }
        }
        public void ClearHUDs()
        {
            while (mHUDStack.Count > 0)
            {
                PopHUD();
            }
        }
        public void PushHUD(UI.Controls.TtUIElement hud)
        {
            TtUIHost tempHost = null;
            if (hud is UI.TtUIHost)
            {
                tempHost = hud as UI.TtUIHost;
            }
            else
            {
                tempHost = new UI.TtUIHost();
                tempHost.Children.Add(hud);
            }

            tempHost.WriteFlag(UI.Controls.TtUIElement.ECoreFlags.IsScreenSpace, true);
            tempHost.RenderCamera = RenderPolicy?.DefaultCamera;
            mHUDStack.Push(tempHost);

            TtEngine.Instance.UIManager.AddActivedUI(tempHost);
        }
        public void PopHUD()
        {
            if (mHUDStack.Count == 0)
                return;
            var hud = mHUDStack.Peek();
            hud.WriteFlag(UI.Controls.TtUIElement.ECoreFlags.IsScreenSpace, false);
            mHUDStack.Pop();

            TtEngine.Instance.UIManager.RemoveActivedUI(hud);
        }
        public GamePlay.TtWorld.TtVisParameter VisParameter = new GamePlay.TtWorld.TtVisParameter();
        public void SetCameraOffset(in DVector3 offset)
        {
            World.CameraOffset = offset;
            RenderPolicy.DefaultCamera.mCoreObject.SetMatrixStartPosition(in offset);
        }
        public virtual async Thread.Async.TtTask Initialize(RName policyName)
        {
            var policyAsset = await policyName.GetAsset<Bricks.RenderPolicyEditor.TtRenderPolicyAsset>();
            RenderPolicy = policyAsset.CreateRenderPolicy(policyName,  null);
            await RenderPolicy.Initialize(null);

            World = new GamePlay.TtWorld(null);
            await this.World.InitWorld();
            World.DirectionLight.Direction = new Vector3(0, 0, 1);
            SetCameraOffset(in DVector3.Zero);

            RenderPolicy.CmdQueue = new NxRHI.TtRCmdQueue();

            await InitParticlePolicy();
        }
        public void SetSize(float w, float h)
        {
            RenderPolicy.OnResize(w, h);
        }
        public void ExecuteRender(bool gatherVisibleMeshes, bool bUpdateGpuParticle)
        {
            VisParameter.World = World;
            VisParameter.CullCamera = RenderPolicy.DefaultCamera;
            if (gatherVisibleMeshes)
                World.GatherVisibleMeshes(VisParameter);

            RenderPolicy.UpdateCameraAttachements(NxRHI.TtCbView.EUpdateMode.Auto);
            
            RenderPolicy.BeginTick(World);
            RenderPolicy.Tick(World, null);
            RenderPolicy.EndTick(World);

            if (bUpdateGpuParticle)
                TickParticleUpdate();

            TtEngine.Instance.GfxDevice.CbvUpdater.UpdateCBVs();
            RenderPolicy.ExecuteCmdQueue(true);
        }
        public void TickSync()
        {
            RenderPolicy?.TickSync();
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public Graphics.Pipeline.TtViewportSlateManager ViewportSlateManager { get; } = new Graphics.Pipeline.TtViewportSlateManager();
        public Graphics.Pipeline.TtInteractiveModeManager InteractiveModeManager { get; } = new Graphics.Pipeline.TtInteractiveModeManager();
    }
}




#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Graphics.Pipeline
{
	partial class TtViewportSlate
	{
		public async Thread.Async.TtTask<bool> macross_Initialize (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtSlateApplication application, RName policyName, float zMin, float zMax) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = await Initialize(application, policyName, zMin, zMax);
			return _return_value;
		}
		public unsafe void macross_SetHUD (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, UI.Controls.TtUIElement hud) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			SetHUD(hud);
		}
		public unsafe void macross_PushHUD (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, UI.Controls.TtUIElement hud) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			PushHUD(hud);
		}
		public unsafe void macross_PopHUD (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			PopHUD();
		}
		public unsafe void macross_ClearHUDs (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			ClearHUDs();
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross