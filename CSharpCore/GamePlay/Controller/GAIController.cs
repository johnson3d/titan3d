using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;

namespace EngineNS.GamePlay.Controller
{
    [Rtti.MetaClass]
    public class GAIControllerInitializer : GComponent.GComponentInitializer
    {

    }
    [Rtti.MetaClass]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GAIControllerInitializer), "AI控制器", "Controller", "AIController")]
    [Editor.Editor_MacrossClassIconAttribute("icon/mcaicontroller_64.txpic", RName.enRNameType.Editor)]
    public class GAIController : GControllerBase
    {
        public GAIController()
        {
            OnlyForGame = true;
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (await base.SetInitializer(rc, host, hostContainer, v) == false)
                return false;
            return true;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McGAIController))]
        public override RName ComponentMacross
        {
            get { return base.ComponentMacross; }
            set
            {
                base.ComponentMacross = value;
                if (McGAIController != null)
                {
                    McGAIController.HostComp = this;
                    McGAIController?.Construct();
                }
            }
        }
        McGAIController McGAIController
        {
            get
            {
                return mMcCompGetter?.CastGet<McGAIController>(OnlyForGame);
            }
        }
        public override void Tick(GPlacementComponent placement)
        {
            base.Tick(placement);
            McGAIController?.Tick(placement);
        }
    }

    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Editor.Editor_MacrossClassIconAttribute("icon/mcaicontroller_64.txpic", RName.enRNameType.Editor)]
    public class McGAIController : McComponent
    {
        public override void OnNewMacross()
        {
            Construct();
        }
        public virtual void Construct()
        {

        }
        public virtual void Tick(GPlacementComponent placement)
        {
            OnTick(HostComp);
        }
    }
}
