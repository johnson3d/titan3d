using Assimp;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace EngineNS.DistanceField
{
    public class FSparseMeshDistanceFieldAsyncTask
    {
	    public FSparseMeshDistanceFieldAsyncTask(
            DistanceFieldConfig InSdfConfig,
            DistanceField.UEmbreeManager InEmbreeManager,
            UEmbreeScene InEmbreeScene,
            List<Vector3> InSampleDirections,
            float InLocalSpaceTraceDistance,
            BoundingBox InVolumeBounds,
            float InLocalToVolumeScale,
            Vector2 InDistanceFieldToVolumeScaleBias,
            Vector3i InBrickCoordinate,
            Vector3i InIndirectionSize,
            bool bInUsePointQuery,
            bool bInUseFloatFormat)
        {
            SdfConfig = InSdfConfig;
            EmbreeManager = InEmbreeManager;
            EmbreeScene = InEmbreeScene;
            SampleDirections = InSampleDirections;
            LocalSpaceTraceDistance = InLocalSpaceTraceDistance;
            VolumeBounds = InVolumeBounds;
            LocalToVolumeScale = InLocalToVolumeScale;
            DistanceFieldToVolumeScaleBias = InDistanceFieldToVolumeScaleBias;
            BrickCoordinate = InBrickCoordinate;
            IndirectionSize = InIndirectionSize;
            bUsePointQuery = bInUsePointQuery;
            bUseFloatFormat = bInUseFloatFormat;

            BrickMaxDistance = Byte.MinValue;
            BrickMinDistance = Byte.MaxValue;
            fBrickMaxDistance = Half.MinValue;
            fBrickMinDistance = Half.MaxValue;
        }

        // Readonly inputs
        DistanceFieldConfig SdfConfig;
        DistanceField.UEmbreeManager EmbreeManager;
        UEmbreeScene EmbreeScene;
    	List<Vector3> SampleDirections;
        float LocalSpaceTraceDistance;
        BoundingBox VolumeBounds;
        float LocalToVolumeScale;
        Vector2 DistanceFieldToVolumeScaleBias;
        public Vector3i BrickCoordinate;
        Vector3i IndirectionSize;
        bool bUsePointQuery;

        bool bUseFloatFormat;

        // Output
        public Byte BrickMaxDistance;
        public Byte BrickMinDistance;
        public List<Byte> DistanceFieldVolume;
        public Half fBrickMaxDistance;
        public Half fBrickMinDistance;
        public List<Half> fDistanceFieldVolume;

        public bool IsValid()
        {
            if(bUseFloatFormat)
            {
                if (fBrickMinDistance < Half.MaxValue && fBrickMaxDistance > Half.MinValue)
                    return true;
            }
            else
            {
                if (BrickMinDistance < Byte.MaxValue && BrickMaxDistance > Byte.MinValue)
                    return true;
            }
            return false;
        }

        [ThreadStatic]
        private static Profiler.TimeScope mScopeSparseMeshSDF;
        private static Profiler.TimeScope ScopeSparseMeshSDF
        {
            get
            {
                if (mScopeSparseMeshSDF == null)
                    mScopeSparseMeshSDF = new Profiler.TimeScope(typeof(FSparseMeshDistanceFieldAsyncTask), nameof(DoWork));
                return mScopeSparseMeshSDF;
            }
        } 
        public void DoWork()
        {
            using (new Profiler.TimeScopeHelper(ScopeSparseMeshSDF))
            {
                Vector3 IndirectionVoxelSize = VolumeBounds.GetSize() / new Vector3(IndirectionSize);
                Vector3 DistanceFieldVoxelSize = IndirectionVoxelSize / new Vector3(SdfConfig.UniqueDataBrickSize);
                Vector3 BrickMinPosition = VolumeBounds.Minimum + new Vector3(BrickCoordinate) * IndirectionVoxelSize;

                if(bUseFloatFormat)
                {
                    fDistanceFieldVolume = new List<Half>();
                    fDistanceFieldVolume.Resize(SdfConfig.BrickSize * SdfConfig.BrickSize * SdfConfig.BrickSize);
                }
                else
                {
                    DistanceFieldVolume = new List<Byte>();
                    DistanceFieldVolume.Resize(SdfConfig.BrickSize * SdfConfig.BrickSize * SdfConfig.BrickSize);
                }

                for (int YIndex = 0; YIndex < SdfConfig.BrickSize; YIndex++)
                {
                    for (int ZIndex = 0; ZIndex < SdfConfig.BrickSize; ZIndex++)
                    {
                        for (int XIndex = 0; XIndex < SdfConfig.BrickSize; XIndex++)
                        {
                            Vector3 VoxelPosition = new Vector3(XIndex, YIndex, ZIndex) * DistanceFieldVoxelSize + BrickMinPosition;
                            int Index = (ZIndex * SdfConfig.BrickSize * SdfConfig.BrickSize + YIndex * SdfConfig.BrickSize + XIndex);

                            float MinLocalSpaceDistance = LocalSpaceTraceDistance;

                            bool bTraceRays = true;

                            if (bUsePointQuery)
                            {
                                float ClosestDistance = 0.0f;
                                EmbreeManager.EmbreePointQuery(EmbreeScene, VoxelPosition, LocalSpaceTraceDistance, ref bTraceRays, ref ClosestDistance);
                                MinLocalSpaceDistance = MathHelper.Min(MinLocalSpaceDistance, ClosestDistance);
                            }

                            if (bTraceRays)
                            {
                                int Hit = 0;
                                int HitBack = 0;

                                for (int SampleIndex = 0; SampleIndex < SampleDirections.Count; SampleIndex++)
                                {
                                    var UnitRayDirection = SampleDirections[SampleIndex];
                                    const float PullbackEpsilon = 0.0001f;
                                    // Pull back the starting position slightly to make sure we hit a triangle that VoxelPosition is exactly on.  
                                    // This happens a lot with boxes, since we trace from voxel corners.
                                    var StartPosition = VoxelPosition - PullbackEpsilon * LocalSpaceTraceDistance * UnitRayDirection;
                                    var EndPosition = VoxelPosition + UnitRayDirection * LocalSpaceTraceDistance;

                                    bool bIntersectVolume = false;
                                    {
                                        var ray = new Ray(StartPosition, EndPosition - StartPosition);
                                        float t = 0;
                                        if (Ray.Intersects(ray, VolumeBounds, out t) == true)
                                        {
                                            bIntersectVolume = (EndPosition - StartPosition).LengthSquared() >= t * t;
                                        }
                                    }
                                    if (bIntersectVolume)
                                    {
                                        var RayDirection = EndPosition - VoxelPosition;
                                        bool bHit = false;
                                        bool bHitTwoSide = false;
                                        Vector3 HitNormal = Vector3.Zero;
                                        float TFar = 1.0f;
                                        EmbreeManager.EmbreeRayTrace(EmbreeScene, StartPosition, RayDirection, ref bHit, ref bHitTwoSide, ref HitNormal, ref TFar);

                                        if (bHit == true)
                                        {
                                            Hit++;

                                            if (Vector3.Dot(UnitRayDirection, HitNormal) > 0 && !bHitTwoSide)
                                            {
                                                HitBack++;
                                            }

                                            if (!bUsePointQuery)
                                            {
                                                float CurrentDistance = TFar * LocalSpaceTraceDistance;

                                                if (CurrentDistance < MinLocalSpaceDistance)
                                                {
                                                    MinLocalSpaceDistance = CurrentDistance;
                                                }
                                            }
                                        }
                                    }
                                }

                                // Consider this voxel 'inside' an object if we hit a significant number of backfaces
                                if (Hit > 0 && HitBack > .25f * SampleDirections.Count)
                                {
                                    MinLocalSpaceDistance *= -1;
                                }
                            }

                            // Transform to the tracing shader's Volume space
                            float VolumeSpaceDistance = MinLocalSpaceDistance * LocalToVolumeScale;
                            if(bUseFloatFormat)
                            {
                                fDistanceFieldVolume[Index] = new Half(VolumeSpaceDistance);
                                if(bTraceRays==true)
                                {
                                    fBrickMaxDistance = Half.Max(fBrickMaxDistance, fDistanceFieldVolume[Index]);
                                    fBrickMinDistance = Half.Min(fBrickMinDistance, fDistanceFieldVolume[Index]);
                                }
                            }
                            else
                            {
                                // Transform to the Distance Field texture's space
                                float RescaledDistance = (VolumeSpaceDistance - DistanceFieldToVolumeScaleBias.Y) / DistanceFieldToVolumeScaleBias.X;
                                Byte QuantizedDistance = (Byte)Math.Clamp((int)Math.Floor(RescaledDistance * 255.0f + .5f), 0, 255);
                                DistanceFieldVolume[Index] = QuantizedDistance;
                                BrickMaxDistance = Math.Max(BrickMaxDistance, QuantizedDistance);
                                BrickMinDistance = Math.Min(BrickMinDistance, QuantizedDistance);
                            }
                        }
                    }
                }
            }                
        }

    };


    public class UMeshUtilities
    {
        static Vector3 UniformSampleHemisphere(Vector2 Uniforms)
        {
            Uniforms = Uniforms * 2.0f - Vector2.One;

            if (Uniforms == Vector2.Zero)
            {
                return Vector3.Zero;
            }

            float R;
            float Theta;

            if (Math.Abs(Uniforms.X) > Math.Abs(Uniforms.Y))
            {
                R = Uniforms.X;
                Theta = MathHelper.PI / 4 * (Uniforms.Y / Uniforms.X);
            }
            else
            {
                R = Uniforms.Y;
                Theta = MathHelper.PI / 2 - MathHelper.PI / 4 * (Uniforms.X / Uniforms.Y);
            }

            // concentric disk sample
            float U = R * MathHelper.Cos(Theta);
            float V = R * MathHelper.Sin(Theta);
            float R2 = R * R;

            var result = new Vector3(U * MathHelper.Sqrt(2 - R2), 1.0f - R2, V * MathHelper.Sqrt(2 - R2));
            return result;
        }


        public static void GenerateStratifiedUniformHemisphereSamples(int NumSamples, ref List<Vector3> Samples)
        {            
            int NumSamplesDim = (int)Math.Truncate(Math.Sqrt(NumSamples));

            Samples.Capacity = NumSamplesDim * NumSamplesDim;
            var rand = new Random();           

            for (int IndexX = 0; IndexX < NumSamplesDim; IndexX++)
            {
                for (int IndexY = 0; IndexY < NumSamplesDim; IndexY++)
                {
                    float U1 = (float)rand.NextDouble();
                    float U2 = (float)rand.NextDouble();

                    float Fraction1 = (IndexX + U1) / (float)NumSamplesDim;
                    float Fraction2 = (IndexY + U2) / (float)NumSamplesDim;

                    Vector3 Tmp = UniformSampleHemisphere(new Vector2(Fraction1, Fraction2));

                    Samples.Add(Tmp);
                }
            }
        }

        public static void GenerateSignedDistanceFieldVolumeData(string MeshName,
            Graphics.Mesh.UMeshDataProvider meshProvider,
            DistanceFieldConfig sdfConfig,
            float DistanceFieldResolutionScale,
            bool bGenerateAsIfTwoSided,
            ref TtSdfAsset OutData)
        {
            if (DistanceFieldResolutionScale <= 0)
                return;

            var StartTime = Support.Time.GetTickCount();

            var embreeScene = new DistanceField.UEmbreeScene();
            var embreeManager = new DistanceField.UEmbreeManager();
            embreeManager.SetupEmbreeScene(MeshName, meshProvider, DistanceFieldResolutionScale, embreeScene);

            // Whether to use an Embree Point Query to compute the closest unsigned distance.  Rays will only be traced to determine backfaces visible for sign.
            const bool bUsePointQuery = true;

            List<Vector3> SampleDirections = new List<Vector3>();
            {
                int NumVoxelDistanceSamples = bUsePointQuery ? 49 : 576;
                GenerateStratifiedUniformHemisphereSamples(NumVoxelDistanceSamples, ref SampleDirections);
                List<Vector3> OtherHemisphereSamples = new List<Vector3>();
                GenerateStratifiedUniformHemisphereSamples(NumVoxelDistanceSamples, ref OtherHemisphereSamples);

                for (int i = 0; i < OtherHemisphereSamples.Count; i++)
                {
                    var Sample = OtherHemisphereSamples[i];
                    Sample.Y *= -1.0f;
                    SampleDirections.Add(Sample);
                }
            }

            int PerMeshMax = sdfConfig.MaxPerMeshResolution;

            // Meshes with explicit artist-specified scale can go higher
            int MaxNumBlocksOneDim = (int)MathHelper.Min(Math.Round((DistanceFieldResolutionScale <= 1 ? PerMeshMax / 2.0 : PerMeshMax) / sdfConfig.UniqueDataBrickSize), sdfConfig.MaxIndirectionDimension - 1);

            float VoxelDensity = sdfConfig.fDefaultVoxelDensity;

            float NumVoxelsPerLocalSpaceUnit = VoxelDensity * DistanceFieldResolutionScale;

            var LocalSpaceMeshBounds = meshProvider.AABB;
            // Make sure the mesh bounding box has positive extents to handle planes
            {
                var MeshBoundsCenter = LocalSpaceMeshBounds.GetCenter();
                var MeshBoundsExtent = Vector3.Maximize(LocalSpaceMeshBounds.GetExtent(), Vector3.One);
                LocalSpaceMeshBounds = new BoundingBox(MeshBoundsCenter - MeshBoundsExtent, MeshBoundsCenter + MeshBoundsExtent);
            }

            // We sample on voxel corners and use central differencing for gradients, so a box mesh using two-sided materials whose vertices lie on LocalSpaceMeshBounds produces a zero gradient on intersection
            // Expand the mesh bounds by a fraction of a voxel to allow room for a pullback on the hit location for computing the gradient.
            // Only expand for two sided meshes as this adds significant Mesh SDF tracing cost
            //if (embreeScene.bMostlyTwoSided)
            //{
                // TODO
            //}

            // The tracing shader uses a Volume space that is normalized by the maximum extent, to keep Volume space within [-1, 1], we must match that behavior when encoding
            float LocalToVolumeScale = 1.0f / LocalSpaceMeshBounds.GetExtent().GetMaxValue();

            Vector3 DesiredDimensions = LocalSpaceMeshBounds.GetSize() * (NumVoxelsPerLocalSpaceUnit / (float)sdfConfig.UniqueDataBrickSize);
            Vector3i Mip0IndirectionDimensions = new Vector3i(
                MathHelper.Clamp((int)Math.Round(DesiredDimensions.X), 1, MaxNumBlocksOneDim),
                MathHelper.Clamp((int)Math.Round(DesiredDimensions.Y), 1, MaxNumBlocksOneDim),
                MathHelper.Clamp((int)Math.Round(DesiredDimensions.Z), 1, MaxNumBlocksOneDim));

            List<Byte> StreamableMipData = new List<byte>();

            for (int MipIndex = 0; MipIndex < DistanceFieldConfig.NumMips; MipIndex++)
            {
                Vector3i IndirectionDimensions = new Vector3i(
                    (int)MathHelper.DivideAndRoundUp((uint)Mip0IndirectionDimensions.X, (uint)(1 << MipIndex)),
                    (int)MathHelper.DivideAndRoundUp((uint)Mip0IndirectionDimensions.Y, (uint)(1 << MipIndex)),
                    (int)MathHelper.DivideAndRoundUp((uint)Mip0IndirectionDimensions.Z, (uint)(1 << MipIndex)));

                BoundingBox DistanceFieldVolumeBounds = LocalSpaceMeshBounds;
                // Expand to guarantee one voxel border for gradient reconstruction using bilinear filtering
                if (sdfConfig.MeshDistanceFieldObjectBorder != 0)
                {
                    Vector3 TexelObjectSpaceSize = LocalSpaceMeshBounds.GetSize() / new Vector3(IndirectionDimensions * sdfConfig.UniqueDataBrickSize - new Vector3i(2 * sdfConfig.MeshDistanceFieldObjectBorder));
                    DistanceFieldVolumeBounds = BoundingBox.ExpandBy(LocalSpaceMeshBounds, TexelObjectSpaceSize);
                }

                Vector3 IndirectionVoxelSize = DistanceFieldVolumeBounds.GetSize() / new Vector3(IndirectionDimensions);
                float IndirectionVoxelRadius = IndirectionVoxelSize.Length();

                bool bUseSimpleTranceDistance = false;
                float LocalSpaceTraceDistance = 0;
                Vector2 DistanceFieldToVolumeScaleBias = Vector2.Zero;
                if (bUseSimpleTranceDistance)
                {
                    // method 1, Trance Distance: LocalSpaceMeshBounds.ExtentLength
                    LocalSpaceTraceDistance = LocalSpaceMeshBounds.GetExtent().Length();
                    float MaxDistanceForEncoding = LocalSpaceMeshBounds.GetExtent().GetMaxValue();
                    DistanceFieldToVolumeScaleBias = new Vector2(2.0f * MaxDistanceForEncoding, -MaxDistanceForEncoding);
                }
                else
                {
                    // method 2, Trance Distance: 4 * voxelSize
                    Vector3 VolumeSpaceDistanceFieldVoxelSize = IndirectionVoxelSize * LocalToVolumeScale / sdfConfig.UniqueDataBrickSize;
                    float MaxDistanceForEncoding = VolumeSpaceDistanceFieldVoxelSize.Length() * sdfConfig.BandSizeInVoxels;
                    LocalSpaceTraceDistance = MaxDistanceForEncoding / LocalToVolumeScale;
                    DistanceFieldToVolumeScaleBias = new Vector2(2.0f * MaxDistanceForEncoding, -MaxDistanceForEncoding);
                }

                bool bUseFloatFormat = false;
                bool bUseMultiThread = true;
                List<FSparseMeshDistanceFieldAsyncTask> sdfTaskList = new List<FSparseMeshDistanceFieldAsyncTask>();
                for (int YIndex = 0; YIndex < IndirectionDimensions.Y; YIndex++)
                {
                    for (int ZIndex = 0; ZIndex < IndirectionDimensions.Z; ZIndex++)
                    {
                        for (int XIndex = 0; XIndex < IndirectionDimensions.X; XIndex++)
                        {
                            sdfTaskList.Add(new FSparseMeshDistanceFieldAsyncTask(
                                sdfConfig,
                                embreeManager,
                                embreeScene,
                                SampleDirections,
                                LocalSpaceTraceDistance,
                                DistanceFieldVolumeBounds,
                                LocalToVolumeScale,
                                DistanceFieldToVolumeScaleBias,
                                new Vector3i(XIndex, YIndex, ZIndex),
                                IndirectionDimensions,
                                bUsePointQuery,
                                bUseFloatFormat));
                        }
                    }
                }
                if(bUseMultiThread == true)
                {
                    TtEngine.Instance.EventPoster.ParrallelFor(sdfTaskList.Count, static (index, arg1, arg2, state) =>
                    {
                        var pTaskList = arg1 as List<FSparseMeshDistanceFieldAsyncTask>;
                        var task = pTaskList[index];

                        task.DoWork();

                    }, sdfTaskList);
                }
                else
                {
                    foreach(var task in sdfTaskList)
                    {
                        task.DoWork();
                    }
                }

                List<uint> IndirectionTable = new List<uint>();
                IndirectionTable.Resize(IndirectionDimensions.X * IndirectionDimensions.Y * IndirectionDimensions.Z, sdfConfig.InvalidBrickIndex);

                List<FSparseMeshDistanceFieldAsyncTask> ValidBricks = new List<FSparseMeshDistanceFieldAsyncTask>();
                foreach (var task in sdfTaskList)
                {
                    if (task.IsValid())
                    {
                        ValidBricks.Add(task);
                    }
                }

                int NumBricks = ValidBricks.Count;
                int BrickSizeBytes = sdfConfig.BrickSize * sdfConfig.BrickSize * sdfConfig.BrickSize;

                var OutMip = new TtSparseSdfMip();
                for (int BrickIndex = 0; BrickIndex < ValidBricks.Count; BrickIndex++)
                {
                    var Brick = ValidBricks[BrickIndex];
                    int IndirectionIndex = (Brick.BrickCoordinate .Z * IndirectionDimensions.Y + Brick.BrickCoordinate.Y) * IndirectionDimensions.X + Brick.BrickCoordinate.X;
                    IndirectionTable[IndirectionIndex] = (uint)BrickIndex;

                    System.Diagnostics.Debug.Assert(BrickSizeBytes == Brick.DistanceFieldVolume.Count);
                    OutMip.DistanceFieldBrickData.AddRange(Brick.DistanceFieldVolume);
                }

                //int IndirectionTableBytes = IndirectionTable.Count * sizeof(uint);
                //int MipDataBytes = IndirectionTableBytes + DistanceFieldBrickData.Count * sizeof(short);

                OutMip.IndirectionDimensions = IndirectionDimensions;
                OutMip.DistanceFieldToVolumeScaleBias = DistanceFieldToVolumeScaleBias;
                OutMip.NumDistanceFieldBricks = NumBricks;
                OutMip.IndirectionTable = IndirectionTable;
                OutData.Mips.Add(OutMip);
            }

            OutData.bMostlyTwoSided = embreeScene.bMostlyTwoSided;
            OutData.LocalSpaceMeshBounds = LocalSpaceMeshBounds;

            embreeManager.DeleteEmbreeScene(embreeScene);

            float BuildTime = (float)(Support.Time.GetTickCount() - StartTime)/1000.0f;

            if (BuildTime > 0.0f)
            {
                float memoryKB = OutData.GetAllocatedSize()/1024.0f;
                var occupied = (int)Math.Round(100.0f * OutData.Mips[0].NumDistanceFieldBricks / (float)(Mip0IndirectionDimensions.X * Mip0IndirectionDimensions.Y * Mip0IndirectionDimensions.Z));
                   
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Info, $"SDF Generate: Finished distance field build in {BuildTime:0.00} - " +
                    $"{Mip0IndirectionDimensions.X * sdfConfig.UniqueDataBrickSize}x{Mip0IndirectionDimensions.Y * sdfConfig.UniqueDataBrickSize}x{Mip0IndirectionDimensions.Z * sdfConfig.UniqueDataBrickSize} " +
                    $"sparse distance field, {memoryKB:0.0}Kb total, {occupied}% occupied, {embreeScene.NumIndices/3} triangles, {MeshName}");
            }
        }
    }
}
