namespace NS_tutorials.particles
{
    [EngineNS.Macross.TtMacross]
    public partial class test01_move : EngineNS.Bricks.Particle.TtNebulaMacross
    {
        public EngineNS.Macross.TtMacrossBreak breaker_Sin_1051780063 = new EngineNS.Macross.TtMacrossBreak("breaker_Sin_1051780063");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateVector3f_4100741359 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateVector3f_4100741359");
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnUpdateEmitter_3880941163 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/test01_move.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnUpdateEmitter(EngineNS.Bricks.Particle.TtNebulaParticle nebula,EngineNS.Bricks.Particle.TtEmitter emitter,EngineNS.Bricks.Particle.UParticleGraphNode particleSystem,System.Single elpased)
        {
            using(var guard_OnUpdateEmitter = new EngineNS.Macross.TtMacrossStackGuard(mFrame_OnUpdateEmitter_3880941163))
            {
                mFrame_OnUpdateEmitter_3880941163.SetWatchVariable("nebula", nebula);
                mFrame_OnUpdateEmitter_3880941163.SetWatchVariable("emitter", emitter);
                mFrame_OnUpdateEmitter_3880941163.SetWatchVariable("particleSystem", particleSystem);
                mFrame_OnUpdateEmitter_3880941163.SetWatchVariable("elpased", elpased);
                System.Single tmp_r_Sin_1051780063 = default(System.Single);
                EngineNS.Vector3 tmp_r_CreateVector3f_4100741359 = default(EngineNS.Vector3);
                mFrame_OnUpdateEmitter_3880941163.SetWatchVariable("v_v_Sin_1051780063", (EngineNS.TtEngine.Instance.TickCountSecond * 0.0001f));
                breaker_Sin_1051780063.TryBreak();
                tmp_r_Sin_1051780063 = EngineNS.MathHelper.Sin((EngineNS.TtEngine.Instance.TickCountSecond * 0.0001f));
                mFrame_OnUpdateEmitter_3880941163.SetWatchVariable("tmp_r_Sin_1051780063", tmp_r_Sin_1051780063);
                mFrame_OnUpdateEmitter_3880941163.SetWatchVariable("v_x_CreateVector3f_4100741359", 0f);
                mFrame_OnUpdateEmitter_3880941163.SetWatchVariable("v_y_CreateVector3f_4100741359", (tmp_r_Sin_1051780063 * 10f));
                mFrame_OnUpdateEmitter_3880941163.SetWatchVariable("v_z_CreateVector3f_4100741359", 0f);
                breaker_CreateVector3f_4100741359.TryBreak();
                tmp_r_CreateVector3f_4100741359 = EngineNS.MathHelper.CreateVector3f(0f,(tmp_r_Sin_1051780063 * 10f),0f);
                mFrame_OnUpdateEmitter_3880941163.SetWatchVariable("tmp_r_CreateVector3f_4100741359", tmp_r_CreateVector3f_4100741359);
                emitter.Location = tmp_r_CreateVector3f_4100741359;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnUpdate_3549459165 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/test01_move.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnUpdate(EngineNS.Bricks.Particle.TtNebulaParticle nebula,EngineNS.Bricks.Particle.UParticleGraphNode particleSystem,System.Single elpased)
        {
            using(var guard_OnUpdate = new EngineNS.Macross.TtMacrossStackGuard(mFrame_OnUpdate_3549459165))
            {
                mFrame_OnUpdate_3549459165.SetWatchVariable("nebula", nebula);
                mFrame_OnUpdate_3549459165.SetWatchVariable("particleSystem", particleSystem);
                mFrame_OnUpdate_3549459165.SetWatchVariable("elpased", elpased);
            }
        }
    }
}
