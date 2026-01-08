using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.WorldSimulator.GOAP
{
    public partial class IAction
    {
        public float TmpCost = float.MaxValue;
        [Rtti.Meta("")]
        public string Name
        {
            get;
            set;
        }
        [Rtti.Meta("")]
        public IWeights Weights
        {
            get;
            set;
        }
        [Rtti.Meta("")]
        public IGoal Goal
        {
            get;
            set;
        }
        [Rtti.Meta("")]
        public virtual bool PassPreCondition(IActor actor, IEnvironment env)
        {
            //actor.Inventory.HaveItem("Money")
            return false;
        }
        [Rtti.Meta("")]
        public virtual void OnStartAction(IActor actor, IEnvironment env)
        {
            
        }
        [Rtti.Meta("")]
        public virtual void OnTickAction(IActor actor, IEnvironment env)
        {

        }
        [Rtti.Meta("")]
        public virtual bool IsFinished(IActor actor, IEnvironment env)
        {
            return true;
        }
        [Rtti.Meta("")]
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


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.WorldSimulator.GOAP
{
	partial class IAction
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_PassPreCondition_1107648517 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.WorldSimulator.GOAP.IAction->bool PassPreCondition(IActor actor, IEnvironment env)");
		public unsafe bool macross_PassPreCondition (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, IActor actor, IEnvironment env) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":actor", actor);
					stackframe.SetWatchVariable(nodeName + ":env", env);
				}
			}
			var _return_value = PassPreCondition(actor, env);
			macross_break_PassPreCondition_1107648517.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnStartAction_1107648517 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.WorldSimulator.GOAP.IAction->void OnStartAction(IActor actor, IEnvironment env)");
		public unsafe void macross_OnStartAction (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, IActor actor, IEnvironment env) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":actor", actor);
					stackframe.SetWatchVariable(nodeName + ":env", env);
				}
			}
			OnStartAction(actor, env);
			macross_break_OnStartAction_1107648517.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnTickAction_1107648517 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.WorldSimulator.GOAP.IAction->void OnTickAction(IActor actor, IEnvironment env)");
		public unsafe void macross_OnTickAction (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, IActor actor, IEnvironment env) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":actor", actor);
					stackframe.SetWatchVariable(nodeName + ":env", env);
				}
			}
			OnTickAction(actor, env);
			macross_break_OnTickAction_1107648517.TryBreak(mcStack);
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_IsFinished_1107648517 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.WorldSimulator.GOAP.IAction->bool IsFinished(IActor actor, IEnvironment env)");
		public unsafe bool macross_IsFinished (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, IActor actor, IEnvironment env) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":actor", actor);
					stackframe.SetWatchVariable(nodeName + ":env", env);
				}
			}
			var _return_value = IsFinished(actor, env);
			macross_break_IsFinished_1107648517.TryBreak(mcStack);
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnActionFinished_1486009775 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.WorldSimulator.GOAP.IAction->void OnActionFinished(bool bSuccessed)");
		public unsafe void macross_OnActionFinished (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, bool bSuccessed) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":bSuccessed", bSuccessed);
				}
			}
			OnActionFinished(bSuccessed);
			macross_break_OnActionFinished_1486009775.TryBreak(mcStack);
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross