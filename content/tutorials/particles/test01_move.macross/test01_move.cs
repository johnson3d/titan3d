namespace NS_tutorials.particles
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/particles/test01_move.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class test01_move : EngineNS.Bricks.Particle.TtNebulaMacross
    {
        public static EngineNS.Macross.TtMacrossBreak breaker_Sin_1260400330 = new EngineNS.Macross.TtMacrossBreak("breaker_Sin_1260400330");
        public static EngineNS.Macross.TtMacrossBreak breaker_CreateVector3f_3352141493 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateVector3f_3352141493");
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnUpdateEmitter_363922588 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/test01_move.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_OnUpdateEmitter_363922588 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override void OnUpdateEmitter(EngineNS.Bricks.Particle.TtNebulaParticle nebula,EngineNS.Bricks.Particle.TtEmitter emitter,EngineNS.Bricks.Particle.TtParticleGraphNode particleSystem,System.Single elpased)
        {
            #if !disable_macross_fea33170_dbce_4809_94ce_fd890ebf6fde
            using(var guard_OnUpdateEmitter = new EngineNS.Macross.TtMacrossStackGuard(mStack_OnUpdateEmitter_363922588,mFrame_OnUpdateEmitter_363922588))
            {
                mFrame_OnUpdateEmitter_363922588.SetWatchVariable("nebula", nebula);
                mFrame_OnUpdateEmitter_363922588.SetWatchVariable("emitter", emitter);
                mFrame_OnUpdateEmitter_363922588.SetWatchVariable("particleSystem", particleSystem);
                mFrame_OnUpdateEmitter_363922588.SetWatchVariable("elpased", elpased);
                System.Single tmp_r_Sin_1260400330 = default(System.Single);
                EngineNS.Vector3 tmp_r_CreateVector3f_3352141493 = default(EngineNS.Vector3);
                mFrame_OnUpdateEmitter_363922588.SetWatchVariable("v_v_Sin_1260400330", (EngineNS.TtEngine.Instance.TickCountSecond * 0.0001f));
                breaker_Sin_1260400330.TryBreak(mStack_OnUpdateEmitter_363922588, this);
                tmp_r_Sin_1260400330 = EngineNS.MathHelper.Sin((EngineNS.TtEngine.Instance.TickCountSecond * 0.0001f));
                mFrame_OnUpdateEmitter_363922588.SetWatchVariable("tmp_r_Sin_1260400330", tmp_r_Sin_1260400330);
                mFrame_OnUpdateEmitter_363922588.SetWatchVariable("v_x_CreateVector3f_3352141493", 0f);
                mFrame_OnUpdateEmitter_363922588.SetWatchVariable("v_y_CreateVector3f_3352141493", (tmp_r_Sin_1260400330 * 10f));
                mFrame_OnUpdateEmitter_363922588.SetWatchVariable("v_z_CreateVector3f_3352141493", 0f);
                breaker_CreateVector3f_3352141493.TryBreak(mStack_OnUpdateEmitter_363922588, this);
                tmp_r_CreateVector3f_3352141493 = EngineNS.MathHelper.CreateVector3f(0f,(tmp_r_Sin_1260400330 * 10f),0f);
                mFrame_OnUpdateEmitter_363922588.SetWatchVariable("tmp_r_CreateVector3f_3352141493", tmp_r_CreateVector3f_3352141493);
                Location = tmp_r_CreateVector3f_3352141493;
            }
            #endif //!disable_macross_fea33170_dbce_4809_94ce_fd890ebf6fde
        }
    }
}
