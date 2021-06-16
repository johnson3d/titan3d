using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public class UEffect
    {
        public const string AssetExt = ".effect";

        public class UEffectDesc : IO.ISerializer
        {
            public void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {

            }

            public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
            {

            }
            [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
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
            public uint PermutationId { get; set; }
            [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
            public string ShadingType { get; set; }
            [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
            public uint InputStreams { get; set; }
        }
        public UEffectDesc Desc { get; set; } = new UEffectDesc();
        public RHI.CShaderProgram ShaderProgram { get; private set; }
        public RHI.CShaderDesc DescVS { get; private set; }
        public RHI.CShaderDesc DescPS { get; private set; }

        public UShadingEnv ShadingEnv { get; internal set; }

        public UInt32 CBPerViewportIndex { get; private set; }
        public UInt32 CBPerFrameIndex { get; private set; }
        public UInt32 CBPerCameraIndex { get; private set; }
        public UInt32 CBPerMeshIndex { get; private set; }
        public UInt32 CBPerMaterialIndex { get; private set; }
        public object TagObject { get; set; }
        public unsafe void SaveTo(Hash160 hash)
        {
            var path = UEngine.Instance.FileManager.GetPath(IO.FileManager.ERootDir.Cache, IO.FileManager.ESystemDir.Effect);
            var file = path + hash.ToString() + UEffect.AssetExt;
            var xnd = new IO.CXndHolder("UEffect", 0, 0);

            var descAttr = new XndAttribute(xnd.RootNode.mCoreObject.GetOrAddAttribute("Desc", 0, 0));
            var ar = descAttr.GetWriter(30);
            ar.Write(Desc);
            descAttr.ReleaseWriter(ref ar);

            var vsNode = xnd.mCoreObject.NewNode("VSCode", 0, 0);
            xnd.RootNode.mCoreObject.AddNode(vsNode);
            DescVS.mCoreObject.Save2Xnd(vsNode, 0xffffffff);
            CoreSDK.IUnknown_Release(vsNode);

            var psNode = xnd.mCoreObject.NewNode("PSCode", 0, 0);
            xnd.RootNode.mCoreObject.AddNode(psNode);
            DescPS.mCoreObject.Save2Xnd(psNode, 0xffffffff);
            CoreSDK.IUnknown_Release(psNode);

            xnd.SaveXnd(file);
        }
        public static async System.Threading.Tasks.Task<UEffect> LoadEffect(Hash160 hash, UShadingEnv shading, UMaterial material, UMdfQueue mdf)
        {
            var path = UEngine.Instance.FileManager.GetPath(IO.FileManager.ERootDir.Cache, IO.FileManager.ESystemDir.Effect);
            var file = path + hash.ToString() + UEffect.AssetExt;

            var xnd = IO.CXndHolder.LoadXnd(file);
            if (xnd == null)
                return null;

            XndAttribute descAttr = new XndAttribute();
            XndNode vsNode = new XndNode();
            XndNode psNode = new XndNode();
            unsafe
            {
                descAttr = xnd.RootNode.mCoreObject.TryGetAttribute("Desc");
                if (descAttr.IsValidPointer == false)
                    return null;
                vsNode = xnd.RootNode.mCoreObject.TryGetChildNode("VSCode");
                if (vsNode.IsValidPointer == false)
                    return null;
                psNode = xnd.RootNode.mCoreObject.TryGetChildNode("PSCode");
                if (psNode.IsValidPointer == false)
                    return null;
            }
            
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var result = await UEngine.Instance.EventPoster.Post(() =>
            {
                var effect = new UEffect();
                IO.ISerializer desc;
                var ar = descAttr.GetReader(effect);
                IO.SerializerHelper.Read(ar, out desc, effect);
                descAttr.ReleaseReader(ref ar);
                var effectDesc = desc as UEffectDesc;
                if (effectDesc == null)
                    return null;

                var shadingCode = Editor.ShaderCompiler.UShaderCodeManager.Instance.GetShaderCode(shading.CodeName);
                if (shadingCode.CodeHash != effectDesc.CodeHash ||
                        material.GetHash() != effectDesc.MaterialHash ||
                        mdf.GetHash() != effectDesc.MdfQueueHash)
                {
                    xnd.mCoreObject.TryReleaseHolder();
                    xnd.Dispose();
                    return null;
                }

                effect.Desc = effectDesc;

                unsafe
                {
                    effect.DescVS = new RHI.CShaderDesc(EShaderType.EST_VertexShader);
                    if (effect.DescVS.mCoreObject.LoadXnd(vsNode) == 0)
                        return null;
                    effect.DescPS = new RHI.CShaderDesc(EShaderType.EST_PixelShader);
                    if (effect.DescPS.mCoreObject.LoadXnd(psNode) == 0)
                        return null;
                }
                return effect;
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result == null)
                return null;

            RHI.CVertexShader VertexShader = null;
            RHI.CPixelShader PixelShader = null;
            bool created = await UEngine.Instance.EventPoster.Post(() =>
            {
                VertexShader = rc.CreateVertexShader(result.DescVS);
                if (VertexShader == null)
                    return false;
                PixelShader = rc.CreatePixelShader(result.DescPS);
                if (PixelShader == null)
                    return false;
                return true;
            }, Thread.Async.EAsyncTarget.Render);
            if (created == false)
                return null;

            VertexShader.mCoreObject.NativeSuper.NativeSuper.SetDebugName($"VS:{shading},{material.AssetName},{Rtti.UTypeDescManager.Instance.GetTypeStringFromType(mdf.GetType())}");
            PixelShader.mCoreObject.NativeSuper.NativeSuper.SetDebugName($"PS:{shading},{material.AssetName},{Rtti.UTypeDescManager.Instance.GetTypeStringFromType(mdf.GetType())}");

            RHI.CInputLayout InputLayout = null;
            unsafe
            {
                var layoutDesc = new IInputLayoutDesc(IMesh.CreateInputLayoutDesc(result.Desc.InputStreams));
                layoutDesc.SetShaderDesc(result.DescVS.mCoreObject);
                UEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, layoutDesc);
                InputLayout = rc.CreateInputLayout(layoutDesc);

                CoreSDK.IUnknown_Release(layoutDesc.NativePointer.ToPointer());
            }

            var progDesc = new IShaderProgramDesc();
            unsafe
            {
                progDesc.InputLayout = InputLayout.mCoreObject;
                progDesc.VertexShader = VertexShader.mCoreObject;
                progDesc.PixelShader = PixelShader.mCoreObject;
                result.ShaderProgram = rc.CreateShaderProgram(ref progDesc);
            }

            if (await LinkShaders(result) == false)
                return null;

            result.ShadingEnv = shading;
            return result;
        }
        public static async System.Threading.Tasks.Task<UEffect> CreateEffect(UShadingEnv shading, uint permutationId, UMaterial material, UMdfQueue mdf)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var result = new UEffect();
            var shadingCode = Editor.ShaderCompiler.UShaderCodeManager.Instance.GetShaderCode(shading.CodeName);
            result.Desc.CodeHash = shadingCode.CodeHash;
            result.Desc.MaterialHash = material.GetHash();
            result.Desc.MdfQueueHash = mdf.GetHash();
            result.Desc.MaterialName = material.AssetName;
            result.Desc.MdfQueueType = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(mdf.GetType());
            result.Desc.ShadingType = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(shading.GetType());
            result.ShadingEnv = shading;

            var defines = new RHI.CShaderDefinitions();
            shading.GetShaderDefines(permutationId, defines);

            var cfg = UEngine.Instance.Config;
            result.DescVS = await UEngine.Instance.EventPoster.Post(() =>
            {
                var compilier = new Editor.ShaderCompiler.UHLSLCompiler();
                return compilier.CompileShader(shading.CodeName.Address, "VS_Main", EShaderType.EST_VertexShader, "5_0", material.AssetName, mdf.GetType(), defines, true);
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (result.DescVS == null)
                return null;

            result.DescPS = await UEngine.Instance.EventPoster.Post(() =>
            {
                var compilier = new Editor.ShaderCompiler.UHLSLCompiler();
                return compilier.CompileShader(shading.CodeName.Address, "PS_Main", EShaderType.EST_PixelShader, "5_0", material.AssetName, mdf.GetType(), defines, true);
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (result.DescPS == null)
                return null;

            RHI.CVertexShader VertexShader = null;
            RHI.CPixelShader PixelShader = null;
            bool created = await UEngine.Instance.EventPoster.Post(() =>
            {
                VertexShader = rc.CreateVertexShader(result.DescVS);
                if (VertexShader == null)
                    return false;
                PixelShader = rc.CreatePixelShader(result.DescPS);
                if (PixelShader == null)
                    return false;
                return true;
            }, Thread.Async.EAsyncTarget.Render);
            if (created == false)
                return null;

            RHI.CInputLayout InputLayout = null;
            unsafe
            {
                uint inputStreams = 0;
                mdf.mCoreObject.GetInputStreams(ref inputStreams);
                var layoutDesc = new IInputLayoutDesc();
                layoutDesc = IMesh.CreateInputLayoutDesc(inputStreams);
                layoutDesc.SetShaderDesc(result.DescVS.mCoreObject);
                UEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, layoutDesc);
                InputLayout = rc.CreateInputLayout(layoutDesc);

                CoreSDK.IUnknown_Release(layoutDesc.NativePointer.ToPointer());

                result.Desc.InputStreams = inputStreams;

                var progDesc = new IShaderProgramDesc();
                progDesc.InputLayout = InputLayout.mCoreObject;
                progDesc.VertexShader = VertexShader.mCoreObject;
                progDesc.PixelShader = PixelShader.mCoreObject;
                result.ShaderProgram = rc.CreateShaderProgram(ref progDesc);
            }
            if (await LinkShaders(result) == false)
                return null;
            return result;
        }
        public async System.Threading.Tasks.Task<bool> RefreshEffect()
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var material = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(this.Desc.MaterialName);
            if (material == null)
                return false;
            
            var defines = new RHI.CShaderDefinitions();
            var type = Rtti.UTypeDescManager.Instance.GetTypeFromString(Desc.ShadingType);
            if (type == null)
                return false;
            var shading = UEngine.Instance.ShadingEnvManager.GetShadingEnv(type);
            if (shading == null)
                return false;
            shading.GetShaderDefines(Desc.PermutationId, defines);

            var mdfType = Rtti.UTypeDesc.TypeOf(Desc.MdfQueueType);
            if (mdfType == null)
                return false;
            var mdf = Rtti.UTypeDescManager.CreateInstance(mdfType) as Pipeline.Shader.UMdfQueue;
            if (mdf == null)
                return false;

            var descVS = await UEngine.Instance.EventPoster.Post(() =>
            {
                var compilier = new Editor.ShaderCompiler.UHLSLCompiler();
                return compilier.CompileShader(shading.CodeName.Address, "VS_Main", EShaderType.EST_VertexShader, "5_0", material.AssetName, mdfType.SystemType, defines, true);
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (descVS == null)
                return false;

            var descPS = await UEngine.Instance.EventPoster.Post(() =>
            {
                var compilier = new Editor.ShaderCompiler.UHLSLCompiler();
                return compilier.CompileShader(shading.CodeName.Address, "PS_Main", EShaderType.EST_PixelShader, "5_0", material.AssetName, mdfType.SystemType, defines, true);
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (descPS == null)
                return false;


            this.DescVS = descVS;
            this.DescPS = descPS;
            this.Desc.MaterialHash = material.GetHash();
            this.Desc.MdfQueueHash = mdf.GetHash();

            var VertexShader = rc.CreateVertexShader(DescVS);
            if (VertexShader == null)
                return false;
            var PixelShader = rc.CreatePixelShader(DescPS);
            if (PixelShader == null)
                return false;

            RHI.CInputLayout InputLayout = null;
            unsafe
            {
                uint inputSteams = 0;
                mdf.mCoreObject.GetInputStreams(ref inputSteams);

                var layoutDesc = IMesh.CreateInputLayoutDesc(inputSteams);
                layoutDesc.SetShaderDesc(DescVS.mCoreObject);
                UEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, *layoutDesc.CppPointer);
                InputLayout = rc.CreateInputLayout(*layoutDesc.CppPointer);                
                CoreSDK.IUnknown_Release(layoutDesc);

                Desc.InputStreams = inputSteams;
            }

            var progDesc = new IShaderProgramDesc();
            unsafe
            {
                progDesc.InputLayout = InputLayout.mCoreObject;
                progDesc.VertexShader = VertexShader.mCoreObject;
                progDesc.PixelShader = PixelShader.mCoreObject;
            }
            ShaderProgram = rc.CreateShaderProgram(ref progDesc);

            if (await LinkShaders(this) == false)
                return false;
            return true;
        }
        private static async System.Threading.Tasks.Task<bool> LinkShaders(UEffect result)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var isOK = await UEngine.Instance.EventPoster.Post(() =>
            {
                unsafe
                {
                    return result.ShaderProgram.mCoreObject.LinkShaders(rc.mCoreObject) != 0;
                }
            }, Thread.Async.EAsyncTarget.Render);

            if (isOK)
            {
                unsafe
                {
                    var shaderProg = result.ShaderProgram.mCoreObject;
                    result.CBPerViewportIndex = shaderProg.FindCBuffer("cbPerViewport");

                    result.CBPerFrameIndex = shaderProg.FindCBuffer("cbPerFrame");
                    if (result.CBPerFrameIndex != 0xFFFFFFFF)
                    {
                        if (UEngine.Instance.GfxDevice.PerFrameCBuffer == null)
                        {
                            UEngine.Instance.GfxDevice.PerFrameCBuffer = rc.CreateConstantBuffer(result.ShaderProgram, result.CBPerFrameIndex);
                        }
                    }

                    result.CBPerCameraIndex = shaderProg.FindCBuffer("cbPerCamera");

                    result.CBPerMeshIndex = shaderProg.FindCBuffer("cbPerMesh");

                    result.CBPerMaterialIndex = shaderProg.FindCBuffer("cbPerMaterial");
                }
            }
            return isOK;
        }
    }
    public class UEffectManager
    {
        public void Cleanup()
        {
            Effects.Clear();
        }
        public Dictionary<Hash160, UEffect> Effects { get; } = new Dictionary<Hash160, UEffect>();
        public UEffect TryGetEffect(Hash160 hash)
        {
            UEffect result;
            if (Effects.TryGetValue(hash, out result))
                return result;

            return null;
        }
        public Hash160 GetShaderHash(UShadingEnv shading, UMaterial material, UMdfQueue mdf)
        {
            return Hash160.CreateHash160($"{shading},{material.AssetName},{Rtti.UTypeDescManager.Instance.GetTypeStringFromType(mdf.GetType())}");
        }
        public async System.Threading.Tasks.Task<UEffect> GetEffect(UShadingEnv shading, UMaterial material, UMdfQueue mdf)
        {
            var hash = GetShaderHash(shading, material, mdf);
            var result = TryGetEffect(hash);
            if (result != null)
            {
                if (result.Desc.MaterialHash != material.MaterialHash)
                {
                    await result.RefreshEffect();
                }
                return result;
            }

            result = await UEffect.LoadEffect(hash, shading, material, mdf);
            if (result != null)
            {
                if (Effects.ContainsKey(hash) == false)
                {
                    Effects[hash] = result;
                    return result;
                }
                else
                {
                    return Effects[hash];
                }
            }

            result = await UEffect.CreateEffect(shading, shading.CurrentPermutationId, material, mdf);
            if (result != null)
            {
                result.Desc.PermutationId = shading.CurrentPermutationId;
                await UEngine.Instance.EventPoster.Post(() =>
                {
                    result.SaveTo(hash);
                    return true;
                }, Thread.Async.EAsyncTarget.AsyncIO);

                if (Effects.ContainsKey(hash) == false)
                {
                    Effects[hash] = result;
                    return result;
                }
                else
                {
                    return Effects[hash];
                }
            }
            else
            {
                return null;
            }
        }
    }
}
