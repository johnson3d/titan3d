using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class TtShaderEffect : AuxPtrType<NxRHI.IGraphicsEffect>
    {
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
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.Shader);
            var file = path + hash.ToString() + TtShader.AssetExt;
            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return null;
                var csShader = NxRHI.TtShader.Load(xnd);
                if (csShader == null)
                    return null;
                var result = TtEngine.Instance.GfxDevice.RenderContext.CreateComputeEffect(csShader);
                return result;
            }   
        }
        public void SaveTo(RName shader, in Hash160 hash)
        {
            ComputeShader.SaveTo(shader, in hash);
        }
    }
}
