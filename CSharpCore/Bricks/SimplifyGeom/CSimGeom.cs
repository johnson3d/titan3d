using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.SimplifyGeom
{
    public struct CConvexDecompDesc
    {
        public void SetDefault()
        {
            mDepth = 8;
            mCpercent = 0.1;
            mPpercent = 30;
            mMaxVertices = 64;
            mSkinWidth = 0;
            mVolumeSplitPercent = 0.1;
        }

        public UInt32 mDepth;    // depth to split, a maximum of 10, generally not over 7.
        public UInt32 mMaxVertices; // maximum number of vertices in the output hull. Recommended 32 or less.
        public double mCpercent; // the concavity threshold percentage.  0=20 is reasonable.
        public double mPpercent; // the percentage volume conservation threshold to collapse hulls. 0-30 is reasonable.
                                 // hull output limits.
        public double mVolumeSplitPercent;
        public double mSkinWidth;   // a skin width to apply to the output hulls.
    }
    public class CSimGeom : AuxCoreObject<CSimGeom.NativePointer>
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


        public CConvexDecompDesc CCDD = new CConvexDecompDesc();
        bool isChange = true;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public UInt32 Depth
        {
            get
            {
                return CCDD.mDepth;
            }
            set
            {
                isChange = true;
                CCDD.mDepth = value;
               // PreViewSimMesh();
            }
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public UInt32 MaxVertices
        {
            get
            {
                return CCDD.mMaxVertices;
            }
            set
            {
                isChange = true;
                CCDD.mMaxVertices = value;
                if (CCDD.mMaxVertices == 0 && Mesh != null)
                {
                    MeshPrimitives = Mesh.MeshPrimitives;
                }
                //PreViewSimMesh();
            }
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public double Cpercent
        {
            get
            {
                return CCDD.mCpercent;
            }
            set
            {
                isChange = true;
                CCDD.mCpercent = value;
                //PreViewSimMesh();
            }
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public double Ppercent
        {
            get
            {
                return CCDD.mPpercent;
            }
            set
            {
                isChange = true;
                CCDD.mPpercent = value;
               // PreViewSimMesh();
            }
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public double SkinWidth
        {
            get
            {
                return CCDD.mSkinWidth;
            }
            set
            {
                isChange = true;
                CCDD.mSkinWidth = value;
                //PreViewSimMesh();
            }
        }

        bool mPreview = false;
        public Graphics.Mesh.CGfxMeshPrimitives MeshPrimitives
        {
            get;
            set;
        } = null;


        public delegate void DelegatePreview(CSimGeom simgeom, bool show);
        public DelegatePreview DPreview;
        //bool isReady = true;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool Preview
        {
            get
            {
                return mPreview;
            }
            set
            {
                mPreview = value;
                if (DPreview != null)
                {
                    DPreview(this, mPreview);
                }
            }
        }

        public delegate void DelegateSavePhyMesh(Graphics.Mesh.CGfxMeshPrimitives MeshPrimitivesh, string type);
        public DelegateSavePhyMesh DSavePhyMesh;
        bool issaveconvex = false;
        [System.ComponentModel.Browsable(false)]
        public bool SaveConvexGeom
        {
            get
            {
                return issaveconvex;
            }
            set
            {
                issaveconvex = value;
                if (issaveconvex)
                {
                    if (DSavePhyMesh != null)
                    {
                        DSavePhyMesh((MaxVertices != 0 && MeshPrimitives != null) ? MeshPrimitives : Mesh.MeshPrimitives, "Convex");
                    }
                    //MeshPrimitives.CookAndSavePhyiscsGeomAsConvex(CEngine.Instance.RenderContext, EngineNS.CEngine.Instance.PhyContext);
                }
            }
        }

        bool issavetri = false;
        [System.ComponentModel.Browsable(false)]
        public bool SaveTriMesh
        {
            get
            {
                return issavetri;
            }
            set
            {
                issavetri = value;
                if (issavetri)
                {
                    if (DSavePhyMesh != null)
                    {
                        DSavePhyMesh((MaxVertices != 0 && MeshPrimitives != null) ? MeshPrimitives : Mesh.MeshPrimitives, "Triangle");
                    }
                    //var info = EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(mp.Value.Name.Address + CEngineDesc.PhysicsGeom);
                    //info.Save();

                }
            }
        }

        [System.ComponentModel.Browsable(false)]
        public bool IsChange
        {
            get
            {
                return isChange;
            }
            set
            {
                isChange = value;
            }
        }

        public EngineNS.Graphics.Mesh.CGfxMesh Mesh = null;
        public CSimGeom()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("SimGeom");
        }

        public bool BuildTriMesh(CRenderContext rc, Graphics.Mesh.CGfxMeshPrimitives mesh, ref CConvexDecompDesc desc, bool doConvex = true)
        {
            unsafe
            {
                mesh.PreUse(true);
                if (doConvex)
                {
                    fixed (CConvexDecompDesc* p = &desc)
                    {
                        return SDK_SimGeom_BuildTriMesh(CoreObject, rc.CoreObject, mesh.CoreObject, p);
                    }
                }
                else
                {
                    return SDK_SimGeom_BuildTriMesh(CoreObject, rc.CoreObject, mesh.CoreObject, (CConvexDecompDesc*)IntPtr.Zero);
                }
            }
        }
        public Graphics.Mesh.CGfxMeshPrimitives CreateMesh(CRenderContext rc)
        {
            var ptr = SDK_SimGeom_CreateMesh(CoreObject, rc.CoreObject);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;

            return new Graphics.Mesh.CGfxMeshPrimitives(ptr);
        }

        public UInt32 ConvexNum
        {
            get
            {
                return SDK_SimGeom_GetConvexNum(CoreObject);
            }
        }
        public Convex[] BuildConvex()
        {
            var num = ConvexNum;
            var result = new Convex[num];
            var pc = new IntPtr[num];
            for (UInt32 i = 0; i < num; i++)
            {
                result[i] = new Convex();
                pc[i] = result[i].CoreObject;
            }
            unsafe
            {
                fixed(IntPtr* ppConvex = &pc[0])
                {
                    SDK_SimGeom_BuildConvex(CoreObject, ppConvex, num);
                }
            }
            return result;
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_SimGeom_BuildTriMesh(NativePointer self, CRenderContext.NativePointer rc, Graphics.Mesh.CGfxMeshPrimitives.NativePointer mesh, CConvexDecompDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Graphics.Mesh.CGfxMeshPrimitives.NativePointer SDK_SimGeom_CreateMesh(NativePointer self, CRenderContext.NativePointer rc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static UInt32 SDK_SimGeom_GetConvexNum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_SimGeom_BuildConvex(NativePointer self, IntPtr* pConvex, UInt32 count); 
        #endregion
    }
}
