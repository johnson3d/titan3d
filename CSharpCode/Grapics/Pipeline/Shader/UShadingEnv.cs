using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    [Rtti.Meta]
    public enum EPixelShaderInput
    {
        PST_Position,
        PST_Normal,
        PST_Color,
        PST_UV,
        PST_WorldPos,
        PST_Tangent,
        PST_LightMap,
        PST_Custom0,
        PST_Custom1,
        PST_Custom2,
        PST_Custom3,
        PST_Custom4,
        PST_PointLightIndices,
        PST_F4_1,
        PST_F4_2,
        PST_F4_3,
        PST_SpecialData,

        PST_Number,
    }
    public enum EPermutation_Bool : int
    {
        FalseValue = 0,
        TrueValue = 1,
        BitWidth = 1,
    }
    public class UShadingEnv
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
        public UPermutationItem PushPermutation<T>(string name, int bitwidth, uint value = 0) where T : struct, IConvertible
        {
            var result = FPermutationId.GetMask(PermutationBitWidth, bitwidth);
            result.Name = name;
            result.TypeDesc = Rtti.UTypeDesc.TypeOf<T>();
            result.Value.SetValue(value, result);

            PermutationValues.Add(result);
            PermutationBitWidth += bitwidth;
            System.Diagnostics.Debug.Assert(PermutationBitWidth <= 32);

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
        
        public FPermutationId mCurrentPermutationId;
        public FPermutationId CurrentPermutationId
        {
            get => mCurrentPermutationId;
        }
        public RName CodeName { get; set; }
        public NxRHI.UCbView PerShadingCBuffer;                
        public bool GetShaderDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {
            for (int i = 0; i < PermutationValues.Count; i++)
            {
                uint v = id.GetValue(PermutationValues[i]);
                //var valueStr = PermutationValues[i].GetValueString(in id);
                defines.mCoreObject.AddDefine(PermutationValues[i].Name, $"{v}");
            }
            EnvShadingDefines(id, defines);
            return true;
        }
        protected virtual void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {

        }
    }

    public abstract class UGraphicsShadingEnv
        : UShadingEnv
    {
        public abstract NxRHI.EVertexStreamType[] GetNeedStreams();
        //public abstract EPixelShaderInput[] GetPSNeedInputs();
        public virtual EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_Normal,
                EPixelShaderInput.PST_Color,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_WorldPos,
                EPixelShaderInput.PST_Tangent,
                EPixelShaderInput.PST_LightMap,
                EPixelShaderInput.PST_Custom0,
                EPixelShaderInput.PST_Custom1,
                EPixelShaderInput.PST_Custom2,
                EPixelShaderInput.PST_Custom3,
                EPixelShaderInput.PST_Custom4,
                EPixelShaderInput.PST_PointLightIndices,
                EPixelShaderInput.PST_F4_1,
                EPixelShaderInput.PST_F4_2,
                EPixelShaderInput.PST_F4_3,
                EPixelShaderInput.PST_SpecialData,
            };
        }
        public virtual void OnBuildDrawCall(URenderPolicy policy, NxRHI.UGraphicDraw drawcall) { }
        public virtual void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.UMesh mesh)
        {

        }
    }
    public abstract class UComputeShadingEnv : UShadingEnv
    {
        [Rtti.Meta]
        public string MainName { get; set; }
        private NxRHI.UComputeEffect CurrentEffect;
        public abstract Vector3ui DispatchArg { get; }
        public override string ToString()
        {
            return base.ToString() + $"[{MainName}:{DispatchArg.ToString()}]";
        }
        public NxRHI.UComputeEffect GetEffect()
        {
            if (CurrentEffect == null || CurrentEffect.PermutationId != this.CurrentPermutationId)
            {
                CurrentEffect = OnCreateEffect();
            }
            return CurrentEffect;
        }
        protected virtual NxRHI.UComputeEffect OnCreateEffect()
        {
            return UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(CodeName,
                MainName, NxRHI.EShaderType.SDT_ComputeShader, this, null, null);
        }
        public virtual void OnDrawCall(NxRHI.UComputeDraw drawcall, URenderPolicy policy)
        {

        }
        public void SetDrawcallDispatch(object tagObject, URenderPolicy policy, NxRHI.UComputeDraw drawcall, uint x, uint y, uint z, bool bRoundupXYZ)
        {
            drawcall.TagObject = tagObject;
            drawcall.SetComputeEffect(GetEffect());
            if (bRoundupXYZ)
            {
                drawcall.SetDispatch(MathHelper.Roundup(x, DispatchArg.X),
                MathHelper.Roundup(y, DispatchArg.Y),
                MathHelper.Roundup(z, DispatchArg.Z));
            }
            else
            {
                drawcall.SetDispatch(x, y, z);
            }

            this.OnDrawCall(drawcall, policy);
        }
        public void SetDrawcallIndirectDispatch(object tagObject, URenderPolicy policy, NxRHI.UComputeDraw drawcall, NxRHI.UBuffer indirectBuffer)
        {
            drawcall.TagObject = tagObject;
            drawcall.SetComputeEffect(GetEffect());
            drawcall.BindIndirectDispatchArgsBuffer(indirectBuffer);

            this.OnDrawCall(drawcall, policy);
        }
        protected override void EnvShadingDefines(in FPermutationId id, UShaderDefinitions defines)
        {
            defines.AddDefine("DispatchX", (int)DispatchArg.X);
            defines.AddDefine("DispatchY", (int)DispatchArg.Y);
            defines.AddDefine("DispatchZ", (int)DispatchArg.Z);
        }
    }
    public class UDummyShading : Shader.UGraphicsShadingEnv
    {
        public UDummyShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/DummyShading.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position, };
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                    EPixelShaderInput.PST_Position,
                };
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

    namespace NxRHI
    {
        public partial class UGraphicDraw
        {
            public Graphics.Pipeline.Shader.UEffect Effect { get; private set; }
            internal Graphics.Pipeline.Shader.UShadingEnv.FPermutationId PermutationId;
            public bool IsPermutationChanged()
            {
                var shading = Effect.ShadingEnv;
                return PermutationId != shading.mCurrentPermutationId;
            }
            public void BindShaderEffect(Graphics.Pipeline.Shader.UEffect effect)
            {
                Effect = effect;
                mCoreObject.BindShaderEffect(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, effect.ShaderEffect.mCoreObject);
            }
        }
    }
}
