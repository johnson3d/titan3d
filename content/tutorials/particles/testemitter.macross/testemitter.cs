namespace NS_tutorials.particles
{
    [EngineNS.Macross.UMacross]
    public partial class testemitter : EngineNS.Bricks.Particle.TtEmitterMacross
    {
        [EngineNS.Rtti.Meta]
        private System.Int32 Member_0 { get; set; } = 0;
        public EngineNS.Macross.UMacrossBreak breaker_SetParticleFlags_4124938284 = new EngineNS.Macross.UMacrossBreak("breaker_SetParticleFlags_4124938284");
        public EngineNS.Macross.UMacrossBreak breaker_Spawn_318270620 = new EngineNS.Macross.UMacrossBreak("breaker_Spawn_318270620");
        public EngineNS.Macross.UMacrossBreak breaker_if_801148221 = new EngineNS.Macross.UMacrossBreak("breaker_if_801148221");
        public EngineNS.Macross.UMacrossBreak breaker_GetParticleData_2624031083 = new EngineNS.Macross.UMacrossBreak("breaker_GetParticleData_2624031083");
        public EngineNS.Macross.UMacrossBreak breaker_SetParticleFlags_43658510 = new EngineNS.Macross.UMacrossBreak("breaker_SetParticleFlags_43658510");
        public EngineNS.Macross.UMacrossBreak breaker_Spawn_2821917696 = new EngineNS.Macross.UMacrossBreak("breaker_Spawn_2821917696");
        public EngineNS.Macross.UMacrossBreak breaker_if_1932775334 = new EngineNS.Macross.UMacrossBreak("breaker_if_1932775334");
        public EngineNS.Macross.UMacrossBreak breaker_SetParticleFlags_2645294717 = new EngineNS.Macross.UMacrossBreak("breaker_SetParticleFlags_2645294717");
        public EngineNS.Macross.UMacrossBreak breaker_Spawn_4269439374 = new EngineNS.Macross.UMacrossBreak("breaker_Spawn_4269439374");
        public EngineNS.Macross.UMacrossBreak breaker_RandomUnit_2399714997 = new EngineNS.Macross.UMacrossBreak("breaker_RandomUnit_2399714997");
        public EngineNS.Macross.UMacrossBreak breaker_RandomNext_2440437032 = new EngineNS.Macross.UMacrossBreak("breaker_RandomNext_2440437032");
        EngineNS.Macross.UMacrossStackFrame mFrame_DoUpdateSystem = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void DoUpdateSystem(EngineNS.Bricks.Particle.TtEmitter emt)
        {
            using(var guard_DoUpdateSystem = new EngineNS.Macross.UMacrossStackGuard(mFrame_DoUpdateSystem))
            {
                mFrame_DoUpdateSystem.SetWatchVariable("emt", emt);
                System.UInt32 tmp_r_SetParticleFlags_4124938284 = default(System.UInt32);
                System.UInt32 tmp_r_Spawn_318270620 = default(System.UInt32);
                mFrame_DoUpdateSystem.SetWatchVariable("Condition0_801148221", (emt.EmitterDataRef.Flags == 0));
                breaker_if_801148221.TryBreak();
                if ((emt.EmitterDataRef.Flags == 0))
                {
                    mFrame_DoUpdateSystem.SetWatchVariable("v_flags_SetParticleFlags_4124938284", EngineNS.Bricks.Particle.EParticleFlags.EmitShape);
                    mFrame_DoUpdateSystem.SetWatchVariable("v_data_SetParticleFlags_4124938284", 0);
                    breaker_SetParticleFlags_4124938284.TryBreak();
                    tmp_r_SetParticleFlags_4124938284 = emt.SetParticleFlags(EngineNS.Bricks.Particle.EParticleFlags.EmitShape,0);
                    mFrame_DoUpdateSystem.SetWatchVariable("tmp_r_SetParticleFlags_4124938284", tmp_r_SetParticleFlags_4124938284);
                    mFrame_DoUpdateSystem.SetWatchVariable("v_num_Spawn_318270620", 512);
                    mFrame_DoUpdateSystem.SetWatchVariable("v_flags_Spawn_318270620", tmp_r_SetParticleFlags_4124938284);
                    mFrame_DoUpdateSystem.SetWatchVariable("v_life_Spawn_318270620", 3f);
                    breaker_Spawn_318270620.TryBreak();
                    tmp_r_Spawn_318270620 = emt.Spawn(512,tmp_r_SetParticleFlags_4124938284,3f);
                    mFrame_DoUpdateSystem.SetWatchVariable("tmp_r_Spawn_318270620", tmp_r_Spawn_318270620);
                    emt.EmitterDataRef.Flags = 1;
                }
                else
                {
                }
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_OnDeadParticle = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnDeadParticle(EngineNS.Bricks.Particle.TtEmitter emt,System.UInt32 index,ref EngineNS.Bricks.Particle.FParticle particle)
        {
            using(var guard_OnDeadParticle = new EngineNS.Macross.UMacrossStackGuard(mFrame_OnDeadParticle))
            {
                mFrame_OnDeadParticle.SetWatchVariable("emt", emt);
                mFrame_OnDeadParticle.SetWatchVariable("index", index);
                mFrame_OnDeadParticle.SetWatchVariable("particle", particle);
                System.UInt32 tmp_r_GetParticleData_2624031083 = default(System.UInt32);
                System.UInt32 tmp_r_SetParticleFlags_43658510 = default(System.UInt32);
                System.UInt32 tmp_r_Spawn_2821917696 = default(System.UInt32);
                System.UInt32 tmp_r_SetParticleFlags_2645294717 = default(System.UInt32);
                System.UInt32 tmp_r_Spawn_4269439374 = default(System.UInt32);
                mFrame_OnDeadParticle.SetWatchVariable("v_flags_GetParticleData_2624031083", particle.Flags);
                breaker_GetParticleData_2624031083.TryBreak();
                tmp_r_GetParticleData_2624031083 = emt.GetParticleData(particle.Flags);
                mFrame_OnDeadParticle.SetWatchVariable("tmp_r_GetParticleData_2624031083", tmp_r_GetParticleData_2624031083);
                mFrame_OnDeadParticle.SetWatchVariable("Condition0_1932775334", (tmp_r_GetParticleData_2624031083 == 0));
                breaker_if_1932775334.TryBreak();
                if ((tmp_r_GetParticleData_2624031083 == 0))
                {
                    mFrame_OnDeadParticle.SetWatchVariable("v_flags_SetParticleFlags_43658510", EngineNS.Bricks.Particle.EParticleFlags.EmitShape);
                    mFrame_OnDeadParticle.SetWatchVariable("v_data_SetParticleFlags_43658510", 1);
                    breaker_SetParticleFlags_43658510.TryBreak();
                    tmp_r_SetParticleFlags_43658510 = emt.SetParticleFlags(EngineNS.Bricks.Particle.EParticleFlags.EmitShape,1);
                    mFrame_OnDeadParticle.SetWatchVariable("tmp_r_SetParticleFlags_43658510", tmp_r_SetParticleFlags_43658510);
                    mFrame_OnDeadParticle.SetWatchVariable("v_num_Spawn_2821917696", 1);
                    mFrame_OnDeadParticle.SetWatchVariable("v_flags_Spawn_2821917696", tmp_r_SetParticleFlags_43658510);
                    mFrame_OnDeadParticle.SetWatchVariable("v_life_Spawn_2821917696", 5f);
                    breaker_Spawn_2821917696.TryBreak();
                    tmp_r_Spawn_2821917696 = emt.Spawn(1,tmp_r_SetParticleFlags_43658510,5f);
                    mFrame_OnDeadParticle.SetWatchVariable("tmp_r_Spawn_2821917696", tmp_r_Spawn_2821917696);
                }
                else
                {
                    mFrame_OnDeadParticle.SetWatchVariable("v_flags_SetParticleFlags_2645294717", EngineNS.Bricks.Particle.EParticleFlags.EmitShape);
                    mFrame_OnDeadParticle.SetWatchVariable("v_data_SetParticleFlags_2645294717", 0);
                    breaker_SetParticleFlags_2645294717.TryBreak();
                    tmp_r_SetParticleFlags_2645294717 = emt.SetParticleFlags(EngineNS.Bricks.Particle.EParticleFlags.EmitShape,0);
                    mFrame_OnDeadParticle.SetWatchVariable("tmp_r_SetParticleFlags_2645294717", tmp_r_SetParticleFlags_2645294717);
                    mFrame_OnDeadParticle.SetWatchVariable("v_num_Spawn_4269439374", 1);
                    mFrame_OnDeadParticle.SetWatchVariable("v_flags_Spawn_4269439374", tmp_r_SetParticleFlags_2645294717);
                    mFrame_OnDeadParticle.SetWatchVariable("v_life_Spawn_4269439374", 3f);
                    breaker_Spawn_4269439374.TryBreak();
                    tmp_r_Spawn_4269439374 = emt.Spawn(1,tmp_r_SetParticleFlags_2645294717,3f);
                    mFrame_OnDeadParticle.SetWatchVariable("tmp_r_Spawn_4269439374", tmp_r_Spawn_4269439374);
                }
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_OnInitParticle = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnInitParticle(EngineNS.Bricks.Particle.TtEmitter emt,ref EngineNS.Bricks.Particle.FParticle particle)
        {
            using(var guard_OnInitParticle = new EngineNS.Macross.UMacrossStackGuard(mFrame_OnInitParticle))
            {
                mFrame_OnInitParticle.SetWatchVariable("emt", emt);
                mFrame_OnInitParticle.SetWatchVariable("particle", particle);
                System.Single tmp_r_RandomUnit_2399714997 = default(System.Single);
                System.Int32 tmp_r_RandomNext_2440437032 = default(System.Int32);
                particle.Velocity = emt.Velocity;
                breaker_RandomUnit_2399714997.TryBreak();
                tmp_r_RandomUnit_2399714997 = emt.RandomUnit();
                mFrame_OnInitParticle.SetWatchVariable("tmp_r_RandomUnit_2399714997", tmp_r_RandomUnit_2399714997);
                particle.Life = (tmp_r_RandomUnit_2399714997 + particle.Life);
                breaker_RandomNext_2440437032.TryBreak();
                tmp_r_RandomNext_2440437032 = emt.RandomNext();
                mFrame_OnInitParticle.SetWatchVariable("tmp_r_RandomNext_2440437032", tmp_r_RandomNext_2440437032);
                particle.Color = (System.UInt32)(tmp_r_RandomNext_2440437032);
                particle.Scale = 1f;
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_OnParticleTick = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnParticleTick(EngineNS.Bricks.Particle.TtEmitter emt,System.Single elapsed,ref EngineNS.Bricks.Particle.FParticle particle)
        {
            using(var guard_OnParticleTick = new EngineNS.Macross.UMacrossStackGuard(mFrame_OnParticleTick))
            {
                mFrame_OnParticleTick.SetWatchVariable("emt", emt);
                mFrame_OnParticleTick.SetWatchVariable("elapsed", elapsed);
                mFrame_OnParticleTick.SetWatchVariable("particle", particle);
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_OnTimer = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnTimer(EngineNS.Bricks.Particle.TtEmitter emt,System.Single second)
        {
            using(var guard_OnTimer = new EngineNS.Macross.UMacrossStackGuard(mFrame_OnTimer))
            {
                mFrame_OnTimer.SetWatchVariable("emt", emt);
                mFrame_OnTimer.SetWatchVariable("second", second);
            }
        }
    }
}
