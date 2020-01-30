using CodeDomNode;
using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleEditor
{
    public class ShowItemNode_ParticleSystemControl : EngineNS.GamePlay.Component.IPlaceable
    {
        [EngineNS.Editor.Editor_ShowOnlyInnerProperties]
        public Object CreateObject
        {
            get;
            set;
        }

        public GPlacementComponent Placement
        {
            get;
            set;
        }

        public void OnPlacementChanged(GPlacementComponent placement)
        {
        }
        //不作用于物理
        public void OnPlacementChangedUninfluencePhysics(GPlacementComponent placement)
        {
        }
    }
}
