using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public class CGfxMeshCooker
    {
        public static void MakeRect2D(CRenderContext rc, CGfxMeshPrimitives result, float x, float y, float w, float h, float z, bool lh=true, bool flipUVWhenGL = false, ECpuAccess cpuAccess = (ECpuAccess)0)
        {
            CDrawPrimitiveDesc dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = 2;
            result.PushAtomLOD(0, ref dpDesc);
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

                var mesh = result.GeometryMesh;
                var vbDesc = new CVertexBufferDesc();
                
                Vector3[] vbPos = new Vector3[4];
                vbPos[0].SetValue(x, y, z);
                vbPos[1].SetValue(x + w, y, z);
                vbPos[2].SetValue(x + w, y + h, z);
                vbPos[3].SetValue(x, y + h, z);
                fixed (Vector3* pV3 = &vbPos[0])
                {
                    vbDesc.InitData = (IntPtr)pV3;
                    vbDesc.Stride = (UInt32)sizeof(Vector3);
                    vbDesc.ByteWidth = (UInt32)sizeof(Vector3) * 4;
                    vbDesc.CPUAccess = (uint)cpuAccess;
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Position, vb);
                }

                Vector2[] vbUV = new Vector2[4];
                vbUV[0].SetValue(0, 0);
                vbUV[1].SetValue(1, 0);
                vbUV[2].SetValue(1, 1);
                vbUV[3].SetValue(0, 1);
                if (lh == false && CEngine.Instance.Desc.RHIType == ERHIType.RHT_D3D11)
                {//全屏幕用的-1，-1，2，2这里要换手系
                    vbUV[0].Y = 1 - vbUV[0].Y;
                    vbUV[1].Y = 1 - vbUV[1].Y;
                    vbUV[2].Y = 1 - vbUV[2].Y;
                    vbUV[3].Y = 1 - vbUV[3].Y;
                }
                else if(flipUVWhenGL && CEngine.Instance.Desc.RHIType == ERHIType.RHT_OGL)
                {
                    vbUV[0].Y = 1 - vbUV[0].Y;
                    vbUV[1].Y = 1 - vbUV[1].Y;
                    vbUV[2].Y = 1 - vbUV[2].Y;
                    vbUV[3].Y = 1 - vbUV[3].Y;
                }
                fixed (Vector2* pV2 = &vbUV[0])
                {
                    vbDesc.InitData = (IntPtr)pV2;
                    vbDesc.Stride = (UInt32)sizeof(Vector2);
                    vbDesc.ByteWidth = (UInt32)sizeof(Vector2) * 4;
                    vbDesc.CPUAccess = (uint)cpuAccess;
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_UV, vb);
                }
                
                fixed (UInt16* pV = &ibData[0])
                {
                    var ibDesc = new CIndexBufferDesc();
                    ibDesc.InitData = (IntPtr)pV;
                    ibDesc.ByteWidth = (UInt32)sizeof(UInt16) * 6;
                    var ib = rc.CreateIndexBuffer(ibDesc);
                    mesh.BindIndexBuffer(ib);
                }
                result.ResourceState.ResourceSize = 20 * 4;
                mesh.Dirty = true;
            }
            result.ResourceState.StreamState = EStreamingState.SS_Valid;
            result.ResourceState.KeepValid = true;
        }
        public static void MakeUIScale9(CRenderContext rc, CGfxMeshPrimitives result, float x, float y, float w, float h, float z, bool lh=true, bool flipUVWhenGL = false)
        {
            var dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = 18;
            result.PushAtomLOD(0, ref dpDesc);
            unsafe
            {
                var ibData = new UInt16[54];
                ibData[0] = 0;
                ibData[1] = 1;
                ibData[2] = 2;
                ibData[3] = 0;
                ibData[4] = 2;
                ibData[5] = 3;
                ibData[6] = 4;
                ibData[7] = 5;
                ibData[8] = 6;
                ibData[9] = 4;
                ibData[10] = 6;
                ibData[11] = 7;
                ibData[12] = 8;
                ibData[13] = 9;
                ibData[14] = 10;
                ibData[15] = 8;
                ibData[16] = 10;
                ibData[17] = 11;
                ibData[18] = 12;
                ibData[19] = 13;
                ibData[20] = 14;
                ibData[21] = 12;
                ibData[22] = 14;
                ibData[23] = 15;
                ibData[24] = 16;
                ibData[25] = 17;
                ibData[26] = 18;
                ibData[27] = 16;
                ibData[28] = 18;
                ibData[29] = 19;
                ibData[30] = 20;
                ibData[31] = 21;
                ibData[32] = 22;
                ibData[33] = 20;
                ibData[34] = 22;
                ibData[35] = 23;
                ibData[36] = 24;
                ibData[37] = 25;
                ibData[38] = 26;
                ibData[39] = 24;
                ibData[40] = 26;
                ibData[41] = 27;
                ibData[42] = 28;
                ibData[43] = 29;
                ibData[44] = 30;
                ibData[45] = 28;
                ibData[46] = 30;
                ibData[47] = 31;
                ibData[48] = 32;
                ibData[49] = 33;
                ibData[50] = 34;
                ibData[51] = 32;
                ibData[52] = 34;
                ibData[53] = 35;
                if(lh == false)
                {
                    ibData[1] = 2;
                    ibData[2] = 1;
                    ibData[4] = 3;
                    ibData[5] = 2;
                    ibData[7] = 6;
                    ibData[8] = 5;
                    ibData[10] = 7;
                    ibData[11] = 6;
                    ibData[13] = 10;
                    ibData[14] = 9;
                    ibData[16] = 11;
                    ibData[17] = 10;
                    ibData[19] = 14;
                    ibData[20] = 13;
                    ibData[22] = 15;
                    ibData[23] = 14;
                    ibData[25] = 18;
                    ibData[26] = 17;
                    ibData[28] = 19;
                    ibData[29] = 18;
                    ibData[31] = 22;
                    ibData[32] = 21;
                    ibData[34] = 23;
                    ibData[35] = 22;
                    ibData[37] = 26;
                    ibData[38] = 25;
                    ibData[40] = 27;
                    ibData[41] = 26;
                    ibData[43] = 30;
                    ibData[44] = 29;
                    ibData[46] = 31;
                    ibData[47] = 30;
                    ibData[49] = 34;
                    ibData[50] = 33;
                    ibData[52] = 35;
                    ibData[53] = 34;
                }

                var mesh = result.GeometryMesh;
                var vbDesc = new CVertexBufferDesc();

                var vbTempPos = new Vector3[16];
                var deltaW = w / 3;
                var deltaH = h / 3;
                for(int yV=0; yV < 4; yV++)
                {
                    for (int xV = 0; xV < 4; xV++)
                    {
                        vbTempPos[yV * 4 + xV].SetValue(x + deltaW * xV, y + deltaH * yV, z);
                    }
                }
                var vbPos = new Vector3[36];
                vbPos[0] = vbTempPos[0];
                vbPos[1] = vbTempPos[1];
                vbPos[2] = vbTempPos[5];
                vbPos[3] = vbTempPos[4];
                vbPos[4] = vbTempPos[1];
                vbPos[5] = vbTempPos[2];
                vbPos[6] = vbTempPos[6];
                vbPos[7] = vbTempPos[5];
                vbPos[8] = vbTempPos[2];
                vbPos[9] = vbTempPos[3];
                vbPos[10] = vbTempPos[7];
                vbPos[11] = vbTempPos[6];
                vbPos[12] = vbTempPos[4];
                vbPos[13] = vbTempPos[5];
                vbPos[14] = vbTempPos[9];
                vbPos[15] = vbTempPos[8];
                vbPos[16] = vbTempPos[5];
                vbPos[17] = vbTempPos[6];
                vbPos[18] = vbTempPos[10];
                vbPos[19] = vbTempPos[9];
                vbPos[20] = vbTempPos[6];
                vbPos[21] = vbTempPos[7];
                vbPos[22] = vbTempPos[11];
                vbPos[23] = vbTempPos[10];
                vbPos[24] = vbTempPos[8];
                vbPos[25] = vbTempPos[9];
                vbPos[26] = vbTempPos[13];
                vbPos[27] = vbTempPos[12];
                vbPos[28] = vbTempPos[9];
                vbPos[29] = vbTempPos[10];
                vbPos[30] = vbTempPos[14];
                vbPos[31] = vbTempPos[13];
                vbPos[32] = vbTempPos[10];
                vbPos[33] = vbTempPos[11];
                vbPos[34] = vbTempPos[15];
                vbPos[35] = vbTempPos[14];
                fixed (Vector3* pV3 = &vbPos[0])
                {
                    vbDesc.InitData = (IntPtr)pV3;
                    vbDesc.Stride = (UInt32)sizeof(Vector3);
                    vbDesc.ByteWidth = (UInt32)sizeof(Vector3) * 36;
                    vbDesc.CPUAccess = (UInt32)ECpuAccess.CAS_WRITE;
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Position, vb);
                }

                var vbTempUV = new Vector2[16];
                var deltaU = 1.0f / 3;
                var deltaV = 1.0f / 3;
                for(int yV = 0; yV < 4; yV++)
                {
                    for(int xV = 0; xV < 4; xV++)
                    {
                        if (lh == false && CEngine.Instance.Desc.RHIType == ERHIType.RHT_D3D11)
                            vbTempUV[yV * 4 + xV].SetValue(deltaU * xV, 1 - deltaV * yV);
                        else if (flipUVWhenGL && CEngine.Instance.Desc.RHIType == ERHIType.RHT_OGL)
                            vbTempUV[yV * 4 + xV].SetValue(deltaU * xV, 1 - deltaV * yV);
                        else
                            vbTempUV[yV * 4 + xV].SetValue(deltaU * xV, deltaV * yV);
                    }
                }
                var vbUV = new Vector2[36];
                vbUV[0] =  vbTempUV[0];
                vbUV[1] =  vbTempUV[1];
                vbUV[2] =  vbTempUV[5];
                vbUV[3] =  vbTempUV[4];
                vbUV[4] =  vbTempUV[1];
                vbUV[5] =  vbTempUV[2];
                vbUV[6] =  vbTempUV[6];
                vbUV[7] =  vbTempUV[5];
                vbUV[8] =  vbTempUV[2];
                vbUV[9] =  vbTempUV[3];
                vbUV[10] = vbTempUV[7];
                vbUV[11] = vbTempUV[6];
                vbUV[12] = vbTempUV[4];
                vbUV[13] = vbTempUV[5];
                vbUV[14] = vbTempUV[9];
                vbUV[15] = vbTempUV[8];
                vbUV[16] = vbTempUV[5];
                vbUV[17] = vbTempUV[6];
                vbUV[18] = vbTempUV[10];
                vbUV[19] = vbTempUV[9];
                vbUV[20] = vbTempUV[6];
                vbUV[21] = vbTempUV[7];
                vbUV[22] = vbTempUV[11];
                vbUV[23] = vbTempUV[10];
                vbUV[24] = vbTempUV[8];
                vbUV[25] = vbTempUV[9];
                vbUV[26] = vbTempUV[13];
                vbUV[27] = vbTempUV[12];
                vbUV[28] = vbTempUV[9];
                vbUV[29] = vbTempUV[10];
                vbUV[30] = vbTempUV[14];
                vbUV[31] = vbTempUV[13];
                vbUV[32] = vbTempUV[10];
                vbUV[33] = vbTempUV[11];
                vbUV[34] = vbTempUV[15];
                vbUV[35] = vbTempUV[14];
                fixed (Vector2* pV2 = &vbUV[0])
                {
                    vbDesc.InitData = (IntPtr)pV2;
                    vbDesc.Stride = (UInt32)sizeof(Vector2);
                    vbDesc.ByteWidth = (UInt32)sizeof(Vector2) * 36;
                    vbDesc.CPUAccess = (UInt32)ECpuAccess.CAS_WRITE;
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_UV, vb);
                }

                fixed(UInt16* pV = &ibData[0])
                {
                    var ibDesc = new CIndexBufferDesc();
                    ibDesc.InitData = (IntPtr)pV;
                    ibDesc.ByteWidth = (UInt32)sizeof(UInt16) * 54;
                    var ib = rc.CreateIndexBuffer(ibDesc);
                    mesh.BindIndexBuffer(ib);
                }
                result.ResourceState.ResourceSize = 20 * 16;
                mesh.Dirty = true;
            }
            result.ResourceState.StreamState = EStreamingState.SS_Valid;
            result.ResourceState.KeepValid = true;
        }
        public static readonly RName CookBoxName = RName.GetRName("@cook/box.vms");
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
        public static void MakeBox(CRenderContext rc, CGfxMeshPrimitives result, float x, float y, float z, float xSize, float ySize, float zSize, 
            EBoxFace faceFlags = EBoxFace.All)
        {
            var dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = 0;
            
            result.AABB = new BoundingBox(x, y, z, x + xSize, y + ySize, z + zSize);

            unsafe
            {
                UInt32 resourceSize = 0;
                var mesh = result.GeometryMesh;
                // 顶点
                var vbDesc = new CVertexBufferDesc();
                var vbPos = new Vector3[8];
                vbPos[0].SetValue(x, y, z);
                vbPos[1].SetValue(x, y + ySize, z);
                vbPos[2].SetValue(x + xSize, y + ySize, z);
                vbPos[3].SetValue(x + xSize, y, z);
                vbPos[4].SetValue(x, y, z + zSize);
                vbPos[5].SetValue(x, y + ySize, z + zSize);
                vbPos[6].SetValue(x + xSize, y + ySize, z + zSize);
                vbPos[7].SetValue(x + xSize, y, z + zSize);
                var pPos = new Vector3[24];
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

                fixed (Vector3* pv3 = &pPos[0])
                {
                    vbDesc.InitData = (IntPtr)pv3;
                    vbDesc.Stride = (UInt32)sizeof(Vector3);
                    vbDesc.ByteWidth = (UInt32)sizeof(Vector3) * 24;
                    resourceSize += vbDesc.ByteWidth;
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Position, vb);
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

                fixed(UInt16* pV = &pIndex[0])
                {
                    var ibDesc = new CIndexBufferDesc();
                    ibDesc.InitData = (IntPtr)pV;
                    ibDesc.ByteWidth = (UInt32)sizeof(UInt16) * curIndex;
                    resourceSize += ibDesc.ByteWidth;
                    var ib = rc.CreateIndexBuffer(ibDesc);
                    mesh.BindIndexBuffer(ib);
                }

                // UV
                var vLB = new Vector2(0, 1);
                var vLT = new Vector2(0, 0);
                var vRT = new Vector2(1, 0);
                var vRB = new Vector2(1, 1);
                var vUV = new Vector2[24];
                vUV[0] = vLB; vUV[1] = vLT; vUV[2] = vRT; vUV[3] = vRB;
                vUV[4] = vRB; vUV[5] = vRT; vUV[6] = vLT; vUV[7] = vLB;
                vUV[8] = vLB; vUV[9] = vLT; vUV[10] = vRT; vUV[11] = vRB;
                vUV[12] = vLB; vUV[13] = vLT; vUV[14] = vRT; vUV[15] = vRB;
                vUV[16] = vLB; vUV[17] = vLT; vUV[18] = vRT; vUV[19] = vRB;
                vUV[20] = vLB; vUV[21] = vLT; vUV[22] = vRT; vUV[23] = vRB;
                fixed(Vector2* pUV = &vUV[0])
                {
                    vbDesc.InitData = (IntPtr)pUV;
                    vbDesc.Stride = (UInt32)sizeof(Vector2);
                    vbDesc.ByteWidth = (UInt32)sizeof(Vector2) * 24;
                    resourceSize += vbDesc.ByteWidth;
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_UV, vb);
                }

                // 法线
                var  vFront = new Vector3(0, 0, -1);
                var  vBack = new Vector3(0, 0, 1);
                var  vLeft = new Vector3(-1, 0, 0);
                var  vRight = new Vector3(1, 0, 0);
                var  vTop = new Vector3(0, 1, 0);
                var vBottom = new Vector3(0, -1, 0);
                var vNor = new Vector3[24];
                vNor[0] = vFront; vNor[1] = vFront; vNor[2] = vFront; vNor[3] = vFront;
                vNor[4] = vBack; vNor[5] = vBack; vNor[6] = vBack; vNor[7] = vBack;
                vNor[8] = vLeft; vNor[9] = vLeft; vNor[10] = vLeft; vNor[11] = vLeft;
                vNor[12] = vRight; vNor[13] = vRight; vNor[14] = vRight; vNor[15] = vRight;
                vNor[16] = vTop; vNor[17] = vTop; vNor[18] = vTop; vNor[19] = vTop;
                vNor[20] = vBottom; vNor[21] = vBottom; vNor[22] = vBottom; vNor[23] = vBottom;
                fixed(Vector3* pNor = &vNor[0])
                {
                    vbDesc.InitData = (IntPtr)pNor;
                    vbDesc.Stride = (UInt32)sizeof(Vector3);
                    vbDesc.ByteWidth = (UInt32)sizeof(Vector3) * 24;
                    resourceSize += vbDesc.ByteWidth;
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Normal, vb);
                }

                mesh.Dirty = true;
                result.ResourceState.ResourceSize = resourceSize;
            }

            result.PushAtomLOD(0, ref dpDesc);
            result.ResourceState.StreamState = EStreamingState.SS_Valid;
            result.ResourceState.KeepValid = true;
        }

        public static void MakeBox(CRenderContext rc, CGfxMeshDataProvider result, float x, float y, float z, float xSize, float ySize, float zSize,
            EBoxFace faceFlags = EBoxFace.All)
        {
            var dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = 0;

            unsafe
            {
                UInt32 streams = (1 << (int)EVertexSteamType.VST_Position) |
                (1 << (int)EVertexSteamType.VST_Normal) |
                (1 << (int)EVertexSteamType.VST_UV);
                result.Init(streams, EIndexBufferType.IBT_Int16, 1);

                // 顶点
                //var vbDesc = new CVertexBufferDesc();
                var vbPos = new Vector3[8];
                vbPos[0].SetValue(x, y, z);
                vbPos[1].SetValue(x, y + ySize, z);
                vbPos[2].SetValue(x + xSize, y + ySize, z);
                vbPos[3].SetValue(x + xSize, y, z);
                vbPos[4].SetValue(x, y, z + zSize);
                vbPos[5].SetValue(x, y + ySize, z + zSize);
                vbPos[6].SetValue(x + xSize, y + ySize, z + zSize);
                vbPos[7].SetValue(x + xSize, y, z + zSize);
                var pPos = new Vector3[24];
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

                // UV
                var vLB = new Vector2(0, 1);
                var vLT = new Vector2(0, 0);
                var vRT = new Vector2(1, 0);
                var vRB = new Vector2(1, 1);
                var vUV = new Vector2[24];
                vUV[0] = vLB; vUV[1] = vLT; vUV[2] = vRT; vUV[3] = vRB;
                vUV[4] = vRB; vUV[5] = vRT; vUV[6] = vLT; vUV[7] = vLB;
                vUV[8] = vLB; vUV[9] = vLT; vUV[10] = vRT; vUV[11] = vRB;
                vUV[12] = vLB; vUV[13] = vLT; vUV[14] = vRT; vUV[15] = vRB;
                vUV[16] = vLB; vUV[17] = vLT; vUV[18] = vRT; vUV[19] = vRB;
                vUV[20] = vLB; vUV[21] = vLT; vUV[22] = vRT; vUV[23] = vRB;

                // 法线
                var vFront = new Vector3(0, 0, -1);
                var vBack = new Vector3(0, 0, 1);
                var vLeft = new Vector3(-1, 0, 0);
                var vRight = new Vector3(1, 0, 0);
                var vTop = new Vector3(0, 1, 0);
                var vBottom = new Vector3(0, -1, 0);
                var vNor = new Vector3[24];
                vNor[0] = vFront; vNor[1] = vFront; vNor[2] = vFront; vNor[3] = vFront;
                vNor[4] = vBack; vNor[5] = vBack; vNor[6] = vBack; vNor[7] = vBack;
                vNor[8] = vLeft; vNor[9] = vLeft; vNor[10] = vLeft; vNor[11] = vLeft;
                vNor[12] = vRight; vNor[13] = vRight; vNor[14] = vRight; vNor[15] = vRight;
                vNor[16] = vTop; vNor[17] = vTop; vNor[18] = vTop; vNor[19] = vTop;
                vNor[20] = vBottom; vNor[21] = vBottom; vNor[22] = vBottom; vNor[23] = vBottom;

                for (int i = 0; i < 24; i++)
                {
                    //result.AddVertex(ref pPos[i], ref vNor[i], ref vUV[i], 0);
                }
                for (int i = 0; i < dpDesc.NumPrimitives * 2; i++)
                {
                    //result.AddTriangle(pIndex[i * 3 + 0], pIndex[i * 3 + 1], pIndex[i * 3 + 2]);
                }
            }

            result.PushAtomLOD(0, ref dpDesc);
        }
        public static void MakeCapsule(CRenderContext rc, CGfxMeshPrimitives result, float radius , float halfHeight,
          EBoxFace faceFlags = EBoxFace.All)
        {
            var dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = 0;

            //result.AABB = new BoundingBox(x, y, z, x + xSize, y + ySize, z + zSize);
        }
        public static float Lerp(float start, float end, float factor)
        {
            return start + factor * (end - start);
        }
        public static void MakePlane10x10(CRenderContext rc, CGfxMeshPrimitives result, Vector2 uvMin, Vector2 uvMax, bool lh = true, bool flipUVWhenGL = false)
        {
            UInt32 tileCount = 10;
            var dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = tileCount * tileCount * 2;
            result.PushAtomLOD(0, ref dpDesc);


            using (var posArray = Support.NativeListProxy<Vector3>.CreateNativeList())
            using (var normalArray = Support.NativeListProxy<Vector3>.CreateNativeList())
            using (var tangentArray = Support.NativeListProxy<Vector4>.CreateNativeList())
            using (var uvArray = Support.NativeListProxy<Vector2>.CreateNativeList())
            using (var indexArray = Support.NativeListProxy<UInt32>.CreateNativeList())
            {
                // -> tileCount * tileCount * 2 triangles
                float Step = 2.0f / tileCount;
                for (Int32 y = 0; y < tileCount; ++y)
                {
                    // implemented this way to avoid cracks, could be optimized
                    float z0 = y * Step - 1.0f;
                    float z1 = (y + 1) * Step - 1.0f;

                    float V0 = Lerp(uvMin.Y, uvMax.Y, z0 * 0.5f + 0.5f);
                    float V1 = Lerp(uvMin.Y, uvMax.Y, z1 * 0.5f + 0.5f);

                    for (Int32 x = 0; x < tileCount; ++x)
                    {
                        // implemented this way to avoid cracks, could be optimized
                        float x0 = x * Step - 1.0f;
                        float x1 = (x + 1) * Step - 1.0f;

                        float U0 = Lerp(uvMin.X, uvMax.X, x0 * 0.5f + 0.5f);
                        float U1 = Lerp(uvMin.X, uvMax.X, x1 * 0.5f + 0.5f);

                        // Calculate verts for a face pointing down Z
                        posArray.Add(new Vector3(x0, 0, z0));
                        posArray.Add(new Vector3(x0, 0, z1));
                        posArray.Add(new Vector3(x1, 0, z1));
                        posArray.Add(new Vector3(x1, 0, z0));
                        normalArray.Add(new Vector3(0, 1, 0));
                        normalArray.Add(new Vector3(0, 1, 0));
                        normalArray.Add(new Vector3(0, 1, 0));
                        normalArray.Add(new Vector3(0, 1, 0));
                        tangentArray.Add(new Vector4(1, 0, 0, 1));
                        tangentArray.Add(new Vector4(1, 0, 0, 1));
                        tangentArray.Add(new Vector4(1, 0, 0, 1));
                        tangentArray.Add(new Vector4(1, 0, 0, 1));
                        uvArray.Add(new Vector2(U0, V0));
                        uvArray.Add(new Vector2(U0, V1));
                        uvArray.Add(new Vector2(U1, V1));
                        uvArray.Add(new Vector2(U1, V0));
                        UInt32 Index = (UInt32)((x + y * tileCount) * 4);
                        indexArray.Add(Index + 0);
                        indexArray.Add(Index + 1);
                        indexArray.Add(Index + 2);
                        indexArray.Add(Index + 0);
                        indexArray.Add(Index + 2);
                        indexArray.Add(Index + 3);
                    }
                }

                unsafe
                {
                    UInt32 resourceSize = 0;
                    var vbDesc = new CVertexBufferDesc();
                    var mesh = result.GeometryMesh;
                    {
                        var pV3 = posArray.UnsafeGetElementAddress(0);
                        vbDesc.InitData = (IntPtr)pV3;
                        vbDesc.Stride = (UInt32)sizeof(Vector3);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Vector3) * posArray.Count);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_Position, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }

                    {
                        var pV3 = normalArray.UnsafeGetElementAddress(0);
                        vbDesc.InitData = (IntPtr)pV3;
                        vbDesc.Stride = (UInt32)sizeof(Vector3);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Vector3) * normalArray.Count);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_Normal, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }

                    {
                        var pV3 = tangentArray.UnsafeGetElementAddress(0);
                        vbDesc.InitData = (IntPtr)pV3;
                        vbDesc.Stride = (UInt32)sizeof(Vector4);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Vector4) * tangentArray.Count);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_Tangent, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }

                    {
                        var pV = uvArray.UnsafeGetElementAddress(0);
                        vbDesc.InitData = (IntPtr)pV;
                        vbDesc.Stride = (UInt32)sizeof(Vector2);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Vector2) * uvArray.Count);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_UV, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }

                    {
                        var pV = indexArray.UnsafeGetElementAddress(0);
                        var ibDesc = new CIndexBufferDesc();
                        ibDesc.InitData = (IntPtr)pV;
                        ibDesc.Type = EIndexBufferType.IBT_Int32;
                        ibDesc.ByteWidth = (UInt32)(sizeof(UInt32) * indexArray.Count);
                        var ib = rc.CreateIndexBuffer(ibDesc);
                        mesh.BindIndexBuffer(ib);
                        resourceSize += ibDesc.ByteWidth;
                    }
                    result.ResourceState.ResourceSize = (UInt32)(resourceSize);
                    mesh.Dirty = true;
                }
            }
            
            result.ResourceState.StreamState = EStreamingState.SS_Valid;
            result.ResourceState.KeepValid = true;
        }

        public static void MakeRect3D(CRenderContext rc, CGfxMeshPrimitives result)
        {
            var dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = 2;
            result.PushAtomLOD(0, ref dpDesc);


            List<Vector3> posArray = new List<Vector3>();
            List<Vector3> normalArray = new List<Vector3>();
            List<Vector4> tangentArray = new List<Vector4>();
            List<Vector2> uvArray = new List<Vector2>();
            List<UInt32> indexArray = new List<UInt32>();

            List<Byte4> Colors = new List<Byte4>();
            Byte4 color = new Byte4();
            color.X = 255;
            color.Y = 255;
            color.Z = 255;
            color.W = 255;
            Colors.Add(color);
            Colors.Add(color);
            Colors.Add(color);
            Colors.Add(color);

            // -> tileCount * tileCount * 2 triangles
            float x0 = -0.5f;
            float x1 = 0.5f;
            float y0 = -0.5f;
            float y1 = 0.5f;

            float V0 = 0.0f;
            float V1 = 1.0f;
            float U0 = 0.0f;
            float U1 = 1.0f;

            posArray.Add(new Vector3(x0, y0, 0));
            posArray.Add(new Vector3(x1, y0, 0));
            posArray.Add(new Vector3(x1, y1, 0));
            posArray.Add(new Vector3(x0, y1, 0));
            normalArray.Add(new Vector3(0, 0, 1));
            normalArray.Add(new Vector3(0, 0, 1));
            normalArray.Add(new Vector3(0, 0, 1));
            normalArray.Add(new Vector3(0, 0, 1));

            
            uvArray.Add(new Vector2(U1, V1));
            uvArray.Add(new Vector2(U0, V1));
            uvArray.Add(new Vector2(U0, V0));
            uvArray.Add(new Vector2(U1, V0));

            var targent = CalculationTargent(posArray[0], posArray[3], posArray[1], normalArray[0], uvArray[0], uvArray[3], uvArray[1]);
            tangentArray.Add(new Vector4(targent.X, targent.Y, targent.Z, 0));

            targent = CalculationTargent(posArray[1], posArray[0], posArray[2], normalArray[1], uvArray[1], uvArray[0], uvArray[2]);
            tangentArray.Add(new Vector4(targent.X, targent.Y, targent.Z, 0));

            targent = CalculationTargent(posArray[2], posArray[1], posArray[3], normalArray[2], uvArray[2], uvArray[1], uvArray[3]);
            tangentArray.Add(new Vector4(targent.X, targent.Y, targent.Z, 0));

            targent = CalculationTargent(posArray[3], posArray[0], posArray[2], normalArray[3], uvArray[3], uvArray[0], uvArray[2]);
            tangentArray.Add(new Vector4(targent.X, targent.Y, targent.Z, 0));

            indexArray.Add(0);
            indexArray.Add(1);
            indexArray.Add(2);
            indexArray.Add(0);
            indexArray.Add(2);
            indexArray.Add(3);

            unsafe
            {
                UInt32 resourceSize = 0;
                var vbDesc = new CVertexBufferDesc();
                var mesh = result.GeometryMesh;
                fixed (Vector3* pV3 = &posArray.ToArray()[0])
                {
                    vbDesc.InitData = (IntPtr)pV3;
                    vbDesc.Stride = (UInt32)sizeof(Vector3);
                    vbDesc.ByteWidth = (UInt32)(sizeof(Vector3) * posArray.Count);
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Position, vb);
                    resourceSize += vbDesc.ByteWidth;
                }

                fixed (Byte4* pc = &Colors.ToArray()[0])
                {
                    vbDesc.InitData = (IntPtr)pc;
                    vbDesc.Stride = (UInt32)sizeof(Byte4);
                    vbDesc.ByteWidth = (UInt32)(sizeof(Byte4) * Colors.Count);
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Color, vb);
                    resourceSize += vbDesc.ByteWidth;
                }

                fixed (Vector3* pV3 = &normalArray.ToArray()[0])
                {
                    vbDesc.InitData = (IntPtr)pV3;
                    vbDesc.Stride = (UInt32)sizeof(Vector3);
                    vbDesc.ByteWidth = (UInt32)(sizeof(Vector3) * normalArray.Count);
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Normal, vb);
                    resourceSize += vbDesc.ByteWidth;
                }

                fixed (Vector4* pV4 = &tangentArray.ToArray()[0])
                {
                    vbDesc.InitData = (IntPtr)pV4;
                    vbDesc.Stride = (UInt32)sizeof(Vector4);
                    vbDesc.ByteWidth = (UInt32)(sizeof(Vector4) * tangentArray.Count);
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Tangent, vb);
                    resourceSize += vbDesc.ByteWidth;
                }

                fixed (Vector2* pV = &uvArray.ToArray()[0])
                {
                    vbDesc.InitData = (IntPtr)pV;
                    vbDesc.Stride = (UInt32)sizeof(Vector2);
                    vbDesc.ByteWidth = (UInt32)(sizeof(Vector2) * uvArray.Count);
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_UV, vb);
                    resourceSize += vbDesc.ByteWidth;
                }

                fixed (UInt32* pV = &indexArray.ToArray()[0])
                {
                    var ibDesc = new CIndexBufferDesc();
                    ibDesc.InitData = (IntPtr)pV;
                    ibDesc.Type = EIndexBufferType.IBT_Int32;
                    ibDesc.ByteWidth = (UInt32)(sizeof(UInt32) * indexArray.Count);
                    var ib = rc.CreateIndexBuffer(ibDesc);
                    mesh.BindIndexBuffer(ib);
                    resourceSize += ibDesc.ByteWidth;
                }
                result.ResourceState.ResourceSize = (UInt32)(resourceSize);
                mesh.Dirty = true;
            }
            result.ResourceState.StreamState = EStreamingState.SS_Valid;
            result.ResourceState.KeepValid = true;
        }

        public static void CalculationTargent(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 normal, ref Vector2 pVtxA, ref Vector2 pVtxB, ref Vector2 pVtxC, out Vector3 vTangent)
        {
            var ab = b - a;
            var ac = c - a;
            CalculationTargent(ref ab, ref ac, ref normal, ref pVtxA, ref pVtxB, ref pVtxC, out vTangent);

        }

        public static Vector3 CalculationTargent(Vector3 a, Vector3 b, Vector3 c, Vector3 normal, Vector2 pVtxA, Vector2 pVtxB, Vector2 pVtxC)
        {
            var ab = b - a;
            var ac = c - a;
            Vector3 vTangent;
            CalculationTargent(ref ab, ref ac, ref normal, ref pVtxA, ref pVtxB, ref pVtxC, out vTangent);
            return vTangent;

        }

        public static void CalculationTargent(ref Vector3 ab, ref Vector3 ac, ref Vector3 normal, ref Vector2 pVtxA, ref Vector2 pVtxB, ref Vector2 pVtxC, out Vector3 vTangent)
        {

            Vector3 vProjAB = ab - (Vector3.Dot(normal, ab) * normal);
            Vector3 vProjAC = ac - (Vector3.Dot(normal, ac) * normal);
            // vProjAB 是 AB在A点的切平面上的投影
            // vProjAC 是 AC在A点的切平面上的投影

            // tu texture coordinate differences
            float duAB = pVtxB.X - pVtxA.X;
            float duAC = pVtxC.X - pVtxA.X;
            // tv texture coordinate differences
            float dvAB = pVtxB.Y - pVtxA.Y;
            float dvAC = pVtxC.Y - pVtxA.Y;

            // 计算AB和AC向量上的纹理坐标差
            //  |duAC   dvAC|
            //  |duAB   dvAB|   
            // 判断符号等同计算法向量方向，从而决定纹理的变化方向
            if ((duAC * dvAB) > (duAB * dvAC))
            {
                duAC = -duAC;
                duAB = -duAB;
            }
            // 规范到标准环境，从谁到谁的问题，从AC在AB的左手，保持坐标系的关系


            vTangent = (duAC * vProjAB) - (duAB * vProjAC);
            // 计算 dv方向对应的方向为切向量
            // 这个公式是这样看的
            // vProjAB对应的纹理是 duAB, dvAB, vProjAC 对应的纹理是 duAC, dvAC
            // duAC * vProjAB - duAB * vProjAC 对应的纹理就是：
            // u: duAC * duAB - duAB * duAC  = 0
            // v: duAC * dvAB - duAb * dvAC  = 非0， 不重要，规范化会解决它

            vTangent.Normalize();
            // 规范化 纹理dv方向对应的切向量
        }

        public static void MakeScreenAlignedTriangle(CRenderContext RHICtx, CGfxMeshPrimitives PrimMesh)
        {
            CDrawPrimitiveDesc dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = 1;
            PrimMesh.PushAtomLOD(0, ref dpDesc);
            unsafe
            {
                var GeomData = PrimMesh.GeometryMesh;
                var vbDesc = new CVertexBufferDesc();

                Vector3[] VtxPosBuffer = new Vector3[3];
                VtxPosBuffer[0].SetValue(-1.0f, -1.0f, 0.0f);
                VtxPosBuffer[1].SetValue(-1.0f, 3.0f, 0.0f);
                VtxPosBuffer[2].SetValue(3.0f, -1.0f, 0.0f);

                fixed (Vector3* refVtxPosBuffer = &VtxPosBuffer[0])
                {
                    vbDesc.InitData = (IntPtr)refVtxPosBuffer;
                    vbDesc.Stride = (UInt32)sizeof(Vector3);
                    vbDesc.ByteWidth = (UInt32)sizeof(Vector3) * 3;
                    var vb = RHICtx.CreateVertexBuffer(vbDesc);
                    GeomData.BindVertexBuffer(EVertexSteamType.VST_Position, vb);
                }

                Vector2[] UVBuffer = new Vector2[3];
                UVBuffer[0].SetValue(0.0f, 1.0f);
                UVBuffer[1].SetValue(0.0f, -1.0f);
                UVBuffer[2].SetValue(2.0f, 1.0f);
                
                if (CEngine.Instance.Desc.RHIType == ERHIType.RHT_OGL)
                {
                    UVBuffer[0].Y = 1.0f - UVBuffer[0].Y;
                    UVBuffer[1].Y = 1.0f - UVBuffer[1].Y;
                    UVBuffer[2].Y = 1.0f - UVBuffer[2].Y;
                }

                fixed (Vector2* refUVBuffer = &UVBuffer[0])
                {
                    vbDesc.InitData = (IntPtr)refUVBuffer;
                    vbDesc.Stride = (UInt32)sizeof(Vector2);
                    vbDesc.ByteWidth = (UInt32)sizeof(Vector2) * 3;
                    var vb = RHICtx.CreateVertexBuffer(vbDesc);
                    GeomData.BindVertexBuffer(EVertexSteamType.VST_UV, vb);
                }

                UInt16[] IdxBuffer = new UInt16[3];
                IdxBuffer[0] = 0;
                IdxBuffer[1] = 1;
                IdxBuffer[2] = 2;

                fixed (UInt16* refIdxBuffer = &IdxBuffer[0])
                {
                    var ibDesc = new CIndexBufferDesc();
                    ibDesc.InitData = (IntPtr)refIdxBuffer;
                    ibDesc.ByteWidth = (UInt32)sizeof(UInt16) * 3;
                    var ib = RHICtx.CreateIndexBuffer(ibDesc);
                    GeomData.BindIndexBuffer(ib);
                }
                PrimMesh.ResourceState.ResourceSize = 66;
                GeomData.Dirty = true;
            }
            PrimMesh.ResourceState.StreamState = EStreamingState.SS_Valid;
            PrimMesh.ResourceState.KeepValid = true;
        }
    }
}
