using Foundation;
using System;
using UIKit;
using CoreAnimation;

namespace Batman.IOS
{
    [Register("ViewController")]
    public  class ViewController : UIViewController
    {
        CADisplayLink mDisplayLink = null;

        public ViewController (IntPtr handle) : base (handle)
        {
        }

        public void DispatchGameLoop()
        {
            mDisplayLink = CADisplayLink.Create(Game.CGameEngine.GameEngine.MainTick);
            //mDisplayLink.FrameInterval = 1;
            mDisplayLink.PreferredFramesPerSecond = 30;
            mDisplayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoop.NSDefaultRunLoopMode);
        }


        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            // Perform any additional setup after loading the view, typically from a nib.

            //string DirBin = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");
            //string DirRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/";
            //string DirPerson = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

        public override void DidReceiveMemoryWarning ()
        {
            base.DidReceiveMemoryWarning ();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}