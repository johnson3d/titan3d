using EngineNS.Bricks.CodeBuilder;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    [Macross.TtMacross]
    [TtMacrossNodeCustomCodeGen]
    public partial class TtPhyEventMacrossBase : TtSceneNodeMacrossBase
    {
        [Rtti.Meta]
        public virtual void OnContact(TtNode selfNode, TtNode otherNode)
        {
            //MacrossGetter.Get()
        }
        [Rtti.Meta]
        public virtual void OnBeginTrigger(TtNode selfNode, TtNode otherNode)
        {
            //MacrossGetter.Get()
        }
        [Rtti.Meta]
        public virtual void OnEndTrigger(TtNode selfNode, TtNode otherNode)
        {
            //MacrossGetter.Get()
        }

        public void SetPropertyValue_Gen(ulong propertyNameHash, in object value)
        {
            
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.PhysicsCore
{
	partial class TtPhyEventMacrossBase
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnContact_602697449 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.PhysicsCore.TtPhyEventMacrossBase->void OnContact(TtNode selfNode, TtNode otherNode)");
		public unsafe void macross_OnContact (string nodeName, TtNode selfNode, TtNode otherNode) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":selfNode", selfNode);
					stackframe.SetWatchVariable(nodeName + ":otherNode", otherNode);
				}
			}
			OnContact(selfNode, otherNode);
			macross_break_OnContact_602697449.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnBeginTrigger_602697449 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.PhysicsCore.TtPhyEventMacrossBase->void OnBeginTrigger(TtNode selfNode, TtNode otherNode)");
		public unsafe void macross_OnBeginTrigger (string nodeName, TtNode selfNode, TtNode otherNode) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":selfNode", selfNode);
					stackframe.SetWatchVariable(nodeName + ":otherNode", otherNode);
				}
			}
			OnBeginTrigger(selfNode, otherNode);
			macross_break_OnBeginTrigger_602697449.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnEndTrigger_602697449 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.PhysicsCore.TtPhyEventMacrossBase->void OnEndTrigger(TtNode selfNode, TtNode otherNode)");
		public unsafe void macross_OnEndTrigger (string nodeName, TtNode selfNode, TtNode otherNode) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":selfNode", selfNode);
					stackframe.SetWatchVariable(nodeName + ":otherNode", otherNode);
				}
			}
			OnEndTrigger(selfNode, otherNode);
			macross_break_OnEndTrigger_602697449.TryBreak();
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross