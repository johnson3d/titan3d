using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Camera
{
    [Bricks.CodeBuilder.ContextMenu("Camera", "Camera", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtGamePlayCamera.TtGamePlayCameraData), DefaultNamePrefix = "Camera")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtGamePlayCamera : GamePlay.Scene.TtLightWeightNodeBase
    {
        public class TtGamePlayCameraData : TtNodeData
        {

        }
        Graphics.Pipeline.TtCamera mCamera;
        public Graphics.Pipeline.TtCamera Camera
        {
            get => mCamera;
            set => mCamera = value;
        }

        #region Fields
        public v3dxFrustum Frustum
        {
            get => mCamera.GetFrustum();
        }
        public EngineNS.DVector3 MatrixStartPosition
        {
            get => mCamera.GetMatrixStartPosition();
            set => mCamera.SetMatrixStartPosition(value);
        }
        public EngineNS.DVector3 Position
        {
            get => mCamera.GetPosition();
        }
        public EngineNS.Vector3 LocalPosition
        {
            get => mCamera.GetLocalPosition();
        }
        public EngineNS.DVector3 LookAt
        {
            get => mCamera.GetLookAt();
        }
        public EngineNS.Vector3 LocalLookAt
        {
            get => mCamera.GetLocalLookAt();
        }
        public EngineNS.Vector3 Direction
        {
            get => mCamera.GetDirection();
        }
        public EngineNS.Vector3 Right
        {
            get => mCamera.GetRight();
        }
        public EngineNS.Vector3 Up
        {
            get => mCamera.GetUp();
        }
        public EngineNS.Matrix ViewMatrix
        {
            get => mCamera.GetViewMatrix();
        }
        public EngineNS.Matrix ViewInverse
        {
            get => mCamera.GetViewInverse();
        }
        public EngineNS.Matrix ProjectionMatrix
        {
            get => mCamera.GetProjectionMatrix();
        }
        public EngineNS.Matrix ProjectionInverse
        {
            get => mCamera.GetProjectionInverse();
        }
        public EngineNS.Matrix ViewProjection
        {
            get => mCamera.GetViewProjection();
        }
        public EngineNS.Matrix ViewProjectionInverse
        {
            get => mCamera.GetViewProjectionInverse();
        }
        public EngineNS.Matrix ViewPortMatrix
        {
            get => mCamera.GetToViewPortMatrix();
        }
        #endregion Fields

        #region Function

        public void PerspectiveFovLH(float fov, float width, float height, float zMin, float zMax)
        {
            mCamera?.PerspectiveFovLH(fov, width, height, zMin, zMax);
        }
        public void MakeOrtho(float w, float h, float zn, float zf)
        {
            mCamera?.MakeOrtho(w, h, zn, zf);
        }
        public void DoOrthoProjectionForShadow(float w, float h, float znear, float zfar, float TexelOffsetNdcX, float TexelOffsetNdcY)
        {
            mCamera?.DoOrthoProjectionForShadow(w, h, znear, zfar, TexelOffsetNdcX, TexelOffsetNdcY);
        }
        public void LookAtLH(in EngineNS.DVector3 eye, in EngineNS.DVector3 lookAt, in EngineNS.Vector3 up)
        {
            mCamera?.LookAtLH(eye, lookAt, up);
        }
        public int GetPickRay(ref EngineNS.Vector3 pvPickRay, float x, float y, float sw, float sh)
        {
            if (mCamera == null)
                return 0;

            return mCamera.GetPickRay(ref pvPickRay, x, y, sw, sh);
        }


        #endregion

        //public override bool OnTickLogic(UWorld world, URenderPolicy policy)
        //{
        //    var dir = Placement.AbsTransform.Quat * Vector3.Forward;
        //    LookAtLH(Location, Location + dir * 10, Vector3.Up);
        //    return base.OnTickLogic(world, policy);
        //}
    }
}
