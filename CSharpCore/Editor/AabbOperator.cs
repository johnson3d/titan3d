using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class AabbOperator
    {
        private BoundingBox mBox;
        public BoundingBox Box
        {
            get { return mBox; }
        }
        private Graphics.Mesh.CGfxMeshDataProvider mBoxMesh = new Graphics.Mesh.CGfxMeshDataProvider();
        public void SetBox(CRenderContext rc, ref BoundingBox box)
        {
            mBox = box;
            var size = box.GetSize();
            //var mesh = CEngine.Instance.MeshPrimitivesManager.CreateMeshPrimitives(rc, 1);
            //Graphics.Mesh.CGfxMeshCooker.MakeBox(rc, mesh, box.Minimum.X,
            //    box.Minimum.Y,
            //    box.Minimum.Z,
            //    size.X,
            //    size.Y,
            //    size.Z);
            //mBoxMesh.InitFromMesh(rc, mesh);

            Graphics.Mesh.CGfxMeshCooker.MakeBox(rc, mBoxMesh, box.Minimum.X,
                box.Minimum.Y,
                box.Minimum.Z,
                size.X,
                size.Y,
                size.Z);
        }
        public Graphics.Mesh.CGfxMeshCooker.EBoxFace HitFace(ref Vector3 vStart, ref Vector3 vEnd)
        {
            float dist;
            int index = mBoxMesh.IntersectTriangle(ref vStart, ref vEnd, out dist);
            if (index < 0)
                return Graphics.Mesh.CGfxMeshCooker.EBoxFace.All;

            UInt32 a = 0, b = 0, c = 0;
            if(false==mBoxMesh.GetTriangle(index, ref a, ref b, ref c))
                return Graphics.Mesh.CGfxMeshCooker.EBoxFace.All;

            var vA = mBoxMesh.GetPositionOrNormal(EVertexSteamType.VST_Position, a);
            var vB = mBoxMesh.GetPositionOrNormal(EVertexSteamType.VST_Position, b);
            var vC = mBoxMesh.GetPositionOrNormal(EVertexSteamType.VST_Position, c);

            var pln = new Plane(vA, vB, vC);

            float dot = Vector3.Dot(pln.Normal, Vector3.UnitX);
            if( Math.Abs(dot-1.0f)<0.01f )
            {
                return Graphics.Mesh.CGfxMeshCooker.EBoxFace.Right;
            }
            dot = Vector3.Dot(pln.Normal, Vector3.UnitY);
            if (Math.Abs(dot - 1.0f) < 0.01f)
            {
                return Graphics.Mesh.CGfxMeshCooker.EBoxFace.Top;
            }
            dot = Vector3.Dot(pln.Normal, Vector3.UnitZ);
            if (Math.Abs(dot - 1.0f) < 0.01f)
            {
                return Graphics.Mesh.CGfxMeshCooker.EBoxFace.Back;
            }
            dot = Vector3.Dot(pln.Normal, -Vector3.UnitX);
            if (Math.Abs(dot - 1.0f) < 0.01f)
            {
                return Graphics.Mesh.CGfxMeshCooker.EBoxFace.Left;
            }
            dot = Vector3.Dot(pln.Normal, -Vector3.UnitY);
            if (Math.Abs(dot - 1.0f) < 0.01f)
            {
                return Graphics.Mesh.CGfxMeshCooker.EBoxFace.Bottom;
            }
            dot = Vector3.Dot(pln.Normal, -Vector3.UnitZ);
            if (Math.Abs(dot - 1.0f) < 0.01f)
            {
                return Graphics.Mesh.CGfxMeshCooker.EBoxFace.Bottom;
            }
            System.Diagnostics.Debug.Assert(false);

            return Graphics.Mesh.CGfxMeshCooker.EBoxFace.All;
        }
        public bool ChangeFace(CRenderContext rc, Graphics.Mesh.CGfxMeshCooker.EBoxFace face, float delta)
        {
            switch(face)
            {
                case Graphics.Mesh.CGfxMeshCooker.EBoxFace.Left:
                    return ChangeLeftFace(rc, delta);
                case Graphics.Mesh.CGfxMeshCooker.EBoxFace.Right:
                    return ChangeRightFace(rc, delta);
                case Graphics.Mesh.CGfxMeshCooker.EBoxFace.Back:
                    return ChangeBackFace(rc, delta);
                case Graphics.Mesh.CGfxMeshCooker.EBoxFace.Front:
                    return ChangeFrontFace(rc, delta);
                case Graphics.Mesh.CGfxMeshCooker.EBoxFace.Top:
                    return ChangeTopFace(rc, delta);
                case Graphics.Mesh.CGfxMeshCooker.EBoxFace.Bottom:
                    return ChangeBottomFace(rc, delta);
                default:
                    return false;
            }
        }
        private bool ChangeRightFace(CRenderContext rc, float delta)
        {
            if (mBox.Maximum.X + delta < mBox.Minimum.X)
                return false;
            mBox.Maximum.X += delta;
            SetBox(rc, ref mBox);
            return true;
        }
        private bool ChangeLeftFace(CRenderContext rc, float delta)
        {
            if (mBox.Minimum.X + delta > mBox.Maximum.X)
                return false;
            mBox.Minimum.X += delta;
            SetBox(rc, ref mBox);
            return true;
        }
        private bool ChangeBackFace(CRenderContext rc, float delta)
        {
            if (mBox.Maximum.Z + delta < mBox.Minimum.Z)
                return false;
            mBox.Maximum.Z += delta;
            SetBox(rc, ref mBox);
            return true;
        }
        private bool ChangeFrontFace(CRenderContext rc, float delta)
        {
            if (mBox.Minimum.Z + delta > mBox.Maximum.Z)
                return false;
            mBox.Minimum.Z += delta;
            SetBox(rc, ref mBox);
            return true;
        }
        private bool ChangeTopFace(CRenderContext rc, float delta)
        {
            if (mBox.Maximum.Y + delta < mBox.Minimum.Y)
                return false;
            mBox.Maximum.Y += delta;
            SetBox(rc, ref mBox);
            return true;
        }
        private bool ChangeBottomFace(CRenderContext rc, float delta)
        {
            if (mBox.Minimum.Y + delta > mBox.Maximum.Y)
                return false;
            mBox.Minimum.Y += delta;
            SetBox(rc, ref mBox);
            return true;
        }
    }
}
