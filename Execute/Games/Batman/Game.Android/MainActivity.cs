using Android.App;
using Android.Widget;
using Android.OS;
using System.Runtime.InteropServices;
using Android.Graphics;
using Android.Views;

namespace Game.Droid
{
    [Activity(Label = "Titan3D", MainLauncher = true, Icon = "@drawable/Titan", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class MainActivity : Activity, Android.OS.MessageQueue.IIdleHandler, Android.Views.ISurfaceHolderCallback, GestureDetector.IOnGestureListener
    {
        public class MainHandler : Android.OS.Handler
        {
            public override void HandleMessage(Message msg)
            {
                base.HandleMessage(msg);
            }
        }
        MainHandler mHandler;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            EngineNS.CIPlatform.Instance.InitAndroid(this.Assets);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            mHandler = new MainHandler();

            mGameView = FindViewById<Android.Views.SurfaceView>(Resource.Id.surfaceView);
            mGameView.Holder.AddCallback(this);
        }
        Android.Views.SurfaceView mGameView;
        public override void OnWindowFocusChanged(bool hasFocus)
        {
            if (hasFocus == false)
                return;

            if (EngineNS.CEngine.Instance == null)
            {
                EngineNS.CEngine.Instance = new CGameEngine();
                EngineNS.CEngine.Instance.InitEngine("Game", null);
                EngineNS.CEngine.Instance.Interval = 20;
                var window = EngineNS.CIPlatform.Instance.WindowFromSurface(mSurfaceHolder.Surface.Handle);
                
                Looper.MyQueue().AddIdleHandler(this);

                System.Action action = async () =>
                {
                    await CGameEngine.GameEngine.InitSystem(window, (System.UInt32)mSurfaceHolder.SurfaceFrame.Width(), (System.UInt32)mSurfaceHolder.SurfaceFrame.Height(), EngineNS.ERHIType.RHT_OGL);
                    //EngineNS.CEngine.Instance.MetaClassManager.CheckNewMetaClass();
                    
                    var desc = new EngineNS.GamePlay.GGameInstanceDesc();
                    desc.SceneName = desc.DefaultMapName;
                    await CGameEngine.GameEngine.OnEngineInited();

                    await CGameEngine.GameEngine.StartGame(typeof(Game.CGameInstance), window,
                        (System.UInt32)mSurfaceHolder.SurfaceFrame.Width(), (System.UInt32)mSurfaceHolder.SurfaceFrame.Height(), desc, null,
                        EngineNS.RName.GetRName("Macross/mygame.macross"));
                };
                action();
                
                //EngineNS.CEngine.Instance.GameInstance.Gesture = new Android.Views.GestureDetector(this);
            }
            else
            {
                var window = EngineNS.CIPlatform.Instance.WindowFromSurface(mSurfaceHolder.Surface.Handle);
                CGameEngine.GameEngine.OnResume(window);
                Looper.MyQueue().AddIdleHandler(this);
            }
        }
        public bool QueueIdle()
        {
            CGameEngine.GameEngine.MainTick();
            mHandler.SendEmptyMessage(0);
            return true;
        }
        protected override void OnPause()
        {
            if (CGameEngine.GameEngine != null)
            {
                Looper.MyQueue().RemoveIdleHandler(this);
                CGameEngine.GameEngine.OnPause();
            }
            base.OnPause();
        }
        protected override void OnResume()
        {
            base.OnResume();
        }

        ISurfaceHolder mSurfaceHolder;
        void ISurfaceHolderCallback.SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
            
        }

        void ISurfaceHolderCallback.SurfaceCreated(ISurfaceHolder holder)
        {
            mSurfaceHolder = holder;
        }

        void ISurfaceHolderCallback.SurfaceDestroyed(ISurfaceHolder holder)
        {
            
        }

        #region Touch

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (EngineNS.CEngine.Instance.GameInstance != null)
            {
                //var gesture = EngineNS.CEngine.Instance.GameInstance.Gesture;
                //gesture.OnTouchEvent(e);
                
                unsafe
                {
                    var ae = new EngineNS.Input.ActionEvent();
                    ae.ViewWidth = this.mGameView.Width;
                    ae.ViewHeight = this.mGameView.Height;
                    ae.X = e.RawX;
                    ae.Y = e.RawY;
                    ae.PointerNumber = e.PointerCount;
                    ae.Flags = 0;
                    if (ae.PointerNumber > 10)
                        ae.PointerNumber = 10;
                    for (int i = 0; i < ae.PointerNumber; i++)
                    {
                        ae.PointX[i] = e.GetX(i);
                        ae.PointY[i] = e.GetY(i);
                    }
                    switch (e.Action)
                    {
                        case MotionEventActions.Down:
                            ae.MotionType = EngineNS.Input.EMotionType.Down;
                            break;
                        case MotionEventActions.Up:
                            ae.MotionType = EngineNS.Input.EMotionType.Up;
                            break;
                        case MotionEventActions.Move:
                            ae.MotionType = EngineNS.Input.EMotionType.Move;
                            break;
                    }

                    EngineNS.CEngine.Instance.GameInstance.OnActionEvent(ref ae);
                }
            }
            return base.OnTouchEvent(e);
        }

        bool GestureDetector.IOnGestureListener.OnDown(MotionEvent e)
        {
            throw new System.NotImplementedException();
        }

        bool GestureDetector.IOnGestureListener.OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            throw new System.NotImplementedException();
        }

        void GestureDetector.IOnGestureListener.OnLongPress(MotionEvent e)
        {
            throw new System.NotImplementedException();
        }

        bool GestureDetector.IOnGestureListener.OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            throw new System.NotImplementedException();
        }

        void GestureDetector.IOnGestureListener.OnShowPress(MotionEvent e)
        {
            throw new System.NotImplementedException();
        }

        bool GestureDetector.IOnGestureListener.OnSingleTapUp(MotionEvent e)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}

