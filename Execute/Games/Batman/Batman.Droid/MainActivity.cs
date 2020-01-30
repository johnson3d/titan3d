using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Content;

namespace Batman.Droid
{
    [Activity(Label = "BatMan", MainLauncher = true, Icon = "@drawable/Titan", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
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
        Android.Views.SurfaceView mGameView;
        PowerManager.WakeLock mWakeLock = null;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            EngineNS.CIPlatform.Instance.InitAndroid(this, this.Assets);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            mHandler = new MainHandler();
            
            mGameView = FindViewById<Android.Views.SurfaceView>(Resource.Id.surfaceView);
            mGameView.Holder.AddCallback(this);
            //var pm = (PowerManager)this.GetSystemService(Context.PowerService);
            //mWakeLock = pm.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.OnAfterRelease, "BatMan");
            //mWakeLock.Acquire();
        }
        public override void OnWindowFocusChanged(bool hasFocus)
        {
            if (hasFocus == false)
                return;

            if (EngineNS.CEngine.Instance == null)
            {
                var assms = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var i in assms)
                {
                    System.Diagnostics.Trace.WriteLine(i.FullName);
                    if (i.FullName.Contains("Game.Android"))
                    {
                        var assm = EngineNS.Rtti.VAssemblyManager.Instance.GetAssembly(i, EngineNS.ECSType.Client, "Game");
                        EngineNS.Rtti.RttiHelper.RegisterAnalyseAssembly("Game", assm);
                        System.Diagnostics.Trace.WriteLine($"Game.Android Found!!!!");
                        break;
                    }
                }

                EngineNS.CEngine.Instance = new Game.CGameEngine();
                EngineNS.CEngine.Instance.PreInitEngine();
                EngineNS.CEngine.Instance.InitEngine("Game", null);
                EngineNS.CEngine.Instance.Interval = 30;
                var window = EngineNS.CIPlatform.Instance.WindowFromSurface(mSurfaceHolder.Surface.Handle);

                Looper.MyQueue().AddIdleHandler(this);

                System.Action action = async () =>
                {
                    await Game.CGameEngine.GameEngine.InitSystem(window, (System.UInt32)mSurfaceHolder.SurfaceFrame.Width(), (System.UInt32)mSurfaceHolder.SurfaceFrame.Height(), 
                        EngineNS.ERHIType.RHT_OGL, false);

                    //EngineNS.CEngine.Instance.MetaClassManager.CheckNewMetaClass();

                    var desc = new EngineNS.GamePlay.GGameInstanceDesc();
                    desc.SceneName = desc.DefaultMapName;
                    await Game.CGameEngine.GameEngine.OnEngineInited();

                    EngineNS.CEngine.Instance.McEngineGetter?.Get(false)?.OnStartGame(EngineNS.CEngine.Instance);
                    await Game.CGameEngine.GameEngine.StartGame(typeof(Game.CGameInstance), window,
                        (System.UInt32)mSurfaceHolder.SurfaceFrame.Width(), (System.UInt32)mSurfaceHolder.SurfaceFrame.Height(), desc, null,
                        Game.CGameEngine.GameEngine.Desc.GameMacross);
                };
                action();
                //EngineNS.CEngine.Instance.GameInstance.Gesture = new Android.Views.GestureDetector(this);
            }
            else
            {
                var window = EngineNS.CIPlatform.Instance.WindowFromSurface(mSurfaceHolder.Surface.Handle);
                Game.CGameEngine.GameEngine.OnResume(window);
                Looper.MyQueue().AddIdleHandler(this);
            }
        }
        public bool QueueIdle()
        {
            Game.CGameEngine.GameEngine.MainTick();
            mHandler.SendEmptyMessage(0);
            return true;
        }
        protected override void OnPause()
        {
            if (Game.CGameEngine.GameEngine != null)
            {
                Looper.MyQueue().RemoveIdleHandler(this);
                Game.CGameEngine.GameEngine.OnPause();
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
            mSurfaceHolder = null;
        }

        #region Touch
        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            return base.OnGenericMotionEvent(e);
        }
        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            EngineNS.Input.Device.Keyboard.KeyboardInputEventArgs kea = new EngineNS.Input.Device.Keyboard.KeyboardInputEventArgs();
            kea.DeviceType = EngineNS.Input.Device.DeviceType.Keyboard;
            var keyCodeStr = Enum.GetName(typeof(Android.Views.Keycode), e.KeyCode);
            EngineNS.Input.Device.Keyboard.Keys keyBoardCode;
            if (!Enum.TryParse<EngineNS.Input.Device.Keyboard.Keys>(keyCodeStr, out keyBoardCode))
                return base.OnKeyDown(keyCode, e);
            kea.KeyboardEvent.KeyCode = keyBoardCode;
            EngineNS.Input.Device.Keyboard.KeyboardEventArgs ke = new EngineNS.Input.Device.Keyboard.KeyboardEventArgs();
            ke.KeyState = EngineNS.Input.Device.Keyboard.KeyState.Press;
            ke.Alt = e.IsAltPressed;
            ke.Control = e.IsCtrlPressed;
            ke.Shift = e.IsShiftPressed;
            ke.KeyCode = keyBoardCode;
            ke.KeyValue = (int)keyBoardCode;
            kea.KeyboardEvent = ke;
            EngineNS.CEngine.Instance.InputServerInstance?.OnInputEvnet(kea);
            return base.OnKeyDown(keyCode, e);
        }
        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            EngineNS.Input.Device.Keyboard.KeyboardInputEventArgs kea = new EngineNS.Input.Device.Keyboard.KeyboardInputEventArgs();
            kea.DeviceType = EngineNS.Input.Device.DeviceType.Keyboard;
            var keyCodeStr = Enum.GetName(typeof(Android.Views.Keycode), e.KeyCode);
            EngineNS.Input.Device.Keyboard.Keys keyBoardCode;
            if (!Enum.TryParse<EngineNS.Input.Device.Keyboard.Keys>(keyCodeStr, out keyBoardCode))
                return base.OnKeyUp(keyCode, e);
            kea.KeyboardEvent.KeyCode = keyBoardCode;
            EngineNS.Input.Device.Keyboard.KeyboardEventArgs ke = new EngineNS.Input.Device.Keyboard.KeyboardEventArgs();
            ke.KeyState = EngineNS.Input.Device.Keyboard.KeyState.Release;
            ke.Alt = e.IsAltPressed;
            ke.Control = e.IsCtrlPressed;
            ke.Shift = e.IsShiftPressed;
            ke.KeyCode = keyBoardCode;
            ke.KeyValue = (int)keyBoardCode;
            kea.KeyboardEvent = ke;
            EngineNS.CEngine.Instance.InputServerInstance?.OnInputEvnet(kea);
            return base.OnKeyUp(keyCode, e);
        }

        EngineNS.Touch mTouch = new EngineNS.Touch();
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (EngineNS.CEngine.Instance.GameInstance != null)
            {
                mTouch.ProcessTouchEvent(e);
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

