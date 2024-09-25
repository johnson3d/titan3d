﻿namespace NS_tutorials.particles
{
    [EngineNS.Macross.TtMacross]
    public partial class testemitter : EngineNS.Bricks.Particle.TtEmitterMacross
    {
        public EngineNS.Macross.TtMacrossBreak breaker_InterlockedAddUInt32_1872985343 = new EngineNS.Macross.TtMacrossBreak("breaker_InterlockedAddUInt32_1872985343");
        public EngineNS.Macross.TtMacrossBreak breaker_SetParticleFlags_4124938284 = new EngineNS.Macross.TtMacrossBreak("breaker_SetParticleFlags_4124938284");
        public EngineNS.Macross.TtMacrossBreak breaker_Spawn_318270620 = new EngineNS.Macross.TtMacrossBreak("breaker_Spawn_318270620");
        public EngineNS.Macross.TtMacrossBreak breaker_if_801148221 = new EngineNS.Macross.TtMacrossBreak("breaker_if_801148221");
        public EngineNS.Macross.TtMacrossBreak breaker_GetParticleData_2624031083 = new EngineNS.Macross.TtMacrossBreak("breaker_GetParticleData_2624031083");
        public EngineNS.Macross.TtMacrossBreak breaker_SetParticleFlags_43658510 = new EngineNS.Macross.TtMacrossBreak("breaker_SetParticleFlags_43658510");
        public EngineNS.Macross.TtMacrossBreak breaker_Spawn_2821917696 = new EngineNS.Macross.TtMacrossBreak("breaker_Spawn_2821917696");
        public EngineNS.Macross.TtMacrossBreak breaker_if_1932775334 = new EngineNS.Macross.TtMacrossBreak("breaker_if_1932775334");
        public EngineNS.Macross.TtMacrossBreak breaker_SetParticleFlags_2645294717 = new EngineNS.Macross.TtMacrossBreak("breaker_SetParticleFlags_2645294717");
        public EngineNS.Macross.TtMacrossBreak breaker_Spawn_4269439374 = new EngineNS.Macross.TtMacrossBreak("breaker_Spawn_4269439374");
        public EngineNS.Macross.TtMacrossBreak breaker_RandomUnit_3537506712 = new EngineNS.Macross.TtMacrossBreak("breaker_RandomUnit_3537506712");
        public EngineNS.Macross.TtMacrossBreak breaker_RandomUnit_2843124104 = new EngineNS.Macross.TtMacrossBreak("breaker_RandomUnit_2843124104");
        public EngineNS.Macross.TtMacrossBreak breaker_Uint2Color4f_3287025661 = new EngineNS.Macross.TtMacrossBreak("breaker_Uint2Color4f_3287025661");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateColor4f_1754197827 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateColor4f_1754197827");
        public EngineNS.Macross.TtMacrossBreak breaker_Color2Uint_1566520051 = new EngineNS.Macross.TtMacrossBreak("breaker_Color2Uint_1566520051");
        EngineNS.Macross.TtMacrossStackFrame mFrame_DoUpdateSystem_3384864483 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void DoUpdateSystem(EngineNS.Bricks.Particle.TtEmitter emt)
        {
            using(var guard_DoUpdateSystem = new EngineNS.Macross.TtMacrossStackGuard(mFrame_DoUpdateSystem_3384864483))
            {
                mFrame_DoUpdateSystem_3384864483.SetWatchVariable("emt", emt);
                System.UInt32 tmp_r_SetParticleFlags_4124938284 = default(System.UInt32);
                System.UInt32 tmp_r_Spawn_318270620 = default(System.UInt32);
                mFrame_DoUpdateSystem_3384864483.SetWatchVariable("Flags", emt.EmitterDataRef.mFlags);
                mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_value_InterlockedAddUInt32_1872985343", 1);
                System.UInt32 v_oriValue_InterlockedAddUInt32_1872985343 = default(System.UInt32);
                mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_oriValue_InterlockedAddUInt32_1872985343", v_oriValue_InterlockedAddUInt32_1872985343);
                breaker_InterlockedAddUInt32_1872985343.TryBreak();
                EngineNS.Graphics.Pipeline.Shader.TtMacrossShaderUtility.InterlockedAddUInt32(ref emt.EmitterDataRef.mFlags,1,out v_oriValue_InterlockedAddUInt32_1872985343);
                mFrame_DoUpdateSystem_3384864483.SetWatchVariable("Condition0_801148221", (v_oriValue_InterlockedAddUInt32_1872985343 < 512));
                breaker_if_801148221.TryBreak();
                if ((v_oriValue_InterlockedAddUInt32_1872985343 < 512))
                {
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_flags_SetParticleFlags_4124938284", EngineNS.Bricks.Particle.EParticleFlags.EmitShape);
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_data_SetParticleFlags_4124938284", 0);
                    breaker_SetParticleFlags_4124938284.TryBreak();
                    tmp_r_SetParticleFlags_4124938284 = emt.SetParticleFlags(EngineNS.Bricks.Particle.EParticleFlags.EmitShape,0);
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("tmp_r_SetParticleFlags_4124938284", tmp_r_SetParticleFlags_4124938284);
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_num_Spawn_318270620", 1);
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_flags_Spawn_318270620", tmp_r_SetParticleFlags_4124938284);
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_life_Spawn_318270620", 3f);
                    breaker_Spawn_318270620.TryBreak();
                    tmp_r_Spawn_318270620 = emt.Spawn(1,tmp_r_SetParticleFlags_4124938284,3f);
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("tmp_r_Spawn_318270620", tmp_r_Spawn_318270620);
                }
                else
                {
                }
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnDeadParticle_2348756594 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnDeadParticle(EngineNS.Bricks.Particle.TtEmitter emt,System.UInt32 index,ref EngineNS.Bricks.Particle.FParticle particle)
        {
            using(var guard_OnDeadParticle = new EngineNS.Macross.TtMacrossStackGuard(mFrame_OnDeadParticle_2348756594))
            {
                mFrame_OnDeadParticle_2348756594.SetWatchVariable("emt", emt);
                mFrame_OnDeadParticle_2348756594.SetWatchVariable("index", index);
                mFrame_OnDeadParticle_2348756594.SetWatchVariable("particle", particle);
                System.UInt32 tmp_r_GetParticleData_2624031083 = default(System.UInt32);
                System.UInt32 tmp_r_SetParticleFlags_43658510 = default(System.UInt32);
                System.UInt32 tmp_r_Spawn_2821917696 = default(System.UInt32);
                System.UInt32 tmp_r_SetParticleFlags_2645294717 = default(System.UInt32);
                System.UInt32 tmp_r_Spawn_4269439374 = default(System.UInt32);
                mFrame_OnDeadParticle_2348756594.SetWatchVariable("v_flags_GetParticleData_2624031083", particle.Flags);
                breaker_GetParticleData_2624031083.TryBreak();
                tmp_r_GetParticleData_2624031083 = emt.GetParticleData(particle.Flags);
                mFrame_OnDeadParticle_2348756594.SetWatchVariable("tmp_r_GetParticleData_2624031083", tmp_r_GetParticleData_2624031083);
                mFrame_OnDeadParticle_2348756594.SetWatchVariable("Condition0_1932775334", (tmp_r_GetParticleData_2624031083 == 0));
                breaker_if_1932775334.TryBreak();
                if ((tmp_r_GetParticleData_2624031083 == 0))
                {
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("v_flags_SetParticleFlags_43658510", EngineNS.Bricks.Particle.EParticleFlags.EmitShape);
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("v_data_SetParticleFlags_43658510", 1);
                    breaker_SetParticleFlags_43658510.TryBreak();
                    tmp_r_SetParticleFlags_43658510 = emt.SetParticleFlags(EngineNS.Bricks.Particle.EParticleFlags.EmitShape,1);
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("tmp_r_SetParticleFlags_43658510", tmp_r_SetParticleFlags_43658510);
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("v_num_Spawn_2821917696", 1);
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("v_flags_Spawn_2821917696", tmp_r_SetParticleFlags_43658510);
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("v_life_Spawn_2821917696", 5f);
                    breaker_Spawn_2821917696.TryBreak();
                    tmp_r_Spawn_2821917696 = emt.Spawn(1,tmp_r_SetParticleFlags_43658510,5f);
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("tmp_r_Spawn_2821917696", tmp_r_Spawn_2821917696);
                }
                else
                {
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("v_flags_SetParticleFlags_2645294717", EngineNS.Bricks.Particle.EParticleFlags.EmitShape);
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("v_data_SetParticleFlags_2645294717", 0);
                    breaker_SetParticleFlags_2645294717.TryBreak();
                    tmp_r_SetParticleFlags_2645294717 = emt.SetParticleFlags(EngineNS.Bricks.Particle.EParticleFlags.EmitShape,0);
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("tmp_r_SetParticleFlags_2645294717", tmp_r_SetParticleFlags_2645294717);
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("v_num_Spawn_4269439374", 1);
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("v_flags_Spawn_4269439374", tmp_r_SetParticleFlags_2645294717);
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("v_life_Spawn_4269439374", 3f);
                    breaker_Spawn_4269439374.TryBreak();
                    tmp_r_Spawn_4269439374 = emt.Spawn(1,tmp_r_SetParticleFlags_2645294717,3f);
                    mFrame_OnDeadParticle_2348756594.SetWatchVariable("tmp_r_Spawn_4269439374", tmp_r_Spawn_4269439374);
                }
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnInitParticle_127506005 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnInitParticle(EngineNS.Bricks.Particle.TtEmitter emt,ref EngineNS.Bricks.Particle.FParticle particle)
        {
            using(var guard_OnInitParticle = new EngineNS.Macross.TtMacrossStackGuard(mFrame_OnInitParticle_127506005))
            {
                mFrame_OnInitParticle_127506005.SetWatchVariable("emt", emt);
                mFrame_OnInitParticle_127506005.SetWatchVariable("particle", particle);
                System.Single tmp_r_RandomUnit_3537506712 = default(System.Single);
                System.Single tmp_r_RandomUnit_2843124104 = default(System.Single);
                particle.Velocity = emt.Velocity;
                mFrame_OnInitParticle_127506005.SetWatchVariable("particle", particle);
                breaker_RandomUnit_3537506712.TryBreak();
                tmp_r_RandomUnit_3537506712 = emt.RandomUnit(ref particle);
                mFrame_OnInitParticle_127506005.SetWatchVariable("tmp_r_RandomUnit_3537506712", tmp_r_RandomUnit_3537506712);
                particle.Life = (tmp_r_RandomUnit_3537506712 + particle.Life);
                mFrame_OnInitParticle_127506005.SetWatchVariable("particle", particle);
                breaker_RandomUnit_2843124104.TryBreak();
                tmp_r_RandomUnit_2843124104 = emt.RandomUnit(ref particle);
                mFrame_OnInitParticle_127506005.SetWatchVariable("tmp_r_RandomUnit_2843124104", tmp_r_RandomUnit_2843124104);
                particle.Color = (System.UInt32)(tmp_r_RandomUnit_2843124104);
                particle.Scale = 1f;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnParticleTick_56379435 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnParticleTick(EngineNS.Bricks.Particle.TtEmitter emt,System.Single elapsed,ref EngineNS.Bricks.Particle.FParticle particle)
        {
            using(var guard_OnParticleTick = new EngineNS.Macross.TtMacrossStackGuard(mFrame_OnParticleTick_56379435))
            {
                mFrame_OnParticleTick_56379435.SetWatchVariable("emt", emt);
                mFrame_OnParticleTick_56379435.SetWatchVariable("elapsed", elapsed);
                mFrame_OnParticleTick_56379435.SetWatchVariable("particle", particle);
                EngineNS.Color4f Angles = new EngineNS.Color4f(0,0,0,0);
                EngineNS.Color4f tmp_r_Uint2Color4f_3287025661 = default(EngineNS.Color4f);
                EngineNS.Color4f tmp_r_CreateColor4f_1754197827 = default(EngineNS.Color4f);
                System.UInt32 tmp_r_Color2Uint_1566520051 = default(System.UInt32);
                mFrame_OnParticleTick_56379435.SetWatchVariable("v_value_Uint2Color4f_3287025661", particle.Rotator);
                breaker_Uint2Color4f_3287025661.TryBreak();
                tmp_r_Uint2Color4f_3287025661 = emt.Uint2Color4f(particle.Rotator);
                mFrame_OnParticleTick_56379435.SetWatchVariable("tmp_r_Uint2Color4f_3287025661", tmp_r_Uint2Color4f_3287025661);
                mFrame_OnParticleTick_56379435.SetWatchVariable("v_r_CreateColor4f_1754197827", 0f);
                mFrame_OnParticleTick_56379435.SetWatchVariable("v_g_CreateColor4f_1754197827", (tmp_r_Uint2Color4f_3287025661.g + (elapsed * 1.14f)));
                mFrame_OnParticleTick_56379435.SetWatchVariable("v_b_CreateColor4f_1754197827", 0f);
                mFrame_OnParticleTick_56379435.SetWatchVariable("v_a_CreateColor4f_1754197827", 0f);
                breaker_CreateColor4f_1754197827.TryBreak();
                tmp_r_CreateColor4f_1754197827 = EngineNS.MathHelper.CreateColor4f(0f,(tmp_r_Uint2Color4f_3287025661.g + (elapsed * 1.14f)),0f,0f);
                mFrame_OnParticleTick_56379435.SetWatchVariable("tmp_r_CreateColor4f_1754197827", tmp_r_CreateColor4f_1754197827);
                mFrame_OnParticleTick_56379435.SetWatchVariable("v_color_Color2Uint_1566520051", tmp_r_CreateColor4f_1754197827);
                breaker_Color2Uint_1566520051.TryBreak();
                tmp_r_Color2Uint_1566520051 = emt.Color2Uint(tmp_r_CreateColor4f_1754197827);
                mFrame_OnParticleTick_56379435.SetWatchVariable("tmp_r_Color2Uint_1566520051", tmp_r_Color2Uint_1566520051);
                particle.Rotator = tmp_r_Color2Uint_1566520051;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnTimer_278687660 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnTimer(EngineNS.Bricks.Particle.TtEmitter emt,System.Single second)
        {
            using(var guard_OnTimer = new EngineNS.Macross.TtMacrossStackGuard(mFrame_OnTimer_278687660))
            {
                mFrame_OnTimer_278687660.SetWatchVariable("emt", emt);
                mFrame_OnTimer_278687660.SetWatchVariable("second", second);
            }
        }
    }
}
