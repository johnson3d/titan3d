using EngineNS.Support;
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
        public Vector2 ImageSize = Vector2.Zero;// new Vector2(32, 32);
        public Vector2 UVMin = new Vector2(0, 0);
        public Vector2 UVMax = new Vector2(1, 1);
        public UInt32 Color = 0xFFFFFFFF;
        public bool IntersectWithCurrentClipRect = true;

        protected System.Threading.Tasks.Task<RHI.CShaderResourceView> mTask;
        public unsafe virtual IntPtr GetImagePtrPointer()
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
                if(ImageSize == Vector2.Zero)
                    ImageSize = new Vector2(mTask.Result.PicDesc.Width, mTask.Result.PicDesc.Height);
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
        public unsafe virtual bool OnDraw(in ImDrawList drawList, in Support.UAnyPointer drawData)
        {
            var startPos = ImGuiAPI.GetCursorScreenPos();
            var endPos = startPos + ImageSize;
            return OnDraw(drawList, startPos, endPos, Color);
        }
        public unsafe virtual bool OnDraw(in ImDrawList drawList, in Vector2 pos)
        {
            var endPos = pos + ImageSize;
            return OnDraw(drawList, pos, endPos, Color);
        }
        public unsafe virtual bool OnDraw(in ImDrawList drawList, in Vector2 start, in Vector2 end)
        {
            return OnDraw(drawList, start, end, Color);
        }
        public unsafe virtual bool OnDraw(in ImDrawList drawList, in Vector2 start, in Vector2 end, uint color)
        {
            if (ImageFile == null)
                return false;

            ImGuiAPI.PushClipRect(in start, in end, IntersectWithCurrentClipRect);
            if (GetImagePtrPointer() != IntPtr.Zero)
                drawList.AddImage(GetImagePtrPointer().ToPointer(), in start, in end, in UVMin, in UVMax, color);
            ImGuiAPI.PopClipRect();
            return true;
        }
    }

    public class BoxImageProxy : ImageProxy
    {
        Thickness mUVMargin;
        Thickness mSizeMargin = Thickness.Empty;

        public BoxImageProxy(in RName imageFile, in Thickness uvMargin)
        {
            ImageFile = imageFile;
            mUVMargin = uvMargin;
        }

        public override IntPtr GetImagePtrPointer()
        {
            var ptr = base.GetImagePtrPointer();
            if(ptr != IntPtr.Zero && mSizeMargin == Thickness.Empty)
            {
                var textureWidth = mTask.Result.PicDesc.Width;
                var textureHeight = mTask.Result.PicDesc.Height;
                var uvSize = UVMax - UVMin;
                mSizeMargin.Left = (mUVMargin.Left * uvSize.X) * textureWidth;
                mSizeMargin.Right = (mUVMargin.Right * uvSize.X) * textureWidth;
                mSizeMargin.Top = (mUVMargin.Top * uvSize.Y) * textureHeight;
                mSizeMargin.Bottom = (mUVMargin.Bottom * uvSize.Y) * textureHeight;
            }
            return ptr;
        }
        unsafe void DrawBoxImage(in ImDrawList drawList, in Vector2 startPos, in Vector2 endPos)
        {
        }
        public unsafe override bool OnDraw(in ImDrawList drawList, in UAnyPointer drawData)
        {
            var startPos = ImGuiAPI.GetCursorScreenPos();
            var endPos = startPos + ImageSize;
            return OnDraw(drawList, startPos, endPos);
        }
        public override bool OnDraw(in ImDrawList drawList, in Vector2 pos)
        {
            var end = pos + ImageSize;
            return OnDraw(drawList, pos, end);
        }
        public unsafe override bool OnDraw(in ImDrawList drawList, in Vector2 startPos, in Vector2 endPos)
        {
            return OnDraw(drawList, startPos, endPos, Color);
        }
        public unsafe override bool OnDraw(in ImDrawList drawList, in Vector2 startPos, in Vector2 endPos, uint color)
        {
            if (ImageFile == null)
                return false;

            ImGuiAPI.PushClipRect(in startPos, in endPos, IntersectWithCurrentClipRect);
            var imgPtr = GetImagePtrPointer();
            if (imgPtr != IntPtr.Zero)
            {
                var uvSize = UVMax - UVMin;
                var realLeft = mUVMargin.Left * uvSize.X;
                var realRight = mUVMargin.Right * uvSize.X;
                var realTop = mUVMargin.Top * uvSize.Y;
                var realBottom = mUVMargin.Bottom * uvSize.Y;
                var tempStart = startPos;
                var tempEnd = startPos + new Vector2(mSizeMargin.Left, mSizeMargin.Top);
                drawList.AddImage(imgPtr.ToPointer(), tempStart, tempEnd, new Vector2(UVMin.X, UVMin.Y), new Vector2(realLeft + UVMin.X, realTop + UVMin.Y), color);
                tempStart = new Vector2(startPos.X + mSizeMargin.Left, startPos.Y);
                tempEnd = new Vector2(endPos.X - mSizeMargin.Right, startPos.Y + mSizeMargin.Top);
                drawList.AddImage(imgPtr.ToPointer(), tempStart, tempEnd, new Vector2(realLeft + UVMin.X, UVMin.Y), new Vector2(UVMax.X - realRight, realTop + UVMin.Y), color);
                tempStart = new Vector2(endPos.X - mSizeMargin.Right, startPos.Y);
                tempEnd = new Vector2(endPos.X, startPos.Y + mSizeMargin.Top);
                drawList.AddImage(imgPtr.ToPointer(), tempStart, tempEnd, new Vector2(UVMax.X - realRight, UVMin.Y), new Vector2(UVMax.X, realTop + UVMin.Y), color);
                tempStart = new Vector2(startPos.X, startPos.Y + mSizeMargin.Top);
                tempEnd = new Vector2(startPos.X + mSizeMargin.Left, endPos.Y - mSizeMargin.Bottom);
                drawList.AddImage(imgPtr.ToPointer(), tempStart, tempEnd, new Vector2(UVMin.X, realTop + UVMin.Y), new Vector2(realLeft + UVMin.X, UVMax.Y - realBottom), color);
                tempStart = new Vector2(startPos.X + mSizeMargin.Left, startPos.Y + mSizeMargin.Top);
                tempEnd = new Vector2(endPos.X - mSizeMargin.Right, endPos.Y - mSizeMargin.Bottom);
                drawList.AddImage(imgPtr.ToPointer(), tempStart, tempEnd, new Vector2(realLeft + UVMin.X, realTop + UVMin.Y), new Vector2(UVMax.X - realRight, UVMax.Y - realBottom), color);
                tempStart = new Vector2(endPos.X - mSizeMargin.Right, startPos.Y + mSizeMargin.Top);
                tempEnd = new Vector2(endPos.X, endPos.Y - mSizeMargin.Bottom);
                drawList.AddImage(imgPtr.ToPointer(), tempStart, tempEnd, new Vector2(UVMax.X - realRight, realTop + UVMin.Y), new Vector2(UVMax.X, UVMax.Y - realBottom), color);
                tempStart = new Vector2(startPos.X, endPos.Y - mSizeMargin.Bottom);
                tempEnd = new Vector2(startPos.X + mSizeMargin.Left, endPos.Y);
                drawList.AddImage(imgPtr.ToPointer(), tempStart, tempEnd, new Vector2(UVMin.X, UVMax.Y - realBottom), new Vector2(realLeft + UVMin.X, UVMax.Y), color);
                tempStart = new Vector2(startPos.X + mSizeMargin.Left, endPos.Y - mSizeMargin.Bottom);
                tempEnd = new Vector2(endPos.X - mSizeMargin.Right, endPos.Y);
                drawList.AddImage(imgPtr.ToPointer(), tempStart, tempEnd, new Vector2(realLeft + UVMin.X, UVMax.Y - realBottom), new Vector2(UVMax.X - realRight, UVMax.Y), color);
                tempStart = new Vector2(endPos.X - mSizeMargin.Right, endPos.Y - mSizeMargin.Bottom);
                tempEnd = new Vector2(endPos.X, endPos.Y);
                drawList.AddImage(imgPtr.ToPointer(), tempStart, tempEnd, new Vector2(UVMax.X - realRight, UVMax.Y - realBottom), new Vector2(UVMax.X, UVMax.Y), color);
            }
            ImGuiAPI.PopClipRect();
            return true;
        }
    }
}
