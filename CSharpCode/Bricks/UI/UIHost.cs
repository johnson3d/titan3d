using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI
{
    public partial class TtUIHost : Controls.Containers.TtContainer
    {
        TtUINode mSceneNode;
        public TtUINode SceneNode
        {
            get => mSceneNode;
            set
            {
                mSceneNode = value;
                if(mSceneNode != null && mSceneNode.UIHost != this)
                {
                    mSceneNode.UIHost = this;
                }
            }
        }
        public Graphics.Pipeline.UViewportSlate ViewportSlate { get; internal set; }

        float mDPIScale = 1.0f;
        [Browsable(false)]
        public float DPIScale => mDPIScale;

        SizeF mWindowSize;
        [Browsable(false)]
        public SizeF WindowSize
        {
            get => mWindowSize;
            set
            {
                if (mWindowSize.Equals(in value))
                    return;

                mWindowSize = value;
                SizeF tagDesignSize;
                mDPIScale = UEngine.Instance.UIManager.Config.GetDPIScaleAndDesignSize(mWindowSize.Width, mWindowSize.Height, out tagDesignSize);
                var newRect = new RectangleF(0, 0, tagDesignSize.Width, tagDesignSize.Height);
                SetDesignRect(in newRect, true);
                var childrenCount = VisualTreeHelper.GetChildrenCount(this);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(this, i);
                    child?.UpdateLayout();
                }
            }
        }

        public override ref FTransform AbsRenderTransform
        {
            get
            {
                if (SceneNode == null)
                    return ref FTransform.IdentityForRef;
                return ref SceneNode.Placement.AbsTransform;
            }
        }

        public RectangleF WindowRectangle = RectangleF.Empty;
        public bool Has3DElement = false;

        [Rtti.Meta]
        [RName.PGRName(FilterExts = TtUIAsset.AssetExt)]
        public RName AssetName
        {
            get;
            set;
        }

        public TtUIHost()
        {
            WindowSize = new SizeF(1920, 1080);
            BypassLayoutPolicies = true;
        }

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            return mWindowSize;
        }
        protected override void ArrangeOverride(in RectangleF arrangeSize)
        {
            var visualChildrenCount = VisualTreeHelper.GetChildrenCount(this);
            for (int i = 0; i < visualChildrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(this, i);
                child?.Arrange(in arrangeSize);
            }
            MeshDirty = true;
        }
        public override Vector2 GetPointWith2DSpacePoint(in Vector2 pt)
        {
            var retPt = Vector2.Zero;
            retPt.X = pt.X / mDPIScale - DesignRect.X;
            retPt.Y = pt.Y / mDPIScale - DesignRect.Y;
            return retPt;
        }

        bool RayIntersect3DElements(TtUIElement element, ref RayIntersectData data)
        {
            if (!element.Is3D)
                return false;
            if(element.RayIntersect(ref data))
            {
                data.IntersectedElement = element;
                return true;
            }
            return false;
        }
        public override bool IsMousePointIn(in Vector2 mousePoint)
        {
            var vp = this.ViewportSlate;
            var aabb = this.BoundingBox;
            if (aabb.IsEmpty())
                return false;
            if (RenderCamera == null)
                return false;
            var data = new RayIntersectData();
            data.Start = RenderCamera.GetLocalPosition();
            Vector3 dir = Vector3.UnitX;
            RenderCamera.GetPickRay(ref dir, mousePoint.X, mousePoint.Y, vp.WindowSize.Width, vp.WindowSize.Height);
            data.Direction = dir;

            var ray = new Ray(data.Start, data.Direction);
            if (!Ray.Intersects(in ray, BoundingBox, out data.Distance))
                return false;

            if(QueryElements(RayIntersect3DElements, ref data))
            {
                return true;
            }

            if(RayIntersect(ref data))
            {
                return true;
            }

            return false;
        }
        // pt为鼠标位置
        public override TtUIElement GetPointAtElement(in Vector2 mousePt, bool onlyClipped = true)
        {
            var data = new RayIntersectData();
            return GetPointAtElement(mousePt, ref data, onlyClipped);
        }
        public TtUIElement GetPointAtElement(in Vector2 mousePt, ref RayIntersectData data, bool onlyClipped = true)
        {
            if (this.mSceneNode == null)
                return null;
            var vp = this.ViewportSlate;
            var aabb = this.BoundingBox;
            if (aabb.Minimum.X >= aabb.Maximum.X ||
                aabb.Minimum.Y >= aabb.Maximum.Y)
                return null;
            if (RenderCamera == null)
                return null;
            var delta = vp.WindowPos - vp.ViewportPos;
            data.Start = RenderCamera.GetLocalPosition();
            Vector3 dir = Vector3.Zero;
            //var mousePt = new Vector2(178, 209) + delta;
            RenderCamera.GetPickRay(ref dir, mousePt.X - delta.X, mousePt.Y - delta.Y, vp.ClientSize.Width, vp.ClientSize.Height);
            if (dir == Vector3.Zero)
                return null;
            data.Direction = dir;

            //UEngine.Instance.UIManager.DebugMousePt = mousePt;// - delta;
            var ray = new Ray(data.Start, data.Direction);
            if (!Ray.Intersects(in ray, BoundingBox, out data.Distance))
                return null;

            if(QueryElements(RayIntersect3DElements, ref data))
            {
                return data.IntersectedElement.GetPointAtElement(in data.IntersectPos, onlyClipped);
            }
            if (!RayIntersect(ref data))
                return null;
            return base.GetPointAtElement(in data.IntersectPos, onlyClipped);
        }

        // white a c# method for line intersect triangle in 3d, and with intersect point out
        //public Vector3 LineIntersectTriangle(Vector3 lineStart, Vector3 lineEnd, Vector3 trianglePoint1, Vector3 trianglePoint2, Vector3 trianglePoint3)
        //{
        //    Vector3 intersectPoint = Vector3.Zero;

        //    // calculate the plane of the triangle
        //    Vector3 triangleNormal = Vector3.Cross(trianglePoint2 - trianglePoint1, trianglePoint3 - trianglePoint1).NormalizeValue;
        //    float triangleDistance = Vector3.Dot(triangleNormal, trianglePoint1);

        //    // calculate the distance of the line from the plane
        //    float lineDistance = Vector3.Dot(triangleNormal, lineStart) - triangleDistance;

        //    // calculate the direction of the line
        //    Vector3 lineDirection = (lineEnd - lineStart).NormalizeValue;

        //    // calculate the intersection point
        //    float t = -lineDistance / Vector3.Dot(triangleNormal, lineDirection);
        //    intersectPoint = lineStart + lineDirection * t;

        //    // check if the intersection point is inside the triangle
        //    if (Vector3.Dot(Vector3.Cross(trianglePoint2 - trianglePoint1, intersectPoint - trianglePoint1), triangleNormal) > 0 &&
        //        Vector3.Dot(Vector3.Cross(trianglePoint3 - trianglePoint2, intersectPoint - trianglePoint2), triangleNormal) > 0 &&
        //        Vector3.Dot(Vector3.Cross(trianglePoint1 - trianglePoint3, intersectPoint - trianglePoint3), triangleNormal) > 0)
        //    {
        //        return intersectPoint;
        //    }

        //    return Vector3.Zero;
        //}
        //[Test]
        //public void LineIntersectTriangle_Intersects_ReturnsIntersectPoint()
        //{
        //    // Arrange
        //    Vector3 lineStart = new Vector3(0, 0, 0);
        //    Vector3 lineEnd = new Vector3(1, 0, 0);
        //    Vector3 trianglePoint1 = new Vector3(0, 1, 0);
        //    Vector3 trianglePoint2 = new Vector3(1, 1, 0);
        //    Vector3 trianglePoint3 = new Vector3(0, 0, 1);
        //    Vector3 expectedIntersectPoint = new Vector3(0.5f, 0.5f, 0);

        //    // Act
        //    Vector3 actualIntersectPoint = LineIntersectTriangle(lineStart, lineEnd, trianglePoint1, trianglePoint2, trianglePoint3);

        //    // Assert
        //    Assert.AreEqual(expectedIntersectPoint, actualIntersectPoint);
        //}

        //[Test]
        //public void LineIntersectTriangle_NoIntersect_ReturnsZeroVector()
        //{
        //    // Arrange
        //    Vector3 lineStart = new Vector3(0, 0, 0);
        //    Vector3 lineEnd = new Vector3(1, 0, 0);
        //    Vector3 trianglePoint1 = new Vector3(0, 1, 0);
        //    Vector3 trianglePoint2 = new Vector3(1, 1, 0);
        //    Vector3 trianglePoint3 = new Vector3(1, 0, 1);
        //    Vector3 expectedIntersectPoint = Vector3.Zero;

        //    // Act
        //    Vector3 actualIntersectPoint = LineIntersectTriangle(lineStart, lineEnd, trianglePoint1, trianglePoint2, trianglePoint3);

        //    // Assert
        //    Assert.AreEqual(expectedIntersectPoint, actualIntersectPoint);
        //}
    }
}
