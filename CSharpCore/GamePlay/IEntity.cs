using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public interface IEntity
    {
        Actor.GCenterData CenterData
        {
            get;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        Vector3 Position
        {
            get;
        }
        Component.GPlacementComponent Placement
        {
            get;
        }
        GComp GetComponent<GComp>() where GComp : Component.GComponent;
    }
}
