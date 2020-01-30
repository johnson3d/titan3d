using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.FreeTypeFont
{
    public class CFTFontManager : AuxCoreObject<CFTFontManager.NativePointer>
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
        public CFTFontManager()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::FTFontManager");
        }
        /// <summary>
        /// 获取字体
        /// </summary>
        /// <param name="name">字体名称</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="cachTexSize">文字缓存纹理大小，多个文字公用一张纹理</param>
        /// <param name="texSize">文字纹理大小，特殊文字使用</param>
        /// <returns></returns>
        public CFTFont GetFont(RName name, int fontSize, int cachTexSize, int texSize)
        {
            foreach(var i in Fonts)
            {
                if (i.Name == name && i.FontSize == fontSize)
                    return i;
            }

            lock (this)
            {
                var ptr = SDK_FTFontManager_GetFont(CoreObject, name.Address, fontSize, cachTexSize, texSize);
                var font = new CFTFont(ptr);
                font.Name = name;
                Fonts.Add(font);
                return font;
            }
        }
        public List<CFTFont> Fonts
        {
            get;
        } = new List<CFTFont>();
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CFTFont.NativePointer SDK_FTFontManager_GetFont(NativePointer self, string file, int fontSize, int cacheTexSize, int texSize);
        #endregion
    }

    public class CFTFontManagerProcessor : CEngineAutoMemberProcessor
    {
        public override async System.Threading.Tasks.Task<object> CreateObject()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return new CFTFontManager();
        }
        public override void Tick(object obj)
        {

        }
        public override void Cleanup(object obj)
        {

        }
    }
}

namespace EngineNS
{
    public partial class CEngine
    {
        [CEngineAutoMemberAttribute(typeof(Bricks.FreeTypeFont.CFTFontManagerProcessor))]
        public Bricks.FreeTypeFont.CFTFontManager FontManager
        {
            get;
            set;
        }
    }
}
