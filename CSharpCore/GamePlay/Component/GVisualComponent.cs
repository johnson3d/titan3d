using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GamePlay.Component.GComponent.GComponentInitializer), "可视化组件","Mesh", "VisualComponent")]
    [Editor.Editor_MacrossClassAttribute(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class GVisualComponent : GComponentsContainer
    {
        public virtual bool Visible { get; set; } = true;
        public GVisualComponent()
        {
            OnlyForGame = false;
        }
        public override bool IsVisualComponent => true;
    }
}
