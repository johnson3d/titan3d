using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class TtShader : AuxPtrType<NxRHI.IShader>
    {
        public class UShaderVarAttribute : Attribute
        {
            public System.Type VarType;
            public int NumElement = 1;
        }
        public class UShaderBinderIndexer
        {
            NxRHI.TtShaderEffect mEffect;
            public unsafe void UpdateBindResouce(NxRHI.TtShaderEffect effect)
            {
                if (mEffect != null)
                    return;
                mEffect = effect;
                var members = this.GetType().GetFields();
                foreach (var i in members)
                {
                    var attrs = i.GetCustomAttributes(typeof(UShaderVarAttribute), true);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }
                    var index = effect.FindBinder(i.Name);
                    i.SetValue(this, index);
                }
            }
        }
        public class UShaderVarIndexer
        {
            TtShaderBinder mBinder;
            public TtShaderBinder Binder
            {
                get => mBinder;
            }
            public uint BufferSize;
            
            public bool UpdateFieldVar(IGraphicsEffect effect, string name)
            {
                if (mBinder != null)
                    return true;
                var binder = effect.FindBinder(VNameString.FromString(name)).GetShaderBinder(EShaderType.SDT_Unknown);
                if (binder.IsValidPointer == false)
                    return false;
                UpdateFieldVar(new TtShaderBinder(binder));
                return true;
            }
            public bool UpdateFieldVar(TtShader shader, string name)
            {
                if (mBinder != null)
                    return true;
                var binder = shader.Reflector.FindBinder(EShaderBindType.SBT_CBuffer, name);
                if (binder.IsValidPointer == false)
                    return false;
                UpdateFieldVar(new TtShaderBinder(binder));
                return true;
            }
            public unsafe void UpdateFieldVar(TtShaderBinder binder)
            {
                if (mBinder != null)
                    return;
                mBinder = binder;
                BufferSize = binder.Size;
                var members = this.GetType().GetFields();
                foreach (var i in members)
                {
                    var attrs = i.GetCustomAttributes(typeof(UShaderVarAttribute), true);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }

                    var varAttr = attrs[0] as UShaderVarAttribute;
                    var fld = binder.FindField(i.Name);
                    if (fld.IsValidPointer)
                    {
                        i.SetValue(this, fld);
                    }
                    else
                    {
                        Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, $"CB({binder.mCoreObject.Name.c_str()}) can't find {i.Name}");
                    }
                }
            }
            public UShaderVarIndexer NextIndexer = null;
        }
        internal TtShaderReflector mReflector;
        public TtShaderReflector Reflector
        {
            get
            {
                if (mReflector == null)
                {
                    mReflector = new TtShaderReflector();
                    mReflector.mCoreObject = mCoreObject.GetReflector();
                    mReflector.mCoreObject.NativeSuper.AddRef();
                }
                return mReflector;
            }
        }

        public Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId PermutationId { get; set; }
        public const string AssetExt = ".shader";
        public unsafe void SaveTo(RName shader, in Hash160 hash)
        {
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.Shader);
            var file = path + hash.ToString() + TtShader.AssetExt;
            var xnd = new IO.TtXndHolder("UShader", 0, 0);

            var descAttr = new XndAttribute(xnd.RootNode.mCoreObject.GetOrAddAttribute("Desc", 0, 0));
            using (var ar = descAttr.GetWriter(30))
            {
                ar.Write(shader);
                ar.Write(this.PermutationId);
                var shadingCode = Editor.ShaderCompiler.TtShaderCodeManager.Instance.GetShaderCode(shader);
                ar.Write(shadingCode.CodeHash);

                ar.Write(mCoreObject.GetDesc().Type);
            }

            using (var vsNode = xnd.mCoreObject.NewNode("ShaderDesc", 0, 0))
            {
                xnd.RootNode.mCoreObject.AddNode(vsNode);
                mCoreObject.GetDesc().SaveXnd(vsNode);
            }

            xnd.SaveXnd(file);
        }
        public unsafe static TtShader Load(IO.TtXndHolder xnd)
        {
            var descAttr = xnd.RootNode.mCoreObject.TryGetAttribute("Desc");
            if (descAttr.IsValidPointer == false)
                return null;
            Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId permutationId = new Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId();
            using (var ar = descAttr.GetReader(null))
            {
                RName shader;
                ar.Read(out shader);
                ar.Read(out permutationId);
                Hash160 hash;
                ar.Read(out hash);
                var shadingCode = Editor.ShaderCompiler.TtShaderCodeManager.Instance.GetShaderCode(shader);
                if (shadingCode.CodeHash != hash)
                    return null;
            }
            
            var vsNode = xnd.RootNode.mCoreObject.TryGetChildNode("ShaderDesc");
            if (vsNode.IsValidPointer == false)
                return null;

            var desc = new TtShaderDesc();
            if (desc.mCoreObject.LoadXnd(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, vsNode) == false)
                return null;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var result = rc.CreateShader(desc);
            result.PermutationId = permutationId;
            return result;
        }
        public void SetDebugName(string name)
        {

        }
    }
    public class TtShaderReflector : AuxPtrType<NxRHI.IShaderReflector>
    {
        public FShaderBinder FindBinder(EShaderBindType type, string name)
        {
            return mCoreObject.FindBinder(type, name);
        }
    }
    public class TtShaderVarDesc : AuxPtrType<NxRHI.FShaderVarDesc>
    {
        public TtShaderVarDesc(NxRHI.FShaderVarDesc ptr)
        {
            mCoreObject = ptr;
            mCoreObject.NativeSuper.AddRef();
        }
        //public void CheckIndex(UShaderEffect effect)
        //{
        //    if (mCoreObject.IsValidPointer)
        //        return;
        //    var cb = effect.FindBinder(CBuffer.c_str());
        //    mCoreObject = cb.FindField(Var.c_str());
        //    mCoreObject.NativeSuper.AddRef();
        //}
    }
    public class TtShaderBinder : AuxPtrType<NxRHI.FShaderBinder>
    {
        public TtShaderBinder(FShaderBinder ptr)
        {
            mCoreObject = ptr;
            mCoreObject.NativeSuper.AddRef();
        }
        public uint Size
        {
            get { return mCoreObject.Size; }
        }
        public FShaderVarDesc FindField(string name)
        {
            return mCoreObject.FindField(name);
        }
    }
}
