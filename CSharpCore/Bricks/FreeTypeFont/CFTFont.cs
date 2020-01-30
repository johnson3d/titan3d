using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.Graphics;
using EngineNS.Graphics.Mesh;

namespace EngineNS.Bricks.FreeTypeFont
{
    public class CFTTextDrawContext : AuxCoreObject<CFTTextDrawContext.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }
        public CFTTextDrawContext()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::FTTextDrawContext");
        }

        private CGfxMaterialInstance mMaterialInst;
        private CGfxEffect[] mEffects;
        private CTextureBindInfo mTextureBindInfo;

        public void ResetContext()
        {
            SDK_FTTextDrawContext_ResetContext(CoreObject);
        }
        public async System.Threading.Tasks.Task<bool> SetMaterial(CRenderContext rc, CGfxMaterialInstance mtl, string textureName)
        {
            mMaterialInst = mtl;
            var senv = CEngine.Instance.PrebuildPassData.Font2DShadingEnvs;
            mEffects = new CGfxEffect[senv.Length];
            for (int i = 0; i < mEffects.Length; i++)
            {
                if (senv[i] == null)
                    continue;

                var desc = CGfxEffectDesc.CreateDesc(mMaterialInst.Material, new CGfxMdfQueue(), senv[i].EnvCode);
                var effect = CEngine.Instance.EffectManager.GetEffect(rc, desc);
                await effect.AwaitLoad();
                mEffects[i] = effect;
            }

            var defEffect = mEffects[(int)PrebuildPassIndex.PPI_Default];
            if (defEffect != null)
            {
                if (textureName != null)
                {
                    mEffects[(int)PrebuildPassIndex.PPI_Default].ShaderProgram.FindTextureBindInfo(mMaterialInst, textureName, ref mTextureBindInfo);
                }
                else
                {
                    mEffects[(int)PrebuildPassIndex.PPI_Default].ShaderProgram.GetTextureBindDesc(0, ref mTextureBindInfo);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private CGfxMeshPrimitives mBuildMeshSource = new CGfxMeshPrimitives();
        public bool BuildMesh(CFontMesh fontMesh, CRenderContext rc, bool flipV)
        {
            var number = (int)SDK_FTTextDrawContext_GetDrawCall(CoreObject);
            if (number <= 0)
                return false;

            unsafe
            {
                var p = stackalloc CShaderResourceView.NativePointer[number];
                {
                    var ptr = SDK_FTTextDrawContext_BuildMesh(CoreObject, rc.CoreObject, p, flipV);
                    if (ptr.Pointer == IntPtr.Zero)
                        return false;

                    mBuildMeshSource.UnsafeReInit(ptr);
                    if (false == fontMesh.Mesh.Init(rc, RName.GetRName(null), mBuildMeshSource/*, CFTShadingEnv.GetFTShadingEnv()*/))
                        return false;
                }

                var senv = CEngine.Instance.PrebuildPassData.Font2DShadingEnvs;
                for (int i = 0; i < number; i++)
                {
                    fontMesh.Mesh.SetMaterialWithEffects(rc, (UInt32)i, mMaterialInst, mEffects, senv);
                }

                for (int i = 0; i < number; i++)
                {
                    var pass = fontMesh.Mesh.MtlMeshArray[i].GetPass((int)PrebuildPassIndex.PPI_Default);
                    pass.ShaderResources.SetUserControlTexture(mTextureBindInfo.PSBindPoint, true);
                    pass.ShaderResources.PSBindTexturePointer(mTextureBindInfo.PSBindPoint, p[i]);
                }
            }

            return true;
        }
        public void BuildMesh2(CFontMesh fontMesh, CCommandList cmd, bool flipV)
        {
            var rc = CEngine.Instance.RenderContext;
            var number = fontMesh.Mesh.MtlMeshArray.Length;
            unsafe
            {
                var p = stackalloc CShaderResourceView.NativePointer[number];
                {
                    var ptr = SDK_FTTextDrawContext_BuildMesh(CoreObject, rc.CoreObject, p, flipV);
                    mBuildMeshSource.UnsafeReInit(ptr);
                }
                for (int i = 0; i < number; i++)
                {
                    var pass = fontMesh.Mesh.MtlMeshArray[i].GetPass((int)PrebuildPassIndex.PPI_Default);

                    pass.ShaderResources.PSBindTexturePointer(mTextureBindInfo.PSBindPoint, p[i]);
                }
            }
        }
        public bool IsValidVersion(CFTFont font)
        {
            return (bool)SDK_FTTextDrawContext_IsValidVersion(CoreObject, font.CoreObject);
        }
        public void RebuildContext(CFTFont font)
        {
            SDK_FTTextDrawContext_RebuildContext(CoreObject, font.CoreObject);
        }
        public void SetClip(int x, int y, int w, int h)
        {
            SDK_FTTextDrawContext_SetClip(CoreObject, x, y, w, h);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static UInt32 SDK_FTTextDrawContext_GetDrawCall(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static unsafe CGfxMeshPrimitives.NativePointer SDK_FTTextDrawContext_BuildMesh(NativePointer self, CRenderContext.NativePointer rc, CShaderResourceView.NativePointer* rsvs, bool bFlipV);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static vBOOL SDK_FTTextDrawContext_IsValidVersion(NativePointer self, CFTFont.NativePointer font);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static void SDK_FTTextDrawContext_RebuildContext(NativePointer self, CFTFont.NativePointer font);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static void SDK_FTTextDrawContext_ResetContext(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static void SDK_FTTextDrawContext_SetClip(NativePointer self, int x, int y, int w, int h);
        #endregion
    }

    public class CFTFont : AuxCoreObject<CFTFont.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }
        public CFTFont(NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();
        }
        public RName Name
        {
            get;
            set;
        }
        public int FontSize
        {
            get
            {
                return SDK_FTFont_GetFontSize(CoreObject);
            }
        }
        [ThreadStatic]
        static byte[] mTempUtf32Bytes = null;
        static void GetUtf32Bytes(string text, ref byte[] utf32Bytes)
        {
            if(mTempUtf32Bytes==null)
                mTempUtf32Bytes = new byte[512];
            var count = Utf32Encoder.GetBytes(text, 0, text.Length, utf32Bytes, 0);
            if(count>=utf32Bytes.Length-8)
            {
                var textData = Utf32Encoder.GetBytes(text);
                utf32Bytes = new byte[textData.Length * 2 + 8];
                count = Utf32Encoder.GetBytes(text, 0, text.Length, utf32Bytes, 0);
            }
            for (int i = 0; i < 8; i++)
            {
                utf32Bytes[count + i] = 0;
            }
        }
        private static UTF32Encoding Utf32Encoder = new UTF32Encoding();
        public void Draw2TextContext(string text, int x, int y, CFTTextDrawContext ctx, bool flipV)
        {
            if(!string.IsNullOrEmpty(text))
            {
#if PWindow
                SDK_FTFont_Draw2TextContext(CoreObject, text, x, y, ctx.CoreObject, flipV);
#else
                GetUtf32Bytes(text, ref mTempUtf32Bytes);
                unsafe
                {
                    fixed (byte* ptr = &mTempUtf32Bytes[0])
                    {
                        SDK_FTFont_Draw2TextContext(CoreObject, (IntPtr)ptr, x, y, ctx.CoreObject, flipV);
                    }
                }
#endif
            }
        }
        public Size MeasureString(string text)
        {
            Size result = new Size();

            int width = 0;
            int height = 0;
            if(!string.IsNullOrEmpty(text))
            {
                unsafe
                {
#if PWindow
                    SDK_FTFont_MeasureString(CoreObject, text, &width, &height);
#else
                    GetUtf32Bytes(text, ref mTempUtf32Bytes);
                    fixed (byte* ptr = &mTempUtf32Bytes[0])
                    {
                        SDK_FTFont_MeasureString(CoreObject, (IntPtr)ptr, &width, &height);
                    }
#endif
                }
            }
            result.Width = width;
            result.Height = height;
            return result;
        }
        public int CheckPointChar(NativePointer self, string text, int x, int y, out int pos)
        {
            unsafe
            {
                if(!string.IsNullOrEmpty(text))
                {
                    fixed (int* ptr = &pos)
                    {
#if PWindow
                        return SDK_FTFont_CheckPointChar(CoreObject, text, x, y, ptr);
#else
                        GetUtf32Bytes(text, ref mTempUtf32Bytes);
                        fixed (byte* pStr = &mTempUtf32Bytes[0])
                        {
                            return SDK_FTFont_CheckPointChar(CoreObject, (IntPtr)pStr, x, y, ptr);
                        }
#endif
                    }
                }
                else
                {
                    pos = -1;
                    return -1;
                }
            }
        }
        public unsafe int CalculateWrap(string text, int* idxArray, int idxArrayLength, int widthLimit, out int height)
        {
            unsafe
            {
                if(!string.IsNullOrEmpty(text))
                {
                    fixed (int* heightPtr = &height)
                    {
#if PWindow
                        return SDK_FTFont_CalculateWrap(CoreObject, text, idxArray, idxArrayLength, widthLimit, heightPtr);
#else
                        GetUtf32Bytes(text, ref mTempUtf32Bytes);
                        fixed(byte* pStr = &mTempUtf32Bytes[0])
                        {
                            return SDK_FTFont_CalculateWrap(CoreObject, (IntPtr)pStr, idxArray, idxArrayLength, widthLimit, heightPtr);
                        }
#endif
                    }
                }
                else
                {
                    height = 0;
                    return -1;
                }
            }
        }
        public unsafe int CalculateWrapWithWord(string text, int* idxArray, int idxArrayLength, int widthLimit, out int height)
        {
            unsafe
            {
                fixed(int* heightPtr = &height)
                {
#if PWindow
                    return SDK_FTFont_CalculateWrapWithWord(CoreObject, text, idxArray, idxArrayLength, widthLimit, heightPtr);
#else
                    GetUtf32Bytes(text, ref mTempUtf32Bytes);
                    fixed(byte* pStr = &mTempUtf32Bytes[0])
                    {
                        return SDK_FTFont_CalculateWrapWithWord(CoreObject, (IntPtr)pStr, idxArray, idxArrayLength, widthLimit, heightPtr);
                    }
#endif
                }
            }
        }
        public void UpdateHotWords()
        {
            SDK_FTFont_UpdateHotWords(CoreObject);
        }
#region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_FTFont_GetName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_FTFont_GetFontSize(NativePointer self);
#if PWindow
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static void SDK_FTFont_Draw2TextContext(NativePointer self, string text, int x, int y, CFTTextDrawContext.NativePointer ctx, bool flipV);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static unsafe void SDK_FTFont_MeasureString(NativePointer self, string text, int* width, int* height);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static unsafe int SDK_FTFont_CheckPointChar(NativePointer self, string text, int x, int y, int* pos);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static unsafe int SDK_FTFont_CalculateWrap(NativePointer self, string text, int* idxArray, int idxArraySize, int widthLimit, int* height);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static unsafe int SDK_FTFont_CalculateWrapWithWord(NativePointer self, string text, int* idxArray, int idxArraySize, int widthLimit, int* height);
#else
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_FTFont_Draw2TextContext(NativePointer self, IntPtr text, int x, int y, CFTTextDrawContext.NativePointer ctx, bool flipV);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_FTFont_MeasureString(NativePointer self, IntPtr text, int* width, int* height);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe int SDK_FTFont_CheckPointChar(NativePointer self, IntPtr text, int x, int y, int* pos);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe int SDK_FTFont_CalculateWrap(NativePointer self, IntPtr text, int* idxArray, int idxArraySize, int widthLimit, int* height);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe int SDK_FTFont_CalculateWrapWithWord(NativePointer self, IntPtr text, int* idxArray, int idxArraySize, int widthLimit, int* height);
#endif
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static void SDK_FTFont_UpdateHotWords(NativePointer self);
#endregion
    }

    public class CFontMesh
    {
        public CFontMesh()
        {
            Mesh = new CGfxMesh();
            Mesh.Tag = this;
        }
        public async System.Threading.Tasks.Task<bool> SetMaterial(CRenderContext rc, CGfxMaterialInstance mtl, string textureName)
        {
            return await TextContext.SetMaterial(rc, mtl, textureName);
        }
        public CGfxMesh Mesh;
        public CConstantBuffer CBuffer;
        private int TextMatrixId = -1;
        public Matrix mRenderMatrix = Matrix.Identity;
        public Matrix RenderMatrix
        {
            get => mRenderMatrix;
            set
            {
                mRenderMatrix = value;
                if(CBuffer != null)
                {
                    if (TextMatrixId == -1)
                        TextMatrixId = CBuffer.FindVar("RenderMatrix");
                    CBuffer.SetValue(TextMatrixId, value, 0);
                }
            }
        }
        private int TextOpacityId = -1;
        private float mTextOpacity = 1.0f;
        public float TextOpacity
        {
            get => mTextOpacity;
            set
            {
                mTextOpacity = value;
                if(CBuffer != null)
                {
                    if (TextOpacityId == -1)
                        TextOpacityId = CBuffer.FindVar("TextOpacity");

                    CBuffer.SetValue(TextOpacityId, value, 0);
                }
            }
        }
        private int TextColorId = -1;
        private Color4 mTextColor = new Color4(1,1,1,1);
        public Color4 TextColor
        {
            get => mTextColor;
            set
            {
                mTextColor = value;
                if(CBuffer != null)
                {
                    if (TextColorId == -1)
                        TextColorId = CBuffer.FindVar("TextColor");

                    CBuffer.SetValue(TextColorId, value, 0);
                }
            }
        }
        public int PassNum
        {
            get
            {
                if (Mesh == null)
                    return 0;
                return Mesh.MtlMeshArray.Length;
            }
        }
        public CPass GetPass(int index)
        {
            if (Mesh == null)
                return null;
            return Mesh.MtlMeshArray[index].GetPass(PrebuildPassIndex.PPI_Default);
        }
        private CFTTextDrawContext TextContext = new EngineNS.Bricks.FreeTypeFont.CFTTextDrawContext();
        /// <summary>
        /// 绘制文字
        /// </summary>
        /// <param name="rc"></param>
        /// <param name="font">字体</param>
        /// <param name="text">文字内容</param>
        /// <param name="resetContext">true:开始下一批次绘制，false:与上次绘制同批次绘制</param>
        public void DrawText(CRenderContext rc, CFTFont font, string text, bool resetContext)
        {
            DrawText(rc, 0, 0, font, text, resetContext);
        }
        public void DrawText(CRenderContext rc, int offsetX, int offsetY, CFTFont font, string text, bool resetContext)
        {
            if (resetContext)
                TextContext.ResetContext();

            bool flipV = (CEngine.Instance.Desc.RHIType == ERHIType.RHT_OGL);

            font.Draw2TextContext(text, offsetX, offsetY, TextContext, flipV);
            //var textSize = font.MeasureString(text);

            TextContext.BuildMesh(this, rc, flipV);
        }
        public void BuildMesh(CCommandList cmd)
        {
            //bool flipV = (CEngine.Instance.Desc.RHIType == ERHIType.RHT_OGL);
            //TextContext.BuildMesh2(this, cmd, flipV);
        }
        //CScissorRect mSRect;
        public void SetClip(int left, int top, int right, int bottom)
        {
            TextContext.SetClip(left, top, right, bottom);
            //if (Mesh == null)
            //    return;
            
            //if(mSRect==null)
            //{
            //    mSRect = new CScissorRect();
            //    mSRect.RectNumber = 1;
            //}
            //mSRect.SetSCRect(0, left, top, right, bottom);
            //for (int i=0; i < Mesh.MtlMeshArray.Length; i++)
            //{
            //    var pass = Mesh.MtlMeshArray[i].GetPass(PrebuildPassIndex.PPI_Default);
            //    pass.Scissor = mSRect;
            //}
        }
        public void DisableClip()
        {
            if (Mesh == null)
                return;
            for (int i = 0; i < Mesh.MtlMeshArray.Length; i++)
            {
                var pass = Mesh.MtlMeshArray[i].GetPass(PrebuildPassIndex.PPI_Default);
                pass.Scissor = null;
            }
        }
    }
}
