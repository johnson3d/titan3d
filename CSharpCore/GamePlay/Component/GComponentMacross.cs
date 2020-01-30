using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    [Obsolete("Deprecated")]
    public class GComponentMacross : GComponent
    {
        //Deprecated
        [Rtti.MetaClass]
        public class GComponentMacrossComponenInitializer : GComponentInitializer
        {
            [Rtti.MetaData]
            public RName Name
            {
                get; set;
            }

        }
        public GComponentMacross()
        {
            Initializer = new GComponentMacrossComponenInitializer();
        }
        bool mIsFirstTicked = false;
        public sealed override void Tick(GPlacementComponent placement)
        {
            if(!mIsFirstTicked)
            {
                OnBeginPlay(placement);
                mIsFirstTicked = true;
            }
            Update(placement);
            OnTick(placement);
            base.Tick(placement);
        }
        public virtual void Update(GPlacementComponent placement)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnInitialize(GPlacementComponent placement)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnUnInitialize(GPlacementComponent placement)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnTick(GPlacementComponent placement)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnBeginPlay(GPlacementComponent placement)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnEndPlay(GPlacementComponent placement)
        {

        }
    }
}
