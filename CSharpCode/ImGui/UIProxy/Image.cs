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
        public unsafe IntPtr GetImagePtrPointer()
        {
            if (mTask == null)
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                mTask = UEngine.Instance.GfxDevice.TextureManager.GetTexture(ImageFile);
                return IntPtr.Zero;
            }
            else if (mTask.IsCompleted == false)
            {
                return IntPtr.Zero;
            }
            else
            {
                return mTask.Result.GetTextureHandle();
            }
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
            if (mTask != null)
            {
                mTask.Result?.FreeTextureHandle();
                mTask = null;
            }
        }
        public unsafe bool OnDraw(in ImDrawList drawList, in Support.UAnyPointer drawData)
        {
            if (ImageFile == null)
                return false;

            var startPos = ImGuiAPI.GetCursorScreenPos();
            var endPos = startPos + ImageSize;
            ImGuiAPI.PushClipRect(in startPos, in endPos, IntersectWithCurrentClipRect);
            if(GetImagePtrPointer() != IntPtr.Zero)
                drawList.AddImage(GetImagePtrPointer().ToPointer(), in startPos, in endPos, in UVMin, in UVMax, Color);
            ImGuiAPI.PopClipRect();
            return true;
        }
        public unsafe void OnDraw(in ImDrawList drawList, in Vector2 pos)
        {
            if (ImageFile == null)
                return;

            var endPos = pos + ImageSize;
            ImGuiAPI.PushClipRect(in pos, in endPos, IntersectWithCurrentClipRect);
            if (GetImagePtrPointer() != IntPtr.Zero)
                drawList.AddImage(GetImagePtrPointer().ToPointer(), in pos, in endPos, in UVMin, in UVMax, Color);
            ImGuiAPI.PopClipRect();
        }
    }
}
