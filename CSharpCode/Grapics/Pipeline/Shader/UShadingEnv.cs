using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public enum EPermutation_Bool : int
    {
        FalseValue = 0,
        TrueValue = 1,
        BitWidth = 1,
    }
    public abstract class UShadingEnv
    {
        public UShadingEnv()
        {
            //var flds = this.GetType().GetFields();
            //foreach (var i in flds)
            //{
            //    if (i.DeclaringType == typeof(UShadingEnv))
            //        continue;
            //    if (i.DeclaringType == typeof(object))
            //        continue;
            //    if (i.DeclaringType == typeof(UPermutationItem))
            //        continue;
            //    System.Diagnostics.Debug.Assert(false);
            //}
            //var props = this.GetType().GetProperties();
            //foreach (var i in props)
            //{
            //    if (i.DeclaringType == typeof(UShadingEnv))
            //        continue;
            //    if (i.DeclaringType == typeof(object))
            //        continue;
            //    if (i.DeclaringType == typeof(UPermutationItem))
            //        continue;
            //    System.Diagnostics.Debug.Assert(false);
            //}
        }
        #region PermutationID
        public class UPermutationItem
        {
            public string Name;
            public Rtti.UTypeDesc TypeDesc;
            public int Start;
            public int Width;
            public FPermutationId Mask;
            public FPermutationId Value;
            public void SetValue(bool v)
            {
                if (!TypeDesc.IsEqual(typeof(EPermutation_Bool)))
                {
                    return;
                }
                if (v)
                {
                    SetValue((uint)EPermutation_Bool.TrueValue);
                }
                else
                {
                    SetValue((uint)EPermutation_Bool.FalseValue);
                }
            }
            public void SetValue(uint v)
            {
                Value.SetValue(v, this);
            }
            public uint GetValue()
            {
                return Value.GetValue(this);
            }
            public string GetValueString(in FPermutationId id)
            {
                //if (!typeof(T).IsEnum)
                //{
                //    return null;
                //}
                //Enum.GetValues(typeof(T));
                uint v = id.GetValue(this);
                return Enum.GetName(TypeDesc.SystemType, v);
            }
        }
        public struct FPermutationId : System.IEquatable<FPermutationId>
        {
            public override string ToString()
            {
                return Data.ToString();
            }
            public FPermutationId(uint v)
            {
                Data = v;
            }
            public uint Data;
            
            public static UPermutationItem GetMask(int start, int width)
            {
                UPermutationItem result = new UPermutationItem();
                result.Start = start;
                result.Width = width;
                result.Mask.Data = 1;
                for (int i = 1; i < width; i++)
                {
                    result.Mask.Data = (result.Mask.Data << 1);
                    result.Mask.Data |= 1;
                }
                result.Mask.Data = (result.Mask.Data << start);
                return result;
            }
            public void Reset()
            {
                Data = 0;
            }
            public static FPermutationId BitOrValue(in FPermutationId lh,  in FPermutationId rh)
            {
                return new FPermutationId(lh.Data | rh.Data);
            }
            public void SetValue(uint value, UPermutationItem mask)
            {
                Data = Data & (~mask.Mask.Data);
                Data |= ((value << mask.Start) & mask.Mask.Data);
            }
            public uint GetValue(UPermutationItem mask)
            {
                return ((Data & mask.Mask.Data) >> mask.Start);
            }
            public override int GetHashCode()
            {
                return (int)Data;
            }
            public override bool Equals(object value)
            {
                if (value == null)
                    return false;

                if (value.GetType() != GetType())
                    return false;

                return Equals((FPermutationId)(value));
            }
            public bool Equals(FPermutationId other)
            {
                return this.Data == other.Data;
            }
            public static bool operator == (in FPermutationId left, in FPermutationId right)
            {
                return left.Equals(right);
                //return Equals( left, right );
            }
            public static bool operator !=(in FPermutationId left, in FPermutationId right)
            {
                return !left.Equals(right);
                //return Equals( left, right );
            }
        }
        protected int PermutationBitWidth;
        protected List<UPermutationItem> PermutationValues { get; } = new List<UPermutationItem>();
        public void BeginPermutaion()
        {
            PermutationBitWidth = 0;
            PermutationValues.Clear();
        }
        public UPermutationItem PushPermutation<T>(string name, int NumEnumMember, uint value = 0) where T : struct, IConvertible
        {
            var result = FPermutationId.GetMask(PermutationBitWidth, NumEnumMember);
            result.Name = name;
            result.TypeDesc = Rtti.UTypeDesc.TypeOf<T>();
            result.Value.SetValue(value, result);

            PermutationValues.Add(result);
            PermutationBitWidth += NumEnumMember;
            return result;
        }
        public void UpdatePermutation()
        {
            mCurrentPermutationId.Reset();
            foreach (var i in PermutationValues)
            {
                mCurrentPermutationId = FPermutationId.BitOrValue(mCurrentPermutationId, in i.Value);
            }
        }
        public virtual bool IsValidPermutation(UMdfQueue mdfQueue, UMaterial mtl)
        {
            return true;
        }
        #endregion
        public override string ToString()
        {
            string result = "";
            result += $"{CodeName}:Permutation={mCurrentPermutationId}\n";
            //foreach (var i in MacroDefines)
            //{
            //    result += $"{i.Name} = {i.Values[i.CurValueIndex]}\n";
            //}
            return result;
        }
        public abstract EVertexStreamType[] GetNeedStreams();
        public virtual void OnBuildDrawCall(URenderPolicy policy, RHI.CDrawCall drawcall) { }
        public FPermutationId mCurrentPermutationId;
        public FPermutationId CurrentPermutationId
        {
            get => mCurrentPermutationId;
        }
        public RName CodeName { get; set; }
        public RHI.CConstantBuffer PerShadingCBuffer;                
        public bool GetShaderDefines(in FPermutationId id, RHI.CShaderDefinitions defines)
        {
            for (int i = 0; i < PermutationValues.Count; i++)
            {
                uint v = id.GetValue(PermutationValues[i]);
                //var valueStr = PermutationValues[i].GetValueString(in id);
                defines.mCoreObject.AddDefine(PermutationValues[i].Name, $"{v}");
            }
            return true;
        }
        public virtual void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, URenderPolicy policy, Mesh.UMesh mesh)
        {
            
        }
    }
    public class UDummyShading : Shader.UShadingEnv
    {
        public UDummyShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/DummyShading.cginc", RName.ERNameType.Engine);
        }
        public override EVertexStreamType[] GetNeedStreams()
        {
            return new EVertexStreamType[] { EVertexStreamType.VST_Position, };
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
