using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.WorldSimulator.GOAP
{
    public partial class IAction
    {
        public float TmpCost = float.MaxValue;
        [Rtti.Meta]
        public string Name
        {
            get;
            set;
        }
        [Rtti.Meta]
        public IWeights Weights
        {
            get;
            set;
        }
        [Rtti.Meta]
        public IGoal Goal
        {
            get;
            set;
        }
        [Rtti.Meta]
        public virtual bool PassPreCondition(IActor actor, IEnvironment env)
        {
            //actor.Inventory.HaveItem("Money")
            return false;
        }
        [Rtti.Meta]
        public virtual void OnStartAction(IActor actor, IEnvironment env)
        {
            
        }
        [Rtti.Meta]
        public virtual void OnTickAction(IActor actor, IEnvironment env)
        {

        }
        [Rtti.Meta]
        public virtual bool IsFinished(IActor actor, IEnvironment env)
        {
            return true;
        }
        [Rtti.Meta]
        public virtual void OnActionFinished(bool bSuccessed)
        {

        }
    }
    public class MoveToAction : IAction
    {
        public MoveToAction()
        {
            Goal = new IArrivedGoal();
        }
    }
    public class PlayAnimation : IAction
    {

    }
    public class UseItem : IAction
    {
        public override void OnStartAction(IActor actor, IEnvironment env)
        {
            //actor.Inventory.GetItem("Money").UseItem();
        }
    }
}
