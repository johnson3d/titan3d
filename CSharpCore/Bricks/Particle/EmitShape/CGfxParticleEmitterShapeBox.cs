using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Particle.EmitShape
{
    [Editor.MacrossPanelPath("粒子系统/创建盒子形状的粒子发射器(Create CGfxParticleEmitterShapeBox)")]
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | Editor.Editor_MacrossClassAttribute.enMacrossType.Createable)]
    public partial class CGfxParticleEmitterShapeBox : CGfxParticleEmitterShape
    {
        public CGfxParticleEmitterShapeBox()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("GfxParticleEmitterShapeBox");
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float SizeX
        {
            get
            {
                return SDK_GfxParticleEmitterShapeBox_GetSizeX(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShapeBox_SetSizeX(CoreObject, value);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float SizeY
        {
            get
            {
                return SDK_GfxParticleEmitterShapeBox_GetSizeY(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShapeBox_SetSizeY(CoreObject, value);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float SizeZ
        {
            get
            {
                return SDK_GfxParticleEmitterShapeBox_GetSizeZ(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShapeBox_SetSizeZ(CoreObject, value);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxParticleEmitterShapeBox_GetSizeX(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShapeBox_SetSizeX(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxParticleEmitterShapeBox_GetSizeY(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShapeBox_SetSizeY(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxParticleEmitterShapeBox_GetSizeZ(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShapeBox_SetSizeZ(NativePointer self, float value);
        #endregion
    }
}
