using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Particle
{
    public partial class CGfxParticleModifier : Graphics.Mesh.CGfxModifier
    {
        public CGfxParticleModifier()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("GfxParticleModifier");
            Name = Rtti.RttiHelper.GetTypeSaveString(typeof(CGfxParticleModifier));
            ShaderModuleName = RName.GetRName("Shaders/Modifier/ParticleModifier");
        }
        public CGfxParticleModifier(NativePointer self)
        {
            mCoreObject = self;
            Name = Rtti.RttiHelper.GetTypeSaveString(typeof(CGfxParticleModifier));
            ShaderModuleName = RName.GetRName("Shaders/Modifier/ParticleModifier");
        }
        public override Graphics.Mesh.CGfxModifier CloneModifier(CRenderContext rc)
        {
            var result = new CGfxParticleModifier();
            result.Name = Name;
            return result;
        }
        public override string FunctionName
        {
            get { return "DoParticleModifierVS"; }
        }
        public CGfxParticleSystem ParticleSys
        {
            private set;
            get;
        } = new CGfxParticleSystem();

        public void SetParticleSystem(CGfxParticleSystem sys)
        {
            ParticleSys = sys;
        }
        public static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxParticleModifier), nameof(TickLogic));
        public override Profiler.TimeScope GetTickTimeLogicScope()
        {
            return ScopeTick;
        }

        public float Speed = 1.0f;
        partial void UpdateTime();
        public override void TickLogic(CRenderContext rc, Graphics.Mesh.CGfxMesh mesh, Int64 time)
        {
            if(ParticleSys.Effector!=null)
            {
                UpdateTime();
                ParticleSys.Effector.Get(false).Tick(rc.ImmCommandList, ParticleSys, CEngine.Instance.EngineElapseTimeSecond * Speed);
                if(mPass!=null)
                    mPass.SetInstanceNumber(ParticleSys.ParticleNumber);
            }
        }
        public CPass mPass;
        public override void OnSetPassData(CPass pass, bool shadow)
        {
            ParticleSys.BindParticleVB(pass);
            pass.SetInstanceNumber(ParticleSys.ParticleNumber);
            mPass = pass;
        }
    }
}
