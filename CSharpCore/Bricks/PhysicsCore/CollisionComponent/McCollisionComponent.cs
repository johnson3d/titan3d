using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore.CollisionComponent
{
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.MacrossGetter)]
    [Editor.Editor_MacrossClassIconAttribute("icon/mctrigger_64.txpic", RName.enRNameType.Editor)]
    public class McCollisionComponent : GamePlay.Component.McComponent
    {
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnTriggerIn(GComponent overlapedComponent,GamePlay.Actor.GActor  otherActor, GComponent otherComponent)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnTriggerOut(GComponent overlapedComponent, GamePlay.Actor.GActor otherActor, GComponent otherComponent)
        {

        }
    }
}
