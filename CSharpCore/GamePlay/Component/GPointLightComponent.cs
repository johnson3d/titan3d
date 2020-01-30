using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.SceneGraph;
using EngineNS.Graphics;

namespace EngineNS.GamePlay.Component
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GPointLightComponentInitializer), "点光源组件", "Light", "PointLightComponent")]
    [Editor.Editor_PlantAbleActor("Light", "GPointLightComponent")]
    public partial class GPointLightComponent : GComponent, Editor.IPlantable, IPlaceable
    {
        [Rtti.MetaClassAttribute]
        public class GPointLightComponentInitializer : GComponentInitializer
        {
            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
            public struct PointLightDesc
            {
                public Vector3 Position;
                public float InvRadius;
                public Vector3 LightColor;
                public float Intensity;
            }
            internal GPointLightComponent Host;
            internal PointLightDesc LightDesc;
            internal float DistSq;
            public GPointLightComponentInitializer()
            {
                LightColor = new Vector3(1,1,1);
                Intensity = 30.0f;
                Radius = 15.0f;
            }
            public Vector3 Position
            {
                get
                {
                    if(LightDesc.Position==Vector3.Zero && Host!=null)
                    {
                        LightDesc.Position = Host.Host.Placement.WorldLocation;
                    }
                    return LightDesc.Position;
                }
                set
                {
                    LightDesc.Position = value;
                }
            }
            [Rtti.MetaData]
            [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public float Radius
            {
                get
                {
                    return 1.0f / LightDesc.InvRadius;
                }
                set
                {
                    LightDesc.InvRadius = 1.0f / value;
                    DistSq = value * value;

                    if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                    {
                        if (Host != null && CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                        {
                            var showMesh = Host.Host.FindComponentBySpecialName("SphereVolumeShow") as GMeshComponent;
                            if (showMesh != null)
                            {
                                showMesh.Placement.Scale = new Vector3(value, value, value);
                            }
                        }
                    }   
                }
            }
            [Rtti.MetaData]
            [EngineNS.Editor.Editor_Color4Picker]
            [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public Color4 LightColor
            {
                get
                {
                    return new Color4(LightDesc.LightColor.X,
                            LightDesc.LightColor.Y,
                            LightDesc.LightColor.Z);
                }
                set
                {
                    LightDesc.LightColor.X = value.Red;
                    LightDesc.LightColor.Y = value.Green;
                    LightDesc.LightColor.Z = value.Blue;

                    if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                    {
                        if (Host != null && Host.Host != null)
                        {
                            var showMesh = Host.Host.FindComponentBySpecialName("EditorShow") as GMeshComponent;
                            if (showMesh != null)
                            {
                                var index = showMesh.SceneMesh.McFindVar(0, "LightColor");
                                showMesh.SceneMesh.McSetVarColor4(0, index, LightDesc.LightColor, 0);
                            }
                            showMesh = Host.Host.FindComponentBySpecialName("SphereVolumeShow") as GMeshComponent;
                            if (showMesh != null)
                            {
                                var index = showMesh.SceneMesh.McFindVar(0, "LightColor");
                                showMesh.SceneMesh.McSetVarColor4(0, index, LightDesc.LightColor, 0);
                            }
                        }
                    }
                }
            }
            [Rtti.MetaData]
            [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public float Intensity
            {
                get
                {
                    return LightDesc.Intensity;
                }
                set
                {
                    LightDesc.Intensity = value;
                }
            }

            [Rtti.MetaData]
            public GPlacementComponent.GPlacementComponentInitializer PlacementComponentInitializer { get; set; } = new GPlacementComponent.GPlacementComponentInitializer();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public GPointLightComponentInitializer PointLightInitializer
        {
            get
            {
                return this.Initializer as GPointLightComponentInitializer;
            }
        }
        public float Radius
        {
            get
            {
                return PointLightInitializer.Radius;
            }
            set
            {
                PointLightInitializer.Radius = value;
            }
        }
        [EngineNS.Editor.Editor_Color4Picker]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Color4 LightColor
        {
            get
            {
               return PointLightInitializer.LightColor;

            }
            set
            {
                PointLightInitializer.LightColor = value;
            }
        }
        [Rtti.MetaData]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Intensity
        {
            get
            {
                return PointLightInitializer.Intensity;
            }
            set
            {
                PointLightInitializer.Intensity = value;
            }
        }
        public GPointLightComponent()
        {
            OnlyForGame = false;
        }
        public override bool IsVisualComponent
        {
            get => true;
        }
        public int IndexInScene = -1;
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            PointLightInitializer.Host = this;
            PointLightInitializer.Radius = PointLightInitializer.Radius;
            if (PointLightInitializer.PlacementComponentInitializer != null)
            {
                await Placement.SetInitializer(rc, host, null, PointLightInitializer.PlacementComponentInitializer);
                Placement.PlaceableHost = this;
            }
            PointLightInitializer.Position = Placement.WorldLocation;
            return true;
        }
        GPlacementComponent mPlacement = new GPlacementComponent();
        public GPlacementComponent Placement 
        {
            get { return mPlacement; }
            set { mPlacement = value; }
        }
        public override void OnAdded()
        {
            base.OnAdded();
            Host?.Scene?.RegPointLight(this);
        }
        public override void OnRemove()
        {
            base.OnRemove();
            Host?.Scene?.UnRegPointLight(this);
        }
        public override void OnAddedScene()
        {
            base.OnAddedScene();
            Host?.Scene?.RegPointLight(this);
        }
        public override void OnRemoveScene()
        {
            base.OnRemoveScene();
            Host?.Scene?.UnRegPointLight(this);
        }
        BoundingSphere LightSphere = new BoundingSphere();
        public void OnPlacementChanged(GPlacementComponent placement)
        {
            if (this.Host.StaticObject)
            {
                this.Host.Scene?.World?.FindNearActor(PointLightInitializer.Position, LightSphere.Radius, (Actor.GActor actor) =>
                {
                    actor.PointLightsSerialId = 0;
                    return false;
                });
            }

            PointLightInitializer.Position = Placement.WorldLocation;
            LightSphere.Center = Placement.WorldLocation;
            LightSphere.Radius = PointLightInitializer.Radius;

            if (this.Host.StaticObject)
            {
                this.Host.Scene?.World?.FindNearActor(PointLightInitializer.Position, LightSphere.Radius, (Actor.GActor actor) =>
                {
                    actor.PointLightsSerialId = 0;
                    return false;
                });
            }
        }
        //不作用于物理
        public void OnPlacementChangedUninfluencePhysics(GPlacementComponent placement)
        {
            PointLightInitializer.Position = placement.Location;
        }
        public override void CommitVisual(CCommandList cmd, CGfxCamera camera, CheckVisibleParam param)
        {
            if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
            {
                var showMesh = Host.FindComponentBySpecialName("SphereVolumeShow") as GMeshComponent;
                if (showMesh != null)
                {
                    showMesh.Visible = CEngine.ShowLightAssist;
                }
            }
            base.CommitVisual(cmd, camera, param);
        }
        protected override void OnAddedComponent(GComponent addedComponent)
        {
            base.OnAddedComponent(addedComponent);

            if(CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                this.LightColor = this.LightColor;
        }
        partial void TickEditor();
        public override void Tick(GPlacementComponent placement)
        {
            base.Tick(placement);
            TickEditor();
        }
        public float GetPointLightEffectFactor(Actor.GActor actor)
        {
            float dist;
            if (BoundingBox.Intersects(ref actor.Placement.ActorAABB, ref LightSphere, out dist) == false)
                return -1;
            return dist;
        }
        public async Task<Actor.GActor> CreateActor(Editor.PlantableItemCreateActorParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            placement.Location = param.Location;
            placement.Rotation = EngineNS.Quaternion.GetQuaternion(Vector3.UnitZ, -Vector3.UnitY);
            
            var meshComp = new EngineNS.GamePlay.Component.GMeshComponent();
            var meshCompInit = new EngineNS.GamePlay.Component.GMeshComponent.GMeshComponentInitializer();
            meshCompInit.SpecialName = "EditorShow";
            meshCompInit.MeshName = EngineNS.RName.GetRName("meshes/pointlight.gms", EngineNS.RName.enRNameType.Editor);
            await meshComp.SetInitializer(rc, actor, actor, meshCompInit);
            meshComp.HideInGame = true;
            actor.AddComponent(meshComp);

            meshComp = new EngineNS.GamePlay.Component.GMeshComponent();
            meshCompInit = new EngineNS.GamePlay.Component.GMeshComponent.GMeshComponentInitializer();
            meshCompInit.SpecialName = "SphereVolumeShow";
            meshCompInit.MeshName = EngineNS.RName.GetRName("meshes/sphere_wireframe.gms", EngineNS.RName.enRNameType.Editor);
            await meshComp.SetInitializer(rc, actor, actor, meshCompInit);
            meshComp.HideInGame = true;
            actor.AddComponent(meshComp);


            var initializer = new GPointLightComponentInitializer();
            initializer.SpecialName = "PointLightComponent";
            await SetInitializer(rc, actor, actor, initializer);
            actor.AddComponent(this);
            actor.AcceptLights = false;

            return actor;
        }
    }
}

namespace EngineNS.GamePlay.SceneGraph
{

    //这里最后要移动到World中管理
    public partial class GSceneGraph
    {
        public List<GamePlay.Component.GPointLightComponent> PointLights = new List<Component.GPointLightComponent>();
        public List<GamePlay.Component.GPointLightComponent> DynPointLights = new List<Component.GPointLightComponent>();
        private void BuildDynPointLights()
        {
            DynPointLights.Clear();
            for (int i = 0; i < PointLights.Count; i++)
            {
                if (PointLights[i] == null)
                    continue;
                if (PointLights[i].Host.StaticObject == false)
                {
                    DynPointLights.Add(PointLights[i]);
                }
            }
        }
        internal UInt32 PointLightsSerialId = 1;
        public int RegPointLight(GamePlay.Component.GPointLightComponent light)
        {
            if (light.IndexInScene != -1)
                return light.IndexInScene;
            lock (PointLights)
            {
                PointLightsSerialId++;
                for (int i = 0; i < PointLights.Count; i++)
                {
                    if (PointLights[i] == null)
                    {
                        light.IndexInScene = i;
                        PointLights[i] = light;
                        return i;
                    }
                }
                light.IndexInScene = PointLights.Count;
                PointLights.Add(light);
                BuildDynPointLights();
                return light.IndexInScene;
            }
        }
        public void UnRegPointLight(GamePlay.Component.GPointLightComponent light)
        {
            if (light.IndexInScene == -1)
                return;
            lock (PointLights)
            {
                PointLights[light.IndexInScene] = null;
                light.IndexInScene = -1;
            }
            PointLightsSerialId++;
            BuildDynPointLights();
        }

        public struct AffectLight
        {
            public GamePlay.Component.GPointLightComponent Light;
            public float Factor;
        }
        private void TryAddLight(List<AffectLight> outLights, GamePlay.Component.GPointLightComponent light, float factor)
        {
            if(outLights.Count<4)
            {
                var al = new AffectLight();
                al.Light = light;
                al.Factor = factor;
                outLights.Add(al);
                return;
            }
            for(int i=0; i<outLights.Count; i++)
            {
                if (outLights[i].Factor > factor)
                {
                    var al = new AffectLight();
                    al.Light = light;
                    al.Factor = factor;

                    outLights[i] = al;
                    return;
                }
            }
        }
        //public static Profiler.TimeScope ScopeAffectLightsStatic = Profiler.TimeScopeManager.GetTimeScope(typeof(GSceneGraph), "GetAffectLights.Static", Profiler.TimeScope.EProfileFlag.Windows);
        //public static Profiler.TimeScope ScopeAffectLightsDynamic = Profiler.TimeScopeManager.GetTimeScope(typeof(GSceneGraph), "GetAffectLights.Dynamic", Profiler.TimeScope.EProfileFlag.Windows);
        //public static Profiler.TimeScope ScopeAffectLightsSetCBuffer = Profiler.TimeScopeManager.GetTimeScope(typeof(GSceneGraph), "GetAffectLights.CBuffer", Profiler.TimeScope.EProfileFlag.Windows);
        public void GetAffectLights(Graphics.View.CGfxSceneView view, Actor.GActor actor, bool bSet2CBuffer)
        {
            if (view == null)
                return;
            //ScopeAffectLightsStatic.Begin();
            var staticlights = actor.StaticLights;
            if (actor.StaticObject)
            {
                if (actor.PointLightsSerialId != actor.Scene.PointLightsSerialId)
                {
                    staticlights.Clear();
                    for (int i = 0; i < PointLights.Count; i++)
                    {
                        if (PointLights[i] == null)
                            continue;
                        if (PointLights[i].Host.StaticObject == false)
                            continue;

                        float factor = PointLights[i].GetPointLightEffectFactor(actor);
                        if (factor < 0)
                            continue;
                        TryAddLight(staticlights, PointLights[i], factor);
                    }
                    actor.PointLightsSerialId = actor.Scene.PointLightsSerialId;
                }
            }
            else
            {
                staticlights.Clear();
            }
            var effectLights = actor.AffectLights;
            effectLights.Clear();
            if (staticlights.Count>0)
            {
                //effectLights.AddRange(staticlights);//不能这样操作，查看List源代码，这里会有一次new T[]的操作，导致GC，Add因为Capacity足够反而不会
                for (int i=0; i < staticlights.Count; i++)
                {
                    effectLights.Add(staticlights[i]);
                }
            }
            //ScopeAffectLightsStatic.End();

            if (actor.StaticObject)
            {
                //ScopeAffectLightsStatic.Begin();
                for (int i = 0; i < DynPointLights.Count; i++)
                {
                    if (DynPointLights[i] == null)
                        continue;
                    float factor = DynPointLights[i].GetPointLightEffectFactor(actor);
                    if (factor < 0)
                        continue;
                    TryAddLight(effectLights, DynPointLights[i], factor);
                }
                //ScopeAffectLightsStatic.End();
            }
            else
            {
                //ScopeAffectLightsDynamic.Begin();
                for (int i = 0; i < PointLights.Count; i++)
                {
                    if (PointLights[i] == null)
                        continue;

                    float factor = PointLights[i].GetPointLightEffectFactor(actor);
                    if (factor < 0)
                        continue;
                    TryAddLight(effectLights, PointLights[i], factor);
                }
                //ScopeAffectLightsDynamic.End();
            }

            if (bSet2CBuffer)
            {
                //ScopeAffectLightsSetCBuffer.Begin();
                SetMeshAffectLights(view, actor);
                //ScopeAffectLightsSetCBuffer.End();
            }
        }
        static Component.GComponentsContainer.FOnVisitComponent mOnVisitComponent_SetPointLights = OnVisitComponent_SetPointLightsOnVisitComponent;
        static void OnVisitComponent_SetPointLightsOnVisitComponent(Component.GComponent comp, object arg)
        {
            var meshComp = comp as Component.GMeshComponent;
            if (meshComp != null && meshComp.SceneMesh != null)
            {
                var actor = meshComp.Host;
                meshComp.SceneMesh.mMeshVars.SetPointLights(actor.AffectLights);
                return;
            }
            var skinMeshComp = comp as Component.GSubSkinComponent;
            if (skinMeshComp != null && skinMeshComp.SceneMesh != null)
            {
                var actor = skinMeshComp.Host;
                skinMeshComp.SceneMesh.mMeshVars.SetPointLights(actor.AffectLights);
                return;
            }
        }
        public void SetMeshAffectLights(Graphics.View.CGfxSceneView view, Actor.GActor actor)
        {
            actor.VisitChildComponents(mOnVisitComponent_SetPointLights, view);
        }
    }
}

namespace EngineNS.Graphics.View
{
    partial class CGfxSceneView
    {
        public void UpdatePointLights(GamePlay.SceneGraph.GSceneGraph ViewScene)
        {
            if (ViewScene == null)
                return;
            for (int i = 0; i < ViewScene.PointLights.Count; i++)
            {
                if (ViewScene.PointLights[i] == null)
                    continue;
                this.SetPointLight((UInt32)i, ref ViewScene.PointLights[i].PointLightInitializer.LightDesc);
            }
        }
    }
}

namespace EngineNS
{
    partial class CEngine
    {
        public static bool ShowLightAssist = false;
    }
}

