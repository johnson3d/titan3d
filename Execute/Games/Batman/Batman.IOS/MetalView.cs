using System;
using System.Drawing;

using CoreAnimation;
using CoreGraphics;
using Foundation;
using Metal;
using ObjCRuntime;
using UIKit;

namespace Batman.IOS
{
    [Register("MetalView")]
    public class MetalView : UIView
    {
        [Export("layerClass")]
        public static Class LayerClass()
        {
            return new Class(typeof(CAMetalLayer));
        }
        public MetalView(IntPtr handle) : base(handle)
        {
            //Initialize();
            BackgroundColor = UIColor.Yellow;
            Opaque = true;
            //BackgroundColor = UIColor.Clear;

            mMetalLayer = (CAMetalLayer)Layer;
            //ICAMetalDrawable Drawable = mMetalLayer.NextDrawable();
            //mMetalLayer.Frame
            mMetalLayer.PixelFormat = MTLPixelFormat.BGRA8Unorm;
            mMetalLayer.FramebufferOnly = true;
            //mWidth = (UInt32)(float)Bounds.Size.Width;// * (float)UIScreen.MainScreen.Scale);
            //mHeight = (UInt32)(float)Bounds.Size.Height;// * (float)UIScreen.MainScreen.Scale);
            //1334×750
            ContentScaleFactor = UIScreen.MainScreen.Scale;
            mWidth = 1334;
            mHeight = 750;
        }
        public MetalView()
        {
            Initialize();
        }

        public MetalView(RectangleF bounds) : base(bounds)
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = UIColor.Red;
        }

        public CAMetalLayer mMetalLayer = null;
        public UInt32 mWidth;
        public UInt32 mHeight;
    }
}