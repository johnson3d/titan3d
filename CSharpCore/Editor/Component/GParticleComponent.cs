using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    partial class GParticleComponent
    {
        partial void TickEditor()
        {
            if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
            {
                var editor = CEngine.Instance.GameEditorInstance as EngineNS.Editor.CEditorInstance;
                if (editor != null)
                {
                    var showMesh = Host.FindComponentBySpecialName("EditorShow") as EngineNS.GamePlay.Component.GMeshComponent;
                    if (showMesh != null)
                    {
                        showMesh.Placement.FaceToCamera(editor.GetMainViewCamera());
                    }
                }
            }
        }

        public async System.Threading.Tasks.Task CreateBoundBox(CRenderContext rc, EngineNS.GamePlay.Actor.GActor actor, EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            this.RemoveComponent("BoxVolumeData");
            if (sys.IsShowBox)
            {
                var boxcom = new LooseOctree.BoxComponent();
                boxcom.Size = new Vector3(sys.AABB.L, sys.AABB.H, sys.AABB.W);
                boxcom.Center = sys.AABB.Center;
                var init = new LooseOctree.BoxComponent.BoxComponentInitializer();
                init.SpecialName = "BoxVolumeData";
                await boxcom.SetInitializer(rc, actor, actor, init);

                boxcom.LineMeshComponent.Host = actor;

                this.AddComponent(boxcom);
            }
        }

        public async System.Threading.Tasks.Task ResetDebugBox(CRenderContext rc, EngineNS.GamePlay.Actor.GActor actor, EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            BoundingBox.Merge(ref Host.LocalBoundingBox, ref sys.AABB.Box, out Host.LocalBoundingBox);
            OnUpdateDrawMatrix(ref Host.Placement.mDrawTransform);

            await CreateBoundBox(rc, Host, sys);
        }
    }
}
