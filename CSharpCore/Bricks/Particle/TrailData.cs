using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{

    public class TrailDataControl
    {
        static Byte4 DefaultColor
        {
            get
            {
                Byte4 color = new Byte4();
                color.X = 255;
                color.X = 255;
                color.X = 255;
                color.X = 255;
                return color;
            }
        }

        [Editor.DisplayParamName("飘带的方向(Vector3)")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Direction { get; set; } = Vector3.UnitZ;
        [Editor.DisplayParamName("飘带的宽度(float)")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Width { get; set; } = 2f;

        [Editor.DisplayParamName("飘带的生命时长(float)")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Life { get; set; } = 5f;

        [Editor.DisplayParamName("飘带的之间最短距离(float)")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float MinVertextDistance { get; set; } = 1f;

        bool first = true;

        Support.NativeList<Vector3> Positions = new Support.NativeList<Vector3>();
        Support.NativeList<int> VertexIndexes = new Support.NativeList<int>();
        Support.NativeList<Byte4> Colors = new Support.NativeList<Byte4>();
        Support.NativeList<float> LifeTicks = new Support.NativeList<float>();
        Support.NativeList<Vector2> UVs = new Support.NativeList<Vector2>();
        Support.NativeList<Vector4> Tangents = new Support.NativeList<Vector4>();
        Support.NativeList<Vector3> Normals = new Support.NativeList<Vector3>();
        Vector3 PrePosition;

        uint ComposeCount = 0;

        public delegate void DealTrailDataColorAndWidthDelegate(float life, float lifetick, out Color4 color, out float width);

        public TrailDataControl()
        {
            //TrailDatas.Add(new TrailData(0.1f, Width, Color.Blue));
            //TrailDatas.Add(new TrailData(0.3f, Width, Color.White));
            //TrailDatas.Add(new TrailData(0.5f, Width * 0.8f, Color.Green));
            //TrailDatas.Add(new TrailData(0.7f, Width, Color.FromArgb(125, 255, 0, 0)));
            //TrailDatas.Add(new TrailData(1.0f, Width, Color.FromArgb(20, 255, 255, 0)));
            ////TrailDatas.Add(new TrailData(0.5f, 4, Color.Blue));
            ////TrailDatas.Add(new TrailData(0.8f, 1, Color.Yellow));
        }

        public void Update(float e, DealTrailDataColorAndWidthDelegate DealTrailDataColorAndWidth)
        {
            float width = Width;
            Color4 color = new Color4(1, 1, 1, 1);
            Byte4 resultcolor = new Byte4();
            for (int i = 0; i < Positions.Count; i++)
            {
                LifeTicks[i] += e;
                var lifetick = LifeTicks[i];
                if (lifetick > Life && Positions.Count > 4)
                {
                    Positions.RemoveAt(i);
                    Colors.RemoveAt(i);
                    UVs.RemoveAt(i);
                    LifeTicks.RemoveAt(i);
                    Tangents.RemoveAt(i);
                    Normals.RemoveAt(i);
                    i--;
                    continue;
                }

                //width = width;
                //UV...
                if (i % 2 == 0)
                {

                    UVs[i] = new Vector2(0, 1f - (float)i / (float)Positions.Count);

                    if (DealTrailDataColorAndWidth != null)
                    {
                        DealTrailDataColorAndWidth?.Invoke(Life, LifeTicks[i], out color, out width);
                        //if (width != Width)
                        {
                            var pos = (Positions[i] + Positions[i + 1]) / 2.0f;
                            var dir1 = Vector3.Normalize(Positions[i] - pos);
                            var dir2 = Vector3.Normalize(Positions[i + 1] - pos);

                            Positions[i] = pos + dir1 * (0.5f * width);
                            Positions[i + 1] = pos + dir2 * (0.5f * width);
                        }

                        resultcolor.X = (byte)(color.Red * 255f);
                        resultcolor.Y = (byte)(color.Green * 255f);
                        resultcolor.Z = (byte)(color.Blue * 255f);

                        resultcolor.W = (byte)(color.Alpha * 255f);
                        Colors[i] = resultcolor;
                    }

                }
                else
                {
                    UVs[i] = new Vector2(1, 1f - (float)(i - 1) / (float)Positions.Count);
                    if (DealTrailDataColorAndWidth != null)
                    {
                        Colors[i] = resultcolor;
                    }
                }

            }
        }

        public bool IsVaild()
        {
            return Positions.Count > 4;
        }

        public void CreateTrailData(ref Vector3 ppos, ref Quaternion prot, ref Vector3 pscale, ref Matrix worldmatrix, bool isbind)
        {
            if (first)
            {
                PrePosition = ppos;
                first = false;
                return;
            }
            var distance = Vector3.Distance(ref ppos, ref PrePosition);
            if (distance < MinVertextDistance)
                return;

            PrePosition = ppos;
            Vector3 p2 = Direction * (-Width) * 0.5f;
            Vector3 p3 = Direction * Width * 0.5f;
            if (isbind)
            {
                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;

                worldmatrix.Decompose(out scale, out rotation, out translation);
                p2 = pscale * p2;
                p3 = pscale * p3;

                if (prot.IsValid)
                {
                    p2 = prot * p2;
                    p3 = prot * p3;
                }

                p2 = scale * p2;
                p3 = scale * p3;

                if (rotation.IsValid)
                {
                    p2 = rotation * p2;
                    p3 = rotation * p3;
                }

                p2 = ppos + p2;
                p3 = ppos + p3;
            }
            else
            {
                Vector4 temp = new Vector4();
                p2 = pscale * p2;
                p3 = pscale * p3;

                if (prot.IsValid)
                {
                    p2 = prot * p2;
                    p3 = prot * p3;
                }


                Vector3.Transform(ref p2, ref worldmatrix, out temp);
                p2.X = temp.X + ppos.X;
                p2.Y = temp.Y + ppos.Y;
                p2.Z = temp.Z + ppos.Z;

                Vector3.Transform(ref p3, ref worldmatrix, out temp);
                p3.X = temp.X + ppos.X;
                p3.Y = temp.Y + ppos.Y;
                p3.Z = temp.Z + ppos.Z;
            }

            Positions.Add(p2);
            Positions.Add(p3);

            Colors.Add(DefaultColor);
            Colors.Add(DefaultColor);

            UVs.Add(new Vector2(0, 0));
            UVs.Add(new Vector2(0, 0));

            Tangents.Add(Vector4.UnitX);
            Tangents.Add(Vector4.UnitX);
            Normals.Add(new Vector3(0, 0, -1));
            Normals.Add(new Vector3(0, 0, -1));

            LifeTicks.Add(0);
            LifeTicks.Add(0);

            //CalculateVertexindexes();
        }

        public void CalculateVertexindexes(int StartIndex)
        {
            //处理索引
            if (Positions.Count < 4)
                return;

            VertexIndexes.Clear((uint)((Positions.Count - 2) * 3));
            for (int i = 2; i < Positions.Count; i += 2)
            {
                VertexIndexes.Add(StartIndex + i - 2);
                VertexIndexes.Add(StartIndex + i - 1);
                VertexIndexes.Add(StartIndex + i);

                VertexIndexes.Add(StartIndex + i + 1);
                VertexIndexes.Add(StartIndex + i);
                VertexIndexes.Add(StartIndex + i - 1);
            }

        }

        int PrePositionCount = 0;
        int PreIndexesCount = 0;
        public void BindBuffs(CRenderContext rc, CGfxMeshPrimitives result)
        {
            if (Positions.Count < 4)
                return;

            CCommandList cmd = rc.ImmCommandList;

            var dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = (uint)Positions.Count - (ComposeCount * 2);
            result.SetAtom(0, 0, ref dpDesc);

            bool needcreate = Positions.Count > PrePositionCount || VertexIndexes.Count > PreIndexesCount;
            if (needcreate)
            {
                PrePositionCount = Positions.Count;
            }

            UInt32 resourceSize = 0;
            if (needcreate == false)
            {
                var mesh = result.GeometryMesh;
                UInt32 ByteWidth = 0;
                //mesh.Cleanup();
                {
                    var vb = mesh.GetVertexBuffer(EVertexSteamType.VST_Position);

                    unsafe
                    {
                        ByteWidth = (UInt32)(sizeof(Vector3) * Positions.Count);
                        vb.UpdateBuffData(cmd, Positions.UnsafeAddressAt(0), ByteWidth);
                        resourceSize += ByteWidth;
                    }
                }

                {
                    var vb = mesh.GetVertexBuffer(EVertexSteamType.VST_Tangent);

                    unsafe
                    {
                        ByteWidth = (UInt32)(sizeof(Vector4) * Tangents.Count);
                        vb.UpdateBuffData(cmd, Tangents.UnsafeAddressAt(0), ByteWidth);
                        resourceSize += ByteWidth;
                    }

                }

                {
                    var vb = mesh.GetVertexBuffer(EVertexSteamType.VST_Normal);

                    unsafe
                    {
                        ByteWidth = (UInt32)(sizeof(Vector3) * Normals.Count);
                        vb.UpdateBuffData(cmd, Normals.UnsafeAddressAt(0), ByteWidth);
                        resourceSize += ByteWidth;
                    }

                }

                {
                    var vb = mesh.GetVertexBuffer(EVertexSteamType.VST_UV);

                    unsafe
                    {
                        ByteWidth = (UInt32)(sizeof(Vector2) * UVs.Count);
                        vb.UpdateBuffData(cmd, UVs.UnsafeAddressAt(0), ByteWidth);
                        resourceSize += ByteWidth;
                    }
                }

                {
                    var vb = mesh.GetVertexBuffer(EVertexSteamType.VST_Color);

                    unsafe
                    {
                        ByteWidth = (UInt32)(sizeof(Byte4) * Colors.Count);
                        vb.UpdateBuffData(cmd, Colors.UnsafeAddressAt(0), ByteWidth);
                        resourceSize += ByteWidth;
                    }
                }

                {
                    //if (VertexIndexes.Count == PreIndexesCount)
                    //{
                    var ib = mesh.GetIndexBuffer();

                    unsafe
                    {
                        ByteWidth = (UInt32)(sizeof(UInt32) * VertexIndexes.Count);
                        ib.UpdateBuffData(cmd, VertexIndexes.UnsafeAddressAt(0), ByteWidth);
                        resourceSize += ByteWidth;
                    }
                    //}
                    //else
                    //{
                    //    var ibDesc = new CIndexBufferDesc();
                    //    ibDesc.CPUAccess = (UInt32)ECpuAccess.CAS_WRITE;
                    //    ibDesc.InitData = VertexIndexes.UnsafeAddressAt(0);
                    //    ibDesc.Type = EIndexBufferType.IBT_Int32;
                    //    ibDesc.ByteWidth = (UInt32)(sizeof(UInt32) * VertexIndexes.Count);
                    //    var ib = rc.CreateIndexBuffer(ibDesc);
                    //    mesh.BindIndexBuffer(ib);
                    //    resourceSize += ibDesc.ByteWidth;

                    //    PreIndexesCount = VertexIndexes.Count;
                    //}

                }

                result.ResourceState.ResourceSize = (UInt32)(resourceSize);
                result.ResourceState.StreamState = EStreamingState.SS_Valid;
                result.ResourceState.KeepValid = true;
                result.GeometryMesh.Dirty = true;
                return;
            }

            //result.GeometryMesh.Cleanup();
            unsafe
            {
                var vbDesc = new CVertexBufferDesc();
                vbDesc.CPUAccess = (UInt32)ECpuAccess.CAS_WRITE;
                var mesh = result.GeometryMesh;
                //mesh.Cleanup();
                {
                    vbDesc.InitData = Positions.UnsafeAddressAt(0);
                    vbDesc.Stride = (UInt32)sizeof(Vector3);
                    vbDesc.ByteWidth = (UInt32)(sizeof(Vector3) * Positions.Count);
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Position, vb);
                    resourceSize += vbDesc.ByteWidth;
                }

                {
                    vbDesc.InitData = Tangents.UnsafeAddressAt(0);
                    vbDesc.Stride = (UInt32)sizeof(Vector4);
                    vbDesc.ByteWidth = (UInt32)(sizeof(Vector4) * Tangents.Count);
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Tangent, vb);
                    resourceSize += vbDesc.ByteWidth;

                }

                {
                    vbDesc.InitData = Normals.UnsafeAddressAt(0);
                    vbDesc.Stride = (UInt32)sizeof(Vector3);
                    vbDesc.ByteWidth = (UInt32)(sizeof(Vector3) * Normals.Count);
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Normal, vb);
                    resourceSize += vbDesc.ByteWidth;

                }

                {
                    vbDesc.InitData = UVs.UnsafeAddressAt(0);
                    vbDesc.Stride = (UInt32)sizeof(Vector2);
                    vbDesc.ByteWidth = (UInt32)(sizeof(Vector2) * UVs.Count);
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_UV, vb);
                    resourceSize += vbDesc.ByteWidth;
                }

                {
                    vbDesc.InitData = Colors.UnsafeAddressAt(0);
                    vbDesc.Stride = (UInt32)sizeof(Byte4);
                    vbDesc.ByteWidth = (UInt32)(sizeof(Byte4) * Colors.Count);
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Color, vb);
                    resourceSize += vbDesc.ByteWidth;
                }

                {
                    var ibDesc = new CIndexBufferDesc();
                    ibDesc.CPUAccess = (UInt32)ECpuAccess.CAS_WRITE;
                    ibDesc.InitData = VertexIndexes.UnsafeAddressAt(0);
                    ibDesc.Type = EIndexBufferType.IBT_Int32;
                    ibDesc.ByteWidth = (UInt32)(sizeof(UInt32) * VertexIndexes.Count);
                    var ib = rc.CreateIndexBuffer(ibDesc);
                    mesh.BindIndexBuffer(ib);
                    resourceSize += ibDesc.ByteWidth;
                    PreIndexesCount = VertexIndexes.Count;

                }
                result.ResourceState.ResourceSize = (UInt32)(resourceSize);
                mesh.Dirty = true;
            }
            result.ResourceState.StreamState = EStreamingState.SS_Valid;
            result.ResourceState.KeepValid = true;
        }

        public void Clear()
        {
            Positions.Clear((uint)Positions.Count);
            VertexIndexes.Clear((uint)VertexIndexes.Count);
            Colors.Clear((uint)Colors.Count);
            LifeTicks.Clear((uint)LifeTicks.Count);
            UVs.Clear((uint)UVs.Count);

            Tangents.Clear((uint)Tangents.Count);
            Normals.Clear((uint)Normals.Count);

            ComposeCount = 0;
        }

        public void Compose(TrailDataControl data)
        {
            if (data.Positions.Count < 4)
                return;

            Positions.Append(data.Positions);
            VertexIndexes.Append(data.VertexIndexes);
            Colors.Append(data.Colors);
            UVs.Append(data.UVs);
            Tangents.Append(data.Tangents);
            Normals.Append(data.Normals);
            ComposeCount++;
        }

        public int GetPositionCount()
        {
            return Positions.Count;
        }
    }

    public class TrailData
    {
        public float Offset = 0f;
        public float Width = 1f;
        public Color Color;
        public TrailData()
        {
        }

        public TrailData(float offset, float width, Color color)
        {
            Offset = offset;
            Width = width;
            Color = color;
        }

    }
}
