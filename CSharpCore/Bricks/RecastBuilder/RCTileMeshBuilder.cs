using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.IO.Serializer;
using EngineNS.IO;

namespace EngineNS.Bricks.RecastBuilder
{
    [Rtti.MetaClass]
    public class RecastBuilder : AuxCoreObject<RecastBuilder.NativePointer>, IO.Serializer.ISerializer
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

        //public struct OffMeshLinksData
        //{
        //    public EngineNS.Vector3 Position1;
        //    public EngineNS.Vector3 Position2;

        //    public EngineNS.Vector3 Lerp(float t)
        //    {
        //        return EngineNS.Vector3.Lerp(Position1, Position2, t);
        //    }
        //}
        //public List<OffMeshLinksData> OffMeshLinksPosition = new List<OffMeshLinksData>();

        #region ISerializer
        public virtual void BeforeRead() { }
        public virtual void BeforeWrite() { }

        public virtual void ReadObject(IReader pkg, Rtti.MetaData metaData)
        {
            SerializerHelper.ReadObject(this, pkg, metaData);
            //for (var i = 0; i < metaData.Members.Count; i++)
            //{
            //    var mbr = metaData.Members[i];
            //    if (mbr.IsList)
            //        mbr.Serializer.ReadValueList(this, mbr.PropInfo, pkg);
            //    else
            //        mbr.Serializer.ReadValue(this, mbr.PropInfo, pkg);
            //}
        }
        public void ReadObject(IReader pkg)
        {
            SerializerHelper.ReadObject(this, pkg);
        }

        public void WriteObject(IWriter pkg)
        {
            SerializerHelper.WriteObject(this, pkg);
        }
        public void WriteObject(IWriter pkg, Rtti.MetaData metaData)
        {
            SerializerHelper.WriteObject(this, pkg, metaData);
            //for (var i = 0; i < metaData.Members.Count; i++)
            //{
            //    var mbr = metaData.Members[i];
            //    if (mbr.IsList)
            //        mbr.Serializer.WriteValueList(this, mbr.PropInfo, pkg);
            //    else
            //        mbr.Serializer.WriteValue(this, mbr.PropInfo, pkg);
            //}
        }

        public virtual ISerializer CloneObject()
        {
            return SerializerHelper.CloneObject(this);
        }



        public void ReadObjectXML(XmlNode node)
        {
            SerializerHelper.ReadObjectXML(this, node);
        }

        public void WriteObjectXML(XmlNode node)
        {
            SerializerHelper.WriteObjectXML(this, node);
        }
        #endregion

        protected RCInputGeom mInputGeom;
        [System.ComponentModel.Browsable(false)]
        public RCInputGeom InputGeom
        {
            get { return mInputGeom; }
            set
            {
                mInputGeom = value;
                SDK_RecastBuilder_SetInputGeom(CoreObject, value.CoreObject);
            }
        }

        public RecastRuntime.CNavMesh BuildNavi()
        {
            var ptr = SDK_RecastBuilder_BuildNavi(CoreObject);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            return new RecastRuntime.CNavMesh(ptr);
        }

        private float mRaycastPhyHeight = 100;
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float RaycastPhyHeight
        {
            get
            {
                return mRaycastPhyHeight;
            }
            set
            {
                mRaycastPhyHeight = value;
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float CellSize
        {
            get
            {
                return SDK_RecastBuilder_GetCellSize(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetCellSize(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float CellHeight
        {
            get
            {
                return SDK_RecastBuilder_GetCellHeight(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetCellHeight(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float AgentHeight
        {
            get
            {
                return SDK_RecastBuilder_GetAgentHeight(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetAgentHeight(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float AgentRadius
        {
            get
            {
                return SDK_RecastBuilder_GetAgentRadius(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetAgentRadius(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float AgentMaxClimb
        {
            get
            {
                return SDK_RecastBuilder_GetAgentMaxClimb(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetAgentMaxClimb(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float RegionMinSize
        {
            get
            {
                return SDK_RecastBuilder_GetRegionMinSize(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetRegionMinSize(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float RegionMergeSize
        {
            get
            {
                return SDK_RecastBuilder_GetRegionMergeSize(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetRegionMergeSize(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float EdgeMaxLen
        {
            get
            {
                return SDK_RecastBuilder_GetEdgeMaxLen(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetEdgeMaxLen(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float EdgeMaxError
        {
            get
            {
                return SDK_RecastBuilder_GetEdgeMaxError(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetEdgeMaxError(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float VertsPerPoly
        {
            get
            {
                return SDK_RecastBuilder_GetVertsPerPoly(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetVertsPerPoly(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float DetailSampleDist
        {
            get
            {
                return SDK_RecastBuilder_GetDetailSampleDist(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetDetailSampleDist(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float DetailSampleMaxError
        {
            get
            {
                return SDK_RecastBuilder_GetDetailSampleMaxError(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetDetailSampleMaxError(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int PartitionType
        {
            get
            {
                return SDK_RecastBuilder_GetPartitionType(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetPartitionType(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 MinBox
        {
            get
            {
                return SDK_RecastBuilder_GetMinBox(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetMinBox(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 MaxBox
        {
            get
            {
                return SDK_RecastBuilder_GetMaxBox(CoreObject);
            }
            set
            {
                SDK_RecastBuilder_SetMaxBox(CoreObject, value);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetInputGeom(NativePointer self, RCInputGeom.NativePointer geom);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static RecastRuntime.CNavMesh.NativePointer SDK_RecastBuilder_BuildNavi(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_RecastBuilder_GetCellSize(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetCellSize(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_RecastBuilder_GetCellHeight(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetCellHeight(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_RecastBuilder_GetAgentHeight(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetAgentHeight(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_RecastBuilder_GetAgentRadius(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetAgentRadius(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_RecastBuilder_GetAgentMaxClimb(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetAgentMaxClimb(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_RecastBuilder_GetRegionMinSize(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetRegionMinSize(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_RecastBuilder_GetRegionMergeSize(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetRegionMergeSize(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_RecastBuilder_GetEdgeMaxLen(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetEdgeMaxLen(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_RecastBuilder_GetEdgeMaxError(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetEdgeMaxError(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_RecastBuilder_GetVertsPerPoly(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetVertsPerPoly(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_RecastBuilder_GetDetailSampleDist(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetDetailSampleDist(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_RecastBuilder_GetDetailSampleMaxError(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetDetailSampleMaxError(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_RecastBuilder_GetPartitionType(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetPartitionType(NativePointer self, int value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_RecastBuilder_GetMinBox(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetMinBox(NativePointer self, Vector3 value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_RecastBuilder_GetMaxBox(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RecastBuilder_SetMaxBox(NativePointer self, Vector3 value);
        #endregion
    }
    [Rtti.MetaClass]
    public class RCTileMeshBuilder : RecastBuilder
    {
        public RCTileMeshBuilder()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("TileMeshBuilder");
            TileSize = 4.0f;
        }
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float TileSize
        {
            get
            {
                return SDK_TileMeshBuilder_GetTileSize(CoreObject);
            }
            set
            {
                SDK_TileMeshBuilder_SetTileSize(CoreObject, value);
            }
        }

        public int mMaxNodes = 65535;//Limits: 0 < value <= 65535
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int MaxNodes
        {
            get
            {
                return mMaxNodes;
            }
            set
            {
                mMaxNodes = value;
            }
        }

        public Vector3 CorrectPosition(Vector3 pos)
        {
            return SDK_TileMeshBuilder_CorrectPosition(mCoreObject, pos);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_TileMeshBuilder_SetTileSize(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_TileMeshBuilder_GetTileSize(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_TileMeshBuilder_CorrectPosition(NativePointer self, Vector3 pos);
        #endregion
    }
    public class RCTileMeshHelper : BrickDescriptor
    {
        public override async System.Threading.Tasks.Task DoTest()
        {
            var builder = new RCTileMeshBuilder();
            var geom = new RCInputGeom();
            var rc = CEngine.Instance.RenderContext;
            var mesh = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(rc, RName.GetRName("Mesh/box.vms"), true);
            geom.LoadMesh(rc, mesh.GeometryMesh, 100.0f);
            builder.InputGeom = geom;
            var navMesh = builder.BuildNavi();
            navMesh.CreateRenderMeshPrimitives(rc);
            //navMesh.Save2Xnd();
            await base.DoTest();
        }
    }
}
