//This is a flag, Please keep it!
using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
namespace TtBehavior_f7f3c834_4354_44d8_bb86_e2e0e319912b
{
	public class test_bhv01 : EngineNS.GamePlay.Scene.TtBehavior
	{
		[Category("Option")]
        [EngineNS.Rtti.Meta]
        public int TestProp { get; set; } = 0;
		public override async TtTask BeginPlay(EngineNS.GamePlay.Scene.TtNode node)
		{
		}
		public override void Tick(EngineNS.GamePlay.Scene.TtNode node)
		{
		}
		public override void DestroyNode(EngineNS.GamePlay.Scene.TtNode node)
		{
		}
	}
}
