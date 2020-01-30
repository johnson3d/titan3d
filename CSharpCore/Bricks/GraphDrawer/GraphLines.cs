using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.GraphDrawer
{
    // 三棱柱线
    public unsafe struct GeometryLinePosition
    {
        public fixed float Vert[18 * 3];
        public unsafe void SetLine(ref Vector3 start, ref Vector3 end, ref Vector3 extDir, float halfWidth)
        {
            var axisDir = end - start;
            EngineNS.Quaternion rot = EngineNS.Quaternion.Identity;
            var rotAngle = (float)(System.Math.PI * 2 / 3);
            fixed (GeometryLinePosition* tempPtr = &this)
            {
                float* vertPtr = (float*)tempPtr;
                for (int i = 0; i < 3; i++)
                {
                    EngineNS.Quaternion.RotationAxis(ref axisDir, rotAngle * i, out rot);
                    var curExtDir = EngineNS.Vector3.TransformCoordinate(extDir, rot);
                    float nextAngle = 0;
                    if (i < 3 - 1)
                    {
                        nextAngle = rotAngle * (i + 1);
                    }
                    EngineNS.Quaternion.RotationAxis(ref axisDir, nextAngle, out rot);
                    var nextExtDir = EngineNS.Vector3.TransformCoordinate(extDir, rot);
                    var v0 = start + nextExtDir * halfWidth;
                    var v1 = start + curExtDir * halfWidth;
                    var v2 = end + curExtDir * halfWidth;
                    var v4 = end + nextExtDir * halfWidth;
                    var v5 = v0;

                    vertPtr[i * 18 + 0] = v0.X;
                    vertPtr[i * 18 + 1] = v0.Y;
                    vertPtr[i * 18 + 2] = v0.Z;

                    vertPtr[i * 18 + 3] = v1.X;
                    vertPtr[i * 18 + 4] = v1.Y;
                    vertPtr[i * 18 + 5] = v1.Z;

                    vertPtr[i * 18 + 6] = v2.X;
                    vertPtr[i * 18 + 7] = v2.Y;
                    vertPtr[i * 18 + 8] = v2.Z;

                    vertPtr[i * 18 + 9] = v2.X;
                    vertPtr[i * 18 + 10] = v2.Y;
                    vertPtr[i * 18 + 11] = v2.Z;

                    vertPtr[i * 18 + 12] = v4.X;
                    vertPtr[i * 18 + 13] = v4.Y;
                    vertPtr[i * 18 + 14] = v4.Z;

                    vertPtr[i * 18 + 15] = v0.X;
                    vertPtr[i * 18 + 16] = v0.Y;
                    vertPtr[i * 18 + 17] = v0.Z;
                }
            }
        }
        public unsafe void CalcNormals(ref GeometryLinePosition result)
        {
            fixed (GeometryLinePosition* tempPtr = &result)
            {
                float* vertPtr = (float*)tempPtr;
                for (int i = 0; i < 3; i++)
                {
                    var Vert0 = new Vector3(vertPtr[i * 3], vertPtr[i * 3 + 1], vertPtr[i * 3 + 2]);
                    var Vert1 = new Vector3(vertPtr[(i + 1) * 3], vertPtr[(i + 1) * 3 + 1], vertPtr[(i + 1) * 3 + 2]);
                    var Vert2 = new Vector3(vertPtr[(i + 2) * 3], vertPtr[(i + 2) * 3 + 1], vertPtr[(i + 2) * 3 + 2]);

                    var v2 = Vert2 - Vert0;
                    var v1 = Vert1 - Vert0;
                    if (v1 == Vector3.Zero)
                        Vert0 = Vector3.UnitZ;
                    else
                    {
                        Vert0 = Vector3.Cross(v2, v1);
                        Vert0.Normalize();
                    }

                    vertPtr[(i + 1) * 3] = Vert0.X;
                    vertPtr[(i + 1) * 3 + 1] = Vert0.Y;
                    vertPtr[(i + 1) * 3 + 2] = Vert0.Z;

                    vertPtr[(i + 2) * 3] = Vert0.X;
                    vertPtr[(i + 2) * 3 + 1] = Vert0.Y;
                    vertPtr[(i + 2) * 3 + 2] = Vert0.Z;

                    vertPtr[(i + 3) * 3] = Vert0.X;
                    vertPtr[(i + 3) * 3 + 1] = Vert0.Y;
                    vertPtr[(i + 3) * 3 + 2] = Vert0.Z;

                    vertPtr[(i + 4) * 3] = Vert0.X;
                    vertPtr[(i + 4) * 3 + 1] = Vert0.Y;
                    vertPtr[(i + 4) * 3 + 2] = Vert0.Z;

                    vertPtr[(i + 5) * 3] = Vert0.X;
                    vertPtr[(i + 5) * 3 + 1] = Vert0.Y;
                    vertPtr[(i + 5) * 3 + 2] = Vert0.Z;
                }
            }
        }
    }
    public unsafe struct GeometryLineUV
    {
        public fixed float Vert[2 * 6 * 3];
        public void SetUV()
        {
            fixed (GeometryLineUV* tempPtr = &this)
            {
                float* vertPtr = (float*)tempPtr;
                for (int i = 0; i < 3; i++)
                {
                    vertPtr[i * 3] = 0;
                    vertPtr[i * 3 + 1] = 0;

                    vertPtr[(i + 1) * 3] = 0;
                    vertPtr[(i + 1) * 3 + 1] = 1;

                    vertPtr[(i + 2) * 3] = 1;
                    vertPtr[(i + 2) * 3 + 1] = 1;

                    vertPtr[(i + 3) * 3] = 1;
                    vertPtr[(i + 3) * 3 + 1] = 1;

                    vertPtr[(i + 4) * 3] = 1;
                    vertPtr[(i + 4) * 3 + 1] = 0;

                    vertPtr[(i + 5) * 3] = 0;
                    vertPtr[(i + 5) * 3 + 1] = 0;
                }
            }
        }
    }

    // 平面线
    public unsafe struct LinePosition
    {
        public Vector3 Vert0;
        public Vector3 Vert1;
        public Vector3 Vert2;
        public Vector3 Vert3;
        public Vector3 Vert4;
        public Vector3 Vert5;
        public void SetLine(ref Vector3 start, ref Vector3 end, ref Vector3 extDir, float halfWidth)
        {
            Vert0 = start - extDir * halfWidth;
            Vert1 = start + extDir * halfWidth;
            Vert2 = end + extDir * halfWidth;
            Vert3 = Vert2;
            Vert4 = end - extDir * halfWidth;
            Vert5 = Vert0;
        }
        public void CalcNormals(ref LinePosition result)
        {
            var v2 = Vert2 - Vert0;
            var v1 = Vert1 - Vert0;

            if (v1 == Vector3.Zero)
            {
                result.Vert0 = Vector3.UnitZ;
            }
            else
            {
                result.Vert0 = Vector3.Cross(v2, v1);
                result.Vert0.Normalize();
            }
            result.Vert1 = result.Vert0;
            result.Vert2 = result.Vert0;
            result.Vert3 = result.Vert0;
            result.Vert4 = result.Vert0;
            result.Vert5 = result.Vert0;
        }
        public void CalcTangents(ref LinePosition result)
        {
            var v2 = Vert2 - Vert0;
            result.Vert0 = v2;
            result.Vert1 = v2;
            result.Vert2 = v2;
            result.Vert3 = v2;
            result.Vert4 = v2;
            result.Vert5 = v2;
        }
    }
    public unsafe struct LineUV
    {
        public Vector2 Vert0;
        public Vector2 Vert1;
        public Vector2 Vert2;
        public Vector2 Vert3;
        public Vector2 Vert4;
        public Vector2 Vert5;
        public void SetUV()
        {
            Vert0.SetValue(0, 0);
            Vert1.SetValue(0, 1);
            Vert2.SetValue(1, 1);
            Vert3 = Vert2;
            Vert4.SetValue(1, 0);
            Vert5 = Vert0;
        }
    }
    public class GraphLines
    {
        public Graphics.Mesh.CGfxMeshPrimitives GeomMesh;
        public GamePlay.Actor.GActor GraphActor;
        public McLinesGen LinesGen = new McLinesGen();
        public bool UseGeometry = false;
        Graphics.CGfxMaterialInstance Material;
        public async System.Threading.Tasks.Task<bool> Init(Graphics.CGfxMaterialInstance material, float halfWidth = 0.1F)
        {
            var rc = CEngine.Instance.RenderContext;

            Material = material;
            GeomMesh = new Graphics.Mesh.CGfxMeshPrimitives();
            GeomMesh.Init(rc, null, 1);
            CDrawPrimitiveDesc dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.StartIndex = 0xFFFFFFFF;
            GeomMesh.PushAtomLOD(0, ref dpDesc);

            var Mesh = new Graphics.Mesh.CGfxMesh();
            Mesh.Init(rc, null, GeomMesh);
            var img = new Graphics.Mesh.CGfxImage2D();
            Mesh.Tag = img;

            UpdateGeomMesh(rc, halfWidth);

            GraphActor = GamePlay.Actor.GActor.NewMeshActorDirect(Mesh);
            var meshComp = GraphActor.GetComponent<GamePlay.Component.GMeshComponent>();
            await meshComp.SetMaterialInstanceAsync(rc, 0, material, null);
            //await meshComp.SetMaterial(rc, 0, material, CEngine.Instance.PrebuildPassData.Image2DShadingEnvs, false, true);
            return true;
        }
        private Support.NativeList<LinePosition> mTempPosition = new Support.NativeList<LinePosition>();
        private Support.NativeList<LineUV> mTempUV = new Support.NativeList<LineUV>();
        private Support.NativeList<LinePosition> mTempNormal = new Support.NativeList<LinePosition>();
        public void UpdateGeomMesh(CRenderContext rc, float halfWidth = 0.1F)
        {
            if (GeomMesh == null)
                return;

            CDrawPrimitiveDesc dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();

            if (UseGeometry)
            {
                mTempPosition.Clear();
                mTempUV.Clear();
                LinesGen.BuildGraph(mTempPosition, mTempUV, halfWidth);

                mTempNormal.Clear();
                mTempNormal.SetGrowStep(mTempPosition.Count);
                for (int i = 0; i < mTempPosition.Count; i++)
                {
                    //mTempNormal.Add(mTempPosition[i].CalcNormals());
                    unsafe
                    {
                        LinePosition tempNorm = new LinePosition();
                        var ptr = (LinePosition*)mTempPosition.UnsafeAddressAt(i).ToPointer();
                        ptr->CalcNormals(ref tempNorm);
                        mTempNormal.Add(tempNorm);
                    }
                }

                dpDesc.StartIndex = 0xFFFFFFFF;
                if (halfWidth == 0)
                {
                    dpDesc.PrimitiveType = EPrimitiveType.EPT_LineList;
                    dpDesc.NumPrimitives = (UInt32)(mTempPosition.Count * 3 * 3);
                }
                else
                {
                    dpDesc.NumPrimitives = (UInt32)(mTempPosition.Count * 3 * 2);
                }
                GeomMesh.SetAtom(0, 0, ref dpDesc);

                unsafe
                {
                    GeomMesh.SetGeomtryMeshStream(rc, EVertexSteamType.VST_Position, mTempPosition.GetBufferPtr(),
                            (UInt32)(sizeof(LinePosition) * mTempPosition.Count), (UInt32)sizeof(Vector3), 0);
                    GeomMesh.SetGeomtryMeshStream(rc, EVertexSteamType.VST_Normal, mTempNormal.GetBufferPtr(),
                            (UInt32)(sizeof(LinePosition) * mTempNormal.Count), (UInt32)sizeof(Vector3), 0);
                    GeomMesh.SetGeomtryMeshStream(rc, EVertexSteamType.VST_UV, mTempUV.GetBufferPtr(),
                            (UInt32)(sizeof(LineUV) * mTempUV.Count), (UInt32)sizeof(Vector2), 0);
                }
            }
            else
            {
                mTempPosition.Clear();
                mTempUV.Clear();
                LinesGen.BuildGraph(mTempPosition, mTempUV, halfWidth);

                mTempNormal.Clear();
                mTempNormal.SetGrowStep(mTempPosition.Count);
                for (int i = 0; i < mTempPosition.Count; i++)
                {
                    //mTempNormal.Add(mTempPosition[i].CalcNormals());
                    unsafe
                    {
                        LinePosition tempNorm = new LinePosition();
                        var ptr = (LinePosition*)mTempPosition.UnsafeAddressAt(i).ToPointer();
                        ptr->CalcNormals(ref tempNorm);
                        mTempNormal.Add(tempNorm);
                    }
                }

                dpDesc.StartIndex = 0xFFFFFFFF;
                if (halfWidth == 0)
                {
                    dpDesc.PrimitiveType = EPrimitiveType.EPT_LineList;
                    dpDesc.NumPrimitives = (UInt32)(mTempPosition.Count * 3);
                }
                else
                {
                    dpDesc.NumPrimitives = (UInt32)(mTempPosition.Count * 2);
                }
                GeomMesh.SetAtom(0, 0, ref dpDesc);

                unsafe
                {
                    GeomMesh.SetGeomtryMeshStream(rc, EVertexSteamType.VST_Position, mTempPosition.GetBufferPtr(),
                            (UInt32)(sizeof(LinePosition) * mTempPosition.Count), (UInt32)sizeof(Vector3), 0);
                    GeomMesh.SetGeomtryMeshStream(rc, EVertexSteamType.VST_Normal, mTempNormal.GetBufferPtr(),
                               (UInt32)(sizeof(LinePosition) * mTempNormal.Count), (UInt32)sizeof(Vector3), 0);
                    GeomMesh.SetGeomtryMeshStream(rc, EVertexSteamType.VST_UV, mTempUV.GetBufferPtr(),
                            (UInt32)(sizeof(LineUV) * mTempUV.Count), (UInt32)sizeof(Vector2), 0);
                }
            }

            GeomMesh.AABB = LinesGen.AABB;
            GeomMesh.GeometryMesh.Dirty = true;
        }
    }

    public class McLinesGen
    {
        private Vector3 mStart;
        public Vector3 Start
        {
            get
            {
                return mStart;
            }
            set
            {
                mStart = value;
                AABB.InitEmptyBox();
                AABB.Merge(ref mStart);
                AABB.Merge(ref mNextTarget);
            }
        }
        private Vector3 mNextTarget;
        public Vector3 NextTarget
        {
            get { return mNextTarget; }
            set
            {
                mNextTarget = value;

                AABB.InitEmptyBox();
                AABB.Merge(ref mStart);
                AABB.Merge(ref mNextTarget);
            }
        }
        public BoundingBox AABB;

        public virtual void NextStartPoint(out Vector3 target)
        {
            target = Start;
        }
        public virtual bool NextEndPoint(out Vector3 target)
        {
            target = NextTarget;
            return false;
        }
        public void BuildGraph(Support.NativeList<GeometryLinePosition> position, Support.NativeList<GeometryLineUV> uv, float halfWidth)
        {
            BeforeGraph(position);

            var rectUV = new GeometryLineUV();
            rectUV.SetUV();

            Vector3 cur, tar;
            bool drawing = true;
            float remainLength = GetSegment(position);
            bool isContinued = false;
            do
            {
                var normal = GetNormalDir();
                NextStartPoint(out cur);
                isContinued = NextEndPoint(out tar); //NextStartPoint(out tar);
                float dist = Vector3.Distance(ref cur, ref tar);
                var dir = tar - cur;
                dir.Normalize();
                Vector3 extDir = Vector3.Cross(dir, normal);
                extDir.Normalize();
                while (dist >= remainLength)
                {
                    if (drawing)
                    {
                        var end = cur + dir * remainLength;
                        var l = new GeometryLinePosition();
                        l.SetLine(ref cur, ref end, ref extDir, halfWidth);
                        //position.Add(l);
                        //uv.Add(rectUV);
                        unsafe
                        {
                            position.Add(l);
                            uv.Add(rectUV);
                        }

                        drawing = false;
                        dist -= remainLength;
                        remainLength = GetInterval(position);

                        cur = end;
                    }
                    else
                    {
                        drawing = true;
                        dist -= remainLength;
                        cur = cur + dir * remainLength;
                        remainLength = GetSegment(position);

                        extDir = Vector3.Cross(dir, normal);
                        extDir.Normalize();
                    }
                }

                if (drawing)
                {
                    var end = cur + dir * dist;
                    var l = new GeometryLinePosition();
                    l.SetLine(ref cur, ref end, ref extDir, halfWidth);
                    remainLength = remainLength - dist;
                    //position.Add(l);
                    //uv.Add(rectUV);
                    unsafe
                    {
                        position.Add(l);
                        uv.Add(rectUV);
                    }
                    dist = 0;
                }
                else
                {
                    remainLength = remainLength - dist;
                    dist = 0;
                }
            } while (isContinued);
        }
        public void BuildGraph(Support.NativeList<LinePosition> position, Support.NativeList<LineUV> uv, float halfWidth)
        {
            BeforeGraph(position);

            LineUV rectUV = new LineUV();
            rectUV.SetUV();

            Vector3 cur, tar;
            bool drawing = true;
            float remainLength = GetSegment(position);
            bool isContinued = false;
            do
            {
                var normal = GetNormalDir();
                NextStartPoint(out cur);
                isContinued = NextEndPoint(out tar); //NextStartPoint(out tar);
                float dist = Vector3.Distance(ref cur, ref tar);
                var dir = tar - cur;
                dir.Normalize();
                Vector3 extDir = Vector3.Cross(dir, normal);
                extDir.Normalize();
                while (dist >= remainLength)
                {
                    if (drawing)
                    {
                        var end = cur + dir * remainLength;
                        var l = new LinePosition();
                        l.SetLine(ref cur, ref end, ref extDir, halfWidth);
                        //position.Add(l);
                        //uv.Add(rectUV);
                        unsafe
                        {
                            position.Add(l);
                            uv.Add(rectUV);
                        }


                        drawing = false;
                        dist -= remainLength;
                        remainLength = GetInterval(position);

                        cur = end;
                    }
                    else
                    {
                        drawing = true;
                        dist -= remainLength;
                        cur = cur + dir * remainLength;
                        remainLength = GetSegment(position);

                        extDir = Vector3.Cross(dir, normal);
                        extDir.Normalize();
                    }
                }

                if (drawing)
                {
                    var end = cur + dir * dist;
                    var l = new LinePosition();
                    l.SetLine(ref cur, ref end, ref extDir, halfWidth);
                    remainLength = remainLength - dist;
                    //position.Add(l);
                    //uv.Add(rectUV);
                    unsafe
                    {
                        position.Add(l);
                        uv.Add(rectUV);
                    }
                    dist = 0;
                }
                else
                {
                    remainLength = remainLength - dist;
                    dist = 0;
                }
            } while (isContinued);
        }
        public float Interval
        {
            get;
            set;
        } = 1;
        public float Segement
        {
            get;
            set;
        } = 1;
        public Vector3 NormalDir = -Vector3.UnitZ;
        protected int Step;
        public virtual float GetInterval(Support.NativeList<LinePosition> lines)
        {
            return Interval;
        }
        public virtual float GetInterval(Support.NativeList<GeometryLinePosition> lines)
        {
            return Interval;
        }
        public virtual float GetSegment(Support.NativeList<LinePosition> lines)
        {
            return Segement;
        }
        public virtual float GetSegment(Support.NativeList<GeometryLinePosition> lines)
        {
            return Segement;
        }
        public virtual Vector3 GetNormalDir()
        {
            return NormalDir;
        }
        public virtual void BeforeGraph(Support.NativeList<LinePosition> lines)
        {
            Step = 0;
        }
        public virtual void BeforeGraph(Support.NativeList<GeometryLinePosition> lines)
        {
            Step = 0;
        }
    }

    public class McRectangleGen : McLinesGen
    {
        private Vector3[] Points = new Vector3[4];
        public void SetRect(Vector3 start, float width, float height)
        {
            Points[0] = start;
            Points[1] = start + Vector3.UnitX * width;
            Points[2] = start + Vector3.UnitX * width - Vector3.UnitY * height;
            Points[3] = start - Vector3.UnitY * height;

            AABB.InitEmptyBox();
            AABB.Merge(ref Points[0]);
            AABB.Merge(ref Points[1]);
            AABB.Merge(ref Points[2]);
            AABB.Merge(ref Points[3]);
        }
        public override void BeforeGraph(Support.NativeList<LinePosition> lines)
        {
            Step = 0;
            Segment = 0;
        }
        public override void BeforeGraph(Support.NativeList<GeometryLinePosition> lines)
        {
            Step = 0;
            Segement = 0;
        }
        public override void NextStartPoint(out Vector3 target)
        {
            target = Points[Step];
        }
        public override bool NextEndPoint(out Vector3 target)
        {
            Step++;
            if (Step < Points.Length)
            {
                target = Points[Step];
                return true;
            }
            else
            {
                target = Points[0];
                return false;
            }
        }
        private int Segment = 0;
        public override Vector3 GetNormalDir()
        {
            NormalDir.Y = (float)System.Math.Sin((float)(0.2F * Segment));
            Segment++;
            return NormalDir;
        }
    }

    public class McCircleGen : McLinesGen
    {
        private Vector3[] Points;
        public void SetCircle(Vector3 center, float radius, int segments)
        {
            AABB.InitEmptyBox();

            Points = new Vector3[segments + 1];
            float step = (float)System.Math.PI * 2 / segments;
            var startPoint = Vector3.UnitX * radius;
            for (int i = 0; i < segments; i++)
            {
                var rotator = Matrix.RotationAxis(GetNormalDir(), step * i);
                Points[i] = center + Vector3.TransformCoordinate(startPoint, rotator);

                AABB.Merge(ref Points[i]);
            }
            //Points[segments] = center + startPoint;
        }
        public override void BeforeGraph(Support.NativeList<LinePosition> lines)
        {
            Step = 0;
        }
        public override void BeforeGraph(Support.NativeList<GeometryLinePosition> lines)
        {
            Step = 0;
        }
        public override void NextStartPoint(out Vector3 target)
        {
            target = Points[Step];
        }
        public override bool NextEndPoint(out Vector3 target)
        {
            Step++;
            if (Step < Points.Length)
            {
                target = Points[Step];
                return true;
            }
            else
            {
                target = Points[0];
                return false;
            }
        }
    }

    public class McBoxGen : McLinesGen
    {
        private Vector3[] Points;
        Vector3 mCenter = Vector3.Zero;
        public void SetBox(Vector3 center, float xSize, float ySize, float zSize)
        {
            var bb = new BoundingBox(new Vector3(center.X - xSize * 0.5f, center.Y - ySize * 0.5f, center.Z - zSize * 0.5f),
                                     new Vector3(center.X + xSize * 0.5f, center.Y + ySize * 0.5f, center.Z + zSize * 0.5f));

            this.SetBoundBox(bb);

            AABB = bb;
        }

        public void SetBoundBox(BoundingBox bb)
        {
            /// y  z
            /// | /
            /// |/
            /// -----x
            ///    0------1
            ///   /|     /|
            ///  / |3   / 2
            /// 4------5  /
            /// | /    | /
            /// |/     |/
            /// 7------6
            var corners = bb.GetCorners();
            Points = new Vector3[8 * 3];
            Points[0] = corners[0];
            Points[1] = corners[1];

            Points[2] = corners[0];
            Points[3] = corners[4];

            Points[4] = corners[0];
            Points[5] = corners[3];

            Points[6] = corners[1];
            Points[7] = corners[5];

            Points[8] = corners[1];
            Points[9] = corners[2];

            Points[10] = corners[2];
            Points[11] = corners[3];

            Points[12] = corners[2];
            Points[13] = corners[6];

            Points[14] = corners[3];
            Points[15] = corners[7];

            Points[16] = corners[4];
            Points[17] = corners[5];

            Points[18] = corners[4];
            Points[19] = corners[7];

            Points[20] = corners[5];
            Points[21] = corners[6];

            Points[22] = corners[6];
            Points[23] = corners[7];
        }

        public override void BeforeGraph(Support.NativeList<LinePosition> lines)
        {
            Step = 0;
        }
        public override void BeforeGraph(Support.NativeList<GeometryLinePosition> lines)
        {
            Step = 0;
        }

        public override void NextStartPoint(out Vector3 target)
        {
            target = Points[Step];
        }
        public override bool NextEndPoint(out Vector3 target)
        {
            target = Points[Step + 1];

            Step += 2;
            if (Step >= Points.Length)
                return false;
            return true;
        }

        public override Vector3 GetNormalDir()
        {
            var pos1 = Points[Step];
            var pos2 = Points[Step + 1];

            var posCenter = (pos1 + pos2) * 0.5f;
            var retVal = posCenter - mCenter;
            retVal.Normalize();
            return retVal;
        }
    }

    public class McMulLinesGen : McLinesGen
    {
        //private Vector3[] Points;
        protected Support.NativeList<Vector3> Points = new Support.NativeList<Vector3>();

        public void SetFloatPoints(float[] data, uint size)
        {
            if (size % 3 != 0)
                return;

            AABB.InitEmptyBox();
            unsafe
            {
                fixed (float* p = &data[0])
                {
                    Points.UnsafeSetDatas((IntPtr)p, (int)size / 3);

                    for (int i = 0; i < size / 3; i++)
                    {
                        AABB.Merge(ref ((Vector3*)p)[i]);
                    }
                }
            }
        }

        public void SetVector3Points(Vector3[] data)
        {
            AABB.InitEmptyBox();
            for (int i = 0; i < data.Length; i++)
            {
                AABB.Merge(ref data[i]);
            }
            unsafe
            {
                fixed (Vector3* p = &data[0])
                {
                    Points.UnsafeSetDatas((IntPtr)p, data.Length);
                }
            }
        }

        public unsafe void UnsafeSetVector3Points(Vector3* ptr, int count)
        {
            Points.UnsafeSetDatas((IntPtr)ptr, count);
        }

        public void SetBezier3D(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, int count)
        {
            EngineNS.Bezier3D bezier3d = new EngineNS.Bezier3D(v1, v2, v3, v4);

            Points.Clear();
            AABB.InitEmptyBox();
            float times = 1.0f / (float)count;
            for (int i = 0; i < count; i++)
            {
                var pt = bezier3d.GetValue(Math.Min(times * i, 1.0f));
                unsafe
                {
                    Points.Add(pt);
                    AABB.Merge(ref pt);
                }
            }
        }
        public override void BeforeGraph(Support.NativeList<LinePosition> lines)
        {
            Step = 0;
        }
        public override void BeforeGraph(Support.NativeList<GeometryLinePosition> lines)
        {
            Step = 0;
        }

        public override void NextStartPoint(out Vector3 target)
        {
            unsafe
            {
                target = *((Vector3*)Points.UnsafeAddressAt(Step).ToPointer());
            }
        }
        public override bool NextEndPoint(out Vector3 target)
        {
            Step++;
            if (Step < Points.Count - 1)
            {
                unsafe
                {
                    target = *((Vector3*)Points.UnsafeAddressAt(Step).ToPointer());
                }
                return true;
            }
            else
            {
                unsafe
                {
                    target = *((Vector3*)Points.UnsafeAddressAt(Points.Count - 1).ToPointer());
                }
                return false;
            }
        }
    }

    public class McMulSegmentsGen : McMulLinesGen
    {
        public override bool NextEndPoint(out Vector3 target)
        {
            Step++;
            if (Step < Points.Count - 1)
            {
                unsafe
                {
                    target = *((Vector3*)Points.UnsafeAddressAt(Step).ToPointer());
                }
                Step++;
                return true;
            }
            else
            {
                unsafe
                {
                    target = *((Vector3*)Points.UnsafeAddressAt(Points.Count - 1).ToPointer());
                }
                Step++;
                return false;
            }
        }
    }

    public class McFrustumGen : McLinesGen
    {
        /*
        6-----7
        |\   /|
        | 2-3 |
        | |+| |
        | 0-1 |
        |/   \|
        4-----5
        */
        private Vector3[] Points = new Vector3[24];
        private Vector3 mCenter;
        public void SetFrustum(EngineNS.Vector3[] vecs)
        {
            if (vecs.Length != 8)
                return;

            mCenter = Vector3.Zero;
            foreach (var vec in vecs)
            {
                mCenter += vec;
            }
            mCenter = mCenter / 8;

            Points[0] = vecs[0];
            Points[1] = vecs[1];

            Points[2] = vecs[0];
            Points[3] = vecs[2];

            Points[4] = vecs[2];
            Points[5] = vecs[3];

            Points[6] = vecs[1];
            Points[7] = vecs[3];

            Points[8] = vecs[0];
            Points[9] = vecs[4];

            Points[10] = vecs[1];
            Points[11] = vecs[5];

            Points[12] = vecs[2];
            Points[13] = vecs[6];

            Points[14] = vecs[3];
            Points[15] = vecs[7];

            Points[16] = vecs[4];
            Points[17] = vecs[5];

            Points[18] = vecs[4];
            Points[19] = vecs[6];

            Points[20] = vecs[5];
            Points[21] = vecs[7];

            Points[22] = vecs[6];
            Points[23] = vecs[7];

            AABB.InitEmptyBox();
            for (int i = 0; i < Points.Length; i++)
            {
                AABB.Merge(ref Points[i]);
            }
        }
        public override void BeforeGraph(Support.NativeList<LinePosition> lines)
        {
            Step = 0;
        }
        public override void BeforeGraph(Support.NativeList<GeometryLinePosition> lines)
        {
            Step = 0;
        }

        public override void NextStartPoint(out Vector3 target)
        {
            target = Points[Step];
        }
        public override bool NextEndPoint(out Vector3 target)
        {
            target = Points[Step + 1];

            Step += 2;
            if (Step >= Points.Length)
                return false;
            return true;
        }

        public override Vector3 GetNormalDir()
        {
            var pos1 = Points[Step];
            var pos2 = Points[Step + 1];

            var posCenter = (pos1 + pos2) * 0.5f;
            var retVal = posCenter - mCenter;
            retVal.Normalize();
            return retVal;
        }
    }

    public class GraphLinesHelper : BrickDescriptor
    {
        public override async System.Threading.Tasks.Task DoTest()
        {
            await base.DoTest();
        }
        public static async System.Threading.Tasks.Task<GraphLines> Init(Vector3 start)
        {
            var mGraph = new GraphLines();

            int slt = 0;
            switch (slt)
            {
                case 0:
                    {
                        var rect = new McLinesGen();
                        rect.Interval = 2.0f;
                        rect.Segement = 2.0f;
                        rect.Start = start;
                        rect.NextTarget = start + Vector3.UnitX * 100;
                        mGraph.LinesGen = rect;
                    }
                    break;
                case 1:
                    {
                        var rect = new McRectangleGen();
                        rect.Interval = 5.0f;
                        rect.Segement = 10.0f;
                        rect.NextTarget = new Vector3(300, 300, 0);
                        rect.SetRect(start, 300, 200);
                        mGraph.LinesGen = rect;
                    }
                    break;
                case 2:
                    {
                        var rect = new McCircleGen();
                        rect.Interval = 10.0f;
                        rect.Segement = 20.0f;
                        rect.SetCircle(start, 150, 30);
                        mGraph.LinesGen = rect;
                    }
                    break;
            }

            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(
                CEngine.Instance.RenderContext,
                //RName.GetRName("material/defaultmaterial.instmtl")); 
                RName.GetRName("material/rotator.instmtl"));
            await mGraph.Init(mtl, 0.3f);

            mGraph.UpdateGeomMesh(CEngine.Instance.RenderContext);
            return mGraph;
        }
    }
}
