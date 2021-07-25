using EngineNS.Editor;
using EngineNS.GamePlay.Action;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public partial class UAxis : GamePlay.Action.IActionRecordable
    {
        public readonly static RName mAxisMaterial_Focus = RName.GetRName(@"axis\axis_focus_matins.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_Face_Focus = RName.GetRName(@"axis\axis_face_focus_matins.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_Center = RName.GetRName(@"axis\axis_all.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_X = RName.GetRName(@"axis\axis_x_matins.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_Y = RName.GetRName(@"axis\axis_y_matins.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_Z = RName.GetRName(@"axis\axis_z_matins.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_X_d = RName.GetRName(@"axis\axis_x_d.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_Y_d = RName.GetRName(@"axis\axis_y_d.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_Z_d = RName.GetRName(@"axis\axis_z_d.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_Focus_d = RName.GetRName(@"axis\axis_focus_d.uminst", RName.ERNameType.Game);
        //public readonly RName mAxisMaterial_TX = RName.GetRName(@"editor\axis\axismaterial_tx.instmtl", RName.ERNameType.Game);
        //public readonly RName mAxisMaterial_TY = RName.GetRName(@"editor\axis\axismaterial_ty.instmtl", RName.ERNameType.Game);
        //public readonly RName mAxisMaterial_TZ = RName.GetRName(@"editor\axis\axismaterial_tz.instmtl", RName.ERNameType.Game);

        public readonly static RName mAxisMeshMoveAll = RName.GetRName(@"axis\moveall.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshMoveX = RName.GetRName(@"axis\movex.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshMoveXY = RName.GetRName(@"axis\movexy.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshMoveXY_Line = RName.GetRName(@"axis\movexy_line.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshRotX = RName.GetRName(@"axis\rotx.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshScaleAll = RName.GetRName(@"axis\scaleall.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshScaleX = RName.GetRName(@"axis\scalex.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshScaleXY = RName.GetRName(@"axis\scalexy.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshScaleXY_Line = RName.GetRName(@"axis\scalexy_line.vms", RName.ERNameType.Game);

        public enum enAxisType
        {
            Null = -1,
            AxisStart = 0,
            Move_Start = 0,
            Move_X = Move_Start,
            Move_Y,
            Move_Z,
            //Move_XY,
            //Move_XZ,
            //Move_YX,
            //Move_YZ,
            //Move_ZX,
            //Move_ZY,
            Move_Line_XY,
            Move_Line_XZ,
            Move_Line_YZ,
            Move_Plane_XY,
            Move_Plane_XZ,
            Move_Plane_YZ,
            Move_XYZ,
            Move_End = Move_XYZ,

            Rot_Start,
            Rot_X = Rot_Start,
            Rot_Y,
            Rot_Z,
            //Rot_Plane_XY,
            //Rot_Plane_XZ,
            //Rot_Plane_YZ,
            Rot_End = Rot_Z,

            Scale_Start,
            Scale_X = Scale_Start,
            Scale_Y,
            Scale_Z,
            //Scale_XY,
            //Scale_XZ,
            //Scale_YX,
            //Scale_YZ,
            //Scale_ZX,
            //Scale_ZY,
            Scale_Line_XY,
            Scale_Line_XZ,
            Scale_Line_YZ,
            Scale_Plane_XY,
            Scale_Plane_XZ,
            Scale_Plane_YZ,
            Scale_XYZ,
            Scale_End = Scale_XYZ,

            Edge_Start,
            Edge_X_Min = Edge_Start,
            Edge_X_MinPlane,
            Edge_X_Max,
            Edge_X_MaxPlane,
            Edge_Y_Min,
            Edge_Y_MinPlane,
            Edge_Y_Max,
            Edge_Y_MaxPlane,
            Edge_Z_Min,
            Edge_Z_MinPlane,
            Edge_Z_Max,
            Edge_Z_MaxPlane,
            Edge_End = Edge_Z_MaxPlane,
            AxisEnd = Edge_End,
        }
        enAxisType mCurrentAxisType = enAxisType.Null;
        public enAxisType CurrentAxisType => mCurrentAxisType;

        public enum enAxisOperationType
        {
            Select = 0,
            Move = 1,
            Rot = 2,
            Scale = 3,
            Edge = 4,
        }
        enAxisOperationType mAxisOperationType = enAxisOperationType.Select;

        public enum enAxisSpace
        {
            World = 0,
            Local = 1,
        }
        enAxisSpace mAxisSpace = enAxisSpace.Local;
        public void SetAxisSpace(enAxisSpace space)
        {
            mAxisSpace = space;

        }

        enum enEdgeType
        {
            MinX = 0,
            MaxX = 1,
            MinY = 2,
            MaxY = 3,
            MinZ = 4,
            MaxZ = 5,
            Total = 6,
        }

        class UAxisNode : Scene.UNode
        {
            public UAxisNode(Scene.UNodeData data, Scene.EBoundVolumeType bvType, Type placementType)
                : base(data, bvType, placementType)
            {

            }

            public override bool DrawNode(UTreeNodeDrawer tree, int index)
            {
                return false;
            }
        }

        class AxisData
        {
            public Scene.UMeshNode MeshNode;
            public enAxisType AxisType;
            Graphics.Pipeline.Shader.UMaterial[] NormalMaterials;
            Graphics.Pipeline.Shader.UMaterial[] FocusMaterials;
            bool mFocused = false;
            public bool Focused
            {
                get => mFocused;
                set
                {
                    if (mFocused == value)
                        return;
                    mFocused = value;
                    if(mFocused)
                    {
                        for (int i = 0; i < FocusMaterials.Length; i++)
                            MeshNode.Mesh.MaterialMesh.Materials[i] = FocusMaterials[i];
                    }
                    else
                    {
                        for (int i = 0; i < NormalMaterials.Length; i++)
                            MeshNode.Mesh.MaterialMesh.Materials[i] = NormalMaterials[i];
                    }
                }
            }

            async System.Threading.Tasks.Task<Graphics.Mesh.UMesh> GetAxisMesh(RName meshName, params RName[] materialNames)
            {
                var materials = new Graphics.Pipeline.Shader.UMaterial[materialNames.Length];
                for (int i = 0; i < materialNames.Length; i++)
                {
                    materials[i] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(materialNames[i]);
                    if (materials[i] == null)
                        return null;
                }
                var mesh = new Graphics.Mesh.UMesh();
                var ok = await mesh.Initialize(meshName, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh_NoShadow>.TypeDesc);
                return ok ? mesh : null;
            }
            public async System.Threading.Tasks.Task Initialize(enAxisType type, GamePlay.UWorld world)
            {
                Graphics.Mesh.UMesh axisMesh = null;
                var meshNodeData = new GamePlay.Scene.UMeshNode.UMeshNodeData();
                Vector3 pos = Vector3.Zero;
                Quaternion rot = Quaternion.Identity;
                Vector3 scale = Vector3.UnitXYZ;
                AxisType = type;
                switch (AxisType)
                {
                    case enAxisType.Move_X:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveX, mAxisMaterial_X_d);
                        meshNodeData.MeshName = mAxisMeshMoveX;
                        meshNodeData.Name = mAxisMeshMoveX.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_X_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Move_Y:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveX, mAxisMaterial_Y_d);
                        rot = Quaternion.RotationAxis(Vector3.Backward, (float)(Math.PI * 0.5f));
                        meshNodeData.MeshName = mAxisMeshMoveX;
                        meshNodeData.Name = mAxisMeshMoveX.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Y_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Move_Z:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveX, mAxisMaterial_Z_d);
                        rot = Quaternion.RotationAxis(Vector3.Down, (float)(Math.PI * 0.5f));
                        meshNodeData.MeshName = mAxisMeshMoveX;
                        meshNodeData.Name = mAxisMeshMoveX.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Z_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Move_Line_XY:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveXY_Line, mAxisMaterial_X_d, mAxisMaterial_Y_d);
                        meshNodeData.MeshName = mAxisMeshMoveXY_Line;
                        meshNodeData.Name = mAxisMeshMoveXY_Line.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_X_d),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Y_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Move_Line_XZ:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveXY_Line, mAxisMaterial_X_d, mAxisMaterial_Z_d);
                        meshNodeData.MeshName = mAxisMeshMoveXY_Line;
                        meshNodeData.Name = mAxisMeshMoveXY_Line.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_X_d),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Z_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Move_Line_YZ:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveXY_Line, mAxisMaterial_Z_d, mAxisMaterial_Y_d);
                        meshNodeData.MeshName = mAxisMeshMoveXY_Line;
                        meshNodeData.Name = mAxisMeshMoveXY_Line.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Z_d),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Y_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Move_Plane_XY:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveXY, mAxisMaterial_Focus_d);
                        meshNodeData.MeshName = mAxisMeshMoveXY;
                        meshNodeData.Name = mAxisMeshMoveXY.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Move_Plane_XZ:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveXY, mAxisMaterial_Face_Focus);
                        meshNodeData.MeshName = mAxisMeshMoveXY;
                        meshNodeData.Name = mAxisMeshMoveXY.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Face_Focus),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Move_Plane_YZ:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveXY, mAxisMaterial_Face_Focus);
                        meshNodeData.MeshName = mAxisMeshMoveXY;
                        meshNodeData.Name = mAxisMeshMoveXY.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Face_Focus),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Move_XYZ:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveAll, mAxisMaterial_Center);
                        meshNodeData.MeshName = mAxisMeshMoveAll;
                        meshNodeData.Name = mAxisMeshMoveAll.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Center),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Rot_X:
                        axisMesh = await GetAxisMesh(mAxisMeshRotX, mAxisMaterial_X);
                        meshNodeData.MeshName = mAxisMeshRotX;
                        meshNodeData.Name = "AxisRotX";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_X),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                        };
                        break;
                    case enAxisType.Rot_Y:
                        axisMesh = await GetAxisMesh(mAxisMeshRotX, mAxisMaterial_Y);
                        rot = Quaternion.RotationAxis(Vector3.Backward, (float)(Math.PI * 0.5f));
                        meshNodeData.MeshName = mAxisMeshRotX;
                        meshNodeData.Name = "AxisRotY";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Y),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                        };
                        break;
                    case enAxisType.Rot_Z: 
                        axisMesh = await GetAxisMesh(mAxisMeshRotX, mAxisMaterial_Z);
                        rot = Quaternion.RotationAxis(Vector3.Down, (float)(Math.PI * 0.5f));
                        meshNodeData.MeshName = mAxisMeshRotX;
                        meshNodeData.Name = "AxisRotZ";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Z),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                        };
                        break;
                    //case enAxisType.Rot_Plane_XY: break;
                    //case enAxisType.Rot_Plane_XZ: break;
                    //case enAxisType.Rot_Plane_YZ: break;
                    case enAxisType.Scale_X:
                        axisMesh = await GetAxisMesh(mAxisMeshScaleX, mAxisMaterial_X_d);
                        meshNodeData.MeshName = mAxisMeshScaleX;
                        meshNodeData.Name = "AxisScaleX";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_X_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Scale_Y: 
                        axisMesh = await GetAxisMesh(mAxisMeshScaleX, mAxisMaterial_Y_d);
                        rot = Quaternion.RotationAxis(Vector3.Backward, (float)(Math.PI * 0.5f));
                        meshNodeData.MeshName = mAxisMeshScaleX;
                        meshNodeData.Name = "AxisScaleY";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Y_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Scale_Z:
                        axisMesh = await GetAxisMesh(mAxisMeshScaleX, mAxisMaterial_Z_d);
                        rot = Quaternion.RotationAxis(Vector3.Down, (float)(Math.PI * 0.5f));
                        meshNodeData.MeshName = mAxisMeshScaleX;
                        meshNodeData.Name = "AxisScaleZ";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Z_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Scale_Line_XY:
                        axisMesh = await GetAxisMesh(mAxisMeshScaleXY_Line, mAxisMaterial_X_d, mAxisMaterial_Y_d);
                        meshNodeData.MeshName = mAxisMeshScaleXY_Line;
                        meshNodeData.Name = "AxisScaleXY";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_X_d),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Y_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Scale_Line_XZ: 
                        axisMesh = await GetAxisMesh(mAxisMeshScaleXY_Line, mAxisMaterial_X_d, mAxisMaterial_Z_d);
                        meshNodeData.MeshName = mAxisMeshScaleXY_Line;
                        meshNodeData.Name = "AxisScaleXY";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_X_d),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Z_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Scale_Line_YZ: 
                        axisMesh = await GetAxisMesh(mAxisMeshScaleXY_Line, mAxisMaterial_Y_d, mAxisMaterial_Z_d);
                        meshNodeData.MeshName = mAxisMeshScaleXY_Line;
                        meshNodeData.Name = "AxisScaleXY";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Y_d),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Z_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Scale_Plane_XY:
                        axisMesh = await GetAxisMesh(mAxisMeshScaleXY, mAxisMaterial_Focus_d);
                        meshNodeData.MeshName = mAxisMeshScaleXY;
                        meshNodeData.Name = "AxisScaleXY";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Scale_Plane_XZ: 
                        axisMesh = await GetAxisMesh(mAxisMeshScaleXY, mAxisMaterial_Focus_d);
                        meshNodeData.MeshName = mAxisMeshScaleXY;
                        meshNodeData.Name = "AxisScaleYZ";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Scale_Plane_YZ:
                        axisMesh = await GetAxisMesh(mAxisMeshScaleXY, mAxisMaterial_Focus_d);
                        meshNodeData.MeshName = mAxisMeshScaleXY;
                        meshNodeData.Name = "AxisScaleYZ";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Scale_XYZ: 
                        axisMesh = await GetAxisMesh(mAxisMeshScaleAll, mAxisMaterial_Center);
                        meshNodeData.MeshName = mAxisMeshScaleAll;
                        meshNodeData.Name = "AxisScaleAll";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Center),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    case enAxisType.Edge_X_Min: break;
                    case enAxisType.Edge_X_MinPlane:  break;
                    case enAxisType.Edge_X_Max:  break;
                    case enAxisType.Edge_X_MaxPlane:  break;
                    case enAxisType.Edge_Y_Min:  break;
                    case enAxisType.Edge_Y_MinPlane:  break;
                    case enAxisType.Edge_Y_Max:  break;
                    case enAxisType.Edge_Y_MaxPlane:  break;
                    case enAxisType.Edge_Z_Min:  break;
                    case enAxisType.Edge_Z_MinPlane:  break;
                    case enAxisType.Edge_Z_Max:  break;
                    case enAxisType.Edge_Z_MaxPlane:  break;
                }

                MeshNode = (Scene.UMeshNode)world.Root.NewNode(typeof(Scene.UMeshNode), meshNodeData, Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
                MeshNode.SetStyle(Scene.UNode.ENodeStyles.HideBoundShape | Scene.UNode.ENodeStyles.NoPickedDraw);
                if(axisMesh != null)
                {
                    MeshNode.Mesh = axisMesh;
                    MeshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                    MeshNode.IsCastShadow = false;
                }
                var placement = MeshNode.Placement as GamePlay.UPlacement;
                placement.Position = pos;
                placement.Quat = rot;
                placement.Scale = scale;
            }
        }
        List<AxisData> mAxisMeshDatas = new List<AxisData>();

        #region CenterAxis
        BoundingBox mEdgeAxisBB = BoundingBox.EmptyBox();
        BoundingBox mEdgeAxisOrigionBBBeforeCenterOperation;
        Transform mAxisOrigionTransformBeforeCenterOperation;
        bool mCenterAxisMode = false;
        public bool CenterAxisMode
        {
            get => mCenterAxisMode;
            set
            {
                mCenterAxisMode = value;
                switch(mAxisOperationType)
                {
                    case enAxisOperationType.Edge:
                        {
                            mEdgeAxisOrigionBBBeforeCenterOperation = mEdgeAxisBB;
                            UpdateEdgeBB();
                            UpdateEdgeAxisTransform();
                        }
                        break;
                    default:
                        {
                            if(mRootNode != null)
                            {
                                if (mCenterAxisMode)
                                {
                                    mAxisOrigionTransformBeforeCenterOperation = ((UPlacement)mRootNode.Placement).TransformData;
                                    CenterAxisOperation();
                                }
                                else
                                {
                                    mRootNode.Placement.SetTransform(ref mAxisOrigionTransformBeforeCenterOperation);
                                }
                            }
                        }
                        break;
                }
            }
        }

        public UActionRecorder ActionRecorder 
        { 
            get; 
            set;
        }

        void CenterAxisOperation()
        {
            if (mRootNode == null)
                return;
            var camera = mCameraController.Camera;
            mRootNode.Placement.Position = camera.mCoreObject.GetPosition() + camera.mCoreObject.GetDirection() * 10;
        }
        void UpdateEdgeBB()
        {
            throw new InvalidOperationException("未实现");
        }
        void UpdateEdgeAxisTransform()
        {
            throw new InvalidOperationException("未实现");
        }
        #endregion

        GamePlay.UWorld mHostWorld;
        UAxisNode mRootNode;
        Scene.UMeshNode mRotArrowAssetNode;
        bool mInitialized = false;
        Graphics.Pipeline.ICameraController mCameraController;

        public async System.Threading.Tasks.Task Initialize(GamePlay.UWorld world, Graphics.Pipeline.ICameraController cameraController)
        {
            if (world == mHostWorld && mInitialized)
                return;

            mHostWorld = world;
            mCameraController = cameraController;

            for (var i=enAxisType.AxisStart; i<=enAxisType.AxisEnd; i++)
            {
                var axisData = new AxisData();
                await axisData.Initialize(i, mHostWorld);
                mAxisMeshDatas.Add(axisData);
            }

            mRootNode = (UAxisNode)world.Root.NewNode(typeof(UAxisNode),
                new GamePlay.Scene.UNodeData()
                {
                    Name = "AxisRootNode"
                },
                Scene.EBoundVolumeType.None, typeof(GamePlay.UPlacement));
            mRootNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
            mRootNode.IsCastShadow = false;
            mRootNode.Parent = world.Root;

            var rotArrowAssetMat = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d);
            var rotArrowAssetMesh = new Graphics.Mesh.UMesh();
            var ok = await rotArrowAssetMesh.Initialize(mAxisMeshMoveX, new Graphics.Pipeline.Shader.UMaterial[] { rotArrowAssetMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh_NoShadow>.TypeDesc);
            if(ok)
            {
                var meshNodeData = new GamePlay.Scene.UMeshNode.UMeshNodeData();
                meshNodeData.MeshName = mAxisMeshMoveX;
                meshNodeData.Name = "RotArrowAsset";
                mRotArrowAssetNode = (Scene.UMeshNode)world.Root.NewNode(typeof(Scene.UMeshNode), meshNodeData, Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
                mRotArrowAssetNode.SetStyle(Scene.UNode.ENodeStyles.HideBoundShape | Scene.UNode.ENodeStyles.NoPickedDraw);
                mRotArrowAssetNode.Mesh = rotArrowAssetMesh;
                mRotArrowAssetNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                mRotArrowAssetNode.IsCastShadow = false;
            }

            //await InitializeDebugAssit();

            mInitialized = true;

            /////////////////////////////////////
            SetAxisOperationType(enAxisOperationType.Select);
            /////////////////////////////////////
        }

        #region Debug
        Scene.UMeshNode mPlaneNode;
        Scene.UMeshNode mPointNode;
        async System.Threading.Tasks.Task InitializeDebugAssit()
        {
            var mesh = new Graphics.Mesh.UMesh();
            var plane = Graphics.Mesh.CMeshDataProvider.MakePlane(1, 1);
            var planeMesh = plane.ToMesh();
            var planeMaterial = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d);
            var ok = mesh.Initialize(
                planeMesh,
                new Graphics.Pipeline.Shader.UMaterial[] { planeMaterial },
                Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh_NoShadow>.TypeDesc);
            if(ok)
            {
                mPlaneNode = Scene.UMeshNode.AddMeshNode(
                    mHostWorld.Root,
                    new Scene.UMeshNode.UMeshNodeData(),
                    typeof(UPlacement),
                    mesh,
                    Vector3.Zero,
                    Vector3.One,
                    Quaternion.Identity);
                mPlaneNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
                mPlaneNode.NodeData.Name = "AxisDebugPlane";
                mPlaneNode.IsScaleChildren = false;
                mPlaneNode.IsCastShadow = false;
                mPlaneNode.Parent = mHostWorld.Root;
            }

            mesh = new Graphics.Mesh.UMesh();
            var point = Graphics.Mesh.CMeshDataProvider.MakeBox(-0.05f, -0.05f, -0.05f, 0.1f, 0.1f, 0.1f);
            var pointMesh = point.ToMesh();
            var pointMaterial = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Center);
            ok = mesh.Initialize(pointMesh, new Graphics.Pipeline.Shader.UMaterial[] { pointMaterial }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh_NoShadow>.TypeDesc);
            if(ok)
            {
                mPointNode = Scene.UMeshNode.AddMeshNode(
                    mHostWorld.Root,
                    new Scene.UMeshNode.UMeshNodeData(),
                    typeof(UPlacement),
                    mesh,
                    Vector3.Zero,
                    Vector3.One,
                    Quaternion.Identity);
                mPointNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
                mPointNode.NodeData.Name = "AxisDebugPoint";
                mPointNode.IsScaleChildren = false;
                mPointNode.IsCastShadow = false;
                mPointNode.Parent = mHostWorld.Root;
            }
        }
        #endregion

        class SelectedNodeData
        {
            public GamePlay.Scene.UNode Node;
            public Transform StartTransform = Transform.Identity;
        }
        List<SelectedNodeData> mSelectedNodes;
        List<SelectedNodeData> SelectedNodes
        {
            get => mSelectedNodes;
            set
            {
                GamePlay.Action.UAction.OnChanged(this, this, "SelectedNodes", mSelectedNodes, value);
                mSelectedNodes = value;
            }
        }
        public void SetSelectedNodes(params GamePlay.Scene.UNode[] nodes)
        {
            if (nodes != null)
            {
                var tempNodes = new List<SelectedNodeData>(nodes.Length);

                Vector3 axisPos = Vector3.Zero;
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i] == null)
                        continue;
                    var nodeData = new SelectedNodeData()
                    {
                        Node = nodes[i],
                    };
                    tempNodes.Add(nodeData);

                    axisPos += nodes[i].Placement.Position;
                }
                if(tempNodes.Count == 0)
                {
                    mRootNode.SetStyle(Scene.UNode.ENodeStyles.Invisible);
                }
                else
                {
                    SelectedNodes = tempNodes;
                    axisPos = axisPos / nodes.Length;
                    mRootNode.Placement.Position = axisPos;
                    switch(mAxisSpace)
                    {
                        case enAxisSpace.Local:
                            {
                                if(mSelectedNodes != null && mSelectedNodes.Count > 0)
                                {
                                    mRootNode.Placement.Quat = mSelectedNodes[mSelectedNodes.Count - 1].Node.Placement.Quat;
                                }
                            }
                            break;
                        case enAxisSpace.World:
                            mRootNode.Placement.Quat = Quaternion.Identity;
                            break;
                    }
                    mRootNode.Placement.Scale = Vector3.UnitXYZ;
                    mRootNode.UnsetStyle(Scene.UNode.ENodeStyles.Invisible);
                }
            }
            else
                SelectedNodes = null;
        }

        public void SetAxisOperationType(enAxisOperationType type)
        {
            for (int i = 0; i < mAxisMeshDatas.Count; i++)
                mAxisMeshDatas[i].MeshNode.Parent = null;

            mAxisOperationType = type;
            switch(mAxisOperationType)
            {
                case enAxisOperationType.Move:
                    {
                        for (int i = (int)enAxisType.Move_Start; i <= (int)enAxisType.Move_End; i++)
                        {
                            mAxisMeshDatas[i].MeshNode.Parent = mRootNode;
                        }
                    }
                    break;
                case enAxisOperationType.Rot:
                    {
                        for(int i=(int)enAxisType.Rot_Start; i <= (int)enAxisType.Rot_End; i++)
                        {
                            mAxisMeshDatas[i].MeshNode.Parent = mRootNode;
                        }
                    }
                    break;
                case enAxisOperationType.Scale:
                    {
                        for(int i=(int)enAxisType.Scale_Start; i <= (int)enAxisType.Scale_End; i++)
                        {
                            mAxisMeshDatas[i].MeshNode.Parent = mRootNode;
                        }
                    }
                    break;
                case enAxisOperationType.Edge:
                    {
                        for (int i = (int)enAxisType.Edge_Start; i <= (int)enAxisType.Edge_End; i++)
                        {
                            mAxisMeshDatas[i].MeshNode.Parent = mRootNode;
                        }
                    }
                    break;
            }
        }

        bool mCtrlKeyIsDown = false;
        bool mShiftKeyIsDown = false;
        bool mAltKeyIsDown = false;
        public void OnEvent(Graphics.Pipeline.UViewportSlate viewport, in SDL2.SDL.SDL_Event e)
        {
            //if(!mRootNode.HasStyle(Scene.UNode.ENodeStyles.Invisible))
            //{
            //    var camPos = mCameraController.Camera.mCoreObject.GetPosition();
            //    mRootNode.Placement.Scale = (mRootNode.Placement.Position - camPos).Length() * 0.1f * Vector3.UnitXYZ;
            //}
            switch (e.type)
            {
                case SDL2.SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    {
                        mFirstTransAxis = true;
                        if(e.button.button == SDL2.SDL.SDL_BUTTON_LEFT && mCurrentAxisType != enAxisType.Null)
                        {
                            StartTransAxis(viewport, e);
                        }
                    }
                    break;
                case SDL2.SDL.SDL_EventType.SDL_MOUSEMOTION:
                    {
                        if(!mIsTransAxisOperation)
                        {
                            var edtorPolicy = viewport.RenderPolicy as Graphics.Pipeline.Mobile.UMobileEditorFSPolicy;
                            if (edtorPolicy != null)
                            {
                                var pos = viewport.Window2Viewport(new Vector2((float)e.motion.x, (float)e.motion.y));
                                var hitObj = edtorPolicy.GetHitproxy((uint)pos.X, (uint)pos.Y);
                                if (hitObj != null)
                                {
                                    switch (mAxisOperationType)
                                    {
                                        case enAxisOperationType.Move:
                                            {
                                                for (int i = (int)enAxisType.Move_Start; i <= (int)enAxisType.Move_End; i++)
                                                {
                                                    if (mAxisMeshDatas[i].MeshNode.HitProxy.ProxyId == hitObj.HitProxy.ProxyId)
                                                    {
                                                        mAxisMeshDatas[i].Focused = true;
                                                        mCurrentAxisType = mAxisMeshDatas[i].AxisType;
                                                    }
                                                    else
                                                        mAxisMeshDatas[i].Focused = false;
                                                }
                                            }
                                            break;
                                        case enAxisOperationType.Rot:
                                            {
                                                for(int i=(int)enAxisType.Rot_Start; i <= (int)enAxisType.Rot_End; i++)
                                                {
                                                    if (mAxisMeshDatas[i].MeshNode.HitProxy.ProxyId == hitObj.HitProxy.ProxyId)
                                                    {
                                                        mAxisMeshDatas[i].Focused = true;
                                                        mCurrentAxisType = mAxisMeshDatas[i].AxisType;
                                                    }
                                                    else
                                                        mAxisMeshDatas[i].Focused = false;
                                                }
                                            }
                                            break;
                                        case enAxisOperationType.Scale:
                                            {
                                                for(int i=(int)enAxisType.Scale_Start; i <= (int)enAxisType.Scale_End; i++)
                                                {
                                                    if (mAxisMeshDatas[i].MeshNode.HitProxy.ProxyId == hitObj.HitProxy.ProxyId)
                                                    {
                                                        mAxisMeshDatas[i].Focused = true;
                                                        mCurrentAxisType = mAxisMeshDatas[i].AxisType;
                                                    }
                                                    else
                                                        mAxisMeshDatas[i].Focused = false;
                                                }
                                            }
                                            break;
                                        case enAxisOperationType.Edge:
                                            {
                                                for (int i = (int)enAxisType.Edge_Start; i <= (int)enAxisType.Edge_End; i++)
                                                {
                                                    if (mAxisMeshDatas[i].MeshNode.HitProxy.ProxyId == hitObj.HitProxy.ProxyId)
                                                    {
                                                        mAxisMeshDatas[i].Focused = true;
                                                        mCurrentAxisType = mAxisMeshDatas[i].AxisType;
                                                    }
                                                    else
                                                        mAxisMeshDatas[i].Focused = false;
                                                }
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    for (int i = (int)enAxisType.AxisStart; i < (int)enAxisType.AxisEnd; i++)
                                    {
                                        mAxisMeshDatas[i].Focused = false;
                                    }
                                    mCurrentAxisType = enAxisType.Null;
                                }
                            }
                        }

                        if (e.button.button == SDL2.SDL.SDL_BUTTON_LEFT)
                        {
                            var noUse = TransAxis(new Vector2(e.motion.x, e.motion.y));
                        }
                    }
                    break;
                case SDL2.SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    {
                        if (e.button.button == SDL2.SDL.SDL_BUTTON_LEFT)
                            EndTransAxis();
                    }
                    break;
                case SDL2.SDL.SDL_EventType.SDL_KEYDOWN:
                    {
                        if (e.key.keysym.sym == SDL2.SDL.SDL_Keycode.SDLK_LCTRL || e.key.keysym.sym == SDL2.SDL.SDL_Keycode.SDLK_RCTRL)
                            mCtrlKeyIsDown = true;
                        if (e.key.keysym.sym == SDL2.SDL.SDL_Keycode.SDLK_LSHIFT || e.key.keysym.sym == SDL2.SDL.SDL_Keycode.SDLK_RSHIFT)
                            mShiftKeyIsDown = true;
                        if (e.key.keysym.sym == SDL2.SDL.SDL_Keycode.SDLK_LALT || e.key.keysym.sym == SDL2.SDL.SDL_Keycode.SDLK_RALT)
                            mAltKeyIsDown = true;
                    }
                    break;
                case SDL2.SDL.SDL_EventType.SDL_KEYUP:
                    {
                        if (e.key.keysym.sym == SDL2.SDL.SDL_Keycode.SDLK_LCTRL || e.key.keysym.sym == SDL2.SDL.SDL_Keycode.SDLK_RCTRL)
                            mCtrlKeyIsDown = false;
                        if (e.key.keysym.sym == SDL2.SDL.SDL_Keycode.SDLK_LSHIFT || e.key.keysym.sym == SDL2.SDL.SDL_Keycode.SDLK_RSHIFT)
                            mShiftKeyIsDown = false;
                        if (e.key.keysym.sym == SDL2.SDL.SDL_Keycode.SDLK_LALT || e.key.keysym.sym == SDL2.SDL.SDL_Keycode.SDLK_RALT)
                            mAltKeyIsDown = false;
                    }
                    break;
            }

        }

        Plane mCheckPlane = new Plane();
        unsafe bool PickPlanePos(Graphics.Pipeline.UViewportSlate viewport, int x, int y, in EngineNS.Vector3 planePos, in EngineNS.Vector3 planeNormal, out EngineNS.Vector3 resultPos)
        {
            resultPos = EngineNS.Vector3.Zero;
            EngineNS.Vector3 pickRay = -EngineNS.Vector3.UnitY;
            var camera = mCameraController.Camera.mCoreObject;
            var pos = viewport.Window2Viewport(new Vector2(x, y));
            var pickResult = camera.GetPickRay(ref pickRay, pos.X, pos.Y, viewport.ClientSize.X, viewport.ClientSize.Y);
            if (pickResult <= 0)
                return false;
            var start = camera.GetPosition();

            mCheckPlane.Normal = planeNormal;
            mCheckPlane.D = -EngineNS.Vector3.Dot(planePos, planeNormal);
            var end = start + pickRay * 10000;
            fixed (Plane* pPlane = &mCheckPlane)
            fixed (Vector3* pHitPos = &resultPos)
            {
                if ((IntPtr)IDllImportApi.v3dxPlaneIntersectLine(pHitPos, pPlane, &start, &end) == IntPtr.Zero)
                    return false;
            }

            return true;
        }

        static bool LineInterCircle(in Vector3 ptStart, in Vector3 ptEnd, in Vector3 ptCenter, float Radius2,
           out Vector3 ptInter1, out Vector3 ptInter2)
        {
            ptInter1 = Vector3.Zero;
            ptInter2 = Vector3.Zero;
            float EPS = 0.00001f;
            //求线段的长度
            var d = ptEnd - ptStart;
            float fDis = d.Length();
            d.Normalize();
            var E = ptCenter - ptStart;
            float a;
            Vector3.Dot(ref E, ref d, out a);
            float a2 = a * a;
            float e2 = E.LengthSquared();
            if ((Radius2 - e2 + a2) < 0)
            {
                return false;
            }
            else
            {
                float f = (float)System.Math.Sqrt(Radius2 - e2 + a2);
                float t = a - f;
                if (((t - 0.0) > -EPS) && (t - fDis) < EPS)
                {
                    ptInter1 = ptStart + t * d;
                }
                t = a + f;
                if (((t - 0.0) > -EPS) && (t - fDis) < EPS)
                {
                    ptInter2 = ptStart + t * d;
                }
                return true;
            }
        }

        Transform mStartTransAxisWorldTransform;
        Transform mCurrentAxisStartTransform;
        BoundingBox mStartEdgeBB;
        Vector3 mMouseStartLocation;
        Vector3 mMouseTransOffset;
        Vector3 mMouseStartScreenLocation;
        bool mIsTransAxisOperation = false;
        void StartTransAxis(Graphics.Pipeline.UViewportSlate viewport, in SDL2.SDL.SDL_Event e)
        {
            if (!mInitialized)
                return;
            if (mCurrentAxisType == enAxisType.Null)
                return;

            mIsTransAxisOperation = true;

            switch (mAxisOperationType)
            {
                case enAxisOperationType.Edge:
                    {
                        if (CenterAxisMode)
                            mStartEdgeBB = mEdgeAxisOrigionBBBeforeCenterOperation;
                        else
                            mStartEdgeBB = mEdgeAxisBB;

                        mCurrentAxisStartTransform = ((UPlacement)mRootNode.Placement).TransformData;
                        // XXX
                    }
                    break;
                default:
                    {
                        if (CenterAxisMode)
                            mStartTransAxisWorldTransform = mAxisOrigionTransformBeforeCenterOperation;
                        else
                            mStartTransAxisWorldTransform = ((UPlacement)mRootNode.Placement).TransformData;

                        mCurrentAxisStartTransform = ((UPlacement)mRootNode.Placement).TransformData;
                    }
                    break;
            }

            var startWorldMat = Matrix.Transformation(mStartTransAxisWorldTransform.mScale, mStartTransAxisWorldTransform.mQuat, mStartTransAxisWorldTransform.mPosition);

            // 计算鼠标在3D空间中的点击位置
            Vector3 planeAxis = Vector3.UnitY;
            var cameraDirection = mCameraController.Camera.mCoreObject.GetDirection();
            switch(mCurrentAxisType)
            {
                case enAxisType.Move_X:
                case enAxisType.Scale_X:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitX, in startWorldMat, out transAxis);
                        var tempNormal = Vector3.Cross(transAxis, cameraDirection);
                        tempNormal.Normalize();
                        planeAxis = Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                        PickPlanePos(viewport, e.motion.x, e.motion.y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Rot_X:
                case enAxisType.Rot_Y:
                case enAxisType.Rot_Z:
                    {
                        Vector3 axisDir = Vector3.UnitY;
                        switch(mCurrentAxisType)
                        {
                            case enAxisType.Rot_X:
                                axisDir = Vector3.UnitX;
                                break;
                            case enAxisType.Rot_Y:
                                axisDir = Vector3.UnitY;
                                break;
                            case enAxisType.Rot_Z:
                                axisDir = Vector3.UnitZ;
                                break;
                        }
                        if (Vector3.Dot(cameraDirection, axisDir) == 0)
                        {
                            var camera = mCameraController.Camera.mCoreObject;

                            EngineNS.Vector3 pickRay = -EngineNS.Vector3.UnitY;
                            var pos = viewport.Window2Viewport(new Vector2(e.motion.x, e.motion.y));
                            var pickResult = camera.GetPickRay(ref pickRay, pos.X, pos.Y, viewport.ClientSize.X, viewport.ClientSize.Y);
                            pickRay.Normalize();
                            var camPos = camera.GetPosition();
                            var dirEnd = camPos + pickRay * 10000;
                            Vector3 inter1, inter2;
                            if (LineInterCircle(in camPos, in dirEnd, in mCurrentAxisStartTransform.mPosition, 2.0f, out inter1, out inter2))
                            {
                                mMouseStartLocation = (inter1 - camPos).Length() < (inter2 - camPos).Length() ? inter1 : inter2;
                            }
                        }
                        else
                        {
                            EngineNS.Vector3.TransformNormal(in axisDir, in startWorldMat, out planeAxis);
                            PickPlanePos(viewport, e.motion.x, e.motion.y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                        }
                    }
                    break;
                case enAxisType.Move_Line_XY:
                case enAxisType.Move_Plane_XY:
                case enAxisType.Scale_Line_XY:
                case enAxisType.Scale_Plane_XY:
                    {
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitZ, in startWorldMat, out planeAxis);
                        PickPlanePos(viewport, e.motion.x, e.motion.y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Move_Y:
                case enAxisType.Scale_Y:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitY, in startWorldMat, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, cameraDirection);
                        tempNormal.Normalize();
                        planeAxis = EngineNS.Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                        PickPlanePos(viewport, e.motion.x, e.motion.y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Move_Line_XZ:
                case enAxisType.Move_Plane_XZ:
                case enAxisType.Scale_Line_XZ:
                case enAxisType.Scale_Plane_XZ:
                    {
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitY, in startWorldMat, out planeAxis);
                        PickPlanePos(viewport, e.motion.x, e.motion.y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Move_Z:
                case enAxisType.Scale_Z:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitZ, in startWorldMat, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, cameraDirection);
                        tempNormal.Normalize();
                        planeAxis = EngineNS.Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                        PickPlanePos(viewport, e.motion.x, e.motion.y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Move_Line_YZ:
                case enAxisType.Move_Plane_YZ:
                case enAxisType.Scale_Line_YZ:
                case enAxisType.Scale_Plane_YZ:
                    {
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitX, in startWorldMat, out planeAxis);
                        PickPlanePos(viewport, e.motion.x, e.motion.y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Scale_XYZ:
                    {
                        planeAxis = -cameraDirection;
                        PickPlanePos(viewport, e.motion.x, e.motion.y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Edge_X_Min:
                case enAxisType.Edge_X_MinPlane:
                case enAxisType.Edge_X_Max:
                case enAxisType.Edge_X_MaxPlane:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitX, in startWorldMat, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, cameraDirection);
                        tempNormal.Normalize();
                        planeAxis = EngineNS.Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                        PickPlanePos(viewport, e.motion.x, e.motion.y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Edge_Y_Min:
                case enAxisType.Edge_Y_MinPlane:
                case enAxisType.Edge_Y_Max:
                case enAxisType.Edge_Y_MaxPlane:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitY, in startWorldMat, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, cameraDirection);
                        tempNormal.Normalize();
                        planeAxis = EngineNS.Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                        PickPlanePos(viewport, e.motion.x, e.motion.y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Edge_Z_Min:
                case enAxisType.Edge_Z_MinPlane:
                case enAxisType.Edge_Z_Max:
                case enAxisType.Edge_Z_MaxPlane:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitZ, in startWorldMat, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, cameraDirection);
                        tempNormal.Normalize();
                        planeAxis = EngineNS.Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                        PickPlanePos(viewport, e.motion.x, e.motion.y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
            }

            mMouseStartScreenLocation = new Vector3(e.motion.x, e.motion.y, 0.0f);
            #region Debug
            if (mPlaneNode != null)
            {
                mPlaneNode.Placement.Position = mMouseStartLocation;
                mPlaneNode.Placement.Quat = Quaternion.GetQuaternion(Vector3.Up, planeAxis);

                mPointNode.Placement.Position = mMouseStartLocation;
            }
            #endregion
            mMouseTransOffset = mMouseStartLocation - mCurrentAxisStartTransform.mPosition;

            if(mSelectedNodes != null)
            {
                for (int i = 0; i < mSelectedNodes.Count; i++)
                {
                    mSelectedNodes[i].StartTransform = ((UPlacement)mSelectedNodes[i].Node.Placement).TransformData;
                }
            }
        }

        bool mFirstTransAxis = false;
        async System.Threading.Tasks.Task TransAxis(Vector2 newMouseLoc)
        {
            if (!mInitialized)
                return;
            if (!mIsTransAxisOperation)
                return;

            if(mFirstTransAxis)
            {
                if(mShiftKeyIsDown)
                {
                    // todo: 复制
                    await Thread.AsyncDummyClass.DummyFunc();
                }
                mFirstTransAxis = false;
            }

            var startTransMat = Matrix.Transformation(mStartTransAxisWorldTransform.Scale, mStartTransAxisWorldTransform.Quat, mStartTransAxisWorldTransform.Position);

            switch(mCurrentAxisType)
            {
                case enAxisType.Move_X:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitX, in startTransMat, out transAxis);
                        MoveWithAxis(in newMouseLoc, transAxis);
                    }
                    break;
                case enAxisType.Move_Y:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitY, in startTransMat, out transAxis);
                        MoveWithAxis(in newMouseLoc, transAxis);
                    }
                    break;
                case enAxisType.Move_Z:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitZ, in startTransMat, out transAxis);
                        MoveWithAxis(in newMouseLoc, transAxis);
                    }
                    break;
                case enAxisType.Move_Line_XY:
                case enAxisType.Move_Plane_XY:
                    break;
                case enAxisType.Move_Line_XZ:
                case enAxisType.Move_Plane_XZ:
                    break;
                case enAxisType.Move_Line_YZ:
                case enAxisType.Move_Plane_YZ:
                    break;
                case enAxisType.Rot_X:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitX, in startTransMat, out transAxis);
                        RotWithAxis(in newMouseLoc, in transAxis);
                    }
                    break;
                case enAxisType.Rot_Y:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitY, in startTransMat, out transAxis);
                        RotWithAxis(in newMouseLoc, in transAxis);
                    }
                    break;
                case enAxisType.Rot_Z:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitZ, in startTransMat, out transAxis);
                        RotWithAxis(in newMouseLoc, in transAxis);
                    }
                    break;
                case enAxisType.Scale_X:
                    {
                        //Vector3 transAxis;
                        //Vector3.TransformNormal(in Vector3.UnitX, in startTransMat, out transAxis);
                        //ScaleWithAxis(in newMouseLoc, in transAxis);
                        ScaleWithAxis(in newMouseLoc, in Vector3.UnitX);
                    }
                    break;
                case enAxisType.Scale_Y:
                    {
                        ScaleWithAxis(in newMouseLoc, in Vector3.UnitY);
                    }
                    break;
                case enAxisType.Scale_Z:
                    {
                        ScaleWithAxis(in newMouseLoc, in Vector3.UnitZ);
                    }
                    break;
                case enAxisType.Scale_XYZ:
                    {
                        ScaleWithAxis(in newMouseLoc, in Vector3.UnitXYZ);
                    }
                    break;
            }
        }
        void MoveWithAxis(in Vector2 newMouseLoc, in Vector3 transAxisDir)
        {
            transAxisDir.Normalize();
            var camera = mCameraController.Camera.mCoreObject;
            if(camera.mIsOrtho)
            {
                throw new InvalidOperationException("没实现");
            }
            else
            {
                var axisStartTransPos = mRootNode.Placement.Position + mMouseTransOffset;
                var c2vMat = camera.GetToViewPortMatrix();
                Vector3 screenAxisLoc;
                var curAxisPosition = axisStartTransPos;
                Vector3.TransformCoordinate(ref curAxisPosition, ref c2vMat, out screenAxisLoc);
                screenAxisLoc.Z = 0;
                var tag = transAxisDir + axisStartTransPos;
                Vector3 screenTag;
                Vector3.TransformCoordinate(ref tag, ref c2vMat, out screenTag);
                screenTag.Z = 0;
                var screenAxisDir = screenTag - screenAxisLoc;
                screenAxisDir.Normalize();

                var camRight = camera.GetRight();
                camRight.Normalize();
                var deltaPos = curAxisPosition + camRight;
                Vector3 screenDeltaPos;
                Vector3.TransformCoordinate(ref deltaPos, ref c2vMat, out screenDeltaPos);
                screenDeltaPos.Z = 0;
                var s2vDelta = camRight.Length() / (screenDeltaPos - screenAxisLoc).Length();
                var lenInScreen = Vector3.Dot((new Vector3(newMouseLoc.X, newMouseLoc.Y, 0) - mMouseStartScreenLocation), screenAxisDir);
                var len = lenInScreen * s2vDelta;

                #region Debug
                //mPlaneNode.Placement.Scale = new Vector3(len, 1, 1);
                //mPlaneNode.Placement.Position = transAxisDir * len * 0.5f + axisStartTransPos;
                #endregion

                var trans = transAxisDir * len;
                var mat = EngineNS.Matrix.Translate(trans);
                if (CenterAxisMode)
                {
                    Matrix startTransAxisMat;
                    Matrix.Transformation(ref mStartTransAxisWorldTransform.mScale, ref mStartTransAxisWorldTransform.mQuat, ref mStartTransAxisWorldTransform.mPosition, out startTransAxisMat);
                    var axisOrigionTransBeforeCenterMat = startTransAxisMat * mat;
                    axisOrigionTransBeforeCenterMat.Decompose(out mAxisOrigionTransformBeforeCenterOperation.mScale, out mAxisOrigionTransformBeforeCenterOperation.mQuat, out mAxisOrigionTransformBeforeCenterOperation.mPosition);

                    mRootNode.Placement.Position = trans + mCurrentAxisStartTransform.Position;
                }
                else
                {
                    mRootNode.Placement.Position = trans + mStartTransAxisWorldTransform.Position;
                }

                if(mSelectedNodes != null)
                {
                    for (int i = 0; i < mSelectedNodes.Count; i++)
                    {
                        var data = mSelectedNodes[i];
                        Matrix startTransMatrix;
                        Matrix.Transformation(ref data.StartTransform.mScale, ref data.StartTransform.mQuat, ref data.StartTransform.mPosition, out startTransMatrix);

                        var transMat = startTransMatrix * mat;
                        if (data.Node.Parent != null)
                        {
                            transMat = transMat * data.Node.Parent.Placement.AbsTransformInv;
                        }
                        Vector3 pos, scale;
                        Quaternion rot;
                        transMat.Decompose(out scale, out rot, out pos);
                        data.Node.Placement.Position = pos;
                        data.Node.Placement.Quat = rot;
                        data.Node.Placement.Scale = scale;
                    }
                }
            }
        }
        void RotWithAxis(in Vector2 newMouseLoc, in Vector3 rotAxisDir)
        {
            rotAxisDir.Normalize();
            var camera = mCameraController.Camera.mCoreObject;
            if(camera.mIsOrtho)
            {
                throw new InvalidOperationException("没实现");
            }
            else
            {
                var c2vMat = camera.GetToViewPortMatrix();
                var tangentDir = Vector3.Cross(rotAxisDir, mMouseStartLocation - mCurrentAxisStartTransform.Position);
                tangentDir.Normalize();

                var camDir = camera.GetDirection();
                camDir.Normalize();
                var camRight = camera.GetRight();
                camRight.Normalize();
                var camUp = camera.GetUp();
                camUp.Normalize();
                Vector3 screenMouseStartLoc;
                Vector3.TransformCoordinate(ref mMouseStartLocation, ref c2vMat, out screenMouseStartLoc);
                var tangTag = tangentDir + mMouseStartLocation;
                Vector3 screenTangTag;
                Vector3.TransformCoordinate(ref tangTag, ref c2vMat, out screenTangTag);
                screenTangTag.Z = 0;
                var screenTangDir = screenTangTag - screenMouseStartLoc;
                screenTangDir.Normalize();

                var deltaPos = mMouseStartLocation + camRight;
                Vector3 screenDeltaPos;
                Vector3.TransformCoordinate(ref deltaPos, ref c2vMat, out screenDeltaPos);
                screenDeltaPos.Z = 0;
                screenDeltaPos = screenDeltaPos - screenMouseStartLoc;
                var s2vDelta = camRight.Length() / screenDeltaPos.Length();

                var lenInScreen = Vector3.Dot((new Vector3(newMouseLoc.X, newMouseLoc.Y, 0) - mMouseStartScreenLocation), screenTangDir);
                var len = lenInScreen * s2vDelta;

                var radius = (mMouseStartLocation - mCurrentAxisStartTransform.Position).Length();
                var angle = len / radius;
                var mat = Matrix.Translate(-mStartTransAxisWorldTransform.mPosition) * Matrix.RotationAxis(rotAxisDir, angle) * Matrix.Translate(mStartTransAxisWorldTransform.mPosition);
                if(CenterAxisMode)
                {
                    Matrix startTransAxisMat;
                    Matrix.Transformation(ref mStartTransAxisWorldTransform.mScale, ref mStartTransAxisWorldTransform.mQuat, ref mStartTransAxisWorldTransform.mPosition, out startTransAxisMat);
                    var axisOrigionTransBeforeCenterMat = startTransAxisMat * mat;
                    axisOrigionTransBeforeCenterMat.Decompose(out mAxisOrigionTransformBeforeCenterOperation.mScale, out mAxisOrigionTransformBeforeCenterOperation.mQuat, out mAxisOrigionTransformBeforeCenterOperation.mPosition);

                    var tempMat = Matrix.Translate(-mCurrentAxisStartTransform.Position) * Matrix.RotationAxis(rotAxisDir, angle) * Matrix.Translate(mCurrentAxisStartTransform.Position);
                    tempMat = startTransAxisMat * tempMat;
                    Vector3 tPos, tScale;
                    Quaternion tRot;
                    tempMat.Decompose(out tScale, out tRot, out tPos);
                    mRootNode.Placement.Position = tPos;
                    mRootNode.Placement.Quat = tRot;
                    mRootNode.Placement.Scale = tScale;
                }
                else
                {
                    Matrix startTransAxisMat;
                    Matrix.Transformation(ref mStartTransAxisWorldTransform.mScale, ref mStartTransAxisWorldTransform.mQuat, ref mStartTransAxisWorldTransform.mPosition, out startTransAxisMat);
                    var tempMat = startTransAxisMat * mat;
                    Vector3 tPos, tScale;
                    Quaternion tRot;
                    tempMat.Decompose(out tScale, out tRot, out tPos);
                    mRootNode.Placement.Position = tPos;
                    mRootNode.Placement.Quat = tRot;
                    mRootNode.Placement.Scale = tScale;
                }

                if (mRotArrowAssetNode.Parent != mHostWorld.Root)
                    mRotArrowAssetNode.Parent = mHostWorld.Root;
                mRotArrowAssetNode.Placement.Position = mMouseStartLocation;
                mRotArrowAssetNode.Placement.Quat = Quaternion.GetQuaternion(len > 0 ? Vector3.UnitX : -Vector3.UnitX, tangentDir);

                if (mSelectedNodes != null)
                {
                    for (int i = 0; i < mSelectedNodes.Count; i++)
                    {
                        var data = mSelectedNodes[i];
                        Matrix startTransMatrix;
                        Matrix.Transformation(ref data.StartTransform.mScale, ref data.StartTransform.mQuat, ref data.StartTransform.mPosition, out startTransMatrix);

                        var transMat = startTransMatrix * mat;
                        if (data.Node.Parent != null)
                        {
                            transMat = transMat * data.Node.Parent.Placement.AbsTransformInv;
                        }
                        Vector3 pos, scale;
                        Quaternion rot;
                        transMat.Decompose(out scale, out rot, out pos);
                        data.Node.Placement.Position = pos;
                        data.Node.Placement.Quat = rot;
                        data.Node.Placement.Scale = scale;
                    }
                }
            }
        }
        void ScaleWithAxis(in Vector2 newMouseLoc, in Vector3 scaleAxisDir)
        {
            scaleAxisDir.Normalize();
            var camera = mCameraController.Camera.mCoreObject;
            if(camera.mIsOrtho)
            {
                throw new InvalidOperationException("没实现");
            }
            else
            {
                var axisStartTransPos = mRootNode.Placement.Position + mMouseTransOffset;
                var c2vMat = camera.GetToViewPortMatrix();
                Vector3 screenAxisLoc;
                var curAxisPosition = axisStartTransPos;
                Vector3.TransformCoordinate(ref curAxisPosition, ref c2vMat, out screenAxisLoc);
                screenAxisLoc.Z = 0;
                var tag = scaleAxisDir + axisStartTransPos;
                Vector3 screenTag;
                Vector3.TransformCoordinate(ref tag, ref c2vMat, out screenTag);
                screenTag.Z = 0;
                var screenAxisDir = screenTag - screenAxisLoc;
                screenAxisDir.Normalize();

                var camRight = camera.GetRight();
                camRight.Normalize();
                var deltaPos = curAxisPosition + camRight;
                Vector3 screenDeltaPos;
                Vector3.TransformCoordinate(ref deltaPos, ref c2vMat, out screenDeltaPos);
                screenDeltaPos.Z = 0;
                var s2vDelta = camRight.Length() / (screenDeltaPos - screenAxisLoc).Length();
                var lenInScreen = Vector3.Dot((new Vector3(newMouseLoc.X, newMouseLoc.Y, 0) - mMouseStartScreenLocation), screenAxisDir);
                var len = lenInScreen * s2vDelta;

                if(mSelectedNodes != null)
                {
                    //var mat = Matrix.Transformation(
                    //    mStartTransAxisWorldTransform.Position,
                    //    mStartTransAxisWorldTransform.Quat,
                    //    scaleAxisDir * len + Vector3.UnitXYZ,
                    //    mStartTransAxisWorldTransform.Position,
                    //    Quaternion.Identity,
                    //    Vector3.Zero);
                    //Matrix.Scaling(scaleAxisDir * len + Vector3.UnitXYZ);
                    var scale = scaleAxisDir * len;
                    for(int i=0; i<mSelectedNodes.Count; i++)
                    {
                        var data = mSelectedNodes[i];
                        //Matrix startTransMatrix;
                        //Matrix.Transformation(ref data.StartTransform.mScale, ref data.StartTransform.mQuat, ref data.StartTransform.mPosition, out startTransMatrix);

                        //var transMat = startTransMatrix * mat;
                        //if(data.Node.Parent != null)
                        //{
                        //    transMat = transMat * data.Node.Parent.Placement.AbsTransformInv;
                        //}
                        //Vector3 pos, scale;
                        //Quaternion rot;
                        //transMat.Decompose(out scale, out rot, out pos);
                        //data.Node.Placement.Position = pos;
                        //data.Node.Placement.Quat = rot;
                        //data.Node.Placement.Scale = scale;
                        data.Node.Placement.Scale = data.StartTransform.mScale + scale;
                    }
                }
            }
        }
        void EndTransAxis()
        {
            if (!mInitialized)
                return;
            mIsTransAxisOperation = false;
            mRotArrowAssetNode.Parent = null;

            switch(mAxisSpace)
            {
                case enAxisSpace.World:
                    mRootNode.Placement.Quat = Quaternion.Identity;
                    break;
            }
        }

        #region UI
        public unsafe void OnDrawUI(in Vector2 startDrawPos)
        {
            ImGuiAPI.SetCursorScreenPos(startDrawPos);

            Vector2 btnSize = new Vector2(24, 24);
            var n = mAxisOperationType == enAxisOperationType.Select;
            if(ImGuiAPI.ToggleButton("N", ref n, in btnSize, 0))
            {
                SetAxisOperationType(enAxisOperationType.Select);
            }
            ImGuiAPI.SameLine(0, 4);
            var m = mAxisOperationType == enAxisOperationType.Move;
            if(ImGuiAPI.ToggleButton("M", ref m, in btnSize, 0))
            {
                SetAxisOperationType(enAxisOperationType.Move);
            }
            ImGuiAPI.SameLine(0, 4);
            var r = mAxisOperationType == enAxisOperationType.Rot;
            if (ImGuiAPI.ToggleButton("R", ref r, in btnSize, 0))
            {
                SetAxisOperationType(enAxisOperationType.Rot);
            }
            ImGuiAPI.SameLine(0, 4);
            var s = mAxisOperationType == enAxisOperationType.Scale;
            if (ImGuiAPI.ToggleButton("S", ref s, in btnSize, 0))
            {
                SetAxisOperationType(enAxisOperationType.Scale);
            }
            //ImGuiAPI.SameLine(0, 4);
            //var e = mAxisOperationType == enAxisOperationType.Edge;
            //if (ImGuiAPI.ToggleButton("E", ref e, in btnSize, 0))
            //{
            //    SetAxisOperationType(enAxisOperationType.Edge);
            //}
            ImGuiAPI.SameLine(0, 8);
            var ls = mAxisSpace == enAxisSpace.Local;
            if(ImGuiAPI.ToggleButton("L", ref ls, in btnSize, 0))
            {
                SetAxisSpace(enAxisSpace.Local);
            }
            ImGuiAPI.SameLine(0, 4);
            var ws = mAxisSpace == enAxisSpace.World;
            if(ImGuiAPI.ToggleButton("W", ref ws, in btnSize, 0))
            {
                SetAxisSpace(enAxisSpace.World);
            }
        }
        #endregion
    }
}
