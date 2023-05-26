using EngineNS.GamePlay.Action;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public partial class UAxis : GamePlay.Action.IActionRecordable
    {
        public readonly static RName mAxisMaterial_Focus = RName.GetRName(@"axis\axis_focus_matins.uminst", RName.ERNameType.Engine);
        public readonly static RName mAxisMaterial_Face_Focus = RName.GetRName(@"axis\axis_face_focus_matins.uminst", RName.ERNameType.Engine);
        public readonly static RName mAxisMaterial_Face = RName.GetRName(@"axis\axis_face.uminst", RName.ERNameType.Engine);
        public readonly static RName mAxisMaterial_Center = RName.GetRName(@"axis\axis_all.uminst", RName.ERNameType.Engine);
        public readonly static RName mAxisMaterial_X = RName.GetRName(@"axis\axis_x_matins.uminst", RName.ERNameType.Engine);
        public readonly static RName mAxisMaterial_Y = RName.GetRName(@"axis\axis_y_matins.uminst", RName.ERNameType.Engine);
        public readonly static RName mAxisMaterial_Z = RName.GetRName(@"axis\axis_z_matins.uminst", RName.ERNameType.Engine);
        public readonly static RName mAxisMaterial_X_d = RName.GetRName(@"axis\axis_x_d.uminst", RName.ERNameType.Engine);
        public readonly static RName mAxisMaterial_Y_d = RName.GetRName(@"axis\axis_y_d.uminst", RName.ERNameType.Engine);
        public readonly static RName mAxisMaterial_Z_d = RName.GetRName(@"axis\axis_z_d.uminst", RName.ERNameType.Engine);
        public readonly static RName mAxisMaterial_Focus_d = RName.GetRName(@"axis\axis_focus_d.uminst", RName.ERNameType.Engine);
        //public readonly RName mAxisMaterial_TX = RName.GetRName(@"editor\axis\axismaterial_tx.instmtl", RName.ERNameType.Engine);
        //public readonly RName mAxisMaterial_TY = RName.GetRName(@"editor\axis\axismaterial_ty.instmtl", RName.ERNameType.Engine);
        //public readonly RName mAxisMaterial_TZ = RName.GetRName(@"editor\axis\axismaterial_tz.instmtl", RName.ERNameType.Engine);

        public readonly static RName mAxisMeshMoveAll = RName.GetRName(@"axis\moveall.vms", RName.ERNameType.Engine);
        public readonly static RName mAxisMeshMoveX = RName.GetRName(@"axis\movex.vms", RName.ERNameType.Engine);
        public readonly static RName mAxisMeshMoveXY = RName.GetRName(@"axis\movexy.vms", RName.ERNameType.Engine);
        public readonly static RName mAxisMeshMoveXY_Line = RName.GetRName(@"axis\movexy_line.vms", RName.ERNameType.Engine);
        public readonly static RName mAxisMeshRotX = RName.GetRName(@"axis\rotx.vms", RName.ERNameType.Engine);
        public readonly static RName mAxisMeshScaleAll = RName.GetRName(@"axis\scaleall.vms", RName.ERNameType.Engine);
        public readonly static RName mAxisMeshScaleX = RName.GetRName(@"axis\scalex.vms", RName.ERNameType.Engine);
        public readonly static RName mAxisMeshScaleXY = RName.GetRName(@"axis\scalexy.vms", RName.ERNameType.Engine);
        public readonly static RName mAxisMeshScaleXY_Line = RName.GetRName(@"axis\scalexy_line.vms", RName.ERNameType.Engine);
        public readonly static RName mAxisMeshEdgeX = RName.GetRName(@"axis\edgex.vms", RName.ERNameType.Engine);

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
            //Edge_X_MinPlane,
            Edge_X_Max,
            //Edge_X_MaxPlane,
            Edge_Y_Min,
            //Edge_Y_MinPlane,
            Edge_Y_Max,
            //Edge_Y_MaxPlane,
            Edge_Z_Min,
            //Edge_Z_MinPlane,
            Edge_Z_Max,
            //Edge_Z_MaxPlane,
            Edge_End = Edge_Z_Max,
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

        public enum enAxisSelectMode
        {
            ObjectsCenter,
            ObjectPos,
        }
        enAxisSelectMode mAxisSelectMode = enAxisSelectMode.ObjectsCenter;

        public enum enAxisSpace
        {
            World = 0,
            Local = 1,
        }
        enAxisSpace mAxisSpace = enAxisSpace.Local;
        public void SetAxisSpace(enAxisSpace space)
        {
            mAxisSpace = space;
            mOldAxisSpace = mAxisSpace;
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

        #region Snap
        [Flags]
        enum enSnapType
        {
            None = 0,
            MoveGrid = 1 << 0,
            MoveVertex = 1 << 1,
            MoveEdge = 1 << 2,
            MoveCenter = 1 << 3,
            MoveFace = 1 << 4,

            RotAngle = 1 << 10,
            ScaleValue = 1 << 15,
        }
        enSnapType mSnapType = enSnapType.None;
        bool HasSnapType(enSnapType type)
        {
            return (mSnapType & type) == type;
        }
        void SetSnapType(enSnapType type)
        {
            mSnapType |= type;
        }
        void UnsetSnapType(enSnapType type)
        {
            mSnapType &= ~type;
        }

        float mSnapRotAngle = 10.0f;
        float mSnapScaleDelta = 0.1f;
        float mSnapGridSize = 0.1f;
        #endregion

        class UAxisNode : Scene.USceneActorNode
        {
            public override async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, Scene.UNodeData data, Scene.EBoundVolumeType bvType, Type placementType)                
            {
                return await base.InitializeNode(world, data, bvType, placementType);
            }

            public override bool DrawNode(EngineNS.Editor.UTreeNodeDrawer tree, int index, int NumOfChild)
            {
                return false;
            }
            public override void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
            {
                base.OnGatherVisibleMeshes(rp);
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
                var ok = await mesh.Initialize(meshName, materials,
                    Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc);
                return ok ? mesh : null;
            }
            public async System.Threading.Tasks.Task Initialize(enAxisType type, GamePlay.UWorld world)
            {
                Graphics.Mesh.UMesh axisMesh = null;
                var meshNodeData = new GamePlay.Scene.UMeshNode.UMeshNodeData();
                DVector3 pos = DVector3.Zero;
                Quaternion rot = Quaternion.Identity;
                Vector3 scale = Vector3.One;
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
                        rot = Quaternion.RotationAxis(Vector3.Left, (float)(Math.PI * 0.5f));
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
                        rot = Quaternion.RotationAxis(Vector3.Up, (float)(Math.PI * 0.5f));
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
                        axisMesh = await GetAxisMesh(mAxisMeshMoveXY, mAxisMaterial_Focus_d);
                        rot = Quaternion.RotationAxis(Vector3.Left, (float)(Math.PI * 0.5f));
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
                    case enAxisType.Move_Plane_YZ:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveXY, mAxisMaterial_Focus_d);
                        rot = Quaternion.RotationAxis(Vector3.Up, (float)(Math.PI * 0.5f));
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
                    case enAxisType.Edge_X_Min:
                        axisMesh = await GetAxisMesh(mAxisMeshEdgeX, mAxisMaterial_X);
                        rot = Quaternion.RotationAxis(Vector3.Backward, (float)(Math.PI));
                        meshNodeData.MeshName = mAxisMeshEdgeX;
                        meshNodeData.Name = "Edge_XMin";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_X_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    //case enAxisType.Edge_X_MinPlane:  break;
                    case enAxisType.Edge_X_Max:
                        axisMesh = await GetAxisMesh(mAxisMeshEdgeX, mAxisMaterial_X);
                        meshNodeData.MeshName = mAxisMeshEdgeX;
                        meshNodeData.Name = "Edge_XMax";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_X_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    //case enAxisType.Edge_X_MaxPlane:  break;
                    case enAxisType.Edge_Y_Min:  
                        axisMesh = await GetAxisMesh(mAxisMeshEdgeX, mAxisMaterial_Y);
                        rot = Quaternion.RotationAxis(Vector3.Backward, (float)(Math.PI * -0.5f));
                        meshNodeData.MeshName = mAxisMeshEdgeX;
                        meshNodeData.Name = "Edge_YMin";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Y_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    //case enAxisType.Edge_Y_MinPlane:  break;
                    case enAxisType.Edge_Y_Max:  
                        axisMesh = await GetAxisMesh(mAxisMeshEdgeX, mAxisMaterial_Y);
                        rot = Quaternion.RotationAxis(Vector3.Backward, (float)(Math.PI * 0.5f));
                        meshNodeData.MeshName = mAxisMeshEdgeX;
                        meshNodeData.Name = "Edge_YMax";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Y_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    //case enAxisType.Edge_Y_MaxPlane:  break;
                    case enAxisType.Edge_Z_Min:
                        axisMesh = await GetAxisMesh(mAxisMeshEdgeX, mAxisMaterial_Z);
                        rot = Quaternion.RotationAxis(Vector3.Down, (float)(Math.PI * -0.5f));
                        meshNodeData.MeshName = mAxisMeshEdgeX;
                        meshNodeData.Name = "Edge_ZMin";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Z_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    //case enAxisType.Edge_Z_MinPlane:  break;
                    case enAxisType.Edge_Z_Max:  
                        axisMesh = await GetAxisMesh(mAxisMeshEdgeX, mAxisMaterial_Z);
                        rot = Quaternion.RotationAxis(Vector3.Down, (float)(Math.PI * 0.5f));
                        meshNodeData.MeshName = mAxisMeshEdgeX;
                        meshNodeData.Name = "Edge_ZMin";
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Z_d),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d),
                        };
                        break;
                    //case enAxisType.Edge_Z_MaxPlane:  break;
                }

                MeshNode = (Scene.UMeshNode) await world.Root.NewNode(world, typeof(Scene.UMeshNode), meshNodeData, Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
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
        List<AxisData> mAxisMeshDatas;

        #region CenterAxis
        DBoundingBox mEdgeAxisBB = DBoundingBox.EmptyBox();
        DBoundingBox mEdgeAxisOrigionBBBeforeCenterOperation;
        FTransform mAxisOrigionTransformBeforeCenterOperation;
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
                                    mRootNode.Placement.Position = GetCenterAxisPosition();
                                }
                                else
                                {
                                    mRootNode.Placement.SetTransform(in mAxisOrigionTransformBeforeCenterOperation);
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

        DVector3 GetCenterAxisPosition()
        {
            var camera = mCameraController.Camera;
            return camera.mCoreObject.GetPosition() + camera.mCoreObject.GetDirection() * 10;
        }

        void UpdateEdgeBB()
        {
            if (mSelectedNodes == null || mSelectedNodes.Count <= 0)
                return;

            switch (mAxisSpace)
            {
                case enAxisSpace.World:
                    mEdgeAxisBB = DBoundingBox.EmptyBox();
                    for (int i = 0; i < mSelectedNodes.Count; i++)
                    {
                        DBoundingBox nodeAABB;
                        mSelectedNodes[i].Node.GetWorldSpaceBoundingBox(out nodeAABB);
                        mEdgeAxisBB = DBoundingBox.Merge(in mEdgeAxisBB, in nodeAABB);
                    }
                    mRootNode.AABB = mEdgeAxisBB;
                    break;
                case enAxisSpace.Local:
                    {
                        var posNode = GetPosNode().Node;
                        mEdgeAxisBB = posNode.AABB;
                        posNode.GetWorldSpaceBoundingBox(out mRootNode.AABB);
                        var mat = posNode.Placement.AbsTransform;
                        //var invMat = posNode.Placement.AbsTransformInv;
                        for (int i=0; i<mSelectedNodes.Count - 1; i++)
                        {
                            var nodeAABB = mSelectedNodes[i].Node.AABB;
                            var localPos = nodeAABB.GetCorners();
                            for(int posIdx = 0; posIdx < localPos.Length; posIdx++)
                            {
                                var tempPos = mSelectedNodes[i].Node.Placement.AbsTransform.TransformPositionNoScale(in localPos[i]);
                                //var tempPos = Vector3.TransformCoordinate(localPos[i], mSelectedNodes[i].Node.Placement.AbsTransform);
                                //Vector3.TransformCoordinate(in tempPos, in invMat, out tempPos);
                                tempPos = posNode.Placement.AbsTransform.InverseTransformPosition(in tempPos);
                                mEdgeAxisBB.Merge(in tempPos);
                            }

                            DBoundingBox nodeWorldAABB;
                            mSelectedNodes[i].Node.GetWorldSpaceBoundingBox(out nodeWorldAABB);
                            mRootNode.AABB = DBoundingBox.Merge(in mRootNode.AABB, in nodeWorldAABB);
                        }
                        mRootNode.AABB = mEdgeAxisBB;
                    }
                    break;
            }

            if(CenterAxisMode)
            {
                mEdgeAxisOrigionBBBeforeCenterOperation = mEdgeAxisBB;
                mEdgeAxisBB = new DBoundingBox(GetCenterAxisPosition(), 1);
            }

        }
        void UpdateEdgeAxisTransform()
        {
            if (mSelectedNodes == null || mSelectedNodes.Count == 0)
                return;

            mRootNode.Placement.Position = DVector3.Zero;
            mRootNode.Placement.Quat = Quaternion.Identity;
            mRootNode.Placement.Scale = Vector3.One;
            var bb = mEdgeAxisBB;
            switch(mAxisSpace)
            {
                case enAxisSpace.World:
                    {
                        var edgeCenter = bb.GetCenter();
                        Quaternion orgRot = Quaternion.Identity;
                        for (enAxisType i=enAxisType.Edge_Start; i<=enAxisType.Edge_End; i++)
                        {
                            switch(i)
                            {
                                case enAxisType.Edge_X_Min:
                                    mAxisMeshDatas[(int)i].MeshNode.Placement.Position = new DVector3(bb.Minimum.X, edgeCenter.Y, edgeCenter.Z);
                                    mAxisMeshDatas[(int)i].MeshNode.Placement.Quat = Quaternion.RotationAxis(Vector3.Backward, (float)Math.PI);
                                    break;
                                case enAxisType.Edge_X_Max:
                                    mAxisMeshDatas[(int)i].MeshNode.Placement.Position = new DVector3(bb.Maximum.X, edgeCenter.Y, edgeCenter.Z);
                                    mAxisMeshDatas[(int)i].MeshNode.Placement.Quat = Quaternion.Identity;
                                    break;
                                case enAxisType.Edge_Y_Min:
                                    mAxisMeshDatas[(int)i].MeshNode.Placement.Position = new DVector3(edgeCenter.X, bb.Minimum.Y, edgeCenter.Z);
                                    mAxisMeshDatas[(int)i].MeshNode.Placement.Quat = Quaternion.RotationAxis(Vector3.Forward, (float)(Math.PI * 0.5f));
                                    break;
                                case enAxisType.Edge_Y_Max:
                                    mAxisMeshDatas[(int)i].MeshNode.Placement.Position = new DVector3(edgeCenter.X, bb.Maximum.Y, edgeCenter.Z);
                                    mAxisMeshDatas[(int)i].MeshNode.Placement.Quat = Quaternion.RotationAxis(Vector3.Backward, (float)(Math.PI * 0.5f));
                                    break;
                                case enAxisType.Edge_Z_Min:
                                    mAxisMeshDatas[(int)i].MeshNode.Placement.Position = new DVector3(edgeCenter.X, edgeCenter.Y, bb.Minimum.Z);
                                    mAxisMeshDatas[(int)i].MeshNode.Placement.Quat = Quaternion.RotationAxis(Vector3.Up, (float)(Math.PI * 0.5f));
                                    break;
                                case enAxisType.Edge_Z_Max:
                                    mAxisMeshDatas[(int)i].MeshNode.Placement.Position = new DVector3(edgeCenter.X, edgeCenter.Y, bb.Maximum.Z);
                                    mAxisMeshDatas[(int)i].MeshNode.Placement.Quat = Quaternion.RotationAxis(Vector3.Down, (float)(Math.PI * 0.5f));
                                    break;
                            }
                        }
                    }
                    break;
                case enAxisSpace.Local:
                    {
                        var edgeCenter = bb.GetCenter();
                        var posNode = GetPosNode().Node;
                        var posNodeRot = posNode.Placement.Quat;

                        Quaternion orgRot = Quaternion.Identity;
                        for (enAxisType i=enAxisType.Edge_Start; i<=enAxisType.Edge_End; i++)
                        {
                            var loc = edgeCenter;
                            switch(i)
                            {
                                case enAxisType.Edge_X_Min:
                                    loc = new DVector3(bb.Minimum.X, edgeCenter.Y, edgeCenter.Z);
                                    orgRot = Quaternion.RotationAxis(Vector3.Backward, (float)Math.PI);
                                    break;
                                case enAxisType.Edge_X_Max:
                                    loc = new DVector3(bb.Maximum.X, edgeCenter.Y, edgeCenter.Z);
                                    orgRot = Quaternion.Identity;
                                    break;
                                case enAxisType.Edge_Y_Min:
                                    loc = new DVector3(edgeCenter.X, bb.Minimum.Y, edgeCenter.Z);
                                    orgRot = Quaternion.RotationAxis(Vector3.Forward, (float)(Math.PI * 0.5f));
                                    break;
                                case enAxisType.Edge_Y_Max:
                                    loc = new DVector3(edgeCenter.X, bb.Maximum.Y, edgeCenter.Z);
                                    orgRot = Quaternion.RotationAxis(Vector3.Backward, (float)(Math.PI * 0.5f));
                                    break;
                                case enAxisType.Edge_Z_Min:
                                    loc = new DVector3(edgeCenter.X, edgeCenter.Y, bb.Minimum.Z);
                                    orgRot = Quaternion.RotationAxis(Vector3.Up, (float)(Math.PI * 0.5f));
                                    break;
                                case enAxisType.Edge_Z_Max:
                                    loc = new DVector3(edgeCenter.X, edgeCenter.Y, bb.Maximum.Z);
                                    orgRot = Quaternion.RotationAxis(Vector3.Down, (float)(Math.PI * 0.5f));
                                    break;
                            }

                            if (CenterAxisMode)
                                loc = DVector3.TransformCoordinate(loc - edgeCenter, posNode.Placement.Quat) + edgeCenter;
                            else
                            {
                                //loc = Vector3.TransformCoordinate(loc, posNode.Placement.AbsTransform);
                                loc = posNode.Placement.AbsTransform.TransformPositionNoScale(loc);
                            }
                            mAxisMeshDatas[(int)i].MeshNode.Placement.Position = loc;
                            mAxisMeshDatas[(int)i].MeshNode.Placement.Quat = orgRot * posNodeRot;// posNodeRot * orgRot;
                        }
                    }
                    break;
            }
        }
        #endregion

        GamePlay.UWorld mHostWorld;
        UAxisNode mRootNode;
        float mRootNodeScaleValue = 1.0f;
        Scene.UMeshNode mRotArrowAssetNode;
        bool mInitialized = false;
        Graphics.Pipeline.ICameraController mCameraController;

        public async System.Threading.Tasks.Task Initialize(GamePlay.UWorld world, Graphics.Pipeline.ICameraController cameraController)
        {
            if (world == mHostWorld && mInitialized)
                return;

            mHostWorld = world;
            mCameraController = cameraController;

            var tmpAxis = new List<AxisData>();
            for (var i=enAxisType.AxisStart; i<=enAxisType.AxisEnd; i++)
            {
                var axisData = new AxisData();
                await axisData.Initialize(i, mHostWorld);
                tmpAxis.Add(axisData);
            }
            mAxisMeshDatas = tmpAxis;

            mRootNode = (UAxisNode)await world.Root.NewNode(world, typeof(UAxisNode),
                new GamePlay.Scene.UNodeData()
                {
                    Name = "AxisRootNode"
                },
                Scene.EBoundVolumeType.None, typeof(GamePlay.UPlacement));
            mRootNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
            mRootNode.IsCastShadow = false;
            mRootNode.Parent = world.Root;
            ((GamePlay.UPlacement)mRootNode.Placement).InheritScale = true;

            var rotArrowAssetMat = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d);
            var rotArrowAssetMesh = new Graphics.Mesh.UMesh();
            var ok = await rotArrowAssetMesh.Initialize(mAxisMeshMoveX, new Graphics.Pipeline.Shader.UMaterial[] { rotArrowAssetMat },
                Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc);
            if(ok)
            {
                var meshNodeData = new GamePlay.Scene.UMeshNode.UMeshNodeData();
                meshNodeData.MeshName = mAxisMeshMoveX;
                meshNodeData.Name = "RotArrowAsset";
                mRotArrowAssetNode = (Scene.UMeshNode)await world.Root.NewNode(world, typeof(Scene.UMeshNode), meshNodeData, Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
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
            var plane = Graphics.Mesh.UMeshDataProvider.MakePlane(1, 1);
            var planeMesh = plane.ToMesh();
            var planeMaterial = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus_d);
            var ok = mesh.Initialize(
                planeMesh,
                new Graphics.Pipeline.Shader.UMaterial[] { planeMaterial },
                Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc);
            if(ok)
            {
                mPlaneNode = await Scene.UMeshNode.AddMeshNode(
                    mHostWorld,
                    mHostWorld.Root,
                    new Scene.UMeshNode.UMeshNodeData(),
                    typeof(UPlacement),
                    mesh,
                    DVector3.Zero,
                    Vector3.One,
                    Quaternion.Identity);
                mPlaneNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
                mPlaneNode.NodeData.Name = "AxisDebugPlane";
                mPlaneNode.IsCastShadow = false;
                mPlaneNode.Parent = mHostWorld.Root;
            }

            mesh = new Graphics.Mesh.UMesh();
            var point = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.05f, -0.05f, -0.05f, 0.1f, 0.1f, 0.1f);
            var pointMesh = point.ToMesh();
            var pointMaterial = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Center);
            ok = mesh.Initialize(pointMesh, new Graphics.Pipeline.Shader.UMaterial[] { pointMaterial },
                Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc);
            if(ok)
            {
                mPointNode = await Scene.UMeshNode.AddMeshNode(
                    mHostWorld,
                    mHostWorld.Root,
                    new Scene.UMeshNode.UMeshNodeData(),
                    typeof(UPlacement),
                    mesh,
                    DVector3.Zero,
                    Vector3.One,
                    Quaternion.Identity);
                mPointNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
                mPointNode.NodeData.Name = "AxisDebugPoint";
                mPointNode.IsCastShadow = false;
                mPointNode.Parent = mHostWorld.Root;
            }
        }
        #endregion

        public class SelectedNodeData
        {
            public GamePlay.Scene.UNode Node;
            public FTransform StartAbsTransform = FTransform.Identity;
            public FTransform StartTransform = FTransform.Identity;
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
        SelectedNodeData mPosNode = null;
        public SelectedNodeData GetPosNode()
        {
            if (mPosNode != null)
                return mPosNode;

            if (mSelectedNodes == null || mSelectedNodes.Count == 0)
                return null;
            return mSelectedNodes[mSelectedNodes.Count - 1];
        }
        public void SetSelectedNodes(params GamePlay.Scene.UNode[] nodes)
        {
            if (nodes != null)
            {
                var tempNodes = new List<SelectedNodeData>(nodes.Length);

                DVector3 axisPos = DVector3.Zero;
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
                SelectedNodes = (tempNodes.Count != 0) ? tempNodes : null;
                    
                //if(tempNodes.Count == 0)
                //{
                //    mRootNode.SetStyle(Scene.UNode.ENodeStyles.Invisible);
                //}
                //else
                //{
                //    SelectedNodes = tempNodes;
                //    axisPos = axisPos / nodes.Length;
                //    mRootNode.Placement.Position = axisPos;
                //    switch(mAxisSpace)
                //    {
                //        case enAxisSpace.Local:
                //            {
                //                if(mSelectedNodes != null && mSelectedNodes.Count > 0)
                //                {
                //                    mRootNode.Placement.Quat = mSelectedNodes[mSelectedNodes.Count - 1].Node.Placement.Quat;
                //                }
                //            }
                //            break;
                //        case enAxisSpace.World:
                //            mRootNode.Placement.Quat = Quaternion.Identity;
                //            break;
                //    }
                //    mRootNode.Placement.Scale = Vector3.UnitXYZ;
                //    mRootNode.UnsetStyle(Scene.UNode.ENodeStyles.Invisible);
                //}
            }
            else
                SelectedNodes = null;
        }

        enAxisSpace mOldAxisSpace;
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
                        mOldAxisSpace = mAxisSpace;
                        mAxisSpace = enAxisSpace.Local;
                    }
                    break;
                case enAxisOperationType.Edge:
                    {
                        for (int i = (int)enAxisType.Edge_Start; i <= (int)enAxisType.Edge_End; i++)
                        {
                            mAxisMeshDatas[i].MeshNode.Parent = mRootNode;
                        }
                        mOldAxisSpace = mAxisSpace;
                        mAxisSpace = enAxisSpace.Local;
                    }
                    break;
            }
        }

        bool mCtrlKeyIsDown = false;
        bool mShiftKeyIsDown = false;
        bool mAltKeyIsDown = false;
        public void OnEvent(Graphics.Pipeline.UViewportSlate viewport, in Bricks.Input.Event e)
        {
            if (mAxisMeshDatas == null)
                return;

            UpdateAxisShow(viewport);            
            switch (e.Type)
            {
                case Bricks.Input.EventType.MOUSEBUTTONDOWN:
                    {
                        mFirstTransAxis = true;
                        if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT && mCurrentAxisType != enAxisType.Null)
                        {
                            StartTransAxis(viewport, in e);
                        }
                    }
                    break;
                case Bricks.Input.EventType.MOUSEMOTION:
                    {
                        if(!mIsTransAxisOperation)
                        {
                            var edtorPolicy = viewport.RenderPolicy as Graphics.Pipeline.URenderPolicy;
                            if (edtorPolicy != null)
                            {
                                var pos = viewport.Window2Viewport(new Vector2((float)e.MouseMotion.X, (float)e.MouseMotion.Y));
                                var hitObj = edtorPolicy.GetHitproxy((uint)pos.X, (uint)pos.Y);
                                if (hitObj != null)
                                {
                                    switch (mAxisOperationType)
                                    {
                                        case enAxisOperationType.Move:
                                            {
                                                for (int i = (int)enAxisType.Move_Start; i <= (int)enAxisType.Move_End; i++)
                                                    mAxisMeshDatas[i].Focused = false;

                                                for (int i = (int)enAxisType.Move_Start; i <= (int)enAxisType.Move_End; i++)
                                                {
                                                    if (mAxisMeshDatas[i].MeshNode.HitProxy.ProxyId == hitObj.HitProxy.ProxyId)
                                                    {
                                                        mAxisMeshDatas[i].Focused = true;
                                                        switch(mAxisMeshDatas[i].AxisType)
                                                        {
                                                            case enAxisType.Move_Line_XY:
                                                                mAxisMeshDatas[(int)enAxisType.Move_Plane_XY].Focused = true;
                                                                break;
                                                            case enAxisType.Move_Line_XZ:
                                                                mAxisMeshDatas[(int)enAxisType.Move_Plane_XZ].Focused = true;
                                                                break;
                                                            case enAxisType.Move_Line_YZ:
                                                                mAxisMeshDatas[(int)enAxisType.Move_Plane_YZ].Focused = true;
                                                                break;
                                                            case enAxisType.Move_Plane_XY:
                                                                mAxisMeshDatas[(int)enAxisType.Move_Line_XY].Focused = true;
                                                                break;
                                                            case enAxisType.Move_Plane_XZ:
                                                                mAxisMeshDatas[(int)enAxisType.Move_Line_XZ].Focused = true;
                                                                break;
                                                            case enAxisType.Move_Plane_YZ:
                                                                mAxisMeshDatas[(int)enAxisType.Move_Line_YZ].Focused = true;
                                                                break;
                                                        }
                                                        mCurrentAxisType = mAxisMeshDatas[i].AxisType;
                                                    }
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
                                                for (int i = (int)enAxisType.Scale_Start; i <= (int)enAxisType.Scale_End; i++)
                                                    mAxisMeshDatas[i].Focused = false;

                                                for (int i=(int)enAxisType.Scale_Start; i <= (int)enAxisType.Scale_End; i++)
                                                {
                                                    if (mAxisMeshDatas[i].MeshNode.HitProxy.ProxyId == hitObj.HitProxy.ProxyId)
                                                    {
                                                        mAxisMeshDatas[i].Focused = true;
                                                        switch(mAxisMeshDatas[i].AxisType)
                                                        {
                                                            case enAxisType.Scale_Line_XY:
                                                                mAxisMeshDatas[(int)enAxisType.Scale_Plane_XY].Focused = true;
                                                                break;
                                                            case enAxisType.Scale_Line_XZ:
                                                                mAxisMeshDatas[(int)enAxisType.Scale_Plane_XZ].Focused = true;
                                                                break;
                                                            case enAxisType.Scale_Line_YZ:
                                                                mAxisMeshDatas[(int)enAxisType.Scale_Plane_YZ].Focused = true;
                                                                break;
                                                            case enAxisType.Scale_Plane_XY:
                                                                mAxisMeshDatas[(int)enAxisType.Scale_Line_XY].Focused = true;
                                                                break;
                                                            case enAxisType.Scale_Plane_XZ:
                                                                mAxisMeshDatas[(int)enAxisType.Scale_Line_XZ].Focused = true;
                                                                break;
                                                            case enAxisType.Scale_Plane_YZ:
                                                                mAxisMeshDatas[(int)enAxisType.Scale_Line_YZ].Focused = true;
                                                                break;
                                                        }
                                                        mCurrentAxisType = mAxisMeshDatas[i].AxisType;
                                                    }
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

                        if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                        {
                            var noUse = TransAxis(new Vector2(e.MouseMotion.X, e.MouseMotion.Y), viewport);
                        }
                    }
                    break;
                case Bricks.Input.EventType.MOUSEBUTTONUP:
                    {
                        if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                            EndTransAxis();
                    }
                    break;
                case Bricks.Input.EventType.KEYDOWN:
                    {
                        if (e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_LCTRL || e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_RCTRL)
                            mCtrlKeyIsDown = true;
                        if (e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_LSHIFT || e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_RSHIFT)
                            mShiftKeyIsDown = true;
                        if (e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_LALT || e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_RALT)
                            mAltKeyIsDown = true;
                    }
                    break;
                case Bricks.Input.EventType.KEYUP:
                    {
                        if (e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_LCTRL || e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_RCTRL)
                            mCtrlKeyIsDown = false;
                        if (e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_LSHIFT || e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_RSHIFT)
                            mShiftKeyIsDown = false;
                        if (e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_LALT || e.Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_RALT)
                            mAltKeyIsDown = false;
                    }
                    break;
            }

        }

        float GetScreenSizeInWorld(in Vector3 worldPos, float screenSize, Graphics.Pipeline.UViewportSlate viewport)
        {
            var camera = mCameraController.Camera.mCoreObject;
            var fov = camera.mFov;
            if (fov == 0)
            {
                return screenSize * (camera.mHeight / viewport.ClientSize.Y) * 1000;
            }
            else
            {
                var dis = (camera.GetPosition() - worldPos).Length();
                var worldFullScreenSize = (float)(Math.Tan(fov * 0.5f) * dis * 2);
                return worldFullScreenSize * screenSize;
            }
        }

        void FitAxisSize(Graphics.Pipeline.UViewportSlate viewport)
        {
            if (!mInitialized)
                return;

            switch(mAxisOperationType)
            {
                case enAxisOperationType.Edge:
                    {
                        for(var i = enAxisType.Edge_Start; i <= enAxisType.Edge_End; i++)
                        {
                            var size = GetScreenSizeInWorld(mAxisMeshDatas[(int)i].MeshNode.Placement.Position.ToSingleVector3(), 0.15f, viewport);
                            mAxisMeshDatas[(int)i].MeshNode.Placement.Scale = size * Vector3.One;
                        }
                    }
                    break;
                default:
                    if (!mRootNode.HasStyle(Scene.UNode.ENodeStyles.Invisible))
                    {
                        var camPos = mCameraController.Camera.mCoreObject.GetPosition();
                        mRootNodeScaleValue = (float)(mRootNode.Placement.Position - camPos).Length() * 0.15f;
                        mRootNode.Placement.Scale = mRootNodeScaleValue * Vector3.One;
                        mRotArrowAssetNode.Placement.Scale = mRootNode.Placement.Scale;
                    }
                    break;
            }
        }
        void GetNodeAbsPosition(Scene.UNode node, ref DVector3 absPos)
        {
            if (node.Parent == null)
                absPos = node.Placement.Position;
            else
            {
                var pos = node.Placement.Position;
                absPos = node.Parent.Placement.AbsTransform.TransformPositionNoScale(in pos);
                //Vector3.TransformCoordinate(in pos, in node.Parent.Placement.mAbsTransform, out absPos);
            }
        }
        void GetNodeAbsRotation(Scene.UNode node, out Quaternion absRot)
        {
            if (node.Parent == null)
                absRot = node.Placement.Quat;
            else
                absRot = node.Placement.AbsTransform.mQuat;

        }
        void UpdateAxisShow(Graphics.Pipeline.UViewportSlate viewport)
        {
            if (!mInitialized)
                return;

            if(mSelectedNodes != null && mSelectedNodes.Count > 0)
            {
                mRootNode.UnsetStyle(Scene.UNode.ENodeStyles.Invisible);
                if (!mIsTransAxisOperation)
                {
                    var camera = mCameraController.Camera.mCoreObject;
                    var cameraPos = camera.GetPosition();
                    var cameraDir = camera.GetDirection();
                    switch(mAxisOperationType)
                    {
                        case enAxisOperationType.Edge:
                            {
                                UpdateEdgeBB();
                                UpdateEdgeAxisTransform();
                            }
                            break;
                        default:
                            {
                                switch(mAxisSelectMode)
                                {
                                    case enAxisSelectMode.ObjectsCenter:
                                        {
                                            var centerPos = DVector3.Zero;
                                            for(int i=0; i<mSelectedNodes.Count; i++)
                                            {
                                                var tempPos = DVector3.Zero;
                                                GetNodeAbsPosition(mSelectedNodes[i].Node, ref tempPos);
                                                centerPos += tempPos;
                                            }
                                            centerPos = centerPos / mSelectedNodes.Count;
                                            mRootNode.Placement.Position = centerPos;
                                            mRootNode.Placement.Quat = Quaternion.Identity;

                                            if(mSelectedNodes.Count == 1)
                                            {
                                                switch(mAxisSpace)
                                                {
                                                    case enAxisSpace.World:
                                                        mRootNode.Placement.Quat = Quaternion.Identity;
                                                        break;
                                                    case enAxisSpace.Local:
                                                        {
                                                            Quaternion tempRot;
                                                            GetNodeAbsRotation(mSelectedNodes[0].Node, out tempRot);
                                                            mRootNode.Placement.Quat = tempRot;
                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                        break;
                                    case enAxisSelectMode.ObjectPos:
                                        {
                                            var node = GetPosNode().Node;

                                            var worldPos = DVector3.Zero;
                                            GetNodeAbsPosition(node, ref worldPos);
                                            mRootNode.Placement.Position = worldPos;
                                            switch(mAxisSpace)
                                            {
                                                case enAxisSpace.World:
                                                    mRootNode.Placement.Quat = Quaternion.Identity;
                                                    break;
                                                case enAxisSpace.Local:
                                                    {
                                                        Quaternion tempRot;
                                                        GetNodeAbsRotation(node, out tempRot);
                                                        mRootNode.Placement.Quat = tempRot;
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                }

                                if(CenterAxisMode)
                                {
                                    mAxisOrigionTransformBeforeCenterOperation = ((UPlacement)mRootNode.Placement).TransformData;
                                    mRootNode.Placement.Position = GetCenterAxisPosition();
                                }

                                var pos = mRootNode.Placement.Position;
                                var rot = mRootNode.Placement.Quat;

                                var xzRot = Quaternion.Identity;
                                var xyRot = Quaternion.Identity;
                                var zxRot = Quaternion.Identity;
                                var zyRot = Quaternion.Identity;
                                var yzRot = Quaternion.Identity;
                                var yxRot = Quaternion.Identity;
                                var deltaPos = cameraPos - pos;
                                var axisX = DVector3.TransformCoordinate(DVector3.UnitX, rot);
                                var projX = DVector3.Dot(deltaPos, axisX);
                                var axisY = DVector3.TransformCoordinate(DVector3.UnitY, rot);
                                var projY = DVector3.Dot(deltaPos, axisY);
                                var axisZ = DVector3.TransformCoordinate(DVector3.UnitZ, rot);
                                var projZ = DVector3.Dot(deltaPos, axisZ);
                                float deltaX = 0;
                                float deltaY = 0;
                                float deltaZ = 0;
                                if(projX < 0)
                                {
                                    xzRot = Quaternion.RotationAxis(Vector3.UnitY, (float)Math.PI);
                                    xyRot = Quaternion.RotationAxis(Vector3.UnitY, (float)Math.PI);
                                    switch(mAxisOperationType)
                                    {
                                        case enAxisOperationType.Move:
                                            deltaX = mAxisMeshDatas[(int)(enAxisType.Move_X)].MeshNode.BoundVolume.mLocalAABB.GetSize().X;
                                            break;
                                        case enAxisOperationType.Scale:
                                            deltaX = mAxisMeshDatas[(int)(enAxisType.Scale_X)].MeshNode.BoundVolume.mLocalAABB.GetSize().X;
                                            break;
                                    }
                                }
                                if(projY < 0)
                                {
                                    yxRot = Quaternion.RotationAxis(Vector3.UnitX, (float)Math.PI);
                                    yzRot = Quaternion.RotationAxis(Vector3.UnitX, (float)Math.PI);
                                    switch(mAxisOperationType)
                                    {
                                        case enAxisOperationType.Move:
                                            deltaY = mAxisMeshDatas[(int)(enAxisType.Move_Y)].MeshNode.BoundVolume.mLocalAABB.GetSize().X;
                                            break;
                                        case enAxisOperationType.Scale:
                                            deltaY = mAxisMeshDatas[(int)(enAxisType.Scale_X)].MeshNode.BoundVolume.mLocalAABB.GetSize().X;
                                            break;
                                    }
                                }
                                if (projZ < 0)
                                {
                                    zyRot = Quaternion.RotationAxis(Vector3.UnitY, (float)Math.PI);
                                    zxRot = Quaternion.RotationAxis(Vector3.UnitX, (float)Math.PI);
                                    switch(mAxisOperationType)
                                    {
                                        case enAxisOperationType.Move:
                                            deltaZ = mAxisMeshDatas[(int)(enAxisType.Move_Z)].MeshNode.BoundVolume.mLocalAABB.GetSize().X;
                                            break;
                                        case enAxisOperationType.Scale:
                                            deltaZ = mAxisMeshDatas[(int)(enAxisType.Scale_X)].MeshNode.BoundVolume.mLocalAABB.GetSize().X;
                                            break;
                                    }
                                }
                                var xzRotation = xzRot * zxRot * Quaternion.RotationAxis(Vector3.Right, (float)(Math.PI * 0.5f));
                                var xyRotation = xyRot * yxRot;
                                var yzRotation = yzRot * zyRot * Quaternion.RotationAxis(Vector3.Down, (float)(Math.PI * 0.5f));
                                switch(mAxisOperationType)
                                {
                                    case enAxisOperationType.Move:
                                        {
                                            mAxisMeshDatas[(int)(enAxisType.Move_Line_XY)].MeshNode.Placement.Quat = xyRotation;
                                            mAxisMeshDatas[(int)(enAxisType.Move_Plane_XY)].MeshNode.Placement.Quat = xyRotation;
                                            mAxisMeshDatas[(int)(enAxisType.Move_Line_XZ)].MeshNode.Placement.Quat = xzRotation;
                                            mAxisMeshDatas[(int)(enAxisType.Move_Plane_XZ)].MeshNode.Placement.Quat = xzRotation;
                                            mAxisMeshDatas[(int)(enAxisType.Move_Line_YZ)].MeshNode.Placement.Quat = yzRotation;
                                            mAxisMeshDatas[(int)(enAxisType.Move_Plane_YZ)].MeshNode.Placement.Quat = yzRotation;

                                            mAxisMeshDatas[(int)(enAxisType.Move_X)].MeshNode.Placement.Position = new DVector3(-deltaX, 0, 0);
                                            mAxisMeshDatas[(int)(enAxisType.Move_Y)].MeshNode.Placement.Position = new DVector3(0, -deltaY, 0);
                                            mAxisMeshDatas[(int)(enAxisType.Move_Z)].MeshNode.Placement.Position = new DVector3(0, 0, -deltaZ);
                                        }
                                        break;
                                    case enAxisOperationType.Scale:
                                        {
                                            mAxisMeshDatas[(int)(enAxisType.Scale_Line_XY)].MeshNode.Placement.Quat = xyRotation;
                                            mAxisMeshDatas[(int)(enAxisType.Scale_Plane_XY)].MeshNode.Placement.Quat = xyRotation;
                                            mAxisMeshDatas[(int)(enAxisType.Scale_Line_XZ)].MeshNode.Placement.Quat = xzRotation;
                                            mAxisMeshDatas[(int)(enAxisType.Scale_Plane_XZ)].MeshNode.Placement.Quat = xzRotation;
                                            mAxisMeshDatas[(int)(enAxisType.Scale_Line_YZ)].MeshNode.Placement.Quat = yzRotation;
                                            mAxisMeshDatas[(int)(enAxisType.Scale_Plane_YZ)].MeshNode.Placement.Quat = yzRotation;

                                            mAxisMeshDatas[(int)(enAxisType.Scale_X)].MeshNode.Placement.Position = new DVector3(-deltaX, 0, 0);
                                            mAxisMeshDatas[(int)(enAxisType.Scale_Y)].MeshNode.Placement.Position = new DVector3(0, -deltaY, 0);
                                            mAxisMeshDatas[(int)(enAxisType.Scale_Z)].MeshNode.Placement.Position = new DVector3(0, 0, -deltaZ);
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }

                FitAxisSize(viewport);
            }
            else
            {
                mRootNode.SetStyle(Scene.UNode.ENodeStyles.Invisible);
            }
        }

        Plane mCheckPlane = new Plane();
        unsafe bool PickPlanePos(Graphics.Pipeline.UViewportSlate viewport, int x, int y, in DVector3 planePos, in EngineNS.Vector3 planeNormal, out DVector3 resultPos)
        {
            resultPos = DVector3.Zero;
            EngineNS.Vector3 pickRay = -EngineNS.Vector3.UnitY;
            var camera = mCameraController.Camera.mCoreObject;
            var pos = viewport.Window2Viewport(new Vector2(x, y));
            var pickResult = camera.GetPickRay(ref pickRay, pos.X, pos.Y, viewport.ClientSize.X, viewport.ClientSize.Y);
            if (pickResult <= 0)
                return false;
            var start = camera.GetLocalPosition();
            var localPlanePos = planePos - camera.GetMatrixStartPosition();
            mCheckPlane.Normal = planeNormal;
            mCheckPlane.D = (float)-DVector3.Dot(localPlanePos, planeNormal);
            var end = start + pickRay * 10000;
            fixed (Plane* pPlane = &mCheckPlane)
            {
                Vector3 hitPos;
                if ((IntPtr)IDllImportApi.v3dxPlaneIntersectLine(&hitPos, pPlane, &start, &end) == IntPtr.Zero)
                    return false;
                resultPos = hitPos.AsDVector() + camera.GetMatrixStartPosition();
            }

            return true;
        }

        static bool LineInterCircle(in DVector3 ptStart, in DVector3 ptEnd, in DVector3 ptCenter, float Radius2,
           out DVector3 ptInter1, out DVector3 ptInter2)
        {
            ptInter1 = DVector3.Zero;
            ptInter2 = DVector3.Zero;
            float EPS = 0.00001f;
            //求线段的长度
            var d = ptEnd - ptStart;
            var fDis = d.Length();
            d.Normalize();
            var E = ptCenter - ptStart;
            double a;
            DVector3.Dot(in E, in d, out a);
            var a2 = a * a;
            var e2 = E.LengthSquared();
            if ((Radius2 - e2 + a2) < 0)
            {
                return false;
            }
            else
            {
                var f = System.Math.Sqrt(Radius2 - e2 + a2);
                var t = a - f;
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

        FTransform mStartTransAxisWorldTransform;
        FTransform mCurrentAxisStartTransform;
        DBoundingBox mStartEdgeBB;
        DVector3 mMouseStartLocation;
        DVector3 mMouseTransOffset;
        Vector3 mMouseStartScreenLocation;
        bool mIsTransAxisOperation = false;
        FTransform mPosNodeStartTransform;
        void StartTransAxis(Graphics.Pipeline.UViewportSlate viewport, in Bricks.Input.Event e)
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

                        var transformData = ((UPlacement)(mAxisMeshDatas[(int)mCurrentAxisType].MeshNode.Placement)).TransformData;
                        switch(mCurrentAxisType)
                        {
                            case enAxisType.Edge_X_Min:
                                transformData.mQuat *= Quaternion.RotationAxis(Vector3.Forward, (float)(Math.PI));
                                break;
                            case enAxisType.Edge_X_Max:
                                break;
                            case enAxisType.Edge_Y_Min:
                                transformData.mQuat *= Quaternion.RotationAxis(Vector3.Backward, (float)(Math.PI * 0.5f));
                                break;
                            case enAxisType.Edge_Y_Max:
                                transformData.mQuat *= Quaternion.RotationAxis(Vector3.Forward, (float)(Math.PI * 0.5f));
                                break;
                            case enAxisType.Edge_Z_Min:
                                transformData.mQuat *= Quaternion.RotationAxis(Vector3.Down, (float)(Math.PI * 0.5f));
                                break;
                            case enAxisType.Edge_Z_Max:
                                transformData.mQuat *= Quaternion.RotationAxis(Vector3.Up, (float)(Math.PI * 0.5f));
                                break;
                        }
                        mStartTransAxisWorldTransform = transformData;


                        var posNode = GetPosNode().Node;
                        if (posNode != null)
                        {
                            switch (mAxisSpace)
                            {
                                case enAxisSpace.Local:
                                    mCurrentAxisStartTransform = ((UPlacement)posNode.Placement).TransformData;
                                    mPosNodeStartTransform = mCurrentAxisStartTransform;
                                    break;
                                case enAxisSpace.World:
                                    mCurrentAxisStartTransform = FTransform.Identity;
                                    mPosNodeStartTransform = ((UPlacement)posNode.Placement).TransformData;
                                    break;
                            }
                        }
                        else
                        {
                            mCurrentAxisStartTransform = FTransform.Identity;
                            mPosNodeStartTransform = FTransform.Identity;
                        }
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

            var startWorldMat = Matrix.Transformation(mStartTransAxisWorldTransform.mScale, mStartTransAxisWorldTransform.mQuat, 
                mStartTransAxisWorldTransform.mPosition.ToSingleVector3());

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
                        PickPlanePos(viewport, e.MouseMotion.X, e.MouseMotion.Y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
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
                            var pos = viewport.Window2Viewport(new Vector2(e.MouseMotion.X, e.MouseMotion.Y));
                            var pickResult = camera.GetPickRay(ref pickRay, pos.X, pos.Y, viewport.ClientSize.X, viewport.ClientSize.Y);
                            pickRay.Normalize();
                            var camPos = camera.GetPosition();
                            var dirEnd = camPos + pickRay * 10000;
                            DVector3 inter1, inter2;
                            if (LineInterCircle(in camPos, in dirEnd, in mCurrentAxisStartTransform.mPosition, 2.0f, out inter1, out inter2))
                            {
                                mMouseStartLocation = (inter1 - camPos).Length() < (inter2 - camPos).Length() ? inter1 : inter2;
                            }
                        }
                        else
                        {
                            EngineNS.Vector3.TransformNormal(in axisDir, in startWorldMat, out planeAxis);
                            PickPlanePos(viewport, e.MouseMotion.X, e.MouseMotion.Y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                        }
                    }
                    break;
                case enAxisType.Move_Line_XY:
                case enAxisType.Move_Plane_XY:
                case enAxisType.Scale_Line_XY:
                case enAxisType.Scale_Plane_XY:
                    {
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitZ, in startWorldMat, out planeAxis);
                        PickPlanePos(viewport, e.MouseMotion.X, e.MouseMotion.Y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
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
                        PickPlanePos(viewport, e.MouseMotion.X, e.MouseMotion.Y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Move_Line_XZ:
                case enAxisType.Move_Plane_XZ:
                case enAxisType.Scale_Line_XZ:
                case enAxisType.Scale_Plane_XZ:
                    {
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitY, in startWorldMat, out planeAxis);
                        PickPlanePos(viewport, e.MouseMotion.X, e.MouseMotion.Y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
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
                        PickPlanePos(viewport, e.MouseMotion.X, e.MouseMotion.Y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Move_Line_YZ:
                case enAxisType.Move_Plane_YZ:
                case enAxisType.Scale_Line_YZ:
                case enAxisType.Scale_Plane_YZ:
                    {
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitX, in startWorldMat, out planeAxis);
                        PickPlanePos(viewport, e.MouseMotion.X, e.MouseMotion.Y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Scale_XYZ:
                    {
                        planeAxis = -cameraDirection;
                        PickPlanePos(viewport, e.MouseMotion.X, e.MouseMotion.Y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Edge_X_Min:
                //case enAxisType.Edge_X_MinPlane:
                case enAxisType.Edge_X_Max:
                //case enAxisType.Edge_X_MaxPlane:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitX, in startWorldMat, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, cameraDirection);
                        tempNormal.Normalize();
                        planeAxis = EngineNS.Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                        PickPlanePos(viewport, e.MouseMotion.X, e.MouseMotion.Y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Edge_Y_Min:
                //case enAxisType.Edge_Y_MinPlane:
                case enAxisType.Edge_Y_Max:
                //case enAxisType.Edge_Y_MaxPlane:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitY, in startWorldMat, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, cameraDirection);
                        tempNormal.Normalize();
                        planeAxis = EngineNS.Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                        PickPlanePos(viewport, e.MouseMotion.X, e.MouseMotion.Y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
                case enAxisType.Edge_Z_Min:
                //case enAxisType.Edge_Z_MinPlane:
                case enAxisType.Edge_Z_Max:
                //case enAxisType.Edge_Z_MaxPlane:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitZ, in startWorldMat, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, cameraDirection);
                        tempNormal.Normalize();
                        planeAxis = EngineNS.Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                        PickPlanePos(viewport, e.MouseMotion.X, e.MouseMotion.Y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
                    }
                    break;
            }

            mMouseStartScreenLocation = new Vector3(e.MouseMotion.X, e.MouseMotion.Y, 0.0f);
            #region Debug
            //if (mPlaneNode != null)
            //{
            //    mPlaneNode.Placement.Position = mMouseStartLocation;
            //    mPlaneNode.Placement.Quat = Quaternion.GetQuaternion(Vector3.Up, planeAxis);

            //    mPointNode.Placement.Position = mMouseStartLocation;
            //}
            #endregion
            mMouseTransOffset = mMouseStartLocation - mCurrentAxisStartTransform.mPosition;

            if(mSelectedNodes != null)
            {
                for (int i = 0; i < mSelectedNodes.Count; i++)
                {
                    mSelectedNodes[i].StartAbsTransform = ((UPlacement)mSelectedNodes[i].Node.Placement).AbsTransform;// .TransformData;
                    mSelectedNodes[i].StartTransform = ((UPlacement)mSelectedNodes[i].Node.Placement).TransformData;
                }
            }
        }

        bool mFirstTransAxis = false;
        async System.Threading.Tasks.Task TransAxis(Vector2 newMouseLoc, Graphics.Pipeline.UViewportSlate viewport)
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
                    await Thread.TtAsyncDummyClass.DummyFunc();
                }
                mFirstTransAxis = false;
            }

            //var startTransMat = Matrix.Transformation(mStartTransAxisWorldTransform.Scale, mStartTransAxisWorldTransform.Quat, mStartTransAxisWorldTransform.Position);
            var startTransMat = Matrix.Transformation(mCurrentAxisStartTransform.Scale, mCurrentAxisStartTransform.Quat, mCurrentAxisStartTransform.Position.ToSingleVector3());

            switch (mCurrentAxisType)
            {
                case enAxisType.Move_X:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitX, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        MoveWithAxis(in newMouseLoc, transAxis);
                    }
                    break;
                case enAxisType.Move_Y:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitY, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        MoveWithAxis(in newMouseLoc, transAxis);
                    }
                    break;
                case enAxisType.Move_Z:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitZ, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        MoveWithAxis(in newMouseLoc, transAxis);
                    }
                    break;
                case enAxisType.Move_Line_XY:
                case enAxisType.Move_Plane_XY:
                    {
                        Vector3 planeNormal;
                        Vector3.TransformNormal(in Vector3.UnitZ, in startTransMat, out planeNormal);
                        planeNormal.Normalize();
                        MoveWithPlane(in newMouseLoc, in planeNormal, viewport);
                    }
                    break;
                case enAxisType.Move_Line_XZ:
                case enAxisType.Move_Plane_XZ:
                    {
                        Vector3 planeNormal;
                        Vector3.TransformNormal(in Vector3.UnitY, in startTransMat, out planeNormal);
                        planeNormal.Normalize();
                        MoveWithPlane(in newMouseLoc, in planeNormal, viewport);
                    }
                    break;
                case enAxisType.Move_Line_YZ:
                case enAxisType.Move_Plane_YZ:
                    {
                        Vector3 planeNormal;
                        Vector3.TransformNormal(in Vector3.UnitX, in startTransMat, out planeNormal);
                        planeNormal.Normalize();
                        MoveWithPlane(in newMouseLoc, in planeNormal, viewport);
                    }
                    break;
                case enAxisType.Rot_X:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitX, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        RotWithAxis(in newMouseLoc, in transAxis, in Vector3.UnitX);
                    }
                    break;
                case enAxisType.Rot_Y:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitY, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        RotWithAxis(in newMouseLoc, in transAxis, in Vector3.UnitY);
                    }
                    break;
                case enAxisType.Rot_Z:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitZ, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        RotWithAxis(in newMouseLoc, in transAxis, in Vector3.UnitZ);
                    }
                    break;
                case enAxisType.Scale_X:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitX, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        ScaleWithAxis(in newMouseLoc, in transAxis, in Vector3.UnitX);
                        //ScaleWithAxis(in newMouseLoc, in Vector3.UnitX);
                    }
                    break;
                case enAxisType.Scale_Y:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitY, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        ScaleWithAxis(in newMouseLoc, in transAxis, in Vector3.UnitY);
                    }
                    break;
                case enAxisType.Scale_Z:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitZ, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        ScaleWithAxis(in newMouseLoc, in transAxis, in Vector3.UnitZ);
                    }
                    break;
                case enAxisType.Scale_Line_XY:
                case enAxisType.Scale_Plane_XY:
                    {
                        var axis = new Vector3(1.0f, 1.0f, 0.0f);
                        Vector3 transAxis;
                        Vector3.TransformNormal(in axis, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        ScaleWithAxis(in newMouseLoc, in transAxis, in axis);
                    }
                    break;
                case enAxisType.Scale_Line_XZ:
                case enAxisType.Scale_Plane_XZ:
                    {
                        var axis = new Vector3(1.0f, 0.0f, 1.0f);
                        Vector3 transAxis;
                        Vector3.TransformNormal(in axis, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        ScaleWithAxis(in newMouseLoc, in transAxis, in axis);
                    }
                    break;
                case enAxisType.Scale_Line_YZ:
                case enAxisType.Scale_Plane_YZ:
                    {
                        var axis = new Vector3(0.0f, 1.0f, 1.0f);
                        Vector3 transAxis;
                        Vector3.TransformNormal(in axis, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        ScaleWithAxis(in newMouseLoc, in transAxis, in axis);
                    }
                    break;
                case enAxisType.Scale_XYZ:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.One, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        ScaleWithAxis(in newMouseLoc, in transAxis, in Vector3.One);
                    }
                    break;
                case enAxisType.Edge_X_Min:
                case enAxisType.Edge_X_Max:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitX, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        EdgeWithAxis(in newMouseLoc, in transAxis, mCurrentAxisType);
                    }
                    break;
                case enAxisType.Edge_Y_Min:
                case enAxisType.Edge_Y_Max:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitY, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        EdgeWithAxis(in newMouseLoc, in transAxis, mCurrentAxisType);
                    }
                    break;
                case enAxisType.Edge_Z_Min:
                case enAxisType.Edge_Z_Max:
                    {
                        Vector3 transAxis;
                        Vector3.TransformNormal(in Vector3.UnitZ, in startTransMat, out transAxis);
                        transAxis.Normalize();
                        EdgeWithAxis(in newMouseLoc, in transAxis, mCurrentAxisType);
                    }
                    break;
            }
        }
        void MoveWithAxis(in Vector2 newMouseLoc, in Vector3 transAxisDir)
        {
            var camera = mCameraController.Camera.mCoreObject;
            if(camera.mIsOrtho)
            {
                throw new InvalidOperationException("没实现");
            }
            else
            {
                var cameraOffset = camera.GetMatrixStartPosition();
                var axisStartTransPos = mRootNode.Placement.Position + mMouseTransOffset;
                var c2vMat = camera.GetToViewPortMatrix();
                DVector3 screenAxisLoc;
                var curAxisPosition = axisStartTransPos;
                DVector3.TransformCoordinate(in curAxisPosition, in cameraOffset, in c2vMat, out screenAxisLoc);
                screenAxisLoc.Z = 0;
                var tag = axisStartTransPos + transAxisDir;
                DVector3 screenTag;
                DVector3.TransformCoordinate(in tag, in cameraOffset, in c2vMat, out screenTag);
                screenTag.Z = 0;
                var screenAxisDir = screenTag - screenAxisLoc;
                screenAxisDir.Normalize();

                var camRight = camera.GetRight();
                camRight.Normalize();
                var deltaPos = curAxisPosition + camRight;
                DVector3 screenDeltaPos;
                DVector3.TransformCoordinate(in deltaPos, in cameraOffset, in c2vMat, out screenDeltaPos);
                screenDeltaPos.Z = 0;
                var s2vDelta = camRight.Length() / (screenDeltaPos - screenAxisLoc).Length();
                var lenInScreen = DVector3.Dot((new DVector3(newMouseLoc.X, newMouseLoc.Y, 0) - mMouseStartScreenLocation), screenAxisDir);
                var len = lenInScreen * s2vDelta;// / mRootNodeScaleValue;

                if(HasSnapType(enSnapType.MoveGrid))
                {
                    len = (int)(len / mSnapGridSize) * mSnapGridSize;
                }

                #region Debug
                //mPlaneNode.Placement.Scale = new Vector3(len, 1, 1);
                //mPlaneNode.Placement.Position = transAxisDir * len * 0.5f + axisStartTransPos;
                #endregion

                var trans = transAxisDir.AsDVector() * len;
                var mat = DMatrix.Translate(trans);
                if (CenterAxisMode)
                {
                    DMatrix startTransAxisMat;
                    DMatrix.Transformation(in mStartTransAxisWorldTransform.mScale, in mStartTransAxisWorldTransform.mQuat, mStartTransAxisWorldTransform.mPosition, out startTransAxisMat);
                    var axisOrigionTransBeforeCenterMat = startTransAxisMat * mat;
                    DVector3 decomposePos;
                    axisOrigionTransBeforeCenterMat.Decompose(out mAxisOrigionTransformBeforeCenterOperation.mScale, 
                        out mAxisOrigionTransformBeforeCenterOperation.mQuat, out decomposePos);

                    mAxisOrigionTransformBeforeCenterOperation.mPosition = decomposePos;
                    mRootNode.Placement.Position = mCurrentAxisStartTransform.Position + trans;
                }
                else
                {
                    mRootNode.Placement.Position = mStartTransAxisWorldTransform.Position + trans;
                }

                if(mSelectedNodes != null)
                {
                    for (int i = 0; i < mSelectedNodes.Count; i++)
                    {
                        var data = mSelectedNodes[i];
                        DMatrix startTransMatrix;
                        DMatrix.Transformation(in data.StartAbsTransform.mScale, in data.StartAbsTransform.mQuat, in data.StartAbsTransform.mPosition, out startTransMatrix);

                        var transMat = startTransMatrix * mat;
                        if (data.Node.Parent != null)
                        {
                            //transMat = transMat * data.Node.Parent.Placement.AbsTransformInv;
                            var absMatrix = data.Node.Parent.Placement.AbsTransform.ToDMatrixNoScale();
                            absMatrix.Inverse();
                            transMat = transMat * absMatrix;
                        }
                        data.Node.Placement.Position = transMat.Translation;// new DVector3(transMat.M41, transMat.M42, transMat.M43);
                    }
                }
            }
        }

        void MoveWithPlane(in Vector2 newMouseLoc, in Vector3 planeNormal, Graphics.Pipeline.UViewportSlate viewport)
        {
            var camera = mCameraController.Camera.mCoreObject;
            if(camera.mIsOrtho)
            {
                throw new InvalidOperationException("没实现");
            }
            else
            {
                DVector3 mouseLoc;
                if(PickPlanePos(viewport, (int)newMouseLoc.X, (int)newMouseLoc.Y, in mCurrentAxisStartTransform.mPosition, in planeNormal, out mouseLoc))
                {
                    mRootNode.Placement.Position = mouseLoc - mMouseTransOffset;
                    var trans = mRootNode.Placement.Position - mCurrentAxisStartTransform.Position;
                    if (HasSnapType(enSnapType.MoveGrid))
                    {
                        trans.X = (int)(trans.X / mSnapGridSize) * mSnapGridSize;
                        trans.Y = (int)(trans.Y / mSnapGridSize) * mSnapGridSize;
                        trans.Z = (int)(trans.Z / mSnapGridSize) * mSnapGridSize;
                    }
                    var mat = DMatrix.Translate(trans);
                    if(CenterAxisMode)
                    {
                        DMatrix startTransAxisMat;
                        DMatrix.Transformation(in mStartTransAxisWorldTransform.mScale, 
                            in mStartTransAxisWorldTransform.mQuat, mStartTransAxisWorldTransform.mPosition, out startTransAxisMat);
                        var axisOrigionTransBeforeCenterMat = startTransAxisMat * mat;
                        axisOrigionTransBeforeCenterMat.Decompose(out mAxisOrigionTransformBeforeCenterOperation.mScale, out mAxisOrigionTransformBeforeCenterOperation.mQuat, out mAxisOrigionTransformBeforeCenterOperation.mPosition);

                        mRootNode.Placement.Position = trans + mCurrentAxisStartTransform.Position;
                    }
                    else
                    {
                        mRootNode.Placement.Position = trans + mStartTransAxisWorldTransform.Position;
                    }

                    if (mSelectedNodes != null)
                    {
                        for (int i = 0; i < mSelectedNodes.Count; i++)
                        {
                            var data = mSelectedNodes[i];
                            DMatrix startTransMatrix;
                            DMatrix.Transformation(in data.StartAbsTransform.mScale, in data.StartAbsTransform.mQuat, in data.StartAbsTransform.mPosition, out startTransMatrix);

                            var transMat = startTransMatrix * mat;
                            if(data.Node.Parent != null)
                            {
                                //transMat = transMat * data.Node.Parent.Placement.AbsTransformInv;
                                var absMatrix = data.Node.Parent.Placement.AbsTransform.ToDMatrixNoScale();
                                absMatrix.Inverse();
                                transMat = transMat * absMatrix;
                            }
                            data.Node.Placement.Position = transMat.Translation;// new DVector3(transMat.M41, transMat.M42, transMat.M43);
                        }
                    }
                }
            }
        }
        void RotWithAxis(in Vector2 newMouseLoc, in Vector3 rotAxisDir, in Vector3 oriAxisDir)
        {
            var camera = mCameraController.Camera.mCoreObject;
            if(camera.mIsOrtho)
            {
                throw new InvalidOperationException("没实现");
            }
            else
            {
                var cameraOffset = camera.GetMatrixStartPosition();
                var c2vMat = camera.GetToViewPortMatrix();
                var tangentDir = Vector3.Cross(rotAxisDir, (mMouseStartLocation - mCurrentAxisStartTransform.Position).ToSingleVector3());
                tangentDir.Normalize();

                var camDir = camera.GetDirection();
                camDir.Normalize();
                var camRight = camera.GetRight();
                camRight.Normalize();
                var camUp = camera.GetUp();
                camUp.Normalize();
                DVector3 screenMouseStartLoc;
                DVector3.TransformCoordinate(in mMouseStartLocation, in cameraOffset, in c2vMat, out screenMouseStartLoc);
                var tangTag = mMouseStartLocation + tangentDir;
                DVector3 screenTangTag;
                DVector3.TransformCoordinate(in tangTag, in cameraOffset, in c2vMat, out screenTangTag);
                screenTangTag.Z = 0;
                var screenTangDir = screenTangTag - screenMouseStartLoc;
                screenTangDir.Normalize();

                var deltaPos = mMouseStartLocation + camRight;
                DVector3 screenDeltaPos;
                DVector3.TransformCoordinate(in deltaPos, in cameraOffset, in c2vMat, out screenDeltaPos);
                screenDeltaPos.Z = 0;
                screenDeltaPos = screenDeltaPos - screenMouseStartLoc;
                var s2vDelta = camRight.Length() / screenDeltaPos.Length();

                var lenInScreen = DVector3.Dot((new DVector3(newMouseLoc.X, newMouseLoc.Y, 0) - mMouseStartScreenLocation), screenTangDir);
                var len = lenInScreen * s2vDelta;

                var radius = (mMouseStartLocation - mCurrentAxisStartTransform.Position).Length();
                var angle = len / radius;

                if(HasSnapType(enSnapType.RotAngle))
                {
                    var snapAgl = (float)(mSnapRotAngle / 180 * Math.PI);
                    angle = (int)(angle / snapAgl) * snapAgl;
                }

                var mat = DMatrix.Translate(-mStartTransAxisWorldTransform.mPosition) * DMatrix.RotationAxis(rotAxisDir.AsDVector(), angle) * DMatrix.Translate(mStartTransAxisWorldTransform.mPosition);
                if(CenterAxisMode)
                {
                    DMatrix startTransAxisMat;
                    DMatrix.Transformation(in mStartTransAxisWorldTransform.mScale, in mStartTransAxisWorldTransform.mQuat, mStartTransAxisWorldTransform.mPosition, out startTransAxisMat);
                    var axisOrigionTransBeforeCenterMat = startTransAxisMat * mat;
                    axisOrigionTransBeforeCenterMat.Decompose(out mAxisOrigionTransformBeforeCenterOperation.mScale, out mAxisOrigionTransformBeforeCenterOperation.mQuat, out mAxisOrigionTransformBeforeCenterOperation.mPosition);

                    var tempMat = DMatrix.Translate(-mCurrentAxisStartTransform.Position) * DMatrix.RotationAxis(rotAxisDir.AsDVector(), angle) * DMatrix.Translate(mCurrentAxisStartTransform.Position);
                    tempMat = startTransAxisMat * tempMat;
                    DVector3 tPos;
                    Vector3 tScale;
                    Quaternion tRot;
                    tempMat.Decompose(out tScale, out tRot, out tPos);
                    //mRootNode.Placement.Position = tPos;
                    mRootNode.Placement.Quat = tRot;
                    //mRootNode.Placement.Scale = tScale;
                }
                else
                {
                    DMatrix startTransAxisMat;
                    DMatrix.Transformation(in mStartTransAxisWorldTransform.mScale, in mStartTransAxisWorldTransform.mQuat, in mStartTransAxisWorldTransform.mPosition, out startTransAxisMat);
                    var tempMat = startTransAxisMat * mat;
                    DVector3 tPos;
                    Vector3 tScale;
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
                        DMatrix startTransMatrix;
                        DMatrix.Transformation(in data.StartAbsTransform.mScale, in data.StartAbsTransform.mQuat, in data.StartAbsTransform.mPosition, out startTransMatrix);

                        var transMat = startTransMatrix * mat;
                        if (data.Node.Parent != null)
                        {
                            //transMat = transMat * data.Node.Parent.Placement.AbsTransformInv;
                            var absMatrix = data.Node.Parent.Placement.AbsTransform.ToDMatrixNoScale();
                            absMatrix.Inverse();
                            transMat = transMat * absMatrix;
                        }
                        DVector3 pos;
                        Vector3 scale;
                        Quaternion rot;
                        transMat.Decompose(out scale, out rot, out pos);
                        data.Node.Placement.Position = pos;
                        data.Node.Placement.Quat = rot;
                        data.Node.Placement.Scale = scale;
                    }
                }
            }
        }
        void ScaleWithAxis(in Vector2 newMouseLoc, in Vector3 scaleAxisDir, in Vector3 origAxisDir)
        {
            var camera = mCameraController.Camera.mCoreObject;
            if(camera.mIsOrtho)
            {
                throw new InvalidOperationException("没实现");
            }
            else
            {
                var cameraOffset = camera.GetMatrixStartPosition();
                var axisStartTransPos = mRootNode.Placement.Position + mMouseTransOffset;
                var c2vMat = camera.GetToViewPortMatrix();
                DVector3 screenAxisLoc;
                var curAxisPosition = axisStartTransPos;
                DVector3.TransformCoordinate(in curAxisPosition, in cameraOffset, in c2vMat, out screenAxisLoc);
                screenAxisLoc.Z = 0;
                var tag = axisStartTransPos + scaleAxisDir;
                DVector3 screenTag;
                DVector3.TransformCoordinate(in tag, in cameraOffset, in c2vMat, out screenTag);
                screenTag.Z = 0;
                var screenAxisDir = screenTag - screenAxisLoc;
                screenAxisDir.Normalize();

                var camRight = camera.GetRight();
                camRight.Normalize();
                var deltaPos = curAxisPosition + camRight;
                DVector3 screenDeltaPos;
                DVector3.TransformCoordinate(in deltaPos, in cameraOffset, in c2vMat, out screenDeltaPos);
                screenDeltaPos.Z = 0;
                var s2vDelta = camRight.Length() / (screenDeltaPos - screenAxisLoc).Length();
                var lenInScreen = DVector3.Dot((new DVector3(newMouseLoc.X, newMouseLoc.Y, 0) - mMouseStartScreenLocation), screenAxisDir);
                var len = lenInScreen * s2vDelta / mRootNodeScaleValue;

                if(HasSnapType(enSnapType.ScaleValue))
                {
                    len = (int)(len / mSnapScaleDelta) * mSnapScaleDelta;
                }

                var mat = DMatrix.Identity;
                var scale = origAxisDir * (float)len + Vector3.One;
                //DMatrix.Transformation(mCurrentAxisStartTransform.Position,
                //    mCurrentAxisStartTransform.Quat, in scale, mCurrentAxisStartTransform.Position, in Quaternion.Identity, in DVector3.Zero, out mat);

                if (mSelectedNodes != null)
                {
                    for(int i=0; i<mSelectedNodes.Count; i++)
                    {
                        var data = mSelectedNodes[i];
                        var delta = data.StartAbsTransform.mPosition - mCurrentAxisStartTransform.Position;
                        //var newPos = DVector3.TransformCoordinate(data.StartAbsTransform.mPosition, mat);
                        var newPos = delta * scale + mCurrentAxisStartTransform.Position;
                        if (data.Node.Parent != null)
                        {
                            newPos -= data.Node.Parent.Placement.AbsTransform.Position;
                        }

                        data.Node.Placement.Position = newPos;
                        data.Node.Placement.Scale = origAxisDir * (float)len + data.StartTransform.Scale;
                    }
                }
            }
        }
        void EdgeWithAxis(in Vector2 newMouseLoc, in Vector3 edgeAxisDir, enAxisType axisType)
        {
            var camera = mCameraController.Camera.mCoreObject;
            if (camera.mIsOrtho)
            {
                throw new InvalidOperationException("没实现");
            }
            else
            {
                var cameraOffset = camera.GetMatrixStartPosition();
                var axisStartTransPos = mStartTransAxisWorldTransform.Position + mMouseTransOffset;
                var c2vMat = camera.GetToViewPortMatrix();
                DVector3 screenAxisLoc;
                var curAxisPosition = axisStartTransPos;
                DVector3.TransformCoordinate(in curAxisPosition, in cameraOffset, in c2vMat, out screenAxisLoc);
                screenAxisLoc.Z = 0;
                var tag = axisStartTransPos + edgeAxisDir;
                DVector3 screenTag;
                DVector3.TransformCoordinate(in tag, cameraOffset, in c2vMat, out screenTag);
                screenTag.Z = 0;
                var screenAxisDir = screenTag - screenAxisLoc;
                screenAxisDir.Normalize();

                var camRight = camera.GetRight();
                camRight.Normalize();
                var deltaPos = curAxisPosition + camRight;
                DVector3 screenDeltaPos;
                DVector3.TransformCoordinate(in deltaPos, cameraOffset, in c2vMat, out screenDeltaPos);
                screenDeltaPos.Z = 0;
                var s2vDelta = camRight.Length() / (screenDeltaPos - screenAxisLoc).Length();
                var lenInScreen = DVector3.Dot((new DVector3(newMouseLoc.X, newMouseLoc.Y, 0) - mMouseStartScreenLocation), screenAxisDir);
                var len = lenInScreen * s2vDelta;

                var tempBB = mStartEdgeBB;
                var center = tempBB.GetCenter();
                var transCenter = center;
                var scale = Vector3.One;
                float scaleDelta = 1.0f;
                var oldSize = tempBB.GetSize();
                var ep = 0.0001f;
                switch(axisType)
                {
                    case enAxisType.Edge_X_Min:
                        {
                            var delta = tempBB.Minimum.X + len;
                            if (delta >= tempBB.Maximum.X)
                                delta = tempBB.Maximum.X - ep;
                            if (!CenterAxisMode)
                                tempBB.Minimum.X = (float)delta;
                            scaleDelta = (float)((tempBB.Maximum.X - delta) / oldSize.X);
                            switch(mAxisSpace)
                            {
                                case enAxisSpace.Local:
                                   scale = new Vector3(scaleDelta, 1, 1);
                                   break;
                                case enAxisSpace.World:
                                    {
                                        var posNodeRotInv = mPosNodeStartTransform.mQuat.Inverse();
                                        var scaleDir = Vector3.TransformCoordinate(edgeAxisDir, posNodeRotInv);
                                        scaleDir = new Vector3(Math.Abs(scaleDir.X), Math.Abs(scaleDir.Y), Math.Abs(scaleDir.Z));
                                        scale = scaleDir * (scaleDelta - 1) + Vector3.One;
                                    }
                                    break;
                            }
                            transCenter = new DVector3(tempBB.Maximum.X, center.Y, center.Z);
                        }
                        break;
                    case enAxisType.Edge_X_Max:
                        {
                            var delta = tempBB.Maximum.X + len;
                            if (delta <= tempBB.Minimum.X)
                                delta = tempBB.Minimum.X + ep;
                            if (!CenterAxisMode)
                                tempBB.Maximum.X = (float)delta;
                            scaleDelta = (float)((delta - tempBB.Minimum.X) / oldSize.X);
                            switch(mAxisSpace)
                            {
                                case enAxisSpace.Local:
                                    scale = new Vector3((float)((delta - tempBB.Minimum.X) / oldSize.X), 1, 1);
                                    break;
                                case enAxisSpace.World:
                                    {
                                        var posNodeRotInv = mPosNodeStartTransform.mQuat.Inverse();
                                        var scaleDir = Vector3.TransformCoordinate(edgeAxisDir, posNodeRotInv);
                                        scaleDir = new Vector3(Math.Abs(scaleDir.X), Math.Abs(scaleDir.Y), Math.Abs(scaleDir.Z));
                                        scale = scaleDir * (scaleDelta - 1) + Vector3.One;
                                    }
                                    break;
                            }
                            transCenter = new DVector3(tempBB.Minimum.X, center.Y, center.Z);
                        }
                        break;
                    case enAxisType.Edge_Y_Min:
                        {
                            var delta = tempBB.Minimum.Y + len;
                            if (delta >= tempBB.Maximum.Y)
                                delta = tempBB.Maximum.Y - ep;
                            if (!CenterAxisMode)
                                tempBB.Minimum.Y = (float)len;
                            scaleDelta = (float)((tempBB.Maximum.Y - delta) / oldSize.Y);
                            switch(mAxisSpace)
                            {
                                case enAxisSpace.Local:
                                    scale = new EngineNS.Vector3(1, (float)((tempBB.Maximum.Y - delta) / oldSize.Y), 1);
                                    break;
                                case enAxisSpace.World:
                                    {
                                        var posNodeRotInv = mPosNodeStartTransform.mQuat.Inverse();
                                        var scaleDir = Vector3.TransformCoordinate(edgeAxisDir, posNodeRotInv);
                                        scaleDir = new Vector3(Math.Abs(scaleDir.X), Math.Abs(scaleDir.Y), Math.Abs(scaleDir.Z));
                                        scale = scaleDir * (scaleDelta - 1) + Vector3.One;
                                    }
                                    break;
                            }
                            transCenter = new EngineNS.DVector3(center.X, tempBB.Maximum.Y, center.Z);
                        }
                        break;
                    case enAxisType.Edge_Y_Max:
                        {
                            var delta = tempBB.Maximum.Y + len;
                            if (delta <= tempBB.Minimum.Y)
                                delta = tempBB.Minimum.Y + ep;
                            if (!CenterAxisMode)
                                tempBB.Maximum.Y = (float)len;
                            scaleDelta = (float)((delta - tempBB.Minimum.Y) / oldSize.Y);
                            switch(mAxisSpace)
                            {
                                case enAxisSpace.Local:
                                    scale = new EngineNS.Vector3(1, (float)((delta - tempBB.Minimum.Y) / oldSize.Y), 1);
                                    break;
                                case enAxisSpace.World:
                                    {
                                        var posNodeRotInv = mPosNodeStartTransform.mQuat.Inverse();
                                        var scaleDir = Vector3.TransformCoordinate(edgeAxisDir, posNodeRotInv);
                                        scaleDir = new Vector3(Math.Abs(scaleDir.X), Math.Abs(scaleDir.Y), Math.Abs(scaleDir.Z));
                                        scale = scaleDir * (scaleDelta - 1) + Vector3.One;
                                    }
                                    break;
                            }
                            transCenter = new EngineNS.DVector3(center.X, tempBB.Minimum.Y, center.Z);
                        }
                        break;
                    case enAxisType.Edge_Z_Min:
                        {
                            var delta = tempBB.Minimum.Z + len;
                            if (delta >= tempBB.Maximum.Z)
                                delta = tempBB.Maximum.Z - ep;
                            if (!CenterAxisMode)
                                tempBB.Minimum.Z = (float)len;
                            scaleDelta = (float)((tempBB.Maximum.Z - delta) / oldSize.Z);
                            switch(mAxisSpace)
                            {
                                case enAxisSpace.Local:
                                    scale = new EngineNS.Vector3(1, 1, (float)((tempBB.Maximum.Z - delta) / oldSize.Z));
                                    break;
                                case enAxisSpace.World:
                                    {
                                        var posNodeRotInv = mPosNodeStartTransform.mQuat.Inverse();
                                        var scaleDir = Vector3.TransformCoordinate(edgeAxisDir, posNodeRotInv);
                                        scaleDir = new Vector3(Math.Abs(scaleDir.X), Math.Abs(scaleDir.Y), Math.Abs(scaleDir.Z));
                                        scale = scaleDir * (scaleDelta - 1) + Vector3.One;
                                    }
                                    break;
                            }
                            transCenter = new EngineNS.DVector3(center.X, center.Y, tempBB.Maximum.Z);
                        }
                        break;
                    case enAxisType.Edge_Z_Max:
                        {
                            var delta = tempBB.Maximum.Z + len;
                            if (delta <= tempBB.Minimum.Z)
                                delta = tempBB.Minimum.Z + ep;
                            if (!CenterAxisMode)
                                tempBB.Maximum.Z = (float)len;
                            scaleDelta = (float)((delta - tempBB.Minimum.Z) / oldSize.Z);
                            switch(mAxisSpace)
                            {
                                case enAxisSpace.Local:
                                    scale = new EngineNS.Vector3(1, 1, (float)((delta - tempBB.Minimum.Z) / oldSize.Z));
                                    break;
                                case enAxisSpace.World:
                                    {
                                        var posNodeRotInv = mPosNodeStartTransform.mQuat.Inverse();
                                        var scaleDir = Vector3.TransformCoordinate(edgeAxisDir, posNodeRotInv);
                                        scaleDir = new Vector3(Math.Abs(scaleDir.X), Math.Abs(scaleDir.Y), Math.Abs(scaleDir.Z));
                                        scale = scaleDir * (scaleDelta - 1) + Vector3.One;
                                    }
                                    break;
                            }
                            transCenter = new EngineNS.DVector3(center.X, center.Y, tempBB.Minimum.Z);
                        }
                        break;
                }
                DMatrix transMat = DMatrix.Identity;

                DMatrix tempMat;
                DMatrix.Transformation(in Vector3.One, in mCurrentAxisStartTransform.mQuat, in mCurrentAxisStartTransform.mPosition, out tempMat);
                DVector3.TransformCoordinate(transCenter, in tempMat, out transCenter);
                #region Debug
                //mPointNode.Placement.Position = transCenter;
                //mPlaneNode.Placement.Position = transCenter;
                #endregion
                DMatrix.Transformation(in transCenter, mPosNodeStartTransform.Quat, in scale, in transCenter, in Quaternion.Identity, in DVector3.Zero, out transMat);

                if (mSelectedNodes != null)
                {
                    for (int i = 0; i < mSelectedNodes.Count; i++)
                    {
                        DMatrix startMat;
                        DMatrix.Transformation(in mSelectedNodes[i].StartAbsTransform.mScale, in mSelectedNodes[i].StartAbsTransform.mQuat, in mSelectedNodes[i].StartAbsTransform.mPosition, out startMat);
                        var nodeMat = startMat * transMat;// mLastSelectedActorMatWithoutScaleInv * transMat * mLastSelectedActorMatWithoutScale;
                        if(mSelectedNodes[i].Node.Parent != null)
                        {
                            //nodeMat = nodeMat * mSelectedNodes[i].Node.Parent.Placement.AbsTransformInv;
                            var absMatrix = mSelectedNodes[i].Node.Parent.Placement.AbsTransform.ToDMatrixNoScale();
                            absMatrix.Inverse();
                            transMat = transMat * absMatrix;
                        }
                        DVector3 finalPos;
                        Vector3 finalScale;
                        Quaternion finalRot;
                        nodeMat.Decompose(out finalScale, out finalRot, out finalPos);
                        mSelectedNodes[i].Node.Placement.Position = finalPos;
                        mSelectedNodes[i].Node.Placement.Quat = finalRot;
                        mSelectedNodes[i].Node.Placement.Scale = finalScale;
                    }
                }

                UpdateEdgeBB();
                UpdateEdgeAxisTransform();
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
        public unsafe bool OnDrawUI(Graphics.Pipeline.UViewportSlate slate, in Vector2 startDrawPos)
        {
            var io = ImGuiAPI.GetIO();
            var oldCaptureValue = io.WantCaptureMouse;
            io.WantCaptureMouse = true;

            bool retValue = false;
            ImGuiAPI.SetCursorScreenPos(startDrawPos);

            slate.IsHoverGuiItem = false;

            Vector2 btnSize = new Vector2(24, 24);
            var n = mAxisOperationType == enAxisOperationType.Select;
            if(ImGuiAPI.ToggleButton("N", ref n, in btnSize, 0))
            {
                SetAxisOperationType(enAxisOperationType.Select);
                retValue = true;
            }
            slate.IsHoverGuiItem |= ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            ImGuiAPI.SameLine(0, 4);
            var m = mAxisOperationType == enAxisOperationType.Move;
            if(ImGuiAPI.ToggleButton("M", ref m, in btnSize, 0))
            {
                SetAxisOperationType(enAxisOperationType.Move);
                retValue = true;
            }
            slate.IsHoverGuiItem |= ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            ImGuiAPI.SameLine(0, 4);
            var r = mAxisOperationType == enAxisOperationType.Rot;
            if (ImGuiAPI.ToggleButton("R", ref r, in btnSize, 0))
            {
                SetAxisOperationType(enAxisOperationType.Rot);
                retValue = true;
            }
            slate.IsHoverGuiItem |= ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            ImGuiAPI.SameLine(0, 4);
            var s = mAxisOperationType == enAxisOperationType.Scale;
            if (ImGuiAPI.ToggleButton("S", ref s, in btnSize, 0))
            {
                SetAxisOperationType(enAxisOperationType.Scale);
                retValue = true;
            }
            slate.IsHoverGuiItem |= ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            ImGuiAPI.SameLine(0, 4);
            var e = mAxisOperationType == enAxisOperationType.Edge;
            if (ImGuiAPI.ToggleButton("E", ref e, in btnSize, 0))
            {
                SetAxisOperationType(enAxisOperationType.Edge);
                retValue = true;
            }
            slate.IsHoverGuiItem |= ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            ImGuiAPI.SameLine(0, 10);
            var ls = mAxisSpace == enAxisSpace.Local;
            if(ImGuiAPI.ToggleButton("L", ref ls, in btnSize, 0))
            {
                SetAxisSpace(enAxisSpace.Local);
                retValue = true;
            }
            slate.IsHoverGuiItem |= ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            switch (mAxisOperationType)
            {
                case enAxisOperationType.Scale:
                case enAxisOperationType.Edge:
                    break;
                default:
                    {
                        ImGuiAPI.SameLine(0, 4);
                        var ws = mAxisSpace == enAxisSpace.World;
                        if (ImGuiAPI.ToggleButton("W", ref ws, in btnSize, 0))
                        {
                            SetAxisSpace(enAxisSpace.World);
                            retValue = true;
                        }
                        slate.IsHoverGuiItem |= ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
                    }
                    break;
            }
            ImGuiAPI.SameLine(0, 10);
            if (ImGuiAPI.ToggleButton("C", ref mCenterAxisMode, in btnSize, 0))
            {
                CenterAxisMode = mCenterAxisMode;
                retValue = true;
            }
            slate.IsHoverGuiItem |= ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);

            ImGuiAPI.SameLine(0, 10);
            var snapGrid = HasSnapType(enSnapType.MoveGrid);
            if (ImGuiAPI.ToggleButton("SM", ref snapGrid, in btnSize, 0))
            {
                SetSnapType(enSnapType.MoveGrid);
                retValue = true;
            }
            slate.IsHoverGuiItem |= ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);

            var minValue = float.MinValue;
            var maxValue = float.MaxValue;
            ImGuiAPI.SameLine(0, 2);
            ImGuiAPI.SetNextItemWidth(50.0f);
            fixed(float* snapGridPtr = &mSnapGridSize)
                ImGuiAPI.DragScalar2("##SnapGrid", ImGuiDataType_.ImGuiDataType_Float, snapGridPtr, 0.1f, &minValue, &maxValue, "%0.3f", ImGuiSliderFlags_.ImGuiSliderFlags_None);

            ImGuiAPI.SameLine(0, 4);
            var snapRot = HasSnapType(enSnapType.RotAngle);
            if (ImGuiAPI.ToggleButton("SR", ref snapRot, in btnSize, 0))
            {
                SetSnapType(enSnapType.RotAngle);
                retValue = true;
            }
            ImGuiAPI.SameLine(0, 2);
            ImGuiAPI.SetNextItemWidth(50.0f);
            fixed (float* snapRotPtr = &mSnapRotAngle)
                ImGuiAPI.DragScalar2("##SnapRot", ImGuiDataType_.ImGuiDataType_Float, snapRotPtr, 0.1f, &minValue, &maxValue, "%0.3f", ImGuiSliderFlags_.ImGuiSliderFlags_None);

            ImGuiAPI.SameLine(0, 4);
            var snapScale = HasSnapType(enSnapType.ScaleValue);
            if(ImGuiAPI.ToggleButton("SS", ref snapScale, in btnSize, 0))
            {
                SetSnapType(enSnapType.ScaleValue);
                retValue = true;
            }
            ImGuiAPI.SameLine(0, 2);
            ImGuiAPI.SetNextItemWidth(100.0f);
            fixed (float* snapScalePtr = &mSnapScaleDelta)
                ImGuiAPI.DragScalar2("##SnapScale", ImGuiDataType_.ImGuiDataType_Float, snapScalePtr, 0.1f, &minValue, &maxValue, "%0.3f", ImGuiSliderFlags_.ImGuiSliderFlags_None);

            io.WantCaptureMouse = oldCaptureValue;

            return retValue;
        }
        #endregion
    }
}
