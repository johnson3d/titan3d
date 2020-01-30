using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DockControl;
using EngineNS;
using EngineNS.IO;

namespace EditorCommon.ViewPort
{
    public class PreviewActorContainer
    {
        public EngineNS.GamePlay.Actor.GActor mPreviewActor = null;
        EngineNS.Thread.Async.TaskLoader.WaitContext mWaitContext = new EngineNS.Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task<EngineNS.Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        {
            return await EngineNS.Thread.Async.TaskLoader.Awaitload(mWaitContext);
        }
        public void ReleaseWaitContext()
        {
            EngineNS.Thread.Async.TaskLoader.Release(ref mWaitContext, this);
        }
    }

    /// <summary>
    /// D3DViewerControl.xaml 的交互逻辑
    /// </summary>
    public partial class ViewPortControl : UserControl, EngineNS.ITickInfo, DockControl.IDockAbleControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public delegate void DelegateRefreshActors();
        public delegate void DelegateOperationActor(EngineNS.GamePlay.Actor.GActor actor);
        public delegate void DelegateSelectActors(SelectActorData[] actors, EngineNS.GamePlay.Actor.GActor actor);

        public event DelegateRefreshActors DRefreshActors;
        public event DelegateOperationActor DAddActor;
        public event DelegateOperationActor DRemoveActor;
        //public event DelegateOperationActor DSelectActor;
        //public event DelegateSelectActors DSelectActors;
        public IntPtr DrawHandle
        {
            get
            {
                return this.DrawPanel.Handle;
            }
        }

        public bool ShowToolbar
        {
            get { return (bool)GetValue(ShowToolbarProperty); }
            set { SetValue(ShowToolbarProperty, value); }
        }
        public static readonly DependencyProperty ShowToolbarProperty = DependencyProperty.Register("ShowToolbar", typeof(bool), typeof(ViewPortControl), new FrameworkPropertyMetadata(true));


        public ViewPortControl()
        {
            InitializeComponent();

            this.Loaded += (object sender, RoutedEventArgs e) => { IsShowing = true; IsActive = true; };
            this.Unloaded += (object sender, RoutedEventArgs e) => { IsActive = false; };

            OnViewSizeChanged = delegate (EditorCommon.ViewPort.ViewPortControl vp)
            {
                var noUsed = vp.ResizeEnviroment();
            };
        }
        //public double GetWidth()
        //{
        //    if (double.IsNaN(ActualWidth) || ActualWidth == 0)
        //        return MinWidth;
        //    return ActualWidth;
        //}
        //public double GetHeight()
        //{
        //    if (double.IsNaN(ActualHeight) || ActualHeight == 0)
        //        return MinHeight;
        //    return ActualHeight;
        //}
        public int GetViewPortWidth()
        {
            return DrawPanel.Width;
        }
        public int GetViewPortHeight()
        {
            return DrawPanel.Height;
        }

        public EngineNS.Graphics.CGfxCamera Camera;
        public EngineNS.Graphics.View.CGfxSceneView EditorViewPort;
        EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile mRPolicy = null;
        public EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile RPolicy
        {
            get
            {
                return mRPolicy;
            }
            set
            {
                var test = InitBaseAix(value);
                mRPolicy = value;
            }
        }

        EngineNS.GamePlay.GWorld mWorld;
        public EngineNS.GamePlay.GWorld World;

        public void Cleanup()
        {
            Camera?.Cleanup();
            RPolicy?.Cleanup();
        }

        public string ViewPortName
        {
            get;
            set;
        } = "";
        bool mIsShowing = false;
        public bool IsShowing
        {
            get => mIsShowing;
            set
            {
                mIsShowing = value;
                OnPropertyChanged("IsShowing");
            }
        }
        bool mIsActive = false;
        public bool IsActive
        {
            get => mIsActive;
            set
            {
                mIsActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        public string KeyValue
        {
            get;
            private set;
        } = "Viewport";

        int mIndex = 0;
        public int Index
        {
            get => mIndex;
            set
            {
                mIndex = value;
                KeyValue = "Viewport " + (mIndex + 1);
                OnPropertyChanged("KeyValue");
            }
        }

        public bool EnableTick
        {
            get;
            set;
        } = true;

        public string DockGroup => "";
        EngineNS.GamePlay.Camera.CameraController mCameraController;
        EngineNS.GamePlay.Actor.GActor mDirLightActor;
        public EngineNS.GamePlay.Actor.GActor DirLightActor => mDirLightActor;

        bool mInitialized = false;
        public delegate System.Threading.Tasks.Task FAfterInitializedAction(ViewPortControl vpctr);
        public FAfterInitializedAction AfterInitializedAction;

        private System.Windows.Forms.MouseEventHandler OnDrawPanelMouseWheel;
        private System.Windows.Forms.MouseEventHandler OnDrawPanelMouseDown;
        private System.Windows.Forms.MouseEventHandler OnDrawPanelMouseUp;
        private System.Windows.Forms.MouseEventHandler OnDrawPanelMouseMove;
        private EventHandler OnDrawPanelMouseEnter;
        private EventHandler OnDrawPanelMouseLeave;
        private System.Windows.Forms.DragEventHandler OnDrawPanelDragEnter;
        private EventHandler OnDrawPanelDragLeave;
        private System.Windows.Forms.DragEventHandler OnDrawPanelDragOver;
        private System.Windows.Forms.DragEventHandler OnDrawPanelDragDrop;

        public void SetDrawPanelMouseDownCallback(System.Windows.Forms.MouseEventHandler hander)
        {
            DrawPanel.MouseDown -= OnDrawPanelMouseDown;
            OnDrawPanelMouseDown = hander;
            DrawPanel.MouseDown += OnDrawPanelMouseDown;
        }
        public void SetDrawPanelMouseUpCallback(System.Windows.Forms.MouseEventHandler hander)
        {
            DrawPanel.MouseUp -= OnDrawPanelMouseUp;
            OnDrawPanelMouseUp = hander;
            DrawPanel.MouseUp += OnDrawPanelMouseUp;
        }
        public void SetDrawPanelMouseMoveCallback(System.Windows.Forms.MouseEventHandler hander)
        {
            DrawPanel.MouseMove -= OnDrawPanelMouseMove;
            OnDrawPanelMouseMove = hander;
            DrawPanel.MouseMove += OnDrawPanelMouseMove;
        }
        public void SetDrawPanelMouseWheelCallback(System.Windows.Forms.MouseEventHandler hander)
        {
            DrawPanel.MouseWheel -= OnDrawPanelMouseWheel;
            OnDrawPanelMouseWheel = hander;
            DrawPanel.MouseWheel += OnDrawPanelMouseWheel;
        }
        public void SetDrawPanelMouseEnterCallback(EventHandler hander)
        {
            DrawPanel.MouseEnter -= OnDrawPanelMouseEnter;
            OnDrawPanelMouseEnter = hander;
            DrawPanel.MouseEnter += OnDrawPanelMouseEnter;
        }
        public void SetDrawPanelMouseLeaveCallback(EventHandler hander)
        {
            DrawPanel.MouseLeave -= OnDrawPanelMouseLeave;
            OnDrawPanelMouseLeave = hander;
            DrawPanel.MouseLeave += OnDrawPanelMouseLeave;
        }
        public void SetDrawPanelDragEnterCallback(System.Windows.Forms.DragEventHandler hander)
        {
            DrawPanel.DragEnter -= OnDrawPanelDragEnter;
            OnDrawPanelDragEnter = hander;
            DrawPanel.DragEnter += OnDrawPanelDragEnter;
        }
        public void SetDrawPanelDragLeaveCallback(EventHandler hander)
        {
            DrawPanel.DragLeave -= OnDrawPanelDragLeave;
            OnDrawPanelDragLeave = hander;
            DrawPanel.DragLeave += OnDrawPanelDragLeave;
        }
        public void SetDrawPanelDragOverCallback(System.Windows.Forms.DragEventHandler hander)
        {
            DrawPanel.DragOver -= OnDrawPanelDragOver;
            OnDrawPanelDragOver = hander;
            DrawPanel.DragOver += OnDrawPanelDragOver;
        }
        public void SetDrawPanelDragDropCallback(System.Windows.Forms.DragEventHandler hander)
        {
            DrawPanel.DragDrop -= OnDrawPanelDragDrop;
            OnDrawPanelDragDrop = hander;
            DrawPanel.DragDrop += OnDrawPanelDragDrop;
        }
        EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new EngineNS.Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task<EngineNS.Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        {
            return await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        }
        public async System.Threading.Tasks.Task<bool> InitEnviroment()
        {
            if (mInitialized)
            {
                return true;
            }
            mInitialized = true;

            await EngineNS.CEngine.Instance.AwaitEngineInited();

            OnDrawPanelMouseDown = DrawPanel_MouseDown_Default;
            OnDrawPanelMouseMove = DrawPanel_MouseMove_Default;
            OnDrawPanelMouseUp = DrawPanel_MouseUp_Default;
            OnDrawPanelMouseWheel = DrawPanel_MouseWheel_Default;
            OnDrawPanelMouseEnter = DrawPanel_MouseEnter_Default;
            OnDrawPanelMouseLeave = DrawPanel_MouseLeave_Default;
            OnDrawPanelDragEnter = DrawPanel_DragEnter_Default;
            OnDrawPanelDragLeave = DrawPanel_DragLeave_Default;
            OnDrawPanelDragOver = DrawPanel_DragOver_Default;
            OnDrawPanelDragDrop = DrawPanel_DragDrop_Default;

            DrawPanel.MouseDown += OnDrawPanelMouseDown;
            DrawPanel.MouseMove += OnDrawPanelMouseMove;
            DrawPanel.MouseUp += OnDrawPanelMouseUp;
            DrawPanel.MouseWheel += OnDrawPanelMouseWheel;
            DrawPanel.MouseEnter += OnDrawPanelMouseEnter;
            DrawPanel.MouseLeave += OnDrawPanelMouseLeave;
            DrawPanel.DragEnter += OnDrawPanelDragEnter;
            DrawPanel.DragLeave += OnDrawPanelDragLeave;
            DrawPanel.DragOver += OnDrawPanelDragOver;
            DrawPanel.DragDrop += OnDrawPanelDragDrop;

            var rc = EngineNS.CEngine.Instance.RenderContext;
            //EngineNS.CSwapChainDesc desc;
            //desc.Format = EngineNS.EPixelFormat.PXF_R8G8B8A8_UNORM;
            //desc.Width = (UInt32)DrawPanel.Width;
            //desc.Height = (UInt32)DrawPanel.Height;
            //desc.WindowHandle = DrawPanel.Handle;
            //SwapChain = rc.CreateSwapChain(desc);

            //rc.BindCurrentSwapChain(SwapChain);

            //var evpDesc = new EngineNS.Graphics.CGfxSceneViewInfo();
            //evpDesc.mDisuseDSV = true;
            //evpDesc.Width = desc.Width;
            //evpDesc.Height = desc.Height;
            //evpDesc.DepthStencil.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
            //evpDesc.DepthStencil.Width = desc.Width;
            //evpDesc.DepthStencil.Height = desc.Height;
            //var rtDesc = new EngineNS.CRenderTargetViewDesc();
            //rtDesc.mCanBeSampled = 0;
            //evpDesc.mRTVDescArray.Add(rtDesc);
            //EditorViewPort = new EngineNS.Graphics.CGfxSceneView();
            //EditorViewPort.Init(rc, SwapChain, evpDesc);

            Camera = new EngineNS.Graphics.CGfxCamera();
            Camera.Init(rc, false);

            var eye = new EngineNS.Vector3();
            eye.SetValue(0.0f, 0.0f, -3.6f);
            var at = new EngineNS.Vector3();
            at.SetValue(0.0f, 0.0f, 0.0f);
            var up = new EngineNS.Vector3();
            up.SetValue(0.0f, 1.0f, 0.0f);
            Camera.LookAtLH(eye, at, up);
            Camera.PerspectiveFovLH(Camera.mDefaultFoV, DrawPanel.Width, DrawPanel.Height, 0.1f, 1000.0f);
            //Camera.MakeOrtho(DrawPanel.Width, DrawPanel.Height, 0.1f, 1000.0f);
            //Camera.SetSceneView(rc, EditorViewPort);

            //var dirLightDirection = new EngineNS.Vector3(1, -1, 1);
            //float DirLightIntensity = 1;
            //float DirLightSpecularIntensity = 1;
            //var DirLightingAmbient = new EngineNS.Color4(0, 0, 0, 0);
            //var DirLightingDiffuse = new EngineNS.Color4(1, 1, 1, 1);
            //var DirLightingSpecular = new EngineNS.Color4(1, 1, 1, 1);
            //float DirLightShadingSSS = 1;

            //dirLightDirection.Normalize();
            //Camera.mSceneView.DirLightDirection = dirLightDirection;
            //Camera.mSceneView.DirLightIntensity = DirLightIntensity;
            //Camera.mSceneView.DirLightSpecularIntensity = DirLightSpecularIntensity;
            //Camera.mSceneView.DirLightingAmbient = DirLightingAmbient;
            //Camera.mSceneView.DirLightingDiffuse = DirLightingDiffuse;
            //Camera.mSceneView.DirLightingSpecular = DirLightingSpecular;
            //Camera.mSceneView.DirLightShadingSSS = DirLightShadingSSS;

            mCameraController = new Editor.Camera.EditorViewCameraController();
            mCameraController.Camera = Camera;

            OnCameraOperationOnMouseMove = CameraOperationOnMouseMove_Default;
            OnCameraOperationOnMouseWheel = CameraOperationOnMouseWheel_Default;

            //var meshSource = EngineNS.CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(rc, EngineNS.CEngine.Instance.FileManager.GetRName("editor/sun.gms", EngineNS.RName.enRNameType.Editor), true);
            //var mesh = EngineNS.CEngine.Instance.MeshManager.CreateMesh(rc, meshSource/*, EngineNS.CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv(EngineNS.RName.GetRName("ShadingEnv/DSBase2RT.senv"))*/);
            mDirLightActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(EngineNS.RName.GetRName("editor/sun.gms"));
            mDirLightActor.Placement.Scale = new EngineNS.Vector3(0.1f);
            var meshComponent = mDirLightActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            var matIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.RName.GetRName("Material/defaultmaterial.instmtl"));
            for (int i = 0; i < meshComponent.MaterialNumber; i++)
            {
                var noused = meshComponent.SetMaterialInstanceAsync(rc, (uint)i, matIns, null);
            }

            OnDirectionLightOperation_MouseDown = DirectionLightOperation_MouseDown_Default;
            OnDirectionLightOperation_MouseMove = DirectionLightOperation_MouseMove_Default;
            OnDirectionLightOperation_MouseUp = DirectionLightOperation_MouseUp_Default;

            //EngineNS.CEngine.Instance.TickManager.AddTickInfo(this);
            EngineNS.CEngine.Instance.GameEditorInstance.AddTickInfo(this);

            AfterInitializedAction?.Invoke(this);
            await InitializeAxis();
            await InitializeGridline();
            if (WaitContext != null)
                EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);

            return true;
        }

        public void AddMouseUpEvent(System.Windows.Forms.MouseEventHandler e)
        {
            DrawPanel.MouseUp -= e;
            DrawPanel.MouseUp += e;
        }
        public async System.Threading.Tasks.Task WaitInitComplated()
        {
            await AwaitLoad();
        }

        public delegate System.Threading.Tasks.Task FOnWindowCreated(ViewPortControl vp);
        public FOnWindowCreated OnWindowCreated;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var noUse = InitEnviroment();
            if (OnWindowCreated != null)
                OnWindowCreated(this);
            EnableTick = true;

            //var test = Init_SYJ();
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            EnableTick = false;
        }
        public delegate void FOnViewSizeChanged(ViewPortControl vp);
        public FOnViewSizeChanged OnViewSizeChanged;
        private void Changed_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (OnViewSizeChanged != null)
                OnViewSizeChanged(this);
        }
        public async System.Threading.Tasks.Task ResizeEnviroment()
        {
            if (EngineNS.CEngine.Instance == null /*|| SwapChain == null*/)
                return;
            await EngineNS.CEngine.Instance.AwaitEngineInited();
            var rc = EngineNS.CEngine.Instance.RenderContext;
            //SwapChain.OnResize((UInt32)DrawPanel.Width, (UInt32)DrawPanel.Height);

            //rc.BindCurrentSwapChain(SwapChain);
            //this.EditorViewPort.OnResize(rc, SwapChain, (UInt32)DrawPanel.Width, (UInt32)DrawPanel.Height);
            //Camera.PerspectiveFovLH(Camera.mDefaultFoV, (float)DrawPanel.Width, (float)DrawPanel.Height);

            RPolicy?.OnResize(rc, RPolicy.SwapChain, (UInt32)DrawPanel.Width, (UInt32)DrawPanel.Height);
        }

        public delegate void FTickViewPortControl(ViewPortControl vp);
        public FTickViewPortControl TickLogicEvent = Default_TickLogic;
        public FTickViewPortControl TickRenderEvent = Default_TickRender;
        public FTickViewPortControl TickSyncEvent = Default_TickSync;
        private static void Default_TickLogic(ViewPortControl vp)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if (/*vp.Camera.mSceneView != null &&*/ vp.World != null)
            {
                //var clrColors = new EngineNS.CMRTClearColor[]
                //{
                //    new EngineNS.CMRTClearColor(0, 0x00FF8080),
                //    new EngineNS.CMRTClearColor(1, 0x00808080)
                //};
                //vp.Camera.DisplayView.ClearMRT(clrColors, true, 1.0F, true, 0);

                //vp.Camera.DisplayView.ClearPasses();

                vp.World.CheckVisible(rc.ImmCommandList, vp.Camera);
                vp.World.Tick();
            }
            vp.RPolicy?.TickLogic(null, rc);
        }
        private static void Default_TickRender(ViewPortControl vp)
        {
            vp.RPolicy?.TickRender(vp.RPolicy.SwapChain);
        }
        private static void Default_TickSync(ViewPortControl vp)
        {
            vp.RPolicy?.TickSync();
        }
        //public static EngineNS.Profiler.TimeScope ScopeTickLogic = EngineNS.Profiler.TimeScopeManager.GetTimeScope(typeof(ViewPortControl), nameof(TickLogic));
        public EngineNS.Profiler.TimeScope GetLogicTimeScope()
        {
            //return ScopeTickLogic;
            return null;
        }
        public void TickLogic()
        {
            if (TickLogicEvent != null)
                TickLogicEvent(this);

            UpdateAxisShow();
            DrawGridline();
        }

        public float MaxMoveSpeed
        {
            get;
        } = 0.1f;
        public float MinMoveSpeed
        {
            get;
        } = 0.001f;
        float mMoveSpeed = 0.01f;
        public float MoveSpeed
        {
            get => mMoveSpeed;
            set
            {
                if (value > MaxMoveSpeed)
                    mMoveSpeed = MaxMoveSpeed;
                else if (value < MinMoveSpeed)
                    mMoveSpeed = MinMoveSpeed;
                else
                    mMoveSpeed = value;
                OnPropertyChanged("MoveSpeed");
            }
        }
        public void Default_KeysTick(ViewPortControl vp)
        {
            // 按键移动摄像机
            if (mMouseDown)
            {
                if (Keyboard.IsKeyDown(Key.W))
                {
                    vp.mCameraController.Move(EngineNS.GamePlay.Camera.eCameraAxis.Forward, EngineNS.CEngine.Instance.EngineElapseTime * MoveSpeed, true);
                    UpdateAxisShow();
                }
                if (Keyboard.IsKeyDown(Key.S))
                {
                    vp.mCameraController.Move(EngineNS.GamePlay.Camera.eCameraAxis.Forward, -EngineNS.CEngine.Instance.EngineElapseTime * MoveSpeed, true);
                    UpdateAxisShow();
                }
                if (Keyboard.IsKeyDown(Key.A))
                {
                    vp.mCameraController.Move(EngineNS.GamePlay.Camera.eCameraAxis.Right, EngineNS.CEngine.Instance.EngineElapseTime * MoveSpeed, true);
                    UpdateAxisShow();
                }
                if (Keyboard.IsKeyDown(Key.D))
                {
                    vp.mCameraController.Move(EngineNS.GamePlay.Camera.eCameraAxis.Right, -EngineNS.CEngine.Instance.EngineElapseTime * MoveSpeed, true);
                    UpdateAxisShow();
                }

            }
        }

        public void TickRender()
        {
            if (TickRenderEvent != null)
                TickRenderEvent(this);
        }
        public void BeforeFrame()
        {
            Camera.BeforeFrame();
        }
        public void TickSync()
        {
            if (TickSyncEvent != null)
                TickSyncEvent(this);

            if (mNeedCameraFocusOpr == true)
            {
                if (float.IsNaN(mCameraPosOffset_FocusOpr.X) ||
                    float.IsNaN(mCameraPosOffset_FocusOpr.Y) ||
                    float.IsNaN(mCameraPosOffset_FocusOpr.Z))
                    mCameraPosOffset_FocusOpr = Vector3.Zero;
                if (float.IsNaN(mCameraLookAtOffset_FocusOpr.X) ||
                    float.IsNaN(mCameraLookAtOffset_FocusOpr.Y) ||
                    float.IsNaN(mCameraLookAtOffset_FocusOpr.Z))
                    mCameraLookAtOffset_FocusOpr = Vector3.Zero;
                Camera.LookAtLH(Camera.CameraData.Position + mCameraPosOffset_FocusOpr, Camera.CameraData.LookAt + mCameraLookAtOffset_FocusOpr, Camera.CameraData.Up);
                Camera.SwapBuffer();
                mStepCount_FocusOpr++;
                if (mStepCount_FocusOpr >= mMaxStep_FocusOpr)
                {
                    mStepCount_FocusOpr = 0;
                    mNeedCameraFocusOpr = false;
                }
            }
            else
            {
                Camera.SwapBuffer();
            }

            //Camera?.CBuffer?.FlushContent2(EngineNS.CEngine.Instance.RenderContext);
            Default_KeysTick(this);
        }

        public void SaveElement(XmlNode node, XmlHolder holder)
        {
            throw new NotImplementedException();
        }

        public IDockAbleControl LoadElement(XmlNode node)
        {
            throw new NotImplementedException();
        }

        public void StartDrag()
        {
        }

        public void EndDrag()
        {
        }

        public bool? CanClose()
        {
            return true;
        }

        public void Closed()
        {
            throw new NotImplementedException();
        }

        public class CameraOperationData
        {
            public System.Drawing.Point PreMousePt;
            public System.Drawing.Point NowMousePt;
            public System.Windows.Forms.MouseEventArgs Args;
        }
        CameraOperationData mCameraOperationData = new CameraOperationData();
        public delegate void Delegate_CameraOperation(CameraOperationData data);
        public Delegate_CameraOperation OnCameraOperationOnMouseMove;
        public Delegate_CameraOperation OnCameraOperationOnMouseWheel;
        private void CameraOperationOnMouseMove_Default(CameraOperationData data)
        {
            if (data.Args.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    mCameraController.Rotate(EngineNS.GamePlay.Camera.eCameraAxis.Up, (data.NowMousePt.X - mPreMousePt.X) * 0.01f);
                    mCameraController.Rotate(EngineNS.GamePlay.Camera.eCameraAxis.Right, (data.NowMousePt.Y - mPreMousePt.Y) * 0.01f);
                    UpdateAxisShow();
                }
            }
            else if (data.Args.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    mCameraController.Move(EngineNS.GamePlay.Camera.eCameraAxis.Forward, (data.NowMousePt.Y - mPreMousePt.Y) * 0.03f);
                    UpdateAxisShow();
                }
                else
                {
                    mCameraController.Rotate(EngineNS.GamePlay.Camera.eCameraAxis.Up, (data.NowMousePt.X - mPreMousePt.X) * 0.01f, true);
                    mCameraController.Rotate(EngineNS.GamePlay.Camera.eCameraAxis.Right, (data.NowMousePt.Y - mPreMousePt.Y) * 0.01f, true);
                    UpdateAxisShow();
                }
            }
            else if (data.Args.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                mCameraController.Move(EngineNS.GamePlay.Camera.eCameraAxis.Right, (data.NowMousePt.X - mPreMousePt.X) * 0.01f);
                mCameraController.Move(EngineNS.GamePlay.Camera.eCameraAxis.Up, (data.NowMousePt.Y - mPreMousePt.Y) * 0.01f);
                UpdateAxisShow();
            }
        }
        private void CameraOperationOnMouseWheel_Default(CameraOperationData data)
        {
            if (mMouseDown)
            {
                MoveSpeed += (float)(data.Args.Delta * 0.00001);
            }
            else
            {
                mCameraController.Move(EngineNS.GamePlay.Camera.eCameraAxis.Forward, data.Args.Delta * 0.005f);
                UpdateAxisShow();
            }
        }

        public class LightOperationData
        {
            public System.Drawing.Point PreMousePt;
            public System.Drawing.Point NowMousePt;
            public System.Windows.Forms.MouseEventArgs Args;
            public float LightDistance = 0;
        }
        LightOperationData mLightOperationData = new LightOperationData();
        public delegate void Delegate_DirectionLightOperation(LightOperationData data);
        public Delegate_DirectionLightOperation OnDirectionLightOperation_MouseDown;
        public Delegate_DirectionLightOperation OnDirectionLightOperation_MouseMove;
        public Delegate_DirectionLightOperation OnDirectionLightOperation_MouseUp;
        private void DirectionLightOperation_MouseDown_Default(LightOperationData data)
        {
            // 添加太阳光显示模型
            if (World != null && mDirLightActor != null)
            {
                World.AddActor(mDirLightActor);
                World.DefaultScene?.AddActor(mDirLightActor);
            }
            // 计算太阳光应出现的位置
            var pos = EngineNS.Vector3.Zero;
            var vpPos = Camera.CameraData.Trans2ViewPort(ref pos);
            var mousePos = new EngineNS.Vector3(data.NowMousePt.X, data.NowMousePt.Y, vpPos.Z);
            var len = (mousePos - vpPos).Length();
            var delta = len * (Camera.CameraData.Position - pos).Length() * 0.001f;
            data.LightDistance = delta;

            var dir = new Vector3(Camera.SceneView.mDirLightDirection_Leak.X, Camera.SceneView.mDirLightDirection_Leak.Y, Camera.SceneView.mDirLightDirection_Leak.Z);
            dir.Normalize();
            mDirLightActor.Placement.Location = dir * data.LightDistance;
        }
        private void DirectionLightOperation_MouseMove_Default(LightOperationData data)
        {
            var dir = new Vector3(Camera.SceneView.mDirLightDirection_Leak.X, Camera.SceneView.mDirLightDirection_Leak.Y, Camera.SceneView.mDirLightDirection_Leak.Z);
            dir.Normalize();
            float TempLeak = Camera.SceneView.mDirLightDirection_Leak.W;

            var up = EngineNS.Vector3.UnitY;
            EngineNS.Vector3 right;
            EngineNS.Vector3.Cross(ref dir, ref up, out right);
            EngineNS.Vector3.Cross(ref dir, ref right, out up);
            var upMat = EngineNS.Matrix.RotationAxis(up, (data.NowMousePt.X - mPreMousePt.X) * 0.01f);
            var rightMat = EngineNS.Matrix.RotationAxis(right, (data.NowMousePt.Y - mPreMousePt.Y) * 0.01f);
            dir = EngineNS.Vector3.TransformCoordinate(dir, upMat);
            dir = EngineNS.Vector3.TransformCoordinate(dir, rightMat);
            mDirLightActor.Placement.Location = -dir * data.LightDistance;
            var quat = EngineNS.Quaternion.GetQuaternion(EngineNS.Vector3.UnitY, dir);
            mDirLightActor.Placement.Rotation = quat;
            Camera.SceneView.mDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, TempLeak);
            
        }
        private void DirectionLightOperation_MouseUp_Default(LightOperationData data)
        {
            if (World != null && mDirLightActor != null)
            {
                World.RemoveActor(mDirLightActor.ActorId);
                World.DefaultScene?.RemoveActor(mDirLightActor.ActorId);
            }
        }

        public bool CanMouseDown = true;
        System.Drawing.Point mPreMousePt;
        System.Drawing.Point mMouseDownPt;
        bool mMouseDown = false;
        public bool IsMouseDown
        {
            get => mMouseDown;
        }
        bool mMouseLeftDown = false;
        private void DrawPanel_MouseDown_Default(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //Off mesh links..
            if (CanMouseDown == false)
            {
                OffMeshLinks_MouseDown(sender, e);
                //return;
            }

            mMouseDown = true;
            mFirstTransAxis = true;
            mPreMousePt = e.Location;
            mMouseDownPt = e.Location;

            mScreenWidthCountX = 0;
            mScreenHeightCountY = 0;
            var screenPos = new ResourceLibrary.Win32.POINT();
            ResourceLibrary.Win32.GetCursorPos(ref screenPos);
            mMouseDownScreenRect = System.Windows.Forms.Screen.GetWorkingArea(new System.Drawing.Point(screenPos.X, screenPos.Y));

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                mMouseLeftDown = true;
            //Mouse.Capture(this);
            if ((System.Windows.Input.Keyboard.GetKeyStates(Key.L) & KeyStates.Down) == KeyStates.Down)
            {
                mLightOperationData.PreMousePt = mPreMousePt;
                mLightOperationData.NowMousePt = e.Location;
                mLightOperationData.Args = e;
                OnDirectionLightOperation_MouseDown?.Invoke(mLightOperationData);
            }

            {
                var ee = new EngineNS.Input.Device.Mouse.MouseEventArgs();
                ee.Button = (EngineNS.Input.Device.Mouse.MouseButtons)(UInt32)e.Button;
                ee.Clicks = e.Clicks;
                ee.Delta = e.Delta;
                ee.X = e.X;
                ee.Y = e.Y;
                var me = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();
                me.MouseEvent = ee;

                try
                {
                    ((EngineNS.GamePlay.IModuleInstance)EngineNS.CEngine.Instance.GameEditorInstance)?.MouseDown(this, sender, me);
                }
                catch (Exception ex)
                {
                    EngineNS.Profiler.Log.WriteException(ex);
                }
            }

            StartTransAxis(e);
        }

        public delegate void DelegateOffMeshLinksClick(EngineNS.Vector3 pos, bool left);
        public event DelegateOffMeshLinksClick OffMeshLinksClick;
        private void OffMeshLinks_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (OffMeshLinksClick == null)
                return;
            EngineNS.Vector3 pos = GetPickRayLineCheckPosition(e.X, e.Y);
            OffMeshLinksClick(pos, e.Button == System.Windows.Forms.MouseButtons.Left);

        }

        int mScreenWidthCountX = 0;
        int mScreenHeightCountY = 0;
        System.Drawing.Rectangle mMouseDownScreenRect;
        private void DrawPanel_MouseMove_Default(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var newLoc = e.Location;
            if (mMouseDown)
            {
                var oldSWCX = mScreenWidthCountX;
                var oldSHCY = mScreenHeightCountY;

                var screenPos = new ResourceLibrary.Win32.POINT();
                ResourceLibrary.Win32.GetCursorPos(ref screenPos);
                if (screenPos.Y <= mMouseDownScreenRect.Top)
                {
                    screenPos.Y = mMouseDownScreenRect.Bottom - 2;
                    ResourceLibrary.Win32.SetCursorPos(screenPos.X, screenPos.Y);
                    mScreenHeightCountY--;
                }
                else if (screenPos.Y >= mMouseDownScreenRect.Bottom - 1)
                {
                    screenPos.Y = mMouseDownScreenRect.Top + 1;
                    ResourceLibrary.Win32.SetCursorPos(screenPos.X, screenPos.Y);
                    mScreenHeightCountY++;
                }
                if (screenPos.X <= mMouseDownScreenRect.Left)
                {
                    screenPos.X = mMouseDownScreenRect.Right - 2;
                    ResourceLibrary.Win32.SetCursorPos(screenPos.X, screenPos.Y);
                    mScreenWidthCountX--;
                }
                else if (screenPos.X >= mMouseDownScreenRect.Right - 1)
                {
                    screenPos.X = mMouseDownScreenRect.Left + 1;
                    ResourceLibrary.Win32.SetCursorPos(screenPos.X, screenPos.Y);
                    mScreenWidthCountX++;
                }

                var offset = DrawPanel.PointToScreen(new System.Drawing.Point(0, 0));
                //if(oldSWCX != mScreenWidthCountX || oldSHCY != mScreenHeightCountY)
                //    System.Diagnostics.Debug.WriteLine($"{mScreenWidthCountX},{mScreenHeightCountY}");

                newLoc.X = screenPos.X - offset.X + mScreenWidthCountX * mMouseDownScreenRect.Width;
                newLoc.Y = screenPos.Y - offset.Y + mScreenHeightCountY * mMouseDownScreenRect.Height;
                if ((System.Windows.Input.Keyboard.GetKeyStates(Key.L) & KeyStates.Down) == KeyStates.Down)
                {
                    mLightOperationData.PreMousePt = mPreMousePt;
                    mLightOperationData.NowMousePt = newLoc;
                    mLightOperationData.Args = e;
                    OnDirectionLightOperation_MouseMove?.Invoke(mLightOperationData);
                }
                else
                {
                    mCameraOperationData.PreMousePt = mPreMousePt;
                    mCameraOperationData.Args = e;
                    mCameraOperationData.NowMousePt = newLoc;
                    OnCameraOperationOnMouseMove?.Invoke(mCameraOperationData);
                }

                mPreMousePt = newLoc;
                var noTUse = TransAxis(newLoc, e);
            }

            var noUse = MousePointToItem(e);

            {
                var ee = new EngineNS.Input.Device.Mouse.MouseEventArgs();
                ee.Button = (EngineNS.Input.Device.Mouse.MouseButtons)(UInt32)e.Button;
                ee.Clicks = e.Clicks;
                ee.Delta = e.Delta;
                ee.X = e.X;
                ee.Y = e.Y;
                var me = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();
                me.MouseEvent = ee;

                try
                {
                    ((EngineNS.GamePlay.IModuleInstance)EngineNS.CEngine.Instance.GameEditorInstance)?.MouseMove(this, sender, me);
                }
                catch (Exception ex)
                {
                    EngineNS.Profiler.Log.WriteException(ex);
                }
            }
        }

        private void DrawPanel_MouseUp_Default(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mMouseDown = false;
            //Mouse.Capture(null);
            mLightOperationData.PreMousePt = mPreMousePt;
            mLightOperationData.NowMousePt = e.Location;
            mLightOperationData.Args = e;
            OnDirectionLightOperation_MouseUp?.Invoke(mLightOperationData);

            {
                var ee = new EngineNS.Input.Device.Mouse.MouseEventArgs();
                ee.Button = (EngineNS.Input.Device.Mouse.MouseButtons)(UInt32)e.Button;
                ee.Clicks = e.Clicks;
                ee.Delta = e.Delta;
                ee.X = e.X;
                ee.Y = e.Y;
                var me = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();
                me.MouseEvent = ee;

                try
                {
                    ((EngineNS.GamePlay.IModuleInstance)EngineNS.CEngine.Instance.GameEditorInstance)?.MouseUp(this, sender, me);
                }
                catch (Exception ex)
                {
                    EngineNS.Profiler.Log.WriteException(ex);
                }
            }

            // 鼠标点选
            if (mMouseLeftDown && !Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
            {
                var nowPos = e.Location;
                if ((nowPos.X - mMouseDownPt.X) < 5 && (nowPos.Y - mMouseDownPt.Y) < 5)
                    MouseSelectItem(e);
            }
            mMouseLeftDown = false;
            EndTransAxis();
        }

        private void DrawPanel_MouseEnter_Default(object sender, EventArgs e)
        {
            //DrawPanel.Focus();
            {
                EngineNS.Input.Device.Mouse.MouseInputEventArgs ee = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();

                try
                {
                    ((EngineNS.GamePlay.IModuleInstance)EngineNS.CEngine.Instance.GameEditorInstance)?.MouseEnter(this, sender, ee);
                }
                catch (Exception ex)
                {
                    EngineNS.Profiler.Log.WriteException(ex);
                }
            }
        }

        private void DrawPanel_MouseLeave_Default(object sender, EventArgs e)
        {
            {
                EngineNS.Input.Device.Mouse.MouseInputEventArgs ee = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();

                try
                {
                    ((EngineNS.GamePlay.IModuleInstance)EngineNS.CEngine.Instance.GameEditorInstance)?.MouseLeave(this, sender, ee);
                }
                catch (Exception ex)
                {
                    EngineNS.Profiler.Log.WriteException(ex);
                }
            }
        }
        private void DrawPanel_MouseWheel_Default(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnCameraOperationOnMouseWheel?.Invoke(new CameraOperationData()
            {
                PreMousePt = mPreMousePt,
                NowMousePt = e.Location,
                Args = e,
            });
            {
                var ee = new EngineNS.Input.Device.Mouse.MouseEventArgs();
                ee.Button = (EngineNS.Input.Device.Mouse.MouseButtons)(UInt32)e.Button;
                ee.Clicks = e.Clicks;
                ee.Delta = e.Delta;
                ee.X = e.X;
                ee.Y = e.Y;
                var me = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();
                me.MouseEvent = ee;

                try
                {
                    ((EngineNS.GamePlay.IModuleInstance)EngineNS.CEngine.Instance.GameEditorInstance)?.MouseWheel(this, sender, me);
                }
                catch (Exception ex)
                {
                    EngineNS.Profiler.Log.WriteException(ex);
                }
            }
        }

        #region drag

        public EngineNS.Vector3 GetPickRayLineCheckPosition(float x, float y)
        {
            EngineNS.Vector3 dir = -EngineNS.Vector3.UnitY;
            if (Camera.GetPickRay(ref dir, x, y, (float)GetViewPortWidth(), (float)GetViewPortHeight()))
            {
                var start = Camera.CameraData.Position;
                var end = start + dir * 1000;
                var hitResult = new EngineNS.GamePlay.SceneGraph.VHitResult();
                EngineNS.Bricks.PhysicsCore.PhyQueryFilterData queryFilterData = new EngineNS.Bricks.PhysicsCore.PhyQueryFilterData();
                if (World.DefaultScene.LineCheckWithFilter(ref start, ref end,ref queryFilterData, ref hitResult))
                {
                    return hitResult.Position;
                }
                else
                {
                    return start + dir * 10;
                }
            }
            else
                return EngineNS.Vector3.Zero;
        }
        public bool CanDragIn { get; set; } = true;
        public bool CanDuplicate { get; set; } = true;
        public bool CanDuplicatePrefab { get; set; } = true;
        private void DrawPanel_DragEnter_Default(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (!CanDragIn)
                return;
            EditorCommon.DragDrop.DragDropManager.Instance.ShowFlyWindow(false);
            e.Effect = System.Windows.Forms.DragDropEffects.Copy;
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                foreach (var dragObj in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                {
                    var resInfo = dragObj as EditorCommon.Resources.IResourceInfoDragToViewport;
                    if (resInfo != null)
                    {
                        var noUse = resInfo.OnDragEnterViewport(this, e);
                    }
                }
            }
        }

        private void DrawPanel_DragLeave_Default(object sender, EventArgs e)
        {
            if (!CanDragIn)
                return;
            EditorCommon.DragDrop.DragDropManager.Instance.ShowFlyWindow(true);
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                foreach (var dragObj in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                {
                    var resInfo = dragObj as EditorCommon.Resources.IResourceInfoDragToViewport;
                    if (resInfo != null)
                    {
                        var noUse = resInfo.OnDragLeaveViewport(this, e);
                    }
                }
            }
        }

        private void DrawPanel_DragOver_Default(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (!CanDragIn)
                return;
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                foreach (var dragObj in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                {
                    var resInfo = dragObj as EditorCommon.Resources.IResourceInfoDragToViewport;
                    if (resInfo != null)
                    {
                        var noUse = resInfo.OnDragOverViewport(this, e);
                    }
                }
            }
        }

        private void DrawPanel_DragDrop_Default(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (!CanDragIn)
                return;
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                foreach (var dragObj in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                {
                    var resInfo = dragObj as EditorCommon.Resources.IResourceInfoDragToViewport;
                    if (resInfo != null)
                    {
                        var noUse = resInfo.OnDragDropViewport(this, e);
                    }
                }
            }
            else if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals(EditorCommon.Controls.ObjectsPlant.PlantItem.DragDropType))
            {
                foreach (var dragObj in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                {
                    var resInfo = dragObj as EditorCommon.Controls.ObjectsPlant.PlantItem;
                    if (resInfo != null)
                    {
                        var noUse = resInfo.OnDragDropViewport(this, e);
                    }
                }
            }
        }
        #endregion

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    {
                        var actors = new List<SelectActorData>(mSelectedActors.Values);
                        var redo = new Action<object>((obj) =>
                        {
                            foreach (var act in actors)
                            {
                                World.RemoveActor(act.Actor.ActorId);
                                World.DefaultScene.RemoveActor(act.Actor.ActorId);
                                //nodes.RemoveActor(act.Value.Actor);
                                DRemoveActor?.Invoke(act.Actor);
                            }
                            mSelectedActors.Clear();

                            //mCurrentAxisActor.Visible = false;
                        });
                        redo.Invoke(null);
                        UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
                        {
                            mSelectedActors.Clear();
                            foreach (var act in actors)
                            {
                                World.AddActor(act.Actor);
                                World.DefaultScene.ReAddActor(act.Actor);

                                mSelectedActors.Add(act.Actor.ActorId, act);
                                DAddActor?.Invoke(act.Actor);
                            }
                            //nodes.RefreshActors();

                        }, $"删除{mSelectedActors.Count}个对象");
                        e.Handled = true;
                    }
                    break;
                case Key.Z:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            UndoRedo.UndoRedoManager.Instance.Undo(UndoRedoKey);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.Y:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            UndoRedo.UndoRedoManager.Instance.Redo(UndoRedoKey);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.Escape:
                    {
                        SelectActor(null);
                    }
                    break;
                case Key.F:
                    {
                        ProcessFocus();
                    }
                    break;
                    //case Key.G:
                    //    {
                    //        // 专门用来根据Mesh刷新Actor SpecialName的
                    //        foreach(var actor in World.Actors)
                    //        {
                    //            if (!string.IsNullOrEmpty(actor.Value.SpecialName))
                    //                continue;
                    //            var meshComp = actor.Value.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                    //            if(meshComp != null)
                    //            {
                    //                var meshInit = meshComp.Initializer as EngineNS.GamePlay.Component.GMeshComponent.GMeshComponentInitializer;
                    //                if(meshInit.MeshName != null)
                    //                    actor.Value.SpecialName = EngineNS.GamePlay.SceneGraph.GSceneGraph.GeneratorActorSpecialNameInEditor(meshInit.MeshName.PureName(), World);
                    //            }
                    //        }
                    //    }
                    //    break;
            }
        }


        private Vector3 mCameraPosOffset_FocusOpr = new Vector3();
        private Vector3 mCameraLookAtOffset_FocusOpr = new Vector3();
        private UInt32 mMaxStep_FocusOpr = 5;
        private UInt32 mStepCount_FocusOpr = 0;
        private bool mNeedCameraFocusOpr = false;
        void ProcessFocus()
        {
            if (FocusFunc == null)
                FocusShow();
            else
                FocusFunc.Invoke();
        }
        void FocusShow()
        {
            if (World == null)
                return;
            EngineNS.BoundingBox aabb = new EngineNS.BoundingBox();
            aabb.InitEmptyBox();
            if (mSelectedActors.Count > 0)
            {
                foreach (var actor in mSelectedActors)
                {
                    EngineNS.BoundingBox actorAABB = new EngineNS.BoundingBox();
                    actor.Value.Actor.GetAABB(ref actorAABB);

                    aabb.Merge2(ref actorAABB, ref aabb);
                }
            }
            else
            {
                foreach (var actor in World.Actors)
                {
                    EngineNS.BoundingBox actorAABB = new EngineNS.BoundingBox();
                    actor.Value.GetAABB(ref actorAABB);

                    aabb.Merge2(ref actorAABB, ref aabb);
                }
            }

            if (aabb.IsEmpty())
                return;
            //EditorCommon.SnapshotProcess.SnapshotCreator.FocusShow(0, 0, (float)GetViewPortWidth(), (float)GetViewPortHeight(), aabb.Maximum, aabb.Minimum, Camera, World, 0.5);

            List<Vector3> VecList = new List<Vector3>();

            if (aabb.Maximum.X > 10000)
                aabb.Maximum.X = 10000;
            if (aabb.Maximum.Y > 10000)
                aabb.Maximum.Y = 10000;
            if (aabb.Maximum.Z > 10000)
                aabb.Maximum.Z = 10000;
            if (aabb.Minimum.X < -10000)
                aabb.Minimum.X = -10000;
            if (aabb.Minimum.Y < -10000)
                aabb.Minimum.Y = -10000;
            if (aabb.Minimum.Z < -10000)
                aabb.Minimum.Z = -10000;

            VecList.Add(aabb.Minimum);
            VecList.Add(new Vector3(aabb.Maximum.X, aabb.Minimum.Y, aabb.Minimum.Z));
            VecList.Add(new Vector3(aabb.Maximum.X, aabb.Maximum.Y, aabb.Minimum.Z));
            VecList.Add(new Vector3(aabb.Minimum.X, aabb.Maximum.Y, aabb.Minimum.Z));
            VecList.Add(new Vector3(aabb.Minimum.X, aabb.Maximum.Y, aabb.Maximum.Z));
            VecList.Add(new Vector3(aabb.Minimum.X, aabb.Minimum.Y, aabb.Maximum.Z));
            VecList.Add(new Vector3(aabb.Maximum.X, aabb.Minimum.Y, aabb.Maximum.Z));
            VecList.Add(aabb.Maximum);

            var vObjCenter = (aabb.Maximum - aabb.Minimum) * 0.5f + aabb.Minimum;

            float radius = Vector3.Distance(ref aabb.Minimum, ref aabb.Maximum)/2;

            Vector3 FinalCameraPos_FocusOpr = new Vector3();
            if (Camera.IsPerspective)
            {
                float fovAngle = 0;
                if(Camera.Aspect>1)
                {
                    fovAngle = Camera.FoV * 0.5f;
                }
                else
                {
                    fovAngle = (float)(Math.Atan(Math.Tan(Camera.FoV * 0.5) * Camera.Aspect));
                }
                float h = radius / (float)Math.Sin(fovAngle);
                FinalCameraPos_FocusOpr = vObjCenter - Camera.CameraData.Direction * h;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
                //var lengthX = (float)(fMaxXLength * 2);
                //var lengthY = lengthX * height / width;
                //Camera.MakeOrtho(lengthX, lengthY, width, height);

                //var deltaSize = aabb.Maximum - aabb.Minimum;
                //length = System.Math.Max(System.Math.Max(deltaSize.X, deltaSize.Y), deltaSize.Z) + 50.0f;
                //FinalCameraPos_FocusOpr = vObjCenter - Camera.CameraData.Direction * length;
            }

            mCameraPosOffset_FocusOpr = (FinalCameraPos_FocusOpr - Camera.CameraData.Position) / mMaxStep_FocusOpr;
            mCameraLookAtOffset_FocusOpr = (vObjCenter - Camera.CameraData.LookAt) / mMaxStep_FocusOpr;
            mNeedCameraFocusOpr = true;
        }
        public void FocusShow(EngineNS.GamePlay.Actor.GActor actor)
        {
            if (World == null)
                return;
            EngineNS.BoundingBox aabb = new EngineNS.BoundingBox();
            aabb.InitEmptyBox();
            EngineNS.BoundingBox actorAABB = new EngineNS.BoundingBox();
            actor.GetAABB(ref actorAABB);
            aabb.Merge2(ref actorAABB, ref aabb);

            List<Vector3> VecList = new List<Vector3>();

            if (aabb.Maximum.X > 10000)
                aabb.Maximum.X = 10000;
            if (aabb.Maximum.Y > 10000)
                aabb.Maximum.Y = 10000;
            if (aabb.Maximum.Z > 10000)
                aabb.Maximum.Z = 10000;
            if (aabb.Minimum.X < -10000)
                aabb.Minimum.X = -10000;
            if (aabb.Minimum.Y < -10000)
                aabb.Minimum.Y = -10000;
            if (aabb.Minimum.Z < -10000)
                aabb.Minimum.Z = -10000;

            VecList.Add(aabb.Minimum);
            VecList.Add(new Vector3(aabb.Maximum.X, aabb.Minimum.Y, aabb.Minimum.Z));
            VecList.Add(new Vector3(aabb.Maximum.X, aabb.Maximum.Y, aabb.Minimum.Z));
            VecList.Add(new Vector3(aabb.Minimum.X, aabb.Maximum.Y, aabb.Minimum.Z));
            VecList.Add(new Vector3(aabb.Minimum.X, aabb.Maximum.Y, aabb.Maximum.Z));
            VecList.Add(new Vector3(aabb.Minimum.X, aabb.Minimum.Y, aabb.Maximum.Z));
            VecList.Add(new Vector3(aabb.Maximum.X, aabb.Minimum.Y, aabb.Maximum.Z));
            VecList.Add(aabb.Maximum);

            var vObjCenter = (aabb.Maximum - aabb.Minimum) * 0.5f + aabb.Minimum;

            float radius = Vector3.Distance(ref aabb.Minimum, ref aabb.Maximum) / 2;

            Vector3 FinalCameraPos_FocusOpr = new Vector3();
            if (Camera.IsPerspective)
            {
                float fovAngle = 0;
                if (Camera.Aspect > 1)
                {
                    fovAngle = Camera.FoV * 0.5f;
                }
                else
                {
                    fovAngle = (float)(Math.Atan(Math.Tan(Camera.FoV * 0.5) * Camera.Aspect));
                }
                float h = radius / (float)Math.Sin(fovAngle);
                FinalCameraPos_FocusOpr = vObjCenter - Camera.CameraData.Direction * h;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
                //var lengthX = (float)(fMaxXLength * 2);
                //var lengthY = lengthX * height / width;
                //Camera.MakeOrtho(lengthX, lengthY, width, height);

                //var deltaSize = aabb.Maximum - aabb.Minimum;
                //length = System.Math.Max(System.Math.Max(deltaSize.X, deltaSize.Y), deltaSize.Z) + 50.0f;
                //FinalCameraPos_FocusOpr = vObjCenter - Camera.CameraData.Direction * length;
            }

            mCameraPosOffset_FocusOpr = (FinalCameraPos_FocusOpr - Camera.CameraData.Position) / mMaxStep_FocusOpr;
            mCameraLookAtOffset_FocusOpr = (vObjCenter - Camera.CameraData.LookAt) / mMaxStep_FocusOpr;
            mNeedCameraFocusOpr = true;
        }
        public void FocusShow(List<EngineNS.GamePlay.Actor.GActor> actors)
        {
            if (World == null)
                return;
            if (actors.Count == 0)
                return;
            EngineNS.BoundingBox aabb = new EngineNS.BoundingBox();
            aabb.InitEmptyBox();
            if (actors.Count > 0)
            {
                foreach (var actor in actors)
                {
                    EngineNS.BoundingBox actorAABB = new EngineNS.BoundingBox();
                    actor.GetAABB(ref actorAABB);

                    aabb.Merge2(ref actorAABB, ref aabb);
                }
            }
            List<Vector3> VecList = new List<Vector3>();
            if (aabb.Maximum.X > 10000)
                aabb.Maximum.X = 10000;
            if (aabb.Maximum.Y > 10000)
                aabb.Maximum.Y = 10000;
            if (aabb.Maximum.Z > 10000)
                aabb.Maximum.Z = 10000;
            if (aabb.Minimum.X < -10000)
                aabb.Minimum.X = -10000;
            if (aabb.Minimum.Y < -10000)
                aabb.Minimum.Y = -10000;
            if (aabb.Minimum.Z < -10000)
                aabb.Minimum.Z = -10000;

            VecList.Add(aabb.Minimum);
            VecList.Add(new Vector3(aabb.Maximum.X, aabb.Minimum.Y, aabb.Minimum.Z));
            VecList.Add(new Vector3(aabb.Maximum.X, aabb.Maximum.Y, aabb.Minimum.Z));
            VecList.Add(new Vector3(aabb.Minimum.X, aabb.Maximum.Y, aabb.Minimum.Z));
            VecList.Add(new Vector3(aabb.Minimum.X, aabb.Maximum.Y, aabb.Maximum.Z));
            VecList.Add(new Vector3(aabb.Minimum.X, aabb.Minimum.Y, aabb.Maximum.Z));
            VecList.Add(new Vector3(aabb.Maximum.X, aabb.Minimum.Y, aabb.Maximum.Z));
            VecList.Add(aabb.Maximum);

            var vObjCenter = (aabb.Maximum - aabb.Minimum) * 0.5f + aabb.Minimum;

            float radius = Vector3.Distance(ref aabb.Minimum, ref aabb.Maximum) / 2;

            Vector3 FinalCameraPos_FocusOpr = new Vector3();
            if (Camera.IsPerspective)
            {
                float fovAngle = 0;
                if (Camera.Aspect > 1)
                {
                    fovAngle = Camera.FoV * 0.5f;
                }
                else
                {
                    fovAngle = (float)(Math.Atan(Math.Tan(Camera.FoV * 0.5) * Camera.Aspect));
                }
                float h = radius / (float)Math.Sin(fovAngle);
                FinalCameraPos_FocusOpr = vObjCenter - Camera.CameraData.Direction * h;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
                //var lengthX = (float)(fMaxXLength * 2);
                //var lengthY = lengthX * height / width;
                //Camera.MakeOrtho(lengthX, lengthY, width, height);

                //var deltaSize = aabb.Maximum - aabb.Minimum;
                //length = System.Math.Max(System.Math.Max(deltaSize.X, deltaSize.Y), deltaSize.Z) + 50.0f;
                //FinalCameraPos_FocusOpr = vObjCenter - Camera.CameraData.Direction * length;
            }

            mCameraPosOffset_FocusOpr = (FinalCameraPos_FocusOpr - Camera.CameraData.Position) / mMaxStep_FocusOpr;
            mCameraLookAtOffset_FocusOpr = (vObjCenter - Camera.CameraData.LookAt) / mMaxStep_FocusOpr;
            mNeedCameraFocusOpr = true;
        }
        public Action FocusFunc = null;
        protected void Button_Focus_Click(object sender, RoutedEventArgs e)
        {
            ProcessFocus();
        }

        public delegate void AddNavActorEventDelegate(EngineNS.GamePlay.Actor.GActor actor);
        public event AddNavActorEventDelegate AddNavActorEvent;
        public void AddActor(EngineNS.GamePlay.Actor.GActor actor)
        {
            //var component = actor.GetComponent<EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent>();
            if (actor.IsNavgation)
            {
                //行走区域的actor单独加到一个actor
                if (World.DefaultScene.NavAreaActor == null)
                {
                    World.DefaultScene.CreateNavActor();
                    World.AddActor(World.DefaultScene.NavAreaActor);
                    World.DefaultScene.AddActor(World.DefaultScene.NavAreaActor);
                    DAddActor?.Invoke(World.DefaultScene.NavAreaActor);
                }

                //World.DefaultScene.NavAreaActor.Children.Add(actor);
                actor.SetParent(World.DefaultScene.NavAreaActor);
                AddNavActorEvent?.Invoke(actor);
                DRefreshActors?.Invoke();
            }

            World.AddActor(actor);
            World.DefaultScene.AddActor(actor);
            DAddActor?.Invoke(actor);

            SelectActor(actor);

            EngineNS.CEngine.Instance.HitProxyManager.MapActor(actor);

        }
        public void _DRemoveActor(EngineNS.GamePlay.Actor.GActor acotr)
        {
            DRemoveActor?.Invoke(acotr);
        }
        public void _DAddActor(EngineNS.GamePlay.Actor.GActor acotr)
        {
            DAddActor?.Invoke(acotr);
        }
        public void RemoveActor(EngineNS.GamePlay.Actor.GActor actor)
        {
            //if (!actor.IsNavgation)
            //{
            World.RemoveActor(actor.ActorId);
            World.DefaultScene.RemoveActor(actor.ActorId);
            //}

            actor.SetParent(null);

            DRemoveActor?.Invoke(actor);
        }
    }
}
