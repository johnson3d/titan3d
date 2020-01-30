using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.GamePlay.Actor;

namespace EngineNS.Bricks.Animation.Notify
{
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Rtti.MetaClass]
    public class GGfxBoxNotify : CGfxNotify
    {
        GamePlay.Component.GMeshComponent mBoxComponent;
        BoundingBox mBoxShape = new BoundingBox(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Rtti.MetaData]
        public BoundingBox BoxShape
        {
            get => mBoxShape;
            set
            {
                mBoxShape = value;
                if (mBoxComponent != null)
                {
                    mBoxComponent.Placement.Scale = BoxShape.GetSize();
                    mBoxComponent.Placement.Location = BoxShape.GetCenter();
                }
            }
        }
        private async System.Threading.Tasks.Task<GamePlay.Component.GMeshComponent> GetBoxComponent(AnimationClip anim, GActor actorEd)
        {
            mBoxComponent = actorEd.FindComponentBySpecialName("Notify_BoxShape") as GamePlay.Component.GMeshComponent;
            if (mBoxComponent == null)
            {
                mBoxComponent = new GamePlay.Component.GMeshComponent();
                var init = new GamePlay.Component.GMeshComponent.GMeshComponentInitializer();
                init.DontSave = true;
                init.MeshName = RName.GetRName("editor/basemesh/box_center.gms");
                await mBoxComponent.SetInitializer(CEngine.Instance.RenderContext, actorEd, null, init);
                mBoxComponent.SpecialName = "Notify_BoxShape";
                actorEd.AddComponent(mBoxComponent);
            }
            
            mBoxComponent.Placement.Scale = BoxShape.GetSize();
            mBoxComponent.Placement.Location = BoxShape.GetCenter();
            return mBoxComponent;
        }
        public override void OnEditor_Selected(AnimationClip anim, GActor actorEd)
        {
            if(mBoxComponent==null)
            {
                var noused = GetBoxComponent(anim, actorEd);
                return;
            }
            mBoxComponent.Visible = true;
            mBoxComponent.DontSave = true;
        }
        public override void OnEditor_UnSelected(AnimationClip anim, GActor actorEd)
        {
            if (mBoxComponent != null)
            {
                mBoxComponent.Visible = false;
            }
        }
    }
}
