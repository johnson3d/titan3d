using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class UShaderEffect : AuxPtrType<NxRHI.IGraphicsEffect>
    {
        public FEffectBinder FindBinder(VNameString name)
        {
            return mCoreObject.FindBinder(name);
        }
        public UEffectBinder FindBinder(string name)
        {
            var ptr = mCoreObject.FindBinder(name);
            if (ptr.IsValidPointer == false)
                return null;
            return new UEffectBinder(ptr);
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
    public class UEffectBinder : AuxPtrType<NxRHI.FEffectBinder>
    {
        public UEffectBinder(FEffectBinder ptr)
        {
            mCoreObject = ptr;
            mCoreObject.NativeSuper.AddRef();
        }
        public UShaderVarDesc FindField(string name)
        {
            var ptr = mCoreObject.FindField(name);
            if (ptr.IsValidPointer == false)
                return null;
            return new UShaderVarDesc(ptr);
        }
        public UShaderBinder GetShaderBinder(EShaderType type = EShaderType.SDT_Unknown)
        {
            var ptr = mCoreObject.GetShaderBinder(type);
            if (ptr.IsValidPointer == false)
                return null;
            return new UShaderBinder(ptr);
        }
    }
    public class UComputeEffect : AuxPtrType<NxRHI.IComputeEffect>
    {
        public Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId PermutationId { get => mComputeShader.PermutationId; }
        internal UShader mComputeShader;
        public UShader ComputeShader
        {
            get => mComputeShader;
        }

        public FShaderBinder FindBinder(VNameString name)
        {
            return mCoreObject.FindBinder(name);
        }
        public UShaderBinder FindBinder(string name)
        {
            var ptr = mCoreObject.FindBinder(name);
            if (ptr.IsValidPointer == false)
                return null;
            return new UShaderBinder(ptr);
        }
        public unsafe static UComputeEffect Load(Hash160 hash)
        {
            var path = UEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.Shader);
            var file = path + hash.ToString() + UShader.AssetExt;
            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return null;
                var csShader = NxRHI.UShader.Load(xnd);
                if (csShader == null)
                    return null;
                var result = UEngine.Instance.GfxDevice.RenderContext.CreateComputeEffect(csShader);
                return result;
            }   
        }
        public void SaveTo(RName shader, in Hash160 hash)
        {
            ComputeShader.SaveTo(shader, in hash);
        }
    }
}
