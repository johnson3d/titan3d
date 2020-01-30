using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EngineNS.Bricks.PhysicsCore
{
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable)]
    public class CPhyMaterial : CPhyEntity
    {
        public static CPhyMaterial DefaultPhyMaterial
        {
            get { return CEngine.Instance.PhyContext.LoadMaterial(RName.GetRName("Physics/PhyMtl/default.phymtl")); }
        }
        public CPhyMaterial(NativePointer self) : base(self)
        {

        }
        public RName Name
        {
            get;
            set;
        }
        [DisplayName("动摩擦")]
        public float DynamicFriction
        {
            get
            {
                return SDK_PhyMaterial_GetDynamicFriction(CoreObject);
            }
            set
            {
                SDK_PhyMaterial_SetDynamicFriction(CoreObject, value);
            }
        }
        [DisplayName("静摩擦")]
        public float StaticFriction
        {
            get
            {
                return SDK_PhyMaterial_GetStaticFriction(CoreObject);
            }
            set
            {
                SDK_PhyMaterial_SetStaticFriction(CoreObject, value);
            }
        }
        [DisplayName("弹性恢复")]
        public float Restitution
        {
            get
            {
                return SDK_PhyMaterial_GetRestitution(CoreObject);
            }
            set
            {
                SDK_PhyMaterial_SetRestitution(CoreObject, value);
            }
        }

        public void Save2Xnd(RName name)
        {
            var xnd = IO.XndHolder.NewXNDHolder();
            var attrib = xnd.Node.AddAttrib("Params");
            attrib.BeginWrite();
            attrib.Write(StaticFriction);
            attrib.Write(DynamicFriction);
            attrib.Write(Restitution);
            attrib.EndWrite();

            IO.XndHolder.SaveXND(name.Address, xnd);
        }
        public static CPhyMaterial LoadXnd(CPhyContext ctx, IO.XndAttrib attr)
        {
            attr.BeginRead();
            float staticFriction;
            float dynamicFriction;
            float restitution;
            attr.Read(out staticFriction);
            attr.Read(out dynamicFriction);
            attr.Read(out restitution);
            attr.EndRead();
            return ctx.CreateMaterial(staticFriction, dynamicFriction, restitution);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static float SDK_PhyMaterial_GetDynamicFriction(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhyMaterial_SetDynamicFriction(NativePointer self, float v);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static float SDK_PhyMaterial_GetStaticFriction(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhyMaterial_SetStaticFriction(NativePointer self, float v);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static float SDK_PhyMaterial_GetRestitution(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhyMaterial_SetRestitution(NativePointer self, float v);
        #endregion
    }
}
