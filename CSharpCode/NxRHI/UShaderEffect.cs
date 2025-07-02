using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class TtShaderEffect : AuxPtrType<NxRHI.IGraphicsEffect>
    {
        public void BindInputLayout(TtInputLayout layout)
        {
            mCoreObject.BindInputLayout(layout.mCoreObject);
        }
        public FEffectBinder FindBinder(VNameString name)
        {
            return mCoreObject.FindBinder(name);
        }
        public TtEffectBinder FindBinder(string name)
        {
            var ptr = mCoreObject.FindBinder(name);
            if (ptr.IsValidPointer == false)
                return null;
            return new TtEffectBinder(ptr);
        }
        public string DebugName
        {
            get
            {
                return mCoreObject.NativeSuper.GetDebugName();
            }
            set
            {
                mCoreObject.NativeSuper.SetDebugName(value);
            }
        }
    }
    public class TtEffectBinder : AuxPtrType<NxRHI.FEffectBinder>
    {
        public TtEffectBinder(FEffectBinder ptr)
        {
            mCoreObject = ptr;
            mCoreObject.NativeSuper.AddRef();
        }
        public override void Dispose()
        {
            base.Dispose();
        }
        public TtShaderVarDesc FindField(string name)
        {
            var ptr = mCoreObject.FindField(name);
            if (ptr.IsValidPointer == false)
                return null;
            return new TtShaderVarDesc(ptr);
        }
        public TtShaderBinder GetShaderBinder(EShaderType type = EShaderType.SDT_Unknown)
        {
            var ptr = mCoreObject.GetShaderBinder(type);
            if (ptr.IsValidPointer == false)
                return null;
            return new TtShaderBinder(ptr);
        }
    }
    public class TtComputeEffect : AuxPtrType<NxRHI.IComputeEffect>
    {
        public Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId PermutationId { get => mComputeShader.PermutationId; }
        internal TtShader mComputeShader;
        public TtShader ComputeShader
        {
            get => mComputeShader;
        }

        public FShaderBinder FindBinder(VNameString name)
        {
            return mCoreObject.FindBinder(name);
        }
        public TtShaderBinder FindBinder(string name)
        {
            var ptr = mCoreObject.FindBinder(name);
            if (ptr.IsValidPointer == false)
                return null;
            return new TtShaderBinder(ptr);
        }
        public unsafe static TtComputeEffect Load(Hash160 hash)
        {
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.ComputeEffect);
            var file = path + hash.ToString() + TtShader.AssetExt;
            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return null;
                Hash160 fileHash;
                var csShader = NxRHI.TtShader.Load(xnd, out fileHash);
                if (csShader == null || fileHash != hash)
                    return null;
                var result = TtEngine.Instance.GfxDevice.RenderContext.CreateComputeEffect(csShader);
                return result;
            }   
        }
        public void SaveTo(RName shader, in Hash160 hash)
        {
            ComputeShader.SaveTo(shader, in hash, EShaderType.SDT_ComputeShader);
        }
    }

    public class TtRayTracingEffect : AuxPtrType<NxRHI.IRayTracingEffect>
    {
        public class TtRTShaderLibDesc
        {
            public class TtHitGroup
            {
                [Rtti.Meta("")]
                public string Name { get; set; }
                [Rtti.Meta("")]
                public string ClosestHitShader { get; set; }
                [Rtti.Meta("")]
                public string AnyHitShader { get; set; }
                [Rtti.Meta("")]
                public string IntersectionShader { get; set; }
                [Rtti.Meta("")]
                public List<string> LocalSignatures { get; set; }
            }
            [Rtti.Meta("")]
            public uint MaxRecursionDepth { get; set; } = 1;
            [Rtti.Meta("")]
            public uint PayloadSize { get; set; }
            [Rtti.Meta("")]
            public uint AttributeSize { get; set; }
            [Rtti.Meta("")]
            public List<string> Functions { get; set; }
            [Rtti.Meta("")]
            public List<string> GlobalSignatures { get; set; }
            [Rtti.Meta("")]
            public List<TtHitGroup> HitGroups { get; set; }
            [Rtti.Meta("")]
            public string RayGenShader { get; set; }
            [Rtti.Meta("")]
            public string MissShader { get; set; }
        }
        public NxRHI.TtRayTracingEffect.TtRTShaderLibDesc ShaderLibDesc { get; set; }
        public const string AssetExt = ".shaderlib";
        public TtShaderDesc ShaderDesc;
        public Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId mPermutationId;
        public Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId PermutationId { get => mPermutationId; }
        public unsafe static TtRayTracingEffect Load(Hash160 hash)
        {
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.RayTracingEffect);
            var file = path + hash.ToString() + AssetExt;
            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return null;
                Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId permutationId;
                Hash160 fileHash;
                var csShader = NxRHI.TtShader.LoadDesc(xnd, out fileHash, out permutationId);
                if (csShader == null || fileHash != hash)
                    return null;
                TtRTShaderLibDesc shaderLibDesc;
                var descAttr = xnd.RootNode.mCoreObject.TryGetAttribute("ShaderLibDesc");
                using (var ar = descAttr.GetReader(null))
                {
                    string jsText;
                    ar.Read(out jsText);
                    shaderLibDesc = IO.TtFileManager.LoadObjectFromJson<NxRHI.TtRayTracingEffect.TtRTShaderLibDesc>(jsText);
                }
                var result = TtEngine.Instance.GfxDevice.RenderContext.CreateRayTracingEffect(csShader, shaderLibDesc);
                result.mPermutationId = permutationId;
                result.ShaderDesc = csShader;

                return result;
            }
        }
        public void SaveTo(RName shader, in Hash160 hash)
        {
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.RayTracingEffect);
            var file = path + hash.ToString() + AssetExt;
            using (var xnd = new IO.TtXndHolder("TtRayTracingEffect", 0, 0))
            {
                var descAttr = xnd.RootNode.mCoreObject.GetOrAddAttribute("ShaderLibDesc", 0, 0, true);
                using (var ar = descAttr.GetWriter(256))
                {
                    var jsText = IO.TtFileManager.SaveObjectToJson(ShaderLibDesc);
                    ar.Write(jsText);
                }
                NxRHI.TtShader.SaveDesc(xnd, shader, hash, ShaderDesc, PermutationId);
                xnd.SaveXnd(file);
            }
        }

        public FShaderBinder FindBinder(EShaderBindType type, string name)
        {
            return mCoreObject.FindBinder(type, VNameString.FromString(name));
        }
    }
}
