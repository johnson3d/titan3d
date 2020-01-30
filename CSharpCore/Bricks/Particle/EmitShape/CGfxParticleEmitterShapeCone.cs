using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Particle.EmitShape
{
    [Editor.MacrossPanelPath("粒子系统/创建锥体形状的粒子发射器(Create CGfxParticleEmitterShapeCone)")]
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | Editor.Editor_MacrossClassAttribute.enMacrossType.Createable)]
    public partial class CGfxParticleEmitterShapeCone : CGfxParticleEmitterShape
    {
        public enum EDirectionType
        {
            EDT_ConeDirUp,
            EDT_ConeDirDown,
            EDT_EmitterDir,
            EDT_NormalOutDir,
            EDT_NormalInDir,
            EDT_OutDir,
            EDT_InDir,
        };
        public CGfxParticleEmitterShapeCone()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("GfxParticleEmitterShapeCone");
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Angle
        {
            get
            {
                return SDK_GfxParticleEmitterShapeCone_GetAngle(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShapeCone_SetAngle(CoreObject, value);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Radius
        {
            get
            {
                return SDK_GfxParticleEmitterShapeCone_GetRadius(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShapeCone_SetRadius(CoreObject, value);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Length
        {
            get
            {
                return SDK_GfxParticleEmitterShapeCone_GetLength(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShapeCone_SetLength(CoreObject, value);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EDirectionType DirType
        {
            get
            {
                return SDK_GfxParticleEmitterShapeCone_GetDirType(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShapeCone_SetDirType(CoreObject, value);
            }
        }
        
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxParticleEmitterShapeCone_GetAngle(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShapeCone_SetAngle(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxParticleEmitterShapeCone_GetRadius(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShapeCone_SetRadius(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxParticleEmitterShapeCone_GetLength(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShapeCone_SetLength(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static EDirectionType SDK_GfxParticleEmitterShapeCone_GetDirType(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShapeCone_SetDirType(NativePointer self, EDirectionType value);
        #endregion
    }
}
