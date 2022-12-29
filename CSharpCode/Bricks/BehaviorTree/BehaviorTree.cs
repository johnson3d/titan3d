using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.BehaviorTree
{
    //public class BehaviorTreeLogicGraph : NodeGraph.LogicGraph<Behavior>
    //{
    //    public NodeGraph.Node<Behavior> Root { get; set; }
    //    public BehaviorStatus Tick(long timeElapse, GamePlay.UCenterData context)
    //    {
    //        if (Root == null)
    //            return BehaviorStatus.Failure;
    //        return Root.DataObject.Tick(timeElapse, context);
    //    }
    //}
    public class BehaviorTree
    {
        public List<BehaviorEvent> BehaviorEvents { get; set; } = new List<BehaviorEvent>();
        protected Behavior mRoot = null;
        //[Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Behavior Root
        {
            get=> mRoot;
            set
            {
                mRoot = value;
            
            }
        }
        public void RegisterEvent()
        {
            mRoot.RegisterEvent(this);
        }
        int RootPriority = 0;
        public void RefreshPriority()
        {
            mRoot.AllocatePriority(ref RootPriority);
        }
        public void AddBehaviorEvent(BehaviorEvent  behaviorEvent)
        {
            BehaviorEvents.Add(behaviorEvent);
        }
        void EvaluateBehaviorEvents(long timeElapse, GamePlay.UCenterData context)
        {
            for (int i = 0; i < BehaviorEvents.Count; ++i)
            {
                if (BehaviorEvents[i].Checking(timeElapse, context))
                {
                    if (mRoot.Status == BehaviorStatus.Running)
                    {
                        var runningBh = mRoot.GetRunningBehavior();
                        if (runningBh == null)
                        {
                            //行为树已经执行完了，不需要其他操作
                        }
                        else
                        {
                            switch (BehaviorEvents[i].Behavior.FlowControl)
                            {
                                case FlowControlType.None:
                                    {

                                    }
                                    break;
                                case FlowControlType.Self:
                                    {
                                        if (BehaviorEvents[i].Behavior.Status == BehaviorStatus.Running)
                                        {
                                            Root.Reset();
                                            Root.Schedule(BehaviorEvents[i].Behavior);
                                        }
                                    }
                                    break;
                                case FlowControlType.LowPriority:
                                    {
                                        if (BehaviorEvents[i].Behavior.Status != BehaviorStatus.Running)
                                        {
                                            if (BehaviorEvents[i].Behavior.Priority < runningBh.Priority)
                                            {
                                                Root.Reset();
                                                Root.Schedule(BehaviorEvents[i].Behavior);
                                            }
                                        }
                                    }
                                    break;
                                case FlowControlType.Both:
                                    {
                                        if (BehaviorEvents[i].Behavior.Priority < runningBh.Priority)
                                        {
                                            Root.Reset();
                                            Root.Schedule(BehaviorEvents[i].Behavior);
                                        }
                                    }
                                    break;
                            }

                        }
                    }

                }
            }
        }
        public BehaviorStatus Tick(long timeElapse, GamePlay.UCenterData context)
        {
            if (Root == null)
                return BehaviorStatus.Failure;
            if (Root.Status != BehaviorStatus.Invalid)
            {
                EvaluateBehaviorEvents(timeElapse, context);
            }
            return Root.Tick(timeElapse, context);
        }
    }
}
