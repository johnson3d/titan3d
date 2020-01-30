using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.BehaviorTree
{
    public enum BehaviorStatus
    {
        Invalid,
        Success,
        Failure,
        Running,
        Aborted,
    }
    public abstract class Behavior
    {
        protected BehaviorStatus mStatus = BehaviorStatus.Invalid;
        public List<Service.ServiceBehavior> Services { get; set; } = new List<Service.ServiceBehavior>();
        public void AddService(Service.ServiceBehavior serivce)
        {
            Services.Add(serivce);
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public BehaviorStatus Status { get => mStatus; }
        protected Action<Behavior> mInitFunc;
        protected Action<Behavior> mExitFunc;
        protected Func<long, GamePlay.Actor.GCenterData, BehaviorStatus> mTickFunc;

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static Leaf.Action.ActionBehavior NewActionBehavior(Action<Behavior> init, Action<Behavior> exit, Func<long, GamePlay.Actor.GCenterData, BehaviorStatus> tick)
        {
            var bhv = new Leaf.Action.ActionBehavior();
            bhv.mInitFunc = init;
            bhv.mExitFunc = exit;
            bhv.mTickFunc = tick;
            return bhv;
        }
        public virtual void OnInitialize() { }
        public virtual void OnTerminate(BehaviorStatus status) { }
        public virtual void RegisterEvent(BehaviorTree tree)
        {

        }
        public int Priority { get; set; } = -1;
        public virtual void AllocatePriority(ref int priority)
        {
            Priority = priority;
        }
        public virtual bool Schedule(Behavior behavior)
        {
            if (behavior == this)
                return true;
            else
                return false;
        }
        public virtual Behavior GetRunningBehavior()
        {
            if (mStatus == BehaviorStatus.Running)
                return this;
            return null;
        }
        public BehaviorStatus Tick(long timeElapse, GamePlay.Actor.GCenterData context)
        {
            if (mStatus == BehaviorStatus.Invalid)
            {
                if (mInitFunc != null)
                    mInitFunc(this);
                OnInitialize();
                McBehaviorGetter?.Get()?.OnInitialize(this);
            }
            McBehaviorGetter?.Get()?.OnBeforeUpdate(this);
            if (mTickFunc != null)
                mTickFunc(timeElapse, context);
            mStatus = Update(timeElapse, context);

            if (mStatus != BehaviorStatus.Running)
            {
                if (mExitFunc != null)
                    mExitFunc(this);
                McBehaviorGetter?.Get()?.OnTerminate(this, mStatus);
                OnTerminate(mStatus);
            }
            for (int i = 0; i < Services.Count; ++i)
            {
                Services[i].Tick(timeElapse, context);
            }
            return mStatus;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public abstract BehaviorStatus Update(long timeElapse, GamePlay.Actor.GCenterData context);
        public virtual void Reset()
        {
            mStatus = BehaviorStatus.Invalid;
        }
        public void Abort()
        {
            OnTerminate(BehaviorStatus.Aborted);
            mStatus = BehaviorStatus.Aborted;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public bool IsTerminated { get => mStatus == BehaviorStatus.Success || mStatus == BehaviorStatus.Failure; }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public bool IsRunning { get => mStatus == BehaviorStatus.Running; }

        #region Macross
        RName mBehaviorMacross;
        [Editor.Editor_RNameMacrossType(typeof(McBehavior))]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual RName BehaviorMacross
        {
            get { return mBehaviorMacross; }
            set
            {
                mBehaviorMacross = value;
                mMcBehaviorGetter = CEngine.Instance.MacrossDataManager.NewObjectGetter<McBehavior>(value);
            }
        }
        protected Macross.MacrossGetter<McBehavior> mMcBehaviorGetter;
        [System.ComponentModel.Browsable(false)]
        public Macross.MacrossGetter<McBehavior> McBehaviorGetter
        {
            get { return mMcBehaviorGetter; }
        }
        #endregion
    }

    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class McBehavior
    {
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnInitialize(Behavior bhv)
        {
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnTerminate(Behavior bhv, BehaviorStatus status)
        {
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnBeforeUpdate(Behavior bhv)
        {

        }
    }
}
