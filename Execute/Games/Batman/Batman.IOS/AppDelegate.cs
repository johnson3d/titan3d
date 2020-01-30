using Foundation;
using UIKit;
using System;

namespace Batman.IOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

            //string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            //string docFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            ViewController view_controller = (ViewController)Window.RootViewController;
            

            if (view_controller == null)
            {
                Console.WriteLine("ERROR: Failed creating a view controller!");
                return false;
            }

            MetalView mtl_view = (MetalView)view_controller.View;
            //MetalView mtl_view = new MetalView(view_controller.View.ClassHandle);
            
            if (mtl_view == null)
            {
                Console.WriteLine("ERROR: Failed creating a renderer view!");
                return false;
            }
            EngineNS.CIPlatform.Instance.InitAppleOS();
            if (EngineNS.CIPlatform.Instance.PlayMode== EngineNS.CIPlatform.enPlayMode.Game)
            {

            }
            if (EngineNS.CEngine.Instance == null)
            {
                var assm = EngineNS.Rtti.VAssemblyManager.Instance.GetAssembly(this.GetType().Assembly, EngineNS.ECSType.Client, "Game");
                EngineNS.Rtti.RttiHelper.RegisterAnalyseAssembly("Game", assm);
                System.Diagnostics.Trace.WriteLine($"Game.IOS Found!!!!");
             
                EngineNS.CEngine.Instance = new Game.CGameEngine();
                EngineNS.CEngine.Instance.InitEngine("Game", null);
                EngineNS.CEngine.Instance.Interval = 30;
                var WindowHandle = mtl_view.mMetalLayer.Handle;
                
                System.Action action = async () =>
                {
                    await Game.CGameEngine.GameEngine.InitSystem(WindowHandle, mtl_view.mWidth, mtl_view.mHeight, EngineNS.ERHIType.RHIType_Metal);

                    //EngineNS.CEngine.Instance.MetaClassManager.CheckNewMetaClass();

                    var desc = new EngineNS.GamePlay.GGameInstanceDesc();
                    desc.SceneName = desc.DefaultMapName;
                    await Game.CGameEngine.GameEngine.OnEngineInited();

                    await Game.CGameEngine.GameEngine.StartGame(typeof(Game.CGameInstance), WindowHandle, mtl_view.mWidth, mtl_view.mHeight, desc, null, 
                        EngineNS.RName.GetRName("Macross/mygame.macross"));
                };
                action();

                //EngineNS.CEngine.Instance.GameInstance.Gesture = new Android.Views.GestureDetector(this);
            }
            
            // run the game loop
            view_controller.DispatchGameLoop();
            return true;

            //return true;
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message)
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive.
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}

