using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EngineNS.Bricks.Particle.EmitShape
{
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | Editor.Editor_MacrossClassAttribute.enMacrossType.Createable)]
    [Editor.MacrossPanelPath("粒子系统/创建粒子发射器形状(Create CGfxParticleEmitterShape)")]
    public partial class CGfxParticleEmitterShape : AuxCoreObject<CGfxParticleEmitterShape.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }

        public void InitAsGfxParticleEmitterShape(CGfxParticleSubState subState)
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("GfxParticleEmitterShape");
        }
        internal void SetEmitter(CGfxParticleSubState subState)
        {
            SDK_GfxParticleEmitterShape_SetEmitter(CoreObject, subState.CoreObject);
        }

        [Editor.Editor_PropertyGridSortIndex(2)]
        [Editor.DisplayParamName("是否启用从轮廓发射功能")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsEmitFromShell
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetIsEmitFromShell(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetIsEmitFromShell(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Browsable(false)]
        public bool IsRandomDirection
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetIsRandomDirection(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetIsRandomDirection(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Browsable(false)]
        public bool RandomDirAvailableX
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetRandomDirAvailableX(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetRandomDirAvailableX(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Browsable(false)]
        public bool RandomDirAvailableY
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetRandomDirAvailableY(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetRandomDirAvailableY(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Browsable(false)]
        public bool RandomDirAvailableZ
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetRandomDirAvailableZ(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetRandomDirAvailableZ(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Browsable(false)]
        public bool RandomDirAvailableInvX
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetRandomDirAvailableInvX(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetRandomDirAvailableInvX(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Browsable(false)]
        public bool RandomDirAvailableInvY
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetRandomDirAvailableInvY(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetRandomDirAvailableInvY(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Browsable(false)]
        public bool RandomDirAvailableInvZ
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetRandomDirAvailableInvZ(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetRandomDirAvailableInvZ(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Category("粒子发射位置 随机")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool RandomPosAvailableX
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetRandomPosAvailableX(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetRandomPosAvailableX(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Category("粒子发射位置 随机")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool RandomPosAvailableY
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetRandomPosAvailableY(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetRandomPosAvailableY(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Category("粒子发射位置 随机")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool RandomPosAvailableZ
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetRandomPosAvailableZ(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetRandomPosAvailableZ(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Category("粒子发射位置 随机")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool RandomPosAvailableInvX
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetRandomPosAvailableInvX(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetRandomPosAvailableInvX(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Category("粒子发射位置 随机")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool RandomPosAvailableInvY
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetRandomPosAvailableInvY(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetRandomPosAvailableInvY(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Category("粒子发射位置 随机")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool RandomPosAvailableInvZ
        {
            get
            {
                return SDK_GfxParticleEmitterShape_GetRandomPosAvailableInvZ(CoreObject);
            }
            set
            {
                SDK_GfxParticleEmitterShape_SetRandomPosAvailableInvZ(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Editor.Editor_PropertyGridSortIndex(1)]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public string Name
        {
            get;
            set;
        } = "ParticleEmitterShape";

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetEmitter(NativePointer self, CGfxParticleSubState.NativePointer pEmitter);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetIsEmitFromShell(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetIsEmitFromShell(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetIsRandomDirection(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetIsRandomDirection(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetRandomDirAvailableX(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetRandomDirAvailableX(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetRandomDirAvailableY(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetRandomDirAvailableY(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetRandomDirAvailableZ(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetRandomDirAvailableZ(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetRandomDirAvailableInvX(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetRandomDirAvailableInvX(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetRandomDirAvailableInvY(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetRandomDirAvailableInvY(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetRandomDirAvailableInvZ(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetRandomDirAvailableInvZ(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetRandomPosAvailableX(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetRandomPosAvailableX(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetRandomPosAvailableY(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetRandomPosAvailableY(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetRandomPosAvailableZ(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetRandomPosAvailableZ(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetRandomPosAvailableInvX(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetRandomPosAvailableInvX(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetRandomPosAvailableInvY(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetRandomPosAvailableInvY(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleEmitterShape_GetRandomPosAvailableInvZ(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleEmitterShape_SetRandomPosAvailableInvZ(NativePointer self, vBOOL value);
        #endregion
    }
    public class ParticleShape : ParticleBase
    {

    }
}
