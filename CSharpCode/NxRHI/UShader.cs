using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class TtShader : AuxPtrType<NxRHI.IShader>
    {
        #region ShaderBinder
        public class TtShaderVarAttribute : Attribute
        {
            public System.Type VarType;
            public int NumElement = 1;
        }
        public class TtShaderBinderIndexer
        {
            private static bool mFinalized = false;
            public static List<TtShaderBinderIndexer> GlobalShaderBinderIndexers { get; } = new List<TtShaderBinderIndexer>();
            internal static void RemoveGlobalBinderIndexer(TtShaderBinderIndexer obj)
            {
                lock (GlobalShaderBinderIndexers)
                {
                    GlobalShaderBinderIndexers.Remove(obj);
                }
            }
            public static void FinalCleanup()
            {
                mFinalized = true;
                foreach (var i in GlobalShaderBinderIndexers)
                {
                    i.Dispose();
                }
                GlobalShaderBinderIndexers.Clear();
            }
            public TtShaderBinderIndexer()
            {
                if (mFinalized)
                    return;
                lock (GlobalShaderBinderIndexers)
                {
                    GlobalShaderBinderIndexers.Add(this);
                }
            }
            public void Dispose()
            {
                if (mEffect == null)
                    return;
                var members = this.GetType().GetFields();
                foreach (var i in members)
                {
                    var attrs = i.GetCustomAttributes(typeof(TtShaderVarAttribute), true);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }
                    i.SetValue(this, null);
                }
                mEffect = null;
            }
            NxRHI.TtShaderEffect mEffect;
            public unsafe void UpdateBindResouce(NxRHI.TtShaderEffect effect)
            {
                if (mEffect != null)
                    return;
                mEffect = effect;
                var members = this.GetType().GetFields();
                foreach (var i in members)
                {
                    var attrs = i.GetCustomAttributes(typeof(TtShaderVarAttribute), true);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }
                    var index = effect.FindBinder(i.Name);
                    i.SetValue(this, index);
                }
            }

            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder cbPerViewport;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder cbPerFrame;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder cbPerCamera;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder cbPerMesh;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder cbPreFramePerMesh;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder cbPerMaterial;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSrView))]
            public NxRHI.TtEffectBinder gEnvMap;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSrView))]
            public NxRHI.TtEffectBinder gShadowMap;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSampler))]
            public NxRHI.TtEffectBinder Samp_gEnvMap;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSampler))]
            public NxRHI.TtEffectBinder Samp_gShadowMap;
        }
        public class AuxShaderBinderIndexer<T> : TtShaderBinderIndexer where T : TtShaderBinderIndexer, new()
        {
            public static T Instance { get; } = new T();
        }
        public class TtCommonShaderResourceIndexer : AuxShaderBinderIndexer<TtCommonShaderResourceIndexer>
        {

        }
        public class TtCBufferVarIndexer
        {
            private static bool mFinalized = false;
            public static List<TtCBufferVarIndexer> CBufferVarIndexers { get; } = new List<TtCBufferVarIndexer>();
            public static void FinalCleanup()
            {
                mFinalized = true;
                foreach (var i in CBufferVarIndexers)
                {
                    i.Dispose();
                }
                CBufferVarIndexers.Clear();
            }
            public void Dispose()
            {
                var members = this.GetType().GetFields();
                foreach (var i in members)
                {
                    if (i.FieldType != typeof(FShaderVarDesc))
                        continue;
                    var attrs = i.GetCustomAttributes(typeof(TtShaderVarAttribute), true);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }
                    i.SetValue(this, null);
                }
                mBinder = null;
            }
            public TtCBufferVarIndexer()
            {
                if (mFinalized)
                    return;
                CBufferVarIndexers.Add(this);
            }
            TtShaderBinder mBinder;
            public TtShaderBinder Binder
            {
                get => mBinder;
            }
            public uint BufferSize;
            public bool FindShaderVar(string name, ref FShaderVarDesc result)
            {
                var members = this.GetType().GetFields();
                foreach (var i in members)
                {
                    if (i.FieldType != typeof(FShaderVarDesc))
                        continue;
                    var attrs = i.GetCustomAttributes(typeof(TtShaderVarAttribute), true);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }
                    if (i.Name == name)
                    {
                        result = (FShaderVarDesc)i.GetValue(this);
                        return true;
                    }
                }
                return false;
            }
            public bool FindFirstShaderVarName<Type>(ref string result)
            {
                var members = this.GetType().GetFields();
                foreach (var i in members)
                {
                    if (i.FieldType != typeof(FShaderVarDesc))
                        continue;
                    var attrs = i.GetCustomAttributes(typeof(TtShaderVarAttribute), true);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }
                    var var = attrs[0] as TtShaderVarAttribute;
                    if (var.VarType == typeof(Type))
                    {
                        result = i.Name;
                        return true;
                    }
                }
                return false;
            }
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
                var binder = shader.Reflector.FindBinder(EShaderBindType.SBT_CBV, name);
                if (binder.IsValidPointer == false)
                    return false;
                UpdateFieldVar(new TtShaderBinder(binder));
                return true;
            }
            private unsafe void UpdateFieldVar(TtShaderBinder binder)
            {
                if (mBinder != null)
                    return;
                mBinder = binder;
                BufferSize = binder.Size;
                var members = this.GetType().GetFields();
                foreach (var i in members)
                {
                    var attrs = i.GetCustomAttributes(typeof(TtShaderVarAttribute), true);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }

                    var varAttr = attrs[0] as TtShaderVarAttribute;
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
        }
        public class AuxCBufferVarIndexer<T> : TtCBufferVarIndexer where T : TtCBufferVarIndexer, new()
        {
            public static T Instance { get; } = new T();
        }
        #endregion

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
        public string TypeExt { get => AssetExt; }
        public unsafe void SaveTo(RName shader, in Hash160 hash, EShaderType eShader)
        {
            if (eShader != EShaderType.SDT_ComputeShader)
                return;
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.ComputeEffect);
            var file = path + hash.ToString() + TtShader.AssetExt;
            var xnd = new IO.TtXndHolder("UShader", 0, 0);

            var descAttr = new XndAttribute(xnd.RootNode.mCoreObject.GetOrAddAttribute("Desc", 0, 0, true));
            using (var ar = descAttr.GetWriter(30))
            {
                ar.Write(shader);
                ar.Write(this.PermutationId);
                ar.Write(hash);

                ar.Write(mCoreObject.GetDesc().Type);
            }

            using (var vsNode = xnd.mCoreObject.NewNode("ShaderDesc", 0, 0))
            {
                xnd.RootNode.mCoreObject.AddNode(vsNode);
                mCoreObject.GetDesc().SaveXnd(vsNode);
            }

            xnd.SaveXnd(file);
        }
        public unsafe static TtShader Load(IO.TtXndHolder xnd, out Hash160 hash)
        {
            Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId permutationId;
            var desc = LoadDesc(xnd, out hash, out permutationId);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var result = rc.CreateShader(desc);
            result.PermutationId = permutationId;
            return result;
        }
        public static unsafe TtShaderDesc LoadDesc(IO.TtXndHolder xnd, out Hash160 hash, out Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId permutationId)
        {
            hash = Hash160.Emtpy;
            permutationId = new Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId();
            var descAttr = xnd.RootNode.mCoreObject.TryGetAttribute("Desc");
            if (descAttr.IsValidPointer == false)
                return null;
            using (var ar = descAttr.GetReader(null))
            {
                RName shader;
                ar.Read(out shader);
                ar.Read(out permutationId);
                ar.Read(out hash);
            }

            var vsNode = xnd.RootNode.mCoreObject.TryGetChildNode("ShaderDesc");
            if (vsNode.IsValidPointer == false)
                return null;

            var desc = new TtShaderDesc();
            if (desc.mCoreObject.LoadXnd(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, vsNode) == false)
                return null;

            return desc;
        }
        public static unsafe bool SaveDesc(IO.TtXndHolder xnd, RName shader, in Hash160 hash, TtShaderDesc desc, Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId permutationId)
        {
            permutationId = new Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId();
            var descAttr = xnd.RootNode.mCoreObject.GetOrAddAttribute("Desc", 0, 0, true);
            using (var ar = descAttr.GetWriter(256))
            {
                ar.Write(shader);
                ar.Write(permutationId);
                var shadingCode = Editor.ShaderCompiler.TtShaderCodeManager.Instance.GetShaderCode(shader);
                //System.Diagnostics.Debug.Assert(hash == shadingCode.CodeHash);
                ar.Write(hash);
            }

            var vsNode = xnd.RootNode.mCoreObject.GetOrAddNode("ShaderDesc", 0, 0, true);
            desc.mCoreObject.SaveXnd(vsNode);
            return true;
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
