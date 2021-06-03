using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public class CMeshDataProvider : AuxPtrType<IMeshDataProvider>
    {
        public CMeshDataProvider()
        {
            mCoreObject = IMeshDataProvider.CreateInstance();
        }
        public CMeshPrimitives ToMesh()
        {
            unsafe
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                var result = new CMeshPrimitives();
                result.mCoreObject.Init(rc.mCoreObject.CppPointer, "", 1);
                mCoreObject.ToMesh(rc.mCoreObject.CppPointer, result.mCoreObject.CppPointer);
                return result;
            }
        }
        [Flags]
        public enum EBoxFace : byte
        {
            Front = 1,
            Back = (1 << 1),
            Left = (1 << 2),
            Right = (1 << 3),
            Top = (1 << 4),
            Bottom = (1 << 5),
            All = Front | Back | Left | Right | Top | Bottom,
            None = 0,
        }
        public static unsafe CMeshDataProvider MakeBox(float x, float y, float z, float xSize, float ySize, float zSize,
            EBoxFace faceFlags = EBoxFace.All)
        {
            var meshBuilder = new Graphics.Mesh.CMeshDataProvider();
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)EVertexSteamType.VST_Position) | 
                (1 << (int)EVertexSteamType.VST_Normal) | 
                (1 << (int)EVertexSteamType.VST_Color) | 
                (1 << (int)EVertexSteamType.VST_UV));
            builder.Init(streams, EIndexBufferType.IBT_Int16, 1);

            var aabb = new BoundingBox(x, y, z, x + xSize, y + ySize, z + zSize);
            builder.SetAABB(ref aabb);

            var dpDesc = new DrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = 0;

            var pPos = stackalloc Vector3[24];
            //创建Position
            {
                var vbPos = new Vector3[8];
                vbPos[0].SetValue(x, y, z);
                vbPos[1].SetValue(x, y + ySize, z);
                vbPos[2].SetValue(x + xSize, y + ySize, z);
                vbPos[3].SetValue(x + xSize, y, z);
                vbPos[4].SetValue(x, y, z + zSize);
                vbPos[5].SetValue(x, y + ySize, z + zSize);
                vbPos[6].SetValue(x + xSize, y + ySize, z + zSize);
                vbPos[7].SetValue(x + xSize, y, z + zSize);
                // front side
                pPos[0] = vbPos[0]; pPos[1] = vbPos[1]; pPos[2] = vbPos[2]; pPos[3] = vbPos[3];
                // back side
                pPos[4] = vbPos[4]; pPos[5] = vbPos[5]; pPos[6] = vbPos[6]; pPos[7] = vbPos[7];
                // left side
                pPos[8] = vbPos[4]; pPos[9] = vbPos[5]; pPos[10] = vbPos[1]; pPos[11] = vbPos[0];
                // right side
                pPos[12] = vbPos[3]; pPos[13] = vbPos[2]; pPos[14] = vbPos[6]; pPos[15] = vbPos[7];
                // top side
                pPos[16] = vbPos[1]; pPos[17] = vbPos[5]; pPos[18] = vbPos[6]; pPos[19] = vbPos[2];
                // bottom side
                pPos[20] = vbPos[4]; pPos[21] = vbPos[0]; pPos[22] = vbPos[3]; pPos[23] = vbPos[7];
            }

            var vNor = stackalloc Vector3[24];
            //创建Normal
            {
                var vFront = new Vector3(0, 0, -1);
                var vBack = new Vector3(0, 0, 1);
                var vLeft = new Vector3(-1, 0, 0);
                var vRight = new Vector3(1, 0, 0);
                var vTop = new Vector3(0, 1, 0);
                var vBottom = new Vector3(0, -1, 0);
                vNor[0] = vFront; vNor[1] = vFront; vNor[2] = vFront; vNor[3] = vFront;
                vNor[4] = vBack; vNor[5] = vBack; vNor[6] = vBack; vNor[7] = vBack;
                vNor[8] = vLeft; vNor[9] = vLeft; vNor[10] = vLeft; vNor[11] = vLeft;
                vNor[12] = vRight; vNor[13] = vRight; vNor[14] = vRight; vNor[15] = vRight;
                vNor[16] = vTop; vNor[17] = vTop; vNor[18] = vTop; vNor[19] = vTop;
                vNor[20] = vBottom; vNor[21] = vBottom; vNor[22] = vBottom; vNor[23] = vBottom;
            }

            var vUV = stackalloc Vector2[24];
            //创建UV
            {
                // UV
                var vLB = new Vector2(0, 1);
                var vLT = new Vector2(0, 0);
                var vRT = new Vector2(1, 0);
                var vRB = new Vector2(1, 1);                
                vUV[0] = vLB; vUV[1] = vLT; vUV[2] = vRT; vUV[3] = vRB;
                vUV[4] = vRB; vUV[5] = vRT; vUV[6] = vLT; vUV[7] = vLB;
                vUV[8] = vLB; vUV[9] = vLT; vUV[10] = vRT; vUV[11] = vRB;
                vUV[12] = vLB; vUV[13] = vLT; vUV[14] = vRT; vUV[15] = vRB;
                vUV[16] = vLB; vUV[17] = vLT; vUV[18] = vRT; vUV[19] = vRB;
                vUV[20] = vLB; vUV[21] = vLT; vUV[22] = vRT; vUV[23] = vRB;
            }
            UInt32 curIndex = 0;
            // 索引
            UInt16[] pIndex = new UInt16[36];
            if ((faceFlags & EBoxFace.Front) != 0)
            {// front side
                pIndex[curIndex++] = 0; pIndex[curIndex++] = 1; pIndex[curIndex++] = 2;
                pIndex[curIndex++] = 0; pIndex[curIndex++] = 2; pIndex[curIndex++] = 3;
                dpDesc.NumPrimitives += 2;
            }
            if ((faceFlags & EBoxFace.Back) != 0)
            {
                // back side
                pIndex[curIndex++] = 4; pIndex[curIndex++] = 6; pIndex[curIndex++] = 5;
                pIndex[curIndex++] = 4; pIndex[curIndex++] = 7; pIndex[curIndex++] = 6;
                dpDesc.NumPrimitives += 2;
            }
            if ((faceFlags & EBoxFace.Left) != 0)
            {
                // left side
                pIndex[curIndex++] = 8; pIndex[curIndex++] = 9; pIndex[curIndex++] = 10;
                pIndex[curIndex++] = 8; pIndex[curIndex++] = 10; pIndex[curIndex++] = 11;
                dpDesc.NumPrimitives += 2;
            }
            if ((faceFlags & EBoxFace.Right) != 0)
            {
                // right side
                pIndex[curIndex++] = 12; pIndex[curIndex++] = 13; pIndex[curIndex++] = 14;
                pIndex[curIndex++] = 12; pIndex[curIndex++] = 14; pIndex[curIndex++] = 15;
                dpDesc.NumPrimitives += 2;
            }
            if ((faceFlags & EBoxFace.Top) != 0)
            {
                // top side
                pIndex[curIndex++] = 16; pIndex[curIndex++] = 17; pIndex[curIndex++] = 18;
                pIndex[curIndex++] = 16; pIndex[curIndex++] = 18; pIndex[curIndex++] = 19;
                dpDesc.NumPrimitives += 2;
            }
            if ((faceFlags & EBoxFace.Bottom) != 0)
            {
                // bottom side
                pIndex[curIndex++] = 20; pIndex[curIndex++] = 21; pIndex[curIndex++] = 22;
                pIndex[curIndex++] = 20; pIndex[curIndex++] = 22; pIndex[curIndex++] = 23;
                dpDesc.NumPrimitives += 2;
            }

            for (int i = 0; i < 24; i++)
            {
                builder.AddVertex(&pPos[i], &vNor[i], &vUV[i], 0xFFFFFFFF);
            }

            for (int i = 0; i < dpDesc.NumPrimitives; i++)
            {
                builder.AddTriangle(pIndex[i * 3], pIndex[i * 3 + 1], pIndex[i * 3 + 2]);
            }

            builder.PushAtomLOD(0, &dpDesc);
            return meshBuilder;
        }
        public static unsafe CMeshDataProvider MakeBoxWireframe(float x, float y, float z, float xSize, float ySize, float zSize, UInt32 color = 0xFFFFFFFF)
        {
            var meshBuilder = new Graphics.Mesh.CMeshDataProvider();
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)EVertexSteamType.VST_Position) |
                (1 << (int)EVertexSteamType.VST_Color));
            builder.Init(streams, EIndexBufferType.IBT_Int16, 1);

            var aabb = new BoundingBox(x, y, z, x + xSize, y + ySize, z + zSize);
            builder.SetAABB(ref aabb);

            var dpDesc = new DrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = 0;
            dpDesc.PrimitiveType = EPrimitiveType.EPT_LineList;

            var pPos = stackalloc Vector3[8];
            //创建Position
            {
                pPos[0].SetValue(x, y, z);
                pPos[1].SetValue(x, y + ySize, z);
                pPos[2].SetValue(x + xSize, y + ySize, z);
                pPos[3].SetValue(x + xSize, y, z);
                pPos[4].SetValue(x, y, z + zSize);
                pPos[5].SetValue(x, y + ySize, z + zSize);
                pPos[6].SetValue(x + xSize, y + ySize, z + zSize);
                pPos[7].SetValue(x + xSize, y, z + zSize);                
            }

            UInt32 curIndex = 0;
            // 索引
            UInt16[] pIndex = new UInt16[24];
            pIndex[curIndex++] = 0;
            pIndex[curIndex++] = 1;
            pIndex[curIndex++] = 1;
            pIndex[curIndex++] = 2;
            pIndex[curIndex++] = 2;
            pIndex[curIndex++] = 3;
            pIndex[curIndex++] = 3;
            pIndex[curIndex++] = 0;

            pIndex[curIndex++] = 4;
            pIndex[curIndex++] = 5;
            pIndex[curIndex++] = 5;
            pIndex[curIndex++] = 6;
            pIndex[curIndex++] = 6;
            pIndex[curIndex++] = 7;
            pIndex[curIndex++] = 7;
            pIndex[curIndex++] = 4;

            pIndex[curIndex++] = 0;
            pIndex[curIndex++] = 4;
            pIndex[curIndex++] = 1;
            pIndex[curIndex++] = 5;
            pIndex[curIndex++] = 2;
            pIndex[curIndex++] = 6;
            pIndex[curIndex++] = 3;
            pIndex[curIndex++] = 7;

            dpDesc.NumPrimitives = 12;

            for (int i = 0; i < 8; i++)
            {
                builder.AddVertex(ref pPos[i], ref Vector3.UnitY, ref Vector2.mUnitXY, color);
            }

            for (int i = 0; i < dpDesc.NumPrimitives; i++)
            {
                builder.AddLine(pIndex[i * 2], pIndex[i * 2 + 1]);
            }

            builder.PushAtomLOD(0, &dpDesc);
            return meshBuilder;
        }
        public static unsafe CMeshDataProvider MakeRect2D(float x, float y, float w, float h, float z, bool lh = true, bool flipUVWhenGL = false, ECpuAccess cpuAccess = (ECpuAccess)0)
        {
            var meshBuilder = new Graphics.Mesh.CMeshDataProvider();
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)EVertexSteamType.VST_Position) |
                (1 << (int)EVertexSteamType.VST_Normal) |
                (1 << (int)EVertexSteamType.VST_Color) |
                (1 << (int)EVertexSteamType.VST_UV));
            builder.Init(streams, EIndexBufferType.IBT_Int16, 1);

            var dpDesc = new DrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = 2;
            builder.PushAtomLOD(0, &dpDesc);

            unsafe
            {
                UInt16[] ibData = new UInt16[6];
                ibData[0] = 0;
                ibData[1] = 1;
                ibData[2] = 2;
                ibData[3] = 0;
                ibData[4] = 2;
                ibData[5] = 3;
                if (lh == false)
                {
                    ibData[1] = 2;
                    ibData[2] = 1;
                    ibData[4] = 3;
                    ibData[5] = 2;
                }

                Vector3[] vbPos = new Vector3[4];
                vbPos[0].SetValue(x, y, z);
                vbPos[1].SetValue(x + w, y, z);
                vbPos[2].SetValue(x + w, y + h, z);
                vbPos[3].SetValue(x, y + h, z);
                
                Vector2[] vbUV = new Vector2[4];
                vbUV[0].SetValue(0, 0);
                vbUV[1].SetValue(1, 0);
                vbUV[2].SetValue(1, 1);
                vbUV[3].SetValue(0, 1);
                if (lh == false && UEngine.Instance.GfxDevice.RenderContext.RHIType == ERHIType.RHT_D3D11)
                {//全屏幕用的-1，-1，2，2这里要换手系
                    vbUV[0].Y = 1 - vbUV[0].Y;
                    vbUV[1].Y = 1 - vbUV[1].Y;
                    vbUV[2].Y = 1 - vbUV[2].Y;
                    vbUV[3].Y = 1 - vbUV[3].Y;
                }
                else if (flipUVWhenGL && UEngine.Instance.GfxDevice.RenderContext.RHIType == ERHIType.RHT_OGL)
                {
                    vbUV[0].Y = 1 - vbUV[0].Y;
                    vbUV[1].Y = 1 - vbUV[1].Y;
                    vbUV[2].Y = 1 - vbUV[2].Y;
                    vbUV[3].Y = 1 - vbUV[3].Y;
                }

                var nor = new Vector3(0, 0, -1);
                for (int i = 0; i < 4; i++)
                {   
                    builder.AddVertex(ref vbPos[i], ref nor, ref vbUV[i], 0xFFFFFFFF);
                }

                for (int i = 0; i < dpDesc.NumPrimitives; i++)
                {
                    builder.AddTriangle(ibData[i * 3], ibData[i * 3 + 1], ibData[i * 3 + 2]);
                }
            }

            return meshBuilder;
        }
        private static uint MakeIndex(ushort x, ushort z)
        {
            return x | ((uint)z) << 16;
        }
        public static unsafe CMeshDataProvider MakeGridIndices(ushort NumX, ushort NumZ)
        {
            var meshBuilder = new Graphics.Mesh.CMeshDataProvider();
            var builder = meshBuilder.mCoreObject;
            uint streams = 0;
            builder.Init(streams, EIndexBufferType.IBT_Int32, 1);

            List<uint> indexer = new List<uint>();
            for (ushort i = 0; i < NumZ; i++)
            {
                for (ushort j = 0; j < NumX; j++)
                {
                    uint a, b, c;
                    a = MakeIndex(j, i);
                    b = MakeIndex(j, (ushort)(i + 1));
                    c = MakeIndex((ushort)(j + 1), (ushort)(i + 1));
                    indexer.Add(a);
                    indexer.Add(b);
                    indexer.Add(c);

                    builder.AddTriangle(a, b, c);

                    a = MakeIndex(j, i);
                    b = MakeIndex((ushort)(j + 1), (ushort)(i + 1));
                    c = MakeIndex((ushort)(j + 1), i);
                    indexer.Add(a);
                    indexer.Add(b);
                    indexer.Add(c);

                    builder.AddTriangle(a, b, c);
                }
            }

            var dpDesc = new DrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = (uint)indexer.Count / 3;
            builder.PushAtomLOD(0, ref dpDesc);

            return meshBuilder;
        }
    }
}

namespace EngineNS.UTest
{
    [UTest]
    public class UTest_Mesh
    {
        public void UnitTestEntrance()
        {
            var box = Graphics.Mesh.CMeshDataProvider.MakeBox(0, 0, 0, 1, 1, 1, Graphics.Mesh.CMeshDataProvider.EBoxFace.All);

            var boxMesh = box.ToMesh();
            box.mCoreObject.GetAtomNumber();
        }
    }
}
