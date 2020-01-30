using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using EngineNS.Macross;

namespace EngineNS.Graphics
{
    [Rtti.MetaClass]
    public class CGfxMaterialParam : EngineNS.IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public delegate bool Delegate_OnReName(CGfxMaterialParam info, string oldName, string newName);
        public Delegate_OnReName OnReName;
        public delegate void Delegate_OnSetValue(CGfxMaterialParam info, string oldValue, string newValue);
        public Delegate_OnSetValue OnSetValue;

        public bool IsDirty
        {
            get;
            set;
        }
        [Rtti.MetaData]
        public string EditorType = "";

        public string OldVarName;

        string mVarName;
        [Rtti.MetaData]
        [Editor.Editor_PackData()]
        public string VarName
        {
            get => mVarName;
            protected set
            {
                if(mVarName != value)
                {
                    mVarName = value;
                    IsDirty = true;
                }
            }
        }
        string mVarValue = "";
        [Rtti.MetaData]
        public string VarValue
        {
            get => mVarValue;
            protected set
            {
                mVarValue = value;
                IsDirty = true;
            }
        }
        RName mTextureRName = RName.EmptyName;
        [Rtti.MetaData]
        public RName TextureRName
        {
            get => mTextureRName;
            protected set
            {
                mTextureRName = value;
                IsDirty = true;
            }
        }
        EShaderVarType mVarType = EShaderVarType.SVT_Unknown;
        [Rtti.MetaData]
        public EShaderVarType VarType
        {
            get => mVarType;
            set
            {
                mVarType = value;
                IsDirty = true;
            }
        }
        UInt32 mElementIdx = 0;
        [Rtti.MetaData]
        public UInt32 ElementIdx
        {
            get => mElementIdx;
            set
            {
                mElementIdx = value;
                IsDirty = true;
            }
        }
        public CGfxMaterialParam()
        {
        }

        public CGfxMaterialParam(string name)
        {
            VarName = name;
        }

        public void Initialize(string name, EShaderVarType varType, string varValue)
        {
            mVarName = name;
            mVarType = varType;
            mVarValue = varValue;
        }
        public void Initialize(string name, RName varValue)
        {
            mVarName = name;
            mVarType = EShaderVarType.SVT_Texture;
            mTextureRName = varValue;
        }

        public void CopyFrom(CGfxMaterialParam param, bool copyName = true)
        {
            if(copyName)
            {
                OldVarName = param.OldVarName;
                VarName = param.VarName;
            }
            VarType = param.VarType;
            VarValue = param.VarValue;
            TextureRName = param.TextureRName;
            EditorType = param.EditorType;
        }
        public void CopyWithNewName(CGfxMaterialParam param, string newName)
        {
            OldVarName = "";
            VarName = newName;
            VarType = param.VarType;
            VarValue = param.VarValue;
            TextureRName = param.TextureRName;
            EditorType = param.EditorType;
        }

        //public static string NewValueString(EShaderVarType varType)
        //{
        //    switch(varType)
        //    {
        //        case EShaderVarType.SVT_Texture:
        //            return EngineNS.CEngine.Instance.GameInstance.Desc.DefaultTextureName.Name;
        //        case EShaderVarType.SVT_Float1:
        //        case EShaderVarType.SVT_Int1:
        //            return "0";
        //        case EShaderVarType.SVT_Float2:
        //        case EShaderVarType.SVT_Int2:
        //            return "0,0";
        //        case EShaderVarType.SVT_Int3:
        //        case EShaderVarType.SVT_Float3:
        //            return "0,0,0";
        //        case EShaderVarType.SVT_Float4:
        //        case EShaderVarType.SVT_Int4:
        //            return "0,0,0,0";
        //    }
        //    return "";
        //}

        public bool Rename(string newName)
        {
            if (OnReName?.Invoke(this, VarName, newName) == false)
                return false;
            OldVarName = VarName;
            VarName = newName;
            return true;
        }

        public void SetValueStr(string value)
        {
            var oldVal = VarValue;
            VarValue = value;
            //OnSetValue?.Invoke(this, oldVal, value);
        }
        public void SetValueStr(RName value)
        {
            var oldVal = TextureRName;
            TextureRName = value;
            //OnSetValue?.Invoke(this, )
        }

        #region Assist
        public static void GetTypeValue(string value, out string outValue)
        {
            outValue = value;
        }
        public static void GetTypeValue(string value, out int outValue)
        {
            outValue = System.Convert.ToInt32(value);
        }
        public static void GetTypeValue(string value, out float outValue)
        {
            outValue = System.Convert.ToSingle(value);
        }
        public static void GetTypeValue(string value, out EngineNS.Vector2 outValue)
        {
            var preStr = "float2(";
            var length = preStr.Length + 1;
            var startIdx = value.IndexOf(preStr);
            if (startIdx < 0)
                length = 0;
            var subStr = value.TrimEnd(')');// value.Substring(length, value.Length - length - 1);
            var splits = subStr.Split(',');
            var x = System.Convert.ToSingle(splits[0]);
            var y = System.Convert.ToSingle(splits[1]);
            outValue = new EngineNS.Vector2(x, y);
        }
        public static void GetTypeValue(string value, out EngineNS.Vector3 outValue)
        {
            var preStr = "float3(";
            var length = preStr.Length + 1;
            var startIdx = value.IndexOf(preStr);
            if (startIdx < 0)
                length = 0;
            var subStr = value.TrimEnd(')');// value.Substring(length, value.Length - length - 1);
            var splits = subStr.Split(',');
            var x = System.Convert.ToSingle(splits[0]);
            var y = System.Convert.ToSingle(splits[1]);
            var z = System.Convert.ToSingle(splits[2]);
            outValue = new EngineNS.Vector3(x, y, z);
        }
        public static void GetTypeValue(string value, out EngineNS.Vector4 outValue)
        {
            var preStr = "float4(";
            var length = preStr.Length + 1;
            var startIdx = value.IndexOf(preStr);
            if (startIdx < 0)
                length = 0;
            var subStr = value.TrimEnd(')');// .Substring(length, value.Length - length - 1);
            var splits = subStr.Split(',');
            var x = System.Convert.ToSingle(splits[0]);
            var y = System.Convert.ToSingle(splits[1]);
            var z = System.Convert.ToSingle(splits[2]);
            var w = System.Convert.ToSingle(splits[3]);
            outValue = new EngineNS.Vector4(x, y, z, w);
        }
        #endregion
    }

    [Rtti.MetaClass]
    public class CGfxMaterial : AuxCoreObject<CGfxMaterial.NativePointer>, IO.IResourceFile, IO.Serializer.ISerializer
    {
        #region ISerializer
        public void ReadObjectXML(EngineNS.IO.XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObjectXML(this, node);
        }

        public void WriteObjectXML(EngineNS.IO.XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObjectXML(this, node);
        }

        public void ReadObject(EngineNS.IO.Serializer.IReader pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg);
        }

        public void ReadObject(EngineNS.IO.Serializer.IReader pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg, metaData);
        }

        public void WriteObject(EngineNS.IO.Serializer.IWriter pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg);
        }

        public void WriteObject(EngineNS.IO.Serializer.IWriter pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg, metaData);
        }
        public EngineNS.IO.Serializer.ISerializer CloneObject()
        {
            return EngineNS.IO.Serializer.SerializerHelper.CloneObject(this);
        }
        #endregion

        public static readonly string ShaderIncludeExtension = ".code";
        public static readonly string ShaderDefineExtension = ".var";
        public static readonly string ShaderLinkExtension = ".link";
        private Hash64 mHash64 = Hash64.Empty;
        private void UpdateHash64()
        {
            var hashStr = Name.Name;
            if (Name.RNameType != RName.enRNameType.Game)
                hashStr += ":" + Name.RNameType;
            if (string.IsNullOrEmpty(hashStr))
                return;
            EngineNS.Hash64.CalcHash64(ref mHash64, hashStr);
        }
        public override Hash64 GetHash64()
        {
            if (mHash64 == Hash64.Empty)
            {
                UpdateHash64();
            }
            return mHash64;
        }

        EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task<Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        {
            return await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
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
        public override string ToString()
        {
            //return this.Name.ToString() + ":" + Version;
            return this.Name.ToString();
        }

        
        public struct MtlMacro
        {
            [Editor.Editor_PackData()]
            public string mMacroName;
            [Editor.Editor_PackData()]
            public string mMacroValue;
        };


        public CGfxMaterial()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxMaterial");
        }

        private string mHash64String;
        public string Hash64String
        {
            get
            {
                if (mHash64String == null)
                    mHash64String = this.GetHash64().ToString();
                return mHash64String;
            }
        }

        private RName mName;
        [Editor.Editor_PackData()]
        [Rtti.MetaData]
        public RName Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
                UpdateHash64();
            }
            //get
            //{
            //    return RName.GetRName(SDK_GfxMaterial_GetName(CoreObject));
            //}
            //set
            //{
            //    SDK_GfxMaterial_SetName(CoreObject, value.Name);
            //}
        }
        UInt32 mVersion = 0;
        [Rtti.MetaData]
        public UInt32 Version
        {
            get => mVersion;
            private set => mVersion = value;
        }
        public void ForceUpdateVersion()
        {
            Version++;
        }

        public string[] GetShaderIncludes()
        {
            return new string[] 
            {
                Name.GetNameWithType() + ShaderIncludeExtension
            };
        }
        public string[] GetShaderDefines()
        {
            return new string[] { Name.GetNameWithType() + ShaderDefineExtension };
        }

        protected List<MtlMacro> mMtlMacroArray = new List<MtlMacro>();
        public List<MtlMacro> GetMtlMacroArray()
        {
            return mMtlMacroArray;
        }

        
        public List<MtlMacro> MtlMacroArray
        {
            get;
        }

        public bool NewMaterial(RName name)
        {
            Name = name;
            var reslut =(bool)SDK_GfxMaterial_Init(CoreObject, name.Name);
            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);
            return reslut;
        }


        #region MTL_IO
        public async System.Threading.Tasks.Task<bool> LoadMaterialAsync(CRenderContext rc, RName name)
        {
            Name = name;
            var retValue = await CEngine.Instance.EventPoster.Post(() =>
            {
                bool needSave = false;
                using (var xnd = IO.XndHolder.SyncLoadXND(name.Address))
                {
                    if (xnd == null)
                        return false;

                    var verAtt = xnd.Node.FindAttrib("saveVer");
                    if (verAtt == null)
                    {
                        // 旧的读取
                        mName.Name = xnd.Node.GetName();
                        var codeAttr = xnd.Node.FindAttrib("Code");
                        if (codeAttr != null)
                        {
                            codeAttr.BeginRead();
                            if (codeAttr.Version == 1)
                                codeAttr.Read(out mVersion);
                            string code;
                            codeAttr.Read(out code);
                            Code = code;
                            codeAttr.EndRead();
                        }

                        var varAttr = xnd.Node.FindAttrib("Vars");
                        if (varAttr != null)
                        {
                            varAttr.BeginRead();
                            UInt32 count;
                            varAttr.Read(out count);
                            for (UInt32 i = 0; i < count; i++)
                            {
                                string varName;
                                varAttr.Read(out varName);
                                UInt32 varType;
                                varAttr.Read(out varType);
                                UInt32 varElements;
                                varAttr.Read(out varElements);

                                var param = new CGfxMaterialParam(varName)
                                {
                                    VarType = (EShaderVarType)varType,
                                    ElementIdx = varElements,
                                };
                                ParamList.Add(param);
                            }
                            varAttr.EndRead();
                        }
                        var srvAttr = xnd.Node.FindAttrib("RSV");
                        if (srvAttr != null)
                        {
                            srvAttr.BeginRead();
                            UInt32 count;
                            srvAttr.Read(out count);
                            for (UInt32 i = 0; i < count; i++)
                            {
                                string varName;
                                srvAttr.Read(out varName);
                                UInt32 varElements;
                                srvAttr.Read(out varElements);

                                var param = new CGfxMaterialParam(varName)
                                {
                                    VarType = EShaderVarType.SVT_Texture,
                                    ElementIdx = varElements,
                                };
                                ParamList.Add(param);
                            }
                            srvAttr.EndRead();
                        }

                        var sampAttr = xnd.Node.FindAttrib("SamplerState");
                        if (sampAttr != null)
                        {
                            sampAttr.BeginRead();
                            UInt32 count;
                            sampAttr.Read(out count);
                            for (int i = 0; i < count; i++)
                            {
                                string sampName;
                                sampAttr.Read(out sampName);
                                var samp = new CSamplerStateDesc();
                                samp.SetDefault();
                                unsafe
                                {
                                    sampAttr.Read((IntPtr)(&samp), sizeof(CSamplerStateDesc));
                                }
                                if ((samp.AddressU < 0 || samp.AddressU > EAddressMode.ADM_MIRROR_ONCE) ||
                                    (samp.AddressV < 0 || samp.AddressV > EAddressMode.ADM_MIRROR_ONCE) ||
                                    (samp.AddressW < 0 || samp.AddressW > EAddressMode.ADM_MIRROR_ONCE))
                                {
                                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Material", $"Material {Name}: SamplerState is invalid");
                                    samp.SetDefault();
                                    needSave = true;
                                }
                                mSamplerStateDescs[sampName] = samp;
                            }
                            sampAttr.EndRead();
                        }

                        var attr = xnd.Node.FindAttrib("_matData");
                        if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game && attr != null)
                        {
                            attr.BeginRead();
                            attr.ReadMetaObject(this);
                            attr.EndRead();
                        }

                        attr = xnd.Node.FindAttrib("MtlMacros");
                        if (attr != null)
                        {
                            attr.BeginRead();
                            int Count = 0;
                            attr.Read(out Count);
                            for (int idx = 0; idx < Count; idx++)
                            {
                                string MacroName;
                                attr.Read(out MacroName);
                                string MacroValue;
                                attr.Read(out MacroValue);

                                MtlMacro mtl_macro = new MtlMacro();
                                mtl_macro.mMacroName = MacroName;
                                mtl_macro.mMacroValue = MacroValue;

                                mMtlMacroArray.Add(mtl_macro);
                            }
                            attr.EndRead();
                        }

                        var dataAttr = xnd.Node.FindAttrib("_matData");
                        if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game && dataAttr != null)
                        {
                            dataAttr.BeginRead();
                            dataAttr.ReadMetaObject(this);
                            dataAttr.EndRead();
                        }
                    }
                    else
                    {
                        verAtt.BeginRead();
                        int saveVer;
                        verAtt.Read(out saveVer);
                        verAtt.EndRead();

                        switch (saveVer)
                        {
                            case 0:
                                {
                                    var attr = xnd.Node.FindAttrib("_matData");
                                    if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game && attr != null)
                                    {
                                        attr.BeginRead();
                                        attr.ReadMetaObject(this);
                                        attr.EndRead();
                                    }

                                    mSamplerStateDescs.Clear();
                                    var sampAttr = xnd.Node.FindAttrib("_samplerState");
                                    if (sampAttr != null)
                                    {
                                        sampAttr.BeginRead();
                                        int sampCount = 0;
                                        sampAttr.Read(out sampCount);
                                        for (int i = 0; i < sampCount; i++)
                                        {
                                            string keyVal;
                                            sampAttr.Read(out keyVal);
                                            var samp = new CSamplerStateDesc();
                                            samp.SetDefault();
                                            unsafe
                                            {
                                                sampAttr.Read((IntPtr)(&samp), sizeof(CSamplerStateDesc));
                                            }
                                            if ((samp.AddressU < 0 || samp.AddressU > EAddressMode.ADM_MIRROR_ONCE) ||
                                                (samp.AddressV < 0 || samp.AddressV > EAddressMode.ADM_MIRROR_ONCE) ||
                                                (samp.AddressW < 0 || samp.AddressW > EAddressMode.ADM_MIRROR_ONCE))
                                            {
                                                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Material", $"Material {Name}: SamplerState is invalid");
                                                samp.SetDefault();
                                                needSave = true;
                                            }
                                            mSamplerStateDescs[keyVal] = samp;
                                        }
                                        sampAttr.EndRead();
                                    }

                                    attr = xnd.Node.FindAttrib("MtlMacros");
                                    if (attr != null)
                                    {
                                        attr.BeginRead();
                                        int Count = 0;
                                        attr.Read(out Count);
                                        for (int idx = 0; idx < Count; idx++)
                                        {
                                            string MacroName;
                                            attr.Read(out MacroName);
                                            string MacroValue;
                                            attr.Read(out MacroValue);

                                            MtlMacro mtl_macro = new MtlMacro();
                                            mtl_macro.mMacroName = MacroName;
                                            mtl_macro.mMacroValue = MacroValue;

                                            mMtlMacroArray.Add(mtl_macro);
                                        }
                                        attr.EndRead();
                                    }
                                }
                                break;
                        }
                    }
                }

                if (needSave || CEngineDesc.ForceSaveResource)
                {
                    this.SaveMaterial();
                }
                return true;
            });
            if(retValue)
            {
                EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);
                return true;
            }
            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
            EngineNS.Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Material", $"LoadMaterialAsync {name.Address} Failed");
            return false;
        }
        public void SaveMaterial()
        {
            SaveMaterial(Name.Address);
        }
        public void SaveMaterial(string absFileName)
        {
            Version++;
            var xnd = IO.XndHolder.NewXNDHolder();

            var verAttr = xnd.Node.AddAttrib("saveVer");
            verAttr.BeginWrite();
            verAttr.Write((int)0);
            verAttr.EndWrite();

            var attr = xnd.Node.AddAttrib("_matData");
            attr.BeginWrite();
            attr.WriteMetaObject(this);
            attr.EndWrite();

            var sampAttr = xnd.Node.AddAttrib("_samplerState");
            sampAttr.BeginWrite();
            sampAttr.Write((int)mSamplerStateDescs.Count);
            foreach(var samp in mSamplerStateDescs)
            {
                sampAttr.Write(samp.Key);
                unsafe
                {
                    var sampTemp = samp.Value;
                    sampAttr.Write((IntPtr)(&sampTemp), sizeof(CSamplerStateDesc));
                }
            }
            sampAttr.EndWrite();

            attr = xnd.Node.AddAttrib("MtlMacros");
            attr.BeginWrite();
            attr.Write(mMtlMacroArray.Count);
            for (int idx = 0; idx < mMtlMacroArray.Count; idx++)
            {
                attr.Write(mMtlMacroArray[idx].mMacroName);
                attr.Write(mMtlMacroArray[idx].mMacroValue);
            }
            attr.EndWrite();

            IO.XndHolder.SaveXND(absFileName, xnd);
        }
        #endregion


        Dictionary<string, CSamplerStateDesc> mSamplerStateDescs = new Dictionary<string, CSamplerStateDesc>();

        
        [Rtti.MetaData]
        public List<CGfxMaterialParam> ParamList
        {
            get;
            protected set;
        } = new List<CGfxMaterialParam>();
        public CGfxMaterialParam GetParam(string name)
        {
            foreach(var param in ParamList)
            {
                if (param.VarName == name)
                    return param;
            }
            return null;
        }

        public CGfxMaterialParam AddVar(string name, EShaderVarType type, UInt32 elements)
        {
            CGfxMaterialParam retParam = null;
            foreach(var param in ParamList)
            {
                if(param.VarName == name)
                {
                    retParam = param;
                    break;
                }
            }
            if(retParam == null)
            {
                retParam = new CGfxMaterialParam(name)
                {
                    VarType = type,
                    ElementIdx = elements,
                };
                ParamList.Add(retParam);
            }
            Version++;
            return retParam;
        }
        public void RemoveVar(string name)
        {
            foreach(var param in ParamList)
            {
                if(param.VarName == name)
                {
                    ParamList.Remove(param);
                    break;
                }
            }
            Version++;
        }
        public unsafe CGfxMaterialParam AddSRV(string name, CSamplerStateDesc samperDesc)
        {
            UInt32 element = 1;

            CGfxMaterialParam retValue = null;
            foreach (var param in ParamList)
            {
                if (param.VarName == name)
                {
                    retValue = param;
                    break;
                }
            }
            if (retValue == null)
            {
                retValue = new CGfxMaterialParam(name)
                {
                    VarType = EShaderVarType.SVT_Texture,
                    ElementIdx = element,
                };
                ParamList.Add(retValue);
            }

            mSamplerStateDescs[name] = samperDesc;

            Version++;
            return retValue;
        }
        public void RemoveSRV(string name)
        {
            foreach (var param in ParamList)
            {
                if (param.VarName == name)
                {
                    ParamList.Remove(param);
                    break;
                }
            }
            mSamplerStateDescs.Remove(name);
            Version++;
        }

        public void SetSamplerStateDesc(string name, CSamplerStateDesc desc)
        {
            mSamplerStateDescs[name] = desc;
        }
        public void GetSamplerStateDesc(string name, ref CSamplerStateDesc desc)
        {
            if(mSamplerStateDescs.ContainsKey(name))
                mSamplerStateDescs.TryGetValue(name, out desc);
        }
        public Dictionary<string, CSamplerStateDesc> GetSamplerStateDescs()
        {
            return mSamplerStateDescs;
        }

        [Rtti.MetaData]
        public string Code
        {
            get;
            private set;
        }
        public void UpdateCode(string code)
        {
            Code = code;
            Version++;
        }

        public void CopyTo(CGfxMaterial target)
        {
            target.ParamList = new List<CGfxMaterialParam>(this.ParamList);
            target.Version = Version;
            target.Code = Code;
            target.mSamplerStateDescs.Clear();
            foreach(var samp in mSamplerStateDescs)
            {
                target.mSamplerStateDescs[samp.Key] = samp.Value;
            }
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMaterial_Init(NativePointer self, string name);
        #endregion
    }

    public class CGfxMaterialManager
    {
        public Dictionary<RName, CGfxMaterial> Materials
        {
            get;
        } = new Dictionary<RName, CGfxMaterial>(new RName.EqualityComparer());
        CGfxMaterial mDefaultMaterial;
        public CGfxMaterial DefaultMaterial
        {
            get { return mDefaultMaterial; }
        }
        public async System.Threading.Tasks.Task<bool> Init(CRenderContext rc, RName dftMtl)
        {
            mDefaultMaterial = await GetMaterialAsync(rc, dftMtl);
            if (mDefaultMaterial == null)
                return true;
            return true;
        }
        public async System.Threading.Tasks.Task<CGfxMaterial> GetMaterialAsync(CRenderContext rc, RName name, bool force = false)
        {
            if (name.IsExtension(CEngineDesc.MaterialExtension) == false)
                return null;
            CGfxMaterial result;
            bool found = false;
            lock(Materials)
            {
                if (Materials.TryGetValue(name, out result) == false)
                {
                    if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor && !EngineNS.CEngine.Instance.FileManager.FileExists(name.Address))
                        return null;
                    result = new CGfxMaterial();
                    Materials.Add(name, result);
                }
                else
                    found = true;
            }
            if(found && !force)
            {
                var context = await result.AwaitLoad();
                if (context != null && context.Result == null)
                    return null;
                return result;
            }

            if (false == await result.LoadMaterialAsync(rc, name))
                return null;
            return result;
        }
        public async System.Threading.Tasks.Task<CGfxMaterial> NewMaterial(CRenderContext rc, RName name, RName sourceMtl)
        {
            if (name.IsExtension(CEngineDesc.MaterialExtension) == false)
                return null;
            if (sourceMtl.IsExtension(CEngineDesc.MaterialExtension) == false)
                return null;
            CGfxMaterial result;
            //if (Materials.TryGetValue(name, out result) == false)
            {
                result = new CGfxMaterial();
                if (false == await result.LoadMaterialAsync(rc, sourceMtl))
                    return null;
                result.Name = name;
                //Materials.Add(name, result);
            }
            return result;
        }
        public CGfxMaterial NewMaterial(RName name)
        {
            if (name.IsExtension(CEngineDesc.MaterialExtension) == false)
                return null;
            if (Materials.ContainsKey(name))
            {
                return null;
            }
            var result = new CGfxMaterial();
            if (false == result.NewMaterial(name))
                return null;
            lock (Materials)
            {
                Materials.Add(name, result);
            }
            return result;
        }

#region ShaderVarAssist
        public static string ValueNamePreString
        {
            get { return "ShaderVar_"; }
        }
        public static string GetValidShaderVarName(string name, string post)
        {
            if (name.IndexOf(ValueNamePreString) < 0)
                return ValueNamePreString + name + "_" + post;
            else
                return name;
        }
        public static string GetPureShaderVarName(string name, string pos)
        {
            if (name.IndexOf(ValueNamePreString) < 0)
                return name;
            else
            {
                var retVal = name.Substring(ValueNamePreString.Length);
                var idx = retVal.IndexOf(pos);
                if(idx >= 0)
                {
                    retVal = retVal.Substring(0, idx - 1);
                }
                return retVal;
            }
        }


#endregion
    }

    [Rtti.MetaClassAttribute()]
    public class CGfxMaterialInstance : AuxCoreObject<CGfxMaterialInstance.NativePointer>, IO.IResourceFile, INotifyPropertyChanged
    {
#region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
#endregion

        EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task<Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        {
            return await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
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

        public CGfxMaterialInstance()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxMaterialInstance");
        }

        public void SetDataToMaterialInstance(CGfxMaterialInstance target)
        {
            bool needReNewMatIns = false;
            if(MaterialName != target.MaterialName)
            {
                needReNewMatIns = true;
            }
            else
            {
                for(int i=0; i<target.mSRViews.Count; i++)
                {
                    bool find = false;
                    for (int pIdx = 0; pIdx < mMaterial.ParamList.Count; pIdx++)
                    {
                        var param = mMaterial.ParamList[pIdx];
                        if (target.mSRViews[i].ShaderName == param.VarName)
                        {
                            find = true;
                            break;
                        }
                    }

                    if (find == false)
                    {
                        needReNewMatIns = true;
                        break;
                    }
                }
                if(needReNewMatIns == false)
                {
                    for (int i = 0; i < target.mVars.Count; i++)
                    {
                        bool find = false;
                        for (int pIdx = 0; pIdx < mMaterial.ParamList.Count; pIdx++)
                        {
                            var param = mMaterial.ParamList[pIdx];
                            if (target.mVars[i].Definition.Name == param.VarName)
                            {
                                find = true;
                                break;
                            }
                        }
                        if (find == false)
                        {
                            needReNewMatIns = true;
                            break;
                        }
                    }
                }
            }
            if(needReNewMatIns)
                target.NewMaterialInstance(EngineNS.CEngine.Instance.RenderContext, Material, target.Name);

            for (UInt32 i = 0; i < SRVNumber; i++)
            {
                var varName = GetSRVShaderName(i, false);
                var idx = target.FindSRVIndex(varName, false);
                if ((int)idx == -1)
                {
                    continue;
                }
                var srv = GetSRV(i);
                target.SetSRV(idx, srv);
            }
            EngineNS.Graphics.CGfxVar var = new EngineNS.Graphics.CGfxVar();
            EngineNS.Graphics.CGfxVar varTarget = new EngineNS.Graphics.CGfxVar();
            for (UInt32 i = 0; i < VarNumber; i++)
            {   
                var varName = GetVarName(i, false);
                var idx = target.FindVarIndex(varName, false);
                if ((int)idx == -1)
                {
                    continue;
                }
                GetVarDesc(i, ref var);
                target.GetVarDesc(idx, ref varTarget);
                if (var.Type != varTarget.Type || var.Elements != varTarget.Elements)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "MaterialInstance", $"SetDataToMaterialInstance Var({varName})'s type is changed!");
                    continue;
                }
                switch (var.Type)
                {
                    case EngineNS.EShaderVarType.SVT_Float1:
                        {
                            float value = 0;
                            for (UInt32 j = 0; j < var.Elements; j++)
                            {
                                GetVarValue(i, j, ref value);
                                target.SetVarValue(idx, j, ref value);
                            }
                        }
                        break;
                    case EngineNS.EShaderVarType.SVT_Float2:
                        {
                            var value = new EngineNS.Vector2();
                            for (UInt32 j = 0; j < var.Elements; j++)
                            {
                                GetVarValue(i, j, ref value);
                                target.SetVarValue(idx, j, ref value);
                            }
                        }
                        break;
                    case EngineNS.EShaderVarType.SVT_Float3:
                        {
                            var value = new EngineNS.Vector3();
                            for (UInt32 j = 0; j < var.Elements; j++)
                            {
                                GetVarValue(i, j, ref value);
                                target.SetVarValue(idx, j, ref value);
                            }
                        }
                        break;
                    case EngineNS.EShaderVarType.SVT_Float4:
                        {
                            var value = new EngineNS.Vector4();
                            for (UInt32 j=0; j<var.Elements; j++)
                            {
                                GetVarValue(i, j, ref value);
                                target.SetVarValue(idx, j, ref value);
                            }
                        }
                        break;
                    case EngineNS.EShaderVarType.SVT_Matrix4x4:
                        {
                            var value = new EngineNS.Matrix();
                            for (UInt32 j = 0; j < var.Elements; j++)
                            {
                                GetVarValue(i, j, ref value);
                                target.SetVarValue(idx, j, ref value);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            target.CustomBlendState = CustomBlendState;
            target.CustomDepthStencilState = CustomDepthStencilState;
            target.CustomRasterizerState = mCustomRasterizerState;

            target.mRenderLayer = mRenderLayer;
        }
        public void SetCBufferVars(CConstantBuffer cb)
        {
            if (cb == null)
                return;
            for (UInt32 i = 0; i < VarNumber; i++)
            {
                var name = GetVarName(i, false);
                var varIdx = cb.FindVar(name);

                CGfxVar varDesc = new CGfxVar();
                GetVarDesc(i, ref varDesc);
                ConstantVarDesc varDesc2 = new ConstantVarDesc();
                var ret = cb.GetVarDesc(varIdx, ref varDesc2);
                if (false == ret)
                    continue;

                if(varDesc.Type == EShaderVarType.SVT_Unknown)
                {
                    //var setter = new Macross.MacrossGetter<McMaterialVarSetter>();
                    //Read MC RName from material
                    //setter.Name = RName.GetRName(name);
                    //setter.Get().OnSetVar(varIdx, varDesc2, cb);
                    continue;
                }

                if (varDesc.Type != varDesc2.Type || varDesc.Elements != varDesc2.Elements)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "ShaderVar", $"MaterialInstance Var {varDesc.Type} don't match CBuffer Var {varDesc2.Type}");
                    continue;
                }

                for (UInt32 j = 0; j < varDesc.Elements; j++)
                {
                    switch (varDesc.Type)
                    {
                        case EShaderVarType.SVT_Float1:
                            {
                                float value = 0;
                                GetVarValue(i, j, ref value);
                                cb.SetValue(varIdx, value, j);
                            }
                            break;
                        case EShaderVarType.SVT_Float2:
                            {
                                Vector2 value = new Vector2();
                                GetVarValue(i, j, ref value);
                                cb.SetValue(varIdx, value, j);
                            }
                            break;
                        case EShaderVarType.SVT_Float3:
                            {
                                Vector3 value = new Vector3();
                                GetVarValue(i, j, ref value);
                                cb.SetValue(varIdx, value, j);
                            }
                            break;
                        case EShaderVarType.SVT_Float4:
                            {
                                Vector4 value = new Vector4();
                                GetVarValue(i, j, ref value);
                                cb.SetValue(varIdx, value, j);
                            }
                            break;
                        case EShaderVarType.SVT_Matrix4x4:
                            {
                                Matrix value = new Matrix();
                                GetVarValue(i, j, ref value);
                                cb.SetValue(varIdx, value, j);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        RName mName;
        [Editor.Editor_PackData()]
        [Rtti.MetaData]
        public RName Name
        {
            get => mName;
            protected set => mName = value;
        }
        UInt32 mVersion = 0;
        [Rtti.MetaData]
        public UInt32 Version
        {
            get => mVersion;
            protected set => mVersion = value;
        }
        public void ForceUpdateVersion()
        {
            mVersion++;
        }
        RName mMaterialName;
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.Material)]
        [Editor.Editor_PackData()]
        [Rtti.MetaData]
        public RName MaterialName
        {
            get => mMaterialName;
            protected set => mMaterialName = value;
		}
        public void OnlySetMaterialName(RName rName)
        {
            mMaterialName = rName;
        }

        protected CGfxMaterial mMaterial;
        
        public CGfxMaterial Material
        {
            get { return mMaterial; }
        }

        public void Editor_SetMaterial(CGfxMaterial material)
        {
            SetMaterial(material, false);
        }
        bool SetMaterial(CGfxMaterial material, bool doClear)
        {
            mMaterial = material;
            mVersion = material.Version;
            mMaterialName = material.Name;

            if(doClear)
            {
                mVars.Clear();
                mSRViews.Clear();
                //mSamplerStateDescArray.Clear();
            }

            for(int i=0; i<material.ParamList.Count; i++)
            {
                var param = material.ParamList[i];
                switch(param.VarType)
                {
                    case EShaderVarType.SVT_Texture:
                    case EShaderVarType.SVT_Sampler:
                        {
                            var srv = FindSRV(param.VarName);
                            if(srv == null)
                            {
                                var temp = new SRVParam();
                                temp.ShaderName = param.VarName;
                                mSRViews.Add(temp);
                            }

                            //CSamplerStateDesc desc = new CSamplerStateDesc();
                            //desc.SetDefault();
                            //material.GetSamplerStateDesc(param.VarName, ref desc);
                            //mSamplerStateDescArray[param.VarName] = desc;
                        }
                        break;
                    default:
                        {
                            var p = FindVar(param.VarName);
                            if(p == null)
                            {
                                p = new CGfxVarValue();
                                p.Definition = new CGfxVar()
                                {
                                    Type = param.VarType,
                                    Elements = param.ElementIdx,
                                    Name = param.VarName,
                                };
                                mVars.Add(p);
                                p.ValueArray = new byte[param.ElementIdx * p.GetValueSize(param.VarType)];
                            }
                            else
                            {
                                if(p.Definition.Name != param.VarName ||
                                   p.Definition.Type != param.VarType ||
                                   p.Definition.Elements != param.ElementIdx)
                                {
                                    p.ValueArray = new byte[param.ElementIdx * p.GetValueSize(param.VarType)];
                                }
                            }
                        }
                        break;
                }
            }

            return true;
        }
        //其实这不需要这个，本身就能存好，这里做一个example而已
        private class ChannelSerializer : IO.Serializer.CustomSerializer
        {
            public override object ReadValue(IO.Serializer.IReader pkg)
            {
                sbyte value;
                pkg.Read(out value);
                return (Graphics.View.ERenderLayer)value;
            }
            public override void WriteValue(object obj, IO.Serializer.IWriter pkg)
            {
                sbyte value = (sbyte)((Graphics.View.ERenderLayer)obj);
                pkg.Write(value);
            }
        }
        [Rtti.MetaDataAttribute()]
        [IO.Serializer.CustomFieldSerializer(typeof(ChannelSerializer))]
        public Graphics.View.ERenderLayer mRenderLayer
        {
            get;
            set;
        } = Graphics.View.ERenderLayer.RL_Opaque;

        protected CRasterizerState mCustomRasterizerState;
        [Category("RHIState")]
        public CRasterizerState CustomRasterizerState
        {
            get { return mCustomRasterizerState; }
            set
            {
                mCustomRasterizerState = value;
            }
        }

        public CRasterizerState mShadowRasterState;
        private static int mDepthBias = 1;
        private static float mSlopeScaledDepthBias = 4.0f;


        protected CDepthStencilState mCustomDepthStencilState;
        [Category("RHIState")]
        public CDepthStencilState CustomDepthStencilState
        {
            get { return mCustomDepthStencilState; }
            set
            {
                mCustomDepthStencilState = value;
            }
        }
        
        protected CBlendState mCustomBlendState;
        [Category("RHIState")]
        public CBlendState CustomBlendState
        {
            get { return mCustomBlendState; }
            set
            {
                mCustomBlendState = value;
                OnPropertyChanged("BlendState");
            }
        }

        [Editor.Editor_PackData()]
        public string[] TextureNames
        {
            get
            {
                string[] names = new string[SRVNumber];
                for (UInt32 i = 0; i < SRVNumber; i++)
                {
                    var srvName = GetSRVName(i);
                    names[i] = srvName.Address;
                }

                return names;
            }
        }
        
        public bool NewMaterialInstance(CRenderContext RHICtx, CGfxMaterial material, RName name)
        {
            mName = name;
            mMaterial = material;
            if(SetMaterial(material, true) == false)
                return false;

            mVersion = material.Version;
            mMaterialName = material.Name;

            CRasterizerStateDesc RSDescCustom = new CRasterizerStateDesc();
            RSDescCustom.InitForCustom();
            mCustomRasterizerState = CEngine.Instance.RasterizerStateManager.GetRasterizerState(RHICtx, RSDescCustom);

            CRasterizerStateDesc RSDescShadow = new CRasterizerStateDesc();
            RSDescShadow.InitForCustom();
            RSDescShadow.DepthBias = mDepthBias;
            RSDescShadow.SlopeScaledDepthBias = mSlopeScaledDepthBias;
            mShadowRasterState = CEngine.Instance.RasterizerStateManager.GetRasterizerState(RHICtx, RSDescShadow);

            //only the custom depth stencil stat needs to do serialization;
            CDepthStencilStateDesc CustomDSSDesc = new CDepthStencilStateDesc();
            CustomDSSDesc.InitForCustomLayers();
            mCustomDepthStencilState = CEngine.Instance.DepthStencilStateManager.GetDepthStencilState(RHICtx, CustomDSSDesc);
            
            CBlendStateDesc CustomBlendDesc = new CBlendStateDesc();
            CustomBlendDesc.InitForCustomLayers();
            CustomBlendState = CEngine.Instance.BlendStateManager.GetBlendState(RHICtx, CustomBlendDesc);

            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);
            return true;
        }
        public async System.Threading.Tasks.Task<bool> PureLoadMaterialInstanceAsync(CRenderContext RHICtx, RName name)
        {
            var loadOk = await CEngine.Instance.EventPoster.Post(() =>
            {
                using (var xnd = IO.XndHolder.SyncLoadXND(name.Address))
                {
                    if (xnd == null)
                        return false;

                    var varAttr = xnd.Node.FindAttrib("saveVer");
                    if (varAttr == null)
                    {
                        var cVarAttr = xnd.Node.FindAttrib("Vars");
                        if (cVarAttr != null)
                        {
                            cVarAttr.BeginRead();
                            cVarAttr.Read(out mVersion);
                            string materialName;
                            cVarAttr.Read(out materialName);
                            mMaterialName = EngineNS.RName.GetRName(materialName);
                            UInt32 count = 0;
                            cVarAttr.Read(out count);
                            mVars.Clear();
                            for (UInt32 i = 0; i < count; i++)
                            {
                                var p = new CGfxVarValue();
                                string varName;
                                cVarAttr.Read(out varName);
                                UInt32 varType;
                                cVarAttr.Read(out varType);
                                UInt32 elements;
                                cVarAttr.Read(out elements);
                                p.Definition = new CGfxVar()
                                {
                                    Name = varName,
                                    Type = (EShaderVarType)varType,
                                    Elements = elements,
                                };
                                var size = (int)(p.Definition.Elements * p.GetValueSize(p.Definition.Type));
                                byte[] valArray;
                                cVarAttr.Read(out valArray, size);
                                p.ValueArray = valArray;
                                System.Diagnostics.Debug.Assert(FindVar(p.Definition.Name) == null);
                                mVars.Add(p);
                            }
                            cVarAttr.EndRead();
                        }

                        mSRViews.Clear();
                        var cSRVAttr = xnd.Node.FindAttrib("RSV");
                        if (cSRVAttr != null)
                        {
                            cSRVAttr.BeginRead();
                            UInt32 count;
                            cSRVAttr.Read(out count);
                            for (UInt32 i = 0; i < count; i++)
                            {
                                var temp = new SRVParam();
                                cSRVAttr.Read(out temp.ShaderName);
                                string rName;
                                cSRVAttr.Read(out rName);
                                temp.RName = EngineNS.RName.GetRName(rName);
                                System.Diagnostics.Debug.Assert(FindSRV(temp.ShaderName) == null);
                                mSRViews.Add(temp);
                            }
                            cSRVAttr.EndRead();
                        }

                        var attr = xnd.Node.FindAttrib("ObjectData");
                        if (attr != null)
                        {
                            attr.BeginRead();
                            attr.ReadMetaObject(this);
                            attr.EndRead();
                        }

                        unsafe
                        {
                            var rStateAttr = xnd.Node.FindAttrib("RasterizerState");
                            if (rStateAttr != null)
                            {
                                rStateAttr.BeginRead();
                                CRasterizerStateDesc desc;
                                rStateAttr.Read((IntPtr)(&desc), sizeof(CRasterizerStateDesc));
                                mCustomRasterizerState = CEngine.Instance.RasterizerStateManager.GetRasterizerState(RHICtx, desc);
                                desc.DepthBias = mDepthBias;
                                desc.SlopeScaledDepthBias = mSlopeScaledDepthBias;
                                mShadowRasterState = CEngine.Instance.RasterizerStateManager.GetRasterizerState(RHICtx, desc);
                                rStateAttr.EndRead();
                            }

                            var dStateAttr = xnd.Node.FindAttrib("DepthStencilState");
                            if (dStateAttr != null)
                            {
                                dStateAttr.BeginRead();
                                CDepthStencilStateDesc desc;
                                dStateAttr.Read((IntPtr)(&desc), sizeof(CDepthStencilStateDesc));
                                mCustomDepthStencilState = CEngine.Instance.DepthStencilStateManager.GetDepthStencilState(RHICtx, desc);
                                dStateAttr.EndRead();
                            }

                            var bStateAttr = xnd.Node.FindAttrib("BlendState");
                            if (bStateAttr != null)
                            {
                                bStateAttr.BeginRead();
                                CBlendStateDesc desc;
                                bStateAttr.Read((IntPtr)(&desc), sizeof(CBlendStateDesc));
                                mCustomBlendState = CEngine.Instance.BlendStateManager.GetBlendState(RHICtx, desc);
                                bStateAttr.EndRead();
                            }
                        }

                        return true;
                    }
                    else
                    {
                        varAttr.BeginRead();
                        int saveVer;
                        varAttr.Read(out saveVer);
                        varAttr.EndRead();

                        switch (saveVer)
                        {
                            case 0:
                                {
                                    var attr = xnd.Node.FindAttrib("ObjectData");
                                    if (attr != null)
                                    {
                                        attr.BeginRead();
                                        attr.ReadMetaObject(this);
                                        attr.EndRead();
                                    }
                                    //else
                                    //{//test code
                                    //    SaveMaterialInstance();
                                    //}

                                    unsafe
                                    {
                                        var rStateAttr = xnd.Node.FindAttrib("RasterizerState");
                                        if (rStateAttr != null)
                                        {
                                            rStateAttr.BeginRead();
                                            CRasterizerStateDesc desc;
                                            rStateAttr.Read((IntPtr)(&desc), sizeof(CRasterizerStateDesc));
                                            mCustomRasterizerState = CEngine.Instance.RasterizerStateManager.GetRasterizerState(RHICtx, desc);
                                            desc.DepthBias = mDepthBias;
                                            desc.SlopeScaledDepthBias = mSlopeScaledDepthBias;
                                            mShadowRasterState = CEngine.Instance.RasterizerStateManager.GetRasterizerState(RHICtx, desc);
                                            rStateAttr.EndRead();
                                        }

                                        var dStateAttr = xnd.Node.FindAttrib("DepthStencilState");
                                        if (dStateAttr != null)
                                        {
                                            dStateAttr.BeginRead();
                                            CDepthStencilStateDesc desc;
                                            dStateAttr.Read((IntPtr)(&desc), sizeof(CDepthStencilStateDesc));
                                            mCustomDepthStencilState = CEngine.Instance.DepthStencilStateManager.GetDepthStencilState(RHICtx, desc);
                                            dStateAttr.EndRead();
                                        }

                                        var bStateAttr = xnd.Node.FindAttrib("BlendState");
                                        if (bStateAttr != null)
                                        {
                                            bStateAttr.BeginRead();
                                            CBlendStateDesc desc;
                                            bStateAttr.Read((IntPtr)(&desc), sizeof(CBlendStateDesc));
                                            mCustomBlendState = CEngine.Instance.BlendStateManager.GetBlendState(RHICtx, desc);
                                            bStateAttr.EndRead();
                                        }
                                    }
                                }
                                break;
                        }

                        return true;
                    }
                }
            });
            if (false == loadOk)
            {
                EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
                EngineNS.Profiler.Log.WriteLine(Profiler.ELogTag.Error, "MaterialInstance", $"LoadMaterialInstanceAsync {name.Address} Failed");
                return false;
            }
            return true;
        }
        public async System.Threading.Tasks.Task<bool> LoadMaterialInstanceAsync(CRenderContext RHICtx, RName name)
        {
            mName = name;
            //IO.XndHolder xnd = null;

            var result = await PureLoadMaterialInstanceAsync(RHICtx, name);
            if (result == false)
                return false;

            for (UInt32 i = 0; i < SRVNumber; i++)
            {
                var srvName = GetSRVName(i);
                var srv = CEngine.Instance.TextureManager.GetShaderRView(RHICtx, srvName);
                if (srv != null)
                    SetSRV(i, srv);
            }

            mMaterial = await CEngine.Instance.MaterialManager.GetMaterialAsync(RHICtx, MaterialName);
            if (mMaterial == null)
            {
                EngineNS.Profiler.Log.WriteLine(Profiler.ELogTag.Error, "MaterialInstance", $"LoadMaterialInstanceAsync {name.Address} GetMaterialAsync {MaterialName.Address} Failed");
                mMaterial = CEngine.Instance.MaterialManager.DefaultMaterial;
                //return false;
            }

            if (false == SetMaterial(mMaterial, false))
            {
                EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
                EngineNS.Profiler.Log.WriteLine(Profiler.ELogTag.Error, "MaterialInstance", $"LoadMaterialInstanceAsync {name.Address} SetMaterial Failed");
                return false;
            }

            if (CEngineDesc.ForceSaveResource)
            {
                this.SaveMaterialInstance();
            }
            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);
            return true;
        }
        public void SaveMaterialInstance()
        {
            SaveMaterialInstance(Name.Address);
        }
        public void SaveMaterialInstance(string absFileName)
        {
            var xnd = IO.XndHolder.NewXNDHolder();

            mVersion++;

            var varAttr = xnd.Node.AddAttrib("saveVer");
            varAttr.BeginWrite();
            varAttr.Write((int)0);
            varAttr.EndWrite();

            var attr = xnd.Node.AddAttrib("ObjectData");
            attr.BeginWrite();
            attr.WriteMetaObject(this);
            attr.EndWrite();

            unsafe
            {
                if (mCustomRasterizerState != null)
                {
                    var stateAttr = xnd.Node.AddAttrib("RasterizerState");
                    stateAttr.BeginWrite();
                    var desc = mCustomRasterizerState.Desc;
                    stateAttr.Write((IntPtr)(&desc), sizeof(CRasterizerStateDesc));
                    stateAttr.EndWrite();
                }
                if(mCustomDepthStencilState != null)
                {
                    var stateAttr = xnd.Node.AddAttrib("DepthStencilState");
                    stateAttr.BeginWrite();
                    var desc = mCustomDepthStencilState.Desc;
                    stateAttr.Write((IntPtr)(&desc), sizeof(CDepthStencilStateDesc));
                    stateAttr.EndWrite();
                }
                if(mCustomBlendState != null)
                {
                    var stateAttr = xnd.Node.AddAttrib("BlendState");
                    stateAttr.BeginWrite();
                    var desc = mCustomBlendState.Desc;
                    stateAttr.Write((IntPtr)(&desc), sizeof(CBlendStateDesc));
                    stateAttr.EndWrite();
                }
            }

            IO.XndHolder.SaveXND(absFileName, xnd);
        }

        public bool GetVarDesc(UInt32 index, ref CGfxVar definition)
        {
            if (index >= (UInt32)mVars.Count)
                return false;

            var param = mVars[(int)index];
            definition.Type = param.Definition.Type;
            definition.Elements = param.Definition.Elements;
            definition.Name = param.Definition.Name;
            return true;
        }

        List<CGfxVarValue> mVars = new List<CGfxVarValue>();
        [Rtti.MetaData]
        public List<CGfxVarValue> Vars
        {
            get => mVars;
            protected set => mVars = value;
        }
        CGfxVarValue FindVar(string name)
        {
            for (int i=0; i<mVars.Count; i++)
            {
                if (mVars[i].Definition.Name == name)
                    return mVars[i];
            }
            return null;
        }
        bool AddVar(string name, EShaderVarType type, UInt32 elements)
        {
            var sVar = FindVar(name);
            if(sVar == null)
            {
                var p = new CGfxVarValue();
                p.Definition = new CGfxVar()
                {
                    Name = name,
                    Type = type,
                    Elements = elements,
                };
                p.ValueArray = new byte[elements * p.GetValueSize(type)];
                mVars.Add(p);
            }
            return true;
        }
        bool RemoveVar(UInt32 index)
        {
            if (index >= (UInt32)mVars.Count)
                return false;

            mVars.RemoveAt((int)index);
            return true;
        }
        public UInt32 VarNumber
        {
            get => (UInt32)mVars.Count;
        }
        public string GetVarName(UInt32 index, bool pureName)
        {
            if (index >= (UInt32)mVars.Count)
                return "";
            var retVal = mVars[(int)index].Definition.Name;
            if (retVal == null)
                return "";
            if(pureName)
                return CGfxMaterialManager.GetPureShaderVarName(retVal, Material.GetHash64().ToString());
            return retVal;
        }
        public UInt32 FindVarIndex(string name, bool nameFix = true)
        {
            if(nameFix)
                name = CGfxMaterialManager.GetValidShaderVarName(name, Material.GetHash64().ToString());
            for(int i=0; i<mVars.Count; i++)
            {
                if (mVars[i].Definition.Name == name)
                    return (UInt32)i;
            }
            return UInt32.MaxValue;
        }
        //下面Set/GetVarValue不会做类型安全判断，用户自己用GetVarDesc提前处理
        public bool GetVarValue(UInt32 index, UInt32 elementIndex, ref float value)
        {
            if (index >= (UInt32)mVars.Count)
                return false;

            var param = mVars[(int)index];
            if (elementIndex >= param.Definition.Elements)
                return false;
            if (param.Definition.Type != EShaderVarType.SVT_Float1 &&
                param.Definition.Type != EShaderVarType.SVT_Int1)
                return false;
            unsafe
            {
                fixed(byte* valP = &param.ValueArray[0])
                fixed (float* p = &value)
                {
                    var size = (UInt32)param.GetValueSize(param.Definition.Type);
                    var ptr = valP + size * elementIndex;
                    EngineNS.CoreSDK.SDK_Memory_Copy(p, ptr, size);
                    return true;
                }
            }
        }
        public bool GetVarValue(UInt32 index, UInt32 elementIndex, ref Vector2 value)
        {
            if (index >= (UInt32)mVars.Count)
                return false;

            var param = mVars[(int)index];
            if (elementIndex >= param.Definition.Elements)
                return false;
            if (param.Definition.Type != EShaderVarType.SVT_Float2 &&
                param.Definition.Type != EShaderVarType.SVT_Int2)
                return false;
            unsafe
            {
                fixed(byte* valP = &param.ValueArray[0])
                fixed (Vector2* p = &value)
                {
                    var size = (UInt32)param.GetValueSize(param.Definition.Type);
                    var ptr = valP + size * elementIndex;
                    EngineNS.CoreSDK.SDK_Memory_Copy(p, ptr, size);
                    return true;
                }
            }
        }
        public bool GetVarValue(UInt32 index, UInt32 elementIndex, ref Vector3 value)
        {
            if (index >= (UInt32)mVars.Count)
                return false;

            var param = mVars[(int)index];
            if (elementIndex >= param.Definition.Elements)
                return false;
            if (param.Definition.Type != EShaderVarType.SVT_Float3 &&
                param.Definition.Type != EShaderVarType.SVT_Int3)
                return false;
            unsafe
            {
                fixed(byte* valP = &param.ValueArray[0])
                fixed (Vector3* p = &value)
                {
                    var size = (UInt32)param.GetValueSize(param.Definition.Type);
                    var ptr = valP + size * elementIndex;
                    EngineNS.CoreSDK.SDK_Memory_Copy(p, ptr, size);
                    return true;
                }
            }
        }
        public bool GetVarValue(UInt32 index, UInt32 elementIndex, ref Vector4 value)
        {
            if (index >= (UInt32)mVars.Count)
                return false;

            var param = mVars[(int)index];
            if (elementIndex >= param.Definition.Elements)
                return false;
            if (param.Definition.Type != EShaderVarType.SVT_Float4 &&
                param.Definition.Type != EShaderVarType.SVT_Int4)
                return false;
            unsafe
            {
                fixed (byte* valP = &param.ValueArray[0])
                fixed (Vector4* p = &value)
                {
                    var size = (UInt32)param.GetValueSize(param.Definition.Type);
                    var ptr = valP + size * elementIndex;
                    EngineNS.CoreSDK.SDK_Memory_Copy(p, ptr, size);
                    return true;
                }
            }
        }
        public bool GetVarValue(UInt32 index, UInt32 elementIndex, ref Matrix value)
        {
            if (index >= (UInt32)mVars.Count)
                return false;

            var param = mVars[(int)index];
            if (elementIndex >= param.Definition.Elements)
                return false;
            if (param.Definition.Type != EShaderVarType.SVT_Matrix4x4)
                return false;
            unsafe
            {
                fixed (byte* valP = &param.ValueArray[0])
                fixed (Matrix* p = &value)
                {
                    var size = (UInt32)param.GetValueSize(param.Definition.Type);
                    var ptr = valP + size * elementIndex;
                    EngineNS.CoreSDK.SDK_Memory_Copy(p, ptr, size);
                    return true;
                }
            }
        }

        public bool SetVarValue(UInt32 index, UInt32 elementIndex, ref float value)
        {
            if (index >= (UInt32)mVars.Count)
                return false;
            var param = mVars[(int)index];
            if (elementIndex >= param.Definition.Elements)
                return false;
            if (param.Definition.Type != EShaderVarType.SVT_Float1 &&
               param.Definition.Type != EShaderVarType.SVT_Int1)
                return false;
            unsafe
            {
                fixed(byte* valP = &param.ValueArray[0])
                fixed (float* p = &value)
                {
                    var size = (UInt32)param.GetValueSize(param.Definition.Type);
                    var ptr = valP + size * elementIndex;
                    EngineNS.CoreSDK.SDK_Memory_Copy(ptr, p, size);
                    return true;
                }
            }
        }
        public bool SetVarValue(UInt32 index, UInt32 elementIndex, ref Vector2 value)
        {
            if (index >= (UInt32)mVars.Count)
                return false;
            var param = mVars[(int)index];
            if (elementIndex >= param.Definition.Elements)
                return false;
            if (param.Definition.Type != EShaderVarType.SVT_Float2 &&
                param.Definition.Type != EShaderVarType.SVT_Int2)
                return false;
            unsafe
            {
                fixed(byte* valP = &param.ValueArray[0])
                fixed (Vector2* p = &value)
                {
                    var size = (UInt32)param.GetValueSize(param.Definition.Type);
                    var ptr = valP + size * elementIndex;
                    EngineNS.CoreSDK.SDK_Memory_Copy(ptr, p, size);
                    return true;
                }
            }
        }
        public bool SetVarValue(UInt32 index, UInt32 elementIndex, ref Vector3 value)
        {
            if (index >= (UInt32)mVars.Count)
                return false;
            var param = mVars[(int)index];
            if (elementIndex >= param.Definition.Elements)
                return false;
            if (param.Definition.Type != EShaderVarType.SVT_Float3 &&
                param.Definition.Type != EShaderVarType.SVT_Int3)
                return false;
            unsafe
            {
                fixed (byte* valP = &param.ValueArray[0])
                fixed (Vector3* p = &value)
                {
                    var size = (UInt32)param.GetValueSize(param.Definition.Type);
                    var ptr = valP + size * elementIndex;
                    EngineNS.CoreSDK.SDK_Memory_Copy(ptr, p, size);
                    return true;
                }
            }
        }
        public bool SetVarValue(UInt32 index, UInt32 elementIndex, ref Vector4 value)
        {
            if (index >= (UInt32)mVars.Count)
                return false;
            var param = mVars[(int)index];
            if (elementIndex >= param.Definition.Elements)
                return false;
            if (param.Definition.Type != EShaderVarType.SVT_Float4 &&
                param.Definition.Type != EShaderVarType.SVT_Int4)
                return false;
            unsafe
            {
                fixed (byte* valP = &param.ValueArray[0])
                fixed (Vector4* p = &value)
                {
                    var size = (UInt32)param.GetValueSize(param.Definition.Type);
                    var ptr = valP + size * elementIndex;
                    EngineNS.CoreSDK.SDK_Memory_Copy(ptr, p, size);
                    return true;
                }
            }
        }
        public bool SetVarValue(UInt32 index, UInt32 elementIndex, ref Matrix value)
        {
            if (index >= (UInt32)mVars.Count)
                return false;
            var param = mVars[(int)index];
            if (elementIndex >= param.Definition.Elements)
                return false;
            if (param.Definition.Type != EShaderVarType.SVT_Matrix4x4)
                return false;
            unsafe
            {
                fixed (byte* valP = &param.ValueArray[0])
                fixed (Matrix* p = &value)
                {
                    var size = (UInt32)param.GetValueSize(param.Definition.Type);
                    var ptr = valP + size * elementIndex;
                    EngineNS.CoreSDK.SDK_Memory_Copy(ptr, p, size);
                    return true;
                }
            }
        }

        List<SRVParam> mSRViews = new List<SRVParam>();
        [Rtti.MetaData]
        public List<SRVParam> SRViews
        {
            get => mSRViews;
            protected set => mSRViews = value;
        }
        public UInt32 FindSRVIndex(string name, bool nameFix = true)
        {
            if (nameFix)
                name = CGfxMaterialManager.GetValidShaderVarName(name, Material.GetHash64().ToString());
            for(int i=0; i<mSRViews.Count; i++)
            {
                if (mSRViews[i].ShaderName == name)
                    return (UInt32)i;
            }
            return UInt32.MaxValue;
        }
        public UInt32 SRVNumber
        {
            get
            {
                return (UInt32)mSRViews.Count;
            }
        }
        SRVParam FindSRV(string name)
        {
            for(int i=0; i<mSRViews.Count; i++)
            {
                if (mSRViews[i].ShaderName == name)
                    return mSRViews[i];
            }
            return null;
        }
        bool AddSRV(string name)
        {
            var srv = FindSRV(name);
            if(srv == null)
            {
                srv = new SRVParam()
                {
                    ShaderName = name,
                };
                mSRViews.Add(srv);
            }
            return true;
        }
        bool RemoveSRV(UInt32 index)
        {
            if (index >= (UInt32)mSRViews.Count)
                return false;
            var i = mSRViews[(int)index];
            mSRViews.RemoveAt((int)index);
            i.Cleanup();
            return true;
        }
        public void SetSRV(UInt32 index, CShaderResourceView rsv)
        {
            if (index >= mSRViews.Count)
                return;

            mSRViews[(int)index].RSView = rsv;
            if (rsv != null)
                mSRViews[(int)index].RName = rsv.Name;
            else
                mSRViews[(int)index].RName = EngineNS.RName.EmptyName;
        }
        public CShaderResourceView GetSRV(UInt32 index)
        {
            if (index > mSRViews.Count)
                return null;
            return mSRViews[(int)index].RSView;
        }
        public RName GetSRVName(UInt32 index)
        {
            if (index >= mSRViews.Count)
                return RName.EmptyName;
            return mSRViews[(int)index].RName;
        }
        public string GetSRVShaderName(UInt32 index, bool pureName)
        {
            if (index >= mSRViews.Count)
                return "";
            var retVal = mSRViews[(int)index].ShaderName;
            if (retVal == null)
                return "";
            if (pureName)
                return CGfxMaterialManager.GetPureShaderVarName(retVal, Material.GetHash64().ToString());
            return retVal;
        }

        //List<CSamplerStateDesc> mSamplerStateDescArray = new List<CSamplerStateDesc>();
        //Dictionary<string, CSamplerStateDesc> mSamplerStateDescArray = new Dictionary<string, CSamplerStateDesc>();
        //public Dictionary<string, CSamplerStateDesc> SamplerStateDescArray
        //{
        //    get
        //    {
        //        return mSamplerStateDescArray;
        //    }
        //    set
        //    {
        //        mSamplerStateDescArray = value;
        //    }
        //}
        //public void SetSamplerStateDesc(UInt32 index, CSamplerStateDesc desc)
        //{
        //    if (index >= mSamplerStateDescArray.Count)
        //        return;

        //    mSamplerStateDescArray[(int)index] = desc;
        //}
        //public void GetSamplerStateDesc(UInt32 index, ref CSamplerStateDesc desc)
        //{
        //    if (index >= mSamplerStateDescArray.Count)
        //        return;
        //    desc = mSamplerStateDescArray[(int)index];
        //    if ((desc.AddressU < 0 || desc.AddressU > EAddressMode.ADM_MIRROR_ONCE) ||
        //                            (desc.AddressV < 0 || desc.AddressV > EAddressMode.ADM_MIRROR_ONCE) ||
        //                            (desc.AddressW < 0 || desc.AddressW > EAddressMode.ADM_MIRROR_ONCE))
        //    {
        //        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Material", $"Material {Name}: SamplerState is invalid");
        //        mSamplerStateDescArray[(int)index].SetDefault();
        //        desc.SetDefault();
        //    }
        //}

        public void SetParam(CGfxMaterialParam param)
        {
            switch(param.VarType)
            {
                case EShaderVarType.SVT_Texture:
                case EShaderVarType.SVT_Sampler:
                    {
                        var idx = FindSRVIndex(param.VarName, false);
                        if(idx < UInt32.MaxValue)
                        {
                            var srvTex = EngineNS.CEngine.Instance.TextureManager.GetShaderRView(EngineNS.CEngine.Instance.RenderContext, param.TextureRName);
                            SetSRV(idx, srvTex);
                        }
                    }
                    break;
                default:
                    {
                        var idx = FindVarIndex(param.VarName, false);
                        if (idx < UInt32.MaxValue)
                        {
                            switch (param.VarType)
                            {
                                case EShaderVarType.SVT_Float1:
                                    {
                                        float val;
                                        CGfxMaterialParam.GetTypeValue(param.VarValue, out val);
                                        SetVarValue(idx, 0, ref val);
                                    }
                                    break;
                                case EShaderVarType.SVT_Float2:
                                    {
                                        EngineNS.Vector2 val;
                                        CGfxMaterialParam.GetTypeValue(param.VarValue, out val);
                                        SetVarValue(idx, 0, ref val);
                                    }
                                    break;
                                case EShaderVarType.SVT_Float3:
                                    {
                                        EngineNS.Vector3 val;
                                        CGfxMaterialParam.GetTypeValue(param.VarValue, out val);
                                        SetVarValue(idx, 0, ref val);
                                    }
                                    break;
                                case EShaderVarType.SVT_Float4:
                                    {
                                        EngineNS.Vector4 val;
                                        CGfxMaterialParam.GetTypeValue(param.VarValue, out val);
                                        SetVarValue(idx, 0, ref val);
                                    }
                                    break;
                                default:
                                    throw new InvalidOperationException();
                            }
                        }
                    }
                    break;
            }
        }
        public void ResetValuesFromMaterial(CGfxMaterial parentMaterial)
        {
            if (parentMaterial == null)
                return;

            foreach(var param in parentMaterial.ParamList)
            {
                switch(param.VarType)
                {
                    case EShaderVarType.SVT_Texture:
                    case EShaderVarType.SVT_Sampler:
                        {
                            var idx = FindSRVIndex(param.VarName, false);
                            if(idx < UInt32.MaxValue)
                            {
                                SetParam(param);
                            }
                        }
                        break;
                    default:
                        {
                            var idx = FindVarIndex(param.VarName, false);
                            if(idx < UInt32.MaxValue)
                            {
                                SetParam(param);
                            }
                        }
                        break;
                }
            }
        }
        public bool RefreshFromMaterial(CGfxMaterial parentMaterial)
        {
            if (parentMaterial == null)
                return false;
            var dic = new Dictionary<string, EngineNS.Graphics.CGfxMaterialParam>();
            foreach (var param in parentMaterial.ParamList)
            {
                dic[param.VarName] = param;
                var oldName = param.OldVarName;
                switch(param.VarType)
                {
                    case EShaderVarType.SVT_Texture:
                    case EShaderVarType.SVT_Sampler:
                        {
                            var idx = FindSRVIndex(param.VarName, false);
                            if(idx == UInt32.MaxValue)
                            {
                                if (AddSRV(param.VarName))
                                {
                                    if(string.IsNullOrEmpty(oldName))
                                        SetParam(param);
                                    else
                                    {
                                        var oldIdx = FindSRVIndex(oldName);
                                        if (oldIdx < UInt32.MaxValue)
                                        {
                                            var val = GetSRVName(oldIdx);
                                            var newParam = new CGfxMaterialParam();
                                            newParam.CopyFrom(param);
                                            newParam.SetValueStr(val);
                                            SetParam(newParam);
                                        }
                                        else
                                            SetParam(param);
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        {
                            var idx = FindVarIndex(param.VarName, false);
                            if(idx == UInt32.MaxValue)
                            {
                                if (AddVar(param.VarName, param.VarType, param.ElementIdx))
                                {
                                    if(string.IsNullOrEmpty(oldName))
                                        SetParam(param);
                                    else
                                    {
                                        var oldIdx = FindVarIndex(oldName);
                                        if (oldIdx < UInt32.MaxValue)
                                            CopyAndSaveParam(oldIdx, param);
                                        else
                                            SetParam(param);
                                    }
                                }
                            }
                            else if (mVars[(int)idx].Definition.Type != param.VarType)
                            {
                                var pv = mVars[(int)idx];
                                pv.Definition.Type = param.VarType;
                                CopyAndSaveParam(idx, param);
                            }
                        }
                        break;
                }
            }

            for(UInt32 i=0; i<VarNumber; i++)
            {
                var name = GetVarName(i, false);
                if(!dic.ContainsKey(name))
                {
                    RemoveVar(i);
                }
            }
            for(UInt32 i=0; i<SRVNumber; i++)
            {
                var name = GetSRVShaderName(i, false);
                if(!dic.ContainsKey(name))
                {
                    RemoveSRV(i);
                }
            }
            return true;
        }
        void CopyAndSaveParam(UInt32 idx, CGfxMaterialParam param)
        {
            var newParam = new CGfxMaterialParam();
            newParam.CopyFrom(param);
            switch (param.VarType)
            {
                case EShaderVarType.SVT_Float1:
                case EShaderVarType.SVT_Int1:
                    {
                        float value = 0.0f;
                        GetVarValue(idx, 0, ref value);
                        newParam.SetValueStr(value.ToString());
                    }
                    break;
                case EShaderVarType.SVT_Float2:
                case EShaderVarType.SVT_Int2:
                    {
                        Vector2 value = Vector2.Zero;
                        GetVarValue(idx, 0, ref value);
                        newParam.SetValueStr($"{value.X},{value.Y}");
                    }
                    break;
                case EShaderVarType.SVT_Float3:
                case EShaderVarType.SVT_Int3:
                    {
                        Vector3 value = Vector3.Zero;
                        GetVarValue(idx, 0, ref value);
                        newParam.SetValueStr($"{value.X},{value.Y},{value.Z}");
                    }
                    break;
                case EShaderVarType.SVT_Float4:
                case EShaderVarType.SVT_Int4:
                    {
                        Vector4 value = Vector4.Zero;
                        GetVarValue(idx, 0, ref value);
                        newParam.SetValueStr($"{value.X},{value.Y},{value.Z},{value.W}");
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
            SetParam(newParam);
        }
        public void BindTextures(CShaderResources srs, CShaderProgram shaderProgram)
        {
            /////
            UInt32 count = SRVNumber;
            for (UInt32 i = 0; i < count; i++)
            {
                //srs.Get
                //得到纹理指针
                var srv = GetSRV(i);
                if (srv == null)
                    continue;
                //得到材质实例里面记录的使用材质的Texture变量shader名
                var shaderName = GetSRVShaderName(i, false);
                //通过shader名获取索引
                CTextureBindInfo info = new CTextureBindInfo();
                if (shaderProgram.FindTextureBindInfo(null, shaderName, ref info))
                {
                    if (srs.IsUserControlTexture(info.PSBindPoint)==false)
                    {
                        //设置纹理绑定点
                        CShaderResources.SDK_IShaderResources_PSBindTexture(srs.CoreObject, (byte)info.PSBindPoint, srv.CoreObject);
                    }
                }
            }
        }
        public void PreUse(bool isSync = false)
        {
            UInt32 count = SRVNumber;
            for (UInt32 i = 0; i < count; i++)
            {
                var srv = GetSRV(i);
                if (srv == null)
                    continue;
                var texture = CEngine.Instance.TextureManager.GetShaderRView(CEngine.Instance.RenderContext, srv.Name);
                texture.PreUse(isSync);
            }
        }

#region SDK
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxMaterialInstance_Init(NativePointer self, CGfxMaterial.NativePointer material, string name);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxMaterialInstance_SetMaterial(NativePointer self, CGfxMaterial.NativePointer material, bool doClear);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static unsafe void SDK_GfxMaterialInstance_GetRSDesc(NativePointer self, CRasterizerStateDesc* desc);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static unsafe void SDK_GfxMaterialInstance_GetDSDesc(NativePointer self, CDepthStencilStateDesc* desc);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static unsafe void SDK_GfxMaterialInstance_GetBLDDesc(NativePointer self, CBlendStateDesc* desc);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxMaterialInstance_SetRasterizerState(NativePointer self, CRasterizerState.NativePointer rs);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxMaterialInstance_SetDepthStencilState(NativePointer self, CDepthStencilState.NativePointer dss);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxMaterialInstance_SetBlendState(NativePointer self, CBlendState.NativePointer bs);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static UInt32 SDK_GfxMaterialInstance_GetVarNumber(NativePointer self);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        //public extern static string SDK_GfxMaterialInstance_GetVarName(NativePointer self, UInt32 index);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static UInt32 SDK_GfxMaterialInstance_FindVarIndex(NativePointer self, string name);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static unsafe vBOOL SDK_GfxMaterialInstance_GetVarValue(NativePointer self, UInt32 index, UInt32 elementIndex, CGfxVar* definition, IntPtr pValue);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxMaterialInstance_SetVarValue(NativePointer self, UInt32 index, UInt32 elementIndex, IntPtr pValue);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static UInt32 SDK_GfxMaterialInstance_FindSRVIndex(NativePointer self, string name);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxMaterialInstance_SetSRV(NativePointer self, UInt32 index, CShaderResourceView.NativePointer rsv);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static CShaderResourceView.NativePointer SDK_GfxMaterialInstance_GetSRV(NativePointer self, UInt32 index);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        //public extern static string SDK_GfxMaterialInstance_GetSRVShaderName(NativePointer self, UInt32 index);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        //public extern static string SDK_GfxMaterialInstance_GetSRVName(NativePointer self, UInt32 index);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static UInt32 SDK_GfxMaterialInstance_GetSRVNumber(NativePointer self);

        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static unsafe void SDK_GfxMaterialInstance_SetSamplerStateDesc(NativePointer self, UInt32 index, CSamplerStateDesc* desc);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static unsafe void SDK_GfxMaterialInstance_GetSamplerStateDesc(NativePointer self, UInt32 index, CSamplerStateDesc* desc);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxMaterialInstance_AddVar(NativePointer self, string name, UInt32 type, UInt32 elements);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxMaterialInstance_RemoveVar(NativePointer self, UInt32 index);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxMaterialInstance_AddSRV(NativePointer self, string name);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxMaterialInstance_RemoveSRV(NativePointer self, UInt32 index);

        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_GfxMaterialInstance_Save2Xnd(NativePointer self, IO.XndNode.NativePointer node);
#endregion
    }
    public class CGfxMaterialInstanceManager
    {
        public Dictionary<RName, CGfxMaterialInstance> Materials
        {
            get;
        } = new Dictionary<RName, CGfxMaterialInstance>(new RName.EqualityComparer());
        public void RemoveMaterialFromDic(RName name)
        {
            lock(Materials)
            {
                Materials.Remove(name);
            }
        }

        CGfxMaterialInstance mDefaultMaterialInstance;
        public CGfxMaterialInstance DefaultMaterialInstance
        {
            get { return mDefaultMaterialInstance; }
        }
        public async System.Threading.Tasks.Task<bool> Init(CRenderContext rc, RName dftMtl)
        {
            mDefaultMaterialInstance = await GetMaterialInstanceAsync(rc, dftMtl);
            if (mDefaultMaterialInstance == null)
                return true;
            return true;
        }
        public async System.Threading.Tasks.Task<CGfxMaterialInstance> GetMaterialInstanceAsync(CRenderContext rc, RName name)
        {
            if (name==null || name.IsExtension(CEngineDesc.MaterialInstanceExtension) == false)
                return null;
            CGfxMaterialInstance result;
            bool found = false;
            lock(Materials)
            {
                if (Materials.TryGetValue(name, out result) == false)
                {
                    result = new CGfxMaterialInstance();
                    Materials.Add(name, result);
                }
                else
                    found = true;
            }
            if(found)
            {
                var context = await result.AwaitLoad();
                if (context != null && context.Result == null)
                    return null;
                return result;
            }

            if (false == await result.LoadMaterialInstanceAsync(rc, name))
                return null;
            return result;
        }
        public CGfxMaterialInstance NewMaterialInstance(CRenderContext rc, CGfxMaterial material, RName name)
        {
            if (name.IsExtension(CEngineDesc.MaterialInstanceExtension) == false)
                return null;
            if (Materials.ContainsKey(name))
            {
                return null;
            }
            var result = new CGfxMaterialInstance();
            if (false == result.NewMaterialInstance(rc, material, name))
                return null;
            lock (Materials)
            {
                Materials.Add(name, result);
            }
            return result;
        }
        public CGfxMaterialInstance NewMaterialInstance(CRenderContext rc, CGfxMaterial material)
        {
            var result = new CGfxMaterialInstance();
            var rName = EngineNS.RName.GetRName("_TemplateNotSaveRName");
            if (false == result.NewMaterialInstance(rc, material, rName))
                return null;
            foreach(var param in material.ParamList)
            {
                result.SetParam(param);
            }
            return result;
        }
    }

    [Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable)]
    public class McMaterialVarSetter
    {
        public int Version
        {
            get;
            set;
        }

        public virtual void OnSetVar(int index, ConstantVarDesc desc, CConstantBuffer cb)
        {

        }
    }
}
