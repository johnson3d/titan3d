using EngineNS.Editor;
using EngineNS.GamePlay.Scene;
using EngineNS.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline
{
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtViewportSlate : IEventProcessor, IDisposable
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
                CoreSDK.DisposeObject(ref mRenderPolicy);
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
        public bool IsValidClientArea()
        {
            return (ClientSize.X >= 1 && ClientSize.Y >= 1);
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
        public virtual void OnDrawViewportUI(in Vector2 startDrawPos) { }
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
                    drawlist.AddImage(showTexture, in min, in max, in uv1, in uv2, 0x01FFFFFF);// 0xFFFFFFFF);abgr
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
                ImGuiAPI.SetKeyOwner(ImGuiKey.ImGuiKey_ModAlt, id, 0);
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
                //        drawlist.AddImage(showTexture.ToPointer(), in min, in max, in uv1, in uv2, 0x01FFFFFF);// 0xFFFFFFFF);abgr
                //    }
                //}

                IsMouseIn = ImGuiAPI.IsMouseHoveringRect(pos + min, pos + max, true);

                if(ImGuiAPI.BeginChild("ViewportClient", in sz, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove| ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground))
                {
                    IsViewportSlateFocused = ImGuiAPI.IsHoverCurrentWindow() && ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_ChildWindows);
                    OnDrawViewportUI(in curPos);                    
                    ImGuiAPI.EndChild();
                }

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
            mScissorRect.MinX = (int)(mViewport.TopLeftY + mViewport.Height);

            if (bSizeChanged)
            {
                RenderPolicy?.OnResize(vpSize.X, vpSize.Y);
            }

            //if (mDefaultHUD != null)
            //    mDefaultHUD.WindowSize = new SizeF(this.ClientSize.X, this.ClientSize.Y);
            foreach (var i in mHUDStack)
            {
                i.WindowSize = new SizeF(this.ClientSize.X, this.ClientSize.Y);
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
        protected virtual void OnMouseUp(in Bricks.Input.Event e)
        {

        }
        protected virtual void OnMouseDown(in Bricks.Input.Event e)
        {

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
            var policy = policyName.GetAsset<Bricks.RenderPolicyEditor.TtRenderPolicyAsset>().GetResultUntilCompleted().CreateRenderPolicy(this);
            if (OnInitialize != null)
            {
                await OnInitialize(this, application, policy, zMin, zMax);
            }
            await this.World.InitWorld();
            SetCameraOffset(in DVector3.Zero);

            //mDefaultHUD.RenderCamera = this.RenderPolicy.DefaultCamera;
            //PushHUD(mDefaultHUD);

            IsInlitialized = true;
            return true;
        }

        #region HUD
        //protected UI.TtUIHost mDefaultHUD = new UI.TtUIHost();
        //[Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly | Rtti.MetaAttribute.EMetaFlags.NoSerializable)]
        //public UI.TtUIHost DefaultHUD
        //{
        //    get => mDefaultHUD;
        //    set
        //    {
        //        if (mDefaultHUD == value)
        //            return;
        //        mDefaultHUD = value;
        //        if(mDefaultHUD != null)
        //        {
        //            mDefaultHUD.RenderCamera = RenderPolicy.DefaultCamera;
        //            PushHUD(mDefaultHUD);
        //            OnClientChanged(true);
        //        }
        //    }
        //}
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
        public virtual unsafe void TickLogic(float ellapse)
        {
            if (IsInlitialized == false)
                return;

            foreach (var i in mHUDStack)
            {
                TtEngine.Instance.TaskCollector.AddWaitTask(i.BuildMesh());
            }

            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (IsDrawing)
                {
                    if (this.IsFocused)
                    {
                        TickOnFocus();
                    }

                    RenderPolicy?.BeginTickLogic(World);

                    World.TickLogic(this.RenderPolicy, ellapse);

                    RenderPolicy?.TickLogic(World, null);

                    RenderPolicy?.EndTickLogic(World);

                    IsDrawing = false;
                }
            }
        }
        protected virtual void TickOnFocus()
        {
            
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
    }
    public class UViewportSlateManager
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

    public partial class TtOffscreenRenderer
    {
        public Graphics.Pipeline.TtRenderPolicy RenderPolicy { get; set; }
        public GamePlay.TtWorld World { get; set; }
        public GamePlay.TtWorld.TtVisParameter VisParameter = new GamePlay.TtWorld.TtVisParameter();
        public void SetCameraOffset(in DVector3 offset)
        {
            World.CameraOffset = offset;
            RenderPolicy.DefaultCamera.mCoreObject.SetMatrixStartPosition(in offset);
        }
        public virtual async System.Threading.Tasks.Task Initialize(RName policyName)
        {
            RenderPolicy = policyName.GetAsset<Bricks.RenderPolicyEditor.TtRenderPolicyAsset>().GetResultUntilCompleted().CreateRenderPolicy(null);
            await RenderPolicy.Initialize(null);

            World = new GamePlay.TtWorld(null);
            await this.World.InitWorld();
            World.DirectionLight.Direction = new Vector3(0, 0, 1);
            SetCameraOffset(in DVector3.Zero);

            RenderPolicy.CmdQueue = new NxRHI.TtRCmdQueue();
        }
        public void SetSize(float w, float h)
        {
            RenderPolicy.OnResize(w, h);
        }
        public void ExecuteRender()
        {
            VisParameter.World = World;
            VisParameter.CullCamera = RenderPolicy.DefaultCamera;
            World.GatherVisibleMeshes(VisParameter);

            RenderPolicy.UpdateCameraAttachements(NxRHI.TtCbView.EUpdateMode.Auto);
            
            RenderPolicy.BeginTickLogic(World);
            RenderPolicy.TickLogic(World, null);
            RenderPolicy.EndTickLogic(World);

            TtEngine.Instance.GfxDevice.CbvUpdater.UpdateCBVs();
            RenderPolicy.ExecuteCmdQueue();
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
        public Graphics.Pipeline.UViewportSlateManager ViewportSlateManager { get; } = new Graphics.Pipeline.UViewportSlateManager();
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Graphics.Pipeline
{
	partial class TtViewportSlate
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_Initialize_3328281008 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Graphics.Pipeline.TtViewportSlate->Thread.Async.TtTask<bool> Initialize(TtSlateApplication application, RName policyName, float zMin, float zMax)");
		public async Thread.Async.TtTask<bool> macross_Initialize (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtSlateApplication application, RName policyName, float zMin, float zMax) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":application", application);
					stackframe.SetWatchVariable(nodeName + ":policyName", policyName);
					stackframe.SetWatchVariable(nodeName + ":zMin", zMin);
					stackframe.SetWatchVariable(nodeName + ":zMax", zMax);
				}
			}
			var _return_value = await Initialize(application, policyName, zMin, zMax);
			macross_break_Initialize_3328281008.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_SetHUD_3408856308 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Graphics.Pipeline.TtViewportSlate->void SetHUD(UI.Controls.TtUIElement hud)");
		public unsafe void macross_SetHUD (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, UI.Controls.TtUIElement hud) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":hud", hud);
				}
			}
			SetHUD(hud);
			macross_break_SetHUD_3408856308.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_PushHUD_3408856308 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Graphics.Pipeline.TtViewportSlate->void PushHUD(UI.Controls.TtUIElement hud)");
		public unsafe void macross_PushHUD (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, UI.Controls.TtUIElement hud) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":hud", hud);
				}
			}
			PushHUD(hud);
			macross_break_PushHUD_3408856308.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_PopHUD_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Graphics.Pipeline.TtViewportSlate->void PopHUD()");
		public unsafe void macross_PopHUD (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			PopHUD();
			macross_break_PopHUD_2609910045.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ClearHUDs_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Graphics.Pipeline.TtViewportSlate->void ClearHUDs()");
		public unsafe void macross_ClearHUDs (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			ClearHUDs();
			macross_break_ClearHUDs_2609910045.TryBreak(mcStack);
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross