using EngineNS.GamePlay;
using EngineNS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Survivor
{
    [EngineNS.Macross.TtMacross]
    public partial class TtMacrossSurvivorGame : EngineNS.GamePlay.TtMacrossGame
    {
        public TtMacrossSurvivorGame()
		{
			this.SetGameMode(new TtGameMode());
		}
        [EngineNS.Rtti.Meta(Flags = EngineNS.Rtti.MetaAttribute.EMetaFlags.NoSerializable)]
        public TtGameMode SurvivorGameMode { get => GameMode as TtGameMode; }

        public override void Tick(TtGameInstance host, float elapsedMillisecond)
        {
            base.Tick(host, elapsedMillisecond);
			if(IsNeedTriggerPlayerDead)
			{
				if (AccTimeToTriggerPlayerDead >= TimeToTriggerPlayerDead)
				{
					OnPlayerDead();
					IsNeedTriggerPlayerDead = false;
					AccTimeToTriggerPlayerDead = 0;

                }
				else
				{
					AccTimeToTriggerPlayerDead += elapsedMillisecond * 0.001f;
				}
            }
        }
        bool IsNeedTriggerPlayerDead = false;
		float TimeToTriggerPlayerDead = 2;
		float AccTimeToTriggerPlayerDead = 0;

        public void CountToTriggerPlayerDead()
		{
			IsNeedTriggerPlayerDead = true;

        }
        [EngineNS.Rtti.Meta]
        public virtual void OnPlayerDead()
        {
			
        }
    }
}
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace Survivor
{
	partial class TtMacrossSurvivorGame
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnPlayerDead_2609910045 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtMacrossSurvivorGame->void OnPlayerDead()");
		public unsafe void macross_OnPlayerDead (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			OnPlayerDead();
			macross_break_OnPlayerDead_2609910045.TryBreak(mcStack);
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross