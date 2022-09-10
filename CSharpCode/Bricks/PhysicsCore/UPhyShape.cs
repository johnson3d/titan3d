using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class UPhyShape : AuxPtrType<PhyShape>
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
                    var lst = info.Value as UPhyMaterial[];
                    if (lst != null)
                        Expandable = lst.Length > 0;
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
            private bool OnArray(EditorInfo info, UPhyMaterial[] materials)
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
                var umesh = info.ObjectInstance as UPhyShape;
                ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None;
                for (int i = 0; i < materials.Length; i++)
                {
                    ImGuiAPI.TableNextRow(in rowData);

                    var name = i.ToString();
                    ImGuiAPI.TableSetColumnIndex(0);
                    ImGuiAPI.AlignTextToFramePadding();
                    var treeNodeRet = ImGuiAPI.TreeNodeEx(name, flags | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf, name);
                    ImGuiAPI.TableNextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var old = materials[i]?.AssetName;

                    mRNameEditor.FilterExts = UPhyMaterial.AssetExt;
                    object newValue;
                    info.Value = materials[i]?.AssetName;
                    mRNameEditor.OnDraw(in info, out newValue);
                    RName rn = (RName)newValue;
                    if (rn != old)
                    {
                        materials[i] = UEngine.Instance.PhyModue.PhyContext.PhyMaterialManager.GetMaterialSync(rn);
                        var hostShape = info.ObjectInstance as UPhyShape;
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
        public UPhyMaterial[] Materials { get; set; }
        public unsafe void FlushMaterials()
        {
            if (Materials == null || Materials.Length == 0)
            {
                return;
            }
            PhyMaterial* pxMtls = stackalloc PhyMaterial[Materials.Length];
            for (int i = 0; i < Materials.Length; i++)
            {
                pxMtls[i] = Materials[i].mCoreObject;
            }
            mCoreObject.SetMaterials((PhyMaterial**)pxMtls, Materials.Length);
        }        
        protected Graphics.Mesh.UMesh mDebugMesh;
        public virtual Graphics.Mesh.UMesh DebugMesh
        {
            get
            {
                if (mDebugMesh == null)
                {
                    Graphics.Mesh.UMeshPrimitives meshPrimitive = null;
                    switch (ShapeType)
                    {
                        case EPhysShapeType.PST_Plane:
                            break;
                        case EPhysShapeType.PST_Box:
                            {
                                var shape = this as UPhyBoxShape;
                                meshPrimitive = Graphics.Mesh.UMeshDataProvider.MakeBox(-shape.HalfExtent.X, -shape.HalfExtent.Y, -shape.HalfExtent.Z,
                                    shape.HalfExtent.X * 2.0f, shape.HalfExtent.Y * 2.0f, shape.HalfExtent.Z * 2.0f, 0xfffffff).ToMesh();
                            }
                            break;
                        case EPhysShapeType.PST_Sphere:
                            {
                                var shape = this as UPhySphereShape;
                                meshPrimitive = Graphics.Mesh.UMeshDataProvider.MakeSphere(shape.Radius, 20, 20, 0xfffffff).ToMesh();
                            }
                            break;
                        case EPhysShapeType.PST_Capsule:
                            {
                                var shape = this as UPhyCapsuleShape;
                                meshPrimitive = Graphics.Mesh.UMeshDataProvider.MakeCapsule(shape.Radius, shape.HalfHeight* 2, 10, 10, 100, Graphics.Mesh.UMeshDataProvider.ECapsuleUvProfile.Aspect, 0xfffffff).ToMesh();
                            }
                            break;
                        case EPhysShapeType.PST_Convex:
                            break;
                        case EPhysShapeType.PST_TriangleMesh:
                            {
                                var shape = this as UPhyTriMeshShape;
                                var triMesh = UEngine.Instance.PhyModue.PhyContext.PhyMeshManager.GetMeshSync(shape.TriMeshSource);
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
                        var matrials = new Graphics.Pipeline.Shader.UMaterial[1];
                        matrials[0] = UEngine.Instance.GfxDevice.MaterialManager.PxDebugMaterial;
                        var ShowMesh = new Graphics.Mesh.UMaterialMesh();
                        ShowMesh.Initialize(meshPrimitive, matrials);
                        mDebugMesh = new Graphics.Mesh.UMesh();
                        mDebugMesh.Initialize(ShowMesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                    }
                }
                return mDebugMesh;
            }
            set
            {
                mDebugMesh = value;
            }
        }
        
        public UPhyShape(PhyShape self)
        {
            mCoreObject = self;
        }
        
        public void RemoveFromActor()
        {
            mCoreObject.RemoveFromActor();
        }
    }
    public class UPhyPlaneShape : UPhyShape
    {
        public UPhyPlaneShape(PhyShape self)
            : base(self)
        {

        }
        public override BoundingBox GetAABB()
        {
            return new BoundingBox(Vector3.Zero, 0);
        }
    }
    public class UPhyBoxShape : UPhyShape
    {
        public UPhyBoxShape(PhyShape self)
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
    }
    public class UPhySphereShape : UPhyShape
    {
        public UPhySphereShape(PhyShape self)
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
    }
    public class UPhyCapsuleShape : UPhyShape
    {
        public UPhyCapsuleShape(PhyShape self)
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
    }
    public class UPhyConvoxShape : UPhyShape
    {
        public UPhyConvoxShape(PhyShape self)
            : base(self)
        {

        }
    }
    public class UPhyTriMeshShape : UPhyShape
    {
        public UPhyTriMeshShape(PhyShape self)
            : base(self)
        {

        }
        public override BoundingBox GetAABB()
        {
            BoundingBox result = DebugMesh.MaterialMesh.Mesh.mCoreObject.mAABB;

            return BoundingBox.TransformNoScale(result, Transform);
        }
        [RName.PGRName(FilterExts = UPhyTriMesh.AssetExt)]
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
    }
    public class UPhyHeightfieldShape : UPhyShape
    {
        public UPhyHeightfieldShape(PhyShape self)
            : base(self)
        {

        }
    }
}
