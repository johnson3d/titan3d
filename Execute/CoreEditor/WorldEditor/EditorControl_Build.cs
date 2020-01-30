using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoreEditor.WorldEditor
{
    public partial class EditorControl
    {
        EngineNS.Graphics.RenderPolicy.CGfxRP_SceneCapture mSceneCaptureRP;
        UInt32 mWidth = 512;
        UInt32 mHeight = 512;

        async Task<bool> BuildScene()
        {
            using(var it = VP1.World.GetSceneEnumerator())
            {
                while(it.MoveNext())
                {
                    var sc = it.Current.Value;
                    BuildGeoScene(sc);

                    // Actor
                    // 取出所有需要build的对象
                    var allActors = new List<EngineNS.GamePlay.Actor.GActor>(sc.Actors.Count);
                    var pvsActors = new List<EngineNS.GamePlay.Actor.GActor>(sc.Actors.Count);
                    var unPVSActors = new List<EngineNS.GamePlay.Actor.GActor>(sc.Actors.Count);
                    var buildItems = new List<EngineNS.GamePlay.Component.ISceneGraphComponent>();
                    EditorCommon.PVSAssist.GetBuildActors(sc, allActors, pvsActors, unPVSActors, buildItems);

                    // 对所有PVS actor编号
                    sc.PVSActors = new EngineNS.GamePlay.Actor.GActor[pvsActors.Count];
                    for (int i = 0; i < pvsActors.Count; i++)
                    {
                        pvsActors[i].PVSId = (UInt32)i;
                        sc.PVSActors[i] = pvsActors[i];
                    }

                    bool reBuildSceneCapture = true;
                    if (reBuildSceneCapture)
                        await BuildSceneCaptures(sc, pvsActors);
                    sc.PvsSets = new List<EngineNS.GamePlay.SceneGraph.GPvsSet>();
                    await BuildPVS(sc, sc.PvsSets);

                    // PVS
                    sc.PVSTree = new EngineNS.GamePlay.SceneGraph.GPVSOctree();
                    await sc.PVSTree.BuildTree(sc.PvsSets);

                    // 所有对象及非PVS对象处理
                    // 全对象的场景图的目的在于防止摄像机不在pvs范围内时看不到对象
                    var containActors = new List<EngineNS.GamePlay.Actor.GActor>();
                    foreach (var item in buildItems)
                    {
                        var cas = new List<EngineNS.GamePlay.Actor.GActor>();
                        if (await item.Build(allActors, unPVSActors, cas) == false)
                            return false;
                        containActors.AddRange(cas);
                    }

                    foreach (var act in containActors)
                        allActors.Remove(act);
                    foreach (var act in pvsActors)
                        allActors.Remove(act);

                    foreach (var act in allActors)
                    {
                        sc.DefaultSceneNode.AddActor(act);
                    }
                }
            }

            //sceneGraph.RefreshFromWorld(VP1.World);

            return true;
        }


        private void MenuItem_BuildSceneCapture_Click(object sender, RoutedEventArgs e)
        {
            //var noUse = BuildSceneCaptures(VP1.World.DefaultScene);
            //var noUse = Test_Init();

            //VP1.TickLogicEvent = Test_TickLogic;
            //VP1.TickRenderEvent = Test_TickRender;
            //VP1.TickSyncEvent = Test_TickSync;
        }

        /*EngineNS.Graphics.CGfxCamera mCamera;
        async Task Test_Init()
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            mCamera = new EngineNS.Graphics.CGfxCamera();
            mCamera.Init(rc, false);
            mCamera.PerspectiveFovLH((float)(System.Math.PI * 0.6f), mWidth, mHeight);
            mSceneCaptureRP = new EngineNS.Graphics.RenderPolicy.CGfxRP_SceneCapture();
            await mSceneCaptureRP.Init(rc, mWidth, mHeight, mCamera, IntPtr.Zero);
        }

        void Test_TickLogic(EditorCommon.ViewPort.ViewPortControl vp)
        {
        }
        private void Test_TickRender(EditorCommon.ViewPort.ViewPortControl vp)
        {
            if (mCamera == null)
                return;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            EngineNS.Vector3[] camDirs = { EngineNS.Vector3.UnitX, -EngineNS.Vector3.UnitZ, -EngineNS.Vector3.UnitX, EngineNS.Vector3.UnitZ, EngineNS.Vector3.UnitY, -EngineNS.Vector3.UnitY };
            EngineNS.Vector3[] camUps = { EngineNS.Vector3.UnitY, EngineNS.Vector3.UnitY, EngineNS.Vector3.UnitY, EngineNS.Vector3.UnitY, EngineNS.Vector3.UnitZ, -EngineNS.Vector3.UnitZ };
            rc.BindCurrentSwapChain(vp.RPolicy.mSwapChain);
            for (int camIdx = 0; camIdx < 6; camIdx++)
            {
                var dir = camDirs[camIdx];
                var pos = new EngineNS.Vector3(0, 1, 0);
                var up = camUps[camIdx];
                var lookat = pos + dir;
                mCamera.LookAtLH(pos, lookat, up);
                mCamera.BeforeFrame();

                vp.World.CheckVisible(rc, mCamera);
                vp.World.Tick();

                mSceneCaptureRP.TickLogic(null, rc);
                mCamera.SwapBuffer(true);

                mSceneCaptureRP.TickRender(null);
                mCamera.ClearAllRenderLayerData();
            }
            //rc.Present(0, 0);

        }
        private void Test_TickSync(EditorCommon.ViewPort.ViewPortControl vp)
        {

        }*/

        async Task BuildSceneCaptures(EngineNS.GamePlay.SceneGraph.GSceneGraph processScene, List<EngineNS.GamePlay.Actor.GActor> drawActors)
        {
            if(processScene.SceneFilename == EngineNS.RName.EmptyName)
            {
                EditorCommon.MessageBox.Show("没有找到当前场景的保存路径！");
                return;
            }
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var camera = new EngineNS.Graphics.CGfxCamera();
            camera.Init(rc, false);
            camera.PerspectiveFovLH((float)(System.Math.PI * 0.6f), mWidth, mHeight);
            mSceneCaptureRP = new EngineNS.Graphics.RenderPolicy.CGfxRP_SceneCapture();
            await mSceneCaptureRP.Init(rc, mWidth, mHeight, camera, IntPtr.Zero);
            mSceneCaptureRP.CaptureRGBData = false;

            // Compute shader
            //////const string CSVersion = "cs_5_0";
            //////var macros = new EngineNS.CShaderDefinitions();
            //////var shaderFile = EngineNS.RName.GetRName("Shaders/Compute/extractID.compute").Address;
            //////var csMain0 = rc.CompileHLSLFromFile(shaderFile, "CSMain", CSVersion, macros);
            //////var csMain1 = rc.CompileHLSLFromFile(shaderFile, "CSMain1", CSVersion, macros);
            //////var cs = rc.CreateComputeShader(csMain0);
            //////var cs1 = rc.CreateComputeShader(csMain1);

            uint numElem = (UInt32)drawActors.Count;
            //// debug
            //numElem = 255 * 255 * 2;
            //////var bfDesc = new EngineNS.CGpuBufferDesc();
            //////bfDesc.ToDefault();
            //////bfDesc.ByteWidth = 4 * numElem;
            //////bfDesc.StructureByteStride = 4;
            //////var buffer = rc.CreateGpuBuffer(bfDesc, IntPtr.Zero);
            //////var uavDesc = new EngineNS.CUnorderedAccessViewDesc();
            //////uavDesc.ToDefault();
            //////uavDesc.Buffer.NumElements = numElem;
            //////var uav = rc.CreateUnorderedAccessView(buffer, uavDesc);

            //////// 一个uint保存16个actorID的权重
            //////var numElemUAV1 = numElem / 16 + 1;
            //////bfDesc.ByteWidth = 4* numElemUAV1;
            //////bfDesc.StructureByteStride = 4;
            //////var buffer1 = rc.CreateGpuBuffer(bfDesc, IntPtr.Zero);
            //////uavDesc.Buffer.NumElements = numElemUAV1;
            //////var uav1 = rc.CreateUnorderedAccessView(buffer1, uavDesc);

            //////var cbIndex = csMain1.FindCBufferDesc("CaptureEnv");
            //////EngineNS.CConstantBuffer cbuffer = null;
            //////if((int)cbIndex!=-1)
            //////{
            //////    cbuffer = rc.CreateConstantBuffer(csMain1, cbIndex);
            
            //////    var varIdx = cbuffer.FindVar("rateWeight");
            //////    cbuffer.SetValue(varIdx, new EngineNS.Quaternion(1, 1, 10000, 100000), 0);
            //////    cbuffer.FlushContent2(rc);
            //////}

            //////var texIdx = csMain0.FindSRVDesc("SrcTexture");

            var cmd = rc.ImmCommandList;

            UInt32 cbIndex;
            EngineNS.CConstantBuffer cbuffer;
            EngineNS.CGpuBuffer buffer_visible;
            EngineNS.CShaderDesc csMain_visible;
            EngineNS.CComputeShader cs_visible;
            EngineNS.CUnorderedAccessView uav_visible;
            EngineNS.CGpuBuffer buffer_setBit;
            EngineNS.CShaderDesc csMain_setBit;
            EngineNS.CComputeShader cs_setBit;
            EngineNS.CUnorderedAccessView uav_setBit;
            EngineNS.CShaderDesc csMain_Clear;
            EngineNS.CComputeShader cs_clear;
            UInt32 textureIdx;
            EditorCommon.PVSAssist.ComputeShaderInit(numElem, out cbIndex, out cbuffer, out buffer_visible, out csMain_visible, out cs_visible, out uav_visible, out buffer_setBit, 
                out csMain_setBit, out cs_setBit, out uav_setBit, 
                out csMain_Clear, out cs_clear, 
                out textureIdx);

            var checkVisibleParam = new EngineNS.GamePlay.SceneGraph.CheckVisibleParam();
            foreach (var actor in drawActors)
            {
                actor.OnCheckVisible(rc.ImmCommandList, processScene, camera, checkVisibleParam);
            }
            //mSceneCaptureRP.TickLogic(null, rc);

            int VoxelMemSize = ((int)numElem / 8 + 1) + 4;

            // 读取geoScene
            var t1 = EngineNS.Support.Time.HighPrecision_GetTickCount();
            EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Info, "Editor", "Begin Capture Scene VIS");
            var geoScene = await EngineNS.Bricks.HollowMaker.GeomScene.CreateGeomScene(processScene.SceneFilename.Address + "/geoscene.dat");
            EditorCommon.Controls.Debugger.PVSDebugger.GeoScene = geoScene;
            for (int j = 0; j < geoScene.AgentDatas.Count; j++)
            {
                var capDataDir = processScene.SceneFilename.Address + "/capturedata/" + j + "/";
                if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(capDataDir))
                {
                    EngineNS.CEngine.Instance.FileManager.CreateDirectory(capDataDir);
                }

                List<EngineNS.Support.BitSet> savedBitsets = new List<EngineNS.Support.BitSet>();
                var agentData = geoScene.AgentDatas[j];
                var memSize = VoxelMemSize * agentData.AgentData.Count;
                for (int i = 0; i < agentData.AgentData.Count; i++)
                {
                    var geoBox = geoScene.AgentDatas[j].AgentData[i];
                    EditorCommon.PVSAssist.CaptureGeoBox(geoBox, agentData, camera, rc, numElem, mSceneCaptureRP, cmd, cbIndex, cbuffer, buffer_visible, csMain_visible, cs_visible, uav_visible, buffer_setBit, csMain_setBit, cs_setBit, uav_setBit, cs_clear, savedBitsets, textureIdx, null);
                }

                var saveXnd = EngineNS.IO.XndHolder.NewXNDHolder();
                var voxelAtt = saveXnd.Node.AddAttrib("voxelData");
                voxelAtt.BeginWrite();
                voxelAtt.Write(agentData.BVSize);
                voxelAtt.Write(agentData.Mat);
                voxelAtt.Write(agentData.AgentData.Count);
                for (int i = 0; i < agentData.AgentData.Count; i++)
                {
                    var geoBox = agentData.AgentData[i];
                    var pvsVoxel = new EngineNS.Bricks.HollowMaker.PVSVoxel();
                    pvsVoxel.Shape = geoBox.Box;
                    pvsVoxel.X = geoBox.X;
                    pvsVoxel.Y = geoBox.Y;
                    pvsVoxel.Z = geoBox.Z;
                    pvsVoxel.Bits = savedBitsets[i];
                    pvsVoxel.LinkedVoxels = new List<int>(geoBox.Neighbors);

                    var str = pvsVoxel.Bits.ToBase64String();
                    pvsVoxel.BitsHash = (UInt32)(str.GetHashCode());
                    
                    voxelAtt.WriteMetaObject(pvsVoxel);
                }
                voxelAtt.EndWrite();
                EngineNS.IO.XndHolder.SaveXND(capDataDir + "vis.data", saveXnd);

                System.GC.Collect();
            }


            var t2 = EngineNS.Support.Time.HighPrecision_GetTickCount();
            EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Info, "Editor", $"End Capture Scene VIS,Time = {(t2-t1)/1000000}");

            mSceneCaptureRP.Cleanup();
        }

        private void MenuItem_BuildPVS_Click(object sender, RoutedEventArgs e)
        {
            List<EngineNS.GamePlay.SceneGraph.GPvsSet> pvsSets = new List<EngineNS.GamePlay.SceneGraph.GPvsSet>();
            var noused = BuildPVS(VP1.World.DefaultScene, pvsSets);
        }

        private void MenuItem_BuildGeoScene_Click(object sender, RoutedEventArgs e)
        {
            BuildGeoScene(VP1.World.DefaultScene);
        }

        public EngineNS.Bricks.HollowMaker.Agent HollowMakerAgent = new EngineNS.Bricks.HollowMaker.Agent();

        public void BuildWalkables(EngineNS.Vector3 startpos, EngineNS.Vector3 pos, EngineNS.Vector3 scale, EngineNS.Quaternion rotation)
        {
            //HollowMakerAgent.BuildGeoBoxs(box.Minimum.X, box.Minimum.Y, box.Minimum.Z,
            //               box.Maximum.X - box.Minimum.X,
            //               box.Maximum.Y - box.Minimum.Y,
            //               box.Maximum.Z - box.Minimum.Z);

            HollowMakerAgent.BuildGeoBoxs(pos, scale, rotation);


            //var iX = Math.Min((int)(((startpos.X - pos.X) + scale.X * 0.5f) / HollowMakerAgent.AgentGridSize), HollowMakerAgent.GeoBoxs.GetLength(0) - 1);
            //var iY = Math.Min((int)(((startpos.Y - pos.Y) + scale.Y * 0.5f) / HollowMakerAgent.AgentGridSize), HollowMakerAgent.GeoBoxs.GetLength(1) - 1);
            //var iZ = Math.Min((int)(((startpos.Z - pos.Z) + scale.Z * 0.5f) / HollowMakerAgent.AgentGridSize), HollowMakerAgent.GeoBoxs.GetLength(2) - 1);
            // startpos的位置是相对与volumn的，这里不能再减volumne的绝对位置，而且要保证startpos在volume里面
            var iX = Math.Min((int)((startpos.X + scale.X * 0.5f) / HollowMakerAgent.AgentGridSize), HollowMakerAgent.GeoBoxs.GetLength(0) - 1);
            var iY = Math.Min((int)((startpos.Y + scale.Y * 0.5f) / HollowMakerAgent.AgentGridSize), HollowMakerAgent.GeoBoxs.GetLength(1) - 1);
            var iZ = Math.Min((int)((startpos.Z + scale.Z * 0.5f) / HollowMakerAgent.AgentGridSize), HollowMakerAgent.GeoBoxs.GetLength(2) - 1);

            //int maxDepth = 0;
            //HollowMakerAgent.BuildWalkables(VP1.World, HollowMakerAgent.GeoBoxs[iX, iY, iZ], 0, ref maxDepth);
            var stack = new Stack<EngineNS.Bricks.HollowMaker.Agent.GeoBox>();
            stack.Push(HollowMakerAgent.GeoBoxs[iX, iY, iZ]);
            HollowMakerAgent.BuildWalkables2(VP1.World, stack);
       
        }

        public void BuildGeoScene(EngineNS.GamePlay.SceneGraph.GSceneGraph scene)
        {
            if (scene.SceneFilename == null)
                return;

            EngineNS.Bricks.HollowMaker.GeomScene geomscene = new EngineNS.Bricks.HollowMaker.GeomScene();
            HollowMakerAgent.AgentComponenetBox = new List<EngineNS.BoundingBox>();
            float osize = HollowMakerAgent.AgentGridSize;
            //先处理一遍摆放好的AgentGeomBoxComponent..
            foreach (var value in scene.Actors)
            {
                EngineNS.GamePlay.Actor.GActor actor = value.Value;
                if (actor != null)
                {
                    EngineNS.Bricks.HollowMaker.AgentGeomBoxComponent acom = actor.GetComponent<EngineNS.Bricks.HollowMaker.AgentGeomBoxComponent>();
                    
                    if (acom != null)
                    {
                        
                        HollowMakerAgent.AgentGridSize = acom.AgentGridSize;
                        EngineNS.Vector3 scale;
                        EngineNS.Quaternion rotation;
                        EngineNS.Vector3 translation;
                        if (actor.Placement != null && actor.Placement.WorldMatrix.Decompose(out scale, out rotation, out translation))
                        {
                            geomscene.AgentData = new List<EngineNS.Bricks.HollowMaker.Agent.GeoBox>();
                            BuildWalkables(acom.StartPos, translation, scale, rotation);
                            HollowMakerAgent.BuildGeomScene(geomscene);
                            //TODO..
                            HollowMakerAgent.AgentComponenetBox.Add(acom.Box);

                            EngineNS.Bricks.HollowMaker.GeomScene.AgentBoxs AgentBoxs  = new EngineNS.Bricks.HollowMaker.GeomScene.AgentBoxs();
                            AgentBoxs.AgentData = geomscene.AgentData;
                            AgentBoxs.Mat = EngineNS.Matrix.Transformation(EngineNS.Vector3.UnitXYZ, rotation, translation);
                            AgentBoxs.BVSize = scale;
                            geomscene.AgentDatas.Add(AgentBoxs);
                        }
                            
                    }
                }
            }

            //HollowMakerAgent.AgentGridSize = osize;
            //foreach (var value in scene.Actors)
            //{
            //    EngineNS.GamePlay.Actor.GActor actor;
            //    var compment = value.Value.TryGetTarget(out actor);
            //    if (actor != null)
            //    {
            //        EngineNS.LooseOctree.OctreeVolumeComponent com = actor.GetComponent<EngineNS.LooseOctree.OctreeVolumeComponent>();
            //        if (com != null)
            //        {
            //            EngineNS.BoundingBox box = new EngineNS.BoundingBox();
            //            actor.GetAABB(ref box);
            //            BuildWalkables(box);
            //            HollowMakerAgent.BuildGeomScene(geomscene);
            //        }
            //    }
            //}

            // var test = geomscene.CreateRenderInfos(VP1.World);

            geomscene.SaveXND(scene.SceneFilename.Address + "/geoscene.dat");
            //EngineNS.Bricks.HollowMaker.GeomScene temp = await EngineNS.Bricks.HollowMaker.GeomScene.CreateGeomScene(scene.SceneFilename.Address + "/geoscene.dat");
            //int xx = 0;
        }


        List<EngineNS.GamePlay.Actor.GActor> mConvexShowActor;
        private async Task BuildPVS(EngineNS.GamePlay.SceneGraph.GSceneGraph scene, List<EngineNS.GamePlay.SceneGraph.GPvsSet> pvsSets)
        {
            pvsSets.Clear();

            var rootDir = scene.SceneFilename.Address + "/capturedata/";
            if(EngineNS.CEngine.Instance.FileManager.DirectoryExists(rootDir))
            {
                var dirs = EngineNS.CEngine.Instance.FileManager.GetDirectories(rootDir);
                foreach (var capDataDir in dirs)
                {
                    int tolerance = 20;
                    var builder = new EngineNS.Bricks.HollowMaker.PVSBuilder();

                    builder.LoadPVSVoxels(capDataDir);
                    builder.BuildUnitedCluster(tolerance);

                    var bvs = new List<EngineNS.Bricks.HollowMaker.CombineVoxel.BoxVolume>();
                    builder.BuildBoxVolume(bvs);

                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Info, "PVSBuilder",
                                $"PVS({capDataDir}):容差={tolerance};BitSet={builder.UnitedCluster.Count}; BoxVolume={bvs.Count};");


                    //var xnd = EngineNS.IO.XndHolder.NewXNDHolder();
                    //builder.SaveXnd(xnd.Node, bvs);
                    var set = builder.CreatePVSSet(bvs);
                    pvsSets.Add(set);

                    //EngineNS.IO.XndHolder.SaveXND(EngineNS.RName.GetRName("Test.pvs").Address, xnd);

                    bool bDebugConvexMesh = false;
                    if (bDebugConvexMesh)
                    {
                        if (mConvexShowActor != null)
                        {
                            foreach (var i in mConvexShowActor)
                            {
                                VP1.World.RemoveEditorActor(i.ActorId);
                            }
                            mConvexShowActor = null;
                        }

                        var meshes = new List<EngineNS.Graphics.Mesh.CGfxMeshPrimitives>();
                        for (int i = 0; i < builder.UnitedCluster.Count; i++)
                        {
                            meshes.Add(builder.UnitedCluster[i].BuildMesh());
                        }
                        //int clustIndex = 1;
                        //meshes.Add(builder.UnitedCluster[clustIndex].BuildMesh());
                        mConvexShowActor = new List<EngineNS.GamePlay.Actor.GActor>();

                        foreach (var i in meshes)
                        {
                            var rc = EngineNS.CEngine.Instance.RenderContext;
                            var gms = EngineNS.CEngine.Instance.MeshManager.CreateMesh(rc, i);

                            var act = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(gms);
                            var meshComp = act.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();

                            var mat = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.RName.GetRName(@"editor\icon\icon_3D\material\area.instmtl"));
                            await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                            VP1.World.AddEditorActor(act);
                            act.Placement.Location = EngineNS.Vector3.TransformCoordinate(EngineNS.Vector3.Zero, set.WorldMatrix);
                            act.Placement.Scale = EngineNS.Vector3.UnitXYZ;
                            act.Placement.Rotation = EngineNS.Quaternion.RotationMatrix(set.WorldMatrix);
                            mConvexShowActor.Add(act);
                        }
                    }
                }
            }
        }
    }
}
