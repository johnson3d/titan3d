using Assimp;
using Assimp.Configs;
using Assimp.Unmanaged;
using EngineNS.Animation.Asset;
using EngineNS.Animation.Base;
using EngineNS.Animation.Curve;
using EngineNS.Animation.SkeletonAnimation.Skeleton;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Graphics.Mesh;
using EngineNS.NxRHI;
using EngineNS.UI.Controls;
using MathNet.Numerics;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using System.Text;
using static NPOI.HSSF.Util.HSSFColor;

using Matrix4x4 = Assimp.Matrix4x4;

namespace EngineNS.Bricks.AssetImpExp
{

    public class TtAssetDescription
    {
        public string FileName { get; set; } = "";
        public int MeshesCount { get; set; } = 0;
        public bool MeshesHaveScale { get; set; } = false;
        public bool MeshesHaveTranslation { get; set; } = false;
        public int AnimationsCount { get; set; } = 0;
        public string UpAxis { get; set; } = "";
        public float UnitScaleFactor { get; set; } = 1;
        public string Generator { get; set; } = "";

    }
    public class TtAssetImportOption_Mesh
    {
        public bool GenerateUMS { get; set; } = false;
        public bool ApplyTransformToVertex { get; set; } = false;
        public bool AsStaticMesh { get; set; } = false;
        public float UnitScale { get; set; } = 0.01f;
    }
    public class TtAssetImportOption_Animation
    {
        public float Scale { get; set; } = 0.01f;
        public bool IgnoreScale { get; set; } = true;
    }
    public class TtAssetImporter
    {
        public Assimp.Scene AiScene { get; set; } = null;
        public string FilePath { get; set; } = null;
        public TtAssetDescription PreImport(string filePath)
        {
            FilePath = filePath;
            Assimp.PostProcessSteps convertToLeftHanded = Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipUVs | Assimp.PostProcessSteps.FlipWindingOrder;
            Assimp.AssimpContext assimpContext = new Assimp.AssimpContext();
            Assimp.PostProcessSteps sceneFlags = convertToLeftHanded | Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.CalculateTangentSpace;
            try
            {
                AiScene = assimpContext.ImportFile(filePath, sceneFlags);
                if (AiScene == null)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
            var meshNodes = AssimpSceneUtil.FindMeshNodes(AiScene);
            bool nodeHasScale = false;
            bool nodeHasTranslation = false;
            foreach (var meshNode in meshNodes)
            {
                var preAssimpTransform = AssimpSceneUtil.AccumulatePreTransform(meshNode.Parent);
                var preTransform = AssimpSceneUtil.AssimpMatrix4x4Decompose(preAssimpTransform);
                var nodeTransform = AssimpSceneUtil.AssimpMatrix4x4Decompose(meshNode.Transform);
                if (preTransform.scaling != Vector3.One || nodeTransform.scaling != Vector3.One)
                {
                    nodeHasScale = true;
                }
                if (preTransform.translation != Vector3.Zero || nodeTransform.translation != Vector3.Zero)
                {
                    nodeHasTranslation = true;
                }
            }
            TtAssetDescription assetsGenerateDescription = new TtAssetDescription();
            assetsGenerateDescription.FileName = Path.GetFileNameWithoutExtension(filePath); ;
            assetsGenerateDescription.MeshesCount = meshNodes.Count;
            assetsGenerateDescription.MeshesHaveScale = nodeHasScale;
            assetsGenerateDescription.MeshesHaveTranslation = nodeHasTranslation;
            assetsGenerateDescription.AnimationsCount = AiScene.AnimationCount;
            string[] Axis = new[] { "X", "Y", "Z" };
            assetsGenerateDescription.UpAxis = (Int32)AiScene.Metadata["UpAxisSign"].Data == -1 ? "-" + Axis[(Int32)AiScene.Metadata["UpAxis"].Data] : Axis[(Int32)AiScene.Metadata["UpAxis"].Data];
            assetsGenerateDescription.UnitScaleFactor = (float)AiScene.Metadata["UnitScaleFactor"].Data;
            assetsGenerateDescription.Generator = (String)AiScene.Metadata["SourceAsset_Generator"].Data;

            return assetsGenerateDescription;
        }
        public List<Assimp.Animation> GetAnimations()
        {
            return AiScene == null ? null : AiScene.Animations;
        }
    }

    public class AssimpSceneUtil
    {
        public static bool IsZUpLeftHandCoordinate(Assimp.Scene scene)
        {
            //the scene has already convert to left hand when read
            var upAxis = scene.Metadata["UpAxis"].DataAs<int>();
            return upAxis == 2;
        }
        public static Assimp.Matrix4x4 GetCoordinateConvertMatrix(Assimp.Scene scene)
        {
            var upAxis = scene.Metadata["UpAxis"].DataAs<int>();
            var upAxisSign = scene.Metadata["UpAxisSign"].DataAs<int>();
            var frontAxis = scene.Metadata["FrontAxis"].DataAs<int>();
            var frontAxisSign = scene.Metadata["FrontAxisSign"].DataAs<int>();
            var coordAxis = scene.Metadata["CoordAxis"].DataAs<int>();
            var coordAxisSign = scene.Metadata["CoordAxisSign"].DataAs<int>();
            Assimp.Vector3D upVec = upAxis == 0 ? new Assimp.Vector3D(upAxisSign.Value, 0, 0) : upAxis == 1 ? new Assimp.Vector3D(0, upAxisSign.Value, 0) : new Assimp.Vector3D(0, 0, upAxisSign.Value);
            Assimp.Vector3D forwardVec = frontAxis == 0 ? new Assimp.Vector3D(frontAxisSign.Value, 0, 0) : frontAxis == 1 ? new Assimp.Vector3D(0, frontAxisSign.Value, 0) : new Assimp.Vector3D(0, 0, frontAxisSign.Value);
            Assimp.Vector3D rightVec = coordAxis == 0 ? new Assimp.Vector3D(coordAxisSign.Value, 0, 0) : coordAxis == 1 ? new Assimp.Vector3D(0, coordAxisSign.Value, 0) : new Assimp.Vector3D(0, 0, coordAxisSign.Value);
            Assimp.Matrix4x4 coordinateConvert = new Assimp.Matrix4x4(rightVec.X, rightVec.Y, rightVec.Z, 0.0f,
                                                            upVec.X, upVec.Y, upVec.Z, 0.0f,
                                                            forwardVec.X, forwardVec.Y, forwardVec.Z, 0.0f,
                                                            0.0f, 0.0f, 0.0f, 1.0f);
            coordinateConvert.Inverse();
            return coordinateConvert;
        }

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
                foreach (var meshIndex in node.MeshIndices)
                {
                    if (scene.Meshes[meshIndex].PrimitiveType != PrimitiveType.Line && scene.Meshes[meshIndex].PrimitiveType != PrimitiveType.Point)
                    {
                        outNodes.Add(node);
                        break;
                    }
                }
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
        public static TtSkinSkeleton FindMeshSkeleton(Assimp.Node meshNode, List<TtSkinSkeleton> skeletons, Assimp.Scene scene)
        {
            var meshes = AssimpSceneUtil.FindMesh(meshNode, scene);
            foreach (var skeleton in skeletons)
            {
                foreach (var mesh in meshes)
                {
                    if (!mesh.HasBones)
                        continue;

                    if (skeleton.FindLimb(mesh.Bones[0].Name) != null)
                    {
                        return skeleton;
                    }
                }
            }
            return null;
        }
        public static Assimp.Node FindNode(string name, Assimp.Scene scene)
        {
            return FindNodeRecursively(name, scene.RootNode, scene);
        }
        public static List<Assimp.Mesh> FindMesh(Assimp.Node meshNode, Assimp.Scene scene)
        {
            List<Assimp.Mesh> meshes = new();
            if (meshNode.HasMeshes)
            {
                foreach (var meshIndex in meshNode.MeshIndices)
                {
                    meshes.Add(scene.Meshes[meshIndex]);
                }
            }
            return meshes;
        }
        public static Assimp.Mesh FindMeshByBone(string boneName, Assimp.Scene scene)
        {
            foreach (var mesh in scene.Meshes)
            {
                foreach (var bone in mesh.Bones)
                {
                    if (bone.Name == boneName)
                    {
                        return mesh;
                    }
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
                if (result != null)
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
            var q = new Quaternion(value.X, value.Y, value.Z, value.W) * Quaternion.FromEuler(new FRotator(0, 90 * FRotator.D2R, 0));
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static Assimp.Matrix4x4 AccumulatePreTransform(Assimp.Node preTransformNode)
        {
            var transform = preTransformNode.Transform;
            if (IsParentIs_AssimpFbxPre_Node(preTransformNode))
            {
                return transform * AccumulatePreTransform(preTransformNode.Parent);
            }
            else
            {
                return transform;
            }
        }
        public static Assimp.Matrix4x4 GetAbsBoneNodeMatrix(Assimp.Node node)
        {
            if (AssimpSceneUtil.IsSceneRootNode(node.Parent) || node.Parent.HasMeshes || AssimpSceneUtil.IsParentIs_AssimpFbxPre_Node(node))
            {
                return node.Transform;
            }
            else
            {
                return node.Transform * GetAbsBoneNodeMatrix(node.Parent);
            }
        }
        public static Assimp.Matrix4x4 GetAbsNodeMatrix(Assimp.Node node)
        {
            if (AssimpSceneUtil.IsSceneRootNode(node) || AssimpSceneUtil.IsSceneRootNode(node.Parent) || node.Parent.HasMeshes)
            {
                return node.Transform;
            }
            else
            {
                node.Transform.Decompose(out var scaling, out var rotation, out var translation);
                if (scaling.X < 0 || scaling.Y < 0 || scaling.Z < 0)
                {
                    var transMat = new Matrix4x4(rotation.GetMatrix()) * Matrix4x4.FromTranslation(translation);

                    return transMat * GetAbsNodeMatrix(node.Parent);
                }
                else
                {
                    return node.Transform * GetAbsNodeMatrix(node.Parent);
                }
            }
        }

        public static bool IsSceneRootNode(Assimp.Node node)
        {
            if (node.Name == "RootNode")
            {
                return true;
            }
            return false;
        }
        public static Assimp.Matrix4x4 GetAssimpFbxGeometricTranslation(Assimp.Node node)
        {
            if (node.Parent.Name.Contains(node.Name) && IsParentIs_AssimpFbxPre_Node(node) && node.Parent.Name.Contains("GeometricTranslation"))
            {
                return node.Parent.Transform;
            }
            return Assimp.Matrix4x4.Identity;
        }
        public static bool IsSceneHave_AssimpFbxPre_Node(Assimp.Node node)
        {
            if (node.Name.Contains("_$AssimpFbx$_"))
            {
                return true;
            }
            else
            {
                foreach (var child in node.Children)
                {
                    if (IsSceneHave_AssimpFbxPre_Node(child))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public static bool IsParentIs_AssimpFbxPre_Node(Assimp.Node node)
        {
            if (node.Parent != null && node.Parent.Name.Contains("_$AssimpFbx$_"))
            {
                return true;
            }
            return false;
        }
        public static bool Is_AssimpFbxPre_Node(Assimp.Node node)
        {
            if (node.Name.Contains("_$AssimpFbx$_"))
            {
                return true;
            }
            return false;
        }
        public static Assimp.Node FindParent_AssimpFbx_Node(Assimp.Node node)
        {
            if (node.Parent == null)
                return null;
            if (node.Parent.Name.Contains("_$AssimpFbx$_"))
            {
                return node.Parent;
            }
            else
            {
                return FindParent_AssimpFbx_Node(node.Parent);
            }
        }
        public static (Vector3 scaling, Quaternion rotation, Vector3 translation) AssimpMatrix4x4Decompose(Assimp.Matrix4x4 matrix)
        {
            Assimp.Vector3D assimpTrans, assimpScaling;
            Assimp.Quaternion assimpQuat;
            matrix.Decompose(out assimpScaling, out assimpQuat, out assimpTrans);
            var translation = new Vector3(assimpTrans.X, assimpTrans.Y, assimpTrans.Z);
            var scaling = new Vector3(assimpScaling.X, assimpScaling.Y, assimpScaling.Z);
            var rotation = new Quaternion(assimpQuat.X, assimpQuat.Y, assimpQuat.Z, assimpQuat.W);
            return (scaling, rotation, translation);
        }

        public static FTransform AssimpMatrix4x4DecomposeToTransform(Assimp.Matrix4x4 matrix)
        {
            var result = AssimpMatrix4x4Decompose(matrix);
            return FTransform.CreateTransform(result.translation.AsDVector(), result.scaling, result.rotation);
        }
        public static Matrix AssimpMatrix4x4ToTtMatrix(Assimp.Matrix4x4 matrix)
        {
            var result = AssimpMatrix4x4Decompose(matrix);
            return Matrix.Transformation(result.scaling, result.rotation, result.translation);
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
                    MarkBonesUpSideAndSelfRecursively(boneNode, scene, ref inOutNodesMap, ref inOutSkeletonRootNodes);
                    MarkBonesDownSideRecursively(boneNode, scene, ref inOutNodesMap, ref inOutSkeletonRootNodes);
                }
            }
        }
        static bool IsSiblingNodeHasMesh(Assimp.Node node)
        {
            if (node.Parent != null)
            {
                foreach (var child in node.Parent.Children)
                {
                    if (child != node)
                    {
                        if (child.HasMeshes)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        static void MarkBonesDownSideRecursively(Assimp.Node node, Assimp.Scene scene, ref Dictionary<Assimp.Node, bool> inOutNodesMap, ref List<Assimp.Node> inOutSkeletonRootNodes)
        {
            foreach (var child in node.Children)
            {
                if (!AssimpSceneUtil.Is_AssimpFbxPre_Node(child))
                {
                    if (inOutNodesMap[child])
                    {
                        continue;
                    }
                    inOutNodesMap[child] = true;
                }
                MarkBonesDownSideRecursively(child, scene, ref inOutNodesMap, ref inOutSkeletonRootNodes);

            }
        }
        static void MarkBonesUpSideAndSelfRecursively(Assimp.Node node, Assimp.Scene scene, ref Dictionary<Assimp.Node, bool> inOutNodesMap, ref List<Assimp.Node> inOutSkeletonRootNodes)
        {
            if (IsSiblingNodeHasMesh(node))
            {
                if (!inOutSkeletonRootNodes.Contains(node))
                {
                    inOutSkeletonRootNodes.Add(node);
                    inOutNodesMap[node] = true;
                }
                return;
            }

            var parent = GetValidParentNode(node, scene);
            if (AssimpSceneUtil.IsSceneRootNode(parent) || parent.HasMeshes)
            {
                if (!inOutSkeletonRootNodes.Contains(node))
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
                MarkBonesUpSideAndSelfRecursively(parent, scene, ref inOutNodesMap, ref inOutSkeletonRootNodes);
            }
            else
            {
                Debug.Assert(false);
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
        static TtBoneDesc MakeBoneDesc(Assimp.Scene scene, Node boneNode, Node rootBoneNode, TtAssetImportOption_Mesh importOption)
        {
            TtBoneDesc boneDesc = new TtBoneDesc();
            boneDesc.Name = boneNode.Name;
            boneDesc.NameHash = Standart.Hash.xxHash.xxHash32.ComputeHash(boneDesc.Name);
            var parentNode = GetValidParentNode(boneNode, scene);
            if (rootBoneNode != boneNode && parentNode != null && !AssimpSceneUtil.IsSceneRootNode(parentNode) && !parentNode.HasMeshes)
            {
                boneDesc.ParentName = parentNode.Name;
                boneDesc.ParentHash = Standart.Hash.xxHash.xxHash32.ComputeHash(boneDesc.ParentName);
            }

            var boneAbsNodeTransform = AssimpSceneUtil.GetAbsNodeMatrix(boneNode);
            Matrix initMatrix = Matrix.Identity;
            if (AssimpSceneUtil.IsZUpLeftHandCoordinate(scene))
            {
                initMatrix = AssimpSceneUtil.AssimpMatrix4x4ToTtMatrix(boneAbsNodeTransform * AssimpSceneUtil.GetCoordinateConvertMatrix(scene));
            }
            else
            {
                initMatrix = AssimpSceneUtil.AssimpMatrix4x4ToTtMatrix(boneAbsNodeTransform);
            }

            var initMatrixScaled = initMatrix;
            initMatrixScaled.NoScale();
            initMatrixScaled.SetTrans(initMatrixScaled.Translation * importOption.UnitScale);
            var invInitMatrix = initMatrixScaled;
            invInitMatrix.Inverse();

            DVector3 invPos = DVector3.Zero;
            Vector3 invScale = Vector3.One;
            Quaternion invQuat = Quaternion.Identity;
            invInitMatrix.Decompose(out invScale, out invQuat, out invPos);
            if (!invQuat.IsNormalized())
            {
                invQuat.Normalize();
            }
            boneDesc.InvScale = invScale;
            boneDesc.InvQuat = invQuat;
            boneDesc.InvPos = invPos.ToSingleVector3();

            boneDesc.InvInitMatrix = invInitMatrix;
            boneDesc.InitMatrix = initMatrixScaled;

            return boneDesc;
        }

        static List<TtSkinSkeleton> MakeSkeletons(Assimp.Scene scene, List<Assimp.Node> skeletonRootNodes, TtAssetImportOption_Mesh importOption, ref Dictionary<Assimp.Node, bool> inOutNodesMap)
        {
            List<TtSkinSkeleton> skeletonsGenerate = new List<TtSkinSkeleton>();
            if (skeletonRootNodes.Count == 0)
                return skeletonsGenerate;

            if (skeletonRootNodes.Count == 1)
            {
                var skeletonRootNode = skeletonRootNodes[0];
                var preAssimpTransform = Assimp.Matrix4x4.Identity;
                if (AssimpSceneUtil.IsZUpLeftHandCoordinate(scene))
                {
                    preAssimpTransform = AssimpSceneUtil.GetCoordinateConvertMatrix(scene);
                }
                else
                {
                    if (AssimpSceneUtil.IsParentIs_AssimpFbxPre_Node(skeletonRootNode))
                    {
                        preAssimpTransform = AssimpSceneUtil.AccumulatePreTransform(skeletonRootNode.Parent);
                        //preAssimpTransform.Inverse();
                    }
                }

                TtSkinSkeleton skeleton = new TtSkinSkeleton();
                foreach (var marked in inOutNodesMap)
                {
                    if (marked.Value)
                    {
                        TtBoneDesc boneDesc = MakeBoneDesc(scene, marked.Key, skeletonRootNode, importOption);
                        skeleton.AddLimb(new TtBone(boneDesc));
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
        public static List<TtSkinSkeleton> Generate(Assimp.Scene scene, TtAssetImportOption_Mesh importOption)
        {
            Dictionary<Assimp.Node, bool> nodesMap = new Dictionary<Assimp.Node, bool>();
            List<Assimp.Node> skeletonRootNodes = new List<Assimp.Node>();

            InitNodesMarkMap(scene.RootNode, ref nodesMap);
            MarkBones(scene, ref nodesMap, ref skeletonRootNodes);
            var skeletons = MakeSkeletons(scene, skeletonRootNodes, importOption, ref nodesMap);
            return skeletons;
        }
        public static Assimp.Node FindSkeletonMeshNode(TtSkinSkeleton skeleton, Assimp.Scene scene)
        {
            foreach (var mesh in scene.Meshes)
            {
                if (mesh.HasBones && skeleton.FindLimb(mesh.Bones[0].Name) != null)
                {
                    var meshNode = AssimpSceneUtil.FindNode(mesh.Name, scene);

                }
            }

            return null;
        }

    }
    public class MeshGenerater
    {
        public static List<TtMeshPrimitives> Generate(Assimp.Scene scene, TtAssetImportOption_Mesh importOption)
        {
            var meshNodes = AssimpSceneUtil.FindMeshNodes(scene);
            var skeletons = SkeletonGenerater.Generate(scene, importOption);
            return Generate(meshNodes, skeletons, scene, importOption);
        }
        public static List<TtMeshPrimitives> Generate(List<TtSkinSkeleton> meshSkeletons, Assimp.Scene scene, TtAssetImportOption_Mesh importOption)
        {
            var meshNodes = AssimpSceneUtil.FindMeshNodes(scene);
            return Generate(meshNodes, meshSkeletons, scene, importOption);
        }
        private static List<TtMeshPrimitives> Generate(List<Assimp.Node> meshNodes, List<TtSkinSkeleton> meshSkeletons, Assimp.Scene scene, TtAssetImportOption_Mesh importOption)
        {
            List<TtMeshPrimitives> meshPrimitives = new List<TtMeshPrimitives>();
            foreach (var meshNode in meshNodes)
            {
                var skeleton = AssimpSceneUtil.FindMeshSkeleton(meshNode, meshSkeletons, scene);
                var meshPrimitive = CreateMeshPrimitives(meshNode, skeleton, scene, importOption);
                meshPrimitive.PartialSkeleton = skeleton;
                meshPrimitives.Add(meshPrimitive);
            }
            return meshPrimitives;
        }

        static List<Mesh> GetValidMesh(Assimp.Node meshNode, Assimp.Scene scene)
        {
            List<Mesh> validMeshes = new();
            foreach (var meshIndex in meshNode.MeshIndices)
            {
                if (scene.Meshes[meshIndex].PrimitiveType != PrimitiveType.Line && scene.Meshes[meshIndex].PrimitiveType != PrimitiveType.Line)
                {
                    validMeshes.Add(scene.Meshes[meshIndex]);
                }
            }
            return validMeshes;
        }
        private static TtMeshPrimitives CreateMeshPrimitives(Assimp.Node meshNode, TtSkinSkeleton skeleton, Assimp.Scene scene, TtAssetImportOption_Mesh importOption)
        {
            var preAssimpTransform = Assimp.Matrix4x4.Identity;
            if (AssimpSceneUtil.IsZUpLeftHandCoordinate(scene))
            {
                preAssimpTransform = AssimpSceneUtil.GetCoordinateConvertMatrix(scene);
            }
            else
            {
                if (AssimpSceneUtil.IsParentIs_AssimpFbxPre_Node(meshNode))
                {
                    preAssimpTransform = AssimpSceneUtil.AccumulatePreTransform(meshNode.Parent);
                }
            }

            Debug.Assert(meshNode.MeshCount > 0);
            var transformTuple = AssimpSceneUtil.AssimpMatrix4x4DecomposeToTransform(preAssimpTransform);
            var vertexPreTransform = FTransform.Identity;
            if (importOption.ApplyTransformToVertex)
            {
                var nodeTransform = AssimpSceneUtil.AssimpMatrix4x4DecomposeToTransform(meshNode.Transform);
                FTransform finalTransform;
                FTransform.Multiply(out finalTransform, transformTuple, nodeTransform);
                vertexPreTransform = FTransform.CreateTransform(finalTransform.Position,
                finalTransform.Scale * importOption.UnitScale, finalTransform.Quat);
            }
            else
            {
                vertexPreTransform = FTransform.CreateTransform(Vector3.Zero.AsDVector(),
                transformTuple.Scale * importOption.UnitScale, transformTuple.Quat);
            }

            var meshes = GetValidMesh(meshNode, scene);

            TtMeshPrimitives meshPrimitives = new TtMeshPrimitives(meshNode.Name, (uint)meshes.Count);
            int vertextCount = 0;
            int indicesCount = 0;
            uint nextStartIndex = 0;
            for (int i = 0; i < meshes.Count; i++)
            {
                var subMesh = meshes[i];
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
            for (int i = 0; i < meshes.Count; i++)
            {
                var subMesh = meshes[i];
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
            foreach (var mesh in meshes)
            {
                if (mesh.HasBones && !importOption.AsStaticMesh)
                {
                    bHasSkin = true;
                    Debug.Assert(skeleton != null);
                    meshPrimitives.PartialSkeleton = skeleton;
                    break;
                }
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
            for (int i = 0; i < meshes.Count; i++)
            {
                var subMesh = meshes[i];
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
                    posStream[vertexIndex] = vertexPreTransform.TransformPosition(AssimpSceneUtil.ConvertVector3(subMesh.Vertices[j]).AsDVector()).ToSingleVector3();
                    normalStream[vertexIndex] = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(subMesh.Normals[j]));

                    if (subMesh.HasTangentBasis)
                    {
                        var normal = AssimpSceneUtil.ConvertVector3(subMesh.Normals[j]);
                        var tangent = AssimpSceneUtil.ConvertVector3(subMesh.Tangents[j]);
                        var binTan = AssimpSceneUtil.ConvertVector3(subMesh.BiTangents[j]);
                        float dp = Vector3.Dot(Vector3.Cross(normal, tangent), binTan);
                        var transformedTangent = vertexPreTransform.TransformVector3NoScale(tangent);
                        float w = dp > 0.0f ? 1.0f : -1.0f;
                        tangentStream[vertexIndex] = new Vector4(transformedTangent, w);
                    }
                    if (hasVertexColor)
                    {
                        vertexColorStream[vertexIndex] = AssimpSceneUtil.ConvertColor(subMesh.VertexColorChannels[0][j]).ToAbgr();
                    }
                    int uvChannels = subMesh.TextureCoordinateChannelCount;
                    if (uvChannels > 0)
                    {
                        var uvChannel = subMesh.TextureCoordinateChannels[0];
                        uvStream[vertexIndex] = AssimpSceneUtil.ConvertVector2(uvChannel[j].X, uvChannel[j].Y);
                        if (uvChannels == 2)
                        {
                            var lightMapChannel = subMesh.TextureCoordinateChannels[1];
                            var lightMapUV = new Vector4(lightMapChannel[j].X, lightMapChannel[j].Y, 0, 0);
                            lightMapStream[vertexIndex] = lightMapUV;
                        }
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
                    var size = vertexSkinIndex[i].Count;
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
            var cmd = TtEngine.Instance.GfxDevice.RenderContext.CreateCommandList();
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
                fixed (void* data = tangentStream)
                {
                    meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_Tangent, data, (uint)(sizeof(Vector4) * vertextCount), (uint)sizeof(Vector4), ECpuAccess.CAS_DEFAULT);
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
        public static TtAnimationChunk Generate(RName assetName, Assimp.Animation aiAnim, Assimp.Scene aiScene, TtAssetImportOption_Animation importOption)
        {
            TtAnimationChunk chunk = null;

            if (aiAnim.HasNodeAnimations)
            {
                //skeleton animation or scene animation
                chunk = GenerateNodeAnimation(assetName, aiAnim, aiScene, importOption);
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

        private static NodeAnimationChannel FindNodeAnimationChannel(string nodeName, Assimp.Animation aiAnim, Scene scene)
        {
            foreach(var element in aiAnim.NodeAnimationChannels)
            {
                if(element.NodeName == nodeName)
                {
                    return element;
                }
            }
            return null;
        }
        private static Vector3D GetScaleRecursively(Node node, Assimp.Animation aiAnim, Assimp.Scene scene, int keyIndex)
        {
            if(AssimpSceneUtil.IsSceneRootNode(node))
            {
                return new Vector3D(1);
            }
            var element = FindNodeAnimationChannel(node.Name, aiAnim, scene);
            if(element != null)
            {
                var scales = element.ScalingKeys;
                return scales[keyIndex].Value * GetScaleRecursively(node.Parent, aiAnim, scene, keyIndex);
            }
            else
            {
                return GetScaleRecursively(node.Parent, aiAnim, scene, keyIndex);
            }
        }

        private static TtAnimationChunk GenerateNodeAnimation(RName assetName, Assimp.Animation aiAnim, Assimp.Scene aiScene, TtAssetImportOption_Animation importOption)
        {
            var animChunk = new EngineNS.Animation.Asset.TtAnimationChunk();
            animChunk.RescouceName = assetName;
            foreach (var element in aiAnim.NodeAnimationChannels)
            {
                TtAnimatedObjectDescription objectBinding = new TtAnimatedObjectDescription();
                objectBinding.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf(typeof(EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableBonePose));
                objectBinding.Name = element.NodeName;

                // position
                {
                    var node = AssimpSceneUtil.FindNode(element.NodeName, aiScene);
                    node.Transform.Decompose(out var s, out var r, out var t);
                    var curve = GeneratePositionCurve(element, aiAnim.TicksPerSecond, aiAnim, aiScene, importOption);
                    animChunk.AnimCurvesList.Add(curve.Id, curve);

                    TtAnimatedPropertyDescription propertyBinding = new TtAnimatedPropertyDescription();
                    propertyBinding.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf<FNullableVector3>();
                    propertyBinding.Name = "Position";
                    propertyBinding.CurveId = curve.Id;
                    objectBinding.TranslationProperty = propertyBinding;
                }

                // rotation
                {
                    var curve = GenerateRotationCurve(element, aiAnim.TicksPerSecond, aiScene);
                    animChunk.AnimCurvesList.Add(curve.Id, curve);

                    TtAnimatedPropertyDescription propertyBinding = new TtAnimatedPropertyDescription();
                    propertyBinding.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf<FNullableVector3>();
                    propertyBinding.Name = "Rotation";
                    propertyBinding.CurveId = curve.Id;
                    objectBinding.RotationProperty = propertyBinding;
                }

                // scale
                if(!importOption.IgnoreScale)
                {
                    var curve = GenerateScaleCurve(element, aiAnim.TicksPerSecond, aiScene);
                    animChunk.AnimCurvesList.Add(curve.Id, curve);

                    TtAnimatedPropertyDescription propertyBinding = new TtAnimatedPropertyDescription();
                    propertyBinding.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf<FNullableVector3>();
                    propertyBinding.Name = "Scale";
                    propertyBinding.CurveId = curve.Id;
                    objectBinding.ScaleProperty = propertyBinding;
                }

                animChunk.AnimatedObjectDescs.Add(objectBinding.Name, objectBinding);
            }
            return animChunk;
        }
        private static TtVector3Curve GeneratePositionCurve(Assimp.NodeAnimationChannel animationChannel, double TicksPerSecond, Assimp.Animation aiAnim, Assimp.Scene aiScene, TtAssetImportOption_Animation importOption)
        {
            var node = AssimpSceneUtil.FindNode(animationChannel.NodeName, aiScene);
            List<Assimp.VectorKey> positionKeys = animationChannel.PositionKeys;
            List<Assimp.VectorKey> scaleKeys = animationChannel.ScalingKeys;
            TtVector3Curve vector3Curve = new TtVector3Curve();
            vector3Curve.XTrack = new TtTrack();
            vector3Curve.YTrack = new TtTrack();
            vector3Curve.ZTrack = new TtTrack();
            for (int i = 0; i < positionKeys.Count; ++i)
            {
                var posKey = positionKeys[i];
                Vector3D transition = posKey.Value;
                if (AssimpSceneUtil.IsZUpLeftHandCoordinate(aiScene))
                {
                    //var coordinateConvertMatrix = AssimpSceneUtil.GetCoordinateConvertMatrix(aiScene);
                    //var finalMat = Matrix4x4.FromTranslation(transition) * coordinateConvertMatrix;
                    //finalMat.Decompose(out var uselessScaling, out var uselessRotation, out transition);
                    //transition = new Vector3D(transition.X, transition.Y, transition.Z);
                }

                Vector3D finalScale = importOption.Scale * new Vector3D(1);
                if (importOption.IgnoreScale)
                {
                    Debug.Assert(posKey.Time == scaleKeys[i].Time);
                    finalScale = finalScale * GetScaleRecursively(node, aiAnim, aiScene, i);
                }

                Vector3D finalPos = transition * finalScale;
                float keyTime = (float)(posKey.Time / TicksPerSecond);
                FKeyframe xKeyFrame = new FKeyframe();
                xKeyFrame.Time = keyTime;
                xKeyFrame.Value = finalPos.X;
                vector3Curve.XTrack.AddKeyframeBack(ref xKeyFrame);

                FKeyframe yKeyFrame = new FKeyframe();
                yKeyFrame.Time = keyTime;
                yKeyFrame.Value = finalPos.Y;
                vector3Curve.YTrack.AddKeyframeBack(ref yKeyFrame);

                FKeyframe zKeyFrame = new FKeyframe();
                zKeyFrame.Time = keyTime;
                zKeyFrame.Value = finalPos.Z;
                vector3Curve.ZTrack.AddKeyframeBack(ref zKeyFrame);
            }
            return vector3Curve;
        }
        private static TtVector3Curve GenerateScaleCurve(Assimp.NodeAnimationChannel animationChannel, double TicksPerSecond, Assimp.Scene aiScene)
        {
            List<Assimp.VectorKey> aiVectorKeys = animationChannel.ScalingKeys;
            TtVector3Curve vector3Curve = new TtVector3Curve();
            vector3Curve.XTrack = new TtTrack();
            vector3Curve.YTrack = new TtTrack();
            vector3Curve.ZTrack = new TtTrack();
            foreach (var key in aiVectorKeys)
            {
                Vector3D scale = key.Value;
                if (AssimpSceneUtil.IsZUpLeftHandCoordinate(aiScene))
                {
                    //var coordinateConvertMatrix = AssimpSceneUtil.GetCoordinateConvertMatrix(aiScene);
                    //var finalMat = Matrix4x4.FromScaling(scale) * coordinateConvertMatrix;
                    //finalMat.Decompose(out scale, out var uselessRotation, out var uselessTransition);
                }
                float keyTime = (float)(key.Time / TicksPerSecond);
                FKeyframe xKeyFrame = new FKeyframe();
                xKeyFrame.Time = keyTime;
                xKeyFrame.Value = scale.X;
                vector3Curve.XTrack.AddKeyframeBack(ref xKeyFrame);

                FKeyframe yKeyFrame = new FKeyframe();
                yKeyFrame.Time = keyTime;
                yKeyFrame.Value = scale.Y;
                vector3Curve.YTrack.AddKeyframeBack(ref yKeyFrame);

                FKeyframe zKeyFrame = new FKeyframe();
                zKeyFrame.Time = keyTime;
                zKeyFrame.Value = scale.Z;
                vector3Curve.ZTrack.AddKeyframeBack(ref zKeyFrame);
            }
            return vector3Curve;
        }
        private static TtQuaternionCurve GenerateRotationCurve(Assimp.NodeAnimationChannel animationChannel, double TicksPerSecond, Assimp.Scene aiScene)
        {
            var node = AssimpSceneUtil.FindNode(animationChannel.NodeName, aiScene);
            List<Assimp.QuaternionKey> aiQuaternionKeys = animationChannel.RotationKeys;
            TtQuaternionCurve curve = new TtQuaternionCurve();
            curve.XTrack = new TtTrack();
            curve.YTrack = new TtTrack();
            curve.ZTrack = new TtTrack();
            curve.WTrack = new TtTrack();
            foreach (var key in aiQuaternionKeys)
            {
                Quaternion quaternion = Quaternion.Identity;
                if (AssimpSceneUtil.IsZUpLeftHandCoordinate(aiScene))
                {
                    if(AssimpSceneUtil.IsSceneRootNode(node.Parent))
                    {
                        var localRotMat = new Matrix4x4(key.Value.GetMatrix());
                        var coordinateConvertMatrix = AssimpSceneUtil.GetCoordinateConvertMatrix(aiScene);
                        var finalMat = localRotMat * coordinateConvertMatrix;
                        finalMat.Decompose(out var uselessScale, out var aiQuat, out var uselessTransition);
                        quaternion = new Quaternion(aiQuat.X, aiQuat.Y, aiQuat.Z, aiQuat.W);
                    }
                    else
                    {
                        quaternion = new Quaternion(key.Value.X, key.Value.Y, key.Value.Z, key.Value.W);
                    }  
                }
                else
                {
                    quaternion = new Quaternion(key.Value.X, key.Value.Y, key.Value.Z, key.Value.W);
                }
                var rot = quaternion.ToEuler();
                float keyTime = (float)(key.Time / TicksPerSecond);
                FKeyframe xKeyFrame = new FKeyframe();
                xKeyFrame.Time = keyTime;
                xKeyFrame.Value = quaternion.X;
                curve.XTrack.AddKeyframeBack(ref xKeyFrame);

                FKeyframe yKeyFrame = new FKeyframe();
                yKeyFrame.Time = keyTime;
                yKeyFrame.Value = quaternion.Y;
                curve.YTrack.AddKeyframeBack(ref yKeyFrame);

                FKeyframe zKeyFrame = new FKeyframe();
                zKeyFrame.Time = keyTime;
                zKeyFrame.Value = quaternion.Z;
                curve.ZTrack.AddKeyframeBack(ref zKeyFrame);

                FKeyframe wKeyFrame = new FKeyframe();
                wKeyFrame.Time = keyTime;
                wKeyFrame.Value = quaternion.W;
                curve.WTrack.AddKeyframeBack(ref wKeyFrame);
            }
            return curve;
        }
    }
}
