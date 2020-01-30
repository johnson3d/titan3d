using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Particle
{
    partial class GParticleSubSystemComponent
    {
        bool mVisible = true;
        public override bool Visible
        {
            get => mVisible;
            set
            {
                base.Visible = value;
                mVisible = value;
                if (SceneMesh != null)
                {
                    for (int i = 0; i < SceneMesh.MtlMeshArray.Length; i ++)
                    {
                        SceneMesh.MtlMeshArray[i].Visible = mVisible;
                    }
                }
               
            }
        }

        public bool IsCheckForVisible
        {
            get;
            set;
        } = true;

        partial void AddHelpMeshComponent(CRenderContext rc, EngineNS.GamePlay.Actor.GActor actor, EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            if (sys.SubStates == null)
                return;

            for (int i = 0; i < sys.SubStates.Length; i++)
            {
                if (sys.SubStates[i].Shape.UseMeshName != null)
                {
                    //sys.SubStates[i]
                    var test = CreateMeshComponent(rc, actor, sys.SubStates[i].Shape);
                }
            }
        }
        
        private async Task CreateMeshComponent(CRenderContext rc, EngineNS.GamePlay.Actor.GActor actor, EmitShape.CGfxParticleEmitterShape shape)
        {
            GMeshComponentInitializer initializer = new GMeshComponentInitializer();
            initializer.MeshName = shape.UseMeshName;

            GMeshComponent component = new GMeshComponent();
            await component.SetInitializer(rc, actor, this, initializer);
            component.SpecialName = shape.Name;
            this.AddComponent(component);

            component.Host = Host;

            shape.BuildMatrix(ref component.Placement.Transform);

            if (component.SceneMesh != null)
            {
                component.SceneMesh.SetPassUserFlags(1);

                //if (component.SceneMesh.MtlMeshArray != null)
                //{
                //    foreach (var mtl in component.SceneMesh.MtlMeshArray)
                //    {
                //        mtl.Visible = false;
                //    }
                //}

                component.Visible = false;

            }
        }
        
    }
}
