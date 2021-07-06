using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class ImageProxy : IUIProxyBase, IDisposable
    {
        RName mImageFile;
        public RName ImageFile
        {
            get => mImageFile;
            set
            {
                if(mImageFile != value)
                    Dispose();
                mImageFile = value;
            }
        }
        public Vector2 ImageSize = new Vector2(32, 32);
        public Vector2 UVMin = new Vector2(0, 0);
        public Vector2 UVMax = new Vector2(1, 1);
        public UInt32 Color = 0xFFFFFFFF;
        public bool IntersectWithCurrentClipRect = true;

        System.Threading.Tasks.Task<RHI.CShaderResourceView> mTask;
        IntPtr mImagePtr;
        public unsafe IntPtr GetImagePtrPointer()
        {
            if (mImagePtr == IntPtr.Zero)
            {
                if (mTask == null)
                {
                    var rc = UEngine.Instance.GfxDevice.RenderContext;
                    mTask = UEngine.Instance.GfxDevice.TextureManager.GetTexture(ImageFile);
                }
                else if (mTask.IsCompleted)
                {
                    mImagePtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(mTask.Result));
                    mTask = null;
                }
            }

            return mImagePtr;
        }

        public ImageProxy()
        {

        }
        public ImageProxy(RName imageFile)
        {
            ImageFile = imageFile;
        }
        ~ImageProxy()
        {
            Dispose();
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            // 图片加载不放到初始化里是为了不绘制就不加载
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        public void Cleanup()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (mImagePtr != IntPtr.Zero)
            {
                System.Runtime.InteropServices.GCHandle.FromIntPtr(mImagePtr).Free();
                mImagePtr = IntPtr.Zero;
            }
        }
        public unsafe bool OnDraw(ref ImDrawList drawList, ref Support.UAnyPointer drawData)
        {
            if (ImageFile == null)
                return false;

            if (mImagePtr == IntPtr.Zero)
            {
                if(mTask == null)
                {
                    var rc = UEngine.Instance.GfxDevice.RenderContext;
                    mTask = UEngine.Instance.GfxDevice.TextureManager.GetTexture(ImageFile);
                }
                else if(mTask.IsCompleted)
                {
                    mImagePtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(mTask.Result));
                    mTask = null;
                }
            }

            var startPos = ImGuiAPI.GetCursorScreenPos();
            var endPos = startPos + ImageSize;
            ImGuiAPI.PushClipRect(ref startPos, ref endPos, IntersectWithCurrentClipRect);
            if(mImagePtr != IntPtr.Zero)
                drawList.AddImage(mImagePtr.ToPointer(), ref startPos, ref endPos, ref UVMin, ref UVMax, Color);
            ImGuiAPI.PopClipRect();
            return true;
        }
        public unsafe void OnDraw(ref ImDrawList drawList, ref Vector2 pos)
        {
            if (ImageFile == null)
                return;

            if (mImagePtr == IntPtr.Zero)
            {
                if (mTask == null)
                {
                    var rc = UEngine.Instance.GfxDevice.RenderContext;
                    mTask = UEngine.Instance.GfxDevice.TextureManager.GetTexture(ImageFile);
                }
                else if (mTask.IsCompleted)
                {
                    mImagePtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(mTask.Result));
                    mTask = null;
                }
            }

            var endPos = pos + ImageSize;
            ImGuiAPI.PushClipRect(ref pos, ref endPos, IntersectWithCurrentClipRect);
            if (mImagePtr != IntPtr.Zero)
                drawList.AddImage(mImagePtr.ToPointer(), ref pos, ref endPos, ref UVMin, ref UVMax, Color);
            ImGuiAPI.PopClipRect();
        }
    }
}
