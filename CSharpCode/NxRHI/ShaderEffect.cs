using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class TtNativeGraphicsEffect : AuxPtrType<NxRHI.IGraphicsEffect>
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
        public const string AssetExt = ".compute";
        public string TypeExt { get => AssetExt; }

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
        public Hash160 CSShaderHash { get; set; }
        public unsafe static TtComputeEffect Load(Hash160 hash)
        {
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.ComputeEffect);
            var file = path + hash.ToString() + TtComputeEffect.AssetExt;
            if (!IO.TtFileManager.FileExists(file))
                return null;
            // Read the effect file to get the shader RhiDataHash
            Hash160 shaderHash;
            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return null;
                var descAttr = xnd.RootNode.mCoreObject.TryGetAttribute("Desc");
                if (descAttr.IsValidPointer == false)
                    return null;
                using (var ar = descAttr.GetReader(null))
                {
                    RName shaderName;
                    ar.Read(out shaderName);
                    Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId permutationId;
                    ar.Read(out permutationId);
                    Hash160 effectHash;
                    ar.Read(out effectHash);
                    ar.Read(out shaderHash);
                }
            }
            if (shaderHash == Hash160.Emtpy)
                return null;
            // Load actual shader binary from cache/shaders/
            var desc = Editor.ShaderCompiler.TtHLSLCompiler.LoadShaderFromCache(shaderHash, EShaderType.SDT_ComputeShader);
            if (desc == null)
                return null;
            var csShader = TtEngine.Instance.GfxDevice.RenderContext.CreateShader(desc);
            if (csShader == null)
                return null;
            var result = TtEngine.Instance.GfxDevice.RenderContext.CreateComputeEffect(csShader);
            if (result != null)
                result.CSShaderHash = shaderHash;
            return result;
        }
        public unsafe void SaveTo(RName shader, in Hash160 hash)
        {
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.ComputeEffect);
            var file = path + hash.ToString() + TtComputeEffect.AssetExt;
            var xnd = new IO.TtXndHolder("TtShader", 0, 0);

            var descAttr = new XndAttribute(xnd.RootNode.mCoreObject.GetOrAddAttribute("Desc", 0, 0, true));
            using (var ar = descAttr.GetWriter(30))
            {
                ar.Write(shader);
                ar.Write(this.PermutationId);
                ar.Write(hash);
                ar.Write(CSShaderHash);
            }
            xnd.SaveXnd(file);
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
        public Hash160 RTShaderHash { get; set; }
        public Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId mPermutationId;
        public Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId PermutationId { get => mPermutationId; }
        public unsafe static TtRayTracingEffect Load(Hash160 hash)
        {
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.RayTracingEffect);
            var file = path + hash.ToString() + AssetExt;
            if (!IO.TtFileManager.FileExists(file))
                return null;
            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return null;

                // Read shader hash and shaderLibDesc
                Hash160 shaderHash;
                Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId permutationId;
                var hashAttr = xnd.RootNode.mCoreObject.TryGetAttribute("Desc");
                if (hashAttr.IsValidPointer == false)
                    return null;
                using (var ar = hashAttr.GetReader(null))
                {
                    ar.Read(out permutationId);
                    ar.Read(out shaderHash);
                }

                TtRTShaderLibDesc shaderLibDesc;
                var libDescAttr = xnd.RootNode.mCoreObject.TryGetAttribute("ShaderLibDesc");
                if (libDescAttr.IsValidPointer == false)
                    return null;
                using (var ar = libDescAttr.GetReader(null))
                {
                    string jsText;
                    ar.Read(out jsText);
                    shaderLibDesc = IO.TtFileManager.LoadObjectFromJson<NxRHI.TtRayTracingEffect.TtRTShaderLibDesc>(jsText);
                }

                // Load actual shader from cache/shaders/
                var desc = Editor.ShaderCompiler.TtHLSLCompiler.LoadShaderFromCache(shaderHash, EShaderType.SDT_RayTracing);
                if (desc == null)
                    return null;

                var result = TtEngine.Instance.GfxDevice.RenderContext.CreateRayTracingEffect(desc, shaderLibDesc);
                if (result == null)
                    return null;
                result.mPermutationId = permutationId;
                result.ShaderDesc = desc;
                result.RTShaderHash = shaderHash;
                return result;
            }
        }
        public void SaveTo(RName shader, in Hash160 hash)
        {
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.RayTracingEffect);
            var file = path + hash.ToString() + AssetExt;
            using (var xnd = new IO.TtXndHolder("TtRayTracingEffect", 0, 0))
            {
                var descAttr = xnd.RootNode.mCoreObject.GetOrAddAttribute("Desc", 0, 0, true);
                using (var ar = descAttr.GetWriter(30))
                {
                    ar.Write(PermutationId);
                    ar.Write(RTShaderHash);
                }

                var libDescAttr = xnd.RootNode.mCoreObject.GetOrAddAttribute("ShaderLibDesc", 0, 0, true);
                using (var ar = libDescAttr.GetWriter(256))
                {
                    var jsText = IO.TtFileManager.SaveObjectToJson(ShaderLibDesc);
                    ar.Write(jsText);
                }
                xnd.SaveXnd(file);
            }
        }

        public FShaderBinder FindBinder(EShaderBindType type, string name)
        {
            return mCoreObject.FindBinder(type, VNameString.FromString(name));
        }
    }
}
