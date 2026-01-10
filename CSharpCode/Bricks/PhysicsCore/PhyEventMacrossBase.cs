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
        [Rtti.Meta("")]
        public virtual void OnContact(TtNode selfNode, TtNode otherNode)
        {
            //MacrossGetter.Get()
        }
        [Rtti.Meta("")]
        public virtual void OnBeginTrigger(TtNode selfNode, TtNode otherNode)
        {
            //MacrossGetter.Get()
        }
        [Rtti.Meta("")]
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
		public unsafe void macross_OnContact (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtNode selfNode, TtNode otherNode) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			OnContact(selfNode, otherNode);
		}
		public unsafe void macross_OnBeginTrigger (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtNode selfNode, TtNode otherNode) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			OnBeginTrigger(selfNode, otherNode);
		}
		public unsafe void macross_OnEndTrigger (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtNode selfNode, TtNode otherNode) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			OnEndTrigger(selfNode, otherNode);
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross