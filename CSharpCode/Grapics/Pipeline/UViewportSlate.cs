using System;
using System.Collections.Generic;

namespace EngineNS.Graphics.Pipeline
{
    public partial class UViewportSlate : IEventProcessor, IDisposable
    {
        public UViewportSlate()
        {
            World = new GamePlay.UWorld(this);
            UEngine.Instance.ViewportSlateManager.AddViewport(this);
        }
        ~UViewportSlate()
        {
            PresentWindow?.UnregEventProcessor(this);
            UEngine.Instance?.ViewportSlateManager.RemoveViewport(this);
            Dispose();
        }
        public virtual void Dispose()
        {
            CoreSDK.DisposeObject(ref mWorld);
        }
        GamePlay.UWorld mWorld;
        [Rtti.Meta()]
        public GamePlay.UWorld World { get => mWorld; protected set => mWorld = value; }
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
                    OnVieportClosed();
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
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(ReadOnly = true, UserDraw = false)]
        public Graphics.Pipeline.URenderPolicy RenderPolicy { get; set; }
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

        protected Graphics.Pipeline.UPresentWindow mPresentWindow;
        public Graphics.Pipeline.UPresentWindow PresentWindow { get => mPresentWindow; }
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
                    bShow = ImGuiAPI.BeginChild(Title, in sz, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground | ImGuiWindowFlags_.ImGuiWindowFlags_NoNavFocus |
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
                    var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
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
                var showTexture = GetShowTexture();
                if (showTexture != IntPtr.Zero)
                {
                    var drawlist = ImGuiAPI.GetWindowDrawList();
                    var uv1 = Vector2.Zero;
                    var uv2 = Vector2.One;
                    unsafe
                    {
                        min = min + pos;
                        max = max + pos;
                        drawlist.AddImage(showTexture.ToPointer(), in min, in max, in uv1, in uv2, 0x01FFFFFF);// 0xFFFFFFFF);abgr
                    }
                }

                IsMouseIn = ImGuiAPI.IsMouseHoveringRect(in min, in max, true);

                if(ImGuiAPI.BeginChild("ViewportClient", in sz, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove| ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground))
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
                OnVieportClosed();
            }
        }
        public Vector2 WorldAxis { get; set; } = new Vector2(80, 50);
        public bool ShowWorldAxis { get; set; } = true;
        public void DrawWorldAxis(UCamera camera)
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
            cmdlst.AddLine(in v2Center, v2Center + v2X, (uint)Color.Red.ToAbgr(), 1);
            cmdlst.AddLine(in v2Center, v2Center + v2Y, (uint)Color.Green.ToAbgr(), 1);
            cmdlst.AddLine(in v2Center, v2Center + v2Z, (uint)Color.Blue.ToAbgr(), 1);

            cmdlst.AddText(v2Center + v2X, (uint)Color.Red.ToAbgr(), "x", null);
            cmdlst.AddText(v2Center + v2Y, (uint)Color.Green.ToAbgr(), "y", null);
            cmdlst.AddText(v2Center + v2Z, (uint)Color.Blue.ToAbgr(), "z", null);
        }
        protected virtual void OnVieportClosed()
        {
            //mPresentWindow.UnregEventProcessor(this);
        }
        protected virtual void OnClientChanged(bool bSizeChanged)
        {
            //if (mDefaultHUD != null)
            //    mDefaultHUD.WindowSize = new SizeF(this.ClientSize.X, this.ClientSize.Y);
            foreach (var i in mHUDStack)
            {
                _ = i.WindowSize = new SizeF(this.ClientSize.X, this.ClientSize.Y);
            }
        }
        public void ProcessHitproxySelected(float mouseX, float mouseY)
        {
            var edtorPolicy = this.RenderPolicy as Graphics.Pipeline.URenderPolicy;
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
            else if(e.Type == Bricks.Input.EventType.MOUSEBUTTONDOWN)
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
        protected virtual IntPtr GetShowTexture()
        {
            return IntPtr.Zero;
        }
        public virtual void OnHitproxySelected(Graphics.Pipeline.IProxiable proxy)
        {
            var edtorPolicy = this.RenderPolicy as Graphics.Pipeline.URenderPolicy;
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
            var edtorPolicy = this.RenderPolicy as Graphics.Pipeline.URenderPolicy;
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
            var editorPolicy = this.RenderPolicy as Graphics.Pipeline.URenderPolicy;
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
        public delegate System.Threading.Tasks.Task FOnInitialize(UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax);
        public FOnInitialize OnInitialize = null;
        [Rtti.Meta]
        public virtual async Task Initialize(USlateApplication application, RName policyName, float zMin, float zMax)
        {
            var policy = Bricks.RenderPolicyEditor.URenderPolicyAsset.LoadAsset(policyName).CreateRenderPolicy(this);
            if (OnInitialize != null)
            {
                await OnInitialize(this, application, policy, zMin, zMax);
            }
            await this.World.InitWorld();
            SetCameraOffset(in DVector3.Zero);

            mDefaultHUD.RenderCamera = this.RenderPolicy.DefaultCamera;
            mHUDStack.Push(mDefaultHUD);

            IsInlitialized = true;
        }

        #region HUD
        protected UI.TtUIHost mDefaultHUD = new UI.TtUIHost();
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly | Rtti.MetaAttribute.EMetaFlags.Unserializable)]
        public UI.TtUIHost DefaultHUD
        {
            get => mDefaultHUD;
            set
            {
                mDefaultHUD = value;
                if(mDefaultHUD != null)
                {
                    mDefaultHUD.RenderCamera = RenderPolicy.DefaultCamera;
                    mHUDStack.Clear();
                    mHUDStack.Push(value);
                    OnClientChanged(true);
                }
            }
        }
        protected Stack<UI.TtUIHost> mHUDStack = new Stack<UI.TtUIHost>();
        public void PushHUD(UI.TtUIHost hud)
        {
            hud.WriteFlag(UI.Controls.TtUIElement.ECoreFlags.IsScreenSpace, true);
            hud.ViewportSlate = this;
            mHUDStack.Push(hud);
        }
        public void PopHUD()
        {
            var hud = mHUDStack.Peek();
            hud.WriteFlag(UI.Controls.TtUIElement.ECoreFlags.IsScreenSpace, false);
            hud.ViewportSlate = null;
            mHUDStack.Pop();
        }
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly | Rtti.MetaAttribute.EMetaFlags.Unserializable)]
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
        public virtual unsafe void TickLogic(float ellapse)
        {
            if (IsInlitialized == false)
                return;

            foreach (var i in mHUDStack)
            {
                _ = i.BuildMesh();
            }
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
        public List<WeakReference<UViewportSlate>> Viewports { get; } = new List<WeakReference<UViewportSlate>>();
        public UViewportSlate GetPressedViewport()
        {
            foreach (var i in Viewports)
            {
                UViewportSlate t;
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
        public void AddViewport(UViewportSlate slate)
        {
            var rmv = new List<WeakReference<UViewportSlate>>();
            foreach (var i in Viewports)
            {
                UViewportSlate t;
                if (i.TryGetTarget(out t) == false)
                {
                    rmv.Add(i);
                    continue;
                }
                else if (t == slate)
                    return;
            }
            Viewports.Add(new WeakReference<UViewportSlate>(slate));
            foreach (var i in rmv)
            {
                Viewports.Remove(i);
            }
        }
        public void RemoveViewport(UViewportSlate slate)
        {
            var rmv = new List<WeakReference<UViewportSlate>>();
            foreach (var i in Viewports)
            {
                UViewportSlate t;
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
}

namespace EngineNS
{
    partial class UEngine
    {
        public Graphics.Pipeline.UViewportSlateManager ViewportSlateManager { get; } = new Graphics.Pipeline.UViewportSlateManager();
    }
}
