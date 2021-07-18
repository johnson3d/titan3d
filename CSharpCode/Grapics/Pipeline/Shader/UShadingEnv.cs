using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public class UShadingEnv
    {
        public UShadingEnv()
        {
            var flds = this.GetType().GetFields();
            foreach (var i in flds)
            {
                if (i.DeclaringType == typeof(UShadingEnv))
                    continue;
                if (i.DeclaringType == typeof(object))
                    continue;
                System.Diagnostics.Debug.Assert(false);
            }
            var props = this.GetType().GetProperties();
            foreach (var i in props)
            {
                if (i.DeclaringType == typeof(UShadingEnv))
                    continue;
                if (i.DeclaringType == typeof(object))
                    continue;
                System.Diagnostics.Debug.Assert(false);
            }
        }
        public override string ToString()
        {
            string result = "";
            result += $"{CodeName}:Permutation={CurrentPermutationId}\n";
            //foreach (var i in MacroDefines)
            //{
            //    result += $"{i.Name} = {i.Values[i.CurValueIndex]}\n";
            //}
            return result;
        }
        public virtual void OnBuildDrawCall(RHI.CDrawCall drawcall) { }
        public uint CurrentPermutationId { get; set; } = 0;
        public RName CodeName { get; set; }
        public RHI.CConstantBuffer PerShadingCBuffer;
        public List<string> mMacroValues = new List<string>();
        public class MacroDefine
        {
            public int BitStart = 0;
            public int BitCount = 0;
            public uint BitMask = 1;
            public string Name { get; set; }
            public List<string> Values { get; } = new List<string>();
            public int CurValueIndex = 0;
            public void UpdateBitMask()
            {
                BitMask = 1;
                BitCount = 1;
                while ((1 << BitCount) < Values.Count)
                {
                    BitMask |= (uint)(1 << BitCount);
                    BitCount++;
                }
            }
            public int GetValueIndex(string value)
            {
                for (int i = 0; i < Values.Count; i++)
                {
                    if (Values[i] == value)
                        return i;
                }
                return -1;
            }
        }
        public void UpdatePermutation(List<string> values)
        {
            uint permuationId;
            this.GetPermutation(values, out permuationId);
            this.CurrentPermutationId = permuationId;
        }
        public List<MacroDefine> MacroDefines = new List<MacroDefine>();
        public uint NumOfBits;
        protected void UpdatePermutationBitMask()
        {
            NumOfBits = 0;
            int bitStart = 0;
            for (int i = 0; i < MacroDefines.Count; i++)
            {
                MacroDefines[i].UpdateBitMask();
                MacroDefines[i].BitStart = bitStart;
                bitStart += MacroDefines[i].BitCount;
                
                NumOfBits += (uint)MacroDefines[i].Values.Count;
                if (NumOfBits > 32)
                    throw new IO.IOException("");
            }
        }
        public int GetMacroIndex(string macroName)
        {
            for (int i = 0; i < MacroDefines.Count; i++)
            {
                if (MacroDefines[i].Name == macroName)
                {
                    return i;
                }
            }
            return -1;
        }
        public bool GetPermutation(List<string> values, out uint permutation)
        {
            permutation = 0;
            if (values.Count != MacroDefines.Count)
                return false;

            for (int i = 0; i < values.Count; i++)
            {
                var index = MacroDefines[i].GetValueIndex(values[i]);
                if (index < 0)
                    return false;

                permutation |= (uint)(index << MacroDefines[i].BitStart);
            }

            return true;
        }
        public bool GetMacroDefines(uint permutation, List<string> values)
        {
            values.Clear();
            for (int i = 0; i < MacroDefines.Count; i++)
            {
                uint t = permutation >> MacroDefines[i].BitStart;
                int index = (int)(t & MacroDefines[i].BitMask);

                if (index >= MacroDefines[i].Values.Count)
                    return false;
                values.Add(MacroDefines[i].Values[index]);
            }
            return true;
        }
        public string GetDefineValue(uint permutation, string macroName)
        {
            foreach(var i in MacroDefines)
            {
                if(i.Name == macroName)
                {
                    uint t = permutation >> i.BitStart;
                    int index = (int)(t & i.BitMask);

                    if (index >= i.Values.Count)
                        return null;
                    return i.Values[index];
                }
            }
            return null;
        }
        public string GetDefineValue(uint permutation, int macroIndex)
        {
            if (macroIndex >= MacroDefines.Count)
                return null;
            uint t = permutation >> MacroDefines[macroIndex].BitStart;
            int index = (int)(t & MacroDefines[macroIndex].BitMask);

            if (index >= MacroDefines[macroIndex].Values.Count)
                return null;
            return MacroDefines[macroIndex].Values[index];
        }
        public virtual bool IsValidPermutation(UMaterial mtl, uint permutation)
        {
            return true;
        }
        public bool GetShaderDefines(uint permutation, RHI.CShaderDefinitions defines)
        {
            for (int i = 0; i < MacroDefines.Count; i++)
            {
                uint t = permutation >> MacroDefines[i].BitStart;
                int index = (int)(t & MacroDefines[i].BitMask);

                if (index >= MacroDefines[i].Values.Count)
                    return false;

                defines.mCoreObject.AddDefine(MacroDefines[i].Name, MacroDefines[i].Values[index]);
            }
            return true;
        }
        public virtual void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, IRenderPolicy policy, Mesh.UMesh mesh)
        {
            
        }
    }
    public class UShadingEnvManager : UModule<UEngine>
    {
        public Dictionary<Type, UShadingEnv> Shadings { get; } = new Dictionary<Type, UShadingEnv>();
        public UShadingEnv GetShadingEnv(Type name)
        {
            UShadingEnv shading;
            if (Shadings.TryGetValue(name, out shading))
                return shading;

            shading = Rtti.UTypeDescManager.CreateInstance(name) as UShadingEnv;
            if (shading == null)
                return null;
            Shadings.Add(name, shading);
            return shading;
        }
        public T GetShadingEnv<T>() where T : UShadingEnv, new()
        {
            UShadingEnv shading;
            if (Shadings.TryGetValue(typeof(T), out shading))
                return shading as T;
            T result = new T();
            Shadings.Add(typeof(T), result);
            return result;
        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        public Graphics.Pipeline.Shader.UShadingEnvManager ShadingEnvManager { get; } = new Graphics.Pipeline.Shader.UShadingEnvManager();
    }
}
