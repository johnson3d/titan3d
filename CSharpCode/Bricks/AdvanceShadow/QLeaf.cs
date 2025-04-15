using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using EngineNS.GamePlay;
using System.ComponentModel;


namespace EngineNS.Bricks.AdvanceShadow
{
    public class TtQLeaf
    {
        public TtQLeaf(TtQNode node)
        {
            HostNode = node;
            ShadowCamera = new TtCamera();
        }
        public uint UpdateShadowMapTime = 0;
        public TtQNode HostNode;
        public TtCamera ShadowCamera;
        public bool IsDirty = true;
        public bool IsContainDynamicObject = false;

        public int PageIndex
        {
            get => HostNode.PageIndex;
        }
        //public ref FShadowPage GetShadowPage()
        //{
        //    return ref HostNode.QTree.ShadowPages[PageIndex];
        //}
        public void UpdateShadowMatrix(TtWorld world)
        {
            if (IsDirty == false)
            {
                return;
            }
            IsDirty = false;
            if (HostNode.ShadowObjects.Count == 0)
                return;

            if (PageIndex >= HostNode.QTree.MaxPageCount)
                return;

            UpdateShadowMapTime = HostNode.QTree.UpdateShadowMapTime;

            ShadowCamera.SetMatrixStartPosition(world.CameraOffset);

            //ref FShadowPage page = ref GetShadowPage();
            DBoundingBox aabb = new DBoundingBox();
            DBoundingBox2D aabb2d = new DBoundingBox2D();
            aabb.InitEmptyBox();
            aabb2d.InitEmptyBox();
            foreach (var i in HostNode.ShadowObjects)
            {
                aabb.Merge(in i.SceneNode.AbsAABB);
                aabb2d.Merge(i.AABB);
            }

            DVector2 c2d;
            float width;
            bool bKeepViewSize = true;
            if (bKeepViewSize == false && HostNode.AABB.Contains(in aabb2d) == ContainmentType.Contains)
            {
                c2d = aabb2d.GetCenter();
                width = (float)aabb2d.GetMaxSide();
            }
            else
            {
                c2d = HostNode.AABB.GetCenter();
                width = (float)HostNode.AABB.GetSize().X;
            }

            var forward = HostNode.QTree.LightDirection;
            var right = Vector3.Right;
            var up = Vector3.Cross(in forward, in right);

            var c3d = new DVector3(c2d.X, 0, c2d.Y);
            var dir = HostNode.QTree.LightDirection.AsDVector();
            dir.Normalize();
            double minDist = double.MaxValue;
            double maxDist = double.MinValue;
            for (int i = 0; i < 8; i++)
            {
                var c = aabb.GetCorner(i) - c3d;
                var d = DVector3.Dot(c, dir);
                if (d > maxDist)
                    maxDist = d;
                if (d < minDist)
                    minDist = d;
            }
            var farLen = maxDist - minDist;
            var eye = c3d + dir * (minDist * 1.5 - 100);
            ShadowCamera.LookAtLH(eye, c3d, in up);

            var shadowZNear = 0.3f;// (\float)shadowCameraBox.Minimum.Z;
            var shadowZFar = (float)(farLen * 1.5 + 200.0);

            ShadowCamera.DoOrthoProjectionForShadow(width, width, shadowZNear, shadowZFar, 0, 0);
            ShadowCamera.UpdateConstBufferData(TtEngine.Instance.GfxDevice.RenderContext);

            Matrix vp = ShadowCamera.GetViewProjection();
            HostNode.ShadowMatrix = vp * HostNode.QTree.mOrtho2UVMtx;
        }
    }
}
