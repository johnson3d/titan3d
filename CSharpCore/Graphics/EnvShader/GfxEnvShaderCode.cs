using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics.EnvShader;

namespace EngineNS.Graphics
{
    public class GfxEnvShaderCode
    {
        public UInt32 Version
        {
            get;
            private set;
        }
        private System.Type mEnvType;
        public System.Type EnvType
        {
            get { return mEnvType; }
        }
        private Hash64 mHash64 = Hash64.Empty;
        private RName mShaderName = RName.EmptyName;
        public RName ShaderName
        {
            get { return mShaderName; }
        }
        private List<CShaderDefinitions.MacroDefine> mMacroDefines;
        internal List<CShaderDefinitions.MacroDefine> MacroDefines
        {
            get { return mMacroDefines; }
        }
        public delegate List<string> FGetMacroValues(string name);
        public FGetMacroValues GetMacroValues;

        public GfxEnvShaderCode CloneEnvShaderCode()
        {
            var result = new GfxEnvShaderCode();
            result.mEnvType = mEnvType;
            result.mHash64 = mHash64;
            result.mShaderName = mShaderName;
            result.mMacroDefines = new List<CShaderDefinitions.MacroDefine>();
            result.mMacroDefines.AddRange(mMacroDefines);
            result.UpdateHash64();
            result.GetMacroValues = GetMacroValues;
            return result;
        }

        public void Init(RName shaderName, List<CShaderDefinitions.MacroDefine> defs, CGfxShadingEnv env)
        {
            mShaderName = RName.GetRName(shaderName.Name, RName.enRNameType.Engine);
            //mShaderName = shaderName;
            mEnvType = env.GetType();
            mMacroDefines = new List<CShaderDefinitions.MacroDefine>();
            mMacroDefines.AddRange(defs);
            UpdateHash64();
        }
        public string GetMacroDefineValue(int index)
        {
            if (index >= mMacroDefines.Count)
                return null;

            return mMacroDefines[index].Definition;
        }
        public bool FindMacroDefine(string name, out CShaderDefinitions.MacroDefine def)
        {
            CShaderDefinitions.MacroDefine tmp;
            for (int i = 0; i < mMacroDefines.Count; i++)
            {
                if (mMacroDefines[i].Name == name)
                {
                    def = mMacroDefines[i];
                    return true;
                }
            }
            def = new CShaderDefinitions.MacroDefine();
            return false;
        }
        public void SetMacroDefineValue(string name, string value)
        {
            CShaderDefinitions.MacroDefine tmp;
            for (int i = 0; i < mMacroDefines.Count; i++)
            {
                if (mMacroDefines[i].Name == name)
                {   
                    tmp.Name = name;
                    tmp.Definition = value;
                    mMacroDefines[i] = tmp;
                    UpdateHash64();
                    return;
                }
            }

            tmp.Name = name;
            tmp.Definition = value;
            mMacroDefines.Add(tmp);
            UpdateHash64();
        }
        public bool SetMacroDefineValue(int index, string value)
        {
            if (index >= mMacroDefines.Count)
                return false;

            CShaderDefinitions.MacroDefine tmp;
            tmp.Name = mMacroDefines[index].Name;
            tmp.Definition = value;
            mMacroDefines[index] = tmp;
            UpdateHash64();
            return true;
        }
        protected void UpdateHash64()
        {
            var hashStr = mShaderName.Name;
            if (mShaderName.RNameType != RName.enRNameType.Game)
                hashStr += ":" + mShaderName.RNameType;

            if (mMacroDefines != null)
            {
                for (int i = 0; i < mMacroDefines.Count; i++)
                {
                    hashStr += mMacroDefines[i].Name + ":" + mMacroDefines[i].Definition;
                }
            }

            EngineNS.Hash64.CalcHash64(ref mHash64, hashStr);
            Version++;
        }
        public Hash64 GetHash64()
        {
            if (mHash64 == Hash64.Empty)
            {
                UpdateHash64();
            }
            return mHash64;
        }
        internal string GetShaderDefinesString()
        {
            string result = "";
            if (MacroDefines != null)
            {
                foreach (var i in MacroDefines)
                {
                    result += i.Definition + ":" + i.Name;
                }
            }
            return result;
        }
        public override string ToString()
        {
            return ShaderName.ToString();
        }
    }

    public class GfxEnvShaderCodeSettings
    { 
        public CGfxMobileCopyEditorSE Setting_CGfxMobileCopyEditorSE
        {
            get
            {
                return CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileCopyEditorSE>();
            }
        }
        public CGfxMobileOpaqueEditorSE Setting_CGfxMobileOpaqueEditorSE
        {
            get
            {
                return CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileOpaqueEditorSE>();
            }
        }
        public CGfxMobileCopySE Setting_CGfxMobileCopySE
        {
            get
            {
                return CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileCopySE>();
            }
        }
        public CGfxMobileOpaqueSE Setting_CGfxMobileOpaqueSE
        {
            get
            {
                return CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CGfxMobileOpaqueSE>();
            }
        }
    }
}
