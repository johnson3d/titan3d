using EngineNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EditorCommon.ViewPort
{
    public partial class ViewPortControl
    {
        readonly string mAxisMaterial_Selected = @"editor\axis\axismaterial_selected.instmtl";
        readonly string mAxisMaterial_T_Selected = @"editor\axis\axismaterial_t_yellow.instmtl";
        readonly string mAxisMaterial_Center = @"editor\axis\axismaterial_center.instmtl";
        readonly string mAxisMaterial_X = @"editor\axis\axismaterial_x.instmtl";
        readonly string mAxisMaterial_Y = @"editor\axis\axismaterial_y.instmtl";
        readonly string mAxisMaterial_Z = @"editor\axis\axismaterial_z.instmtl";
        readonly string mAxisMaterial_TX = @"editor\axis\axismaterial_tx.instmtl";
        readonly string mAxisMaterial_TY = @"editor\axis\axismaterial_ty.instmtl";
        readonly string mAxisMaterial_TZ = @"editor\axis\axismaterial_tz.instmtl";

        public string UndoRedoKey
        {
            get;
            set;
        }

        enum enAxisType
        {
            Null = -1,
            Move_Start = 0,
            Move_X = 0,
            Move_Y = 1,
            Move_Z = 2,
            Move_XY = 3,
            Move_XZ = 4,
            Move_YX = 5,
            Move_YZ = 6,
            Move_ZX = 7,
            Move_ZY = 8,
            Move_Plane_XY = 9,
            Move_Plane_XZ = 10,
            Move_Plane_YZ = 11,
            Move_XYZ = 12,

            Rot_Start = 13,
            Rot_Line_XY = 13,
            Rot_Line_XZ = 14,
            Rot_Line_YZ = 15,
            Rot_Plane_XY = 16,
            Rot_Plane_XZ = 17,
            Rot_Plane_YZ = 18,

            Scale_Start = 19,
            Scale_X = 19,
            Scale_Y = 20,
            Scale_Z = 21,
            Scale_XY = 22,
            Scale_XZ = 23,
            Scale_YX = 24,
            Scale_YZ = 25,
            Scale_ZX = 26,
            Scale_ZY = 27,
            Scale_Plane_XY = 28,
            Scale_Plane_XZ = 29,
            Scale_Plane_YZ = 30,
            Scale_XYZ = 31,

            Edge_Start = 32,
            Edge_X_Min = 32,
            Edge_X_MinPlane = 33,
            Edge_X_Max = 34,
            Edge_X_MaxPlane = 35,
            Edge_Y_Min = 36,
            Edge_Y_MinPlane = 37,
            Edge_Y_Max = 38,
            Edge_Y_MaxPlane = 39,
            Edge_Z_Min = 40,
            Edge_Z_MinPlane = 41,
            Edge_Z_Max = 42,
            Edge_Z_MaxPlane = 43,
        }
        enAxisType mCurrentAxisType = enAxisType.Null;
        enum enAxisOperationType
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

        bool mCenterAxisMode = false;
        public bool CenterAxisMode
        {
            get => mCenterAxisMode;
            set
            {
                mCenterAxisMode = value;
                switch (mAxisOperationType)
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
                            if (mCurrentAxisActor != null)
                            {
                                if (mCenterAxisMode)
                                {
                                    mAxisOrigionMatrixBeforeCenterOperation = mCurrentAxisActor.Placement.WorldMatrix;
                                    CenterAxisOperation();
                                }
                                else
                                {
                                    mCurrentAxisActor.Placement.SetMatrix(ref mAxisOrigionMatrixBeforeCenterOperation);
                                }
                            }
                        }
                        break;
                }
                OnPropertyChanged("CenterAxisMode");
            }
        }

        void UpdateEdgeBB()
        {
            var selectActorsArray = mSelectedActors.Values.ToArray();
            if (selectActorsArray.Length <= 0)
                return;

            switch (mAxisWLType)
            {
                case enAxisWLType.World:
                    {
                        mEdgeAxisBB = EngineNS.BoundingBox.EmptyBox();
                        foreach (var actor in selectActorsArray)
                        {
                            var childActor = actor.Actor;
                            var actorAABB = EngineNS.BoundingBox.EmptyBox();
                            childActor.GetAABB(ref actorAABB);
                            EngineNS.BoundingBox.Merge(ref actorAABB, ref mEdgeAxisBB, out mEdgeAxisBB);
                        }
                    }
                    break;
                case enAxisWLType.Local:
                    {
                        var lastActor = selectActorsArray[selectActorsArray.Length - 1];
                        mEdgeAxisBB = lastActor.Actor.LocalBoundingBox;
                        var scaleMat = EngineNS.Matrix.Scaling(lastActor.Actor.Placement.Scale);
                        var max = EngineNS.Vector3.TransformCoordinate(mEdgeAxisBB.Maximum, scaleMat);
                        var min = EngineNS.Vector3.TransformCoordinate(mEdgeAxisBB.Minimum, scaleMat);
                        mEdgeAxisBB = new EngineNS.BoundingBox(min, max);
                        var mat = lastActor.Actor.Placement.WorldMatrix;
                        var invWorldMat = EngineNS.Matrix.Invert(ref mat);
                        for (int i = 0; i < selectActorsArray.Length - 1; i++)
                        {
                            var childActor = selectActorsArray[i].Actor;
                            var tempScaleMat = EngineNS.Matrix.Scaling(childActor.Placement.Scale);
                            var tempBB = childActor.LocalBoundingBox;
                            var tempMax = EngineNS.Vector3.TransformCoordinate(tempBB.Maximum, tempScaleMat);
                            var tempMin = EngineNS.Vector3.TransformCoordinate(tempBB.Minimum, tempScaleMat);
                            tempBB = new EngineNS.BoundingBox(tempMin, tempMax);
                            var localPos = tempBB.GetCorners();
                            foreach (var pos in localPos)
                            {
                                var tempPos = EngineNS.Vector3.TransformCoordinate(pos, childActor.Placement.WorldMatrix);
                                EngineNS.Vector3.TransformCoordinate(ref tempPos, ref invWorldMat, out tempPos);
                                mEdgeAxisBB.Merge(ref tempPos);
                            }
                        }
                    }
                    break;
            }

            if (CenterAxisMode)
            {
                mEdgeAxisOrigionBBBeforeCenterOperation = mEdgeAxisBB;
                mEdgeAxisBB = new EngineNS.BoundingBox(1.0f, 1.0f, 1.0f);
            }
        }
        void UpdateEdgeAxisTransform()
        {
            if (mSelectedActors.Count == 0)
                return;

            var bb = mEdgeAxisBB;
            switch (mAxisWLType)
            {
                case enAxisWLType.World:
                    {
                        var edgeCenter = bb.GetCenter();
                        foreach (var child in mEdgePrefabs)
                        {
                            var axisType = (enEdgeType)child.Tag;
                            switch (axisType)
                            {
                                case enEdgeType.MinX:
                                    child.Placement.Location = new EngineNS.Vector3(bb.Minimum.X, edgeCenter.Y, edgeCenter.Z);
                                    break;
                                case enEdgeType.MaxX:
                                    child.Placement.Location = new EngineNS.Vector3(bb.Maximum.X, edgeCenter.Y, edgeCenter.Z);
                                    break;
                                case enEdgeType.MinY:
                                    child.Placement.Location = new EngineNS.Vector3(edgeCenter.X, bb.Minimum.Y, edgeCenter.Z);
                                    break;
                                case enEdgeType.MaxY:
                                    child.Placement.Location = new EngineNS.Vector3(edgeCenter.X, bb.Maximum.Y, edgeCenter.Z);
                                    break;
                                case enEdgeType.MinZ:
                                    child.Placement.Location = new EngineNS.Vector3(edgeCenter.X, edgeCenter.Y, bb.Minimum.Z);
                                    break;
                                case enEdgeType.MaxZ:
                                    child.Placement.Location = new EngineNS.Vector3(edgeCenter.X, edgeCenter.Y, bb.Maximum.Z);
                                    break;
                            }
                            if (CenterAxisMode)
                                child.Placement.Location += Camera.CameraData.Position + Camera.CameraData.Direction * 10;
                            child.Placement.Rotation = EngineNS.Quaternion.Identity;
                        }
                    }
                    break;
                case enAxisWLType.Local:
                    {
                        var selActors = mSelectedActors.ToArray();
                        var lastActor = selActors[selActors.Length - 1];
                        var edgeCenter = bb.GetCenter();
                        var actorRot = lastActor.Value.Actor.Placement.Rotation;
                        EngineNS.Vector3 actorLoc;
                        if (CenterAxisMode)
                            actorLoc = Camera.CameraData.Position + Camera.CameraData.Direction * 10;
                        else
                            actorLoc = lastActor.Value.Actor.Placement.Location;
                        foreach (var child in mEdgePrefabs)
                        {
                            var axisType = (enEdgeType)child.Tag;
                            EngineNS.Vector3 loc = edgeCenter;
                            switch (axisType)
                            {
                                case enEdgeType.MinX:
                                    loc = new EngineNS.Vector3(bb.Minimum.X, edgeCenter.Y, edgeCenter.Z);
                                    break;
                                case enEdgeType.MaxX:
                                    loc = new EngineNS.Vector3(bb.Maximum.X, edgeCenter.Y, edgeCenter.Z);
                                    break;
                                case enEdgeType.MinY:
                                    loc = new EngineNS.Vector3(edgeCenter.X, bb.Minimum.Y, edgeCenter.Z);
                                    break;
                                case enEdgeType.MaxY:
                                    loc = new EngineNS.Vector3(edgeCenter.X, bb.Maximum.Y, edgeCenter.Z);
                                    break;
                                case enEdgeType.MinZ:
                                    loc = new EngineNS.Vector3(edgeCenter.X, edgeCenter.Y, bb.Minimum.Z);
                                    break;
                                case enEdgeType.MaxZ:
                                    loc = new EngineNS.Vector3(edgeCenter.X, edgeCenter.Y, bb.Maximum.Z);
                                    break;
                            }

                            loc = EngineNS.Vector3.TransformCoordinate(loc, actorRot);
                            loc += actorLoc;
                            child.Placement.Location = loc;
                            child.Placement.Rotation = actorRot;
                        }
                    }
                    break;
            }
        }

        // Local模式为OBB，World模式为AABB
        EngineNS.BoundingBox mEdgeAxisBB = EngineNS.BoundingBox.EmptyBox();
        EngineNS.BoundingBox mEdgeAxisOrigionBBBeforeCenterOperation;
        EngineNS.Matrix mAxisOrigionMatrixBeforeCenterOperation;
        // 将坐标轴放置到屏幕中间
        void CenterAxisOperation()
        {
            if (mCurrentAxisActor == null)
                return;

            mCurrentAxisActor.Placement.Location = Camera.CameraData.Position + Camera.CameraData.Direction * 10;
        }

        public void ShowRotationActor()
        {
            if (mCurrentAxisActor != null)
                World.RemoveEditorActor(mCurrentAxisActor.ActorId);

            foreach (var prefab in mEdgePrefabs)
            {
                World.RemoveEditorActor(prefab.ActorId);
            }

            mCurrentAxisActor = mRotPrefab;
            World.AddEditorActor(mCurrentAxisActor);
            World.RemoveEditorActor(mMovePrefab.ActorId);
            World.RemoveEditorActor(mScalePrefab.ActorId);
            mCurrentAxisActor.Visible = true;
        }

        private void RadioButton_AxisOperationType_Checked(object sender, RoutedEventArgs e)
        {
            var rBtn = sender as RadioButton;
            mAxisOperationType = (enAxisOperationType)(System.Convert.ToInt32(rBtn.Tag));
            if (mAxisInitialized)
            {
                switch (mAxisOperationType)
                {
                    case enAxisOperationType.Move:
                        {
                            if (mCurrentAxisActor != null)
                                World.RemoveEditorActor(mCurrentAxisActor.ActorId);

                            foreach (var prefab in mEdgePrefabs)
                            {
                                World.RemoveEditorActor(prefab.ActorId);
                            }

                            mCurrentAxisActor = mMovePrefab;
                            World.AddEditorActor(mCurrentAxisActor);
                        }
                        break;
                    case enAxisOperationType.Rot:
                        {
                            if (mCurrentAxisActor != null)
                                World.RemoveEditorActor(mCurrentAxisActor.ActorId);

                            foreach (var prefab in mEdgePrefabs)
                            {
                                World.RemoveEditorActor(prefab.ActorId);
                            }

                            mCurrentAxisActor = mRotPrefab;
                            World.AddEditorActor(mCurrentAxisActor);
                            World.RemoveEditorActor(mMovePrefab.ActorId);
                            World.RemoveEditorActor(mScalePrefab.ActorId);
                        }
                        break;
                    case enAxisOperationType.Scale:
                        {
                            if (mCurrentAxisActor != null)
                                World.RemoveEditorActor(mCurrentAxisActor.ActorId);

                            foreach (var prefab in mEdgePrefabs)
                            {
                                World.RemoveEditorActor(prefab.ActorId);
                            }

                            mCurrentAxisActor = mScalePrefab;
                            World.AddEditorActor(mCurrentAxisActor);
                            World.RemoveEditorActor(mMovePrefab.ActorId);
                            World.RemoveEditorActor(mRotPrefab.ActorId);
                        }
                        break;
                    case enAxisOperationType.Edge:
                        {
                            if (mCurrentAxisActor != null)
                                World.RemoveEditorActor(mCurrentAxisActor.ActorId);

                            foreach (var prefab in mEdgePrefabs)
                            {
                                World.AddEditorActor(prefab);
                            }

                            UpdateEdgeBB();
                        }
                        break;
                    default:
                        {
                            if (mCurrentAxisActor != null)
                                World.RemoveEditorActor(mCurrentAxisActor.ActorId);
                            foreach (var prefab in mEdgePrefabs)
                            {
                                World.RemoveEditorActor(prefab.ActorId);
                            }
                        }
                        break;
                }
                CheckUpdateWithCenterAxisMode();
            }
        }

        void CheckUpdateWithCenterAxisMode()
        {
            if (mCurrentAxisActor != null)
            {
                var restore = CenterAxisMode;
                CenterAxisMode = false;
                UpdateAxisShow();
                CenterAxisMode = restore;
            }
        }

        EngineNS.GamePlay.Actor.GPrefab mCurrentAxisActor;
        EngineNS.GamePlay.Actor.GActor mAxisPosActor;
        bool mEnableTick = true;
        void UpdateAxisShow()
        {
            if (!mEnableTick)
                return;
            if (!mAxisInitialized)
                return;

            if (mSelectedActors.Count > 0)
            {
                if (!mIsTransAxisOperation)
                {
                    switch (mAxisOperationType)
                    {
                        case enAxisOperationType.Edge:
                            {
                                UpdateEdgeBB();
                                UpdateEdgeAxisTransform();
                                foreach (var pref in mEdgePrefabs)
                                {
                                    pref.Visible = true;
                                }
                            }
                            break;
                        default:
                            if (mCurrentAxisActor != null)
                            {
                                switch (mAxisSelectMode)
                                {
                                    case enAxisSelectMode.ObjectsCenter:
                                        {
                                            EngineNS.Vector3 centerPos = EngineNS.Vector3.Zero;
                                            foreach (var data in mSelectedActors)
                                            {
                                                centerPos += data.Value.Actor.Placement.WorldLocation;
                                            }
                                            centerPos = centerPos / mSelectedActors.Count;
                                            mCurrentAxisActor.Placement.Location = centerPos;
                                            mCurrentAxisActor.Placement.Rotation = EngineNS.Quaternion.Identity;

                                            if (mSelectedActors.Count == 1)
                                            {
                                                switch (mAxisWLType)
                                                {
                                                    case enAxisWLType.World:
                                                        mCurrentAxisActor.Placement.Rotation = EngineNS.Quaternion.Identity;
                                                        break;
                                                    case enAxisWLType.Local:
                                                        mCurrentAxisActor.Placement.Rotation = mSelectedActors.Last().Value.Actor.Placement.WorldRotation;
                                                        break;
                                                }
                                            }
                                        }
                                        break;
                                    case enAxisSelectMode.ObjectPos:
                                        {
                                            var actor = mAxisPosActor;
                                            if (mAxisPosActor == null)
                                            {
                                                actor = mSelectedActors.Last().Value.Actor;
                                            }

                                            mCurrentAxisActor.Placement.Location = actor.Placement.WorldLocation;
                                            switch (mAxisWLType)
                                            {
                                                case enAxisWLType.World:
                                                    mCurrentAxisActor.Placement.Rotation = EngineNS.Quaternion.Identity;
                                                    break;
                                                case enAxisWLType.Local:
                                                    mCurrentAxisActor.Placement.Rotation = actor.Placement.WorldRotation;
                                                    break;
                                            }
                                        }
                                        break;
                                }

                                if (CenterAxisMode)
                                {
                                    CenterAxisOperation();
                                }
                                mCurrentAxisActor.Visible = true;

                                EngineNS.Vector3 pos = mCurrentAxisActor.Placement.Location;
                                EngineNS.Quaternion rot = mCurrentAxisActor.Placement.Rotation;

                                var xzRot = EngineNS.Quaternion.Identity;
                                var xyRot = EngineNS.Quaternion.Identity;
                                var zxRot = EngineNS.Quaternion.Identity;
                                var zyRot = EngineNS.Quaternion.Identity;
                                var yzRot = EngineNS.Quaternion.Identity;
                                var yxRot = EngineNS.Quaternion.Identity;
                                var deltaPos = Camera.CameraData.Position - pos;// mCurrentAxisActor.Placement.Location;
                                var axisX = EngineNS.Vector3.TransformCoordinate(EngineNS.Vector3.UnitX, rot);// mCurrentAxisActor.Placement.Rotation);
                                var projX = EngineNS.Vector3.Dot(deltaPos, axisX);
                                var axisY = EngineNS.Vector3.TransformCoordinate(EngineNS.Vector3.UnitY, rot);// mCurrentAxisActor.Placement.Rotation);
                                var projY = EngineNS.Vector3.Dot(deltaPos, axisY);
                                var axisZ = EngineNS.Vector3.TransformCoordinate(EngineNS.Vector3.UnitZ, rot);// mCurrentAxisActor.Placement.Rotation);
                                var projZ = EngineNS.Vector3.Dot(deltaPos, axisZ);
                                if (projX < 0)
                                {
                                    xzRot = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitZ, (float)(System.Math.PI));
                                    xyRot = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitY, (float)(System.Math.PI));
                                }
                                if (projY < 0)
                                {
                                    yxRot = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitX, (float)(System.Math.PI));
                                    yzRot = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitZ, (float)(System.Math.PI));
                                }
                                if (projZ < 0)
                                {
                                    zyRot = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitY, (float)(System.Math.PI));
                                    zxRot = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitX, (float)(System.Math.PI));
                                }
                                var xzRotation = xzRot * zxRot;
                                var xyRotation = xyRot * yxRot;
                                var yzRotation = yzRot * zyRot;
                                List<EngineNS.GamePlay.Actor.GActor> Children = mCurrentAxisActor.GetChildrenUnsafe();
                                switch (mAxisOperationType)
                                {
                                    case enAxisOperationType.Move:
                                        {
                                            Children[(int)(enAxisType.Move_XZ)].Placement.Rotation = xzRotation;
                                            Children[(int)(enAxisType.Move_ZX)].Placement.Rotation = xzRotation;
                                            Children[(int)(enAxisType.Move_Plane_XZ)].Placement.Rotation = xzRotation;
                                            Children[(int)(enAxisType.Move_XY)].Placement.Rotation = xyRotation;
                                            Children[(int)(enAxisType.Move_YX)].Placement.Rotation = xyRotation;
                                            Children[(int)(enAxisType.Move_Plane_XY)].Placement.Rotation = xyRotation;
                                            Children[(int)(enAxisType.Move_YZ)].Placement.Rotation = yzRotation;
                                            Children[(int)(enAxisType.Move_ZY)].Placement.Rotation = yzRotation;
                                            Children[(int)(enAxisType.Move_Plane_YZ)].Placement.Rotation = yzRotation;
                                        }
                                        break;
                                    case enAxisOperationType.Rot:
                                        {
                                            var startIdx = enAxisType.Rot_Start;
                                            Children[(int)(enAxisType.Rot_Plane_XY - startIdx)].Placement.Rotation = xyRotation;
                                            Children[(int)(enAxisType.Rot_Plane_XZ - startIdx)].Placement.Rotation = xzRotation;
                                            Children[(int)(enAxisType.Rot_Plane_YZ - startIdx)].Placement.Rotation = yzRotation;
                                            Children[(int)(enAxisType.Rot_Line_XY - startIdx)].Placement.Rotation = xyRotation;
                                            Children[(int)(enAxisType.Rot_Line_XZ - startIdx)].Placement.Rotation = xzRotation;
                                            Children[(int)(enAxisType.Rot_Line_YZ - startIdx)].Placement.Rotation = yzRotation;
                                        }
                                        break;
                                    case enAxisOperationType.Scale:
                                        {
                                            var startIdx = enAxisType.Scale_Start;
                                            Children[(int)(enAxisType.Scale_XY - startIdx)].Placement.Rotation = xyRotation;
                                            Children[(int)(enAxisType.Scale_XZ - startIdx)].Placement.Rotation = xzRotation;
                                            Children[(int)(enAxisType.Scale_YZ - startIdx)].Placement.Rotation = yzRotation;
                                            Children[(int)(enAxisType.Scale_YX - startIdx)].Placement.Rotation = xyRotation;
                                            Children[(int)(enAxisType.Scale_ZX - startIdx)].Placement.Rotation = xzRotation;
                                            Children[(int)(enAxisType.Scale_ZY - startIdx)].Placement.Rotation = yzRotation;
                                            Children[(int)(enAxisType.Scale_Plane_XY - startIdx)].Placement.Rotation = xyRotation;
                                            Children[(int)(enAxisType.Scale_Plane_XZ - startIdx)].Placement.Rotation = xzRotation;
                                            Children[(int)(enAxisType.Scale_Plane_YZ - startIdx)].Placement.Rotation = yzRotation;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }

                FitAxisSize();
            }
            else
            {
                if (mCurrentAxisActor != null)
                    mCurrentAxisActor.Visible = false;
                foreach (var pref in mEdgePrefabs)
                {
                    pref.Visible = false;
                }
            }
        }

        enum enAxisSelectMode
        {
            ObjectsCenter,
            ObjectPos,
        }
        enAxisSelectMode mAxisSelectMode = enAxisSelectMode.ObjectsCenter;
        private void ComboBox_AxisSelectMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_AxisSelectMode.SelectedIndex < 0)
                return;

            mAxisSelectMode = (enAxisSelectMode)(ComboBox_AxisSelectMode.SelectedIndex);
        }

        enum enAxisWLType
        {
            World,
            Local,
        }
        enAxisWLType mAxisWLType = enAxisWLType.World;
        private void ComboBox_AxisWLType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_AxisWLType.SelectedIndex < 0)
                return;

            mAxisWLType = (enAxisWLType)(ComboBox_AxisWLType.SelectedIndex);
        }

        EngineNS.GamePlay.Actor.GPrefab mMovePrefab;
        EngineNS.GamePlay.Actor.GPrefab mBaseAixPrefab;
        EngineNS.GamePlay.Actor.GPrefab mRotPrefab;
        EngineNS.GamePlay.Actor.GPrefab mScalePrefab;
        EngineNS.GamePlay.Actor.GPrefab[] mEdgePrefabs = new EngineNS.GamePlay.Actor.GPrefab[(int)enEdgeType.Total];

        public EngineNS.GamePlay.Actor.GPrefab BaseAixPrefab
        {
            get
            {
                return mBaseAixPrefab;
            }
        }

        public EngineNS.GamePlay.GOffScreenViewer AxisShower = new EngineNS.GamePlay.GOffScreenViewer();
        public async Task InitBaseAix(EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile RPolicy)
        {
            if (RPolicy == null)
                return;

            await AxisShower.InitEnviroment(256, 256, "AxisShower");

            if (BaseAixPrefab == null)
                await CreateBaseAixActor();

            //var axisAct = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName(@"editor\basemesh\box.gms"));


            AxisShower.World.AddActor(BaseAixPrefab);
            AxisShower.World.GetScene(RName.GetRName("AxisShower")).AddActor(BaseAixPrefab);

            BaseAixPrefab.Placement.Location = new Vector3(0, 0, 0);
            AxisShower.Start();

            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            var image = await EngineNS.Graphics.Mesh.CGfxImage2D.CreateImage2D(RHICtx, RName.GetRName("MaterialEditor/baseaix.instmtl", RName.enRNameType.Editor), -50, -50, 0, 100, 100);
            var texture = AxisShower.OffScreenBaseTexture;
            image.SetTexture("txDiffuse", texture);
            //BaseAixPrefab.Placement.Transform = 
            //Camera.ViewProjection* Camera.ViewProjection* BaseAixPrefab
            BaseAixPrefab.Placement.Scale = new Vector3(1.5f, 1.5f, 1.5f);
            RPolicy.OnDrawUI += (cmd, view) =>
            {

                {
                    Vector3 scale;
                    Vector3 pos;
                    Quaternion rot;
                    Camera.CameraData.ViewProjection.Decompose(out scale, out rot, out pos);
                    //BaseAixPrefab.Placement.Scale = scale;
                    if(BaseAixPrefab.Placement.Rotation != rot)
                    BaseAixPrefab.Placement.Rotation = rot;
                    var tmp = Matrix.Transformation(Vector3.UnitXYZ,
                        Quaternion.Identity, Vector3.Zero);
                    tmp.M41 = 50;
                    tmp.M42 = (float)ActualHeight - 70;
                    image.RenderMatrix = tmp;
                    var pass = image.GetPass();

                    pass.ViewPort = view.Viewport;
                    pass.BindCBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_View, view.ScreenViewCB);
                    pass.ShadingEnv.BindResources(image.Mesh, pass);
                    cmd.PushPass(pass);
                }
            };
        }

        bool mAxisInitialized = false;
        public async Task InitializeAxis()
        {
            if (mAxisInitialized)
                return;

            var rc = EngineNS.CEngine.Instance.RenderContext;

            var moveAxisFiles = new string[]
            {
                @"editor\axis\position\axis_x.gms",
                @"editor\axis\position\axis_y.gms",
                @"editor\axis\position\axis_z.gms",
                @"editor\axis\position\axis_s_xy.gms",
                @"editor\axis\position\axis_s_xz.gms",
                @"editor\axis\position\axis_s_yx.gms",
                @"editor\axis\position\axis_s_yz.gms",
                @"editor\axis\position\axis_s_zx.gms",
                @"editor\axis\position\axis_s_zy.gms",
                @"editor\axis\position\plane_xy.gms",
                @"editor\axis\position\plane_xz.gms",
                @"editor\axis\position\plane_yz.gms",
                @"editor\axis\position\centre.gms",
            };
            List<EngineNS.GamePlay.Actor.GActor> moveAxisActors = new List<EngineNS.GamePlay.Actor.GActor>();
            for (int i = 0; i < moveAxisFiles.Length; i++)
            {
                var axisFile = moveAxisFiles[i];
                var axisAct = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(EngineNS.CEngine.Instance.FileManager.GetRName(axisFile));
                axisAct.Tag = i;
                axisAct.SpecialName = $"moveAxis{((enAxisType)i).ToString()}({i})";
                switch ((enAxisType)i)
                {
                    //case enAxisType.Move_Plane_XY:
                    //case enAxisType.Move_Plane_XZ:
                    //case enAxisType.Move_Plane_YZ:
                    //    axisAct.Visible = false;
                    //    break;
                    default:
                        EngineNS.CEngine.Instance.HitProxyManager.MapActor(axisAct);
                        break;
                }
                moveAxisActors.Add(axisAct);
            }
            mMovePrefab = await EngineNS.GamePlay.Actor.GPrefab.CreatePrefab(moveAxisActors);
            var rotAxisFiles = new string[]
            {
                @"editor\axis\rotation\line_xy.gms",
                @"editor\axis\rotation\line_xz.gms",
                @"editor\axis\rotation\line_yz.gms",
                @"editor\axis\rotation\plane_xy.gms",
                @"editor\axis\rotation\plane_xz.gms",
                @"editor\axis\rotation\plane_yz.gms",
            };
            List<EngineNS.GamePlay.Actor.GActor> rotAxisActors = new List<EngineNS.GamePlay.Actor.GActor>();
            for (int i = 0; i < rotAxisFiles.Length; i++)
            {
                var axisFile = rotAxisFiles[i];
                var axisAct = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(EngineNS.CEngine.Instance.FileManager.GetRName(axisFile));
                var tagType = i + enAxisType.Rot_Start;
                axisAct.Tag = tagType;
                axisAct.SpecialName = $"rotAxis{(tagType).ToString()}({i})";
                EngineNS.CEngine.Instance.HitProxyManager.MapActor(axisAct);
                rotAxisActors.Add(axisAct);
            }
            mRotPrefab = await EngineNS.GamePlay.Actor.GPrefab.CreatePrefab(rotAxisActors);

            var scaleAxisFiles = new string[]
            {
                @"editor\axis\scale\axis_x.gms",
                @"editor\axis\scale\axis_y.gms",
                @"editor\axis\scale\axis_z.gms",
                @"editor\axis\scale\axis_s_xy.gms",
                @"editor\axis\scale\axis_s_xz.gms",
                @"editor\axis\scale\axis_s_yx.gms",
                @"editor\axis\scale\axis_s_yz.gms",
                @"editor\axis\scale\axis_s_zx.gms",
                @"editor\axis\scale\axis_s_zy.gms",
                @"editor\axis\scale\plane_xy.gms",
                @"editor\axis\scale\plane_xz.gms",
                @"editor\axis\scale\plane_yz.gms",
                @"editor\axis\scale\centre.gms",
            };
            List<EngineNS.GamePlay.Actor.GActor> scaleAxisActors = new List<EngineNS.GamePlay.Actor.GActor>();
            for (int i = 0; i < scaleAxisFiles.Length; i++)
            {
                var axisFile = scaleAxisFiles[i];
                var axisAct = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(EngineNS.CEngine.Instance.FileManager.GetRName(axisFile));
                var tagType = i + enAxisType.Scale_Start;
                axisAct.Tag = tagType;
                axisAct.SpecialName = $"scaleAxis{(tagType).ToString()}({i})";
                switch (tagType)
                {
                    //case enAxisType.Scale_Plane_XY:
                    //case enAxisType.Scale_Plane_XZ:
                    //case enAxisType.Scale_Plane_YZ:
                    //    axisAct.Visible = false;
                    //    break;
                    default:
                        EngineNS.CEngine.Instance.HitProxyManager.MapActor(axisAct);
                        break;
                }
                scaleAxisActors.Add(axisAct);
            }
            mScalePrefab = await EngineNS.GamePlay.Actor.GPrefab.CreatePrefab(scaleAxisActors);

            var edgeAxisFiles = new string[]
            {
                @"editor\axis\extrude_edge\axis_1.gms",
                @"editor\axis\extrude_edge\plane.gms",
            };

            for (int i = (int)enEdgeType.MinX; i < (int)enEdgeType.Total; i++)
            {
                var edgeAxisActors = new List<EngineNS.GamePlay.Actor.GActor>();
                var axisAct = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(EngineNS.CEngine.Instance.FileManager.GetRName(edgeAxisFiles[0]));
                var meshComp = axisAct.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                var tagType = i * 2 + enAxisType.Edge_Start;
                axisAct.Tag = tagType;
                axisAct.SpecialName = $"edgeAxis{(tagType).ToString()}({i})";
                EngineNS.CEngine.Instance.HitProxyManager.MapActor(axisAct);
                edgeAxisActors.Add(axisAct);

                switch (tagType)
                {
                    case enAxisType.Edge_X_Min:
                    case enAxisType.Edge_X_Max:
                        {
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_X), null);
                            axisAct.Placement.Rotation = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitY, (float)(System.Math.PI * 0.5));
                        }
                        break;
                    case enAxisType.Edge_Y_Min:
                    case enAxisType.Edge_Y_Max:
                        {
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Y), null);
                            axisAct.Placement.Rotation = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitX, (float)(System.Math.PI * 0.5));
                        }
                        break;
                    case enAxisType.Edge_Z_Min:
                    case enAxisType.Edge_Z_Max:
                        {
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Z), null);
                        }
                        break;
                }

                var planeAct = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(EngineNS.CEngine.Instance.FileManager.GetRName(edgeAxisFiles[1]));
                var planeMeshComp = planeAct.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                var planeTagType = i * 2 + enAxisType.Edge_Start + 1;
                planeAct.Tag = planeTagType;
                planeAct.SpecialName = $"edgePlane{(tagType).ToString()}({i})";
                EngineNS.CEngine.Instance.HitProxyManager.MapActor(planeAct);
                edgeAxisActors.Add(planeAct);

                switch (planeTagType)
                {
                    case enAxisType.Edge_X_MinPlane:
                    case enAxisType.Edge_X_MaxPlane:
                        {
                            await planeMeshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TX), null);
                            planeAct.Placement.Rotation = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitY, (float)(System.Math.PI * 0.5));
                        }
                        break;
                    case enAxisType.Edge_Y_MinPlane:
                    case enAxisType.Edge_Y_MaxPlane:
                        {
                            await planeMeshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TY), null);
                            planeAct.Placement.Rotation = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitX, (float)(System.Math.PI * 0.5));
                        }
                        break;
                    case enAxisType.Edge_Z_MinPlane:
                    case enAxisType.Edge_Z_MaxPlane:
                        {
                            await planeMeshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TZ), null);
                        }
                        break;
                }

                var prefab = await EngineNS.GamePlay.Actor.GPrefab.CreatePrefab(edgeAxisActors);
                prefab.Tag = i;
                mEdgePrefabs[i] = prefab;
            }

            mAxisInitialized = true;

            //var test = CreateBaseAixActor();
        }

        public async Task CreateBaseAixActor()
        {
            if (mBaseAixPrefab != null)
                return;
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var baseAxisFiles = new string[]
            {
                @"editor\axis\position\axis_x.gms",
                @"editor\axis\position\axis_y.gms",
                @"editor\axis\position\axis_z.gms",
                @"editor\axis\position\centre.gms",
            };
            List<EngineNS.GamePlay.Actor.GActor> baseAxisActors = new List<EngineNS.GamePlay.Actor.GActor>();
            for (int i = 0; i < baseAxisFiles.Length; i++)
            {
                var axisFile = baseAxisFiles[i];
                var axisAct = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(EngineNS.CEngine.Instance.FileManager.GetRName(axisFile));
                axisAct.Tag = i;
                axisAct.SpecialName = $"baseAxis{((enAxisType)i).ToString()}({i})";
                baseAxisActors.Add(axisAct);
            }
            mBaseAixPrefab = await EngineNS.GamePlay.Actor.GPrefab.CreatePrefab(baseAxisActors);

            mBaseAixPrefab.Placement.Location = new EngineNS.Vector3(0, 0, 0);

        }

        enAxisType CheckAxisType(uint x, uint y)
        {
            var id = RPolicy.mHitProxy.GetHitProxyID(x, y);
            var actor = EngineNS.CEngine.Instance.HitProxyManager.FindActor(id);
            if (actor == null)
                return enAxisType.Null;
            else
            {
                return (enAxisType)(actor.Tag);
            }
        }

        public class SelectActorData
        {
            public EngineNS.GamePlay.Actor.GActor Actor;
            public EngineNS.Matrix StartTransMatrix;
        }
        public class DuplicateActorEventArgs
        {
            public EngineNS.GamePlay.Actor.GActor Origin;
            public EngineNS.GamePlay.Actor.GActor Duplicated;
            public DuplicateActorEventArgs(EngineNS.GamePlay.Actor.GActor origin, EngineNS.GamePlay.Actor.GActor duplicated)
            {
                Origin = origin;
                Duplicated = duplicated;
            }
        }
        public event EventHandler<SelectActorData[]> OnSelectAcotrs;
        //public event EventHandler<SelectActorData[]> OnDuplicateAcotrs;
        public event EventHandler<DuplicateActorEventArgs> OnDuplicateAcotr;
        Dictionary<Guid, SelectActorData> mSelectedActors = new Dictionary<Guid, SelectActorData>();

        public int SelectActtorCount
        {
            get
            {
                return mSelectedActors.Count;
            }
        }
        public SelectActorData[] GetSelectedActors()
        {
            return mSelectedActors.Values.ToArray();
        }
        public void SelectActors(SelectActorData[] actors)
        {
            if (actors == null || actors.Length == 0)
            {
                SelectActor(null);
            }
            else
            {
                mSelectedActors.Clear();
                foreach (var act in actors)
                {
                    act.Actor.Selected = true;
                    mSelectedActors.Add(act.Actor.ActorId, act);
                }

                var lst = new List<EngineNS.GamePlay.Actor.GActor>();
                foreach (var i in mSelectedActors)
                {
                    lst.Add(i.Value.Actor);
                }
                EngineNS.CEngine.Instance.GameEditorInstance.EditOperator.RaiseOnSelectedActorsChanged(this, lst);
                OnSelectAcotrs?.Invoke(this, actors);
            }

            CheckUpdateWithCenterAxisMode();
        }
        public void SelectActor(EngineNS.GamePlay.Actor.GActor actor)
        {
            if (actor == null)
            {
                foreach (var i in mSelectedActors)
                {
                    i.Value.Actor.Selected = false;
                }
                mSelectedActors.Clear();
                if (mCurrentAxisActor != null)
                    mCurrentAxisActor.Visible = false;
                foreach (var pref in mEdgePrefabs)
                {
                    pref.Visible = false;
                }

                {
                    var lst = new List<EngineNS.GamePlay.Actor.GActor>();
                    foreach (var i in mSelectedActors)
                    {
                        lst.Add(i.Value.Actor);
                    }
                    EngineNS.CEngine.Instance.GameEditorInstance.EditOperator.RaiseOnSelectedActorsChanged(this, lst);
                    OnSelectAcotrs?.Invoke(this, GetSelectedActors());
                }
                return;
            }
            // todo: 处理选中Prefab的情况
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                // 多选
                if (mSelectedActors.ContainsKey(actor.ActorId))
                {
                    // 取消选择
                    actor.Selected = false;
                    mSelectedActors.Remove(actor.ActorId);

                    {
                        var lst = new List<EngineNS.GamePlay.Actor.GActor>();
                        foreach (var i in mSelectedActors)
                        {
                            lst.Add(i.Value.Actor);
                        }
                        EngineNS.CEngine.Instance.GameEditorInstance.EditOperator.RaiseOnSelectedActorsChanged(this, lst);
                        OnSelectAcotrs?.Invoke(this, GetSelectedActors());
                    }
                }
                else
                {
                    // 选择
                    var data = new SelectActorData()
                    {
                        Actor = actor,
                        StartTransMatrix = actor.Placement.WorldMatrix,
                    };
                    actor.Selected = true;
                    mSelectedActors.Add(actor.ActorId, data);

                    {
                        var lst = new List<EngineNS.GamePlay.Actor.GActor>();
                        foreach (var i in mSelectedActors)
                        {
                            lst.Add(i.Value.Actor);
                        }
                        EngineNS.CEngine.Instance.GameEditorInstance.EditOperator.RaiseOnSelectedActorsChanged(this, lst);
                        OnSelectAcotrs?.Invoke(this, GetSelectedActors());
                    }
                }
            }
            else
            {
                // 单选
                foreach (var i in mSelectedActors)
                {
                    i.Value.Actor.Selected = false;
                }
                mSelectedActors.Clear();
                var data = new SelectActorData()
                {
                    Actor = actor,
                    StartTransMatrix = actor.Placement.WorldMatrix,
                };
                actor.Selected = true;
                mSelectedActors[actor.ActorId] = data;

                {
                    var lst = new List<EngineNS.GamePlay.Actor.GActor>();
                    foreach (var i in mSelectedActors)
                    {
                        lst.Add(i.Value.Actor);
                    }
                    EngineNS.CEngine.Instance.GameEditorInstance.EditOperator.RaiseOnSelectedActorsChanged(this, lst);
                    OnSelectAcotrs?.Invoke(this, GetSelectedActors());
                }
            }
            mAxisPosActor = null;

            CheckUpdateWithCenterAxisMode();
        }
        public void SelectActorsWithoutNotify(SelectActorData[] actors)
        {
            if (actors == null || actors.Length == 0)
            {
                SelectActorWithoutNotify(null);
            }
            else
            {
                mSelectedActors.Clear();
                foreach (var act in actors)
                {
                    act.Actor.Selected = true;
                    mSelectedActors.Add(act.Actor.ActorId, act);
                }

                var lst = new List<EngineNS.GamePlay.Actor.GActor>();
                foreach (var i in mSelectedActors)
                {
                    lst.Add(i.Value.Actor);
                }
                EngineNS.CEngine.Instance.GameEditorInstance.EditOperator.RaiseOnSelectedActorsChanged(this, lst);
            }

            CheckUpdateWithCenterAxisMode();
        }
        public void SelectActorWithoutNotify(EngineNS.GamePlay.Actor.GActor actor)
        {
            if (actor == null)
            {
                foreach (var i in mSelectedActors)
                {
                    i.Value.Actor.Selected = false;
                }
                mSelectedActors.Clear();
                if (mCurrentAxisActor != null)
                    mCurrentAxisActor.Visible = false;
                foreach (var pref in mEdgePrefabs)
                {
                    pref.Visible = false;
                }

                {
                    var lst = new List<EngineNS.GamePlay.Actor.GActor>();
                    foreach (var i in mSelectedActors)
                    {
                        lst.Add(i.Value.Actor);
                    }
                    EngineNS.CEngine.Instance.GameEditorInstance.EditOperator.RaiseOnSelectedActorsChanged(this, lst);
                }
                return;
            }
            // todo: 处理选中Prefab的情况
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                // 多选
                if (mSelectedActors.ContainsKey(actor.ActorId))
                {
                    // 取消选择
                    actor.Selected = false;
                    mSelectedActors.Remove(actor.ActorId);

                    {
                        var lst = new List<EngineNS.GamePlay.Actor.GActor>();
                        foreach (var i in mSelectedActors)
                        {
                            lst.Add(i.Value.Actor);
                        }
                        EngineNS.CEngine.Instance.GameEditorInstance.EditOperator.RaiseOnSelectedActorsChanged(this, lst);
                    }
                }
                else
                {
                    // 选择
                    var data = new SelectActorData()
                    {
                        Actor = actor,
                        StartTransMatrix = actor.Placement.WorldMatrix,
                    };
                    actor.Selected = true;
                    mSelectedActors.Add(actor.ActorId, data);

                    {
                        var lst = new List<EngineNS.GamePlay.Actor.GActor>();
                        foreach (var i in mSelectedActors)
                        {
                            lst.Add(i.Value.Actor);
                        }
                        EngineNS.CEngine.Instance.GameEditorInstance.EditOperator.RaiseOnSelectedActorsChanged(this, lst);
                    }
                }
            }
            else
            {
                // 单选
                foreach (var i in mSelectedActors)
                {
                    i.Value.Actor.Selected = false;
                }
                mSelectedActors.Clear();
                var data = new SelectActorData()
                {
                    Actor = actor,
                    StartTransMatrix = actor.Placement.WorldMatrix,
                };
                actor.Selected = true;
                mSelectedActors[actor.ActorId] = data;

                {
                    var lst = new List<EngineNS.GamePlay.Actor.GActor>();
                    foreach (var i in mSelectedActors)
                    {
                        lst.Add(i.Value.Actor);
                    }
                    EngineNS.CEngine.Instance.GameEditorInstance.EditOperator.RaiseOnSelectedActorsChanged(this, lst);
                }
            }
            mAxisPosActor = null;

            CheckUpdateWithCenterAxisMode();
        }
        void MouseSelectItem(System.Windows.Forms.MouseEventArgs e)
        {
            if (RPolicy == null)
            {
                return;
            }
            if (!RPolicy.mHitProxy.mEnabled)
                return;
            if (mIsTransAxisOperation)
                return;
            var id = RPolicy.mHitProxy.GetHitProxyID((uint)(e.X), (uint)(e.Y));
            var actor = EngineNS.CEngine.Instance.HitProxyManager.FindActor(id);
            SelectActor(actor);

            //if (DSelectActors != null )
            //    DSelectActors(GetSelectedActors(), actor);
        }

        async Task ResetAxisMaterial(enAxisType axisType)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if (axisType != enAxisType.Null)
            {
                var children = new List<EngineNS.GamePlay.Actor.GActor>();
                var actorIdx = 0;
                switch (mAxisOperationType)
                {
                    case enAxisOperationType.Move:
                        actorIdx = (int)axisType;
                        children = mCurrentAxisActor.GetChildrenUnsafe();
                        break;
                    case enAxisOperationType.Rot:
                        actorIdx = (int)axisType - (int)enAxisType.Rot_Start;
                        children = mCurrentAxisActor.GetChildrenUnsafe();
                        break;
                    case enAxisOperationType.Scale:
                        actorIdx = (int)axisType - (int)enAxisType.Scale_Start;
                        children = mCurrentAxisActor.GetChildrenUnsafe();
                        break;
                    case enAxisOperationType.Edge:
                        actorIdx = (int)axisType - (int)enAxisType.Edge_Start;
                        foreach (var pref in mEdgePrefabs)
                        {
                            children.AddRange(pref.GetChildrenUnsafe());
                        }
                        break;
                }

                switch (axisType)
                {
                    case enAxisType.Move_X:
                    case enAxisType.Scale_X:
                        {
                            var mat = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_X));
                            var axisPointAtActor = children[actorIdx];
                            var meshComp = axisPointAtActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                        }
                        break;
                    case enAxisType.Move_Y:
                    case enAxisType.Scale_Y:
                        {
                            var mat = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Y));
                            var axisPointAtActor = children[actorIdx];
                            var meshComp = axisPointAtActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                        }
                        break;
                    case enAxisType.Move_Z:
                    case enAxisType.Scale_Z:
                        {
                            var mat = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Z));
                            var axisPointAtActor = children[actorIdx];
                            var meshComp = axisPointAtActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                        }
                        break;
                    case enAxisType.Move_XY:
                    case enAxisType.Move_YX:
                    case enAxisType.Move_Plane_XY:
                        {
                            var matX = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_X));
                            var meshComp = children[(int)enAxisType.Move_X].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            meshComp = children[(int)enAxisType.Move_XY].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            var matY = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Y));
                            meshComp = children[(int)enAxisType.Move_Y].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);
                            meshComp = children[(int)enAxisType.Move_YX].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);

                            var matXY = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TZ));
                            meshComp = children[(int)enAxisType.Move_Plane_XY].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matXY, null);
                            //Children[(int)enAxisType.Move_Plane_XY].Visible = false;
                        }
                        break;
                    case enAxisType.Move_XZ:
                    case enAxisType.Move_ZX:
                    case enAxisType.Move_Plane_XZ:
                        {
                            var matX = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_X));
                            var meshComp = children[(int)enAxisType.Move_X].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            meshComp = children[(int)enAxisType.Move_XZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            var matY = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Z));
                            meshComp = children[(int)enAxisType.Move_Z].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);
                            meshComp = children[(int)enAxisType.Move_ZX].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);

                            var matXZ = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TY));
                            meshComp = children[(int)enAxisType.Move_Plane_XZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matXZ, null);
                            //Children[(int)enAxisType.Move_Plane_XZ].Visible = false;
                        }
                        break;
                    case enAxisType.Move_YZ:
                    case enAxisType.Move_ZY:
                    case enAxisType.Move_Plane_YZ:
                        {
                            var matX = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Y));
                            var meshComp = children[(int)enAxisType.Move_Y].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            meshComp = children[(int)enAxisType.Move_YZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            var matY = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Z));
                            meshComp = children[(int)enAxisType.Move_Z].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);
                            meshComp = children[(int)enAxisType.Move_ZY].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);

                            var matYZ = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TX));
                            meshComp = children[(int)enAxisType.Move_Plane_YZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matYZ, null);
                            //Children[(int)enAxisType.Move_Plane_YZ].Visible = false;
                        }
                        break;
                    case enAxisType.Scale_XY:
                    case enAxisType.Scale_YX:
                    case enAxisType.Scale_Plane_XY:
                        {
                            var matX = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_X));
                            var meshComp = children[(int)enAxisType.Scale_X - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            meshComp = children[(int)enAxisType.Scale_XY - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            var matY = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Y));
                            meshComp = children[(int)enAxisType.Scale_Y - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);
                            meshComp = children[(int)enAxisType.Scale_YX - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);

                            var matXY = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TZ));
                            meshComp = children[(int)enAxisType.Scale_Plane_XY - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matXY, null);
                            //Children[(int)enAxisType.Scale_Plane_XY - (int)enAxisType.Scale_Start].Visible = false;
                        }
                        break;
                    case enAxisType.Scale_XZ:
                    case enAxisType.Scale_ZX:
                    case enAxisType.Scale_Plane_XZ:
                        {
                            var matX = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_X));
                            var meshComp = children[(int)enAxisType.Scale_X - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            meshComp = children[(int)enAxisType.Scale_XZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            var matY = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Z));
                            meshComp = children[(int)enAxisType.Scale_Z - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);
                            meshComp = children[(int)enAxisType.Scale_ZX - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);

                            var matXZ = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TY));
                            meshComp = children[(int)enAxisType.Scale_Plane_XZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matXZ, null);
                            //Children[(int)enAxisType.Scale_Plane_XZ - (int)enAxisType.Scale_Start].Visible = false;
                        }
                        break;
                    case enAxisType.Scale_YZ:
                    case enAxisType.Scale_ZY:
                    case enAxisType.Scale_Plane_YZ:
                        {
                            var matX = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Y));
                            var meshComp = children[(int)enAxisType.Scale_Y - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            meshComp = children[(int)enAxisType.Scale_YZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            var matY = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Z));
                            meshComp = children[(int)enAxisType.Scale_Z - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);
                            meshComp = children[(int)enAxisType.Scale_ZY - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);

                            var matYZ = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TX));
                            meshComp = children[(int)enAxisType.Scale_Plane_YZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matYZ, null);
                            //Children[(int)enAxisType.Scale_Plane_YZ - (int)enAxisType.Scale_Start].Visible = false;
                        }
                        break;
                    case enAxisType.Rot_Line_XY:
                    case enAxisType.Rot_Plane_XY:
                        {
                            var matPlane = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TZ));
                            var meshComp = children[(int)enAxisType.Rot_Plane_XY - (int)enAxisType.Rot_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matPlane, null);
                            var matLine = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Z));
                            meshComp = children[(int)enAxisType.Rot_Line_XY - (int)enAxisType.Rot_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matLine, null);
                        }
                        break;
                    case enAxisType.Rot_Line_XZ:
                    case enAxisType.Rot_Plane_XZ:
                        {
                            var matPlane = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TY));
                            var meshComp = children[(int)enAxisType.Rot_Plane_XZ - (int)enAxisType.Rot_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matPlane, null);
                            var matLine = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Y));
                            meshComp = children[(int)enAxisType.Rot_Line_XZ - (int)enAxisType.Rot_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matLine, null);
                        }
                        break;
                    case enAxisType.Rot_Line_YZ:
                    case enAxisType.Rot_Plane_YZ:
                        {
                            var matPlane = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TX));
                            var meshComp = children[(int)enAxisType.Rot_Plane_YZ - (int)enAxisType.Rot_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matPlane, null);
                            var matLine = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_X));
                            meshComp = children[(int)enAxisType.Rot_Line_YZ - (int)enAxisType.Rot_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matLine, null);
                        }
                        break;
                    case enAxisType.Move_XYZ:
                        {
                            var mat = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Center));
                            var axisPointAtActor = children[actorIdx];
                            var meshComp = axisPointAtActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);

                            var matX = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_X));
                            meshComp = children[(int)enAxisType.Move_X].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            meshComp = children[(int)enAxisType.Move_XY].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            var matY = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Y));
                            meshComp = children[(int)enAxisType.Move_Y].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);
                            meshComp = children[(int)enAxisType.Move_YX].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);

                            var matXY = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TZ));
                            meshComp = children[(int)enAxisType.Move_Plane_XY].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matXY, null);

                            meshComp = children[(int)enAxisType.Move_XZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            var matZ = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Z));
                            meshComp = children[(int)enAxisType.Move_Z].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matZ, null);
                            meshComp = children[(int)enAxisType.Move_ZX].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matZ, null);

                            var matXZ = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TY));
                            meshComp = children[(int)enAxisType.Move_Plane_XZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matXZ, null);

                            meshComp = children[(int)enAxisType.Move_YZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);
                            meshComp = children[(int)enAxisType.Move_ZY].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matZ, null);

                            var matYZ = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TX));
                            meshComp = children[(int)enAxisType.Move_Plane_YZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matYZ, null);

                        }
                        break;
                    case enAxisType.Scale_XYZ:
                        {
                            var mat = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Center));
                            var axisPointAtActor = children[actorIdx];
                            var meshComp = axisPointAtActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);

                            var matY = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Y));
                            meshComp = children[(int)enAxisType.Scale_Y - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);
                            meshComp = children[(int)enAxisType.Scale_YZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);
                            var matZ = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Z));
                            meshComp = children[(int)enAxisType.Scale_Z - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matZ, null);
                            meshComp = children[(int)enAxisType.Scale_ZY - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matZ, null);

                            var matYZ = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TX));
                            meshComp = children[(int)enAxisType.Scale_Plane_YZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matYZ, null);

                            var matX = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_X));
                            meshComp = children[(int)enAxisType.Scale_X - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            meshComp = children[(int)enAxisType.Scale_XZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            meshComp = children[(int)enAxisType.Scale_ZX - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matZ, null);

                            var matXZ = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TY));
                            meshComp = children[(int)enAxisType.Scale_Plane_XZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matXZ, null);

                            meshComp = children[(int)enAxisType.Scale_XY - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matX, null);
                            meshComp = children[(int)enAxisType.Scale_YX - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matY, null);

                            var matXY = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TZ));
                            meshComp = children[(int)enAxisType.Scale_Plane_XY - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstanceAsync(rc, 0, matXY, null);
                        }
                        break;
                    case enAxisType.Edge_X_Min:
                    case enAxisType.Edge_X_MinPlane:
                        {
                            var meshComp = children[(int)enAxisType.Edge_X_Min - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_X), null);
                            meshComp = children[(int)enAxisType.Edge_X_MinPlane - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TX), null);
                        }
                        break;
                    case enAxisType.Edge_X_Max:
                    case enAxisType.Edge_X_MaxPlane:
                        {
                            var meshComp = children[(int)enAxisType.Edge_X_Max - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_X), null);
                            meshComp = children[(int)enAxisType.Edge_X_MaxPlane - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TX), null);
                        }
                        break;

                    case enAxisType.Edge_Y_Min:
                    case enAxisType.Edge_Y_MinPlane:
                        {
                            var meshComp = children[(int)enAxisType.Edge_Y_Min - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Y), null);
                            meshComp = children[(int)enAxisType.Edge_Y_MinPlane - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TY), null);
                        }
                        break;
                    case enAxisType.Edge_Y_Max:
                    case enAxisType.Edge_Y_MaxPlane:
                        {
                            var meshComp = children[(int)enAxisType.Edge_Y_Max - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Y), null);
                            meshComp = children[(int)enAxisType.Edge_Y_MaxPlane - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TY), null);
                        }
                        break;

                    case enAxisType.Edge_Z_Min:
                    case enAxisType.Edge_Z_MinPlane:
                        {
                            var meshComp = children[(int)enAxisType.Edge_Z_Min - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Z), null);
                            meshComp = children[(int)enAxisType.Edge_Z_MinPlane - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TZ), null);
                        }
                        break;
                    case enAxisType.Edge_Z_Max:
                    case enAxisType.Edge_Z_MaxPlane:
                        {
                            var meshComp = children[(int)enAxisType.Edge_Z_Max - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Z), null);
                            meshComp = children[(int)enAxisType.Edge_Z_MaxPlane - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                            await meshComp.SetMaterialInstance(rc, 0, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_TZ), null);
                        }
                        break;
                }
            }
        }
        async Task MousePointToItem(System.Windows.Forms.MouseEventArgs e)
        {
            if (!mAxisInitialized)
                return;
            if (mIsTransAxisOperation)
                return;
            if (RPolicy == null)
            {
                return;
            }

            if (!RPolicy.mHitProxy.mEnabled)
                return;

            var id = RPolicy.mHitProxy.GetHitProxyID((uint)(e.X), (uint)(e.Y));
            var actor = EngineNS.CEngine.Instance.HitProxyManager.FindActor(id);
            var rc = EngineNS.CEngine.Instance.RenderContext;

            List<EngineNS.GamePlay.Actor.GActor> children = new List<EngineNS.GamePlay.Actor.GActor>();
            switch (mAxisOperationType)
            {
                case enAxisOperationType.Edge:
                    children.Clear();
                    foreach (var pref in mEdgePrefabs)
                    {
                        children.AddRange(pref.GetChildrenUnsafe());
                    }
                    break;
                default:
                    if (mCurrentAxisActor != null)
                        children = mCurrentAxisActor.GetChildrenUnsafe();
                    break;
            }

            int idx = -1;
            if (actor != null)
                idx = children.IndexOf(actor);
            if (idx >= 0)
            {
                // 选中对象变黄
                enAxisType tagAxisType = enAxisType.Null;
                switch (mAxisOperationType)
                {
                    case enAxisOperationType.Move:
                        tagAxisType = (enAxisType)idx;
                        break;
                    case enAxisOperationType.Rot:
                        tagAxisType = (enAxisType)(idx + enAxisType.Rot_Start);
                        break;
                    case enAxisOperationType.Scale:
                        tagAxisType = (enAxisType)(idx + enAxisType.Scale_Start);
                        break;
                    case enAxisOperationType.Edge:
                        tagAxisType = (enAxisType)(idx + enAxisType.Edge_Start);
                        break;
                }
                if (tagAxisType != mCurrentAxisType)
                {
                    var oldType = mCurrentAxisType;
                    mCurrentAxisType = tagAxisType;
                    await ResetAxisMaterial(oldType);
                    var mat = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_Selected));
                    var mat_t = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mAxisMaterial_T_Selected));

                    switch (mCurrentAxisType)
                    {
                        case enAxisType.Move_X:
                        case enAxisType.Move_Y:
                        case enAxisType.Move_Z:
                        case enAxisType.Scale_X:
                        case enAxisType.Scale_Y:
                        case enAxisType.Scale_Z:
                            {
                                var meshComp = actor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                            }
                            break;
                        case enAxisType.Move_XYZ:
                            {
                                foreach (var child in children)
                                {
                                    var meshComp = child.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                    await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                }
                                var tMeshComp = children[(int)enAxisType.Move_Plane_XY].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await tMeshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                                tMeshComp = children[(int)enAxisType.Move_Plane_YZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await tMeshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                                tMeshComp = children[(int)enAxisType.Move_Plane_XZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await tMeshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                            }
                            break;
                        case enAxisType.Scale_XYZ:
                            {
                                foreach (var child in children)
                                {
                                    var meshComp = child.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                    await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                }
                                var tMeshComp = children[(int)enAxisType.Scale_Plane_XY - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await tMeshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                                tMeshComp = children[(int)enAxisType.Scale_Plane_YZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await tMeshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                                tMeshComp = children[(int)enAxisType.Scale_Plane_XZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await tMeshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                            }
                            break;
                        case enAxisType.Move_XY:
                        case enAxisType.Move_YX:
                        case enAxisType.Move_Plane_XY:
                            {
                                var meshComp = children[(int)enAxisType.Move_X].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Move_Y].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Move_XY].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Move_YX].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);

                                meshComp = children[(int)enAxisType.Move_Plane_XY].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                                //Children[(int)enAxisType.Move_Plane_XY].Visible = true;
                            }
                            break;
                        case enAxisType.Move_XZ:
                        case enAxisType.Move_ZX:
                        case enAxisType.Move_Plane_XZ:
                            {
                                var meshComp = children[(int)enAxisType.Move_X].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Move_Z].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Move_XZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Move_ZX].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);

                                meshComp = children[(int)enAxisType.Move_Plane_XZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                                //Children[(int)enAxisType.Move_Plane_XZ].Visible = true;
                            }
                            break;
                        case enAxisType.Move_YZ:
                        case enAxisType.Move_ZY:
                        case enAxisType.Move_Plane_YZ:
                            {
                                var meshComp = children[(int)enAxisType.Move_Y].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Move_Z].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Move_YZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Move_ZY].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);

                                meshComp = children[(int)enAxisType.Move_Plane_YZ].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                                //Children[(int)enAxisType.Move_Plane_YZ].Visible = true;
                            }
                            break;
                        case enAxisType.Rot_Line_XY:
                        case enAxisType.Rot_Plane_XY:
                            {
                                var meshComp = children[(int)enAxisType.Rot_Line_XY - (int)enAxisType.Rot_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Rot_Plane_XY - (int)enAxisType.Rot_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                            }
                            break;
                        case enAxisType.Rot_Line_XZ:
                        case enAxisType.Rot_Plane_XZ:
                            {
                                var meshComp = children[(int)enAxisType.Rot_Line_XZ - (int)enAxisType.Rot_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Rot_Plane_XZ - (int)enAxisType.Rot_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                            }
                            break;
                        case enAxisType.Rot_Line_YZ:
                        case enAxisType.Rot_Plane_YZ:
                            {
                                var meshComp = children[(int)enAxisType.Rot_Line_YZ - (int)enAxisType.Rot_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Rot_Plane_YZ - (int)enAxisType.Rot_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                            }
                            break;
                        case enAxisType.Scale_XY:
                        case enAxisType.Scale_YX:
                        case enAxisType.Scale_Plane_XY:
                            {
                                var meshComp = children[(int)enAxisType.Scale_X - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Scale_Y - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Scale_XY - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Scale_YX - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);

                                meshComp = children[(int)enAxisType.Scale_Plane_XY - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                                //Children[(int)enAxisType.Scale_Plane_XY - (int)enAxisType.Scale_Start].Visible = true;
                            }
                            break;
                        case enAxisType.Scale_XZ:
                        case enAxisType.Scale_ZX:
                        case enAxisType.Scale_Plane_XZ:
                            {
                                var meshComp = children[(int)enAxisType.Scale_X - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Scale_Z - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Scale_XZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Scale_ZX - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);

                                meshComp = children[(int)enAxisType.Scale_Plane_XZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                                //Children[(int)enAxisType.Scale_Plane_XZ - (int)enAxisType.Scale_Start].Visible = true;
                            }
                            break;
                        case enAxisType.Scale_YZ:
                        case enAxisType.Scale_ZY:
                        case enAxisType.Scale_Plane_YZ:
                            {
                                var meshComp = children[(int)enAxisType.Scale_Y - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Scale_Z - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Scale_YZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Scale_ZY - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);

                                meshComp = children[(int)enAxisType.Scale_Plane_YZ - (int)enAxisType.Scale_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                                //Children[(int)enAxisType.Scale_Plane_YZ - (int)enAxisType.Scale_Start].Visible = true;
                            }
                            break;
                        case enAxisType.Edge_X_Min:
                        case enAxisType.Edge_X_MinPlane:
                            {
                                var meshComp = children[(int)enAxisType.Edge_X_Min - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Edge_X_MinPlane - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                            }
                            break;
                        case enAxisType.Edge_X_Max:
                        case enAxisType.Edge_X_MaxPlane:
                            {
                                var meshComp = children[(int)enAxisType.Edge_X_Max - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Edge_X_MaxPlane - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                            }
                            break;

                        case enAxisType.Edge_Y_Min:
                        case enAxisType.Edge_Y_MinPlane:
                            {
                                var meshComp = children[(int)enAxisType.Edge_Y_Min - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Edge_Y_MinPlane - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                            }
                            break;
                        case enAxisType.Edge_Y_Max:
                        case enAxisType.Edge_Y_MaxPlane:
                            {
                                var meshComp = children[(int)enAxisType.Edge_Y_Max - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Edge_Y_MaxPlane - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                            }
                            break;

                        case enAxisType.Edge_Z_Min:
                        case enAxisType.Edge_Z_MinPlane:
                            {
                                var meshComp = children[(int)enAxisType.Edge_Z_Min - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Edge_Z_MinPlane - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                            }
                            break;
                        case enAxisType.Edge_Z_Max:
                        case enAxisType.Edge_Z_MaxPlane:
                            {
                                var meshComp = children[(int)enAxisType.Edge_Z_Max - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat, null);
                                meshComp = children[(int)enAxisType.Edge_Z_MaxPlane - (int)enAxisType.Edge_Start].GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                                await meshComp.SetMaterialInstanceAsync(rc, 0, mat_t, null);
                            }
                            break;
                    }
                }
            }
            else
            {
                var oldType = mCurrentAxisType;
                mCurrentAxisType = enAxisType.Null;
                await ResetAxisMaterial(oldType);
            }

            if (mAxisSelectMode == enAxisSelectMode.ObjectPos)
            {
                if (actor != null)
                {
                    if (mSelectedActors.ContainsKey(actor.ActorId))
                    {
                        mAxisPosActor = actor;
                    }
                }
            }
        }

        void FitAxisSize()
        {
            if (!mAxisInitialized)
                return;

            var screenSize = 0.15f;
            var meshSize = 1.0f;
            switch (mAxisOperationType)
            {
                case enAxisOperationType.Edge:
                    {
                        foreach (var actor in mEdgePrefabs)
                        {
                            var size = Camera.GetScreenSizeInWorld(actor.Placement.Location, screenSize);
                            actor.Placement.Scale = size * EngineNS.Vector3.UnitXYZ / meshSize;
                        }
                    }
                    break;
                default:
                    if (mCurrentAxisActor != null)
                    {
                        var size = Camera.GetScreenSizeInWorld(mCurrentAxisActor.Placement.Location, screenSize);
                        mCurrentAxisActor.Placement.Scale = size * EngineNS.Vector3.UnitXYZ / meshSize;
                    }
                    break;
            }
        }

        EngineNS.Matrix mStartTransAxisWorldMatrix;
        EngineNS.Matrix mStartTransAxisWorldMatrixInv;
        EngineNS.Vector3 mStartTransAxisLocation;
        EngineNS.Vector3 mMouseStartLocation;
        EngineNS.Vector3 mMouseTransOffset;
        EngineNS.Vector3 mMouseStartScreenLocation;
        EngineNS.Vector3 mCurrentAxisStartLocation;
        EngineNS.Matrix mCurrentAxisStartMatrix;
        EngineNS.Quaternion mCurrentAxisStartRot;
        EngineNS.BoundingBox mStartEdgeBB;
        EngineNS.Matrix mLastSelectedActorMatWithoutScale;
        EngineNS.Matrix mLastSelectedActorMatWithoutScaleInv;
        EngineNS.Quaternion mLastSelectedActorRot;
        // 正在操作坐标轴
        bool mIsTransAxisOperation = false;
        void StartTransAxis(System.Windows.Forms.MouseEventArgs e)
        {
            if (!mAxisInitialized)
                return;
            if (mCurrentAxisType == enAxisType.Null)
                return;

            EngineNS.Vector3 scale;
            EngineNS.Quaternion rot;
            mCopyWhenTrans = false;
            mIsTransAxisOperation = true;
            mLastSelectedActorMatWithoutScale = EngineNS.Matrix.Identity;
            mLastSelectedActorRot = EngineNS.Quaternion.Identity;

            switch (mAxisOperationType)
            {
                case enAxisOperationType.Edge:
                    {
                        var prefab = mEdgePrefabs[(mCurrentAxisType - enAxisType.Edge_Start) / 2];
                        if (CenterAxisMode)
                        {
                            //mStartTransAxisWorldMatrix = mAxisOrigionMatrixBeforeCenterOperation;
                            mStartEdgeBB = mEdgeAxisOrigionBBBeforeCenterOperation;
                        }
                        else
                        {
                            mStartEdgeBB = mEdgeAxisBB;
                        }
                        mStartTransAxisWorldMatrix = prefab.Placement.WorldMatrix;
                        mCurrentAxisStartLocation = prefab.Placement.Location;
                        mCurrentAxisStartMatrix = prefab.Placement.WorldMatrix;
                        mCurrentAxisStartRot = prefab.Placement.Rotation;

                        if (mSelectedActors.Count > 0)
                        {
                            var actors = mSelectedActors.Values.ToArray();
                            var actor = actors[actors.Length - 1].Actor;
                            switch (mAxisWLType)
                            {
                                case enAxisWLType.Local:
                                    mLastSelectedActorMatWithoutScale = EngineNS.Matrix.Transformation(EngineNS.Vector3.UnitXYZ, actor.Placement.Rotation, actor.Placement.Location);
                                    break;
                            }
                            mLastSelectedActorRot = actor.Placement.Rotation;
                        }

                        mLastSelectedActorMatWithoutScaleInv = EngineNS.Matrix.Invert(ref mLastSelectedActorMatWithoutScale);
                    }
                    break;
                default:
                    {
                        mCurrentAxisActor.NeedRefreshNavgation = false;

                        if (CenterAxisMode)
                            mStartTransAxisWorldMatrix = mAxisOrigionMatrixBeforeCenterOperation;
                        else
                            mStartTransAxisWorldMatrix = mCurrentAxisActor.Placement.WorldMatrix;
                        //mStartTransAxisLocation = mCurrentAxisActor.Placement.Location;
                        mCurrentAxisStartLocation = mCurrentAxisActor.Placement.Location;
                        mCurrentAxisStartMatrix = mCurrentAxisActor.Placement.WorldMatrix;
                        mCurrentAxisStartRot = mCurrentAxisActor.Placement.Rotation;
                    }
                    break;
            }
            mStartTransAxisWorldMatrixInv = EngineNS.Matrix.Invert(ref mStartTransAxisWorldMatrix);
            mStartTransAxisWorldMatrix.Decompose(out scale, out rot, out mStartTransAxisLocation);

            // 计算鼠标在3D空间中的点击位置
            EngineNS.Vector3 planeAxis = EngineNS.Vector3.UnitY;
            switch (mCurrentAxisType)
            {
                case enAxisType.Move_X:
                case enAxisType.Scale_X:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitX, ref mStartTransAxisWorldMatrix, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, Camera.CameraData.Direction);
                        tempNormal.Normalize();
                        planeAxis = EngineNS.Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                    }
                    break;
                case enAxisType.Move_XY:
                case enAxisType.Move_YX:
                case enAxisType.Move_Plane_XY:
                case enAxisType.Rot_Line_XY:
                case enAxisType.Rot_Plane_XY:
                case enAxisType.Scale_XY:
                case enAxisType.Scale_YX:
                case enAxisType.Scale_Plane_XY:
                    {
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitZ, ref mStartTransAxisWorldMatrix, out planeAxis);
                    }
                    break;
                case enAxisType.Move_Y:
                case enAxisType.Scale_Y:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitY, ref mStartTransAxisWorldMatrix, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, Camera.CameraData.Direction);
                        tempNormal.Normalize();
                        planeAxis = EngineNS.Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                    }
                    break;
                case enAxisType.Move_XZ:
                case enAxisType.Move_ZX:
                case enAxisType.Move_Plane_XZ:
                case enAxisType.Rot_Line_XZ:
                case enAxisType.Rot_Plane_XZ:
                case enAxisType.Scale_XZ:
                case enAxisType.Scale_ZX:
                case enAxisType.Scale_Plane_XZ:
                    {
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitY, ref mStartTransAxisWorldMatrix, out planeAxis);
                    }
                    break;
                case enAxisType.Move_Z:
                case enAxisType.Scale_Z:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitZ, ref mStartTransAxisWorldMatrix, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, Camera.CameraData.Direction);
                        tempNormal.Normalize();
                        planeAxis = EngineNS.Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                    }
                    break;
                case enAxisType.Move_YZ:
                case enAxisType.Move_ZY:
                case enAxisType.Move_Plane_YZ:
                case enAxisType.Rot_Line_YZ:
                case enAxisType.Rot_Plane_YZ:
                case enAxisType.Scale_YZ:
                case enAxisType.Scale_ZY:
                case enAxisType.Scale_Plane_YZ:
                    {
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitX, ref mStartTransAxisWorldMatrix, out planeAxis);
                    }
                    break;
                case enAxisType.Scale_XYZ:
                    {
                        planeAxis = -Camera.CameraData.Direction;
                    }
                    break;
                case enAxisType.Edge_X_Min:
                case enAxisType.Edge_X_MinPlane:
                case enAxisType.Edge_X_Max:
                case enAxisType.Edge_X_MaxPlane:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitX, ref mStartTransAxisWorldMatrix, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, Camera.CameraData.Direction);
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
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitY, ref mStartTransAxisWorldMatrix, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, Camera.CameraData.Direction);
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
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitZ, ref mStartTransAxisWorldMatrix, out transAxis);
                        var tempNormal = EngineNS.Vector3.Cross(transAxis, Camera.CameraData.Direction);
                        tempNormal.Normalize();
                        planeAxis = EngineNS.Vector3.Cross(transAxis, tempNormal);
                        planeAxis.Normalize();
                    }
                    break;
            }
            mMouseStartScreenLocation = new EngineNS.Vector3(e.X, e.Y, 0);
            PickPlanePos(e.X, e.Y, ref mCurrentAxisStartLocation, ref planeAxis, out mMouseStartLocation);
            mMouseTransOffset = mMouseStartLocation - mCurrentAxisStartLocation;

            // 记录选中对象矩阵，用于Undo等
            foreach (var data in mSelectedActors)
            {
                data.Value.StartTransMatrix = data.Value.Actor.Placement.WorldMatrix;
            }
        }
        bool mFirstTransAxis = false;
        bool mCopyWhenTrans = false;
        async System.Threading.Tasks.Task TransAxis(System.Drawing.Point newLoc, System.Windows.Forms.MouseEventArgs e)
        {
            if (!mAxisInitialized)
                return;
            if (!mIsTransAxisOperation)
                return;

            if (mFirstTransAxis)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (CanDuplicate)
                    {
                        mEnableTick = false;
                        // 复制Actors
                        var actors = new List<SelectActorData>(mSelectedActors.Values);
                        List<SelectActorData> selDatas = new List<SelectActorData>();
                        foreach (var actor in actors)
                        {
                            if (actor.Actor is EngineNS.GamePlay.Actor.GPrefab && !CanDuplicatePrefab)
                            {
                                continue;
                            }
                            var copyedActor = await actor.Actor.Clone(EngineNS.CEngine.Instance.RenderContext);
                            copyedActor.SetParent(actor.Actor.Parent);
                            EngineNS.CEngine.Instance.HitProxyManager.MapActor(copyedActor);
                            copyedActor.SpecialName = EngineNS.GamePlay.SceneGraph.GSceneGraph.GeneratorActorSpecialNameInEditor(actor.Actor.SpecialName, World);
                            OnDuplicateAcotr?.Invoke(this, new DuplicateActorEventArgs(actor.Actor, copyedActor));
                            World.AddActor(copyedActor);
                            World.DefaultScene.AddActor(copyedActor);
                            if (DAddActor != null)
                                DAddActor(copyedActor);

                            actor.Actor.Selected = false;
                            copyedActor.Selected = true;
                            actor.Actor = copyedActor;
                            selDatas.Add(actor);
                        }
                        if (selDatas.Count > 0)
                        {
                            mSelectedActors.Clear();
                            SelectActors(selDatas.ToArray());
                            mCopyWhenTrans = true;
                        }
                        mEnableTick = true;
                    }
                }
                mFirstTransAxis = false;
            }

            switch (mCurrentAxisType)
            {
                case enAxisType.Move_X:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitX, ref mStartTransAxisWorldMatrix, out transAxis);
                        MoveWithAxis(ref newLoc, ref transAxis);
                    }
                    break;
                case enAxisType.Move_XY:
                case enAxisType.Move_YX:
                case enAxisType.Move_Plane_XY:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitZ, ref mStartTransAxisWorldMatrix, out transAxis);
                        MoveWithPlane(e.X, e.Y, ref transAxis);
                    }
                    break;
                case enAxisType.Move_Y:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitY, ref mStartTransAxisWorldMatrix, out transAxis);
                        MoveWithAxis(ref newLoc, ref transAxis);
                    }
                    break;
                case enAxisType.Move_XZ:
                case enAxisType.Move_ZX:
                case enAxisType.Move_Plane_XZ:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitY, ref mStartTransAxisWorldMatrix, out transAxis);
                        MoveWithPlane(e.X, e.Y, ref transAxis);
                    }
                    break;
                case enAxisType.Move_Z:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitZ, ref mStartTransAxisWorldMatrix, out transAxis);
                        MoveWithAxis(ref newLoc, ref transAxis);
                    }
                    break;
                case enAxisType.Move_YZ:
                case enAxisType.Move_ZY:
                case enAxisType.Move_Plane_YZ:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitX, ref mStartTransAxisWorldMatrix, out transAxis);
                        MoveWithPlane(e.X, e.Y, ref transAxis);
                    }
                    break;
                case enAxisType.Rot_Line_XY:
                case enAxisType.Rot_Plane_XY:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitZ, ref mStartTransAxisWorldMatrix, out transAxis);
                        RotWithAxis(ref newLoc, ref transAxis);
                    }
                    break;
                case enAxisType.Rot_Line_XZ:
                case enAxisType.Rot_Plane_XZ:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitY, ref mStartTransAxisWorldMatrix, out transAxis);
                        RotWithAxis(ref newLoc, ref transAxis);
                    }
                    break;
                case enAxisType.Rot_Line_YZ:
                case enAxisType.Rot_Plane_YZ:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitX, ref mStartTransAxisWorldMatrix, out transAxis);
                        RotWithAxis(ref newLoc, ref transAxis);
                    }
                    break;
                case enAxisType.Scale_X:
                    {
                        var origionAxis = EngineNS.Vector3.UnitX;
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref origionAxis, ref mStartTransAxisWorldMatrix, out transAxis);
                        transAxis.Normalize();
                        var tempDir = EngineNS.Vector3.Cross(transAxis, Camera.CameraData.Direction);
                        tempDir.Normalize();
                        var planeAxis = EngineNS.Vector3.Cross(transAxis, tempDir);
                        planeAxis.Normalize();

                        ScaleWithAxis(ref newLoc, ref planeAxis, ref transAxis, ref origionAxis);
                    }
                    break;
                case enAxisType.Scale_Y:
                    {
                        var origionAxis = EngineNS.Vector3.UnitY;
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref origionAxis, ref mStartTransAxisWorldMatrix, out transAxis);
                        transAxis.Normalize();
                        var tempDir = EngineNS.Vector3.Cross(transAxis, Camera.CameraData.Direction);
                        tempDir.Normalize();
                        var planeAxis = EngineNS.Vector3.Cross(transAxis, tempDir);
                        planeAxis.Normalize();
                        ScaleWithAxis(ref newLoc, ref planeAxis, ref transAxis, ref origionAxis);
                    }
                    break;
                case enAxisType.Scale_Z:
                    {
                        var origionAxis = EngineNS.Vector3.UnitZ;
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref origionAxis, ref mStartTransAxisWorldMatrix, out transAxis);
                        transAxis.Normalize();
                        var tempDir = EngineNS.Vector3.Cross(transAxis, Camera.CameraData.Direction);
                        tempDir.Normalize();
                        var planeAxis = EngineNS.Vector3.Cross(transAxis, tempDir);
                        planeAxis.Normalize();
                        ScaleWithAxis(ref newLoc, ref planeAxis, ref transAxis, ref origionAxis);
                    }
                    break;
                case enAxisType.Scale_XZ:
                case enAxisType.Scale_ZX:
                case enAxisType.Scale_Plane_XZ:
                    {
                        var origionAxis = EngineNS.Vector3.UnitX + EngineNS.Vector3.UnitZ;
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref origionAxis, ref mStartTransAxisWorldMatrix, out transAxis);
                        transAxis.Normalize();
                        var planeAxis = EngineNS.Vector3.TransformNormal(EngineNS.Vector3.UnitY, mStartTransAxisWorldMatrix);
                        ScaleWithAxis(ref newLoc, ref planeAxis, ref transAxis, ref origionAxis);
                    }
                    break;
                case enAxisType.Scale_YX:
                case enAxisType.Scale_XY:
                case enAxisType.Scale_Plane_XY:
                    {
                        var origionAxis = EngineNS.Vector3.UnitX + EngineNS.Vector3.UnitY;
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref origionAxis, ref mStartTransAxisWorldMatrix, out transAxis);
                        transAxis.Normalize();
                        var planeAxis = EngineNS.Vector3.TransformNormal(EngineNS.Vector3.UnitZ, mStartTransAxisWorldMatrix);
                        ScaleWithAxis(ref newLoc, ref planeAxis, ref transAxis, ref origionAxis);
                    }
                    break;
                case enAxisType.Scale_YZ:
                case enAxisType.Scale_ZY:
                case enAxisType.Scale_Plane_YZ:
                    {
                        var origionAxis = EngineNS.Vector3.UnitZ + EngineNS.Vector3.UnitY;
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref origionAxis, ref mStartTransAxisWorldMatrix, out transAxis);
                        transAxis.Normalize();
                        var planeAxis = EngineNS.Vector3.TransformNormal(EngineNS.Vector3.UnitX, mStartTransAxisWorldMatrix);
                        ScaleWithAxis(ref newLoc, ref planeAxis, ref transAxis, ref origionAxis);
                    }
                    break;
                case enAxisType.Scale_XYZ:
                    {
                        var origionAxis = EngineNS.Vector3.UnitXYZ;
                        var transAxis = Camera.CameraData.Up;
                        var planeAxis = -Camera.CameraData.Direction;
                        ScaleWithAxis(ref newLoc, ref planeAxis, ref transAxis, ref origionAxis);
                    }
                    break;
                case enAxisType.Edge_X_Min:
                case enAxisType.Edge_X_MinPlane:
                case enAxisType.Edge_X_Max:
                case enAxisType.Edge_X_MaxPlane:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitX, ref mStartTransAxisWorldMatrix, out transAxis);
                        EdgeWithAxis(ref newLoc, ref transAxis, mCurrentAxisType);
                    }
                    break;
                case enAxisType.Edge_Y_Min:
                case enAxisType.Edge_Y_MinPlane:
                case enAxisType.Edge_Y_Max:
                case enAxisType.Edge_Y_MaxPlane:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitY, ref mStartTransAxisWorldMatrix, out transAxis);
                        EdgeWithAxis(ref newLoc, ref transAxis, mCurrentAxisType);
                    }
                    break;
                case enAxisType.Edge_Z_Min:
                case enAxisType.Edge_Z_MinPlane:
                case enAxisType.Edge_Z_Max:
                case enAxisType.Edge_Z_MaxPlane:
                    {
                        EngineNS.Vector3 transAxis;
                        EngineNS.Vector3.TransformNormal(ref EngineNS.Vector3.UnitZ, ref mStartTransAxisWorldMatrix, out transAxis);
                        EdgeWithAxis(ref newLoc, ref transAxis, mCurrentAxisType);
                    }
                    break;
            }
        }
        void EndTransAxis()
        {
            if (!mAxisInitialized)
                return;
            if (!mIsTransAxisOperation)
                return;
            mIsTransAxisOperation = false;
            if (mCurrentAxisActor != null)
                mCurrentAxisActor.NeedRefreshNavgation = true;
            // UndoRedo操作
            var copyWhenTrans = mCopyWhenTrans;
            var oldAxisPosActor = mAxisPosActor;
            var oldActorMatrixs = new List<SelectActorData>();
            var newActorMatrixs = new List<SelectActorData>();
            foreach (var data in mSelectedActors)
            {
                var oldData = new SelectActorData()
                {
                    StartTransMatrix = data.Value.StartTransMatrix,
                    Actor = data.Value.Actor,
                };
                oldActorMatrixs.Add(oldData);
                var newData = new SelectActorData()
                {
                    StartTransMatrix = data.Value.Actor.Placement.WorldMatrix,
                    Actor = data.Value.Actor,
                };
                newActorMatrixs.Add(newData);
            }
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, (obj) =>
            {
                if (copyWhenTrans)
                {
                    foreach (var data in newActorMatrixs)
                    {
                        World.AddActor(data.Actor);
                        World.DefaultScene.AddActor(data.Actor);
                        if (DAddActor != null)
                            DAddActor(data.Actor);

                        data.Actor.Selected = true;
                    }

                }

                foreach (var data in mSelectedActors)
                {
                    data.Value.Actor.Selected = false;
                }
                mSelectedActors.Clear();
                foreach (var data in newActorMatrixs)
                {
                    data.Actor.Selected = true;
                    data.Actor.Placement.SetMatrix(ref data.StartTransMatrix);
                    mSelectedActors.Add(data.Actor.ActorId, data);
                }
                mAxisPosActor = oldAxisPosActor;
            }, null,
            (obj) =>
            {
                if (copyWhenTrans)
                {
                    foreach (var data in oldActorMatrixs)
                    {
                        data.Actor.Selected = false;
                        World.RemoveActor(data.Actor.ActorId);
                        World.DefaultScene.RemoveActor(data.Actor.ActorId);
                    }
                    mSelectedActors.Clear();
                }
                else
                {
                    mSelectedActors.Clear();
                    foreach (var data in oldActorMatrixs)
                    {
                        data.Actor.Selected = true;
                        data.Actor.Placement.SetMatrix(ref data.StartTransMatrix);
                        mSelectedActors.Add(data.Actor.ActorId, data);
                    }
                }
                mAxisPosActor = oldAxisPosActor;
            }, $"操作{mCurrentAxisType}");
        }
        EngineNS.Plane mCheckPlane = new EngineNS.Plane();
        bool PickPlanePos(int x, int y, ref EngineNS.Vector3 planePos, ref EngineNS.Vector3 planeNormal, out EngineNS.Vector3 resultPos)
        {
            resultPos = EngineNS.Vector3.Zero;
            EngineNS.Vector3 pickRay = -EngineNS.Vector3.UnitY;
            var pickResult = Camera.GetPickRay(ref pickRay, x, y, (float)GetViewPortWidth(), (float)GetViewPortHeight());
            if (!pickResult)
                return false;
            var start = Camera.CameraData.Position;

            mCheckPlane.Normal = planeNormal;
            mCheckPlane.D = -EngineNS.Vector3.Dot(planePos, planeNormal);
            var end = start + pickRay * 10000;
            EngineNS.GamePlay.SceneGraph.VHitResult hitResult = new EngineNS.GamePlay.SceneGraph.VHitResult();
            if (EngineNS.GamePlay.SceneGraph.GSceneGraph.LineCheckWithPlane(ref start, ref end, ref mCheckPlane, ref hitResult))
            {
                resultPos = hitResult.Position;
                return true;
            }
            return false;
            //var t = (EngineNS.Vector3.Dot(planeNormal, planePos) - EngineNS.Vector3.Dot(planeNormal, start)) / EngineNS.Vector3.Dot(planeNormal, pickRay);
            //var tagPos = start + pickRay * t;
            //return tagPos;
        }
        void MoveWithAxis([ReadOnly(true)]ref System.Drawing.Point newLoc, ref EngineNS.Vector3 transAxisDir)
        {
            transAxisDir.Normalize();
            if (Camera.IsPerspective)
            {
                var screenAxisLoc = Camera.CameraData.Trans2ViewPort(ref mCurrentAxisStartLocation);
                screenAxisLoc.Z = 0;
                var tag = transAxisDir + mMouseStartLocation;
                var screenTag = Camera.CameraData.Trans2ViewPort(ref tag);
                screenTag.Z = 0;
                var screenAxisDir = screenTag - screenAxisLoc;
                screenAxisDir.Normalize();

                var camRight = Camera.CameraData.Right;
                camRight.Normalize();
                var deltaPos = mCurrentAxisStartLocation + camRight;
                var screenDeltaPos = Camera.CameraData.Trans2ViewPort(ref deltaPos);
                screenDeltaPos.Z = 0;
                var deltaLen = (screenDeltaPos - screenAxisLoc).Length() * (mCurrentAxisStartLocation - Camera.CameraData.Position).LengthSquared();

                var len = (float)(EngineNS.Vector3.Dot(new EngineNS.Vector3(newLoc.X, newLoc.Y, 0) - mMouseStartScreenLocation, screenAxisDir) * 0.000006 * deltaLen);

                var trans = transAxisDir * len;
                var mat = EngineNS.Matrix.Translate(trans);
                if (CenterAxisMode)
                {
                    mAxisOrigionMatrixBeforeCenterOperation = mStartTransAxisWorldMatrix * mat;
                    mCurrentAxisActor.Placement.Location = trans + mCurrentAxisStartLocation;
                }
                else
                {
                    mCurrentAxisActor.Placement.Location = trans + mStartTransAxisLocation;
                }

                foreach (var data in mSelectedActors)
                {
                    var transMat = data.Value.StartTransMatrix * mat;
                    if(data.Value.Actor.Parent != null)
                    {
                        var parentWorldMat = data.Value.Actor.Placement.ParentWorldMatrix;
                        parentWorldMat.Inverse();
                        transMat = transMat * parentWorldMat;
                    }
                    data.Value.Actor.Placement.SetMatrix(ref transMat);
                }
            }
            else
                throw new InvalidOperationException("没实现");
        }
        void MoveWithPlane(int x, int y, ref EngineNS.Vector3 planeNormal)
        {
            planeNormal.Normalize();
            if (Camera.IsPerspective)
            {
                // todo: 计算吸附相关
                EngineNS.Vector3 mouseLoc;
                if (PickPlanePos(x, y, ref mCurrentAxisStartLocation, ref planeNormal, out mouseLoc))
                {
                    mCurrentAxisActor.Placement.Location = mouseLoc - mMouseTransOffset;
                    var mat = EngineNS.Matrix.Translate(mCurrentAxisActor.Placement.Location - mCurrentAxisStartLocation);

                    foreach (var data in mSelectedActors)
                    {
                        var transMat = data.Value.StartTransMatrix * mat;
                        if(data.Value.Actor.Parent != null)
                        {
                            var parentWorldMat = data.Value.Actor.Placement.ParentWorldMatrix;
                            parentWorldMat.Inverse();
                            transMat = transMat * parentWorldMat;
                        }
                        data.Value.Actor.Placement.SetMatrix(ref transMat);
                    }
                }
            }
            else
                throw new InvalidOperationException("没实现");
        }

        bool mIsSnapRotation = true;
        public bool IsSnapRotation
        {
            get => mIsSnapRotation;
            set
            {
                mIsSnapRotation = value;
                OnPropertyChanged("IsSnapRotation");
            }
        }
        float mSnapRotationValue = 5.0f;
        public float SnapRotationValue
        {
            get => mSnapRotationValue;
            set
            {
                mSnapRotationValue = value;
                OnPropertyChanged("SnapRotationValue");
            }
        }

        void RotWithAxis([ReadOnly(true)]ref System.Drawing.Point newLoc, ref EngineNS.Vector3 rotAxisDir)
        {
            rotAxisDir.Normalize();
            if (Camera.IsPerspective)
            {
                var screenAxisLoc = Camera.CameraData.Trans2ViewPort(ref mCurrentAxisStartLocation);
                screenAxisLoc.Z = 0;
                //var screenStart = Camera.Trans2ViewPort(ref mMouseStartLocation);
                var tangentDir = EngineNS.Vector3.Cross(rotAxisDir, mMouseStartLocation - mCurrentAxisStartLocation);
                tangentDir.Normalize();
                var tangTag = tangentDir + mMouseStartLocation;
                var screenTangTag = Camera.CameraData.Trans2ViewPort(ref tangTag);
                screenTangTag.Z = 0;
                var screenTanDir = screenTangTag - mMouseStartScreenLocation;
                screenTanDir.Normalize();

                var len = EngineNS.Vector3.Dot(new EngineNS.Vector3(newLoc.X, newLoc.Y, 0) - mMouseStartScreenLocation, screenTanDir) * 0.01f;
                if (IsSnapRotation && SnapRotationValue > 0)
                {
                    var snapValue = SnapRotationValue / 180 * System.Math.PI;
                    len = (float)((int)(len / snapValue) * snapValue);
                }

                var axisMat = EngineNS.Matrix.Translate(-mStartTransAxisLocation) * EngineNS.Matrix.RotationAxis(rotAxisDir, len) * EngineNS.Matrix.Translate(mStartTransAxisLocation);

                foreach (var data in mSelectedActors)
                {
                    var transMat = data.Value.StartTransMatrix * axisMat;
                    if (data.Value.Actor.Parent != null)
                    {
                        var parentWorldMat = data.Value.Actor.Placement.ParentWorldMatrix;
                        parentWorldMat.Inverse();
                        transMat = transMat * parentWorldMat;
                    }
                    data.Value.Actor.Placement.SetMatrix(ref transMat);
                }

                if (CenterAxisMode)
                {
                    mAxisOrigionMatrixBeforeCenterOperation = mStartTransAxisWorldMatrix * axisMat;
                    var mat = EngineNS.Matrix.Translate(-mCurrentAxisStartLocation) * EngineNS.Matrix.RotationAxis(rotAxisDir, len) * EngineNS.Matrix.Translate(mCurrentAxisStartLocation);
                    mat = mCurrentAxisStartMatrix * mat;
                    mCurrentAxisActor.Placement.SetMatrix(ref mat);
                }
                else
                {
                    axisMat = mStartTransAxisWorldMatrix * axisMat;
                    mCurrentAxisActor.Placement.SetMatrix(ref axisMat);
                }
            }
            else
                throw new InvalidOperationException("没实现");
        }

        bool mIsSnapScale = true;
        public bool IsSnapScale
        {
            get => mIsSnapScale;
            set
            {
                mIsSnapScale = value;
                OnPropertyChanged("IsSnapScale");
            }
        }
        float mSnapScaleValue = 0.1f;
        public float SnapScaleValue
        {
            get => mSnapScaleValue;
            set
            {
                mSnapScaleValue = value;
                OnPropertyChanged("SnapScaleValue");
            }
        }
        void ScaleWithAxis([ReadOnly(true)]ref System.Drawing.Point newLoc, ref EngineNS.Vector3 planeAxis, ref EngineNS.Vector3 transAxisDir, ref EngineNS.Vector3 origionAxisDir)
        {
            if (Camera.IsPerspective)
            {
                var screenAxisLoc = Camera.CameraData.Trans2ViewPort(ref mCurrentAxisStartLocation);
                screenAxisLoc.Z = 0;
                var tag = transAxisDir + mMouseStartLocation;
                var screenTag = Camera.CameraData.Trans2ViewPort(ref tag);
                screenTag.Z = 0;
                var screenAxisDir = screenTag - screenAxisLoc;
                screenAxisDir.Normalize();

                var camRight = Camera.CameraData.Right;
                camRight.Normalize();
                var deltaPos = mCurrentAxisStartLocation + camRight;
                var screenDeltaPos = Camera.CameraData.Trans2ViewPort(ref deltaPos);
                screenDeltaPos.Z = 0;
                var deltaLen = (screenDeltaPos - screenAxisLoc).Length() * (mCurrentAxisStartLocation - Camera.CameraData.Position).LengthSquared();

                var len = (float)(EngineNS.Vector3.Dot(new EngineNS.Vector3(newLoc.X, newLoc.Y, 0) - mMouseStartScreenLocation, screenAxisDir) * 0.000006 * deltaLen);
                if (IsSnapScale && SnapScaleValue > 0)
                {
                    len = (float)((int)(len / SnapScaleValue) * SnapScaleValue);
                }

                foreach (var data in mSelectedActors)
                {
                    EngineNS.Vector3 scale, translation;
                    EngineNS.Quaternion rot;
                    data.Value.StartTransMatrix.Decompose(out scale, out rot, out translation);

                    var transMatrix = EngineNS.Matrix.Transformation(mStartTransAxisLocation, rot, origionAxisDir * len + EngineNS.Vector3.UnitXYZ, mStartTransAxisLocation, EngineNS.Quaternion.Identity, EngineNS.Vector3.Zero);
                    var transMat = data.Value.StartTransMatrix * transMatrix;
                    if (data.Value.Actor.Parent != null)
                    {
                        var parentWorldMat = data.Value.Actor.Placement.ParentWorldMatrix;
                        parentWorldMat.Inverse();
                        transMat = transMat * parentWorldMat;
                    }
                    data.Value.Actor.Placement.SetMatrix(ref transMat);
                }
            }
            else
                throw new InvalidOperationException("没实现");
        }

        void EdgeWithAxis(ref System.Drawing.Point newLoc, ref EngineNS.Vector3 transAxisDir, enAxisType axisType)
        {
            transAxisDir.Normalize();
            if (Camera.IsPerspective)
            {
                var screenAxisLoc = Camera.CameraData.Trans2ViewPort(ref mCurrentAxisStartLocation);
                screenAxisLoc.Z = 0;
                var tag = transAxisDir + mMouseStartLocation;
                var screenTag = Camera.CameraData.Trans2ViewPort(ref tag);
                screenTag.Z = 0;
                var screenAxisDir = screenTag - screenAxisLoc;
                screenAxisDir.Normalize();

                var camRight = Camera.CameraData.Right;
                camRight.Normalize();
                var deltaPos = mCurrentAxisStartLocation + camRight;
                var screenDeltaPos = Camera.CameraData.Trans2ViewPort(ref deltaPos);
                screenDeltaPos.Z = 0;
                var deltaLen = (screenDeltaPos - screenAxisLoc).Length() * (mCurrentAxisStartLocation - Camera.CameraData.Position).LengthSquared();

                var len = (float)(EngineNS.Vector3.Dot(new EngineNS.Vector3(newLoc.X, newLoc.Y, 0) - mMouseStartScreenLocation, screenAxisDir) * 0.000006 * deltaLen);

                var transMat = EngineNS.Matrix.Identity;
                EngineNS.BoundingBox tempBB = mStartEdgeBB;
                var center = tempBB.GetCenter();
                var transCenter = center;
                var scale = EngineNS.Vector3.UnitXYZ;
                var oldSize = tempBB.GetSize();
                switch (axisType)
                {
                    case enAxisType.Edge_X_Min:
                    case enAxisType.Edge_X_MinPlane:
                        {
                            var delta = tempBB.Minimum.X + len;
                            if (!CenterAxisMode)
                                tempBB.Minimum.X = delta;
                            scale = new EngineNS.Vector3((tempBB.Maximum.X - delta) / oldSize.X, 1, 1);
                            transCenter = new EngineNS.Vector3(tempBB.Maximum.X, center.Y, center.Z);
                        }
                        break;
                    case enAxisType.Edge_X_Max:
                    case enAxisType.Edge_X_MaxPlane:
                        {
                            var delta = tempBB.Maximum.X + len;
                            if (!CenterAxisMode)
                                tempBB.Maximum.X = delta;
                            scale = new EngineNS.Vector3((delta - tempBB.Minimum.X) / oldSize.X, 1, 1);
                            transCenter = new EngineNS.Vector3(tempBB.Minimum.X, center.Y, center.Z);
                        }
                        break;
                    case enAxisType.Edge_Y_Min:
                    case enAxisType.Edge_Y_MinPlane:
                        {
                            var delta = tempBB.Minimum.Y + len;
                            if (!CenterAxisMode)
                                tempBB.Minimum.Y = len;
                            scale = new EngineNS.Vector3(1, (tempBB.Maximum.Y - delta) / oldSize.Y, 1);
                            transCenter = new EngineNS.Vector3(center.X, tempBB.Maximum.Y, center.Z);
                        }
                        break;
                    case enAxisType.Edge_Y_Max:
                    case enAxisType.Edge_Y_MaxPlane:
                        {
                            var delta = tempBB.Maximum.Y + len;
                            if (!CenterAxisMode)
                                tempBB.Maximum.Y = len;
                            scale = new EngineNS.Vector3(1, (delta - tempBB.Minimum.Y) / oldSize.Y, 1);
                            transCenter = new EngineNS.Vector3(center.X, tempBB.Minimum.Y, center.Z);
                        }
                        break;
                    case enAxisType.Edge_Z_Min:
                    case enAxisType.Edge_Z_MinPlane:
                        {
                            var delta = tempBB.Minimum.Z + len;
                            if (!CenterAxisMode)
                                tempBB.Minimum.Z = len;
                            scale = new EngineNS.Vector3(1, 1, (tempBB.Maximum.Z - delta) / oldSize.Z);
                            transCenter = new EngineNS.Vector3(center.X, center.Y, tempBB.Maximum.Z);
                        }
                        break;
                    case enAxisType.Edge_Z_Max:
                    case enAxisType.Edge_Z_MaxPlane:
                        {
                            var delta = tempBB.Maximum.Z + len;
                            if (!CenterAxisMode)
                                tempBB.Maximum.Z = len;
                            scale = new EngineNS.Vector3(1, 1, (delta - tempBB.Minimum.Z) / oldSize.Z);
                            transCenter = new EngineNS.Vector3(center.X, center.Y, tempBB.Minimum.Z);
                        }
                        break;
                }
                transMat = EngineNS.Matrix.Transformation(transCenter, EngineNS.Quaternion.Identity, scale, transCenter, EngineNS.Quaternion.Identity, EngineNS.Vector3.Zero);

                foreach (var data in mSelectedActors.Values)
                {
                    var actMat = data.StartTransMatrix * mLastSelectedActorMatWithoutScaleInv * transMat * mLastSelectedActorMatWithoutScale;
                    //var actMat = data.StartTransMatrix * transMat;
                    if (data.Actor.Parent != null)
                    {
                        var parentWorldMat = data.Actor.Placement.ParentWorldMatrix;
                        parentWorldMat.Inverse();
                        actMat = actMat * parentWorldMat;
                    }
                    data.Actor.Placement.SetMatrix(ref actMat);
                }

                UpdateEdgeBB();
                UpdateEdgeAxisTransform();
            }
            else
                throw new InvalidOperationException("没实现");
        }
    }
}
