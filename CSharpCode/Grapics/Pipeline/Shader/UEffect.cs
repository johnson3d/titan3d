using EngineNS.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public class TtEffect : IDisposable
    {
        public const string AssetExt = ".effect";
        public string TypeExt { get => AssetExt; }
        public override string ToString()
        {
            return base.ToString();
        }
        public void Dispose()
        {
            DescMS?.Dispose();
            DescVS?.Dispose();
            DescPS?.Dispose();
            ShadingEnv = null;
            mBindIndexer = null;
            ShaderEffect = null;
        }
        public class TtEffectDesc : IO.ISerializer
        {
            public void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {

            }

            public void OnPropertyRead(object tagObject, string prop, bool fromXml)
            {

            }
            public const uint CurrentEffectVersion = 6;
            [Rtti.Meta()]
            public uint EffectVersion { get; set; } = CurrentEffectVersion;
            [Rtti.Meta()]
            public Hash160 GlobalEnvHash { get; set; }
            [Rtti.Meta()]
            public Hash160 CodeHash { get; set; }
            [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
            public Hash160 MaterialHash { get; set; }
            [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
            public Hash160 MdfQueueHash { get; set; }
            [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
            public RName MaterialName { get; set; }
            [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
            public string MdfQueueType { get; set; }
            [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
            public Shader.TtShadingEnv.FPermutationId PermutationId { get; set; }
            [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
            public string ShadingType { get; set; }
            [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
            public uint InputStreams { get; set; }

            public Hash160 EffectHash;
        }
        public TtEffectDesc Desc { get; set; } = new TtEffectDesc();
        public NxRHI.TtShaderEffect ShaderEffect { get; private set; }
        public NxRHI.TtShaderDesc DescAS { get; private set; }
        public NxRHI.TtShaderDesc DescMS { get; private set; }
        public NxRHI.TtShaderDesc DescVS { get; private set; }
        public NxRHI.TtShaderDesc DescPS { get; private set; }
        public TtShadingEnv ShadingEnv { get; internal set; }

        internal TtEffect UnsafeCloneForEditor()
        {
            var result = new TtEffect();
            result.ShaderEffect = ShaderEffect;
            result.DescAS = DescAS;
            result.DescMS = DescMS;
            result.DescVS = DescVS;
            result.DescPS = DescPS;
            result.ShadingEnv = ShadingEnv;
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.TtTypeDescGetter<TtEffectDesc>.TypeDesc);
            meta.CopyObjectMetaField(result.Desc, Desc);
            return result;
        }

        public unsafe void SaveTo(Hash160 hash)
        {
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.GraphicEffect);
            var file = path + hash.ToString() + TtEffect.AssetExt;
            var xnd = new IO.TtXndHolder("TtEffect", 0, 0);

            var descAttr = new XndAttribute(xnd.RootNode.mCoreObject.GetOrAddAttribute("Desc", 0, 0, true));
            using (var ar = descAttr.GetWriter(30))
            {
                ar.Write(Desc);
            }

            if (DescAS != null)
            {
                var asNode = xnd.mCoreObject.NewNode("ASCode", 0, 0);
                xnd.RootNode.mCoreObject.AddNode(asNode);
                DescMS.mCoreObject.SaveXnd(asNode);
                CoreSDK.PtrType_Release(asNode);
            }

            if (DescMS != null)
            {
                var msNode = xnd.mCoreObject.NewNode("MSCode", 0, 0);
                xnd.RootNode.mCoreObject.AddNode(msNode);
                DescMS.mCoreObject.SaveXnd(msNode);
                CoreSDK.PtrType_Release(msNode);
            }

            if (DescVS != null)
            {
                var vsNode = xnd.mCoreObject.NewNode("VSCode", 0, 0);
                xnd.RootNode.mCoreObject.AddNode(vsNode);
                DescVS.mCoreObject.SaveXnd(vsNode);
                CoreSDK.PtrType_Release(vsNode);
            }

            if (DescPS != null)
            {
                var psNode = xnd.mCoreObject.NewNode("PSCode", 0, 0);
                xnd.RootNode.mCoreObject.AddNode(psNode);
                DescPS.mCoreObject.SaveXnd(psNode);
                CoreSDK.PtrType_Release(psNode);
            }

            xnd.SaveXnd(file);
        }
        public static TtEffectDesc LoadEffectDesc(string file)
        {
            //var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.Effect);
            //var file = path + hash.ToString() + TtEffect.AssetExt;
            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                var descAttr = xnd.RootNode.mCoreObject.TryGetAttribute("Desc");
                IO.ISerializer desc;
                using (var ar = descAttr.GetReader(null))
                {
                    IO.SerializerHelper.Read(ar, out desc, null);
                }
                var effectDesc = desc as TtEffectDesc;
                if (effectDesc == null)
                    return null;
                return effectDesc;
            }
        }
        public static async Thread.Async.TtTask<TtEffect> LoadEffect(Hash160 hash, TtShadingEnv shading, TtMaterial material, TtMdfQueueBase mdf)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.GraphicEffect);
            var file = path + hash.ToString() + TtEffect.AssetExt;

            TtEffect result = null;
            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return null;

                XndAttribute descAttr = new XndAttribute();
                XndNode asNode = new XndNode();
                XndNode msNode = new XndNode();
                XndNode vsNode = new XndNode();
                XndNode psNode = new XndNode();
                unsafe
                {
                    descAttr = xnd.RootNode.mCoreObject.TryGetAttribute("Desc");
                    if (descAttr.IsValidPointer == false)
                        return null;
                    asNode = xnd.RootNode.mCoreObject.TryGetChildNode("ASCode");
                    
                    msNode = xnd.RootNode.mCoreObject.TryGetChildNode("MSCode");
                    if (msNode.IsValidPointer == false)
                    {
                        vsNode = xnd.RootNode.mCoreObject.TryGetChildNode("VSCode");
                        if (vsNode.IsValidPointer == false)
                            return null;
                    }
                    
                    psNode = xnd.RootNode.mCoreObject.TryGetChildNode("PSCode");
                    if (psNode.IsValidPointer == false)
                        return null;
                }

                result = await TtEngine.Instance.EventPoster.Post((Thread.Async.FPostEvent<TtEffect>)((state) =>
                {
                    var effect = new TtEffect();
                    IO.ISerializer desc;
                    using (var ar = descAttr.GetReader(effect))
                    {
                        IO.SerializerHelper.Read(ar, out desc, effect);
                    }
                    var effectDesc = desc as TtEffectDesc;
                    if (effectDesc == null)
                        return null;

                    if (effectDesc.EffectVersion != TtEffectDesc.CurrentEffectVersion)
                        return null;

                    var shadingCode = Editor.ShaderCompiler.TtShaderCodeManager.Instance.GetShaderCode(shading.CodeName);
                    if (rc.GlobalEnvHash != effectDesc.GlobalEnvHash ||
                            shadingCode.CodeHash != effectDesc.CodeHash ||
                            material.GetHash() != effectDesc.MaterialHash ||
                            mdf.GetHash() != effectDesc.MdfQueueHash)
                    {
                        return null;
                    }

                    effect.Desc = effectDesc;

                    unsafe
                    {
                        if (asNode.IsValidPointer)
                        {
                            effect.DescAS = new NxRHI.TtShaderDesc(NxRHI.EShaderType.SDT_AmplificationShader);
                            if (effect.DescAS.mCoreObject.LoadXnd(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, asNode) == false)
                                return null;
                        }
                        if (msNode.IsValidPointer)
                        {
                            effect.DescMS = new NxRHI.TtShaderDesc(NxRHI.EShaderType.SDT_MeshShader);
                            if (effect.DescMS.mCoreObject.LoadXnd(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, msNode) == false)
                                return null;
                        }
                        else
                        {
                            effect.DescVS = new NxRHI.TtShaderDesc(NxRHI.EShaderType.SDT_VertexShader);
                            if (effect.DescVS.mCoreObject.LoadXnd(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, vsNode) == false)
                                return null;
                        }
                        
                        effect.DescPS = new NxRHI.TtShaderDesc(NxRHI.EShaderType.SDT_PixelShader);
                        if (effect.DescPS.mCoreObject.LoadXnd(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, psNode) == false)
                            return null;
                        effect.DescVS.mCoreObject.Type = NxRHI.EShaderType.SDT_VertexShader;
                        effect.DescPS.mCoreObject.Type = NxRHI.EShaderType.SDT_PixelShader;
                    }
                    return effect;
                }), Thread.Async.EAsyncTarget.AsyncIO);

                if (result == null)
                    return null;
            }

            NxRHI.TtShader AmplificationShader = null;
            NxRHI.TtShader MeshShader = null;
            NxRHI.TtShader VertexShader = null;
            NxRHI.TtShader PixelShader = null;
            if (TtEngine.Instance.GfxDevice.RenderContext.RhiType == NxRHI.ERhiType.RHI_GL)
            {
                bool created = await TtEngine.Instance.EventPoster.Post((state) =>
                {
                    VertexShader = rc.CreateShader(result.DescVS);
                    if (VertexShader == null)
                        return false;
                    PixelShader = rc.CreateShader(result.DescPS);
                    if (PixelShader == null)
                        return false;
                    return true;
                }, Thread.Async.EAsyncTarget.Render);
                if (created == false)
                    return null;
            }
            else
            {
                if (result.DescAS != null)
                {
                    AmplificationShader = rc.CreateShader(result.DescAS);
                }
                if (result.DescMS != null)
                {
                    MeshShader = rc.CreateShader(result.DescMS);
                }
                else
                {
                    VertexShader = rc.CreateShader(result.DescVS);
                    if (VertexShader == null)
                        return null;
                }
                
                PixelShader = rc.CreateShader(result.DescPS);
                if (PixelShader == null)
                    return null;
            }

            NxRHI.TtInputLayout InputLayout = null;
            unsafe
            {
                var layoutDesc = new NxRHI.TtInputLayoutDesc(IMesh.CreateInputLayoutDesc(result.Desc.InputStreams));
                layoutDesc.mCoreObject.SetShaderDesc(result.DescVS.mCoreObject);
                InputLayout = TtEngine.Instance.GfxDevice.RenderContext.CreateInputLayout(layoutDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, layoutDesc);
                if (InputLayout == null)
                {
                    System.Diagnostics.Debug.Assert(false);
                }

                result.ShaderEffect = rc.CreateShaderEffect(AmplificationShader, MeshShader, VertexShader, PixelShader, hash.ToString());
                if (result.ShaderEffect == null)
                    return null;
                result.ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);
            }

            var dbg_name = $"{shading},{material.AssetName},{Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(mdf.GetType())}";
            MeshShader?.SetDebugName($"MS:{dbg_name}");
            VertexShader?.SetDebugName($"VS:{dbg_name}");
            PixelShader.SetDebugName($"PS:{dbg_name}");
            result.ShaderEffect.DebugName = dbg_name;
            if (await LinkShaders(result) == false)
                return null;

            result.Desc.EffectHash = hash;
            result.ShadingEnv = shading;
            return result;
        }
        public static async Thread.Async.TtTask<TtEffect> CreateEffect(TtShadingEnv shading, TtShadingEnv.FPermutationId permutationId, TtMaterial material, TtMdfQueueBase mdf)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var result = new TtEffect();
            var shadingCode = Editor.ShaderCompiler.TtShaderCodeManager.Instance.GetShaderCode(shading.CodeName);
            if (shadingCode == null)
                return null;

            result.Desc.GlobalEnvHash = Hash160.CreateHash160(rc.GlobalEnvDefines.ToString());
            result.Desc.CodeHash = shadingCode.CodeHash;
            result.Desc.MaterialHash = material.GetHash();
            result.Desc.MdfQueueHash = mdf.GetHash();
            result.Desc.MaterialName = material.AssetName;
            result.Desc.MdfQueueType = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(mdf.GetType());
            result.Desc.ShadingType = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(shading.GetType());
            result.ShadingEnv = shading;

            var defines = new NxRHI.TtShaderDefinitions();
            shading.GetShaderDefines(in permutationId, defines);

            var cfg = TtEngine.Instance.Config;
            result.DescAS = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                var compilier = new Editor.ShaderCompiler.TtHLSLCompiler();
                compilier.MdfQueue = mdf;
                return compilier.CompileShader(shading.CodeName.Address, "AS_Main", NxRHI.EShaderType.SDT_AmplificationShader,
                    shading, material, mdf.GetType(), defines, null, null, TtEngine.Instance.Config.IsDebugShader);
            }, Thread.Async.EAsyncTarget.AsyncIO);

            result.DescMS = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                var compilier = new Editor.ShaderCompiler.TtHLSLCompiler();
                compilier.MdfQueue = mdf;
                return compilier.CompileShader(shading.CodeName.Address, "MS_Main", NxRHI.EShaderType.SDT_MeshShader,
                    shading, material, mdf.GetType(), defines, null, null, TtEngine.Instance.Config.IsDebugShader);
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result.DescMS == null)
            {
                result.DescVS = await TtEngine.Instance.EventPoster.Post((state) =>
                {
                    var compilier = new Editor.ShaderCompiler.TtHLSLCompiler();
                    compilier.MdfQueue = mdf;
                    return compilier.CompileShader(shading.CodeName.Address, "VS_Main", NxRHI.EShaderType.SDT_VertexShader,
                        shading, material, mdf.GetType(), defines, null, null, TtEngine.Instance.Config.IsDebugShader);
                }, Thread.Async.EAsyncTarget.AsyncIO);
                if (result.DescVS == null)
                    return null;
            }

            result.DescPS = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                var compilier = new Editor.ShaderCompiler.TtHLSLCompiler();
                compilier.MdfQueue = mdf;
                return compilier.CompileShader(shading.CodeName.Address, "PS_Main", NxRHI.EShaderType.SDT_PixelShader, 
                    shading, material, mdf.GetType(), defines, null, null, TtEngine.Instance.Config.IsDebugShader);
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (result.DescPS == null)
                return null;

            NxRHI.TtShader AmplificationShader = null;
            NxRHI.TtShader MeshShader = null;
            NxRHI.TtShader VertexShader = null;
            NxRHI.TtShader PixelShader = null;
            if (TtEngine.Instance.GfxDevice.RenderContext.RhiType == NxRHI.ERhiType.RHI_GL)
            {
                bool created = await TtEngine.Instance.EventPoster.Post((state) =>
                {
                    VertexShader = rc.CreateShader(result.DescVS);
                    if (VertexShader == null)
                        return false;
                    PixelShader = rc.CreateShader(result.DescPS);
                    if (PixelShader == null)
                        return false;
                    return true;
                }, Thread.Async.EAsyncTarget.Render);
                if (created == false)
                    return null;
            }
            else
            {
                if (result.DescAS != null)
                {
                    AmplificationShader = rc.CreateShader(result.DescAS);
                }
                if (result.DescMS == null)
                {
                    VertexShader = rc.CreateShader(result.DescVS);
                    if (VertexShader == null)
                        return null;
                }
                else
                {
                    MeshShader = rc.CreateShader(result.DescMS);
                }
                
                PixelShader = rc.CreateShader(result.DescPS);
                if (PixelShader == null)
                    return null;
            }

            NxRHI.TtInputLayout InputLayout = null;
            unsafe
            {
                uint inputStreams = 0;
                mdf.mCoreObject.GetInputStreams(ref inputStreams);
                var layoutDesc = new NxRHI.TtInputLayoutDesc(IMesh.CreateInputLayoutDesc(inputStreams));
                layoutDesc.mCoreObject.SetShaderDesc(result.DescVS.mCoreObject);
                InputLayout = TtEngine.Instance.GfxDevice.RenderContext.CreateInputLayout(layoutDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, layoutDesc);

                result.Desc.InputStreams = inputStreams;

                var hash = TtEffectManager.GetShaderHash(shading, material, mdf);
                result.ShaderEffect = rc.CreateShaderEffect(AmplificationShader, MeshShader, VertexShader, PixelShader, hash.ToString());
                if (result.ShaderEffect == null)
                    return null;
                result.ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);
            }
            if (await LinkShaders(result) == false)
                return null;

            MeshShader?.SetDebugName($"MS:{shading},{material.AssetName},{Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(mdf.GetType())}");
            VertexShader?.SetDebugName($"VS:{shading},{material.AssetName},{Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(mdf.GetType())}");
            PixelShader.SetDebugName($"PS:{shading},{material.AssetName},{Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(mdf.GetType())}");
            result.ShaderEffect.DebugName = $"{shading},{material.AssetName},{Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(mdf.GetType())}";
            return result;
        }
        public async Thread.Async.TtTask<bool> RefreshEffect(TtMaterial material)
        {
            {
                var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.GraphicEffect);
                var file = path + this.Desc.EffectHash.ToString() + TtEffect.AssetExt;
                if (IO.TtFileManager.FileExists(file))
                    IO.TtFileManager.DeleteFile(file);
            }

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var defines = new NxRHI.TtShaderDefinitions();
            var type = Rtti.TtTypeDescManager.Instance.GetTypeFromString(Desc.ShadingType);
            if (type == null)
                return false;
            var shading = TtEngine.Instance.ShadingEnvManager.GetShadingEnv(type);
            if (shading == null)
                return false;
            shading.GetShaderDefines(Desc.PermutationId, defines);

            var mdfType = Rtti.TtTypeDesc.TypeOf(Desc.MdfQueueType);
            if (mdfType == null)
                return false;
            var mdf = Rtti.TtTypeDescManager.CreateInstance(mdfType) as Pipeline.Shader.TtMdfQueueBase;
            if (mdf == null)
                return false;

            mdf.Initialize(null);
            this.Desc.MaterialHash = material.GetHash();
            this.Desc.MdfQueueHash = mdf.GetHash();

            NxRHI.TtShaderDesc descVS = null;
            var descAS = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                var compilier = new Editor.ShaderCompiler.TtHLSLCompiler();
                compilier.MdfQueue = mdf;
                return compilier.CompileShader(shading.CodeName.Address, "AS_Main", NxRHI.EShaderType.SDT_AmplificationShader,
                    shading, material, mdfType.SystemType, defines, null, null, TtEngine.Instance.Config.IsDebugShader);
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (descAS == null)
            {
                this.DescAS = descAS;
            }
            
            var descMS = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                var compilier = new Editor.ShaderCompiler.TtHLSLCompiler();
                compilier.MdfQueue = mdf;
                return compilier.CompileShader(shading.CodeName.Address, "MS_Main", NxRHI.EShaderType.SDT_MeshShader,
                    shading, material, mdfType.SystemType, defines, null, null, TtEngine.Instance.Config.IsDebugShader);
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (descMS == null)
            {
                descVS = await TtEngine.Instance.EventPoster.Post((state) =>
                {
                    var compilier = new Editor.ShaderCompiler.TtHLSLCompiler();
                    compilier.MdfQueue = mdf;
                    return compilier.CompileShader(shading.CodeName.Address, "VS_Main", NxRHI.EShaderType.SDT_VertexShader,
                        shading, material, mdfType.SystemType, defines, null, null, TtEngine.Instance.Config.IsDebugShader);
                }, Thread.Async.EAsyncTarget.AsyncIO);
                if (descVS == null)
                    return false;
                this.DescVS = descVS;
            }
            else
            {
                this.DescMS = descMS;
            }

            var descPS = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                var compilier = new Editor.ShaderCompiler.TtHLSLCompiler();
                compilier.MdfQueue = mdf;
                return compilier.CompileShader(shading.CodeName.Address, "PS_Main", NxRHI.EShaderType.SDT_PixelShader, 
                    shading, material, mdfType.SystemType, defines, null, null, TtEngine.Instance.Config.IsDebugShader);
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (descPS == null)
                return false;

            this.DescPS = descPS;

            NxRHI.TtShader AmplificationShader = null;
            NxRHI.TtShader MeshShader = null;
            NxRHI.TtShader VertexShader = null;
            if (DescAS != null)
            {
                AmplificationShader = rc.CreateShader(DescAS);
            }
            if (DescMS != null)
            {
                MeshShader = rc.CreateShader(DescMS);
            }
            else
            {
                VertexShader = rc.CreateShader(DescVS);
                if (VertexShader == null)
                    return false;
            }
            var PixelShader = rc.CreateShader(DescPS);
            if (PixelShader == null)
                return false;

            NxRHI.TtInputLayout InputLayout = null;
            unsafe
            {
                uint inputSteams = 0;
                mdf.mCoreObject.GetInputStreams(ref inputSteams);

                var layoutDesc = new NxRHI.TtInputLayoutDesc(IMesh.CreateInputLayoutDesc(inputSteams));
                layoutDesc.mCoreObject.SetShaderDesc(descVS.mCoreObject);
                InputLayout = TtEngine.Instance.GfxDevice.RenderContext.CreateInputLayout(layoutDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, layoutDesc);
                
                Desc.InputStreams = inputSteams;
            }

            var hash = TtEffectManager.GetShaderHash(shading, material, mdf);
            ShaderEffect = rc.CreateShaderEffect(AmplificationShader, MeshShader, VertexShader, PixelShader, hash.ToString());
            if (ShaderEffect == null)
                return false;
            ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);
            if (await LinkShaders(this) == false)
                return false;

            var dbg_name = $"{shading},{material.AssetName},{Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(mdf.GetType())}";
            MeshShader?.SetDebugName($"MS:{dbg_name}");
            VertexShader?.SetDebugName($"VS:{dbg_name}");
            PixelShader.SetDebugName($"PS:{dbg_name}");
            ShaderEffect.DebugName = dbg_name;
            return true;
        }
        private static async System.Threading.Tasks.Task<bool> LinkShaders(TtEffect result)
        {
            //result.GraphicsEffect.mCoreObject.LinkShaders();
            return true;
        }

        protected NxRHI.TtShader.TtShaderBinderIndexer mBindIndexer;
        public NxRHI.TtShader.TtShaderBinderIndexer BindIndexer
        {
            get
            {
                GetTypedBindIndexer<NxRHI.TtShader.TtCommonShaderResourceIndexer>();
                return mBindIndexer;
            }
        }
        public T GetTypedBindIndexer<T>() where T : NxRHI.TtShader.TtShaderBinderIndexer, new()
        {
            if (mBindIndexer == null ||
                (mBindIndexer.GetType() != typeof(T) && typeof(T) != typeof(NxRHI.TtShader.TtCommonShaderResourceIndexer)))
            {
                mBindIndexer = new T();
                NxRHI.TtShader.TtShaderBinderIndexer.RemoveGlobalBinderIndexer(mBindIndexer);
                mBindIndexer.UpdateBindResouce(this.ShaderEffect);
            }
            return mBindIndexer as T;
        }
    }
    public class TtEffectManager : IDisposable
    {
        public TtEffect DummyEffect;
        public async System.Threading.Tasks.Task<bool> Initialize(TtGfxDevice device)
        {
            var shading = TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtDummyShading>();
            DummyEffect = await this.GetGraphicEffect(await shading, device.MaterialManager.ScreenMaterial, new Mesh.TtMdfStaticMesh());

            if (DummyEffect == null)
            {
                DummyEffect = await TtEffect.CreateEffect(await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtDummyShading>(),
                   new TtShadingEnv.FPermutationId(0), device.MaterialManager.ScreenMaterial, new Mesh.TtMdfStaticMesh());
            }
            if (DummyEffect == null)
                return false;

            TtCoreShaderBinder.InitializeCoreBinder(DummyEffect.ShaderEffect);
            return true;
        }
        public void Dispose()
        {
            //System.Diagnostics.Debug.Assert(mCreatingSession.mSessions.Count == 0);
            foreach (var i in Effects)
            {
                i.Value.Dispose();
            }
            Effects.Clear();
            foreach (var i in ComputeEffects)
            {
                i.Value.Dispose();
            }
            ComputeEffects.Clear();
            foreach (var i in RayTracingEffects)
            {
                i.Value.Dispose();
            }
            RayTracingEffects.Clear();
            DummyEffect = null;
            TtCoreShaderBinder.FinalCleanup();
        }
        private Thread.TtAwaitSessionManager<Hash160, TtEffect> mCreatingSession = new Thread.TtAwaitSessionManager<Hash160, TtEffect>();
        public Dictionary<Hash160, TtEffect> Effects { get; } = new Dictionary<Hash160, TtEffect>();
        public Dictionary<Hash160, NxRHI.TtComputeEffect> ComputeEffects { get; } = new Dictionary<Hash160, NxRHI.TtComputeEffect>();
        public Dictionary<Hash160, NxRHI.TtRayTracingEffect> RayTracingEffects { get; } = new Dictionary<Hash160, NxRHI.TtRayTracingEffect>();
        public NxRHI.TtComputeEffect TryGetComputeEffect(Hash160 hash)
        {
            lock (ComputeEffects)
            {
                NxRHI.TtComputeEffect result;
                if (ComputeEffects.TryGetValue(hash, out result))
                    return result;

                return null;
            }
        }
        public TtEffect TryGetGraiphicEffect(Hash160 hash)
        {
            lock (Effects)
            {
                TtEffect result;
                if (Effects.TryGetValue(hash, out result))
                    return result;

                return null;
            }
        }
        public static Hash160 GetShaderHash(TtShadingEnv shading, TtMaterial material, TtMdfQueueBase mdf)
        {
            //var caps = new IRenderContextCaps();
            //TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetRenderContextCaps(ref caps);
            //return Hash160.CreateHash160($"{shading},{material.AssetName},{mdf},{caps}");
            return Hash160.CreateHash160($"{TtEngine.Instance.GfxDevice.RenderContext.GlobalEnvHash},{shading},{material.AssetName},{mdf}");
        }
        public Dictionary<Hash160, TtEffect> MaterialEditingEffects { get; } = new Dictionary<Hash160, TtEffect>();
        public async Thread.Async.TtTask<TtEffect> GetGraphicEffect(TtShadingEnv shading, TtMaterial material, TtMdfQueueBase mdf)
        {
            TtEffect result = null;
            Hash160 hash = new Hash160();
            if (material.IsEditingMaterial)
            {
                hash = GetShaderHash(shading, material, mdf);
                if (MaterialEditingEffects.TryGetValue(hash, out result))
                {
                    if (result.Desc.MaterialHash != material.MaterialHash)
                    {
                        await result.RefreshEffect(material);
                    }
                    return result;
                }
            }
            result = await GetGraphicEffectImpl(shading, material, mdf);
            if (material.IsEditingMaterial)
            {
                TtEffect nr = null;
                if (MaterialEditingEffects.TryGetValue(hash, out nr))
                {
                    return nr;
                }
                result = result?.UnsafeCloneForEditor();
                if (result != null)
                    MaterialEditingEffects.Add(hash, result);
                return result;
            }
            return result;
        }
        public void RemoveEditingMaterial(RName material)
        {
            var rmv = new List<Hash160>();
            foreach (var i in MaterialEditingEffects)
            {
                if (i.Value.Desc.MaterialName == material)
                {
                    rmv.Add(i.Key);
                }
            }
            foreach (var i in rmv)
            {
                MaterialEditingEffects.Remove(i);
            }
        }
        private async Thread.Async.TtTask<TtEffect> GetGraphicEffectImpl(TtShadingEnv shading, TtMaterial material, TtMdfQueueBase mdf)
        {
            TtEffect result = null;
            //if (material.IsEditingMaterial)
            //{
            //    result = await UEffect.CreateEffect(shading, shading.mCurrentPermutationId, material, mdf);
            //    return result;
            //}

            var hash = GetShaderHash(shading, material, mdf);
            result = TryGetGraiphicEffect(hash);
            if (result != null)
            {
                if (result.Desc.MaterialHash != material.MaterialHash)
                {
                    await result.RefreshEffect(material);
                }
                return result;
            }

            bool isNewSession;
            var session = mCreatingSession.GetOrNewSession(hash, out isNewSession);
            if (isNewSession == false)
            {
                return await session.Await();
            }

            try
            {
                result = await TtEffect.LoadEffect(hash, shading, material, mdf);
                if (result != null)
                {
                    if (result.DescMS != null && TtEngine.Instance.GfxDevice.RenderContext.DeviceCaps.IsSupportMeshShader == false)
                    {
                        return null;
                    }
                    if (Effects.ContainsKey(hash) == false)
                    {
                        lock (Effects)
                        {
                            Effects[hash] = result;
                        }
                        return result;
                    }
                    else
                    {
                        result = Effects[hash];
                        return result;
                    }
                }

                result = await TtEffect.CreateEffect(shading, shading.mCurrentPermutationId, material, mdf);
                if (result != null)
                {
                    result.Desc.EffectHash = hash;
                    result.Desc.PermutationId = shading.mCurrentPermutationId;
                    //if (material.IsEditingMaterial == false)
                    {
                        await TtEngine.Instance.EventPoster.Post((state) =>
                        {
                            result.SaveTo(hash);
                            return true;
                        }, Thread.Async.EAsyncTarget.AsyncIO);
                    }

                    if (result.DescMS != null && TtEngine.Instance.GfxDevice.RenderContext.DeviceCaps.IsSupportMeshShader == false)
                    {
                        return null;
                    }

                    if (Effects.ContainsKey(hash) == false)
                    {
                        lock (Effects)
                        {
                            Effects[hash] = result;
                        }
                        return result;
                    }
                    else
                    {
                        result = Effects[hash];
                        return result;
                    }
                }
                else
                {
                    result = null;
                    return result;
                }
            }
            finally
            {
                mCreatingSession.FinishSession(hash, session, result);
            }
        }
        //需要逐渐被Tt ComputeShadingEnv替换
        public async Thread.Async.TtTask<NxRHI.TtComputeEffect> GetComputeEffect(RName shaderName, string entry, NxRHI.EShaderType type,
            Graphics.Pipeline.Shader.TtShadingEnv shadingEnv, NxRHI.TtShaderDefinitions defines, 
            Editor.ShaderCompiler.TtHLSLInclude incProvider, string sm = null, bool bDebugShader = true)
        {
            var shader = shaderName.Address;
            var hashStr = shaderName.Address;
            hashStr += entry;
            if (shadingEnv != null)
                hashStr += shadingEnv.ToString();
            if (defines != null)
                hashStr += defines.ToString();
            hashStr += TtEngine.Instance.GfxDevice.RenderContext.GlobalEnvHash.ToString();
            hashStr += Editor.ShaderCompiler.TtShaderCodeManager.Instance.GetShaderCode(shaderName).CodeHash;
            var hash = Hash160.CreateHash160(hashStr);
            NxRHI.TtComputeEffect result;
            lock (ComputeEffects)
            {
                if (ComputeEffects.TryGetValue(hash, out result))
                    return result;
            }
            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                return NxRHI.TtComputeEffect.Load(hash); ;
            }, Thread.Async.EAsyncTarget.AsyncIO);
            //result = NxRHI.UComputeEffect.Load(hash);
            if (result != null)
            {
                lock (Effects)
                {
                    if (ComputeEffects.TryGetValue(hash, out var nt))
                        return nt;
                    ComputeEffects.Add(hash, result);
                }
                return result;
            }
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var compiler = new Editor.ShaderCompiler.TtHLSLCompiler();
            compiler.MdfQueue = null;
            if (defines == null)
            {
                defines = new NxRHI.TtShaderDefinitions();
            }
            if (shadingEnv != null)
                shadingEnv.GetShaderDefines(shadingEnv.CurrentPermutationId, defines);
            var shaderDesc = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                return compiler.CompileShader(shader, entry, type, shadingEnv, null, null, defines, incProvider, sm, bDebugShader);
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (shaderDesc == null)
                return null;
            var csShader = rc.CreateShader(shaderDesc);
            if (csShader == null)
                return null;

            result = TtEngine.Instance.GfxDevice.RenderContext.CreateComputeEffect(csShader);

            lock (ComputeEffects)
            {
                if (ComputeEffects.TryGetValue(hash, out var nt))
                    return nt;
                ComputeEffects.Add(hash, result);
            }

            TtEngine.Instance.EventPoster.RunOn((state) =>
            {
                result.SaveTo(shaderName, hash);
                return true;
            }, Thread.Async.EAsyncTarget.AsyncIO);

            return result;
        }
        public async Thread.Async.TtTask<NxRHI.TtRayTracingEffect> GetRayTracingEffect(RName shaderName, string entry, 
            Graphics.Pipeline.Shader.TtRayTracingShadingEnv shadingEnv, NxRHI.TtShaderDefinitions defines,
            Editor.ShaderCompiler.TtHLSLInclude incProvider, string sm = null, bool bDebugShader = true)
        {
            var shader = shaderName.Address;
            var hashStr = shaderName.Address;
            hashStr += entry;
            if (shadingEnv != null)
                hashStr += shadingEnv.ToString();
            if (defines != null)
                hashStr += defines.ToString();
            hashStr += TtEngine.Instance.GfxDevice.RenderContext.GlobalEnvHash.ToString();
            hashStr += Editor.ShaderCompiler.TtShaderCodeManager.Instance.GetShaderCode(shaderName).CodeHash;
            var hash = Hash160.CreateHash160(hashStr);
            
            NxRHI.TtRayTracingEffect result;
            lock (RayTracingEffects)
            {
                if (RayTracingEffects.TryGetValue(hash, out result))
                    return result;
            }
            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                return NxRHI.TtRayTracingEffect.Load(hash); ;
            }, Thread.Async.EAsyncTarget.AsyncIO);
            //result = NxRHI.UComputeEffect.Load(hash);
            if (result != null)
            {
                lock (RayTracingEffects)
                {
                    if (RayTracingEffects.TryGetValue(hash, out var nt))
                        return nt;
                    RayTracingEffects.Add(hash, result);
                }
                return result;
            }
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var compiler = new Editor.ShaderCompiler.TtHLSLCompiler();
            compiler.MdfQueue = null;
            if (defines == null)
            {
                defines = new NxRHI.TtShaderDefinitions();
            }
            if (shadingEnv != null)
                shadingEnv.GetShaderDefines(shadingEnv.CurrentPermutationId, defines);
            var shaderDesc = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                return compiler.CompileShader(shader, entry, NxRHI.EShaderType.SDT_RayTracing, shadingEnv, null, null, defines, incProvider, sm, bDebugShader, "2021", true);
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (shaderDesc == null)
                return null;

            NxRHI.TtRayTracingEffect.TtRTShaderLibDesc shaderLibDesc = null;
            var code = Editor.ShaderCompiler.TtShaderCodeManager.Instance.GetShaderCode(shaderName).SourceCode.TextCode;
            var start = code.IndexOf("/*<RTShaderLibDesc>");
            if (start >= 0)
            {
                start += "/*<RTShaderLibDesc>".Length;
                var end = code.IndexOf("<RTShaderLibDesc>*/", start);
                if (end >= 0)
                {
                    var descText = code.Substring(start, end - start);
                    shaderLibDesc = TtFileManager.LoadObjectFromJson<NxRHI.TtRayTracingEffect.TtRTShaderLibDesc>(descText);
                }
            }
            if (shaderLibDesc == null)
                return null;
            result = TtEngine.Instance.GfxDevice.RenderContext.CreateRayTracingEffect(shaderDesc, shaderLibDesc);

            lock (RayTracingEffects)
            {
                if (RayTracingEffects.TryGetValue(hash, out var nt))
                    return nt;
                RayTracingEffects.Add(hash, result);
            }

            TtEngine.Instance.EventPoster.RunOn((state) =>
            {
                result.SaveTo(shaderName, hash);
                return true;
            }, Thread.Async.EAsyncTarget.AsyncIO);

            return result;
        }
    }
}
