using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;

namespace EngineNS.Graphics
{

    [Rtti.MetaClass]
    public abstract class CGfxShadingEnv : AuxIOCoreObject<CGfxShadingEnv.NativePointer>
    {
        public CGfxShadingEnv()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxShadingEnv");
        }
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
        private GfxEnvShaderCode mEnvCode;
        public GfxEnvShaderCode EnvCode
        {
            get { return mEnvCode; }
        }
        protected bool SetMacroDefineValue(int index, string value)
        {
            return mEnvCode.SetMacroDefineValue(index, value);
        }
        public void SetMacroDefineValue(string name, string value)
        {
            mEnvCode.SetMacroDefineValue(name, value);
        }
        public override Hash64 GetHash64()
        {
            return EnvCode.GetHash64();
        }
        public void InitCodeOnce(GfxEnvShaderCode code)
        {
            var defs = new List<CShaderDefinitions.MacroDefine>();
            GetMacroDefines(defs);
            code.Init(ShadingEnvName, defs, this);

            OnShadingEnvInit(code);
        }
        protected virtual void OnShadingEnvInit(GfxEnvShaderCode code)
        {

        }
        public virtual void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
            defs.Add(new CShaderDefinitions.MacroDefine("ENV_DISABLE_SHADOW", "0"));
            defs.Add(new CShaderDefinitions.MacroDefine("ENV_DISABLE_AO", "0"));
            defs.Add(new CShaderDefinitions.MacroDefine("ENV_DISABLE_POINTLIGHTS", "0"));
        }
        public abstract RName ShadingEnvName
        {
            get;
        }
        public virtual CGfxShadingEnv CloneShadingEnv()
        {
            var result = System.Activator.CreateInstance(this.GetType()) as CGfxShadingEnv;
            result.Init(mEnvCode);
            return result;
        }
        public bool Init(GfxEnvShaderCode code)
        {
            mEnvCode = code;
            OnCreated();
            return true;
        }
        protected virtual void OnCreated()
        {

        }

        //RName mName;
        //[Editor.Editor_PackData()]
        //public RName Name
        //{
        //    get => mName;
        //    protected set => mName = value;
        //}
        [Editor.Editor_PackData()]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.ShaderCode)]
        public RName ShaderName
        {
            get
            {
                if (EnvCode == null)
                    return null;
                return EnvCode.ShaderName;
            }
        }
        Dictionary<string, CGfxVarValue> mVars = new Dictionary<string, CGfxVarValue>();
        public void AddVar(string name, EShaderVarType type, UInt32 elements)
        {
            CGfxVarValue val;
            if (mVars.TryGetValue(name, out val))
            {
                if (val.Definition.Type == type && val.Definition.Elements == elements)
                    return;
                val.Definition = new CGfxVar()
                {
                    Name = name,
                    Type = type,
                    Elements = elements,
                };
                var size = val.GetValueSize(type) * elements;
                val.ValueArray = new byte[size];
            }
            else
            {
                var tmp = new CGfxVarValue();
                tmp.Definition = new CGfxVar()
                {
                    Name = name,
                    Type = type,
                    Elements = elements,
                };
                var size = tmp.GetValueSize(type) * elements;
                tmp.ValueArray = new byte[size];
                mVars[name] = tmp;
            }
        }
        public void RemoveVar(string name)
        {
            mVars.Remove(name);
        }

        Dictionary<string, SRVParam> mSRViews = new Dictionary<string, SRVParam>();
        public void AddSRV(string name)
        {
            SRVParam param;
            if(!mSRViews.TryGetValue(name, out param))
            {
                var tmp = new SRVParam()
                {
                    ShaderName = name,
                };
                mSRViews[name] = tmp;
            }
        }
        public void RemoveSRV(string name)
        {
            mSRViews.Remove(name);
        }
        public void SetSRV(string name, CShaderResourceView srv)
        {
            SRVParam param;
            if(mSRViews.TryGetValue(name, out param))
            {
                param.RSView.Cleanup();
                param.RSView = srv;
                if (srv != null)
                    param.RName = srv.Name;
                else
                    param.RName = RName.EmptyName;
            }
        }

        public virtual void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
        {
            //每个shadingenv根据自己的shader代码，在这里绑定纹理，设置cbPerInstance变量
            //正常情况ShadingEnv不应该给cbPerInstance设置变量，特殊需求在这个函数特殊写
            //shadingEnv的变量，应该主要在非cbPerInstance里面
        }

        #region SDK
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxShadingEnv_Init(NativePointer self, string name, string shader);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static UInt32 SDK_GfxShadingEnv_GetVersion(NativePointer self);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        //public extern static string SDK_GfxShadingEnv_GetName(NativePointer self);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        //public extern static string SDK_GfxShadingEnv_GetShaderName(NativePointer self);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxShadingEnv_AddVar(NativePointer self, string name, EShaderVarType type, UInt32 elements);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxShadingEnv_RemoveVar(NativePointer self, string name);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxShadingEnv_AddSRV(NativePointer self, string name);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxShadingEnv_RemoveSRV(NativePointer self, string name);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxShadingEnv_SetSRV(NativePointer self, string name, CShaderResourceView.NativePointer srv);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //   public extern static CShaderResourceView.NativePointer SDK_GfxShadingEnv_GetSRV(NativePointer self, string name);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxShadingEnv_Save2Xnd(NativePointer self, IO.XndNode.NativePointer node);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxShadingEnv_LoadXnd(NativePointer self, IO.XndNode.NativePointer node);

        #endregion
    }

    public class CShadingPermutation
    {
        public static void CollectShaderInfo(CGfxEffectDesc desc)
        {
            if (desc.EnvShaderPatch.GetMacroValues == null)
                return;

            CollectShaderInfoImpl(desc, 0);
        }
        private static void CollectShaderInfoImpl(CGfxEffectDesc desc, int index)
        {
            var cloneDesc = desc.CloneEffectDesc();
            GfxEnvShaderCode shadingEnv = cloneDesc.EnvShaderPatch;
            if (index >= shadingEnv.MacroDefines.Count)
                return;

            for (int i = index; i < shadingEnv.MacroDefines.Count; i++)
            {
                var values = shadingEnv.GetMacroValues(shadingEnv.MacroDefines[i].Name);
                if (values == null)
                    continue;
                for (int j = 0; j < values.Count; j++)
                {   
                    if (shadingEnv.SetMacroDefineValue(i, values[j]))
                    {
                        cloneDesc.UpdateHash64(true);
                        Hash64 hash;
                        hash = cloneDesc.GetHash64();
                        var file = CGfxEffectDesc.GetShaderInfoFileName(hash);
                        if (CEngine.Instance.FileManager.FileExists(file) == false)
                        {
                            cloneDesc.SaveXML(null, hash);
                        }

                        CollectShaderInfoImpl(cloneDesc, i + 1);
                    }
                }
            }
        }
    }

    public class CGfxShadingEnvManager
    {
        
        public Dictionary<Type, CGfxShadingEnv> ShadingEnvs
        {
            get;
        } = new Dictionary<Type, CGfxShadingEnv>();
        public GfxEnvShaderCode FindEnvShaderCode(Type type)
        {
            CGfxShadingEnv tmp;
            if (ShadingEnvs.TryGetValue(type, out tmp))
            {
                return tmp.EnvCode;
            }
            return null;
        }
        public EnvType GetGfxShadingEnv<EnvType>() where EnvType : CGfxShadingEnv, new()
        {
            CGfxShadingEnv tmp;
            if (ShadingEnvs.TryGetValue(typeof(EnvType), out tmp))
            {
                return tmp.CloneShadingEnv() as EnvType;
                //return tmp as EnvType;
            }

            EnvType result = new EnvType();
            GfxEnvShaderCode code = new GfxEnvShaderCode();
            result.InitCodeOnce(code);
            if (false == result.Init(code))
                return null;

            ShadingEnvs.Add(typeof(EnvType), result);
            return result;
        }

        public bool Init()
        {
            mDefaultShadingEnv = GetGfxShadingEnv<EnvShader.CGfxDefaultSE>();
            return true;
        }
        CGfxShadingEnv mDefaultShadingEnv;
        public CGfxShadingEnv DefaultShadingEnv
        {
            get
            {       
                return mDefaultShadingEnv;
            }
        }
    }
}
