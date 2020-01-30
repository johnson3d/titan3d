using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;

namespace EngineNS.GamePlay.Component
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GDirLightComponentInitializer), "平行光组件", "Light", "DirLightComponent")]
    [Editor.Editor_PlantAbleActor("Light", "GDirLightComponent")]
    [Editor.Editor_ComponentClassIconAttribute("icon/directionallight_64x.txpic", RName.enRNameType.Editor)]
    public class GDirLightComponent : GComponent, EngineNS.Editor.IPlantable
    {
        [Rtti.MetaClassAttribute]
        public class GDirLightComponentInitializer : GComponentInitializer
        {
            public GDirLightComponentInitializer()
            {
                DirLightColor = Color.White;
                DirLightIntensity = 4.0f;
                ShadowAlpha = 0.15f;
            }
            [Rtti.MetaData]
            [EngineNS.Editor.Editor_ColorPicker]
            public Color DirLightColor
            {
                get;
                set;
            } = Color.White;
            [Rtti.MetaData]
            [Editor.Editor_ValueWithRange(0.0f, 20.0f)]
            public float DirLightIntensity
            {
                get;
                set;
            }

            [Rtti.MetaData]
            [Editor.Editor_ValueWithRange(0.0f, 0.5f)]
            public float ShadowAlpha
            {
                get;
                set ;
            }


            [Rtti.MetaData]
            [EngineNS.Editor.Editor_ColorPicker]
            public Color SkyLightColor
            {
                get;
                set;
            } = Color.White;
            
            [Rtti.MetaData]
            [EngineNS.Editor.Editor_ColorPicker]
            public Color GroundLightColor
            {
                get;
                set;
            } = Color.DarkGray;

        }
        public GDirLightComponentInitializer DirLightInitializer
        {
            get;
            set;
        }
        public GDirLightComponent()
        {
            OnlyForGame = false;
        }
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            
            DirLightInitializer = v as GDirLightComponentInitializer;
            return true;
        }
        public Graphics.View.CGfxSceneView View;
        public override void Tick(GPlacementComponent placement)
        {
            base.Tick(placement);

            if (View != null)
            {
                View.DirLightColor_Intensity = new Vector4(DirLightInitializer.DirLightColor.R / 255.0f, DirLightInitializer.DirLightColor.G / 255.0f, DirLightInitializer.DirLightColor.B / 255.0f, 
                    DirLightInitializer.DirLightIntensity);
                //var matrix = Matrix.RotationY(CEngine.Instance.EngineTime/5000.0f);
                Vector3 LightDir = Vector3.TransformNormal(Vector3.UnitZ, Matrix.RotationQuaternion(placement.Rotation));
                View.mDirLightDirection_Leak = new Vector4(LightDir, DirLightInitializer.ShadowAlpha);
                View.mSkyLightColor = new Vector3(DirLightInitializer.SkyLightColor.R / 255.0f, DirLightInitializer.SkyLightColor.G / 255.0f, DirLightInitializer.SkyLightColor.B / 255.0f); 
                View.GroundLightColor = new Vector3(DirLightInitializer.GroundLightColor.R / 255.0f, DirLightInitializer.GroundLightColor.G / 255.0f, DirLightInitializer.GroundLightColor.B / 255.0f);
            }
        }

        public async Task<GActor> CreateActor(Editor.PlantableItemCreateActorParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            placement.Location = param.Location;
            placement.Rotation = EngineNS.Quaternion.GetQuaternion(Vector3.UnitZ, -Vector3.UnitY);
            actor.SpecialName = "SunActor";

            var initializer = new GDirLightComponentInitializer();
            initializer.SpecialName = "LightData";
            await SetInitializer(rc, actor, actor, initializer);
            this.View = param.View;

            var meshComp = new EngineNS.GamePlay.Component.GMeshComponent();
            var meshCompInit = new EngineNS.GamePlay.Component.GMeshComponent.GMeshComponentInitializer();
            meshCompInit.SpecialName = "VisualMesh";
            meshCompInit.MeshName = EngineNS.RName.GetRName("editor/sun.gms", EngineNS.RName.enRNameType.Game);
            var test = meshComp.SetInitializer(rc, actor, actor, meshCompInit);
            actor.AddComponent(meshComp);

            actor.AddComponent(this);
            return actor;
        }

        public override void OnAddedScene()
        {
            if(!Host.Scene.IsLoading)
            {
                base.OnAddedScene();
                Host.Scene.SunActor = this.Host;
            }
        }
    }
}
