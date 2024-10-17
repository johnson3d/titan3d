using System;
using System.Collections.Generic;
using System.Text;
using static EngineNS.Bricks.PhysicsCore.TtPhyPlaneShape;

namespace EngineNS.Bricks.PhysicsCore
{
    public class TtPhyShape : AuxPtrType<PhyShape>
    {
        public EPhysShapeType ShapeType 
        { 
            get
            {
                return mCoreObject.mType;
            }
        }
        public FTransform Transform
        {
            get
            {
                FTransform result = FTransform.Identity;
                Vector3 pos = new Vector3();
                mCoreObject.GetLocalPose(ref pos, ref result.mQuat);
                result.mPosition = pos.AsDVector();
                return result;
            }
        }
        public virtual BoundingBox GetAABB()
        {
            return BoundingBox.EmptyBox();
        }
        public class PGPxMaterialsAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            RName.PGRNameAttribute mRNameEditor;

            public PGPxMaterialsAttribute()
            {
                FullRedraw = false;
            }
            protected override async System.Threading.Tasks.Task<bool> Initialize_Override()
            {
                mRNameEditor = new RName.PGRNameAttribute();
                await mRNameEditor.Initialize();
                return await base.Initialize_Override();
            }
            protected override void Cleanup_Override()
            {
                mRNameEditor?.Cleanup();
                base.Cleanup_Override();
            }
            public override bool OnDraw(in EditorInfo info, out object newValue)
            {
                bool valueChanged = false;
                newValue = info.Value;
                var multiValue = newValue as EGui.Controls.PropertyGrid.PropertyMultiValue;
                if (multiValue != null)
                {
                    ImGuiAPI.Text(multiValue.MultiValueString);
                }
                else
                {
                    ImGuiAPI.Text(info.Type.ToString());
                    var lst = info.Value as List<TtPhyMaterial>;
                    if (lst != null)
                        Expandable = lst.Count > 0;
                    if (info.Expand)
                    {
                        if (OnArray(info, lst))
                        {
                            valueChanged = true;
                        }
                    }
                }

                return valueChanged;
            }
            private bool OnArray(EditorInfo info, List<TtPhyMaterial> materials)
            {
                if (materials == null)
                    return false;
                bool valueChanged = false;
                var sz = new Vector2(0, 0);
                ImGuiTableRowData rowData;
                unsafe
                {
                    rowData = new ImGuiTableRowData()
                    {
                        IndentTextureId = info.HostPropertyGrid.IndentDec.GetImagePtrPointer().ToPointer(),
                        MinHeight = 0,
                        CellPaddingYEnd = info.HostPropertyGrid.EndRowPadding,
                        CellPaddingYBegin = info.HostPropertyGrid.BeginRowPadding,
                        IndentImageWidth = info.HostPropertyGrid.Indent,
                        IndentTextureUVMin = Vector2.Zero,
                        IndentTextureUVMax = Vector2.One,
                        IndentColor = info.HostPropertyGrid.IndentColor,
                        HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                        Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
                    };
                }
                var umesh = info.ObjectInstance as TtPhyShape;
                ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None;
                for (int i = 0; i < materials.Count; i++)
                {
                    ImGuiAPI.TableNextRow(in rowData);

                    var name = i.ToString();
                    ImGuiAPI.TableSetColumnIndex(0);
                    ImGuiAPI.AlignTextToFramePadding();
                    var treeNodeRet = ImGuiAPI.TreeNodeEx(name, flags | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf, name);
                    ImGuiAPI.TableNextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var old = materials[i]?.AssetName;

                    mRNameEditor.FilterExts = TtPhyMaterial.AssetExt;
                    object newValue;
                    info.Value = materials[i]?.AssetName;
                    mRNameEditor.OnDraw(in info, out newValue);
                    RName rn = (RName)newValue;
                    if (rn != old)
                    {
                        materials[i] = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(rn);
                        var hostShape = info.ObjectInstance as TtPhyShape;
                        //if (hostShape != null)
                        hostShape.FlushMaterials();
                    }
                    if (treeNodeRet)
                        ImGuiAPI.TreePop();
                }
                return valueChanged;
            }
        }
        [PGPxMaterials]
        public List<TtPhyMaterial> Materials { get; set; }
        public unsafe void FlushMaterials()
        {
            if (Materials == null || Materials.Count == 0)
            {
                return;
            }
            PhyMaterial* pxMtls = stackalloc PhyMaterial[Materials.Count];
            for (int i = 0; i < Materials.Count; i++)
            {
                pxMtls[i] = Materials[i].mCoreObject;
            }
            mCoreObject.SetMaterials((PhyMaterial**)pxMtls, Materials.Count);
        }        
        protected Graphics.Mesh.TtMesh mDebugMesh;
        public virtual Graphics.Mesh.TtMesh DebugMesh
        {
            get
            {
                if (mDebugMesh == null)
                {
                    Graphics.Mesh.TtMeshPrimitives meshPrimitive = null;
                    switch (ShapeType)
                    {
                        case EPhysShapeType.PST_Plane:
                            break;
                        case EPhysShapeType.PST_Box:
                            {
                                var shape = this as TtPhyBoxShape;
                                meshPrimitive = Graphics.Mesh.TtMeshDataProvider.MakeBox(-shape.HalfExtent.X, -shape.HalfExtent.Y, -shape.HalfExtent.Z,
                                    shape.HalfExtent.X * 2.0f, shape.HalfExtent.Y * 2.0f, shape.HalfExtent.Z * 2.0f, 0xfffffff).ToMesh();
                            }
                            break;
                        case EPhysShapeType.PST_Sphere:
                            {
                                var shape = this as TtPhySphereShape;
                                meshPrimitive = Graphics.Mesh.TtMeshDataProvider.MakeSphere(shape.Radius, 20, 20, 0xfffffff).ToMesh();
                            }
                            break;
                        case EPhysShapeType.PST_Capsule:
                            {
                                var shape = this as TtPhyCapsuleShape;
                                meshPrimitive = Graphics.Mesh.TtMeshDataProvider.MakeCapsule(shape.Radius, shape.HalfHeight* 2, 10, 10, 100, Graphics.Mesh.TtMeshDataProvider.ECapsuleUvProfile.Aspect, 0xfffffff).ToMesh();
                            }
                            break;
                        case EPhysShapeType.PST_Convex:
                            break;
                        case EPhysShapeType.PST_TriangleMesh:
                            {
                                var shape = this as TtPhyTriMeshShape;
                                var triMesh = TtEngine.Instance.PhyModule.PhyContext.PhyMeshManager.GetMeshSync(shape.TriMeshSource);
                                if(triMesh!=null)
                                {
                                    meshPrimitive = triMesh.ToMeshProvider().ToMesh();
                                }
                            }
                            break;
                        case EPhysShapeType.PST_Unknown:
                            break;
                    }

                    if (meshPrimitive != null)
                    {
                        var matrials = new Graphics.Pipeline.Shader.TtMaterial[1];
                        matrials[0] = TtEngine.Instance.GfxDevice.MaterialManager.PxDebugMaterial;
                        var ShowMesh = new Graphics.Mesh.TtMaterialMesh();
                        ShowMesh.Initialize(new List<Graphics.Mesh.TtMeshPrimitives>() { meshPrimitive },
                                new List<Graphics.Pipeline.Shader.TtMaterial[]>() { matrials });
                        mDebugMesh = new Graphics.Mesh.TtMesh();
                        mDebugMesh.Initialize(ShowMesh, Rtti.TtTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                    }
                }
                return mDebugMesh;
            }
            set
            {
                mDebugMesh = value;
            }
        }
        
        public TtPhyShape(PhyShape self)
        {
            mCoreObject = self;
        }
        
        public void RemoveFromActor()
        {
            mCoreObject.RemoveFromActor();
        }

        public class TtShapeSerializer : IO.ISerializer
        {
            #region IO
            public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {

            }
            public virtual void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
            {

            }
            #endregion

            [Rtti.Meta]
            [RName.PGRName(FilterExts = TtPhyMaterial.AssetExt)]
            public RName PxMaterialName { get; set; }
        }
        public virtual TtShapeSerializer GetShapeSerializer()
        {
            return null;
        }
        public static TtPhyShape CreateShape(TtShapeSerializer sr)
        {
            var pc = TtEngine.Instance.PhyModule.PhyContext;
            var mtl = pc.PhyMaterialManager.GetMaterialSync(sr.PxMaterialName);
            if (sr.GetType() == typeof(TtPhyBoxShape.TtBoxSerializer))
            {
                var box = sr as TtPhyBoxShape.TtBoxSerializer;
                return pc.CreateShapeBox(mtl, box.HalfExtent);
            }
            else if (sr.GetType() == typeof(TtPhyTriMeshShape.TtTriMeshSerializer))
            {
                var mesh = sr as TtPhyTriMeshShape.TtTriMeshSerializer;
                var triMesh = TtEngine.Instance.PhyModule.PhyContext.PhyMeshManager.GetMeshSync(sr.PxMaterialName);
                return pc.CreateShapeTriMesh(mesh.Materials, triMesh, mesh.Scale, in Quaternion.Identity);
            }
            return null;
        }
    }
    public class TtPhyPlaneShape : TtPhyShape
    {
        public TtPhyPlaneShape(PhyShape self)
            : base(self)
        {

        }
        public override BoundingBox GetAABB()
        {
            return new BoundingBox(Vector3.Zero, 0);
        }
        public class TtPlaneSerializer : TtShapeSerializer
        {
        }
        public override TtShapeSerializer GetShapeSerializer()
        {
            var result = new TtPlaneSerializer();
            return result;
        }
    }
    public class TtPhyBoxShape : TtPhyShape
    {
        public TtPhyBoxShape(PhyShape self)
            : base(self)
        {

        }
        public override BoundingBox GetAABB()
        {
            var halfExt = HalfExtent;
            BoundingBox result = new BoundingBox(-halfExt, halfExt);

            return BoundingBox.TransformNoScale(result, Transform);
        }
        public Vector3 HalfExtent
        {
            get
            {
                Vector3 halfExtent = new Vector3();
                mCoreObject.IfGetBox(ref halfExtent);
                return halfExtent;
            }
            set
            {
                mCoreObject.IfSetBox(in value);
            }
        }

        public class TtBoxSerializer : TtShapeSerializer
        {
            [Rtti.Meta]
            public Vector3 HalfExtent { get; set; }
        }
        public override TtShapeSerializer GetShapeSerializer()
        {
            var result = new TtBoxSerializer();
            result.HalfExtent = HalfExtent;
            return result;
        }
    }
    public class TtPhySphereShape : TtPhyShape
    {
        public TtPhySphereShape(PhyShape self)
            : base(self)
        {

        }
        public override BoundingBox GetAABB()
        {
            var halfExt = new Vector3(Radius);
            BoundingBox result = new BoundingBox(-halfExt, halfExt);

            return BoundingBox.TransformNoScale(result, Transform);
        }
        public float Radius
        {
            get
            {
                float fRadius = 0;
                mCoreObject.IfGetSphere(ref fRadius);
                return fRadius;
            }
            set
            {
                mCoreObject.IfSetSphere(value);
            }
        }
        public class TtSphereSerializer : TtShapeSerializer
        {
            [Rtti.Meta]
            public float Radius { get; set; }
        }
        public override TtShapeSerializer GetShapeSerializer()
        {
            var result = new TtSphereSerializer();
            result.Radius = Radius;
            return result;
        }
    }
    public class TtPhyCapsuleShape : TtPhyShape
    {
        public TtPhyCapsuleShape(PhyShape self)
            : base(self)
        {

        }
        public float Radius
        {
            get
            {
                float fRadius = 0;
                float fHalfHeight = 0;
                mCoreObject.IfGetCapsule(ref fRadius, ref fHalfHeight);
                return fRadius;
            }
            set
            {
                mCoreObject.IfSetCapsule(value, HalfHeight);
            }
        }
        public float HalfHeight
        {
            get
            {
                float fRadius = 0;
                float fHalfHeight = 0;
                mCoreObject.IfGetCapsule(ref fRadius, ref fHalfHeight);
                return fRadius;
            }
            set
            {
                mCoreObject.IfSetCapsule(Radius, value);
            }
        }
        public class TtCapsuleSerializer : TtShapeSerializer
        {
            [Rtti.Meta]
            public float Radius { get; set; }
            [Rtti.Meta]
            public float HalfHeight { get; set; }
        }
        public override TtShapeSerializer GetShapeSerializer()
        {
            var result = new TtCapsuleSerializer();
            result.Radius = Radius;
            result.HalfHeight = HalfHeight;
            return result;
        }
    }
    public class TtPhyConvoxShape : TtPhyShape
    {
        public TtPhyConvoxShape(PhyShape self)
            : base(self)
        {

        }
        public class TtConvoxSerializer : TtShapeSerializer
        {
        }
        public override TtShapeSerializer GetShapeSerializer()
        {
            var result = new TtConvoxSerializer();
            return result;
        }
    }
    public class TtPhyTriMeshShape : TtPhyShape
    {
        public TtPhyTriMeshShape(PhyShape self)
            : base(self)
        {

        }
        public override BoundingBox GetAABB()
        {
            BoundingBox result = DebugMesh.MaterialMesh.AABB;

            return BoundingBox.TransformNoScale(result, Transform);
        }
        [RName.PGRName(FilterExts = TtPhyTriMesh.AssetExt)]
        public RName TriMeshSource { get; set; }
        public Vector3 Scale
        {
            get
            {
                Vector3 scale = new Vector3();
                Quaternion scaleRot = new Quaternion();
                mCoreObject.IfGetTriMeshScaling(ref scale, ref scaleRot);
                return scale;
            }
            set
            {
                mCoreObject.IfSetTriMeshScaling(in value, ScaleRotation);
            }
        }
        public Quaternion ScaleRotation
        {
            get
            {
                Vector3 scale = new Vector3();
                Quaternion scaleRot = new Quaternion();
                mCoreObject.IfGetTriMeshScaling(ref scale, ref scaleRot);
                return scaleRot;
            }
            set
            {
                mCoreObject.IfSetTriMeshScaling(Scale, in value);
            }
        }
        public class TtTriMeshSerializer : TtShapeSerializer
        {
            [RName.PGRName(FilterExts = TtPhyTriMesh.AssetExt)]
            [Rtti.Meta]
            public RName TriMeshSource { get; set; }
            [Rtti.Meta]
            [PGPxMaterials]
            public List<TtPhyMaterial> Materials { get; set; }
            [Rtti.Meta]
            public Vector3 Scale { get; set; }
        }
        public override TtShapeSerializer GetShapeSerializer()
        {
            var result = new TtTriMeshSerializer();
            result.TriMeshSource = TriMeshSource;
            result.Scale = Scale;
            return result;
        }
    }
    public class TtPhyHeightfieldShape : TtPhyShape
    {
        public TtPhyHeightfieldShape(PhyShape self)
            : base(self)
        {

        }
        public class TtHeightfieldSerializer : TtShapeSerializer
        {
            
        }
        public override TtShapeSerializer GetShapeSerializer()
        {
            var result = new TtHeightfieldSerializer();
            return result;
        }
    }
}
