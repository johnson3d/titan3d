using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

namespace EngineNS
{
    public partial class CShaderDesc : AuxCoreObject<CShaderDesc.NativePointer>
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
        public CShaderDesc(EShaderType type)
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::IShaderDesc");
            SDK_IShaderDesc_SetShaderType(CoreObject, type);
        }
        public CShaderDesc(NativePointer ptr)
        {
            mCoreObject = ptr;
        }
        public EShaderType ShaderType
        {
            get
            {
                return SDK_IShaderDesc_GetShaderType(CoreObject);
            }
        }
        public void OptimizeGLES300(CShaderDesc desc)
        {
            OptimizeGLES300_Windows(desc);
        }
        partial void OptimizeGLES300_Windows(CShaderDesc desc);
        public string GLCode
        {
            get
            {
                return SDK_IShaderDesc_GetGLCode(CoreObject);
            }
        }
        public string MetalCode
        {
            get
            {
                return SDK_IShaderDesc_GetMetalCode(CoreObject);
            }
        }
        public void SetGLCode(string code)
        {
            SDK_IShaderDesc_SetGLCode(CoreObject, code);
        }
        public void SetMetalCode(string code)
        {
            SDK_IShaderDesc_SetMetalCode(CoreObject, code);
        }
        public void Save2Xnd(IO.XndNode node, EPlatformType platforms)
        {
            SDK_IShaderDesc_Save2Xnd(CoreObject, node.CoreObject, (UInt32)platforms);
        }
        public bool LoadXnd(IO.XndNode node)
        {
            return (bool)SDK_IShaderDesc_LoadXnd(CoreObject, node.CoreObject);
        }
        public UInt32 CBufferNum
        {
            get
            {
                return SDK_IShaderDesc_GetCBufferNum(CoreObject);
            }
        }
        public UInt32 SRVNum
        {
            get
            {
                return SDK_IShaderDesc_GetSRVNum(CoreObject);
            }
        }
        public UInt32 SamplerNum
        {
            get
            {
                return SDK_IShaderDesc_GetSamplerNum(CoreObject);
            }
        }
        public bool GetCBufferDesc(UInt32 index, ref CConstantBufferDesc info)
        {
            unsafe
            {
                fixed (CConstantBufferDesc* p = &info)
                {
                    return SDK_IShaderDesc_GetCBufferDesc(CoreObject, index, p);
                }
            }
        }
        public bool GetSRVDesc(UInt32 index, ref CTextureBindInfo info)
        {
            unsafe
            {
                fixed (CTextureBindInfo* p = &info)
                {
                    return SDK_IShaderDesc_GetSRVDesc(CoreObject, index, p);
                }
            }
        }
        public bool GetSamplerDesc(UInt32 index, ref CSamplerBindInfo info)
        {
            unsafe
            {
                fixed (CSamplerBindInfo* p = &info)
                {
                    return SDK_IShaderDesc_GetSamplerDesc(CoreObject, index, p);
                }
            }
        }
        public UInt32 FindCBufferDesc(string info)
        {
            return SDK_IShaderDesc_FindCBufferDesc(CoreObject, info);
            //if ( CEngine.Instance.Desc.RHIType != ERHIType.RHT_D3D11 )
            //    return SDK_IShaderDesc_FindCBufferDesc(CoreObject, "type_" + info);
            //else
            //    return SDK_IShaderDesc_FindCBufferDesc(CoreObject, info);
        }
        public UInt32 FindSRVDesc(string info)
        {
            return SDK_IShaderDesc_FindSRVDesc(CoreObject, info);
        }
        public UInt32 FindSamplerDesc(string info)
        {
            return SDK_IShaderDesc_FindSamplerDesc(CoreObject, info);
        }
        public bool GetCBufferDesc(string name, ref CConstantBufferDesc info)
        {
            var index = FindCBufferDesc(name);
            if (index == uint.MaxValue)
                return false;
            return GetCBufferDesc(index, ref info);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderDesc_SetShaderType(NativePointer self, EShaderType type);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static EShaderType SDK_IShaderDesc_GetShaderType(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderDesc_SetGLCode(NativePointer self, string code);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderDesc_SetMetalCode(NativePointer self, string code);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_IShaderDesc_GetGLCode(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_IShaderDesc_GetMetalCode(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderDesc_Save2Xnd(NativePointer self, IO.XndNode.NativePointer node, UInt32 platforms);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_IShaderDesc_LoadXnd(NativePointer self, IO.XndNode.NativePointer node);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IShaderDesc_GetCBufferNum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IShaderDesc_GetSRVNum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IShaderDesc_GetSamplerNum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_IShaderDesc_GetCBufferDesc(NativePointer self, UInt32 index, CConstantBufferDesc* info);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_IShaderDesc_GetSRVDesc(NativePointer self, UInt32 index, CTextureBindInfo* info);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_IShaderDesc_GetSamplerDesc(NativePointer self, UInt32 index, CSamplerBindInfo* info);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe UInt32 SDK_IShaderDesc_FindCBufferDesc(NativePointer self, string info);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe UInt32 SDK_IShaderDesc_FindSRVDesc(NativePointer self, string info);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe UInt32 SDK_IShaderDesc_FindSamplerDesc(NativePointer self, string info);
        #endregion
    }

    public class CShaderDefinitions : AuxCoreObject<CShaderDefinitions.NativePointer>
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
        public CShaderDefinitions()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::IShaderDefinitions");
        }
        public struct MacroDefine
        {
            public MacroDefine(string n, string d)
            {
                Name = n;
                Definition = d;
            }
            public string Name;
            public string Definition;
        }
        public List<MacroDefine> mShaderMacroArray
        {
            get;
        } = new List<MacroDefine>();
        public void SetDefine(string name, string value)
        {
            MacroDefine def = new MacroDefine();
            foreach(var i in mShaderMacroArray)
            {
                if (i.Name == name)
                {
                    def.Definition = value;
                    SDK_IShaderDefinitions_AddDefine(CoreObject, name, value);
                    return;
                }
            }
            def = new MacroDefine(name, value);
            mShaderMacroArray.Add(def);
            SDK_IShaderDefinitions_AddDefine(CoreObject, name, value);
        }
        public void RemoveDefine(string name)
        {
            for(int i= 0; i < mShaderMacroArray.Count; i++)
            {
                if (mShaderMacroArray[i].Name == name)
                {
                    mShaderMacroArray.RemoveAt(i);
                    break;
                }
            }
            SDK_IShaderDefinitions_RemoveDefine(CoreObject, name);
        }
        public void ClearDefines()
        {
            mShaderMacroArray.Clear();
            ExtraIncludes.Clear();
            ExtraVarDefines.Clear();
            SDK_IShaderDefinitions_ClearDefines(CoreObject);
        }
        public void MergeDefinitions(CShaderDefinitions defs)
        {
            //todo
        }
        public override Hash64 GetHash64()
        {
            var result = Hash64.Empty;

            if (mShaderMacroArray.Count > 0)
            {
                Hash64.CalcHash64(ref result, ToString());
            }
            return result;
        }
        public override string ToString()
        {
            var useDefs = mShaderMacroArray.OrderByDescending(s => s.Name).ToList<MacroDefine>();
            string result = "Macro:";
            foreach(var i in useDefs)
            {
                result += $"{i.Name}={i.Definition}";
            }
            return result;
        }

        public enum EExtraIncludeType
        {
            Material,
            ShadingEnv,
            MdfQueue,
        }
        private Dictionary<EExtraIncludeType, string[]> ExtraIncludes = new Dictionary<EExtraIncludeType, string[]>();
        public void SetExtraInclude(EExtraIncludeType type, string[] files)
        {
            ExtraIncludes[type] = files;
            string incStr = "";
            foreach(var i in ExtraIncludes)
            {
                foreach (var j in i.Value)
                {
                    incStr += j + ";";
                }
            }
            SetDefine("EXTRA_Include", incStr);
        }
        private Dictionary<EExtraIncludeType, string[]> ExtraVarDefines = new Dictionary<EExtraIncludeType, string[]>();
        public void SetExtraDefines(EExtraIncludeType type, string[] files)
        {
            ExtraVarDefines[type] = files;
            string incStr = "";
            foreach (var i in ExtraVarDefines)
            {
                foreach (var j in i.Value)
                {
                    incStr += j + ";";
                }
            }
            SetDefine("EXTRA_Defines", incStr);
        }


        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderDefinitions_AddDefine(NativePointer self, string name, string value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderDefinitions_RemoveDefine(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderDefinitions_ClearDefines(NativePointer self);
        #endregion
    }
}
