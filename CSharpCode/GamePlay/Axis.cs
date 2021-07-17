using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public partial class UAxis
    {
        public readonly static RName mAxisMaterial_Focus = RName.GetRName(@"axis\axis_focus_matins.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_Face_Focus = RName.GetRName(@"axis\axis_face_focus_matins.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_Center = RName.GetRName(@"axis\axis_all.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_X = RName.GetRName(@"axis\axis_x_matins.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_Y = RName.GetRName(@"axis\axis_y_matins.uminst", RName.ERNameType.Game);
        public readonly static RName mAxisMaterial_Z = RName.GetRName(@"axis\axis_z_matins.uminst", RName.ERNameType.Game);
        //public readonly RName mAxisMaterial_TX = RName.GetRName(@"editor\axis\axismaterial_tx.instmtl", RName.ERNameType.Game);
        //public readonly RName mAxisMaterial_TY = RName.GetRName(@"editor\axis\axismaterial_ty.instmtl", RName.ERNameType.Game);
        //public readonly RName mAxisMaterial_TZ = RName.GetRName(@"editor\axis\axismaterial_tz.instmtl", RName.ERNameType.Game);

        public readonly static RName mAxisMeshMoveAll = RName.GetRName(@"axis\moveall.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshMoveX = RName.GetRName(@"axis\movex.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshMoveXY = RName.GetRName(@"axis\movexy.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshMoveXY_Line = RName.GetRName(@"axis\movexy_line.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshRotX = RName.GetRName(@"axis\rotxy.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshScaleAll = RName.GetRName(@"axis\scaleall.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshScaleX = RName.GetRName(@"axis\scalex.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshScaleXY = RName.GetRName(@"axis\scalexy.vms", RName.ERNameType.Game);
        public readonly static RName mAxisMeshScaleXY_Line = RName.GetRName(@"axis\scalexy_line.vms", RName.ERNameType.Game);

        enum enAxisType
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
            Rot_Line_XY = Rot_Start,
            Rot_Line_XZ,
            Rot_Line_YZ,
            Rot_Plane_XY,
            Rot_Plane_XZ,
            Rot_Plane_YZ,
            Rot_End = Rot_Plane_YZ,

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

        public enum enAxisOperationType
        {
            Select = 0,
            Move = 1,
            Rot = 2,
            Scale = 3,
            Edge = 4,
        }
        enAxisOperationType mAxisOperationType = enAxisOperationType.Select;

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

        class AxisData
        {
            public Scene.UMeshNode MeshNode;
            public Matrix OffsetMatrix;
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
                        axisMesh = await GetAxisMesh(mAxisMeshMoveX, mAxisMaterial_X);
                        meshNodeData.MeshName = mAxisMeshMoveX;
                        meshNodeData.Name = mAxisMeshMoveX.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_X),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                        };
                        break;
                    case enAxisType.Move_Y:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveX, mAxisMaterial_Y);
                        rot = Quaternion.RotationAxis(Vector3.Backward, (float)(Math.PI * 0.5f));
                        meshNodeData.MeshName = mAxisMeshMoveX;
                        meshNodeData.Name = mAxisMeshMoveX.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Y),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                        };
                        break;
                    case enAxisType.Move_Z:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveX, mAxisMaterial_Z);
                        rot = Quaternion.RotationAxis(Vector3.Down, (float)(Math.PI * 0.5f));
                        meshNodeData.MeshName = mAxisMeshMoveX;
                        meshNodeData.Name = mAxisMeshMoveX.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Z),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                        };
                        break;
                    case enAxisType.Move_Line_XY:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveXY_Line, mAxisMaterial_X, mAxisMaterial_Y);
                        meshNodeData.MeshName = mAxisMeshMoveXY_Line;
                        meshNodeData.Name = mAxisMeshMoveXY_Line.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_X),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Y),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                        };
                        break;
                    case enAxisType.Move_Line_XZ:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveXY_Line, mAxisMaterial_X, mAxisMaterial_Z);
                        meshNodeData.MeshName = mAxisMeshMoveXY_Line;
                        meshNodeData.Name = mAxisMeshMoveXY_Line.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_X),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Z),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                        };
                        break;
                    case enAxisType.Move_Line_YZ:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveXY_Line, mAxisMaterial_Z, mAxisMaterial_Y);
                        meshNodeData.MeshName = mAxisMeshMoveXY_Line;
                        meshNodeData.Name = mAxisMeshMoveXY_Line.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Z),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Y),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                        };
                        break;
                    case enAxisType.Move_Plane_XY:
                        axisMesh = await GetAxisMesh(mAxisMeshMoveXY, mAxisMaterial_Focus);
                        meshNodeData.MeshName = mAxisMeshMoveXY;
                        meshNodeData.Name = mAxisMeshMoveXY.Name;
                        NormalMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                        };
                        FocusMaterials = new Graphics.Pipeline.Shader.UMaterial[]
                        {
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
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
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
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
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
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
                            await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(mAxisMaterial_Focus),
                        };
                        break;
                    case enAxisType.Rot_Line_XY: break;
                    case enAxisType.Rot_Line_XZ: break;
                    case enAxisType.Rot_Line_YZ: break;
                    case enAxisType.Rot_Plane_XY: break;
                    case enAxisType.Rot_Plane_XZ: break;
                    case enAxisType.Rot_Plane_YZ: break;
                    case enAxisType.Scale_X: break;
                    case enAxisType.Scale_Y: break;
                    case enAxisType.Scale_Z: break;
                    case enAxisType.Scale_Line_XY: break;
                    case enAxisType.Scale_Line_XZ: break;
                    case enAxisType.Scale_Line_YZ: break;
                    case enAxisType.Scale_Plane_XY: break;
                    case enAxisType.Scale_Plane_XZ: break;
                    case enAxisType.Scale_Plane_YZ: break;
                    case enAxisType.Scale_XYZ: break;
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

        GamePlay.UWorld mHostWorld;
        Scene.UNode mRootNode;
        bool mInitialized = false;

        public async System.Threading.Tasks.Task Initialize(GamePlay.UWorld world)
        {
            if (world == mHostWorld && mInitialized)
                return;

            mHostWorld = world;

            for(var i=enAxisType.AxisStart; i<=enAxisType.AxisEnd; i++)
            {
                var axisData = new AxisData();
                await axisData.Initialize(i, mHostWorld);
                mAxisMeshDatas.Add(axisData);
            }

            mRootNode = world.Root.NewNode(typeof(Scene.UNode), new GamePlay.Scene.UNodeData(), Scene.EBoundVolumeType.None, typeof(GamePlay.UPlacement));
            mRootNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
            mRootNode.IsCastShadow = false;
            mRootNode.Parent = world.Root;
            mInitialized = true;

            /////////////////////////////////////
            SetAxisOperationType(enAxisOperationType.Move);
            /////////////////////////////////////
        }

        GamePlay.Scene.UNode[] mOperationNodes;
        public void SetOperationNodes(params GamePlay.Scene.UNode[] nodes)
        {
            mOperationNodes = nodes;
            // 更新位置
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
            }
        }

        public void OnEvent(Graphics.Pipeline.UViewportSlate viewport, in SDL2.SDL.SDL_Event e)
        {
            if (e.type == SDL2.SDL.SDL_EventType.SDL_MOUSEMOTION)
            {
                var edtorPolicy = viewport.RenderPolicy as Graphics.Pipeline.Mobile.UMobileEditorFSPolicy;
                if (edtorPolicy != null)
                {
                    var pos = viewport.Window2Viewport(new Vector2((float)e.motion.x, (float)e.motion.y));
                    var hitObj = edtorPolicy.GetHitproxy((uint)pos.X, (uint)pos.Y);
                    if(hitObj != null)
                    {
                        switch (mAxisOperationType)
                        {
                            case enAxisOperationType.Move:
                                {
                                    for (int i = (int)enAxisType.Move_Start; i < (int)enAxisType.Move_End; i++)
                                    {
                                        if (mAxisMeshDatas[i].MeshNode.HitProxy.ProxyId == hitObj.HitProxy.ProxyId)
                                            mAxisMeshDatas[i].Focused = true;
                                        else
                                            mAxisMeshDatas[i].Focused = false;
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        for(int i=(int)enAxisType.AxisStart; i<(int)enAxisType.AxisEnd; i++)
                        {
                            mAxisMeshDatas[i].Focused = false;
                        }
                    }
                }
            }
        }
    }
}
