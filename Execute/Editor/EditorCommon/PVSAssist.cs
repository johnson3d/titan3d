using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon
{
    public class PVSAssist
    {
        public static void GetBuildActors(EngineNS.GamePlay.SceneGraph.GSceneGraph sc,
                                            List<EngineNS.GamePlay.Actor.GActor> allActors,
                                            List<EngineNS.GamePlay.Actor.GActor> pvsActors,
                                            List<EngineNS.GamePlay.Actor.GActor> unPVSActors,
                                            List<EngineNS.GamePlay.Component.ISceneGraphComponent> buildItems)
        {
            using (var i = sc.Actors.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    EngineNS.GamePlay.Actor.GActor actor = i.Current.Value;
                    // 不包含平行光
                    if (actor.GetComponent(typeof(EngineNS.GamePlay.Component.GDirLightComponent)) != null)
                        continue;
                    // 不包含带有SceneGraphComponent的Actor
                    var comp = actor.GetComponent(typeof(EngineNS.GamePlay.Component.ISceneGraphComponent)) as EngineNS.GamePlay.Component.ISceneGraphComponent;
                    if (comp != null)
                    {
                        buildItems.Add(comp);
                        continue;
                    }

                    allActors.Add(actor);

                    if (actor.Initializer.InPVS)
                        pvsActors.Add(actor);
                    else
                        unPVSActors.Add(actor);
                    sc.DefaultSceneNode.RemoveActor(actor);
                }
            }
        }

        public static void CaptureWithPoint(EngineNS.Vector3 point,
                                         EngineNS.Matrix worldMatrix,
                                         EngineNS.Graphics.CGfxCamera camera,
                                         EngineNS.CRenderContext rc,
                                         EngineNS.Vector3 camDir,
                                         EngineNS.Vector3 camUp,
                                         UInt32 actorsCount,
                                         EngineNS.Graphics.RenderPolicy.CGfxRP_SceneCapture rp,
                                         UInt32 textureIdx,
                                         ref EngineNS.Support.CBlobObject dataBlob,
                                         ref EngineNS.Support.CBlobObject picBlob)
        {
            var lookat = point + camDir;
            lookat = EngineNS.Vector3.TransformCoordinate(lookat, worldMatrix);
            point = EngineNS.Vector3.TransformCoordinate(point, worldMatrix);
            camUp = EngineNS.Vector3.TransformNormal(camUp, worldMatrix);
            camera.LookAtLH(point, lookat, camUp);
            camera.BeforeFrame();

            //world.Tick();
            //world.SlowDrawAll(rc, camera);

            var maxId = actorsCount;

            rp.TickLogic(null, rc);
            camera.SwapBuffer(true);
            rp.TickRender(null);
            //camera.ClearAllRenderLayerData();

            // debug
#pragma warning disable 0162
            if (false)
            {
                var texRGB = rp.CaptureSV.FrameBuffer.GetSRV_RenderTarget(1);
                texRGB.Save2File(rc, "e:/testCapture.bmp", EngineNS.EIMAGE_FILE_FORMAT.BMP);
            }
#pragma warning restore 0162

            if (rp.UseCapture)
            {
                var blob = new EngineNS.Support.CBlobObject();
                blob.PushData(rp.TexData0.Data, rp.TexData0.Size);
                dataBlob = blob;
                if (rp.CaptureRGBData)
                {
                    blob = new EngineNS.Support.CBlobObject();
                    blob.PushData(rp.TexData1.Data, rp.TexData1.Size);
                    picBlob = blob;
                }
                //await EngineNS.CEngine.Instance.EventPoster.Post(() =>
                //{
                //    rp.CaptureSV.mFrameBuffer.GetSRV_RenderTarget(0).Save2File($"D:\\{camIdx}.png", (int)mWidth, (int)mHeight, (int)(mWidth * 4), rp.TexData0.ToBytes(), EngineNS.EIMAGE_FILE_FORMAT.PNG);
                //    rp.CaptureSV.mFrameBuffer.GetSRV_RenderTarget(1).Save2File($"D:\\{camIdx}_ref.png", (int)mWidth, (int)mHeight, (int)(mWidth * 4), rp.TexData1.ToBytes(), EngineNS.EIMAGE_FILE_FORMAT.PNG);

                //    return true;
                //}, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            }

            var cmd = EngineNS.CEngine.Instance.RenderContext.ImmCommandList;
            var actirIdTexture = rp.GetActoridTexture();
            cmd.CSSetShaderResource(textureIdx, actirIdTexture);


            uint texSize = 512;
            cmd.CSDispatch(texSize / 8, texSize / 8, 1);
        }

        static EngineNS.Vector3[] camDirs = { EngineNS.Vector3.UnitX, -EngineNS.Vector3.UnitZ, -EngineNS.Vector3.UnitX, EngineNS.Vector3.UnitZ, EngineNS.Vector3.UnitY, -EngineNS.Vector3.UnitY };
        static EngineNS.Vector3[] camUps = { EngineNS.Vector3.UnitY, EngineNS.Vector3.UnitY, EngineNS.Vector3.UnitY, EngineNS.Vector3.UnitY, EngineNS.Vector3.UnitZ, -EngineNS.Vector3.UnitZ };

        public static void ComputeShaderInit(UInt32 numElem,
                                             out UInt32 cbIndex,
                                             out EngineNS.CConstantBuffer cbuffer,
                                             out EngineNS.CGpuBuffer buffer_visible,
                                             out EngineNS.CShaderDesc csMain_visible,
                                             out EngineNS.CComputeShader cs_visible,
                                             out EngineNS.CUnorderedAccessView uav_visible,
                                             out EngineNS.CGpuBuffer buffer_setBit,
                                             out EngineNS.CShaderDesc csMain_setBit,
                                             out EngineNS.CComputeShader cs_setBit,
                                             out EngineNS.CUnorderedAccessView uav_setBit,
                                             out EngineNS.CShaderDesc csMain_Clear,
                                             out EngineNS.CComputeShader cs_Clear,
                                             out UInt32 textureIdx)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;

            const string CSVersion = "cs_5_0";
            var macros = new EngineNS.CShaderDefinitions();
            var shaderFile = EngineNS.RName.GetRName("Shaders/Compute/extractID.compute").Address;
            var shaderFile2 = EngineNS.RName.GetRName("Shaders/Compute/calcrate.compute").Address;
            var shaderFile3 = EngineNS.RName.GetRName("Shaders/Compute/clearid.compute").Address;
            csMain_visible = rc.CompileHLSLFromFile(shaderFile, "CSMain", CSVersion, macros, EngineNS.CIPlatform.Instance.PlatformType);
            csMain_setBit = rc.CompileHLSLFromFile(shaderFile2, "CSMain1", CSVersion, macros, EngineNS.CIPlatform.Instance.PlatformType);
            csMain_Clear = rc.CompileHLSLFromFile(shaderFile3, "CSMain_Clear", CSVersion, macros, EngineNS.CIPlatform.Instance.PlatformType);
            cs_visible = rc.CreateComputeShader(csMain_visible);
            cs_setBit = rc.CreateComputeShader(csMain_setBit);
            cs_Clear = rc.CreateComputeShader(csMain_Clear);

            var bfDesc = new EngineNS.CGpuBufferDesc();
            bfDesc.SetMode(false, true);
            bfDesc.ByteWidth = 4 * numElem;
            bfDesc.StructureByteStride = 4;
            buffer_visible = rc.CreateGpuBuffer(bfDesc, IntPtr.Zero);
            var uavDesc = new EngineNS.CUnorderedAccessViewDesc();
            uavDesc.ToDefault();
            uavDesc.Buffer.NumElements = numElem;
            uav_visible = rc.CreateUnorderedAccessView(buffer_visible, uavDesc);

            // 一个uint保存16个actorID的权重
            var numElemUAV1 = numElem / 16 + 1;
            bfDesc.ByteWidth = 4 * numElemUAV1;
            bfDesc.StructureByteStride = 4;
            buffer_setBit = rc.CreateGpuBuffer(bfDesc, IntPtr.Zero);
            uavDesc.Buffer.NumElements = numElemUAV1;
            uav_setBit = rc.CreateUnorderedAccessView(buffer_setBit, uavDesc);

            cbIndex = csMain_setBit.FindCBufferDesc("CaptureEnv");
            cbuffer = null;
            if ((int)cbIndex != -1)
            {
                cbuffer = rc.CreateConstantBuffer(csMain_setBit, cbIndex);

                var varIdx = cbuffer.FindVar("rateWeight");
                cbuffer.SetValue(varIdx, new EngineNS.Quaternion(1, 1, 10000, 100000), 0);
                cbuffer.FlushContent(rc.ImmCommandList);
            }

            textureIdx = csMain_visible.FindSRVDesc("SrcTexture");

        }
        public static void CaptureGeoBox(EngineNS.Bricks.HollowMaker.Agent.GeoBox geoBox,
                                         EngineNS.Bricks.HollowMaker.GeomScene.AgentBoxs agentData,
                                         EngineNS.Graphics.CGfxCamera camera,
                                         EngineNS.CRenderContext rc,
                                         UInt32 numElem,
                                         EngineNS.Graphics.RenderPolicy.CGfxRP_SceneCapture rp,
                                         EngineNS.CCommandList cmd,
                                         UInt32 cbIndex,
                                         EngineNS.CConstantBuffer cbuffer,
                                         EngineNS.CGpuBuffer buffer_visible,
                                         EngineNS.CShaderDesc csMain_visible,
                                         EngineNS.CComputeShader cs_visible,
                                         EngineNS.CUnorderedAccessView uav_visible,
                                         EngineNS.CGpuBuffer buffer_setBit,
                                         EngineNS.CShaderDesc csMain_setBit,
                                         EngineNS.CComputeShader cs_setBit,
                                         EngineNS.CUnorderedAccessView uav_setBit,
                                         EngineNS.CComputeShader cs_Clear,
                                         List<EngineNS.Support.BitSet> savedBitsets,
                                         UInt32 textureIdx,
                                         Action<int, EngineNS.Support.CBlobObject, EngineNS.Support.CBlobObject> actionAfterCapturePerDir)
        {
            var cornerPos = geoBox.Box.GetCorners();
            //for(int posIdx = 0; posIdx < cornerPos.Length; posIdx++)
            //{
            //    await CaptureSceneWithPoint(cornerPos[posIdx], world, camera, rc, camDirs, camUps, dataBlobs, picBlobs);
            //}
            cmd.SetComputeShader(cs_visible);
            UInt32[] pUAVInitialCounts = new UInt32[1] { 1, };
            cmd.CSSetUnorderedAccessView(0, uav_visible, pUAVInitialCounts);

            //await CaptureSceneWithPoint(geoBox.Box.GetCenter(), agentData.Mat , camera, rc, camDirs, camUps, numElem/*, dataBlobs, picBlobs*/);
            for (int camIdx = 0; camIdx < 6; camIdx++)
            {
                EngineNS.Support.CBlobObject idBlob = null;
                EngineNS.Support.CBlobObject picBlob = null;
                EditorCommon.PVSAssist.CaptureWithPoint(geoBox.Box.GetCenter(), agentData.Mat, camera, rc, camDirs[camIdx], camUps[camIdx], numElem, rp, textureIdx, ref idBlob, ref picBlob);

                actionAfterCapturePerDir?.Invoke(camIdx, idBlob, picBlob);
            }

            // gbuffer0 
            var blob = new EngineNS.Support.CBlobObject();
            buffer_visible.GetBufferData(rc, blob);
            var idArray = blob.ToUInts();

            // fill uav1, clear uav0
            cmd.SetComputeShader(cs_setBit);
            UInt32[] pUAVInitialCounts1 = new UInt32[1] { 1, };
            cmd.CSSetUnorderedAccessView(0, uav_visible, pUAVInitialCounts);
            cmd.CSSetUnorderedAccessView(1, uav_setBit, pUAVInitialCounts);
            var cbDesc = new EngineNS.CConstantBufferDesc();
            if (csMain_setBit.GetCBufferDesc(cbIndex, ref cbDesc))
            {
                if (cbDesc.Type == EngineNS.ECBufferRhiType.SIT_CBUFFER)
                {
                    cmd.CSSetConstantBuffer(cbDesc.CSBindPoint, cbuffer);
                }
            }
            cmd.CSDispatch(numElem, 1, 1);

            var blob0 = new EngineNS.Support.CBlobObject();
            buffer_visible.GetBufferData(rc, blob0);
            var visArray = blob0.ToUInts();
            // gbuffer1
            var blob1 = new EngineNS.Support.CBlobObject();
            buffer_setBit.GetBufferData(rc, blob1);
            var idArray1 = blob1.ToBytes();

            var bitSet = new EngineNS.Support.BitSet();
            var bitSet1 = new EngineNS.Support.BitSet();
            bitSet.Init(numElem * 2, idArray1);
            bitSet1.Init(numElem);
            for (UInt32 e = 0; e < numElem; e++)
            {
                var bit1 = bitSet.IsBit(e * 2 + 0);
                var bit2 = bitSet.IsBit(e * 2 + 1);

                bitSet1.SetBit(e, bit1 || bit2);
            }

            savedBitsets.Add(bitSet1);

            cmd.SetComputeShader(cs_Clear);
            cmd.CSSetUnorderedAccessView(0, uav_visible, pUAVInitialCounts);
            cmd.CSSetUnorderedAccessView(1, uav_setBit, pUAVInitialCounts);
            cmd.CSDispatch(numElem / 16 + 1, 1, 1);
        }
    }
}
