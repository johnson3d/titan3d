using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.WorldSimulator.GOAP
{
    public partial class IActor
    {
        //角色可以进行的行为
        public Dictionary<string, IAction> Actions { get; } = new Dictionary<string, IAction>();
        //角色的任务目标
        public Dictionary<IWeights, IGoal> Goals { get; } = new Dictionary<IWeights, IGoal>();
        //角色具备的物资数据
        public IInventory Inventory { get; } = new IInventory();
        //角色感知能力与数据
        public ISensor Senseor { get; } = new ISensor();

        public List<IAction> PlannedActions { get; } = new List<IAction>();

        public IGoal CurrentGoal { get; set; }
        public IGoal GetBestGoal()
        {
            float goalOrder = 0;
            IGoal goal = null;
            foreach (var i in Goals)
            {
                if(i.Key.GetWeights() > goalOrder)
                {
                    goal = i.Value;
                }
            }
            return goal;
        }
        public bool HaveAction(string action)
        {
            return Actions.ContainsKey(action);
        }
        public IAction GetAction(string action)
        {
            IAction result;
            if (Actions.TryGetValue(action, out result))
                return result;
            return null;
        }
        [Rtti.Meta("")]
        public virtual void OnPickedItem(IItem item, IInventory invetory, IItemContain contain)
        {

        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.WorldSimulator.GOAP
{
	partial class IActor
	{
		public unsafe void macross_OnPickedItem (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, IItem item, IInventory invetory, IItemContain contain) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			OnPickedItem(item, invetory, contain);
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross