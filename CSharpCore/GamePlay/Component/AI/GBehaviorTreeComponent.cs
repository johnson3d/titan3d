using EngineNS.GamePlay.Actor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.Component.AI
{
    [Rtti.MetaClass]
    public class GBehaviorTreeComponentInitializer : GComponent.GComponentInitializer
    {
    }
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GBehaviorTreeComponentInitializer), "行为树组件", "BehaviorTree", "BehaviorTreeComponent")]
    [Editor.Editor_ComponentClassIconAttribute("icon/behaviortree_64x.txpic", RName.enRNameType.Editor)]
    public class GBehaviorTreeComponent : GComponent
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McBehaviorTreeComponent))]
        public override RName ComponentMacross
        {
            get
            {
                return base.ComponentMacross;
            }
            set
            {
                base.ComponentMacross = value;
                if (McBehaviorTree != null)
                {
                    McBehaviorTree.HostComp = this;
                    if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                        McBehaviorTree?.Construct();
                }
            }
        }

        McBehaviorTreeComponent McBehaviorTree
        {
            get
            {
                return mMcCompGetter?.CastGet<McBehaviorTreeComponent>(OnlyForGame);
            }
        }
        [Browsable(false)]
        public GBehaviorTreeComponentInitializer BehaviorTreeComponentInitializer
        {
            get
            {
                var v = Initializer as GBehaviorTreeComponentInitializer;
                if (v != null)
                    return v;
                return null;
            }
        }
        public GBehaviorTreeComponent()
        {
            Initializer = new GBehaviorTreeComponentInitializer();
        }
        public override Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            return base.SetInitializer(rc, host, hostContainer, v);
        }
        public override Profiler.TimeScope GetTickTimeScope()
        {
            return ScopeTick;
        }
        public static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(GBehaviorTreeComponent), nameof(Tick));
        public override void Tick(GPlacementComponent placement)
        {
            McBehaviorTree?.Tick(placement);
        }
        public override void OnActorLoaded(GActor actor)
        {
            base.OnActorLoaded(actor);
            McBehaviorTree?.Construct();
        }
    }
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.MacrossGetter)]
    public class McBehaviorTreeComponent : Controller.McGAIController
    {
        public Bricks.AI.BehaviorTree.BehaviorTree BehaviorTree { get; set; } = new Bricks.AI.BehaviorTree.BehaviorTree();
        public McBehaviorTreeComponent()
        {
        }
        public override void OnNewMacross()
        {
            //Construct();
        }
        public override void Construct()
        {
            ConstructBTGraph();
            InitBehaviorTreeFunc?.Invoke();
            BehaviorTree.RegisterEvent();
            BehaviorTree.RefreshPriority();
            BehaviorTreeConstructed();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void BehaviorTreeConstructed()
        {
        }
        public virtual void ConstructBTGraph()
        {
        }
        public Action InitBehaviorTreeFunc { get; set; } = null;
        public override void Tick(GPlacementComponent placement)
        {
            OnTick(HostComp);
            BehaviorTree.Tick(CEngine.Instance.EngineElapseTime,HostActor.CenterData);
        }
    }
}
