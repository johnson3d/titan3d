namespace NS_tutorials.particles
{
    [EngineNS.Macross.UMacross]
    public partial class test01_move : EngineNS.Bricks.Particle.TtNebulaMacross
    {
        public EngineNS.Macross.UMacrossBreak breaker_Sin_1051780063 = new EngineNS.Macross.UMacrossBreak("breaker_Sin_1051780063");
        public EngineNS.Macross.UMacrossBreak breaker_CreateVector3_1518611406 = new EngineNS.Macross.UMacrossBreak("breaker_CreateVector3_1518611406");
        EngineNS.Macross.UMacrossStackFrame mFrame_OnUpdate = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/test01_move.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnUpdate(EngineNS.Bricks.Particle.TtNebulaParticle nebula,EngineNS.Bricks.Particle.UParticleGraphNode particleSystem,System.Single elpased)
        {
            using(var guard_OnUpdate = new EngineNS.Macross.UMacrossStackGuard(mFrame_OnUpdate))
            {
                mFrame_OnUpdate.SetWatchVariable("nebula", nebula);
                mFrame_OnUpdate.SetWatchVariable("particleSystem", particleSystem);
                mFrame_OnUpdate.SetWatchVariable("elpased", elpased);
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_OnUpdateEmitter = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/test01_move.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnUpdateEmitter(EngineNS.Bricks.Particle.TtNebulaParticle nebula,EngineNS.Bricks.Particle.TtEmitter emitter,EngineNS.Bricks.Particle.UParticleGraphNode particleSystem,System.Single elpased)
        {
            using(var guard_OnUpdateEmitter = new EngineNS.Macross.UMacrossStackGuard(mFrame_OnUpdateEmitter))
            {
                mFrame_OnUpdateEmitter.SetWatchVariable("nebula", nebula);
                mFrame_OnUpdateEmitter.SetWatchVariable("emitter", emitter);
                mFrame_OnUpdateEmitter.SetWatchVariable("particleSystem", particleSystem);
                mFrame_OnUpdateEmitter.SetWatchVariable("elpased", elpased);
                System.Single tmp_r_Sin_1051780063 = default(System.Single);
                EngineNS.Vector3 tmp_r_CreateVector3_1518611406 = default(EngineNS.Vector3);
                mFrame_OnUpdateEmitter.SetWatchVariable("v_v_Sin_1051780063", (EngineNS.UEngine.Instance.TickCountSecond * 0.0001f));
                breaker_Sin_1051780063.TryBreak();
                tmp_r_Sin_1051780063 = EngineNS.MathHelper.Sin((EngineNS.UEngine.Instance.TickCountSecond * 0.0001f));
                mFrame_OnUpdateEmitter.SetWatchVariable("tmp_r_Sin_1051780063", tmp_r_Sin_1051780063);
                mFrame_OnUpdateEmitter.SetWatchVariable("v_x_CreateVector3_1518611406", (tmp_r_Sin_1051780063 * 10f));
                mFrame_OnUpdateEmitter.SetWatchVariable("v_y_CreateVector3_1518611406", (tmp_r_Sin_1051780063 * 10f));
                mFrame_OnUpdateEmitter.SetWatchVariable("v_z_CreateVector3_1518611406", 0f);
                breaker_CreateVector3_1518611406.TryBreak();
                tmp_r_CreateVector3_1518611406 = EngineNS.MathHelper.CreateVector3((tmp_r_Sin_1051780063 * 10f),(tmp_r_Sin_1051780063 * 10f),0f);
                mFrame_OnUpdateEmitter.SetWatchVariable("tmp_r_CreateVector3_1518611406", tmp_r_CreateVector3_1518611406);
                emitter.Location = tmp_r_CreateVector3_1518611406;
            }
        }
    }
}
