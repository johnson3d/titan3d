using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetImport;
using EngineNS;

namespace CoreEditor
{
    
    class CEditorInstance : EngineNS.Editor.CEditorInstance
    {
        public EngineNS.GamePlay.Actor.GActor ActorBeauty0;
        public EngineNS.GamePlay.Actor.GActor ActorBeauty1;
        public EngineNS.GamePlay.Actor.GActor ActorBeautyNew0;
        public EngineNS.GamePlay.Actor.GActor ActorBeautyNew1;
        public EngineNS.GamePlay.Actor.GActor ActorBeautyNew2;
        public EngineNS.GamePlay.Actor.GActor ActorBeautyNew3;
        public EngineNS.GamePlay.Actor.GActor ActorBeautyNew4;
        public EngineNS.GamePlay.Actor.GActor ActorBeautyNew5;

        public EngineNS.GamePlay.Actor.GActor ActorFont0;

        public EngineNS.GamePlay.Actor.GActor ActorEye0;
        public EngineNS.GamePlay.Actor.GActor ActorEye1;
        public EngineNS.GamePlay.Actor.GActor ActorEye2;

        public EngineNS.GamePlay.Actor.GActor ActorRoom;


        bool mInitialized = false;
        EditorCommon.ViewPort.ViewPortControl MainEdViewport;
        EngineNS.Bricks.GraphDrawer.GraphLinesHelper LineDrawer = null;
        public async System.Threading.Tasks.Task InitEditor(MainWindow window, EngineNS.GamePlay.GGameInstanceDesc desc)
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            await EngineNS.CEngine.Instance.OnEngineInited();
            await InitGame(desc);

            MainEdViewport = window.MainCtrl.VP1;
            MainEdViewport.InitEnviroment();

            Vector3 Eye = new Vector3();
            Eye.SetValue(0.0f, 1.5f, -3.6f);
            Vector3 At = new Vector3();
            At.SetValue(0.0f, 1.0f, 0.0f);
            Vector3 Up = new Vector3();
            Up.SetValue(0.0f, 1.0f, 0.0f);
            MainEdViewport.Camera.LookAtLH(Eye, At, Up);

            MainEdViewport.World = this.World;
            //MainEdViewport.RPolicy = new EngineNS.Graphics.CGfxRPolicy_Default();
            MainEdViewport.RPolicy = new EngineNS.Graphics.RenderPolicy.CGfxRP_UFOMobile();
            //MainEdViewport.RPolicy = new EngineNS.Graphics.RenderPolicy.CGfxRP_UFOMobileNPR();


            //((EngineNS.Graphics.CGfxRPolicy_Default)MainEdViewport.RPolicy).Init(RHICtx, (uint)MainEdViewport.GetWidth(), (uint)MainEdViewport.GetHeight(), MainEdViewport.Camera, MainEdViewport.DrawHandle);
            ((EngineNS.Graphics.RenderPolicy.CGfxRP_UFOMobile)MainEdViewport.RPolicy).Init(RHICtx, (uint)MainEdViewport.GetWidth(), (uint)MainEdViewport.GetHeight(), MainEdViewport.Camera, MainEdViewport.DrawHandle);
            //((EngineNS.Graphics.RenderPolicy.CGfxRP_UFOMobileNPR)MainEdViewport.RPolicy).Init(RHICtx, (uint)MainEdViewport.GetWidth(), (uint)MainEdViewport.GetHeight(), MainEdViewport.Camera, MainEdViewport.DrawHandle);


            MainEdViewport.TickLogicEvent = TickMainEdViewport;
            

            //var fbxName = RName.GetRName("Mesh/text.fbx");
            //CreateFromFBX(fbxName);
            //fbxName = RName.GetRName("Mesh/batmanrun.fbx");
            //CreateFromFBX(fbxName);

            //TheBox = await CreateMeshActorAsync(this.World, desc.SceneName
            //    , RName.GetRName("Mesh/tutor_elf_majestic.gms")
            //    , new Vector3(0, -0.5f, -5.0f)
            //    , new Vector3(1, 1, 1) * 0.01f);

            var meshes = new List<RName>()
            {
                RName.GetRName("Mesh/mannequin_geo_arm_l.gms"),
                RName.GetRName("Mesh/mannequin_geo_arm_r.gms"),
                RName.GetRName("Mesh/mannequin_geo_calf_l.gms"),
                RName.GetRName("Mesh/mannequin_geo_calf_r.gms"),
                RName.GetRName("Mesh/mannequin_geo_chest.gms"),
                RName.GetRName("Mesh/mannequin_geo_foot_l.gms"),
                RName.GetRName("Mesh/mannequin_geo_foot_r.gms"),
                RName.GetRName("Mesh/mannequin_geo_forearm_l.gms"),
                RName.GetRName("Mesh/mannequin_geo_forearm_r.gms"),
                RName.GetRName("Mesh/mannequin_geo_hand_l.gms"),
                RName.GetRName("Mesh/mannequin_geo_hand_r.gms"),
                RName.GetRName("Mesh/mannequin_geo_head.gms"),
                RName.GetRName("Mesh/mannequin_geo_logo.gms"),
                RName.GetRName("Mesh/mannequin_geo_lower_torso.gms"),
                RName.GetRName("Mesh/mannequin_geo_neck.gms"),
                RName.GetRName("Mesh/mannequin_geo_pelvis.gms"),
                RName.GetRName("Mesh/mannequin_geo_thigh_l.gms"),
                RName.GetRName("Mesh/mannequin_geo_thigh_r.gms"),
        };

            {
                ActorRoom = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/room.gms"));
                ActorRoom.Placement.Location = new Vector3(0.0f, 0.0f, 0.0f);
                ActorRoom.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, 0.0f);
                ActorRoom.Placement.Scale /=  40.0f;
                this.World.AddActor(ActorRoom);
                this.World.Scenes[desc.SceneName].AddActor(ActorRoom);


                ActorFont0 = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/text.gms"));
                ActorFont0.Placement.Location = new Vector3(0.0f, 1.8f, 3.0f);
                ActorFont0.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitX, 3.14f);
                ActorFont0.Placement.Scale /= 100.0f;
                this.World.AddActor(ActorFont0);
                this.World.Scenes[desc.SceneName].AddActor(ActorFont0);


                ActorBeauty0 = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/tutor_elf_majestic.gms"));
                ActorBeauty0.Placement.Location = new Vector3(-2.5f, 0.0f, 0.0f);
                ActorBeauty0.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, 1.57f);
                ActorBeauty0.Placement.Scale /= 100;
                this.World.AddActor(ActorBeauty0);
                this.World.Scenes[desc.SceneName].AddActor(ActorBeauty0);


                ActorBeauty1 = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/tutor_elf_majestic.gms"));
                ActorBeauty1.Placement.Location = new Vector3(-1.5f, 0.0f, 0.0f);
                ActorBeauty1.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, -1.57f);
                ActorBeauty1.Placement.Scale /= 100;
                this.World.AddActor(ActorBeauty1);
                this.World.Scenes[desc.SceneName].AddActor(ActorBeauty1);


                ActorBeautyNew0 = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/meimei.gms"));
                ActorBeautyNew0.Placement.Location = new Vector3(1.5f, 0.0f, 0.0f);
                ActorBeautyNew0.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, 1.57f);
                ActorBeautyNew0.Placement.Scale /= 40;
                this.World.AddActor(ActorBeautyNew0);
                this.World.Scenes[desc.SceneName].AddActor(ActorBeautyNew0);

                ActorBeautyNew1 = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/meimei.gms"));
                ActorBeautyNew1.Placement.Location = new Vector3(2.5f, 0.0f, 0.0f);
                ActorBeautyNew1.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, -1.57f);
                ActorBeautyNew1.Placement.Scale /= 40;
                this.World.AddActor(ActorBeautyNew1);
                this.World.Scenes[desc.SceneName].AddActor(ActorBeautyNew1);


                ActorBeautyNew2 = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/mm.gms"));
                ActorBeautyNew2.Placement.Location = new Vector3(3.5f, 0.0f, 0.0f);
                ActorBeautyNew2.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, 1.57f);
                ActorBeautyNew2.Placement.Scale /= 40;
                this.World.AddActor(ActorBeautyNew2);
                this.World.Scenes[desc.SceneName].AddActor(ActorBeautyNew2);

                ActorBeautyNew3 = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/mm.gms"));
                ActorBeautyNew3.Placement.Location = new Vector3(4.5f, 0.0f, 0.0f);
                ActorBeautyNew3.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, -1.57f);
                ActorBeautyNew3.Placement.Scale /= 40;
                this.World.AddActor(ActorBeautyNew3);
                this.World.Scenes[desc.SceneName].AddActor(ActorBeautyNew3);

                ActorBeautyNew4 = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/mmhair05.gms"));
                ActorBeautyNew4.Placement.Location = new Vector3(0.5f, 0.0f, 0.0f);
                ActorBeautyNew4.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, -1.57f);
                ActorBeautyNew4.Placement.Scale /= 40;
                this.World.AddActor(ActorBeautyNew4);
                this.World.Scenes[desc.SceneName].AddActor(ActorBeautyNew4);

                ActorBeautyNew5 = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/mmhair05.gms"));
                ActorBeautyNew5.Placement.Location = new Vector3(-0.5f, 0.0f, 0.0f);
                ActorBeautyNew5.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, 1.57f);
                ActorBeautyNew5.Placement.Scale /= 40;
                this.World.AddActor(ActorBeautyNew5);
                this.World.Scenes[desc.SceneName].AddActor(ActorBeautyNew5);



                var EyeMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(RHICtx, RName.GetRName("Mesh/sphere001.gms"));
                ActorEye0 = EngineNS.GamePlay.Actor.GActor.NewMeshActor(EyeMesh);
                ActorEye0.Placement.Location = new Vector3(-20.0f, 0.0f, 0.0f);
                ActorEye0.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, 1.57f);
                ActorEye0.Placement.Scale /= 2;
                this.World.AddActor(ActorEye0);
                this.World.Scenes[desc.SceneName].AddActor(ActorEye0);

                EyeMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(RHICtx, RName.GetRName("Mesh/eye0.gms"));
                ActorEye1 = EngineNS.GamePlay.Actor.GActor.NewMeshActor(EyeMesh);
                ActorEye1.Placement.Location = new Vector3(-24.0f, 0.0f, 0.0f);
                ActorEye1.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, 1.57f);
                ActorEye1.Placement.Scale /= 1;
                this.World.AddActor(ActorEye1);
                this.World.Scenes[desc.SceneName].AddActor(ActorEye1);

                EyeMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(RHICtx, RName.GetRName("Mesh/eye3.gms"));
                ActorEye2 = EngineNS.GamePlay.Actor.GActor.NewMeshActor(EyeMesh);
                ActorEye2.Placement.Location = new Vector3(-28.0f, 0.0f, 0.0f);
                ActorEye2.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, 1.57f);
                ActorEye2.Placement.Scale /= 1;
                this.World.AddActor(ActorEye2);
                this.World.Scenes[desc.SceneName].AddActor(ActorEye2);



            }

            //{
            //    await CreateMeshActorAsync(this.World, desc.SceneName
            //        , RName.GetRName("Mesh/kianac5.gms")
            //        , new Vector3(0.0f, 0.0f, 0.0f)
            //        , new Vector3(1, 1, 1));

            //    await CreateMeshActorAsync(this.World, desc.SceneName
            //        , RName.GetRName("Mesh/kianac5.gms")
            //        , new Vector3(1.5f, 0.0f, 0.0f)
            //        , Quaternion.RotationAxis(Vector3.UnitY, 3.14f)
            //        , new Vector3(1, 1, 1));


            //    await CreateMeshActorAsync(this.World, desc.SceneName
            //        , RName.GetRName("Mesh/unitsphere.gms")
            //        , new Vector3(-15.0f, 1.0f, 0.0f)
            //        , new Vector3(1, 1, 1));


            //    var gmsName = RName.GetRName("Mesh/delishatest.gms");
            //    var animationName = RName.GetRName("Mesh/delisha" + CEngineDesc.AnimationSegementExtension);
            //    //var gmsName = RName.GetRName("Mesh/batman_highpoly_1.gms");
            //    //var animationName = RName.GetRName("Mesh/batman_highpoly_1" + CEngineDesc.AnimationSegementExtension);

            //    TheActor = await CreateMeshActorAsync(this.World, desc.SceneName
            //        , gmsName
            //        , new Vector3(8.0f, 0.0f, 0.0f)
            //        , Vector3.UnitXYZ * 0.1f);
            //    var animation = CreateAnimation(TheActor, animationName);
            //    animation.PlayRate = 1.0f;
            //    CreateLegAndFootIK(TheActor);
            //}


            //TheBox = await CreateMeshActorAsync(this.World, desc.SceneName
            //    , RName.GetRName("Mesh/unitbox.gms")
            //    , new Vector3(0, -0.5f, -5.0f)
            //    , new Vector3(10, 1, 10));

            var skyMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(RHICtx, RName.GetRName("Mesh/sky.gms"));
            TheSky = EngineNS.GamePlay.Actor.GActor.NewMeshActor(skyMesh);
            TheSky.Placement.Scale = new Vector3(0.3F, 0.3F, 0.3F);
            this.World.AddActor(TheSky);
            this.World.Scenes[desc.SceneName].AddActor(TheSky);

            //////var particle = new EngineNS.Bricks.Particle.McParticleHelper().CreateParticleActor();
            //////this.World.AddActor(particle);
            //////this.World.Scenes[desc.SceneName].AddActor(particle);

            CEngine.Instance.TickManager.AddTickInfo(this);

            bool doUnitTest = false;
            if (doUnitTest)
            {
                await DoUnitTest();
            }

            //var start = new Vector3(350, 350, 0);
            //var ldr = new EngineNS.Bricks.GraphDrawer.GraphLinesHelper();
            //await ldr.Init(start);
            //LineDrawer = ldr;

            EngineNS.Profiler.NativeMemory.BeginProfile();

            mInitialized = true;
        }
        private async System.Threading.Tasks.Task DoUnitTest()
        {
            await new EngineNS.Bricks.HotfixScript.CSharpScriptHelper().DoTest();
            await new EngineNS.Bricks.GraphDrawer.GraphLinesHelper().DoTest();
            await new EditorCommon.Excel.ExcelHelper().DoTest();
            await new EngineNS.Bricks.Particle.McParticleHelper().DoTest();
            await new EngineNS.Bricks.RecastBuilder.RCTileMeshHelper().DoTest();
            //await new SuperSocket.SuperSocketHelper().DoTest();
            await new EngineNS.Bricks.RemoteServices.RemoteServicesHelper().DoTest();
        }
        public void Cleanup()
        {
            MainEdViewport?.Cleanup();
        }

        float t = 0.0f;
        struct LightStruct
        {
            public Vector4 Diffuse;
            public Vector4 Specular;
            public Vector3 Position;
            public float Shiness;
        }
        Vector3 dir = new Vector3(1, 0, 0);
        public void TickMainEdViewport(EditorCommon.ViewPort.ViewPortControl vpc)
        {
            if (!mInitialized)
                return;
            if (EditorCommon.GamePlay.Instance.IsInPIEMode)
                return;
            //if (vpc.Camera.mSceneView == null)
            //    return;

            //if(testDoing==false)
            //{
            //    TestAwait();
            //}

            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

            //Vector3 Eye = new Vector3();
            //Eye.SetValue(0.0f, 1.5f, -15.0f);
            //Vector3 At = new Vector3();
            //At.SetValue(0.0f, 1.0f, 0.0f);
            //Vector3 Up = new Vector3();
            //Up.SetValue(0.0f, 1.0f, 0.0f);
            //vpc.Camera.LookAtLH(Eye, At, Up);

            this.World.CheckVisible(RHICtx, vpc.Camera);
            this.World.Tick();
            vpc.RPolicy?.TickLogic(vpc.Camera.mSceneView, RHICtx);

            if (true)
            {
                t += (float)EngineNS.MathHelper.V_PI * 0.0125f * EngineNS.CEngine.Instance.EngineElapseTime / 1000;

                //TheActor.Placement.Rotation = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitY, t);

                //if (TheBox.Placement.Location.X < 1)
                //    dir.X = -dir.X;
                //if (TheBox.Placement.Location.X > 4)
                //    dir.X = -dir.X;
                //if (TheBox.Placement.Location.Z < 1)
                //    dir.Z = -dir.Z;
                //if (TheBox.Placement.Location.Z > 4)
                //    dir.Z = -dir.Z;




                //TheBox.Placement.Location += dir * 0.08f;
                //lookAt.TargetPosition = TheBox.Placement.Location;
                //var trans = TheActor.Placement.Transform;
                //trans.Inverse();
                //var absLoc = TheBox.Placement.Location;
                //var loc = Vector3.TransformCoordinate(absLoc, trans);
                //ccdIk.TargetPosition = loc;



                // var meshComp = TheActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                //var varIndex = meshComp.mSceneMesh.FindVar(0, "vMeshColor");

                //var vMeshColor = new EngineNS.Color4(1.0f, (float)System.Math.Sin(t), 0.7f, 0.7f);
                //meshComp.mSceneMesh.SetVar(0, varIndex, vMeshColor, 0);

                //var skinModifier = meshComp.mSceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                //if (skinModifier != null)
                //{
                //    skinModifier.SetToRenderStream(meshComp.mSceneMesh);
                //    //var animBlender = skinModifier.Skeleton.AnimBlender;
                //    //if (animBlender != null)
                //    //{
                //    //    animBlender.UpdateNodes(EngineNS.CEngine.Instance.EngineTime);
                //    //    animBlender.BlendActions();
                //    //    //skinModifier.Skeleton.SetTransforms(meshComp.mSceneMesh);
                //    //}
                //}

                //unsafe
                //{
                //    varIndex = meshComp.mSceneMesh.FindVar(0, "PLight");
                //    if (varIndex > 0)
                //    {
                //        LightStruct light = new LightStruct();
                //        light.Shiness = 1;

                //        meshComp.mSceneMesh.SetVarValue(0, varIndex, &light, sizeof(LightStruct), 0);
                //    }
                //}
                if (vpc.Camera.mSceneView != null && LineDrawer != null)
                {
                    var pass = LineDrawer.CreateRenderPass(vpc.Camera.mSceneView);
                    pass.ViewPort = vpc.Camera.mSceneView.Viewport;
                    vpc.Camera.mSceneView.SendPassToCorrectRenderLayer(EngineNS.Graphics.ERenderLayer.RL_Translucent, pass);
                }
            }

            if (DoCaptureMemory)
            {
                var memStat = EngineNS.Profiler.NativeMemory.CurrentProfiler.CaptureMemory();
                DoCaptureMemory = false;
                if (mPrevStat != null)
                {
                    var delta = EngineNS.Profiler.NativeMemory.CurrentProfiler.GetDelta(mPrevStat, memStat);
                    foreach (var i in delta)
                    {
                        System.Diagnostics.Debug.WriteLine($"{i.Key}: Size={i.Value.Size};Count={i.Value.Count}");
                    }
                }
                mPrevStat = memStat;

                System.Diagnostics.Debug.WriteLine($"Total Alloc Count = {EngineNS.Profiler.NativeMemory.CurrentProfiler.TotalCount - TotalAllocCount}");
                TotalAllocCount = EngineNS.Profiler.NativeMemory.CurrentProfiler.TotalCount;
            }

        }

        public bool DoCaptureMemory = false;
        private Dictionary<string, EngineNS.Profiler.NativeProfiler.NMDesc> mPrevStat;
        private Int64 TotalAllocCount = 0;



        #region Test
        public EngineNS.GamePlay.Actor.GActor TheActor;
        public EngineNS.GamePlay.Actor.GActor TheBox;
        public EngineNS.GamePlay.Actor.GActor TheSky;
        public EngineNS.GamePlay.Actor.GActor ActorTest0;
        public EngineNS.GamePlay.Actor.GActor ActorSphere0;
        EngineNS.Graphics.Mesh.SkeletonControl.CGfxLookAt lookAt;

        #region TestGfxMesh
        Vector3[] vertices_pos = new Vector3[]
        {
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),

            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),

            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),

            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),

            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),

            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f)
        };
        Vector2[] vertices_uv = new Vector2[]
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),
        };
        UInt16[] indices = new UInt16[]
        {
            3,1,0,
            2,1,3,

            6,4,5,
            7,4,6,

            11,9,8,
            10,9,11,

            14,12,13,
            15,12,14,

            19,17,16,
            18,17,19,

            22,20,21,
            23,20,22
        };

        void CreateSkeletonAsset(EngineNS.Graphics.Mesh.CGfxMesh mesh)
        {

        }
        private EngineNS.GamePlay.Actor.GActor CreateActorFBX(RName assetName)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var sa = new AssetImport.FbxImporter();

            //var assetName = RName.GetRName("Mesh/AxisWithAni.FBX");
            ImportOption importOption;
            importOption.SkeletonAssetName = null;
            sa.Import(assetName, RName.GetRName("Mesh"), importOption, (uint)(AssetImport.aiPostProcessSteps.aiProcess_DontSplitMeshByMaterial));
            var meshName = RName.GetRName("Mesh/" + sa.MeshPrimitives.First().Key + ".gms");
            var meshPrimitiveName = sa.MeshPrimitives.First().Value.Name;
            var mesh = CEngine.Instance.MeshManager.NewMesh(rc, meshName, meshPrimitiveName);

            mesh.Init(rc, meshName, sa.MeshPrimitives.First().Value);
            var mtl = EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstance(rc, RName.GetRName("Material/defaultmaterial.instmtl"));
            for (int i = 0; i < mesh.MtlMeshArray.Length; ++i)
            {
                mesh.SetMaterial(rc, (uint)i, mtl);
            }

            var actor = EngineNS.GamePlay.Actor.GActor.NewMeshActor(mesh);
            var skinModifier = mesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
            var skeletonAnimationNode = sa.Actions.First().Value;
            //var skeletonAnimationNode = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence();
            //skeletonAnimationNode.Init(sklAction);
            skeletonAnimationNode.AnimationPose = skinModifier.Skeleton.BoneTab;
            var animationCom = new EngineNS.GamePlay.Component.GAnimationInstance();
            animationCom.AnimationNode = skeletonAnimationNode;
            actor.AddComponent(animationCom);

            actor.Placement.Location = new Vector3(-1.0f, 0.0f, 0.0f);
            actor.Placement.Scale /= 10;
            return actor;
        }
        private EngineNS.GamePlay.Actor.GActor CreateMeshActor(EngineNS.GamePlay.GWorld world, RName scene, RName meshName, Vector3 loc, Vector3 scale)
        {
            var mesh = CEngine.Instance.MeshManager.CreateMesh(EngineNS.CEngine.Instance.RenderContext, meshName);
            var actor = EngineNS.GamePlay.Actor.GActor.NewMeshActor(mesh);
            actor.Placement.Location = loc;
            actor.Placement.Scale = scale;
            this.World.AddActor(actor);
            this.World.Scenes[scene].AddActor(actor);
            return actor;
        }
        private async System.Threading.Tasks.Task<EngineNS.GamePlay.Actor.GActor> CreateMeshActorAsync(EngineNS.GamePlay.GWorld world, RName scene, RName meshName, Vector3 loc, Vector3 scale)
        {
            var mesh = await CEngine.Instance.MeshManager.CreateMeshAsync(EngineNS.CEngine.Instance.RenderContext, meshName);
            var actor = EngineNS.GamePlay.Actor.GActor.NewMeshActor(mesh);
            actor.Placement.Location = loc;
            actor.Placement.Scale = scale;
            this.World.AddActor(actor);
            this.World.Scenes[scene].AddActor(actor);
            return actor;
        }

        private async System.Threading.Tasks.Task<EngineNS.GamePlay.Actor.GActor> CreateMeshActorAsync(EngineNS.GamePlay.GWorld world, RName scene, RName meshName, Vector3 loc, Quaternion rotation, Vector3 scale)
        {
            var mesh = await CEngine.Instance.MeshManager.CreateMeshAsync(EngineNS.CEngine.Instance.RenderContext, meshName);
            var actor = EngineNS.GamePlay.Actor.GActor.NewMeshActor(mesh);
            actor.Placement.Location = loc;
            actor.Placement.Rotation = rotation;
            actor.Placement.Scale = scale;
            this.World.AddActor(actor);
            this.World.Scenes[scene].AddActor(actor);
            return actor;
        }

      
        private void CreateLegAndFootIK(EngineNS.GamePlay.Actor.GActor actor)
        {
            var meshComp = actor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            var animationCom = actor.GetComponent<EngineNS.GamePlay.Component.GAnimationInstance>();
            var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
            var FinalPose = skinModifier.AnimationPose;
            var rightLegCcdIk = new EngineNS.Graphics.Mesh.SkeletonControl.CGfxCCDIK();
            rightLegCcdIk.AnimationPose = FinalPose;
            rightLegCcdIk.Alpha = 1.0f;
            rightLegCcdIk.TargetBoneName = EngineNS.Support.LanguageManager.JP932_GB2312("右足ＩＫ");
            //rightLegCcdIk.TargetPosition = targetp;
            rightLegCcdIk.EndEffecterBoneName = EngineNS.Support.LanguageManager.JP932_GB2312("右足首");
            rightLegCcdIk.LimitAngle = 114.5916f;
            rightLegCcdIk.Iteration = 15;
            rightLegCcdIk.Depth = 3;

            var leftLegCcdIk = new EngineNS.Graphics.Mesh.SkeletonControl.CGfxCCDIK();
            leftLegCcdIk.AnimationPose = rightLegCcdIk.AnimationPose;
            leftLegCcdIk.Alpha = 1.0f;
            leftLegCcdIk.TargetBoneName = EngineNS.Support.LanguageManager.JP932_GB2312("左足ＩＫ");
            //leftLegCcdIk.TargetPosition = targetp;
            leftLegCcdIk.EndEffecterBoneName = EngineNS.Support.LanguageManager.JP932_GB2312("左足首");
            leftLegCcdIk.LimitAngle = 114.5916f;
            leftLegCcdIk.Iteration = 15;
            leftLegCcdIk.Depth = 3;

            var rightFootCcdIk = new EngineNS.Graphics.Mesh.SkeletonControl.CGfxCCDIK();
            rightFootCcdIk.AnimationPose = leftLegCcdIk.AnimationPose;
            //rightFootCcdIk.AnimationPose = FinalPose;
            rightFootCcdIk.Alpha = 1.0f;
            rightFootCcdIk.TargetBoneName = EngineNS.Support.LanguageManager.JP932_GB2312("右つま先ＩＫ");
            //rightFootCcdIk.TargetPosition = targetp;
            rightFootCcdIk.EndEffecterBoneName = EngineNS.Support.LanguageManager.JP932_GB2312("右足首D");
            rightFootCcdIk.Iteration = 15;
            rightFootCcdIk.Depth = 1;

            var leftFootCcdIk = new EngineNS.Graphics.Mesh.SkeletonControl.CGfxCCDIK();
            leftFootCcdIk.AnimationPose = rightFootCcdIk.AnimationPose;
            leftFootCcdIk.Alpha = 1.0f;
            leftFootCcdIk.TargetBoneName = EngineNS.Support.LanguageManager.JP932_GB2312("左つま先ＩＫ");
            //leftFootCcdIk.TargetPosition = targetp;
            leftFootCcdIk.EndEffecterBoneName = EngineNS.Support.LanguageManager.JP932_GB2312("左足首D");
            leftFootCcdIk.Iteration = 15;
            leftFootCcdIk.Depth = 1;

            //animationCom.skeletonControls.Add(lookAt);

            animationCom.skeletonControls.Add(rightLegCcdIk);
            animationCom.skeletonControls.Add(leftLegCcdIk);
            //animationCom.skeletonControls.Add(rightFootCcdIk);
            //animationCom.skeletonControls.Add(leftFootCcdIk);
            CreateLookAt(actor, EngineNS.Support.LanguageManager.JP932_GB2312("左足首"), EngineNS.Support.LanguageManager.JP932_GB2312("左つま先ＩＫ"), Vector3.UnitZ);
            CreateLookAt(actor, EngineNS.Support.LanguageManager.JP932_GB2312("右足首"), EngineNS.Support.LanguageManager.JP932_GB2312("右つま先ＩＫ"), Vector3.UnitZ);
        }
        private void CreateLookAt(EngineNS.GamePlay.Actor.GActor actor,string modifyBone,string targetBone , Vector3 loolAtAxis)
        {
            var meshComp = actor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            var animationCom = actor.GetComponent<EngineNS.GamePlay.Component.GAnimationInstance>();
            var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
            var FinalPose = skinModifier.AnimationPose;
            lookAt = new EngineNS.Graphics.Mesh.SkeletonControl.CGfxLookAt();
            lookAt.ModifyBoneName = modifyBone;
            if(targetBone!= null)
             lookAt.TargetBoneName = targetBone;
            lookAt.LookAtAxis = Vector3.UnitX;
            lookAt.AnimationPose = FinalPose;
            lookAt.Alpha = 1;
            animationCom.skeletonControls.Add(lookAt);
        }


        private List<RName> CreateFromFBX(RName name)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var sa = new AssetImport.FbxImporter();

            var assetName = name;
            var meshesList = new List<RName>();
            ImportOption importOption;
            importOption.SkeletonAssetName = null;
            sa.Import(assetName, RName.GetRName("Mesh"), importOption, (uint)(AssetImport.aiPostProcessSteps.aiProcess_DontSplitMeshByMaterial|AssetImport.aiPostProcessSteps.aiProcess_GenNormals));
            foreach (var meshPri in sa.MeshPrimitives)
            {
                var meshName = RName.GetRName("Mesh/" + meshPri.Key + CEngineDesc.MeshExtension);
                var meshPrimitiveName = meshPri.Value.Name;
                // 用createMesh 不行
                //var mesh = CEngine.Instance.MeshManager.CreateMesh(rc, meshName, CEngine.Instance.ShadingEnvManager.DefaultShadingEnv);
                var mesh = CEngine.Instance.MeshManager.NewMesh(rc, meshName, meshPrimitiveName);

                mesh.Init(rc, meshName, meshPri.Value);
                var mtl = EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstance(rc, RName.GetRName("Material/defaultmaterial.instmtl"));
                for (int i = 0; i < mesh.MtlMeshArray.Length; ++i)
                {
                    mesh.SetMaterial(rc, (uint)i, mtl);
                }

                mesh.SaveMesh();
                meshesList.Add(meshName);
            }
            return meshesList;
        }

        private async System.Threading.Tasks.Task<EngineNS.GamePlay.Actor.GActor> CreateMeshActorAsync(EngineNS.GamePlay.GWorld world, RName scene, List<RName> meshesName, Vector3 loc, Vector3 scale)
        {
            var actor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(meshesName);
            actor.Placement.Location = loc;
            actor.Placement.Scale = scale;
            this.World.AddActor(actor);
            this.World.Scenes[scene].AddActor(actor);
            return actor;
        }

        private EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence CreateAnimation(EngineNS.GamePlay.Actor.GActor actor, RName animationName)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var animationCom = new EngineNS.GamePlay.Component.GAnimationInstance(RName.GetRName("delisha.skt"));
            actor.AddComponent(animationCom);
            var animationSege = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence();
            animationSege.Init(EngineNS.CEngine.Instance.RenderContext, animationName);//RName.GetRName("Mesh/fouraxisanim" + CEngineDesc.AnimationSegementExtension)

            var meshComp = actor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            //var pose = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(rc, RName.GetRName("delisha.skt")).BoneTab.Clone();
            var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
            skinModifier.AnimationPose = animationCom.AnimationPose;
            animationSege.AnimationPose = animationCom.AnimationPose;
            animationCom.AnimationNode = animationSege;
            return animationSege;
        }
        private EngineNS.Graphics.Mesh.Animation.CGfxAnimationBlendSpace1D CreateAnimationBlenSpace1D(EngineNS.GamePlay.Actor.GActor actor)
        {
            RName first = RName.GetRName("Mesh/Crouch_Aim_LD45" + CEngineDesc.AnimationSequenceExtension);
            RName second = RName.GetRName("Mesh/Crouch_Aim_RD45" + CEngineDesc.AnimationSequenceExtension);
            RName third = RName.GetRName("Mesh/Crouch_Aim_Center" + CEngineDesc.AnimationSequenceExtension);
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var animationCom = new EngineNS.GamePlay.Component.GAnimationInstance(RName.GetRName("robot2.skt"));
            actor.AddComponent(animationCom);
            var firstAnim = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence();
            firstAnim.Init(EngineNS.CEngine.Instance.RenderContext, first);
            var secondAnim = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence();
            secondAnim.Init(EngineNS.CEngine.Instance.RenderContext, second);
            var thirdAnim = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence();
            thirdAnim.Init(EngineNS.CEngine.Instance.RenderContext, third);

            var blendSpace = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationBlendSpace1D();
            blendSpace.AnimationPose = animationCom.AnimationPose;

            var meshComp = actor.GetComponent<EngineNS.GamePlay.Component.GMutiMeshComponent>();
            foreach (var subMesh in meshComp.Meshes)
            {
                var skinModifier = subMesh.Value.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                skinModifier.AnimationPose = animationCom.AnimationPose;
            }
            blendSpace.AddSample(firstAnim, Vector3.Zero);
            blendSpace.AddSample(secondAnim, new Vector3(100,0,0));
            blendSpace.AddSample(thirdAnim, new Vector3(50, 0, 0));

            animationCom.AnimationNode = blendSpace;
            return blendSpace;
        }
        private EngineNS.Graphics.Mesh.Animation.CGfxAnimationBlendSpace CreateAnimationBlenSpace2D(EngineNS.GamePlay.Actor.GActor actor)
        {
            RName first = RName.GetRName("Mesh/Crouch_Aim_LD45" + CEngineDesc.AnimationSequenceExtension);
            RName second = RName.GetRName("Mesh/Crouch_Aim_RD45" + CEngineDesc.AnimationSequenceExtension);
            RName third = RName.GetRName("Mesh/Crouch_Aim_Center" + CEngineDesc.AnimationSequenceExtension);
            RName forth = RName.GetRName("Mesh/walk" + CEngineDesc.AnimationSequenceExtension);
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var animationCom = new EngineNS.GamePlay.Component.GAnimationInstance(RName.GetRName("robot2.skt"));
            actor.AddComponent(animationCom);
            var firstAnim = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence();
            firstAnim.Init(EngineNS.CEngine.Instance.RenderContext, first);
            var secondAnim = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence();
            secondAnim.Init(EngineNS.CEngine.Instance.RenderContext, second);
            var thirdAnim = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence();
            thirdAnim.Init(EngineNS.CEngine.Instance.RenderContext, third);
            var forthAnim = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence();
            forthAnim.Init(EngineNS.CEngine.Instance.RenderContext, forth);


            var blendSpace = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationBlendSpace();
            blendSpace.AnimationPose = animationCom.AnimationPose;

            var meshComp = actor.GetComponent<EngineNS.GamePlay.Component.GMutiMeshComponent>();
            foreach (var subMesh in meshComp.Meshes)
            {
                var skinModifier = subMesh.Value.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                skinModifier.AnimationPose = animationCom.AnimationPose;
            }
            blendSpace.AddSample(firstAnim, Vector3.Zero);
            blendSpace.AddSample(secondAnim, new Vector3(100, 0, 0));
            blendSpace.AddSample(thirdAnim, new Vector3(0, 100, 0));
            blendSpace.AddSample(forthAnim, new Vector3(100, 100, 0));
            animationCom.AnimationNode = blendSpace;
            return blendSpace;
        }
        private EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence CreateMutiMeshAnimation(EngineNS.GamePlay.Actor.GActor actor, RName animationName)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var animationCom = new EngineNS.GamePlay.Component.GAnimationInstance(RName.GetRName("robot2.skt"));
            actor.AddComponent(animationCom);
            var animationSege = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence();
            animationSege.Init(EngineNS.CEngine.Instance.RenderContext, animationName);//RName.GetRName("Mesh/fouraxisanim" + CEngineDesc.AnimationSegementExtension)

            var meshComp = actor.GetComponent<EngineNS.GamePlay.Component.GMutiMeshComponent>();
            foreach (var subMesh in meshComp.Meshes)
            {
                var skinModifier = subMesh.Value.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                skinModifier.AnimationPose = animationCom.AnimationPose;
            }
            animationSege.AnimationPose = animationCom.AnimationPose;
            animationCom.AnimationNode = animationSege;
            return animationSege;
        }

        public void CreateAnimationInstanceMacross(EngineNS.GamePlay.Actor.GActor actor)
        {
            EngineNS.Macross.MacrossGetter<EngineNS.GamePlay.Component.GAnimationInstance> macrossGetter = new EngineNS.Macross.MacrossGetter<EngineNS.GamePlay.Component.GAnimationInstance>();
            macrossGetter.Name = RName.GetRName("Animation/amc_robot.macross");
            var animationCom =  macrossGetter.Get();
            animationCom.Init();
            actor.AddComponent(animationCom);
            var meshComp = actor.GetComponent<EngineNS.GamePlay.Component.GMutiMeshComponent>();
            foreach (var subMesh in meshComp.Meshes)
            {
                var skinModifier = subMesh.Value.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                if(animationCom.AnimationPose!=null)
                    skinModifier.AnimationPose = animationCom.AnimationPose;
            }
        }

        Vector3 targetp = new Vector3(100.5f, 200, 200.4f);
        void VisitSkeleton(EngineNS.Graphics.Mesh.Skeleton.CGfxBoneTable tab, EngineNS.Graphics.Mesh.Skeleton.CGfxBone bone)
        {
            System.Diagnostics.Debug.WriteLine($"{bone.BoneDesc.Name} -> {bone.BoneDesc.Parent}");
            for (UInt32 i = 0; i < bone.ChildNumber; i++)
            {
                var index = bone.GetChild(i);
                var cbone = tab.GetBone(index);
                VisitSkeleton(tab, cbone);
            }
        }
        #endregion

        #endregion
    }
}
