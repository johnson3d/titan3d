using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using EngineNS;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Actor;
using EngineNS.Bricks.AI;
using EngineNS.Bricks.AI.BehaviorTree;
using EngineNS.Bricks.AI.BehaviorTree.Composite;
using EngineNS.Bricks.AI.BehaviorTree.Leaf.Action;
using EngineNS.Bricks.AI.BehaviorTree.Leaf.Condition;
using EngineNS.GamePlay.Component;
using EngineNS.Support;
using System.Threading.Tasks;

namespace Game
{
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable)]
    public partial class CGameInstance : EngineNS.GamePlay.GGameInstance
    {
        public override async System.Threading.Tasks.Task<bool> InitGame(IntPtr WinHandle, UInt32 width, UInt32 height, EngineNS.GamePlay.GGameInstanceDesc desc, EngineNS.Graphics.CGfxCamera camera)
        {
#if PWindow
            EngineNS.Bricks.RemoteServices.RPCExecuter.Instance.SaveCode();
#elif PAndroid
            AccessGameRes.CheckAndCreateFirstAssetList();
#endif
            
            return await base.InitGame(WinHandle, width, height, desc, camera);
        }
        public override async Task OnGameInited()
        {
            await base.OnGameInited();
            //var rp = this.RenderPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_GameMobile;
            //rp.mOpaqueSE.DisableShadow = true;
            //rp.mOpaqueSE.DisableAO = true;
            //rp.mOpaqueSE.DisablePointLights = true;
        }
        EngineNS.Bricks.FreeTypeFont.CFontMesh mFontMesh;
        
        public override void TickSync()
        {
            base.TickSync();

            EngineNS.Bricks.Particle.McParticleHelper.Benchmark();
        }
    }
}
