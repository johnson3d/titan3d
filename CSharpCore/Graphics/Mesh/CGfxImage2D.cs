using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Graphics.Mesh
{
    public class CGfxImage2D
    {
        public static async System.Threading.Tasks.Task<CGfxImage2D> CreateImage2D(CRenderContext rc, RName mtlName,
                        float x, float y, float z, float w, float h)
        {
            var rectMesh = CEngine.Instance.MeshPrimitivesManager.CreateMeshPrimitives(rc, 1);
            var mtlInst = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, mtlName);
            if (mtlInst == null)
                return null;

            //EngineNS.Graphics.Mesh.CGfxMeshCooker.MakeRect2D(rc, rectMesh, x, y, w, h, z, true, true, ECpuAccess.CAS_WRITE);
            EngineNS.Graphics.Mesh.CGfxMeshCooker.MakeUIScale9(rc, rectMesh, x, y, w, h, z, true, true);
            var rect = CEngine.Instance.MeshManager.CreateMesh(rc, rectMesh);

            await rect.SetMaterialInstanceAsync(rc, 0, mtlInst, CEngine.Instance.PrebuildPassData.Image2DShadingEnvs);

            var result = new CGfxImage2D();
            result.Mesh = rect;
            rect.Tag = result;

            var pass = result.GetPass();
            if(pass!=null)
            {
                if(pass.Effect!=null)
                {
                    //保证Effect加载有效
                    await pass.Effect.AwaitLoad();
                    //保证pass在effect有效后，把必要的RenderResource绑定正确
                    pass.PreUse();
                    result.CBuffer = CEngine.Instance.RenderContext.CreateConstantBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_ShadingEnv);
                    var CBuffer = result.CBuffer;
                    if (result.RenderMatrixId == -1)
                        result.RenderMatrixId = CBuffer.FindVar("RenderMatrix");
                    //if (result.RenderOpacityId == -1)
                    //    result.RenderOpacityId = CBuffer.FindVar("RenderOpacity");
                    if (result.RenderColorId == -1)
                        result.RenderColorId = CBuffer.FindVar("RenderColor");
                    CBuffer.SetValue(result.RenderMatrixId, result.RenderMatrix, 0);
                    //CBuffer.SetValue(result.RenderOpacityId, result.RenderOpacity, 0);
                    CBuffer.SetValue(result.RenderColorId, result.RenderColor, 0);
                }
                pass.RenderPipeline.DepthStencilState = CEngine.Instance.PrebuildPassData.mDisableDepthStencilStat;
                pass.RenderPipeline.RasterizerState = CEngine.Instance.PrebuildPassData.mNoCullingRasterStat;
            }
            return result;
        }
        public CGfxMesh Mesh
        {
            get;
            protected set;
        }
        public CConstantBuffer CBuffer;
        int RenderMatrixId = -1;
        private Matrix mRenderMatrix = Matrix.Identity;
        public Matrix RenderMatrix
        {
            get { return mRenderMatrix; }
            set
            {
                mRenderMatrix = value;
                if(CBuffer!=null)
                {
                    if (RenderMatrixId == -1)
                        RenderMatrixId = CBuffer.FindVar("RenderMatrix");
                    
                    CBuffer.SetValue(RenderMatrixId, value, 0);
                }
            }
        }
        //int RenderOpacityId = -1;
        //float mRenderOpacity = 1.0f;
        //public float RenderOpacity
        //{
        //    get
        //    {
        //        return mRenderOpacity;
        //    }
        //    set
        //    {
        //        mRenderOpacity = value;
        //        if(CBuffer != null)
        //        {
        //            if (RenderOpacityId == -1)
        //                RenderOpacityId = CBuffer.FindVar("RenderOpacity");
        //            CBuffer.SetValue(RenderOpacityId, value, 0);
        //        }
        //    }
        //}
        int RenderColorId = -1;
        Color4 mRenderColor = new Color4(1, 1, 1, 1);
        public Color4 RenderColor
        {
            get => mRenderColor;
            set
            {
                mRenderColor = value;
                if(CBuffer != null)
                {
                    if (RenderColorId == -1)
                        RenderColorId = CBuffer.FindVar("RenderColor");
                    CBuffer.SetValue(RenderColorId, value, 0);
                }
            }
        }
        public CPass GetPass()
        {
            if (Mesh == null)
                return null;
            return Mesh.MtlMeshArray[0].GetPass(PrebuildPassIndex.PPI_Default);
        }
        public async Task<bool> SetMaterialInstance(EngineNS.CRenderContext rc, RName mtlInsName)
        {
            var mtlInst = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, mtlInsName);
            if (mtlInst == null)
                return false;
            Mesh.SetMaterialInstance(rc, 0, mtlInst, CEngine.Instance.PrebuildPassData.Image2DShadingEnvs, true);
            await Mesh.AwaitEffects();
            
            return true;
        }
        public void SetTexture(UInt32 slot, CShaderResourceView srv)
        {
            var pass = GetPass();
            if (pass == null)
                return;
            pass.ShaderResources.PSBindTexture(slot, srv);
        }
        public void SetTexture(string shaderName, CShaderResourceView srv)
        {
            var pass = GetPass();
            if (pass == null)
                return;
            CTextureBindInfo bindInfo = new CTextureBindInfo();
            pass.Effect.PreUse((success) =>
            {
                if (pass.Effect.ShaderProgram.FindTextureBindInfo(Mesh.MtlMeshArray[0].MtlInst, shaderName, ref bindInfo))
                {
                    pass.ShaderResources.PSBindTexture(bindInfo.PSBindPoint, srv);
                }
            });
        }
        public void SetTexture(CRenderContext rc, string shaderName, RName texRName)
        {
            var texture = CEngine.Instance.TextureManager.GetShaderRView(rc, texRName);
            SetTexture(shaderName, texture);
        }

        public bool SetUV(EngineNS.Support.NativeListProxy<EngineNS.Vector2> uvs, CCommandList cmd)
        {
            if (uvs.Count != 36)
                throw new InvalidOperationException("UV数组数量不是36");

            var mesh = Mesh.MeshPrimitives.GeometryMesh;
            var uvVB = mesh.GetVertexBuffer(EVertexSteamType.VST_UV);
            if (uvVB == null)
                return false;

            unsafe
            {
                uvVB.UpdateBuffData(cmd, uvs.UnsafeAddressAt(0), (UInt32)(sizeof(Vector2) * 36));
            }
            return true;
        }
        public bool SetVertexBuffer(EngineNS.Support.NativeListProxy<EngineNS.Vector3> vertexes, CCommandList cmd)
        {
            if(vertexes.Count != 36)
                throw new InvalidOperationException("顶点数组数量不是36");

            var mesh = Mesh.MeshPrimitives.GeometryMesh;
            var vVB = mesh.GetVertexBuffer(EVertexSteamType.VST_Position);
            if (vVB == null)
                return false;

            unsafe
            {
                vVB.UpdateBuffData(cmd, vertexes.UnsafeAddressAt(0), (UInt32)(sizeof(Vector3) * 36));
            }
            return true;
        }

        public void UpdateForEditerMode(CRenderContext RHICtx)
        {
            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game)
            {
                var shadingEnv = CEngine.Instance.PrebuildPassData.Image2DShadingEnvs[(int)PrebuildPassIndex.PPI_Default];
                for (UInt32 i = 0; i < Mesh.MtlMeshArray.Length; i++)
                {
                    var MtlMesh = Mesh.MtlMeshArray[i];
                    if (MtlMesh == null)
                    {
                        continue;
                    }

                    if (MtlMesh.MtlInstVersion != MtlMesh.MtlInst.Version)
                    {
                        MtlMesh.MtlInstVersion = MtlMesh.MtlInst.Version;

                        MtlMesh.MtlInst.SetCBufferVars(MtlMesh.CBuffer);
                        var refPass = GetPass();
                        var effect = MtlMesh.TryGetEffect(RHICtx, MtlMesh.mRootSceneMesh, true, shadingEnv.EnvCode);
                        if (effect == null)
                            continue;
                        refPass.Effect = effect;
                        MtlMesh.MtlInst.BindTextures(refPass.ShaderResources, effect.ShaderProgram);
                        refPass.BindCBuffer(effect.ShaderProgram, effect.CacheData.PerInstanceId, MtlMesh.CBuffer);
                        refPass.BindCBuffer(effect.ShaderProgram, effect.CacheData.CBID_Mesh, Mesh.CBuffer);

                        refPass.RenderPipeline.RasterizerState = MtlMesh.MtlInst.CustomRasterizerState;
                        refPass.RenderPipeline.DepthStencilState = MtlMesh.MtlInst.CustomDepthStencilState;
                        refPass.RenderPipeline.BlendState = MtlMesh.MtlInst.CustomBlendState;
                    }
                }
            }
        }
    }

    public class CImage2DShadingEnv : EngineNS.Graphics.CGfxShadingEnv
    {
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
            defs.Add(new CShaderDefinitions.MacroDefine("UserDef_ShadingEnv", "matrix RenderMatrix; float4 RenderColor;"));
        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/base2d.shadingenv");
            }
        }
        protected override void OnCreated()
        {

        }
        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
        {
            var fmesh = mesh.Tag as CGfxImage2D;
            if (fmesh != null)
            {
                if (fmesh.CBuffer == null)
                {
                    fmesh.CBuffer = CEngine.Instance.RenderContext.CreateConstantBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_ShadingEnv);
                    int RenderMatrixId = fmesh.CBuffer.FindVar("RenderMatrix");
                    //int RenderOpacityId = fmesh.CBuffer.FindVar("RenderOpacity");
                    int RenderColorId = fmesh.CBuffer.FindVar("RenderColor");
                    fmesh.CBuffer.SetValue(RenderMatrixId, fmesh.RenderMatrix, 0);
                    //fmesh.CBuffer.SetValue(RenderOpacityId, fmesh.RenderOpacity, 0);
                    fmesh.CBuffer.SetValue(RenderColorId, fmesh.RenderColor, 0);
                }
                if (fmesh.CBuffer != null)
                {
                    if (pass.PreUse())
                    {
                        pass.BindCBuffer(pass.GpuProgram, pass.Effect.CacheData.CBID_ShadingEnv, fmesh.CBuffer);
                    }
                }
            }
        }
    }
}
