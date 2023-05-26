using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public class UMeshDataProvider : AuxPtrType<NxRHI.FMeshDataProvider>
    {
        public UMeshDataProvider()
        {
            mCoreObject = NxRHI.FMeshDataProvider.CreateInstance();
        }
        public UMeshDataProvider(NxRHI.FMeshDataProvider self)
        {
            mCoreObject = self;
        }
        public bool IsIndex32
        {
            get
            {
                return mCoreObject.IsIndex32;
            }
        }
        public uint NumAtom
        {
            get
            {
                return mCoreObject.GetAtomNumber();
            }
        }
        public VIUnknown GetAtomExtData(uint index)
        {
            return mCoreObject.GetAtomExtData(index);
        }

        public UMeshPrimitives ToMesh()
        {
            unsafe
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                var result = new UMeshPrimitives();
                result.Init("", 1);
                
                mCoreObject.ToMesh(rc.mCoreObject, result.mCoreObject);
                result.AssetName = AssetName;
                return result;
            }
        }
        public void ToMesh(UMeshPrimitives mesh)
        {
            unsafe
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;

                mCoreObject.ToMesh(rc.mCoreObject, mesh.mCoreObject);
            }
        }
        public RName AssetName { get; set; }
        #region box
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
        public class UMakeBoxParameter
        {
            public Vector3 Position { get; set; }
            public Vector3 Extent { get; set; } = Vector3.One;
            [EGui.Controls.PropertyGrid.Color4PickerEditor()]
            public Vector4 Color { get; set; }
            public EBoxFace FaceFlags { get; set; } = EBoxFace.All;
        }
        public static unsafe UMeshDataProvider MakeBox(float x, float y, float z, float xSize, float ySize, float zSize, uint color = 0xffffffff,
            EBoxFace faceFlags = EBoxFace.All)
        {
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakeBox", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) | 
                (1 << (int)NxRHI.EVertexStreamType.VST_Normal) | 
                (1 << (int)NxRHI.EVertexStreamType.VST_Color) | 
                (1 << (int)NxRHI.EVertexStreamType.VST_UV));
            builder.Init(streams, false, 1);

            var aabb = new BoundingBox(x, y, z, x + xSize, y + ySize, z + zSize);
            builder.SetAABB(ref aabb);

            var dpDesc = new NxRHI.FMeshAtomDesc();
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
                builder.AddVertex(&pPos[i], &vNor[i], &vUV[i], color);
            }

            for (int i = 0; i < dpDesc.NumPrimitives; i++)
            {
                builder.AddTriangle(pIndex[i * 3], pIndex[i * 3 + 1], pIndex[i * 3 + 2]);
            }

            builder.PushAtomLOD(0, &dpDesc);
            return meshBuilder;
        }
        public static unsafe UMeshDataProvider MakeBoxWireframe(float x, float y, float z, float xSize, float ySize, float zSize, UInt32 color = 0xFFFFFFFF)
        {
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakeBoxWireframe", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Color));
            builder.Init(streams, false, 1);

            var aabb = new BoundingBox(x, y, z, x + xSize, y + ySize, z + zSize);
            builder.SetAABB(ref aabb);

            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = 0;
            dpDesc.PrimitiveType = NxRHI.EPrimitiveType.EPT_LineList;

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
                builder.AddVertex(in pPos[i], in Vector3.UnitY, in Vector2.One, color);
            }

            for (int i = 0; i < dpDesc.NumPrimitives; i++)
            {
                builder.AddLine(pIndex[i * 2], pIndex[i * 2 + 1]);
            }

            builder.PushAtomLOD(0, &dpDesc);
            return meshBuilder;
        }
        #endregion

        #region Rect2D
        public class UMakeRect2DParameter
        {
            public Vector3 Position { get; set; }
            public float Width { get; set; } = 1.0f;
            public float Height { get; set; } = 1.0f;
        }
        public static unsafe UMeshDataProvider MakeRect2D(float x, float y, float w, float h, float z, bool lh = true, NxRHI.ECpuAccess cpuAccess = NxRHI.ECpuAccess.CAS_DEFAULT)
        {
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakeRect2D", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Normal) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
                (1 << (int)NxRHI.EVertexStreamType.VST_UV));
            builder.Init(streams, false, 1);

            var dpDesc = new NxRHI.FMeshAtomDesc();
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
                vbPos[(int)ENUM_FRUSTUM_CORNER.ENUM_FRUSTUMCN_0].SetValue(x, y, z);
                vbPos[(int)ENUM_FRUSTUM_CORNER.ENUM_FRUSTUMCN_1].SetValue(x + w, y, z);
                vbPos[(int)ENUM_FRUSTUM_CORNER.ENUM_FRUSTUMCN_2].SetValue(x + w, y + h, z);
                vbPos[(int)ENUM_FRUSTUM_CORNER.ENUM_FRUSTUMCN_3].SetValue(x, y + h, z);
                
                Vector2[] vbUV = new Vector2[4];
                vbUV[(int)ENUM_FRUSTUM_CORNER.ENUM_FRUSTUMCN_0].SetValue(0, 0);
                vbUV[(int)ENUM_FRUSTUM_CORNER.ENUM_FRUSTUMCN_1].SetValue(1, 0);
                vbUV[(int)ENUM_FRUSTUM_CORNER.ENUM_FRUSTUMCN_2].SetValue(1, 1);
                vbUV[(int)ENUM_FRUSTUM_CORNER.ENUM_FRUSTUMCN_3].SetValue(0, 1);
                if (lh == false && (UEngine.Instance.GfxDevice.RenderContext.mCoreObject.Desc.RhiType == NxRHI.ERhiType.RHI_D3D11 ||
                    UEngine.Instance.GfxDevice.RenderContext.mCoreObject.Desc.RhiType == NxRHI.ERhiType.RHI_D3D12 ||
                    UEngine.Instance.GfxDevice.RenderContext.mCoreObject.Desc.RhiType == NxRHI.ERhiType.RHI_VK))
                {//全屏幕用的-1，-1，2，2这里要换手系
                    vbUV[0].Y = 1 - vbUV[0].Y;
                    vbUV[1].Y = 1 - vbUV[1].Y;
                    vbUV[2].Y = 1 - vbUV[2].Y;
                    vbUV[3].Y = 1 - vbUV[3].Y;
                }
                //else if (UEngine.Instance.GfxDevice.RenderContext.mCoreObject.Desc.RhiType == NxRHI.ERhiType.RHI_VK)// UEngine.Instance.GfxDevice.RenderContext.RHIType == ERHIType.RHT_OGL)
                //{
                //    vbUV[0].Y = 1 - vbUV[0].Y;
                //    vbUV[1].Y = 1 - vbUV[1].Y;
                //    vbUV[2].Y = 1 - vbUV[2].Y;
                //    vbUV[3].Y = 1 - vbUV[3].Y;
                //}

                var nor = new Vector3(0, 0, -1);
                for (int i = 0; i < 4; i++)
                {   
                    builder.AddVertex(in vbPos[i], in nor, in vbUV[i], 0xFFFFFFFF);
                }

                for (int i = 0; i < dpDesc.NumPrimitives; i++)
                {
                    builder.AddTriangle(ibData[i * 3], ibData[i * 3 + 1], ibData[i * 3 + 2]);
                }
            }

            return meshBuilder;
        }
        #endregion

        #region Sphere
        private static uint MakeIndex(ushort x, ushort z)
        {
            return x | ((uint)z) << 16;
        }
        private static uint CalcVertexIndex(uint z, uint x, uint NumX)
        {
            return (uint)(z * (NumX + 1) + x);
        }
        private static unsafe void SphereBuildSinCos(Vector2[] sincos_table, float angle_start, float angle_step)
        {
            float angle;
            int i;

            angle = angle_start;
            for (i = 0; i < sincos_table.Length; i++)
            {
                sincos_table[i].X = (float)Math.Sin(angle);
                sincos_table[i].Y = (float)Math.Cos(angle);
                angle += angle_step;
            }
        }
        private static uint SphereVertexIndex(uint slices, uint slice, uint stack)
        {
            return (uint)stack * slices + (uint)slice + 1;
        }
        public class UMakeSphereParameter
        {
            public float Radius { get; set; } = 1.0f;
            public uint Slices { get; set; } = 30;
            public uint Stacks { get; set; } = 30;
            [EGui.Controls.PropertyGrid.Color4PickerEditor()]
            public Vector4 Color { get; set; } = Vector4.One;
        }
        public static unsafe UMeshDataProvider MakeSphere(float radius, uint slices, uint stacks, uint color)
        {
            uint number_of_vertices, number_of_faces;            
            float phi_step, phi_start;
            float theta_step, theta, sin_theta, cos_theta;
            uint vertex, face, stack, slice;
            
            if (radius< 0.0f || slices< 2 || stacks< 2)
            {
                return null;
            }

            number_of_vertices = 2 + slices* (stacks-1);
            number_of_faces = 2 * slices + (stacks - 2) * (2 * slices);
 
            /* phi = angle on xz plane wrt z axis */
            phi_step = -2.0f * (float)Math.PI / slices;
            phi_start = (float)Math.PI / 2.0f;

            var sincos_table = new Vector2[slices];
            SphereBuildSinCos(sincos_table, phi_start, phi_step);

            /* theta = angle on xy plane wrt x axis */
            theta_step = (float)Math.PI / stacks;
            theta = theta_step;

            vertex = 0;
            face = 0;

            var faces = new Vector3ui[(int)number_of_faces];
            var pPos = new Vector3[(int)number_of_vertices];
            var pNor = new Vector3[(int)number_of_vertices];
            var pUV = new Vector2[(int)number_of_vertices];
            pNor[vertex].X = 0.0f;
            pNor[vertex].Y = 0.0f;
            pNor[vertex].Z = 1.0f;
            pPos[vertex].X = 0.0f;
            pPos[vertex].Y = 0.0f;
            pPos[vertex].Z = radius;
            pUV[vertex].X = 0;
            pUV[vertex].Y = 0;
            vertex++;

            for (stack = 0; stack < stacks - 1; stack++)
            {
                sin_theta = (float)Math.Sin(theta);
                cos_theta = (float)Math.Cos(theta);

                for (slice = 0; slice < slices; slice++)
                {
                    pNor[vertex].X = sin_theta * sincos_table[slice].Y;
                    pNor[vertex].Y = sin_theta * sincos_table[slice].X;
                    pNor[vertex].Z = cos_theta;
                    pPos[vertex].X = radius * sin_theta * sincos_table[slice].Y;
                    pPos[vertex].Y = radius * sin_theta * sincos_table[slice].X;
                    pPos[vertex].Z = radius * cos_theta;
                    pUV[vertex].X = (float)stack / (float)stacks;
                    pUV[vertex].Y = (float)slice / (float)slices;
                    vertex++;

                    if (slice > 0)
                    {
                        if (stack == 0)
                        {
                            /* top stack is triangle fan */
                            faces[(int)face].X = 0;
                            faces[(int)face].Y = slice + 1;
                            faces[(int)face].Z = slice;
                            face++;
                        }
                        else
                        {
                            /* stacks in between top and bottom are quad strips */
                            faces[(int)face].X = SphereVertexIndex(slices, slice - 1, stack - 1);
                            faces[(int)face].Y = SphereVertexIndex(slices, slice, stack - 1);
                            faces[(int)face].Z = SphereVertexIndex(slices, slice - 1, stack);
                            face++;

                            faces[(int)face].X = SphereVertexIndex(slices, slice, stack - 1);
                            faces[(int)face].Y = SphereVertexIndex(slices, slice, stack);
                            faces[(int)face].Z = SphereVertexIndex(slices, slice - 1, stack);
                            face++;
                        }
                    }
                }

                theta += theta_step;

                if (stack == 0)
                {
                    faces[(int)face].X = 0;
                    faces[(int)face].Y = 1;
                    faces[(int)face].Z = slice;
                    face++;
                }
                else
                {
                    faces[(int)face].X = SphereVertexIndex(slices, slice - 1, stack - 1);
                    faces[(int)face].Y = SphereVertexIndex(slices, 0, stack - 1);
                    faces[(int)face].Z = SphereVertexIndex(slices, slice - 1, stack);
                    face++;

                    faces[(int)face].X = SphereVertexIndex(slices, 0, stack - 1);
                    faces[(int)face].Y = SphereVertexIndex(slices, 0, stack);
                    faces[(int)face].Z = SphereVertexIndex(slices, slice - 1, stack);
                    face++;
                }
            }

            pPos[vertex].X = 0.0f;
            pPos[vertex].Y = 0.0f;
            pPos[vertex].Z = -radius;
            pNor[vertex].X = 0.0f;
            pNor[vertex].Y = 0.0f;
            pNor[vertex].Z = -1.0f;
            pUV[vertex].X = 0;
            pUV[vertex].Y = 0;

            /* bottom stack is triangle fan */
            for (slice = 1; slice < slices; slice++)
            {
                faces[(int)face].X = SphereVertexIndex(slices, slice - 1, stack - 1);
                faces[(int)face].Y = SphereVertexIndex(slices, slice, stack - 1);
                faces[(int)face].Z = vertex;
                face++;
            }

            faces[(int)face].X = SphereVertexIndex(slices, slice - 1, stack - 1);
            faces[(int)face].Y = SphereVertexIndex(slices, 0, stack - 1);
            faces[(int)face].Z = vertex;

            //=======================
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakeSphere", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Normal) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
                (1 << (int)NxRHI.EVertexStreamType.VST_UV));

            if (number_of_faces * 3 >= UInt16.MaxValue)
                builder.Init(streams, true, 1);
            else
                builder.Init(streams, false, 1);

            var aabb = new BoundingBox(-radius, -radius, -radius, radius, radius, radius);
            builder.SetAABB(ref aabb);

            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = number_of_faces;

            for (int i = 0; i < number_of_vertices; i++)
            {
                builder.AddVertex(in pPos[i], in pNor[i], in pUV[i], color);
            }

            for (int i = 0; i < number_of_faces; i++)
            {
                builder.AddTriangle(faces[i].X, faces[i].Y, faces[i].Z);
            }

            builder.PushAtomLOD(0, &dpDesc);
            return meshBuilder;
        }
        #endregion

        #region Cyliner
        public class UMakeCylinderParameter
        {
            public float Radius1 { get; set; } = 1.0f;
            public float Radius2 { get; set; } = 1.0f;
            public float Length { get; set; } = 1.0f;
            public uint Slices { get; set; } = 30;
            public uint Stacks { get; set; } = 30;
            [EGui.Controls.PropertyGrid.Color4PickerEditor()]
            public Vector4 Color { get; set; } = Vector4.One;
        }
        public static unsafe UMeshDataProvider MakeCylinder(float radius1, float radius2, float length, uint slices, uint stacks, uint color)
        {
            uint number_of_vertices, number_of_faces;
            
            float theta_step, theta_start;            
            float delta_radius, radius, radius_step;
            float z, z_step, z_normal;
            uint vertex, face, slice, stack;

            if (radius1< 0.0f || radius2< 0.0f || length< 0.0f || slices< 2 || stacks< 1)
            {
                return null;
            }

            number_of_vertices = 2 + (slices* (3 + stacks));
            number_of_faces = 2 * slices + stacks* (2 * slices);
 
            /* theta = angle on xy plane wrt x axis */
            theta_step = -2.0f * (float)Math.PI / slices;
            theta_start = (float)Math.PI / 2.0f;

            var theta = new Vector2[slices];
            SphereBuildSinCos(theta, theta_start, theta_step);

            vertex = 0;
            face = 0;

            delta_radius = radius1 - radius2;
            radius = radius1;
            radius_step = delta_radius / stacks;

            z = -length / 2;
            z_step = length / stacks;
            z_normal = delta_radius / length;
            if (z_normal == Single.NaN)
            {
                z_normal = 0.0f;
            }

            var faces = new Vector3ui[(int)number_of_faces];
            var pPos = new Vector3[(int)number_of_vertices];
            var pNor = new Vector3[(int)number_of_vertices];
            var pUV = new Vector2[(int)number_of_vertices];

            pNor[vertex].X = 0.0f;
            pNor[vertex].Y = 0.0f;
            pNor[vertex].Z = -1.0f;
            pPos[vertex].X = 0.0f;
            pPos[vertex].Y = 0.0f;
            pPos[vertex].Z = z;
            pUV[vertex].X = 0;
            pUV[vertex].Y = 0;
            vertex++;

            for (slice = 0; slice < slices; slice++, vertex++)
            {
                pNor[vertex].X = 0.0f;
                pNor[vertex].Y = 0.0f;
                pNor[vertex].Z = -1.0f;
                pPos[vertex].X = radius * theta[slice].Y;
                pPos[vertex].Y = radius * theta[slice].X;
                pPos[vertex].Z = z;
                pUV[vertex].X = theta[slice].X;
                pUV[vertex].Y = theta[slice].Y;

                if (slice > 0)
                {
                    faces[(int)face].X = 0;
                    faces[(int)face].Y = slice;
                    faces[(int)(face++)].Z = slice + 1;
                }
            }

            faces[(int)face].X = 0;
            faces[(int)face].Y = slice;
            faces[(int)(face++)].Z = 1;

            for (stack = 1; stack <= stacks + 1; stack++)
            {
                for (slice = 0; slice < slices; slice++, vertex++)
                {
                    pNor[vertex].X = theta[slice].Y;
                    pNor[vertex].Y = theta[slice].X;
                    pNor[vertex].Z = z_normal;
                    pNor[vertex].Normalize();
                    pPos[vertex].X = radius * theta[slice].Y;
                    pPos[vertex].Y = radius * theta[slice].X;
                    pPos[vertex].Z = z;
                    pUV[vertex].X = (float)stack / (float)stacks;
                    pUV[vertex].Y = (float)slice / (float)slices;

                    if (stack > 1 && slice > 0)
                    {
                        faces[(int)face].X = SphereVertexIndex(slices, slice - 1, stack - 1);
                        faces[(int)face].Y = SphereVertexIndex(slices, slice - 1, stack);
                        faces[(int)(face++)].Z = SphereVertexIndex(slices, slice, stack - 1);

                        faces[(int)face].X = SphereVertexIndex(slices, slice, stack - 1);
                        faces[(int)face].Y = SphereVertexIndex(slices, slice - 1, stack);
                        faces[(int)(face++)].Z = SphereVertexIndex(slices, slice, stack);
                    }
                }

                if (stack > 1)
                {
                    faces[(int)face].X = SphereVertexIndex(slices, slice - 1, stack - 1);
                    faces[(int)face].Y = SphereVertexIndex(slices, slice - 1, stack);
                    faces[(int)(face++)].Z = SphereVertexIndex(slices, 0, stack - 1);

                    faces[(int)face].X = SphereVertexIndex(slices, 0, stack - 1);
                    faces[(int)face].Y = SphereVertexIndex(slices, slice - 1, stack);
                    faces[(int)(face++)].Z = SphereVertexIndex(slices, 0, stack);
                }

                if (stack < stacks + 1)
                {
                    z += z_step;
                    radius -= radius_step;
                }
            }

            for (slice = 0; slice < slices; slice++, vertex++)
            {
                pNor[vertex].X = 0.0f;
                pNor[vertex].Y = 0.0f;
                pNor[vertex].Z = 1.0f;
                pPos[vertex].X = radius * theta[slice].Y;
                pPos[vertex].Y = radius * theta[slice].X;
                pPos[vertex].Z = z;
                pUV[vertex].X = theta[slice].Y;
                pUV[vertex].Y = theta[slice].X;

                if (slice > 0)
                {
                    faces[(int)face].X = SphereVertexIndex(slices, slice - 1, stack);
                    faces[(int)face].Y = number_of_vertices - 1;
                    faces[(int)(face++)].Z = SphereVertexIndex(slices, slice, stack);
                }
            }

            //vertex = number_of_vertices - 1;
            pPos[vertex].X = 0.0f;
            pPos[vertex].Y = 0.0f;
            pPos[vertex].Z = length / 2;//z;
            pNor[vertex].X = 0.0f;
            pNor[vertex].Y = 0.0f;
            pNor[vertex].Z = 1.0f;
            pUV[vertex].X = 0;
            pUV[vertex].Y = 0;

            faces[(int)face].X = SphereVertexIndex(slices, slice - 1, stack);
            faces[(int)face].Y = number_of_vertices - 1;
            faces[(int)face].Z = SphereVertexIndex(slices, 0, stack);

            //=======================
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakeCylinder", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Normal) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
                (1 << (int)NxRHI.EVertexStreamType.VST_UV));

            if (number_of_faces * 3 >= UInt16.MaxValue)
                builder.Init(streams, true, 1);
            else
                builder.Init(streams, false, 1);

            float mr = Math.Max(radius1, radius2);
            
            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = number_of_faces;

            var aabb = new BoundingBox();
            aabb.InitEmptyBox();
            for (int i = 0; i < number_of_vertices; i++)
            {
                var save = pPos[i].Z;
                pPos[i].Z = pPos[i].Y;
                pPos[i].Y = save;

                aabb.Merge(in pPos[i]);

                save = pNor[i].Z;
                pNor[i].Z = pNor[i].Y;
                pNor[i].Y = save;
                builder.AddVertex(in pPos[i], in pNor[i], in pUV[i], color);
            }
            builder.SetAABB(ref aabb);

            for (int i = 0; i < number_of_faces; i++)
            {
                //builder.AddTriangle(faces[i].x, faces[i].y, faces[i].z);
                builder.AddTriangle(faces[i].X, faces[i].Z, faces[i].Y);
            }

            builder.PushAtomLOD(0, &dpDesc);
            return meshBuilder;
        }
        #endregion

        #region Torus
        public class UMakeTorusParameter
        {
            public float InnerRadius { get; set; } = 0.5f;
            public float OutRadius2 { get; set; } = 1.0f;
            public uint Slices { get; set; } = 30;
            public uint Rings { get; set; } = 30;
            [EGui.Controls.PropertyGrid.Color4PickerEditor()]
            public Vector4 Color { get; set; } = Vector4.One;
        }
        public static unsafe UMeshDataProvider MakeTorus(float innerradius, float outerradius, uint sides, uint rings, uint color = 0xffffffff)
        {
            float phi, phi_step, sin_phi, cos_phi;
            float theta, theta_step, sin_theta, cos_theta;
            uint i, j, numvert, numfaces;

            numvert = sides * rings;
            numfaces = numvert * 2;

            var faces = new Vector3ui[(int)numfaces];
            var pPos = new Vector3[(int)numvert];
            var pNor = new Vector3[(int)numvert];
            var pUV = new Vector2[(int)numvert];

            if (innerradius < 0.0f || outerradius < 0.0f || sides < 3 || rings < 3)
            {
                return null;
            }

            phi_step = (float)Math.PI / sides * 2.0f;
            theta_step = (float)Math.PI / rings * -2.0f;

            theta = 0.0f;

            for (i = 0; i < rings; ++i)
            {
                phi = 0.0f;

                sin_theta = (float)Math.Sin(theta);
                cos_theta = (float)Math.Cos(theta);

                for (j = 0; j < sides; ++j)
                {
                    sin_phi = (float)Math.Sin(phi);
                    cos_phi = (float)Math.Cos(phi);

                    pPos[i * sides + j].X = (innerradius * cos_phi + outerradius) * cos_theta;
                    pPos[i * sides + j].Y = (innerradius * cos_phi + outerradius) * sin_theta;
                    pPos[i * sides + j].Z = innerradius * sin_phi;
                    pNor[i * sides + j].X = cos_phi * cos_theta;
                    pNor[i * sides + j].Y = cos_phi * sin_theta;
                    pNor[i * sides + j].Z = sin_phi;

                    phi += phi_step;
                }

                theta += theta_step;
            }

            for (i = 0; i < numfaces - sides * 2; ++i)
            {
                faces[(int)i].X = ((i % 2)!=0) ? i / 2 + sides : i / 2;
                faces[(int)i].Y = (((i / 2 + 1) % sides)!=0) ? i / 2 + 1 : i / 2 + 1 - sides;
                faces[(int)i].Z = (((i + 1) % (sides * 2))!=0) ? (i + 1) / 2 + sides : (i + 1) / 2;
            }

            for (j = 0; i < numfaces; ++i, ++j)
            {
                faces[(int)i].X = ((i % 2)!=0) ? j / 2 : i / 2;
                faces[(int)i].Y = (((i / 2 + 1) % sides)!=0) ? i / 2 + 1 : i / 2 + 1 - sides;
                faces[(int)i].Z = i == numfaces - 1 ? 0 : (j + 1) / 2;
            }

            //=======================
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakeTorus", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Normal) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
                (1 << (int)NxRHI.EVertexStreamType.VST_UV));

            if (numfaces * 3 >= UInt16.MaxValue)
                builder.Init(streams, true, 1);
            else
                builder.Init(streams, false, 1);

            var h = outerradius - innerradius;
            var aabb = new BoundingBox(-outerradius, -outerradius, -h, outerradius, outerradius, h);
            builder.SetAABB(ref aabb);

            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = numfaces;

            for (i = 0; i < numvert; i++)
            {
                builder.AddVertex(in pPos[i], in pNor[i], in pUV[i], color);
            }

            for (i = 0; i < numfaces; i++)
            {
                builder.AddTriangle(faces[i].X, faces[i].Y, faces[i].Z);
            }

            builder.PushAtomLOD(0, &dpDesc);
            return meshBuilder;
        }
        #endregion

        #region Capsule
        public enum ECapsuleUvProfile
        {
            Aspect,
            Uniform,
            Fixed,
        }
        public class MakeCapsuleParameter
        {
            public float Radius { get; set; } = 0.5f;
            public float Depth { get; set; } = 1.0f;
            public uint Latitudes { get; set; } = 10;
            public uint Longitudes { get; set; } = 10;
            public uint Rings { get; set; } = 100;
            public uint Slices { get; set; } = 30;
            public Graphics.Mesh.UMeshDataProvider.ECapsuleUvProfile UvProfile { get; set; } = ECapsuleUvProfile.Aspect;

            [EGui.Controls.PropertyGrid.Color4PickerEditor()]
            public Vector4 Color { get; set; } = Vector4.One;
        }
        public static unsafe UMeshDataProvider MakeCapsule(float radius, float depth, int latitudes, int longitudes, int rings, ECapsuleUvProfile profile, uint color = 0xffffffff)
        {//https://gist.github.com/behreajj/d84525c0e1a0738c4de2c00a411e4b73#file-capsulemaker-cs
            bool calcMiddle = rings > 0;
            int halfLats = latitudes / 2;
            int halfLatsn1 = halfLats - 1;
            int halfLatsn2 = halfLats - 2;
            int ringsp1 = rings + 1;
            int lonsp1 = longitudes + 1;
            float halfDepth = depth * 0.5f;
            float summit = halfDepth + radius;

            // Vertex index offsets.
            int vertOffsetNorthHemi = longitudes;
            int vertOffsetNorthEquator = vertOffsetNorthHemi + lonsp1 * halfLatsn1;
            int vertOffsetCylinder = vertOffsetNorthEquator + lonsp1;
            int vertOffsetSouthEquator = calcMiddle ?
                vertOffsetCylinder + lonsp1 * rings :
                vertOffsetCylinder;
            int vertOffsetSouthHemi = vertOffsetSouthEquator + lonsp1;
            int vertOffsetSouthPolar = vertOffsetSouthHemi + lonsp1 * halfLatsn2;
            int vertOffsetSouthCap = vertOffsetSouthPolar + lonsp1;

            // Initialize arrays.
            int vertLen = vertOffsetSouthCap + longitudes;
            Vector3[] vs = new Vector3[vertLen];
            Vector2[] vts = new Vector2[vertLen];
            Vector3[] vns = new Vector3[vertLen];

            float toTheta = 2.0f * (float)Math.PI / longitudes;
            float toPhi = (float)Math.PI / latitudes;
            float toTexHorizontal = 1.0f / longitudes;
            float toTexVertical = 1.0f / halfLats;

            // Calculate positions for texture coordinates vertical.
            float vtAspectRatio = 1.0f;
            switch (profile)
            {
                case ECapsuleUvProfile.Aspect:
                    vtAspectRatio = radius / (depth + radius + radius);
                    break;

                case ECapsuleUvProfile.Uniform:
                    vtAspectRatio = (float)halfLats / (ringsp1 + latitudes);
                    break;

                case ECapsuleUvProfile.Fixed:
                default:
                    vtAspectRatio = 1.0f / 3.0f;
                    break;
            }

            float vtAspectNorth = 1.0f - vtAspectRatio;
            float vtAspectSouth = vtAspectRatio;

            Vector2[] thetaCartesian = new Vector2[longitudes];
            Vector2[] rhoThetaCartesian = new Vector2[longitudes];
            float[] sTextureCache = new float[lonsp1];

            // Polar vertices.
            for (int j = 0; j < longitudes; ++j)
            {
                float jf = j;
                float sTexturePolar = 1.0f - ((jf + 0.5f) * toTexHorizontal);
                float theta = jf * toTheta;

                float cosTheta = (float)Math.Cos(theta);
                float sinTheta = (float)Math.Sin(theta);

                thetaCartesian[j] = new Vector2(cosTheta, sinTheta);
                rhoThetaCartesian[j] = new Vector2(
                    radius * cosTheta,
                    radius * sinTheta);

                // North.
                vs[j] = new Vector3(0.0f, summit, 0.0f);
                vts[j] = new Vector2(sTexturePolar, 1.0f);
                vns[j] = new Vector3(0.0f, 1.0f, 0f);

                // South.
                int idx = vertOffsetSouthCap + j;
                vs[idx] = new Vector3(0.0f, -summit, 0.0f);
                vts[idx] = new Vector2(sTexturePolar, 0.0f);
                vns[idx] = new Vector3(0.0f, -1.0f, 0.0f);
            }

            // Equatorial vertices.
            for (int j = 0; j < lonsp1; ++j)
            {
                float sTexture = 1.0f - j * toTexHorizontal;
                sTextureCache[j] = sTexture;

                // Wrap to first element upon reaching last.
                int jMod = j % longitudes;
                Vector2 tc = thetaCartesian[jMod];
                Vector2 rtc = rhoThetaCartesian[jMod];

                // North equator.
                int idxn = vertOffsetNorthEquator + j;
                vs[idxn] = new Vector3(rtc.X, halfDepth, -rtc.Y);
                vts[idxn] = new Vector2(sTexture, vtAspectNorth);
                vns[idxn] = new Vector3(tc.X, 0.0f, -tc.Y);

                // South equator.
                int idxs = vertOffsetSouthEquator + j;
                vs[idxs] = new Vector3(rtc.X, -halfDepth, -rtc.Y);
                vts[idxs] = new Vector2(sTexture, vtAspectSouth);
                vns[idxs] = new Vector3(tc.X, 0.0f, -tc.Y);
            }

            // Hemisphere vertices.
            for (int i = 0; i < halfLatsn1; ++i)
            {
                float ip1f = i + 1.0f;
                float phi = ip1f * toPhi;

                // For coordinates.
                float cosPhiSouth = (float)Math.Cos(phi);
                float sinPhiSouth = (float)Math.Sin(phi);

                // Symmetrical hemispheres mean cosine and sine only needs
                // to be calculated once.
                float cosPhiNorth = sinPhiSouth;
                float sinPhiNorth = -cosPhiSouth;

                float rhoCosPhiNorth = radius * cosPhiNorth;
                float rhoSinPhiNorth = radius * sinPhiNorth;
                float zOffsetNorth = halfDepth - rhoSinPhiNorth;

                float rhoCosPhiSouth = radius * cosPhiSouth;
                float rhoSinPhiSouth = radius * sinPhiSouth;
                float zOffsetSouth = -halfDepth - rhoSinPhiSouth;

                // For texture coordinates.
                float tTexFac = ip1f * toTexVertical;
                float cmplTexFac = 1.0f - tTexFac;
                float tTexNorth = cmplTexFac + vtAspectNorth * tTexFac;
                float tTexSouth = cmplTexFac * vtAspectSouth;

                int iLonsp1 = i * lonsp1;
                int vertCurrLatNorth = vertOffsetNorthHemi + iLonsp1;
                int vertCurrLatSouth = vertOffsetSouthHemi + iLonsp1;

                for (int j = 0; j < lonsp1; ++j)
                {
                    int jMod = j % longitudes;

                    float sTexture = sTextureCache[j];
                    Vector2 tc = thetaCartesian[jMod];

                    // North hemisphere.
                    int idxn = vertCurrLatNorth + j;
                    vs[idxn] = new Vector3(
                        rhoCosPhiNorth * tc.X,
                        zOffsetNorth,
                        -rhoCosPhiNorth * tc.Y);
                    vts[idxn] = new Vector2(sTexture, tTexNorth);
                    vns[idxn] = new Vector3(
                        cosPhiNorth * tc.X,
                        -sinPhiNorth,
                        -cosPhiNorth * tc.Y);

                    // South hemisphere.
                    int idxs = vertCurrLatSouth + j;
                    vs[idxs] = new Vector3(
                        rhoCosPhiSouth * tc.X,
                        zOffsetSouth,
                        -rhoCosPhiSouth * tc.Y);
                    vts[idxs] = new Vector2(sTexture, tTexSouth);
                    vns[idxs] = new Vector3(
                        cosPhiSouth * tc.X,
                        -sinPhiSouth,
                        -cosPhiSouth * tc.Y);
                }
            }

            // Cylinder vertices.
            if (calcMiddle)
            {
                // Exclude both origin and destination edges
                // (North and South equators) from the interpolation.
                float toFac = 1.0f / ringsp1;
                int idxCylLat = vertOffsetCylinder;

                for (int h = 1; h < ringsp1; ++h)
                {
                    float fac = h * toFac;
                    float cmplFac = 1.0f - fac;
                    float tTexture = cmplFac * vtAspectNorth + fac * vtAspectSouth;
                    float z = halfDepth - depth * fac;

                    for (int j = 0; j < lonsp1; ++j)
                    {
                        int jMod = j % longitudes;
                        Vector2 tc = thetaCartesian[jMod];
                        Vector2 rtc = rhoThetaCartesian[jMod];
                        float sTexture = sTextureCache[j];

                        vs[idxCylLat] = new Vector3(rtc.X, z, -rtc.Y);
                        vts[idxCylLat] = new Vector2(sTexture, tTexture);
                        vns[idxCylLat] = new Vector3(tc.X, 0.0f, -tc.Y);

                        ++idxCylLat;
                    }
                }
            }

            // Triangle indices.
            // Stride is 3 for polar triangles;
            // stride is 6 for two triangles forming a quad.
            int lons3 = longitudes * 3;
            int lons6 = longitudes * 6;
            int hemiLons = halfLatsn1 * lons6;

            int triOffsetNorthHemi = lons3;
            int triOffsetCylinder = triOffsetNorthHemi + hemiLons;
            int triOffsetSouthHemi = triOffsetCylinder + ringsp1 * lons6;
            int triOffsetSouthCap = triOffsetSouthHemi + hemiLons;

            int fsLen = triOffsetSouthCap + lons3;
            int[] tris = new int[fsLen];

            // Polar caps.
            for (int i = 0, k = 0, m = triOffsetSouthCap; i < longitudes; ++i, k += 3, m += 3)
            {
                // North.
                tris[k] = i;
                tris[k + 1] = vertOffsetNorthHemi + i;
                tris[k + 2] = vertOffsetNorthHemi + i + 1;

                // South.
                tris[m] = vertOffsetSouthCap + i;
                tris[m + 1] = vertOffsetSouthPolar + i + 1;
                tris[m + 2] = vertOffsetSouthPolar + i;
            }

            // Hemispheres.
            for (int i = 0, k = triOffsetNorthHemi, m = triOffsetSouthHemi; i < halfLatsn1; ++i)
            {
                int iLonsp1 = i * lonsp1;

                int vertCurrLatNorth = vertOffsetNorthHemi + iLonsp1;
                int vertNextLatNorth = vertCurrLatNorth + lonsp1;

                int vertCurrLatSouth = vertOffsetSouthEquator + iLonsp1;
                int vertNextLatSouth = vertCurrLatSouth + lonsp1;

                for (int j = 0; j < longitudes; ++j, k += 6, m += 6)
                {
                    // North.
                    int north00 = vertCurrLatNorth + j;
                    int north01 = vertNextLatNorth + j;
                    int north11 = vertNextLatNorth + j + 1;
                    int north10 = vertCurrLatNorth + j + 1;

                    tris[k] = north00;
                    tris[k + 1] = north11;
                    tris[k + 2] = north10;

                    tris[k + 3] = north00;
                    tris[k + 4] = north01;
                    tris[k + 5] = north11;

                    // South.
                    int south00 = vertCurrLatSouth + j;
                    int south01 = vertNextLatSouth + j;
                    int south11 = vertNextLatSouth + j + 1;
                    int south10 = vertCurrLatSouth + j + 1;

                    tris[m] = south00;
                    tris[m + 1] = south11;
                    tris[m + 2] = south10;

                    tris[m + 3] = south00;
                    tris[m + 4] = south01;
                    tris[m + 5] = south11;
                }
            }

            // Cylinder.
            for (int i = 0, k = triOffsetCylinder; i < ringsp1; ++i)
            {
                int vertCurrLat = vertOffsetNorthEquator + i * lonsp1;
                int vertNextLat = vertCurrLat + lonsp1;

                for (int j = 0; j < longitudes; ++j, k += 6)
                {
                    int cy00 = vertCurrLat + j;
                    int cy01 = vertNextLat + j;
                    int cy11 = vertNextLat + j + 1;
                    int cy10 = vertCurrLat + j + 1;

                    tris[k] = cy00;
                    tris[k + 1] = cy11;
                    tris[k + 2] = cy10;

                    tris[k + 3] = cy00;
                    tris[k + 4] = cy01;
                    tris[k + 5] = cy11;
                }
            }

            //=======================
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakeCapsule", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Normal) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
                (1 << (int)NxRHI.EVertexStreamType.VST_UV));

            if (tris.Length >= UInt16.MaxValue)
                builder.Init(streams, true, 1);
            else
                builder.Init(streams, false, 1);

            
            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = (uint)tris.Length / 3;

            var aabb = new BoundingBox();
            aabb.InitEmptyBox();
            for (int i = 0; i < vs.Length; i++)
            {
                builder.AddVertex(in vs[i], in vns[i], in vts[i], color);
                aabb.Merge(in vs[i]);
            }
            builder.SetAABB(ref aabb);

            for (int i = 0; i < dpDesc.NumPrimitives; i++)
            {
                builder.AddTriangle((uint)tris[i * 3 + 0], (uint)tris[i * 3 + 1], (uint)tris[i * 3 + 2]);
            }

            builder.PushAtomLOD(0, &dpDesc);
            return meshBuilder;
        }
        #endregion

        #region Plane
        public static unsafe UMeshDataProvider MakeGridIndices(ushort NumX, ushort NumZ)
        {
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakeGridIndices", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = 0;
            builder.Init(streams, true, 1);

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

            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = (uint)indexer.Count / 3;
            builder.PushAtomLOD(0, in dpDesc);

            return meshBuilder;
        }
        
        public static unsafe UMeshDataProvider MakeGridForTerrain(ushort NumX, ushort NumZ)
        {
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakeGridForTerrain", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)(1 << (int)NxRHI.EVertexStreamType.VST_Position);
            builder.Init(streams, true, 1);
            for (ushort i = 0; i < NumZ + 1; i++)
            {
                for (ushort j = 0; j < NumX + 1; j++)
                {
                    Vector3 pos = new Vector3(0);
                    pos.X = ((float)j) / ((float)NumX);
                    pos.Z = ((float)i) / ((float)NumZ);
                    builder.AddVertex(in pos, in Vector3.Zero, in Vector2.Zero, 0xFFFFFFFF);
                }
            }
            
            for (ushort i = 0; i < NumZ; i++)
            {
                for (ushort j = 0; j < NumX; j++)
                {
                    uint a, b, c;                    
                    a = CalcVertexIndex(i, j, NumX);
                    b = CalcVertexIndex((uint)(i + 1), j, NumX);
                    c = CalcVertexIndex((uint)(i + 1), (uint)(j + 1), NumX);
                    
                    builder.AddTriangle(a, b, c);

                    a = CalcVertexIndex(i, j, NumX);
                    b = CalcVertexIndex((uint)(i + 1), (uint)(j + 1), NumX);
                    c = CalcVertexIndex(i, (uint)(j + 1), NumX);
                    
                    builder.AddTriangle(a, b, c);
                }
            }

            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = (uint)((NumZ) * (NumX) * 2);
            builder.PushAtomLOD(0, in dpDesc);

            return meshBuilder;
        }

        public static float Lerp(float start, float end, float factor)
        {
            return start + factor * (end - start);
        }
        public static UMeshDataProvider MakeGridPlane(NxRHI.UGpuDevice rc, Vector2 uvMin, Vector2 uvMax, UInt32 tileCount = 10)
        {//reference:DrawGridline
            UMeshDataProvider meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakeGridPlane", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Normal) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
                (1 << (int)NxRHI.EVertexStreamType.VST_UV) |
                (1 << (int)NxRHI.EVertexStreamType.VST_LightMap));
            builder.Init(streams, true, 1);

            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = tileCount * tileCount * 2;
            builder.PushAtomLOD(0, in dpDesc);

            using (var posArray = Support.UNativeArray<Vector3>.CreateInstance())
            using (var normalArray = Support.UNativeArray<Vector3>.CreateInstance())
            using (var tangentArray = Support.UNativeArray<Vector4>.CreateInstance())
            using (var lightmapUVArray = Support.UNativeArray<Vector4>.CreateInstance())
            using (var uvArray = Support.UNativeArray<Vector2>.CreateInstance())
            using (var indexArray = Support.UNativeArray<UInt32>.CreateInstance())
            {
                // -> tileCount * tileCount * 2 triangles
                float Step = 2.0f / tileCount;
                for (Int32 y = 0; y < tileCount; ++y)
                {
                    // implemented this way to avoid cracks, could be optimized
                    float z0 = y * Step - 1.0f;
                    float z1 = (y + 1) * Step - 1.0f;

                    //float V0 = Lerp(uvMin.Y, uvMax.Y, z0 * 0.5f + 0.5f);
                    //float V1 = Lerp(uvMin.Y, uvMax.Y, z1 * 0.5f + 0.5f);
                    float V0 = z0 * 0.5f + 0.5f;
                    float V1 = z1 * 0.5f + 0.5f;

                    for (Int32 x = 0; x < tileCount; ++x)
                    {
                        // implemented this way to avoid cracks, could be optimized
                        float x0 = x * Step - 1.0f;
                        float x1 = (x + 1) * Step - 1.0f;

                        //float U0 = Lerp(uvMin.X, uvMax.X, x0 * 0.5f + 0.5f);
                        //float U1 = Lerp(uvMin.X, uvMax.X, x1 * 0.5f + 0.5f);
                        float U0 = x0 * 0.5f + 0.5f;
                        float U1 = x1 * 0.5f + 0.5f;

                        var lightmapUV = new Quaternion();
                        lightmapUV.X = U0;
                        lightmapUV.Y = U1;
                        lightmapUV.Z = V0;
                        lightmapUV.W = V1;

                        // Calculate verts for a face pointing down Z
                        var pos = new Vector3(x0, 0, z0);
                        var nor = new Vector3(0, 1, 0);
                        //var uv = new Vector2(U0, V0);
                        var uv = new Vector2(0, 0);

                        builder.AddVertex(in pos, in nor, in uv, in lightmapUV, 0xFFFFFFFF);
                        pos = new Vector3(x0, 0, z1);
                        nor = new Vector3(0, 1, 0);
                        //uv = new Vector2(U0, V1);
                        uv = new Vector2(1, 0);
                        builder.AddVertex(in pos, in nor, in uv, in lightmapUV, 0xFFFFFFFF);
                        pos = new Vector3(x1, 0, z1);
                        nor = new Vector3(0, 1, 0);
                        //uv = new Vector2(U1, V1);
                        uv = new Vector2(2, 0);
                        builder.AddVertex(in pos, in nor, in uv, in lightmapUV, 0xFFFFFFFF);
                        pos = new Vector3(x1, 0, z0);
                        nor = new Vector3(0, 1, 0);
                        //uv = new Vector2(U1, V0);
                        uv = new Vector2(3, 0);
                        builder.AddVertex(in pos, in nor, in uv, in lightmapUV, 0xFFFFFFFF);

                        UInt32 Index = (UInt32)((x + y * tileCount) * 4);
                        builder.AddTriangle(Index + 0, Index + 1, Index + 2);
                        builder.AddTriangle(Index + 0, Index + 2, Index + 3);
                    }
                }
            }
            return meshBuilder;
        }

        public static unsafe UMeshDataProvider MakePlane(float width, float length)
        {
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakePlane", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Normal) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
                (1 << (int)NxRHI.EVertexStreamType.VST_UV));
            builder.Init(streams, false, 1);

            var halfWidth = width * 0.5f;
            var halfLength = length * 0.5f;
            var aabb = new BoundingBox(-halfWidth, -0.0001f, -halfLength, halfWidth, 0.0001f, halfLength);
            builder.SetAABB(ref aabb);

            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = 2;

            var pPos = stackalloc Vector3[4];
            // 创建Position
            {
                pPos[0].SetValue(-halfWidth, 0, -halfLength);
                pPos[1].SetValue(-halfWidth, 0, halfLength);
                pPos[2].SetValue(halfWidth, 0, halfLength);
                pPos[3].SetValue(halfWidth, 0, -halfLength);
            }
            var vNor = stackalloc Vector3[4];
            // 创建Normal
            for(int i=0; i<4; i++)
            {
                vNor[i] = Vector3.Up;
            }
            var vUV = stackalloc Vector2[4];
            // 创建UV
            {
                vUV[0] = new Vector2(0, 1);
                vUV[1] = new Vector2(0, 0);
                vUV[2] = new Vector2(1, 0);
                vUV[3] = new Vector2(1, 1);
            }
            // 索引
            UInt16[] pIndex = new UInt16[6];
            pIndex[0] = 0;
            pIndex[1] = 1;
            pIndex[2] = 2;
            pIndex[3] = 0;
            pIndex[4] = 2;
            pIndex[5] = 3;
            for(int i=0; i<4; i++)
            {
                builder.AddVertex(&pPos[i], &vNor[i], &vUV[i], 0xFFFFFFFF);
            }
            for(int i=0; i<dpDesc.NumPrimitives; i++)
            {
                builder.AddTriangle(pIndex[i * 3], pIndex[i * 3 + 1], pIndex[i * 3 + 2]);
            }
            builder.PushAtomLOD(0, &dpDesc);
            return meshBuilder;
        }
        #endregion

        #region line
        public static unsafe UMeshDataProvider MakeLine(in Vector3 from, in Vector3 to, uint color)
        {
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakeLine", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
               (1 << (int)NxRHI.EVertexStreamType.VST_Normal) |
               (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
               (1 << (int)NxRHI.EVertexStreamType.VST_UV));
            builder.Init(streams, false, 1);

            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.PrimitiveType = NxRHI.EPrimitiveType.EPT_LineList;
            dpDesc.NumPrimitives = 0;
            dpDesc.StartIndex = 0xffffffff;

            var aabb = new BoundingBox();
            aabb.InitEmptyBox();
            aabb.Merge(in from);
            aabb.Merge(in to);
            builder.AddVertex(in from, in Vector3.UnitX, in Vector2.Zero, color);
            builder.AddVertex(in to, in Vector3.UnitX, in Vector2.Zero, color);
            dpDesc.NumPrimitives++;

            builder.PushAtomLOD(0, &dpDesc);
            builder.SetAABB(ref aabb);
            return meshBuilder;
        }
        public static unsafe UMeshDataProvider MakeBezier3DSpline(UBezier3DSpline spline, uint color)
        {
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@MakeBezier3DSpline", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Normal) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
                (1 << (int)NxRHI.EVertexStreamType.VST_UV));
            builder.Init(streams, false, 1);

            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.PrimitiveType = NxRHI.EPrimitiveType.EPT_LineStrip;
            dpDesc.NumPrimitives = 0;
            dpDesc.StartIndex = 0xffffffff;

            var aabb = new BoundingBox();
            aabb.InitEmptyBox();
            foreach (var i in spline.Curves)
            {
                if (i.Start == i.End)
                    continue;

                var cache = i.GetPointCache(spline.Segments);
                aabb = BoundingBox.Merge(in aabb, in cache.AABB);

                for (int j = 0; j < cache.CachedPoints.Length; j++)
                {
                    ref var start = ref cache.CachedPoints[j].Position;
                    //ref var end = ref cache.CachedPoints[j + 1].Position;

                    builder.AddVertex(in start, in Vector3.UnitX, in Vector2.Zero, color);
                    dpDesc.NumPrimitives++;
                }
            }
            dpDesc.NumPrimitives--;
            builder.PushAtomLOD(0, &dpDesc);
            builder.SetAABB(ref aabb);
            return meshBuilder;
        }
        #endregion
    }
}
