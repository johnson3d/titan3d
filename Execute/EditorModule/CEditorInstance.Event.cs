using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineNS;
using EngineNS.Input;

namespace CoreEditor
{
    partial class CEditorInstance
    {
        public override void MouseDown(object host, object sender,EngineNS.Input.Device.Mouse.MouseInputEventArgs e)
        {
            if (this.World == null)
                return;
            var sg = this.World.DefaultScene;
            if (sg == null)
                return;

            if (host == MainEdViewport)
            {
                Vector3 start = MainEdViewport.Camera.CameraData.Position;
                Vector3 dir = new Vector3();
                if (MainEdViewport.Camera.GetPickRay(ref dir, e.MouseEvent.X, e.MouseEvent.Y, (float)MainEdViewport.GetViewPortWidth(), (float)MainEdViewport.GetViewPortHeight()))
                {
                    var hitResult = new EngineNS.GamePlay.SceneGraph.VHitResult();
                    var end = start + dir * 1000;
                    if(sg.LineCheck(ref start, ref end, ref hitResult))
                    {
                        var hitActor = this.World.DefaultScene.GetHitActor(ref hitResult);
                        if(hitActor!=null)
                        {
                            System.Diagnostics.Debug.WriteLine($"LineCheck Actor {hitActor.Tag}");
                        }
                    }
                }
            }
            base.MouseDown(host, sender, e);

            //// debug
            //TestComputeShader();
        }
        public void TestComputeShader()
        {
            const string CSVersion = "cs_5_0";
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var macros = new CShaderDefinitions();
            macros.SetDefine("USE_STRUCTURED_BUFFERS", "");
            var shaderFile = RName.GetRName("Shaders/Compute/test1.compute").Address;
            var shaderRName_extractID = RName.GetRName("Shaders/Compute/test1.compute");
            var shaderRName_calcRate = RName.GetRName("Shaders/Compute/calcrate.compute");
            //var shaderRName_clearID = RName.GetRName("Shaders/Compute/clearid.compute");

            var csMain0 = rc.CreateShaderDesc(shaderRName_extractID, "CSMain", EShaderType.EST_ComputeShader, macros, EngineNS.CIPlatform.Instance.PlatformType);
            var csMain1 = rc.CreateShaderDesc(shaderRName_calcRate, "CSMain1", EShaderType.EST_ComputeShader, macros, EngineNS.CIPlatform.Instance.PlatformType);
            //var csMain2 = rc.CreateShaderDesc(shaderRName_clearID, "CSMain_Clear", EShaderType.EST_ComputeShader, macros, EngineNS.CIPlatform.Instance.PlatformType);
            var csExtractID = rc.CreateComputeShader(csMain0);
            var csCalcRate = rc.CreateComputeShader(csMain1);
            //var csClearID = rc.CreateComputeShader(csMain2);

            uint numElem = 8;
            var bfDesc = new CGpuBufferDesc();
            bfDesc.SetMode(false, true);
            bfDesc.ByteWidth = 4* numElem;
            bfDesc.StructureByteStride = 4;
            var buffer = rc.CreateGpuBuffer(bfDesc, IntPtr.Zero);
            var uavDesc = new CUnorderedAccessViewDesc();
            uavDesc.ToDefault();
            uavDesc.Buffer.NumElements = numElem;
            var uav = rc.CreateUnorderedAccessView(buffer, uavDesc);

            var buffer1 = rc.CreateGpuBuffer(bfDesc, IntPtr.Zero);
            var uav1 = rc.CreateUnorderedAccessView(buffer1, uavDesc);

            var cmd = rc.ImmCommandList;
            cmd.SetComputeShader(csExtractID);

            UInt32[] pUAVInitialCounts = new UInt32[1] { 1, };

            var dsv = this.RenderPolicy.BaseSceneView.FrameBuffer.GetSRV_DepthStencil();
            var camVP = this.RenderPolicy.Camera.CameraData.ViewProjection;
            //var srcTex = CEngine.Instance.TextureManager.GetShaderRView(rc, RName.GetRName("Texture/testActorID.txpic"), true);
            //if (srcTex != null)
            //{
            //    srcTex.PreUse(true);
            //    cmd.CSSetShaderResource(0, srcTex);
            //}
            cmd.CSSetShaderResource(0, dsv);
            cmd.CSSetUnorderedAccessView(0, uav, pUAVInitialCounts);
            uint texSizeW = 1092;
            uint texSizeH = 516;
            cmd.CSDispatch(texSizeW / 4, texSizeH / 4, 1);
            //uint texSize = 512;
            //cmd.CSDispatch(texSize/8, texSize/8, 1);

            var blob = new EngineNS.Support.CBlobObject();
            buffer.GetBufferData(rc, blob);
            var idArray = blob.ToUInts();

            // fill uav1, clear uav0
            cmd.SetComputeShader(csCalcRate);
            UInt32[] pUAVInitialCounts1 = new UInt32[1] { 1, };
            var tempBufferData = new uint[8];
            tempBufferData[0] = 9543;
            tempBufferData[1] = 3756;
            tempBufferData[2] = 2716;
            tempBufferData[3] = 297;
            tempBufferData[4] = 961;
            tempBufferData[5] = 45046;
            tempBufferData[6] = 0;
            tempBufferData[7] = 5686;
            unsafe
            {
                fixed (uint* pData = &tempBufferData[0])
                {
                    var buffer2 = rc.CreateGpuBuffer(bfDesc, (IntPtr)pData);
                    var uav2 = rc.CreateUnorderedAccessView(buffer2, uavDesc);
                    cmd.CSSetUnorderedAccessView(0, uav2, pUAVInitialCounts);
                }
            }
            //cmd.CSSetUnorderedAccessView(0, uav, pUAVInitialCounts);
            cmd.CSSetUnorderedAccessView(1, uav1, pUAVInitialCounts);
            var cbIndex = csMain1.FindCBufferDesc("CaptureEnv");
            if(cbIndex!=uint.MaxValue)
            {
                var cbuffer = rc.CreateConstantBuffer(csMain1, cbIndex);
                var varIdx = cbuffer.FindVar("rateWeight");
                cbuffer.SetValue(varIdx, new EngineNS.Quaternion(100, 1000, 10000, 100000), 0);
                cbuffer.FlushContent(rc.ImmCommandList);
                var cbDesc = new EngineNS.CConstantBufferDesc();
                if (csMain1.GetCBufferDesc(cbIndex, ref cbDesc))
                {
                    if (cbDesc.Type == EngineNS.ECBufferRhiType.SIT_CBUFFER)
                    {
                        cmd.CSSetConstantBuffer(cbDesc.CSBindPoint, cbuffer);
                    }
                }
            }

            cmd.CSDispatch(8, 1, 1);

            // gbuffer1
            buffer.GetBufferData(rc, blob);
            idArray = blob.ToUInts();
            var blob1 = new EngineNS.Support.CBlobObject();
            buffer1.GetBufferData(rc, blob1);
            var idArray1 = blob1.ToBytes();
            var idUintArray1 = blob1.ToUInts();

        }

        public EngineNS.GamePlay.GOffScreenViewer AxisShower = new EngineNS.GamePlay.GOffScreenViewer();
        private async Task Init_SYJ()
        {
            return;
            await AxisShower.InitEnviroment(256, 256, "AxisShower");

            var axisAct = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName(@"editor\basemesh\box.gms"));

            //axisAct.PreUse(true);//就这个地方用，别的地方别乱用，效率不好
            //AxisShower.Camera.MakeOrtho(2, 2, 0, 2);
            //AxisShower.Camera.LookAtLH(new Vector3(-100, 0, -10), Vector3.Zero, Vector3.UnitY);
            AxisShower.World.AddActor(axisAct);
            AxisShower.World.GetScene(RName.GetRName("AxisShower")).AddActor(axisAct);

            axisAct.Placement.Location = new Vector3(0, 0, 0);

            AxisShower.Start();

            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            var image = await EngineNS.Graphics.Mesh.CGfxImage2D.CreateImage2D(RHICtx, RName.GetRName("Material/bozi.instmtl"), -50, -50, 0, 100, 100);
            var texture = AxisShower.OffScreenTexture;
            image.SetTexture("txDiffuse", texture);

            this.RenderPolicy.OnDrawUI += (cmd, view) =>
            {
                {
                    var tmp = Matrix.Transformation(Vector3.UnitXYZ,
                        Quaternion.Identity, Vector3.Zero);
                    tmp.M41 = 300;
                    tmp.M42 = 300;
                    image.RenderMatrix = tmp;
                    //image.RenderMatrix.M41 = 300;
                    //image.RenderMatrix.M42 = 300; //new Vector3(300, 200, 0)
                    // Matrix.RotationZ(CEngine.Instance.EngineTime * 0.0001f);

                    var pass = image.GetPass();

                    pass.ViewPort = view.Viewport;
                    pass.BindCBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_View, view.ScreenViewCB);
                    pass.ShadingEnv.BindResources(image.Mesh, pass);
                    cmd.PushPass(pass);
                }
            };
        }
    }
}