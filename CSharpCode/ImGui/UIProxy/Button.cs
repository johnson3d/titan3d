using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class ImageButtonProxy : IUIProxyBase
    {
        public RName ImageFile
        {
            get => mImage.ImageFile;
            set => mImage.ImageFile = value;
        }
        Vector2 mSize = new Vector2(32, 32);
        public Vector2 Size
        {
            get => mSize;
            set
            {
                mSize = value;
                mImage.ImageSize = mSize - ImagePadding * 2;
            }
        }
        public Vector2 UVMin
        {
            get => mImage.UVMin;
            set => mImage.UVMin = value;
        }
        public Vector2 UVMax
        {
            get => mImage.UVMax;
            set => mImage.UVMax = value;
        }
        public Vector2 ImagePadding = new Vector2(2);
        public Action Action;

        System.Threading.Tasks.Task<RHI.CShaderResourceView> mTask;
        IntPtr mImagePtr;

        ImageProxy mImage = new ImageProxy();

        ~ImageButtonProxy()
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

        public unsafe void OnDraw(ref ImDrawList drawList)
        {
            var rectStart = ImGuiAPI.GetCursorScreenPos();
            var rectEnd = rectStart + mSize;
            var imgPos = rectStart + ImagePadding;
            mImage.OnDraw(ref drawList, ref imgPos);

            //if (mImagePtr == IntPtr.Zero)
            //{
            //    if (mTask == null)
            //    {
            //        var rc = UEngine.Instance.GfxDevice.RenderContext;
            //        mTask = UEngine.Instance.GfxDevice.TextureManager.GetTexture(ImageFile);
            //    }
            //    else if (mTask.IsCompleted)
            //    {
            //        mImagePtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(mTask.Result));
            //        mTask = null;
            //    }
            //}

            //Vector4 bg = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            //Vector4 tint = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            //var size = Size;
            //var uvMin = UVMin;
            //var uvMax = UVMax;
            //ImGuiAPI.ImageButton(mImagePtr.ToPointer(), ref mImage.ImageSize, ref mImage.UVMin, ref mImage.UVMax, 2, ref bg, ref tint);

            if (ImGuiAPI.IsMouseHoveringRect(ref rectStart, ref rectEnd, true))
            {
                mImage.Color = 0xFFFFFFFF;
                if(ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    Action?.Invoke();
                }
            }
            else
            {
                mImage.Color = 0xFFBFBFBF;
            }

            ImGuiAPI.ItemSize(ref mSize, 0);
            ImGuiAPI.ItemAdd(ref rectStart, ref rectEnd, ImGuiAPI.GetID("#Button"), 0);
        }
    }
}
