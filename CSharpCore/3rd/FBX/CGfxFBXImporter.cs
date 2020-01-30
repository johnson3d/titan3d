using EngineNS;
using EngineNS.Bricks.AssetImpExp;
using EngineNS.Bricks.AssetImpExp.Creater;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FBX
{
    public class CGfxFBXImporter : AuxCoreObject<CGfxFBXImporter.NativePointer>
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
        public string FileName { get; set; } = "";


        public CGfxFBXImporter()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxFBXImporter");
        }
        public async System.Threading.Tasks.Task<bool> PreImport(string fileName, CGfxFileImportOption fileImportOption)
        {
            await CEngine.Instance.EventPoster.Post(() =>
            {
                FileName = fileName;
                return SDK_GfxFBXImporter_PreImport(mCoreObject, fileName, fileImportOption.CoreObject, CGfxFBXManager.Instance.CoreObject);
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncIO);
            return true;
        }
        public async System.Threading.Tasks.Task Import(CGfxAsset_File assetFile)
        {
            bool async = true;
            if (async)
            {
                await CEngine.Instance.EventPoster.Post(() =>
                {
                    SDK_GfxFBXImporter_Import(mCoreObject, CEngine.Instance.RenderContext.CoreObject, FileName, assetFile.CoreObject, CGfxFBXManager.Instance.CoreObject);
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.AsyncIO);
            }
            else
            {
                SDK_GfxFBXImporter_Import(mCoreObject, CEngine.Instance.RenderContext.CoreObject, FileName, assetFile.CoreObject, CGfxFBXManager.Instance.CoreObject);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxFBXImporter_PreImport(NativePointer self, string fileName, CGfxFileImportOption.NativePointer assetImportOption, CGfxFBXManager.NativePointer manager);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxFBXImporter_Import(NativePointer self, CRenderContext.NativePointer rc, string fileName, CGfxAsset_File.NativePointer assetImportOption, CGfxFBXManager.NativePointer manager);
        #endregion
    }
}
