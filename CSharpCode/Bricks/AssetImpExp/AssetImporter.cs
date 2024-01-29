using Assimp.Unmanaged;
using EngineNS.Animation.Asset;
using EngineNS.Animation.Base;
using EngineNS.Animation.Curve;
using EngineNS.Animation.SkeletonAnimation.Skeleton;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using EngineNS.Graphics.Mesh;
using EngineNS.NxRHI;
using MathNet.Numerics;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace EngineNS.Bricks.AssetImpExp
{
    
    public class AssetDescription
    {
        public string FileName { get; set; } = "";
        public int MeshesCount { get; set; } = 0;
        public int AnimationsCount { get; set; } = 0;

    }
    public class AssetImportOption
    {
        public bool GenerateUMS { get; set; } = false;
        public bool AsStaticMesh { get; set; } = false;
    }
    public class AssetImporter
    {
        public Assimp.Scene AiScene { get; set; } = null;
        public string FilePath { get; set; } = null;
        public AssetDescription PreImport(string filePath)
        {
            FilePath = filePath;
            Assimp.PostProcessSteps convertToLeftHanded = Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipUVs | Assimp.PostProcessSteps.FlipWindingOrder;
            Assimp.AssimpContext assimpContext = new Assimp.AssimpContext();
            Assimp.PostProcessSteps sceneFlags = convertToLeftHanded | Assimp.PostProcessSteps.Triangulate;
            try
            {
                AiScene = assimpContext.ImportFile(filePath, sceneFlags);
                if(AiScene == null)
                {
                    return null;
                }
            }
            catch(Exception)
            {
                return null;
            }
            var meshNodes = AssimpSceneUtil.FindMeshNodes(AiScene);
            AssetDescription assetsGenerateDescription = new AssetDescription();
            assetsGenerateDescription.FileName = Path.GetFileNameWithoutExtension(filePath); ;
            assetsGenerateDescription.MeshesCount = meshNodes.Count;
            assetsGenerateDescription.AnimationsCount = AiScene.AnimationCount;
            return assetsGenerateDescription;
        }
        public List<Assimp.Animation> GetAnimations()
        {
            return AiScene == null ? null : AiScene.Animations;
        }
    }

    public class AssimpSceneUtil
    {
        public static List<Assimp.Node> FindMeshNodes(Assimp.Scene scene)
        {
            List<Assimp.Node> meshNodes = new List<Assimp.Node>();
            FindAllNodesContainsMeshRecursively(scene, scene.RootNode, ref meshNodes);
            return meshNodes;
        }
        private static void FindAllNodesContainsMeshRecursively(Assimp.Scene scene, Assimp.Node node, ref List<Assimp.Node> outNodes)
        {
            if (node.MeshCount != 0)
            {
                outNodes.Add(node);
            }
            else
            {
                foreach (var child in node.Children)
                {
                    FindAllNodesContainsMeshRecursively(scene, child, ref outNodes);
                }
            }
        }
        public static Assimp.Bone FindBone(string name, Assimp.Scene scene)
        {
            foreach (var mesh in scene.Meshes)
            {
                foreach (var bone in mesh.Bones)
                {
                    if (bone.Name == name)
                    {
                        return bone;
                    }
                }
            }
            return null;
        }
        public static USkinSkeleton FindMeshSkeleton(Assimp.Node meshNode, List<USkinSkeleton> skeletons, Assimp.Scene scene)
        {
            var mesh = AssimpSceneUtil.FindMesh(meshNode.Name, scene);
            Debug.Assert(mesh != null);
            foreach (var skeleton in skeletons)
            {
                if (skeleton.FindLimb(mesh.Bones[0].Name) != null)
                {
                    return skeleton;
                }
            }
            return null;
        }
        public static Assimp.Node FindNode(string name, Assimp.Scene scene)
        {
            return FindNodeRecursively(name, scene.RootNode, scene);
        }
        public static Assimp.Mesh FindMesh(string name, Assimp.Scene scene)
        {
            foreach (var mesh in scene.Meshes)
            {
                if (mesh.Name == name)
                {
                    return mesh;
                }
            }
            return null;
        }

        private static Assimp.Node FindNodeRecursively(string name, Assimp.Node parentNode, Assimp.Scene scene)
        {
            foreach (var child in parentNode.Children)
            {
                if (child.Name == name)
                {
                    return child;
                }
                var result = FindNodeRecursively(name, child, scene);
                if(result != null)
                {
                    return result;
                }
            }
            return null;
        }
        public static Vector2 ConvertVector2(Assimp.Vector2D value)
        {
            return new Vector2(value.X, value.Y);
        }
        public static Vector2 ConvertVector2(float x, float y)
        {
            return new Vector2(x, y);
        }
        public static Vector3 ConvertVector3(Assimp.Vector3D value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }
        public static Color4f ConvertColor(Assimp.Color4D value)
        {
            return new Color4f(value.R, value.G, value.B, value.A);
        }
        public static Quaternion ConvertQuaternion(Assimp.Quaternion value)
        {
            return new Quaternion(value.X, value.Y, value.Z, value.W);
        }
    }
    public class SkeletonGenerater
    {
        static void InitNodesMarkMap(Assimp.Node node, ref Dictionary<Assimp.Node, bool> inOutNodesMap)
        {
            inOutNodesMap.Add(node, false);
            foreach (var child in node.Children)
            {
                InitNodesMarkMap(child, ref inOutNodesMap);
            }
        }
        static void MarkBones(Assimp.Scene scene, ref Dictionary<Assimp.Node, bool> inOutNodesMap, ref List<Assimp.Node> inOutSkeletonRootNodes)
        {
            foreach (var mesh in scene.Meshes)
            {
                foreach (var bone in mesh.Bones)
                {
                    var boneNode = AssimpSceneUtil.FindNode(bone.Name, scene);
                    Debug.Assert(boneNode != null);
                    MarkBonesRecursively(boneNode, scene, ref inOutNodesMap, ref inOutSkeletonRootNodes);
                }
            }
        }
        static void MarkBonesRecursively(Assimp.Node node, Assimp.Scene scene, ref Dictionary<Assimp.Node, bool> inOutNodesMap, ref List<Assimp.Node> inOutSkeletonRootNodes)
        {
            if (node.HasMeshes)
            {
                inOutSkeletonRootNodes.Add(node);
                return;
            }
            var parent = GetValidParentNode(node, scene);
            if (IsRootBoneNodeParent(parent, scene))
            {
                if(!inOutSkeletonRootNodes.Contains(node))
                {
                    inOutSkeletonRootNodes.Add(node);
                    inOutNodesMap[node] = true;
                }
                return;
            }

            if (!node.Name.Contains("_$AssimpFbx$_") && inOutNodesMap.ContainsKey(node))
            {
                if (inOutNodesMap[node])
                {
                    return;
                }
                else
                {
                    inOutNodesMap[node] = true;
                }
            }
            else
            {
                MarkBonesRecursively(parent, scene, ref inOutNodesMap, ref inOutSkeletonRootNodes);

            }
        }
        static Assimp.Node GetValidParentNode(Assimp.Node node, Assimp.Scene scene)
        {
            var parent = node.Parent;
            while (parent != null)
            {
                if (parent == scene.RootNode)
                {
                    return parent;
                }
                if (parent.Name.Contains("_$AssimpFbx$_"))
                {
                    parent = parent.Parent;
                }
                else
                {
                    return parent;
                }
            }
            Debug.Assert(false);
            return null;
        }
        static bool IsRootBoneNodeParent(Assimp.Node node, Assimp.Scene scene)
        {
            if (node.Name == "RootNode" || node.HasMeshes)
            {
                return true;
            }
            return false;
        }
        static List<USkinSkeleton> MakeSkeletons(Assimp.Scene scene, List<Assimp.Node> skeletonRootNodes, ref Dictionary<Assimp.Node, bool> inOutNodesMap)
        {
            List<USkinSkeleton> skeletonsGenerate = new List<USkinSkeleton>();
            if (skeletonRootNodes.Count == 1)
            {
                USkinSkeleton skeleton = new USkinSkeleton();
                foreach (var marked in inOutNodesMap)
                {
                    if (marked.Value)
                    {
                        UBoneDesc boneDesc = new UBoneDesc();
                        boneDesc.Name = marked.Key.Name;
                        boneDesc.NameHash = Standart.Hash.xxHash.xxHash32.ComputeHash(boneDesc.Name);
                        var parentNode = GetValidParentNode(marked.Key, scene);
                        if (parentNode == null || !IsRootBoneNodeParent(parentNode, scene))
                        {
                            boneDesc.ParentName = parentNode.Name;
                            boneDesc.ParentHash = Standart.Hash.xxHash.xxHash32.ComputeHash(boneDesc.ParentName);
                        }

                        Assimp.Vector3D assimpTrans, assimpScaling;
                        Assimp.Quaternion assimpQuat;
                        AssimpSceneUtil.FindBone(boneDesc.Name, scene).OffsetMatrix.Decompose(out assimpScaling, out assimpQuat, out assimpTrans);
                        boneDesc.InvPos = new Vector3(assimpTrans.X, assimpTrans.Y, assimpTrans.Z);
                        boneDesc.InvScale = new Vector3(assimpScaling.X, assimpScaling.Y, assimpScaling.Z);
                        boneDesc.InvQuat = new Quaternion(assimpQuat.X, assimpQuat.Y, assimpQuat.Z, assimpQuat.W);
                        var invInitMatrix = Matrix.Transformation(boneDesc.InvScale, boneDesc.InvQuat, boneDesc.InvPos);

                        boneDesc.InvInitMatrix = invInitMatrix;
                        invInitMatrix.Inverse();
                        boneDesc.InitMatrix = invInitMatrix;
                        skeleton.AddLimb(new UBone(boneDesc));
                    }
                }
                skeleton.ConstructHierarchy();
                skeletonsGenerate.Add(skeleton);
            }
            else
            {
                //TODO: muti skeletons in the scene
                System.Diagnostics.Debug.Assert(false);
            }
            return skeletonsGenerate;
        }
        static public List<USkinSkeleton> Generate(Assimp.Scene scene)
        {
            Dictionary<Assimp.Node, bool> nodesMap = new Dictionary<Assimp.Node, bool>();
            List<Assimp.Node> skeletonRootNodes = new List<Assimp.Node>();

            InitNodesMarkMap(scene.RootNode, ref nodesMap);
            MarkBones(scene, ref nodesMap, ref skeletonRootNodes);
            var skeletons = MakeSkeletons(scene, skeletonRootNodes, ref nodesMap);
            return skeletons;
        }
        static public Assimp.Node FindSkeletonMeshNode(USkinSkeleton skeleton, Assimp.Scene scene)
        {
            foreach(var mesh in scene.Meshes)
            {
                if(mesh.HasBones && skeleton.FindLimb(mesh.Bones[0].Name) != null)
                {
                    var meshNode = AssimpSceneUtil.FindNode(mesh.Name, scene);
                    
                }
            }

            return null;
        }

    }
    public class MeshGenerater
    {
        public static List<UMeshPrimitives> Generate(Assimp.Scene scene, AssetImportOption importOption)
        {
            var meshNodes = AssimpSceneUtil.FindMeshNodes(scene);
            var skeletons = SkeletonGenerater.Generate(scene);
            return Generate(meshNodes, skeletons, scene, importOption);
        }
        public static List<UMeshPrimitives> Generate(List<USkinSkeleton> meshSkeletons, Assimp.Scene scene, AssetImportOption importOption)
        {
            var meshNodes = AssimpSceneUtil.FindMeshNodes(scene);
            var skeletons = SkeletonGenerater.Generate(scene);
            return Generate(meshNodes, skeletons, scene, importOption);
        }
        public static List<UMeshPrimitives> Generate(List<Assimp.Node> meshNodes, List<USkinSkeleton> meshSkeletons, Assimp.Scene scene, AssetImportOption importOption)
        {
            List<UMeshPrimitives> meshPrimitives = new List<UMeshPrimitives>();
            foreach (var meshNode in meshNodes)
            {
                var skeleton = AssimpSceneUtil.FindMeshSkeleton(meshNode, meshSkeletons, scene);
                var meshPrimitive = CreateMeshPrimitives(meshNode, skeleton, scene, importOption);
                meshPrimitive.PartialSkeleton = skeleton;
                meshPrimitives.Add(meshPrimitive);
            }
            return meshPrimitives;
        }

        private static UMeshPrimitives CreateMeshPrimitives(Assimp.Node meshNode, USkinSkeleton skeleton, Assimp.Scene scene, AssetImportOption importOption)
        {
            Debug.Assert(meshNode.MeshCount > 0);
            UMeshPrimitives meshPrimitives = new UMeshPrimitives(meshNode.Name, (uint)meshNode.MeshCount);
            int vertextCount = 0;
            int indicesCount = 0;
            uint nextStartIndex = 0;
            for (int i = 0; i < meshNode.MeshCount; i++)
            {
                var subMesh = scene.Meshes[meshNode.MeshIndices[i]];
                FMeshAtomDesc atomDesc = new FMeshAtomDesc();
                atomDesc.PrimitiveType = EPrimitiveType.EPT_TriangleList;
                atomDesc.BaseVertexIndex = 0;
                atomDesc.StartIndex = nextStartIndex;
                atomDesc.NumInstances = 1;
                atomDesc.NumPrimitives = (uint)subMesh.FaceCount;
                meshPrimitives.PushAtom((uint)i, atomDesc);
                vertextCount += subMesh.VertexCount;
                nextStartIndex += atomDesc.NumPrimitives * 3;
                indicesCount += subMesh.GetIndices().Length;
            }
            bool hasVertexColor = true;
            for (int i = 0; i < meshNode.MeshCount; i++)
            {
                var subMesh = scene.Meshes[meshNode.MeshIndices[i]];
                if (!subMesh.HasVertexColors(0))
                {
                    hasVertexColor = false;
                    break;
                }
            }

            Vector3[] posStream = new Vector3[vertextCount];
            Vector3[] normalStream = new Vector3[vertextCount];
            Vector4[] tangentStream = new Vector4[vertextCount];
            Vector2[] uvStream = new Vector2[vertextCount];
            Vector4[] lightMapStream = new Vector4[vertextCount];
            UInt32[] vertexColorStream = null;
            Byte[] skinIndexsStream = null;
            float[] skinWeightsStream = null;
            List<List<uint>> vertexSkinIndex = null;
            List<List<float>> vertexSkinWeight = null;
            UInt16[] renderIndex16 = null;
            UInt32[] renderIndex32 = null;
            bool isIndex32 = false;
            if (indicesCount > 65535)
            {
                isIndex32 = true;
                renderIndex32 = new UInt32[indicesCount];
            }
            else
            {
                renderIndex16 = new UInt16[indicesCount];
            }
            if (hasVertexColor)
            {
                vertexColorStream = new UInt32[vertextCount];
            }
            bool bHasSkin = false;
            var mesh = AssimpSceneUtil.FindMesh(meshNode.Name, scene);
            if (mesh.HasBones && !importOption.AsStaticMesh)
            {
                bHasSkin = true;
                Debug.Assert(skeleton != null);
                meshPrimitives.PartialSkeleton = skeleton;
            }
            if (bHasSkin)
            {
                skinIndexsStream = new Byte[4 * vertextCount];
                skinWeightsStream = new float[4 * vertextCount];
                vertexSkinIndex = new List<List<uint>>(vertextCount);
                vertexSkinWeight = new List<List<float>>(vertextCount);
                for (int i = 0; i < vertextCount; i++)
                {
                    vertexSkinIndex.Add(new List<uint>());
                    vertexSkinWeight.Add(new List<float>());
                }
            }


            int indicesIndex = 0;
            int vertexIndex = 0;
            int vertexCounting = 0;
            for (int i = 0; i < meshNode.MeshCount; i++)
            {
                var subMesh = scene.Meshes[meshNode.MeshIndices[i]];
                //build indices
                var meshIndices = subMesh.GetIndices();
                for (int j = 0; j < meshIndices.Length; j++)
                {
                    if (isIndex32)
                    {
                        renderIndex32[indicesIndex] = (UInt32)(vertexCounting + meshIndices[j]);
                    }
                    else
                    {
                        renderIndex16[indicesIndex] = (UInt16)(vertexCounting + meshIndices[j]);
                    }
                    indicesIndex++;
                }

                //build vertex
                for (int j = 0; j < subMesh.VertexCount; j++)
                {
                    vertexIndex = vertexCounting + j;
                    posStream[vertexIndex] = AssimpSceneUtil.ConvertVector3(subMesh.Vertices[j]) * 0.01f;
                    normalStream[vertexIndex] = AssimpSceneUtil.ConvertVector3(subMesh.Normals[j]);

                    //TODO: TangentChirality
                    //tangentStream[i] = ConvertVector3(mesh->mTangents[j]);
                    if (hasVertexColor)
                    {
                        vertexColorStream[vertexIndex] = AssimpSceneUtil.ConvertColor(subMesh.VertexColorChannels[0][j]).ToAbgr();
                    }
                    int uvChannels = subMesh.TextureCoordinateChannelCount;
                    var uvChannel = subMesh.TextureCoordinateChannels[0];
                    uvStream[vertexIndex] = AssimpSceneUtil.ConvertVector2(uvChannel[j].X, uvChannel[j].Y);
                    if (uvChannels == 2)
                    {
                        var lightMapChannel = subMesh.TextureCoordinateChannels[1];
                        var lightMapUV = new Vector4(lightMapChannel[j].X, lightMapChannel[j].Y, 0, 0);
                        lightMapStream[vertexIndex] = lightMapUV;
                    }
                }

                //build skin
                if (bHasSkin)
                {
                    for (int j = 0; j < subMesh.BoneCount; j++)
                    {
                        var bone = subMesh.Bones[j];
                        var boneIndex = skeleton.FindLimb(bone.Name).Index;
                        Debug.Assert(boneIndex.Value != -1);
                        for (int k = 0; k < bone.VertexWeightCount; k++)
                        {
                            var weight = bone.VertexWeights[k];
                            var vertexId = weight.VertexID;
                            Debug.Assert(boneIndex.IsValid());
                            vertexSkinIndex[vertexCounting + vertexId].Add((uint)boneIndex.Value);
                            vertexSkinWeight[vertexCounting + vertexId].Add(weight.Weight);
                        }
                    }
                }
                vertexCounting += subMesh.VertexCount;
            }

            if (bHasSkin)
            {
                Debug.Assert(vertextCount == vertexSkinIndex.Count);
                Debug.Assert(vertextCount == vertexSkinWeight.Count);
                for (int i = 0; i < vertextCount; i++)
                {
                    var size = vertexSkinIndex[i].Count();
                    float totalWeight = 0.0f;
                    if (size > 4)
                        size = 4;
                    for (int j = 0; j < size; ++j)
                    {
                        totalWeight += vertexSkinWeight[i][j];
                    }
                    for (int j = 0; j < 4; ++j)
                    {
                        if (j < size)
                        {
                            skinIndexsStream[i * 4 + j] = (Byte)vertexSkinIndex[i][j];
                            skinWeightsStream[i * 4 + j] = vertexSkinWeight[i][j] / totalWeight;
                        }
                        else
                        {
                            skinIndexsStream[i * 4 + j] = 0;
                            skinWeightsStream[i * 4 + j] = 0;
                        }
                    }
                }
            }

            //set stream
            var cmd = UEngine.Instance.GfxDevice.RenderContext.CreateCommandList();
            unsafe
            {
                fixed (void* data = posStream)
                {
                    meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_Position, data, (uint)(sizeof(Vector3) * vertextCount), (uint)sizeof(Vector3), ECpuAccess.CAS_DEFAULT);
                }
                fixed (void* data = vertexColorStream)
                {
                    if (hasVertexColor)
                    {
                        meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_Color, data, (uint)(sizeof(uint) * vertextCount), (uint)sizeof(uint), ECpuAccess.CAS_DEFAULT);
                    }
                }
                fixed (void* data = normalStream)
                {
                    meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_Normal, data, (uint)(sizeof(Vector3) * vertextCount), (uint)sizeof(Vector3), ECpuAccess.CAS_DEFAULT);
                }
                fixed (void* data = uvStream)
                {
                    meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_UV, data, (uint)(sizeof(Vector2) * vertextCount), (uint)sizeof(Vector2), ECpuAccess.CAS_DEFAULT);
                }
                fixed (void* data = lightMapStream)
                {
                    meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_LightMap, data, (uint)(sizeof(Vector4) * vertextCount), (uint)sizeof(Vector4), ECpuAccess.CAS_DEFAULT);
                }
                if (isIndex32)
                {
                    fixed (void* data = renderIndex32)
                    {
                        meshPrimitives.mCoreObject.SetGeomtryMeshIndex(cmd.mCoreObject, data, (uint)(sizeof(uint) * indicesCount), isIndex32, ECpuAccess.CAS_DEFAULT);
                    }
                }
                else
                {
                    fixed (void* data = renderIndex16)
                    {
                        meshPrimitives.mCoreObject.SetGeomtryMeshIndex(cmd.mCoreObject, data, (uint)(sizeof(ushort) * indicesCount), isIndex32, ECpuAccess.CAS_DEFAULT);
                    }
                }
                if (bHasSkin)
                {
                    fixed (void* data = skinIndexsStream)
                    {
                        meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_SkinIndex, data, (uint)(sizeof(Byte) * vertextCount * 4), 4 * (uint)sizeof(Byte), ECpuAccess.CAS_DEFAULT);
                    }
                    fixed (void* data = skinWeightsStream)
                    {
                        meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_SkinWeight, data, (uint)(sizeof(float) * vertextCount * 4), 4 * (uint)sizeof(float), ECpuAccess.CAS_DEFAULT);
                    }
                }
            }

            //build aabb
            BoundingBox aabb = new BoundingBox();
            for (int i = 0; i < vertextCount; i++)
            {
                aabb.Merge(posStream[i]);
            }
            meshPrimitives.mCoreObject.SetAABB(ref aabb);

            return meshPrimitives;
        }
    }
    public class AnimationChunkGenerater
    {
        public static UAnimationChunk Generate(RName assetName, Assimp.Animation aiAnim, Assimp.Scene aiScene)
        {
            //UAnimationClip animClip = new UAnimationClip();
            //animClip.SampleRate = (float)aiAnim.TicksPerSecond;
            //animClip.Duration = (float)(aiAnim.DurationInTicks / aiAnim.TicksPerSecond);
            UAnimationChunk chunk = null;

            if (aiAnim.HasNodeAnimations)
            {
                //skeleton animation or scene animation
                chunk = GenerateNodeAnimation(assetName, aiAnim, aiScene);
            }
            else if (aiAnim.HasMeshAnimations)
            {
                //vertex-based animation
            }
            else if (aiAnim.MeshMorphAnimationChannelCount > 0)
            {
                // morphing animation
            }

            Debug.Assert(chunk != null);
            return chunk;
        }

        private static UAnimationChunk GenerateNodeAnimation(RName assetName, Assimp.Animation aiAnim, Assimp.Scene aiScene)
        {
            var animChunk = new EngineNS.Animation.Asset.UAnimationChunk();
            animChunk.RescouceName = assetName;
            Dictionary<uint, UAnimHierarchy> animHDic = new Dictionary<uint, UAnimHierarchy>();
            foreach (var element in aiAnim.NodeAnimationChannels)
            {
                UAnimHierarchy animHierarchy = new UAnimHierarchy();

                AnimatableObjectClassDesc objectClassDesc = new AnimatableObjectClassDesc();
                objectClassDesc.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf(typeof(EngineNS.Animation.SkeletonAnimation.AnimatablePose.UAnimatableBonePose));
                var t = objectClassDesc.ClassType;
                objectClassDesc.Name = element.NodeName;
                uint hash = UniHash32.APHash(objectClassDesc.Name);

                // position
                {
                    var curve = GenerateCurve(element.PositionKeys);
                    animChunk.AnimCurvesList.Add(curve.Id, curve);

                    AnimatableObjectPropertyDesc posDesc = new AnimatableObjectPropertyDesc();
                    posDesc.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf<NullableVector3>();
                    posDesc.Name = "Position";
                    posDesc.CurveId = curve.Id;
                    objectClassDesc.Properties.Add(posDesc);
                }

                // rotation
                {
                    var curve = GenerateCurve(element.RotationKeys);
                    animChunk.AnimCurvesList.Add(curve.Id, curve);

                    AnimatableObjectPropertyDesc rotDesc = new AnimatableObjectPropertyDesc();
                    rotDesc.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf<NullableVector3>();
                    rotDesc.Name = "Rotation";
                    rotDesc.CurveId = curve.Id;
                    objectClassDesc.Properties.Add(rotDesc);
                }

                // scale
                {
                    var curve = GenerateCurve(element.ScalingKeys);
                    animChunk.AnimCurvesList.Add(curve.Id, curve);

                    AnimatableObjectPropertyDesc scaleDesc = new AnimatableObjectPropertyDesc();
                    scaleDesc.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf<NullableVector3>();
                    scaleDesc.Name = "Scale";
                    scaleDesc.CurveId = curve.Id;
                    objectClassDesc.Properties.Add(scaleDesc);
                }

                animHierarchy.Node = objectClassDesc;
                animHDic.Add(hash, animHierarchy);
            }

            //TODO 这里不在需要用这个TtAnimHierarchy了 打算改成列表
            System.Diagnostics.Debug.Assert(animHDic.Count == aiAnim.NodeAnimationChannelCount);
            UAnimHierarchy Root = null;
            //bool rootCheck = false;
            //for (int i = 0; i < animHDic.Count; ++i)
            //{
            //    TtAnimHierarchy parent = null;
            //    var hasParent = animHDic.TryGetValue(animElements[i].Desc.ParentHash, out parent);
            //    if (hasParent)
            //    {
            //        TtAnimHierarchy child = null;
            //        var isExist = animHDic.TryGetValue(animElements[i].Desc.NameHash, out child);
            //        if (isExist)
            //        {
            //            parent.Children.Add(child);
            //            child.Parent = parent;
            //        }
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.Assert(!rootCheck);
            //        rootCheck = true;
            //        TtAnimHierarchy root = null;
            //        var isExist = animHDic.TryGetValue(animElements[i].Desc.NameHash, out root);
            //        if (isExist)
            //        {
            //            Root = root;
            //        }
            //    }
            //}

            animChunk.AnimatedHierarchy = Root;

            return animChunk;
        }
        private static Vector3Curve GenerateCurve(List<Assimp.VectorKey> aiVectorKeys)
        {
            Vector3Curve vector3Curve = new Vector3Curve();
            vector3Curve.XCurve = new UCurve();
            vector3Curve.YCurve = new UCurve();
            vector3Curve.ZCurve = new UCurve();
            foreach (var key in aiVectorKeys)
            {
                Keyframe xKeyFrame = new Keyframe();
                xKeyFrame.Time = (float)key.Time;
                xKeyFrame.Value = key.Value.X;
                vector3Curve.XCurve.AddKeyframeBack(ref xKeyFrame);

                Keyframe yKeyFrame = new Keyframe();
                yKeyFrame.Time = (float)key.Time;
                yKeyFrame.Value = key.Value.Y;
                vector3Curve.YCurve.AddKeyframeBack(ref yKeyFrame);

                Keyframe zKeyFrame = new Keyframe();
                zKeyFrame.Time = (float)key.Time;
                zKeyFrame.Value = key.Value.Z;
                vector3Curve.ZCurve.AddKeyframeBack(ref zKeyFrame);
            }
            return vector3Curve;
        }
        private static Vector3Curve GenerateCurve(List<Assimp.QuaternionKey> aiQuaternionKeys)
        {
            Vector3Curve vector3Curve = new Vector3Curve();
            vector3Curve.XCurve = new UCurve();
            vector3Curve.YCurve = new UCurve();
            vector3Curve.ZCurve = new UCurve();
            foreach (var key in aiQuaternionKeys)
            {
                Quaternion quaternion = AssimpSceneUtil.ConvertQuaternion(key.Value);
                var euler = quaternion.ToEuler();
                Keyframe xKeyFrame = new Keyframe();
                xKeyFrame.Time = (float)key.Time;
                xKeyFrame.Value = euler.X;
                vector3Curve.XCurve.AddKeyframeBack(ref xKeyFrame);

                Keyframe yKeyFrame = new Keyframe();
                yKeyFrame.Time = (float)key.Time;
                yKeyFrame.Value = euler.Y;
                vector3Curve.YCurve.AddKeyframeBack(ref yKeyFrame);

                Keyframe zKeyFrame = new Keyframe();
                zKeyFrame.Time = (float)key.Time;
                zKeyFrame.Value = euler.Z;
                vector3Curve.ZCurve.AddKeyframeBack(ref zKeyFrame);
            }
            return vector3Curve;
        }
    }
}
