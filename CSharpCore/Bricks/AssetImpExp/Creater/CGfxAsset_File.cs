using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.AssetImpExp.Creater
{
    public class CGfxAsset_File : AuxCoreObject<CGfxAsset_File.NativePointer>
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
        public CGfxAsset_File()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxAsset_File");
        }
        CGfxFileImportOption mFileOption = null;
        public CGfxFileImportOption FileOption
        {
            get
            {
                if(mFileOption == null)
                {
                    //native Get;
                }
                return mFileOption;
            }
            set
            {
                mFileOption = value;
                SDK_GfxAsset_File_SetImportOption(CoreObject, value.CoreObject);
            }
        }
        public Dictionary<uint, CGfxAssetCreater> AssetCreaters { get; set; } = new Dictionary<uint, CGfxAssetCreater>();
        public void AddCreaters(uint hash, CGfxAssetCreater creater)
        {
            AssetCreaters.Add(hash, creater);
            SDK_GfxAsset_File_AddCreater(CoreObject, hash, creater.CoreObject);
        }

        public async System.Threading.Tasks.Task SaveAsset()
        {
            using (var it = AssetCreaters.GetEnumerator())
            {
                while(it.MoveNext())
                {
                    var creater =  it.Current.Value;
                    await creater.SaveAsset();
                }
            }
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAsset_File_SetImportOption(NativePointer self, CGfxFileImportOption.NativePointer nativeOption);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAsset_File_AddCreater(NativePointer self, uint index,CGfxAssetCreater.NativePointer creater);
        #endregion
    }
}
