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
        Scene.UNode mRootNode;
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

            mRootNode = world.Root.NewNode(typeof(Scene.UNode), new GamePlay.Scene.UNodeData(), Scene.EBoundVolumeType.None, typeof(GamePlay.UPlacement));
            mRootNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
            mRootNode.IsCastShadow = false;
            mRootNode.Parent = world.Root;
            mInitialized = true;

            /////////////////////////////////////
            SetAxisOperationType(enAxisOperationType.Move);
            /////////////////////////////////////
        }

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
            }
        }

        bool mCtrlKeyIsDown = false;
        bool mShiftKeyIsDown = false;
        bool mAltKeyIsDown = false;
        public void OnEvent(Graphics.Pipeline.UViewportSlate viewport, in SDL2.SDL.SDL_Event e)
        {
            switch(e.type)
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
                                                for (int i = (int)enAxisType.Move_Start; i < (int)enAxisType.Move_End; i++)
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
            var pickResult = camera.GetPickRay(ref pickRay, x, y, viewport.ClientSize.X, viewport.ClientSize.Y);
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
                    }
                    break;
                case enAxisType.Move_Line_XY:
                case enAxisType.Move_Plane_XY:
                case enAxisType.Rot_Line_XY:
                case enAxisType.Rot_Plane_XY:
                case enAxisType.Scale_Line_XY:
                case enAxisType.Scale_Plane_XY:
                    {
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitZ, in startWorldMat, out planeAxis);
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
                    }
                    break;
                case enAxisType.Move_Line_XZ:
                case enAxisType.Move_Plane_XZ:
                case enAxisType.Rot_Line_XZ:
                case enAxisType.Rot_Plane_XZ:
                case enAxisType.Scale_Line_XZ:
                case enAxisType.Scale_Plane_XZ:
                    {
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitY, in startWorldMat, out planeAxis);
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
                    }
                    break;
                case enAxisType.Move_Line_YZ:
                case enAxisType.Move_Plane_YZ:
                case enAxisType.Rot_Line_YZ:
                case enAxisType.Rot_Plane_YZ:
                case enAxisType.Scale_Line_YZ:
                case enAxisType.Scale_Plane_YZ:
                    {
                        EngineNS.Vector3.TransformNormal(in EngineNS.Vector3.UnitX, in startWorldMat, out planeAxis);
                    }
                    break;
                case enAxisType.Scale_XYZ:
                    {
                        planeAxis = -cameraDirection;
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
                    }
                    break;
            }

            mMouseStartScreenLocation = new Vector3(e.motion.x, e.motion.y, 0.0f);
            PickPlanePos(viewport, e.motion.x, e.motion.y, in mCurrentAxisStartTransform.mPosition, in planeAxis, out mMouseStartLocation);
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
                var c2vMat = camera.GetToViewPortMatrix();
                Vector3 screenAxisLoc;
                Vector3.TransformCoordinate(ref mCurrentAxisStartTransform.mPosition, ref c2vMat, out screenAxisLoc);
                screenAxisLoc.Z = 0;
                var tag = transAxisDir + mMouseStartLocation;
                Vector3 screenTag;
                Vector3.TransformCoordinate(ref tag, ref c2vMat, out screenTag);
                screenTag.Z = 0;
                var screenAxisDir = screenTag - screenAxisLoc;
                screenAxisDir.Normalize();

                var camRight = camera.GetRight();
                camRight.Normalize();
                var deltaPos = mCurrentAxisStartTransform.mPosition + camRight;
                Vector3 screenDeltaPos;
                Vector3.TransformCoordinate(ref deltaPos, ref c2vMat, out screenDeltaPos);
                screenDeltaPos.Z = 0;
                var deltaLen = (screenDeltaPos - screenAxisLoc).Length() * (mCurrentAxisStartTransform.mPosition - camera.GetPosition()).LengthSquared();

                var len = (float)(EngineNS.Vector3.Dot(new EngineNS.Vector3(newMouseLoc.X, newMouseLoc.Y, 0) - mMouseStartScreenLocation, screenAxisDir) * 0.000006 * deltaLen);

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
        void EndTransAxis()
        {
            if (!mInitialized)
                return;
            mIsTransAxisOperation = false;
        }
    }
}
