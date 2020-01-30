using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Particle.EmitShape
{
    [Editor.MacrossPanelPath("粒子系统/创建球体形状的粒子发射器(Create CGfxParticleEmitterShapeSphere)")]
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | Editor.Editor_MacrossClassAttribute.enMacrossType.Createable)]
    public partial class CGfxParticleEmitterShapeSphere : CGfxParticleEmitterShape
    {
        public CGfxParticleEmitterShapeSphere()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("GfxParticleEmitterShapeSphere");
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Radius
        {
            get
            {
                return SDK_GfxParticleEmitterShapeSphere_GetRadius(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShapeSphere_SetRadius(CoreObject, value);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsRadialOutDirection
        {
            get
            {
                return SDK_GfxParticleEmitterShapeSphere_GetIsRadialOutDirection(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShapeSphere_SetIsRadialOutDirection(CoreObject, vBOOL.FromBoolean(value));
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsRadialInDirection
        {
            get
            {
                return SDK_GfxParticleEmitterShapeSphere_GetIsRadialInDirection(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShapeSphere_SetIsRadialInDirection(CoreObject, vBOOL.FromBoolean(value));
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsHemiSphere
        {
            get
            {
                return SDK_GfxParticleEmitterShapeSphere_GetIsHemiSphere(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShapeSphere_SetIsHemiSphere(CoreObject, vBOOL.FromBoolean(value));
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxParticleEmitterShapeSphere_GetRadius(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShapeSphere_SetRadius(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShapeSphere_GetIsRadialOutDirection(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShapeSphere_SetIsRadialOutDirection(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShapeSphere_GetIsRadialInDirection(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShapeSphere_SetIsRadialInDirection(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShapeSphere_GetIsHemiSphere(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShapeSphere_SetIsHemiSphere(NativePointer self, vBOOL value);
        #endregion
    }
}
