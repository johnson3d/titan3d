using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    [Rtti.Meta("")]
    public enum EPixelShaderInput
    {
        PST_Position,
        PST_Normal,
        PST_Color,
        PST_UV,
        PST_WorldPos,
        PST_Tangent,
        PST_ExtraUV,
        PST_Custom0,
        PST_Custom1,
        PST_Custom2,
        PST_Custom3,
        PST_Custom4,
        PST_F4_1,
        PST_F4_2,
        PST_F4_3,
        PST_SpecialData,
        PST_InstanceID,

        PST_Number,
    }
    public enum EPermutation_Bool : int
    {
        FalseValue = 0,
        TrueValue = 1,
        BitWidth = 1,
    }
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtShadingEnv
    {
        public static int GetBitWidth(int num)
        {
            int result = 0;
            while (num > 0)
            {
                result++;
                num = num / 2;
            }
            return result;
        }
        public TtShadingEnv()
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
        public bool IsOnlyBuildMode { get; set; } = false;
        public virtual TtShadingEnv Clone()
        {
            var result = Activator.CreateInstance(this.GetType()) as TtShadingEnv;
            result.ShaderDefinitions = this.ShaderDefinitions;
            result.ShaderIncludeProvider = this.ShaderIncludeProvider;
            result.CodeName = this.CodeName;
            result.mCurrentPermutationId = this.mCurrentPermutationId;
            result.PermutationBitWidth = this.PermutationBitWidth;
            result.IsOnlyBuildMode = IsOnlyBuildMode;
            foreach (var i in this.PermutationValues)
            {
                var newItem = new TtPermutationItem();
                newItem.Name = i.Name;
                newItem.TypeDesc = i.TypeDesc;
                newItem.Start = i.Start;
                newItem.Width = i.Width;
                newItem.Mask = i.Mask;
                newItem.Value = i.Value;
                result.PermutationValues.Add(newItem);
            }
            return result;
        }
        protected virtual async Thread.Async.TtTask<bool> OnUpdatePermutation(FPermutationId targetPermutationId)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        // 统一的 ShadingEnv 工厂方法: new + UpdatePermutation 一锤子完成 effect 创建.
        // 调用方只需要 await 一次即可拿到已经完成首次 effect 编译的 shading env.
        // T 需要可无参 new (大部分 ShadingEnv 都是无参构造).
        public static async Thread.Async.TtTask<T> CreateShadingEnv<T>() where T : TtShadingEnv, new()
        {
            T result = new T();
            await result.UpdatePermutation();
            return result;
        }
        // 按 Type 创建 ShadingEnv (用于 effect 反序列化等场景, 此时只有运行时 Type, 没有泛型).
        public static async Thread.Async.TtTask<TtShadingEnv> CreateShadingEnv(Type type)
        {
            var result = Rtti.TtTypeDescManager.CreateInstance(type) as TtShadingEnv;
            if (result == null)
                return null;
            await result.UpdatePermutation();
            return result;
        }
        #region PermutationID
        public class TtPermutationItem
        {
            public string Name;
            public Rtti.TtTypeDesc TypeDesc;
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
            
            public static TtPermutationItem GetMask(int start, int width)
            {
                TtPermutationItem result = new TtPermutationItem();
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
            public void SetValue(uint value, TtPermutationItem mask)
            {
                Data = Data & (~mask.Mask.Data);
                Data |= ((value << mask.Start) & mask.Mask.Data);
            }
            public uint GetValue(TtPermutationItem mask)
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
        protected List<TtPermutationItem> PermutationValues { get; } = new List<TtPermutationItem>();
        public void BeginPermutaion()
        {
            PermutationBitWidth = 0;
            PermutationValues.Clear();
        }
        public TtPermutationItem PushPermutation<T>(string name, int bitwidth, uint value = 0) where T : struct, IConvertible
        {
            var result = FPermutationId.GetMask(PermutationBitWidth, bitwidth);
            result.Name = name;
            result.TypeDesc = Rtti.TtTypeDesc.TypeOf<T>();
            result.Value.SetValue(value, result);

            PermutationValues.Add(result);
            PermutationBitWidth += bitwidth;
            System.Diagnostics.Debug.Assert(PermutationBitWidth <= 32);

            return result;
        }
        public virtual async Thread.Async.TtTask UpdatePermutation()
        {
            mCurrentPermutationId.Reset();
            foreach (var i in PermutationValues)
            {
                mCurrentPermutationId = FPermutationId.BitOrValue(mCurrentPermutationId, in i.Value);
            }
            await this.OnUpdatePermutation(mCurrentPermutationId);
        }
        public virtual bool IsValidPermutation(TtMdfQueueBase mdfQueue, TtMaterial mtl)
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

        #region ShadingEnv Instance Data
        public FPermutationId mCurrentPermutationId;
        public FPermutationId CurrentPermutationId
        {
            get => mCurrentPermutationId;
        }
        public TtShaderDefinitions ShaderDefinitions = null;
        public Editor.ShaderCompiler.TtHLSLInclude ShaderIncludeProvider = null;
        public virtual RName CodeName { get; set; }
        public NxRHI.TtCbView PerShadingCBuffer;
        #endregion

        public bool GetShaderDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
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
        protected virtual void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
        {

        }
    }

    public abstract class TtGraphicsShadingEnv
        : TtShadingEnv
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
                EPixelShaderInput.PST_ExtraUV,
                EPixelShaderInput.PST_Custom0,
                EPixelShaderInput.PST_Custom1,
                EPixelShaderInput.PST_Custom2,
                EPixelShaderInput.PST_Custom3,
                EPixelShaderInput.PST_Custom4,
                EPixelShaderInput.PST_F4_1,
                EPixelShaderInput.PST_F4_2,
                EPixelShaderInput.PST_F4_3,
                EPixelShaderInput.PST_SpecialData,
            };
        }
        public virtual void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall) { }
        public virtual void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtRenderMesh.TtAtom atom)
        {

        }
    }
    public abstract class TtComputeShadingEnv : TtShadingEnv
    {
        public virtual string MainName { get; set; }
        private NxRHI.TtComputeEffect mCurrentEffect;
        public NxRHI.TtComputeEffect CurrentEffect
        {
            get => mCurrentEffect;
        }

        public bool IsReady
        {
            get => CurrentEffect != null;
        }
        public abstract Vector3ui DispatchArg { get; }
        public override string ToString()
        {
            return base.ToString() + $"[{MainName}:{DispatchArg.ToString()}]";
        }
        public override async Thread.Async.TtTask UpdatePermutation()
        {
            await base.UpdatePermutation();
        }
        protected override async Thread.Async.TtTask<bool> OnUpdatePermutation(FPermutationId targetPermutationId)
        {
            var tmp = await TtEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(CodeName,
                MainName, NxRHI.EShaderType.SDT_ComputeShader, this, this.ShaderDefinitions, this.ShaderIncludeProvider);
            if (targetPermutationId != tmp.PermutationId)
            {//为什么会出现这种情况呢，因为多次调用OnUpdatePermutation，因为时机不一样，根据this->PermutaitionId查询出来的tmp可能是之前请求的
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, $"ShadingEnv {CodeName} MainName={MainName} 请求的PermutationId={targetPermutationId} 与实际得到的PermutationId={tmp.PermutationId} 不匹配，可能是多次调用UpdatePermutation导致的时序问题");
            }
            if (mCurrentEffect == null || mCurrentPermutationId == tmp.PermutationId)
                mCurrentEffect = tmp;
            return true;
        }
        public virtual void OnDrawCall(NxRHI.TtComputeDraw drawcall, TtRenderPolicy policy)
        {

        }
        public void SetDrawcallDispatch(object tagObject, TtRenderPolicy policy, NxRHI.TtComputeDraw drawcall, uint x, uint y, uint z, bool bRoundupXYZ)
        {
            drawcall.TagObject = tagObject;
            
            drawcall.BindShaderEffect(CurrentEffect);
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
        public void SetDrawcallIndirectDispatch(object tagObject, TtRenderPolicy policy, NxRHI.TtComputeDraw drawcall, NxRHI.TtBuffer indirectBuffer)
        {
            drawcall.TagObject = tagObject;
            drawcall.BindShaderEffect(CurrentEffect);
            drawcall.BindIndirectDispatchArgsBuffer(indirectBuffer);

            this.OnDrawCall(drawcall, policy);
        }
        protected override void EnvShadingDefines(in FPermutationId id, TtShaderDefinitions defines)
        {
            defines.AddDefine("DispatchX", (int)DispatchArg.X);
            defines.AddDefine("DispatchY", (int)DispatchArg.Y);
            defines.AddDefine("DispatchZ", (int)DispatchArg.Z);
        }
    }

    public abstract class TtRayTracingShadingEnv : TtShadingEnv
    {
        public virtual string MainName { get; set; }
        private NxRHI.TtRayTracingEffect mCurrentEffect;
        public NxRHI.TtRayTracingEffect CurrentEffect
        {
            get => mCurrentEffect;
        }

        public bool IsReady
        {
            get => CurrentEffect != null;
        }
        public override string ToString()
        {
            return base.ToString();
        }
        public override async Thread.Async.TtTask UpdatePermutation()
        {
            await base.UpdatePermutation();
        }
        protected override async Thread.Async.TtTask<bool> OnUpdatePermutation(FPermutationId targetPermutationId)
        {
            mCurrentEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetRayTracingEffect(CodeName,
                MainName, this, null, null);
            System.Diagnostics.Debug.Assert(targetPermutationId == CurrentEffect.PermutationId);

            return true;
        }
        public virtual void OnDrawCall(NxRHI.TtRayTracingDraw drawcall, TtRenderPolicy policy)
        {

        }
        public void SetDispatchRay(object tagObject, TtRenderPolicy policy, NxRHI.TtRayTracingDraw drawcall, uint w, uint h, uint d)
        {
            drawcall.TagObject = tagObject;

            drawcall.BindShaderEffect(CurrentEffect);
            drawcall.SetDispatchRay(w, h, d);

            this.OnDrawCall(drawcall, policy);
        }
        //public void SetDrawcallIndirectDispatch(object tagObject, TtRenderPolicy policy, NxRHI.TtRayTracingDraw drawcall, NxRHI.TtBuffer indirectBuffer)
        //{
        //    drawcall.TagObject = tagObject;
        //    //drawcall.SetComputeEffect(CurrentEffect);
        //    drawcall.BindIndirectDispatchArgsBuffer(indirectBuffer);

        //    this.OnDrawCall(drawcall, policy);
        //}
        protected override void EnvShadingDefines(in FPermutationId id, TtShaderDefinitions defines)
        {
            
        }
    }

    public class TtDummyShading : Shader.TtGraphicsShadingEnv
    {
        public TtDummyShading()
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
        public override string ToString()
        {
            return base.ToString();
            //string code = $"//{CodeName}:Permutation={mCurrentPermutationId}\n";
            //var indexers = NxRHI.TtShader.TtCBufferVarIndexer.CBufferVarIndexers;
            //foreach (var i in indexers)
            //{
            //    string Name = "";
            //    if (i.FindFirstShaderVarName<float>(ref Name))
            //    {
            //        code += $"result += {Name};\n";
            //    }
            //    else if (i.FindFirstShaderVarName<Vector2>(ref Name))
            //    {
            //        code += $"result += {Name}.x;\n";
            //    }
            //    else if (i.FindFirstShaderVarName<Vector3>(ref Name))
            //    {
            //        code += $"result += {Name}.x;\n";
            //    }
            //    else if (i.FindFirstShaderVarName<Vector4>(ref Name))
            //    {
            //        code += $"result += {Name}.x;\n";
            //    }
            //    else if (i.FindFirstShaderVarName<int>(ref Name))
            //    {
            //        code += $"result += (float){Name};\n";
            //    }
            //    else if (i.FindFirstShaderVarName<Vector2i>(ref Name))
            //    {
            //        code += $"result += (float){Name}.x;\n";
            //    }
            //    else if (i.FindFirstShaderVarName<Vector3i>(ref Name))
            //    {
            //        code += $"result += (float){Name}.x;\n";
            //    }
            //    else if (i.FindFirstShaderVarName<Vector4i>(ref Name))
            //    {
            //        code += $"result += (float){Name}.x;\n";
            //    }
            //    else if (i.FindFirstShaderVarName<Matrix>(ref Name))
            //    {
            //        code += $"result += (float){Name}[0];\n";
            //    }
            //    else
            //    {
            //        //int xx = 0;
            //    }
            //}
            //return code;
        }
        protected override void EnvShadingDefines(in FPermutationId id, TtShaderDefinitions defines)
        {
            base.EnvShadingDefines(id, defines);

            var indexers = NxRHI.TtShader.TtCBufferVarIndexer.CBufferVarIndexers;
            foreach (var i in indexers)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Info, $"请确保Shaders/ShadingEnv/DummyShading.cginc包含了{i.GetType().FullName}的cbuffer定义");
            }
            //var code = ToString();
            //defines.AddDefine("DummyShadingUseCBuffer", code);
        }
    }
    public class TtMacrossShadingEnvAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtMacrossShadingEnv.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "McShading";
        }
        public override async Thread.Async.TtTask<IO.IAsset> GetAsset(params object[] args)
        {
            //return await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
            return null;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return false;
        }
        [Rtti.Meta("")]
        public string ShaderType { get; set; }
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = TtShaderAsset.AssetExt)]
        public RName TemplateName
        {
            get;
            set;
        }
    }
    [Rtti.Meta("")]
    [TtMacrossShadingEnv.TtMacrossShadingEnvImport]
    [IO.AssetCreateMenu(MenuName = "FX/McShader")]
    [EngineNS.Editor.TtAssetEditor(EditorType = typeof(TtMacrossShadingEnvEditor))]
    public class TtMacrossShadingEnv : TtComputeShadingEnv, IO.IAsset
    {
        public const string AssetExt = ".mcshading";
        public string TypeExt { get => AssetExt; }
        public class TtMacrossShadingEnvImportAttribute : IO.CommonCreateAttribute
        {
            protected override bool CheckAsset()
            {
                var material = mAsset as TtMacrossShadingEnv;
                if (material == null)
                    return false;

                return true;
            }
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                mAsset = new TtMacrossShadingEnv();
                PGAsset.Target = this;
            }
            protected override bool DoImportAsset()
            {
                base.DoImportAsset();

                var ameta = mAsset.GetAMeta() as TtMacrossShadingEnvAMeta;
                mAsset.SaveAssetTo(mAsset.AssetName);

                return true;
            }
        }
        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtMacrossShadingEnvAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
            IO.TtFileManager.SaveObjectToXml(name.Address, this);

            name.AMeta.AddAssetFile(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address, true);
        }
        public static TtMacrossShadingEnv LoadAsset(RName rn)
        {
            var result = new TtMacrossShadingEnv();
            IO.TtFileManager.LoadXmlToObject(rn.Address, result);
            return result;
        }
        [Rtti.Meta("")]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion
        public override Vector3ui DispatchArg
        {
            get
            {
                if (McObject != null && McObject.Get() != null)
                {
                    return McObject.Get().GetDispatchArg(this);
                }
                else
                {
                    return Vector3ui.Zero;
                }
            }
        }
        protected override void EnvShadingDefines(in FPermutationId id, TtShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);

        }
        [Category("Option")]
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtShaderAsset.AssetExt, ShaderType = "ComputeShadingMacross")]
        public override RName CodeName
        {
            get => base.CodeName;
            set
            {
                base.CodeName = value;
                ShaderAsset = value.GetAsset<TtShaderAsset>().GetResultUntilCompleted();
            }
        }
        [Category("Option")]
        [Rtti.Meta("")]
        public override string MainName
        {
            get => base.MainName;
            set => base.MainName = value;
        }
        [Category("Option")]
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = Bricks.CodeBuilder.TtMacross.AssetExt, MacrossType = typeof(TtShadingMacross))]
        public RName McName
        {
            get
            {
                if (mMcObject == null)
                    return null;
                return mMcObject.Name;
            }
            set
            {
                if (value == null)
                {
                    mMcObject = null;
                    return;
                }
                if (mMcObject == null)
                {
                    mMcObject = Macross.TtMacrossGetter<TtShadingMacross>.NewInstance();
                }
                mMcObject.Name = value;
            }
        }
        Macross.TtMacrossGetter<TtShadingMacross> mMcObject;
        public Macross.TtMacrossGetter<TtShadingMacross> McObject
        {
            get => mMcObject;
        }

        public Shader.TtShaderAsset ShaderAsset { get; protected set; }
    }
    [Macross.TtMacross(IsGenShader = true)]
    public partial class TtShadingMacross : Macross.AuxMacrossObject
    {
        [Rtti.Meta("")]
        public virtual Vector3ui GetDispatchArg(TtMacrossShadingEnv shading)
        {
            return Vector3ui.Zero;
        }
        [Rtti.Meta("",ShaderName = "CSMacrossShaderMain")]
        public virtual void CSMacrossShaderMain(TtMacrossShadingEnv shading)
        {

        }
    }

    public partial class TtMacrossShadingEnvEditor : EngineNS.Editor.IAssetEditor, IRootForm
    {
        #region IAssetEditor
        public RName AssetName { get; set; }
        public EGui.Controls.PropertyGrid.TtPropertyGrid AssetPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        public float LeftWidth = 0;
        public TtMacrossShadingEnv ShaderAsset;
        bool IsStarting = false;
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Thread.Async.TtTask<bool> OpenEditor(EngineNS.Editor.TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            if (IsStarting)
                return false;

            IsStarting = true;

            AssetName = name;
            IsStarting = false;

            await AssetPropGrid.Initialize();

            ShaderAsset = TtMacrossShadingEnv.LoadAsset(name);

            mShaderEditor.mCoreObject.ApplyLangDefine();
            mShaderEditor.mCoreObject.ApplyErrorMarkers();
            if (ShaderAsset.CodeName != null)
            {
                mShaderEditor.mCoreObject.SetText(ShaderAsset.ShaderAsset.GetShaderCode());
            }

            AssetPropGrid.Target = this;
            return true;
        }
        public void OnCloseEditor()
        {
        }
        public void OnEvent(in Bricks.Input.Event e)
        {
            //PreviewViewport.OnEvent(ref e);
        }
        #endregion

        public void Dispose()
        {

        }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public IRootForm GetRootForm()
        {
            return this;
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        #region DrawUI
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public bool IsDrawing { get; set; }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.TtMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(IsDrawing);

            DrawNodeDetails();
            DrawTextEditor();
        }
        bool mDockInitialized = false;
        protected void ResetDockspace(bool force = false)
        {
            var pos = ImGuiAPI.GetCursorPos();
            var id = ImGuiAPI.GetID(AssetName.Name + "_Dockspace");
            mDockKeyClass.ClassId = id;
            ImGuiAPI.DockSpace(id, Vector2.Zero, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, mDockKeyClass);
            if (mDockInitialized && !force)
                return;
            ImGuiAPI.DockBuilderRemoveNode(id);
            ImGuiAPI.DockBuilderAddNode(id, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None);
            ImGuiAPI.DockBuilderSetNodePos(id, pos);
            ImGuiAPI.DockBuilderSetNodeSize(id, Vector2.One);
            mDockInitialized = true;

            var rightId = id;
            uint middleId = 0;
            uint downId = 0;
            uint leftId = 0;
            uint rightUpId = 0;
            uint rightDownId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.8f, ref middleId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Down, 0.5f, ref rightDownId, ref rightUpId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir.ImGuiDir_Down, 0.3f, ref downId, ref middleId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref middleId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("NodeDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("TextEditor", mDockKeyClass), middleId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                ShaderAsset.SaveAssetTo(AssetName);
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("SaveShader", in btSize))
            {
                var blob = new Support.TtBlobObject();
                mShaderEditor.mCoreObject.GetText(blob.mCoreObject);
                var code = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)blob.mCoreObject.GetData(), (int)blob.mCoreObject.GetSize());
                IO.TtFileManager.WriteAllText(ShaderAsset.CodeName.Address, code);
            }
            ImGuiAPI.SameLine(0, -1);
            // 统一Undo/Redo归口: 纯文本编辑器的撤销由TtCodeEditor原生托管(含Ctrl+Z),
            // 不进统一TtEditorHistory栈(文本粒度命令成本过高), 按钮直接转发原生实现
            if (EGui.UIProxy.CustomButton.ToolButton("Undo", in btSize))
            {
                mShaderEditor.mCoreObject.Undo();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Redo", in btSize))
            {
                mShaderEditor.mCoreObject.Redo();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Copy", in btSize))
            {
                mShaderEditor.mCoreObject.Copy();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Paste", in btSize))
            {
                mShaderEditor.mCoreObject.Paste();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Snapshot", in btSize))
            {
                var presentWindow = ImGuiAPI.GetWindowViewportData();
                Editor.TtSnapshot.Save(AssetName, ShaderAsset.GetAMeta(), presentWindow.SwapChain.mCoreObject.GetBackBuffer(0),
                            (uint)DrawOffset.X, (uint)DrawOffset.Y, (uint)GraphSize.X, (uint)GraphSize.Y, Editor.TtSnapshot.ESnapSide.Left);
            }
        }

        bool ShowMeshPropGrid = true;
        protected void DrawNodeDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "NodeDetails", ref ShowMeshPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                AssetPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        EGui.TtCodeEditor mShaderEditor = new EGui.TtCodeEditor();
        Vector2 DrawOffset;
        protected Vector2 GraphSize;
        bool ShowTextEditor = true;
        protected void DrawTextEditor()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "TextEditor", ref ShowTextEditor, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                var winPos = ImGuiAPI.GetWindowPos();
                var vpMin = ImGuiAPI.GetWindowContentRegionMin();
                var vpMax = ImGuiAPI.GetWindowContentRegionMax();
                DrawOffset.SetValue(winPos.X + vpMin.X, winPos.Y + vpMin.Y);
                GraphSize = sz;
                mShaderEditor.mCoreObject.Render(AssetName.Name, in Vector2.Zero, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        public string GetWindowsName()
        {
            return AssetName.Name;
        }
        #endregion

        [Category("Option")]
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtShaderAsset.AssetExt, ShaderType = "ComputeShadingMacross")]
        public RName CodeName
        {
            get => ShaderAsset.CodeName;
            set
            {
                ShaderAsset.CodeName = value;

                if (ShaderAsset.CodeName != null)
                {
                    mShaderEditor.mCoreObject.SetText(ShaderAsset.ShaderAsset.GetShaderCode());
                }
            }
        }
        [Category("Option")]
        [Rtti.Meta("")]
        public string MainName
        {
            get => ShaderAsset.MainName;
            set
            {
                ShaderAsset.MainName = value;
            }
        }
        [Category("Option")]
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = Bricks.CodeBuilder.TtMacross.AssetExt, MacrossType = typeof(TtShadingMacross))]
        public RName McName
        {
            get
            {
                return ShaderAsset.McName;
            }
            set
            {
                ShaderAsset.McName = value;
            }
        }
    }
}

namespace EngineNS
{
    namespace NxRHI
    {
        public partial class TtGraphicDraw
        {
            //Effect只能当Shader一个模板用，ShadingEnv才是控制当前变体检测用的，因为ShadingEnv不是单例
            public Graphics.Pipeline.Shader.TtGraphicsEffect Effect { get; private set; }
            public Graphics.Pipeline.Shader.TtGraphicsShadingEnv ShadingEnv { get; private set; }
            internal Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId PermutationId;
            public bool IsPermutationChanged()
            {
                if (ShadingEnv == null)
                    return false;
                return PermutationId != ShadingEnv.mCurrentPermutationId;
            }
            public void BindShaderEffect(Graphics.Pipeline.Shader.TtGraphicsEffect effect, Graphics.Pipeline.Shader.TtGraphicsShadingEnv shading)
            {
                Effect = effect;
                ShadingEnv = shading;
                mCoreObject.BindShaderEffect(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, effect.ShaderEffect.mCoreObject);
            }
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Graphics.Pipeline.Shader
{
	partial class TtShadingMacross
	{
		public unsafe Vector3ui macross_GetDispatchArg (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtMacrossShadingEnv shading) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = GetDispatchArg(shading);
			return _return_value;
		}
		public unsafe void macross_CSMacrossShaderMain (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtMacrossShadingEnv shading) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			CSMacrossShaderMain(shading);
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross