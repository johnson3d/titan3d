using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EngineNS.NxRHI;
using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;

namespace EngineNS.Graphics.Pipeline.Common.Post
{
    public class SWRasterizerCS : Shader.UComputeShadingEnv
    {
        public override Vector3ui DispatchArg 
        {
            get => new Vector3ui(64, 1, 1);
        }
       
        public SWRasterizerCS()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Post/SWRasterizer.compute", RName.ERNameType.Engine);
            MainName = "CS_Main";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, UShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);

        }
        protected override NxRHI.UComputeEffect OnCreateEffect()
        {
            return UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(CodeName,
                MainName, NxRHI.EShaderType.SDT_ComputeShader, this, null, null);
        }
        public override void OnDrawCall(NxRHI.UComputeDraw drawcall, URenderPolicy policy)
        {
            var rasterizerNode = drawcall.TagObject as SWRasterizerNode;
            if (rasterizerNode == null)
            {
                var pipelinePolicy = policy;
                rasterizerNode = pipelinePolicy.FindFirstNode<SWRasterizerNode>();
            }
            {
                var index = drawcall.FindBinder(EShaderBindType.SBT_SRV, "InputTriangles");
                drawcall.BindSrv(index, rasterizerNode.TrianglesView);
            }
            {
                var index = drawcall.FindBinder(EShaderBindType.SBT_UAV, "OutputColor");
                if (index.IsValidPointer)
                {
                    var attachBuffer = rasterizerNode.GetAttachBuffer(rasterizerNode.ColorOutput);
                    drawcall.BindUav(index, attachBuffer.Uav);
                }
            }   
        }
    }

   
    public class SWRasterizerNode : URenderGraphNode
    {
        //public Common.URenderGraphPin ColorPinIn = Common.URenderGraphPin.CreateInputOutput("InputTriangle");
        public Common.URenderGraphPin ColorOutput = Common.URenderGraphPin.CreateOutput("ColorOut", false, EPixelFormat.PXF_R8G8B8A8_UNORM);
        
        public SWRasterizerCS SWRasterizer;
        private NxRHI.UComputeDraw SWRasterizerDrawcall;

        public NxRHI.UBuffer TrianglesBuffer;
        public NxRHI.USrView TrianglesView;

        int NANITE_SUBPIXEL_SAMPLES = 256;
        

        struct Triangle
        {
            public Vector3 pos0;
            public Vector3 pos1;
            public Vector3 pos2;

            public Triangle(Vector3 p0, Vector3 p1, Vector3 p2)
            {
                pos0 = p0;
                pos1 = p1;
                pos2 = p2;
            }
        }
        public SWRasterizerNode()
        {
            Name = "SWRasterizerNode";
            unsafe
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                var bfDesc = new NxRHI.FBufferDesc();

                bfDesc.SetDefault(false);
                bfDesc.Type = NxRHI.EBufferType.BFT_SRV;
                bfDesc.Size = 2 * (uint)sizeof(Triangle);
                bfDesc.StructureStride = (uint)sizeof(Triangle);
                var initData = new Triangle[2];
                initData[0] = new Triangle(new Vector3(50 * NANITE_SUBPIXEL_SAMPLES, 50 * NANITE_SUBPIXEL_SAMPLES, 0), 
                                           new Vector3(50 * NANITE_SUBPIXEL_SAMPLES, 100 * NANITE_SUBPIXEL_SAMPLES, 0), 
                                           new Vector3(80 * NANITE_SUBPIXEL_SAMPLES, 50 * NANITE_SUBPIXEL_SAMPLES, 0));
                initData[1] = new Triangle(new Vector3(100 * NANITE_SUBPIXEL_SAMPLES, 50 * NANITE_SUBPIXEL_SAMPLES, 0), 
                                           new Vector3(100 * NANITE_SUBPIXEL_SAMPLES, 100 * NANITE_SUBPIXEL_SAMPLES, 0), 
                                           new Vector3(150 * NANITE_SUBPIXEL_SAMPLES, 50 * NANITE_SUBPIXEL_SAMPLES, 0));

                fixed (Triangle* pAddr = &initData[0])
                {
                    bfDesc.InitData = pAddr;
                    TrianglesBuffer = rc.CreateBuffer(in bfDesc);
                }

                var srvDesc = new NxRHI.FSrvDesc();
                srvDesc.SetBuffer(0);
                srvDesc.Buffer.FirstElement = 0;
                srvDesc.Buffer.NumElements = 2;
                srvDesc.Buffer.StructureByteStride = (uint)sizeof(Triangle);
                TrianglesView = rc.CreateSRV(TrianglesBuffer, in srvDesc);
            }
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref SWRasterizerDrawcall);
            base.Dispose();
        }
        public override void InitNodePins()
        {
            //AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
            ColorOutput.Attachement.Width = ColorOutput.Attachement.Height = 1024;
            AddOutput(ColorOutput, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
        }
        
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            CoreSDK.DisposeObject(ref SWRasterizerDrawcall);
            SWRasterizerDrawcall = rc.CreateComputeDraw();
            SWRasterizer = UEngine.Instance.ShadingEnvManager.GetShadingEnv<SWRasterizerCS>();
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            base.OnResize(policy, x, y);            
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            
        }
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            const uint threadGroupWorkRegionDim = 16;
            var dispatchX = MathHelper.Roundup(2, threadGroupWorkRegionDim);
            uint dispatchY = 1;// MathHelper.Roundup(ColorOutput.Attachement.Height, threadGroupWorkRegionDim);

            var cmd = BasePass.DrawCmdList;
            cmd.BeginCommand();
            SWRasterizer.SetDrawcallDispatch(policy, SWRasterizerDrawcall, dispatchX, dispatchY, 1, false);
            SWRasterizerDrawcall.Commit(cmd);

            cmd.EndCommand();
            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
        }
        
    }
}
