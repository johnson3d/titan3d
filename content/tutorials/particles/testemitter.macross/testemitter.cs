namespace NS_tutorials.particles
{
    [EngineNS.Macross.UMacross]
    public partial class testemitter : EngineNS.Bricks.Particle.TtEmitterMacross
    {
        EngineNS.Macross.UMacrossStackFrame mFrame_DoUpdateSystem = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void DoUpdateSystem(EngineNS.Bricks.Particle.TtEmitter emt)
        {
            using(var guard_DoUpdateSystem = new EngineNS.Macross.UMacrossStackGuard(mFrame_DoUpdateSystem))
            {
                System.UInt32 tmp_r_SetParticleFlags_4124938284 = default(System.UInt32);
                System.UInt32 tmp_r_Spawn_318270620 = default(System.UInt32);
                System.UInt32 v_oldValue_AtomicAdd_EmitterFlags_2788520947 = default(System.UInt32);
                emt.AtomicAdd_EmitterFlags(1,out v_oldValue_AtomicAdd_EmitterFlags_2788520947);
                if ((v_oldValue_AtomicAdd_EmitterFlags_2788520947 < 512))
                {
                    tmp_r_SetParticleFlags_4124938284 = emt.SetParticleFlags(EngineNS.Bricks.Particle.EParticleFlags.EmitShape,0);
                    tmp_r_Spawn_318270620 = emt.Spawn(1,tmp_r_SetParticleFlags_4124938284,3f);
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
                System.UInt32 tmp_r_GetParticleData_2624031083 = default(System.UInt32);
                System.UInt32 tmp_r_SetParticleFlags_43658510 = default(System.UInt32);
                System.UInt32 tmp_r_Spawn_2821917696 = default(System.UInt32);
                System.UInt32 tmp_r_SetParticleFlags_2645294717 = default(System.UInt32);
                System.UInt32 tmp_r_Spawn_4269439374 = default(System.UInt32);
                tmp_r_GetParticleData_2624031083 = emt.GetParticleData(particle.Flags);
                if ((tmp_r_GetParticleData_2624031083 == 0))
                {
                    tmp_r_SetParticleFlags_43658510 = emt.SetParticleFlags(EngineNS.Bricks.Particle.EParticleFlags.EmitShape,1);
                    tmp_r_Spawn_2821917696 = emt.Spawn(1,tmp_r_SetParticleFlags_43658510,5f);
                }
                else
                {
                    tmp_r_SetParticleFlags_2645294717 = emt.SetParticleFlags(EngineNS.Bricks.Particle.EParticleFlags.EmitShape,0);
                    tmp_r_Spawn_4269439374 = emt.Spawn(1,tmp_r_SetParticleFlags_2645294717,3f);
                }
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_OnInitParticle = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnInitParticle(EngineNS.Bricks.Particle.TtEmitter emt,ref EngineNS.Bricks.Particle.FParticle particle)
        {
            using(var guard_OnInitParticle = new EngineNS.Macross.UMacrossStackGuard(mFrame_OnInitParticle))
            {
                System.Single tmp_r_RandomUnit_3537506712 = default(System.Single);
                System.Single tmp_r_RandomUnit_2843124104 = default(System.Single);
                particle.Velocity = emt.Velocity;
                tmp_r_RandomUnit_3537506712 = emt.RandomUnit(ref particle);
                particle.Life = (tmp_r_RandomUnit_3537506712 + particle.Life);
                tmp_r_RandomUnit_2843124104 = emt.RandomUnit(ref particle);
                particle.Color = (System.UInt32)(tmp_r_RandomUnit_2843124104);
                particle.Scale = 1f;
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_OnParticleTick = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnParticleTick(EngineNS.Bricks.Particle.TtEmitter emt,System.Single elapsed,ref EngineNS.Bricks.Particle.FParticle particle)
        {
            using(var guard_OnParticleTick = new EngineNS.Macross.UMacrossStackGuard(mFrame_OnParticleTick))
            {
                EngineNS.Color4f Angles = new EngineNS.Color4f(0,0,0,0);
                EngineNS.Color4f tmp_r_Uint2Color4f_3287025661 = default(EngineNS.Color4f);
                EngineNS.Color4f tmp_r_CreateColor4f_1754197827 = default(EngineNS.Color4f);
                System.UInt32 tmp_r_Color2Uint_1566520051 = default(System.UInt32);
                tmp_r_Uint2Color4f_3287025661 = emt.Uint2Color4f(particle.Rotator);
                tmp_r_CreateColor4f_1754197827 = EngineNS.MathHelper.CreateColor4f(0f,(tmp_r_Uint2Color4f_3287025661.g + (elapsed * 1.14f)),0f,0f);
                tmp_r_Color2Uint_1566520051 = emt.Color2Uint(tmp_r_CreateColor4f_1754197827);
                particle.Rotator = tmp_r_Color2Uint_1566520051;
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_OnTimer = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/particles/testemitter.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void OnTimer(EngineNS.Bricks.Particle.TtEmitter emt,System.Single second)
        {
            using(var guard_OnTimer = new EngineNS.Macross.UMacrossStackGuard(mFrame_OnTimer))
            {
            }
        }
    }
}
