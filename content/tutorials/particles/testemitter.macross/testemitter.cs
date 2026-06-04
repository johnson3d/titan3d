namespace NS_tutorials.particles
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/particles/testemitter.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class testemitter : EngineNS.Bricks.Particle.TtEmitterMacross
    {
        public static EngineNS.Macross.TtMacrossBreak breaker_InterlockedAddUInt32_1872985343 = new EngineNS.Macross.TtMacrossBreak("breaker_InterlockedAddUInt32_1872985343");
        public static EngineNS.Macross.TtMacrossBreak breaker_SetParticleFlags_4124938284 = new EngineNS.Macross.TtMacrossBreak("breaker_SetParticleFlags_4124938284");
        public static EngineNS.Macross.TtMacrossBreak breaker_Spawn_318270620 = new EngineNS.Macross.TtMacrossBreak("breaker_Spawn_318270620");
        public static EngineNS.Macross.TtMacrossBreak breaker_if_801148221 = new EngineNS.Macross.TtMacrossBreak("breaker_if_801148221");
        public static EngineNS.Macross.TtMacrossBreak breaker_GetParticleData_2624031083 = new EngineNS.Macross.TtMacrossBreak("breaker_GetParticleData_2624031083");
        public static EngineNS.Macross.TtMacrossBreak breaker_SetParticleFlags_43658510 = new EngineNS.Macross.TtMacrossBreak("breaker_SetParticleFlags_43658510");
        public static EngineNS.Macross.TtMacrossBreak breaker_Spawn_2821917696 = new EngineNS.Macross.TtMacrossBreak("breaker_Spawn_2821917696");
        public static EngineNS.Macross.TtMacrossBreak breaker_if_1932775334 = new EngineNS.Macross.TtMacrossBreak("breaker_if_1932775334");
        public static EngineNS.Macross.TtMacrossBreak breaker_SetParticleFlags_2645294717 = new EngineNS.Macross.TtMacrossBreak("breaker_SetParticleFlags_2645294717");
        public static EngineNS.Macross.TtMacrossBreak breaker_Spawn_4269439374 = new EngineNS.Macross.TtMacrossBreak("breaker_Spawn_4269439374");
        public static EngineNS.Macross.TtMacrossBreak breaker_RandomUnit_3537506712 = new EngineNS.Macross.TtMacrossBreak("breaker_RandomUnit_3537506712");
        public static EngineNS.Macross.TtMacrossBreak breaker_RandomUnit_2843124104 = new EngineNS.Macross.TtMacrossBreak("breaker_RandomUnit_2843124104");
        public static EngineNS.Macross.TtMacrossBreak breaker_Uint2Color4f_3287025661 = new EngineNS.Macross.TtMacrossBreak("breaker_Uint2Color4f_3287025661");
        public static EngineNS.Macross.TtMacrossBreak breaker_CreateColor4f_1754197827 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateColor4f_1754197827");
        public static EngineNS.Macross.TtMacrossBreak breaker_Color2Uint_1566520051 = new EngineNS.Macross.TtMacrossBreak("breaker_Color2Uint_1566520051");
        EngineNS.Macross.TtMacrossStackFrame mFrame_DoUpdateSystem_3384864483 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_DoUpdateSystem_3384864483 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override void DoUpdateSystem(EngineNS.Bricks.Particle.TtEmitter emt)
        {
            using(var guard_DoUpdateSystem = new EngineNS.Macross.TtMacrossStackGuard(mStack_DoUpdateSystem_3384864483,mFrame_DoUpdateSystem_3384864483))
            {
                mFrame_DoUpdateSystem_3384864483.SetWatchVariable("emt", emt);
                System.UInt32 tmp_r_SetParticleFlags_4124938284 = default(System.UInt32);
                System.UInt32 tmp_r_Spawn_318270620 = default(System.UInt32);
                mFrame_DoUpdateSystem_3384864483.SetWatchVariable("Flags", emt.EmitterDataRef.mFlags);
                mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_value_InterlockedAddUInt32_1872985343", 1);
                System.UInt32 v_oriValue_InterlockedAddUInt32_1872985343 = default(System.UInt32);
                mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_oriValue_InterlockedAddUInt32_1872985343", v_oriValue_InterlockedAddUInt32_1872985343);
                breaker_InterlockedAddUInt32_1872985343.TryBreak(mStack_DoUpdateSystem_3384864483, this);
                EngineNS.Graphics.Pipeline.Shader.TtMacrossShaderUtility.InterlockedAddUInt32(ref emt.EmitterDataRef.mFlags,1,out v_oriValue_InterlockedAddUInt32_1872985343);
                mFrame_DoUpdateSystem_3384864483.SetWatchVariable("Condition0_801148221", (v_oriValue_InterlockedAddUInt32_1872985343 < 512));
                breaker_if_801148221.TryBreak(mStack_DoUpdateSystem_3384864483, this);
                if ((v_oriValue_InterlockedAddUInt32_1872985343 < 512))
                {
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_flags_SetParticleFlags_4124938284", EngineNS.Bricks.Particle.EParticleFlags.EmitShape);
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_data_SetParticleFlags_4124938284", 0);
                    breaker_SetParticleFlags_4124938284.TryBreak(mStack_DoUpdateSystem_3384864483, this);
                    tmp_r_SetParticleFlags_4124938284 = emt.SetParticleFlags(EngineNS.Bricks.Particle.EParticleFlags.EmitShape,0);
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("tmp_r_SetParticleFlags_4124938284", tmp_r_SetParticleFlags_4124938284);
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_num_Spawn_318270620", 1);
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_flags_Spawn_318270620", tmp_r_SetParticleFlags_4124938284);
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("v_life_Spawn_318270620", 3f);
                    breaker_Spawn_318270620.TryBreak(mStack_DoUpdateSystem_3384864483, this);
                    tmp_r_Spawn_318270620 = emt.Spawn(1,tmp_r_SetParticleFlags_4124938284,3f);
                    mFrame_DoUpdateSystem_3384864483.SetWatchVariable("tmp_r_Spawn_318270620", tmp_r_Spawn_318270620);
                }
                else
                {
                }
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnTimer_278687660 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_OnTimer_278687660 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override void OnTimer(EngineNS.Bricks.Particle.TtEmitter emt,System.Single second)
        {
            using(var guard_OnTimer = new EngineNS.Macross.TtMacrossStackGuard(mStack_OnTimer_278687660,mFrame_OnTimer_278687660))
            {
                mFrame_OnTimer_278687660.SetWatchVariable("emt", emt);
                mFrame_OnTimer_278687660.SetWatchVariable("second", second);
            }
        }
    }
}
