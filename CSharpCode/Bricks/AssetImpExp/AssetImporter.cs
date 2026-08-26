using Assimp;
using Assimp.Configs;
using Assimp.Unmanaged;
using EngineNS.Animation.Asset;
using EngineNS.Animation.Base;
using EngineNS.Animation.Curve;
using EngineNS.Animation.SkeletonAnimation.Skeleton;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using EngineNS.Graphics.Mesh;
using EngineNS.NxRHI;
using System.Diagnostics;
using System.Numerics;

namespace EngineNS.Bricks.AssetImpExp
{

    public class TtAssetDescription
    {
        public string FileName { get; set; } = "";
        public string FileFormat { get; set; } = "";
        public string FileFormatVersion { get; set; } = "";
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
        public bool GenerateTexture { get; set; } = true;
        public bool ApplyTransformToVertex { get; set; } = false;
        public bool AsStaticMesh { get; set; } = false;
        public float UnitScale { get; set; } = 1f;
    }
    public class TtAssetImportOption_Animation
    {
        public float Scale { get; set; } = 1f;
        public bool IgnoreScale { get; set; } = true;
    }
    public class TtAssetImporter
    {
        public Assimp.Scene AiScene { get; set; } = null;
        public string FilePath { get; set; } = null;
        public static Assimp.PostProcessSteps DefaultSceneFlags = PostProcessSteps.MakeLeftHanded | PostProcessSteps.FlipUVs | PostProcessSteps.FlipWindingOrder | PostProcessSteps.Triangulate | PostProcessSteps.CalculateTangentSpace;
        public TtAssetDescription PreImport(string filePath)
        {
            FilePath = filePath;
            Assimp.AssimpContext assimpContext = new Assimp.AssimpContext();
            try
            {
                AiScene = assimpContext.ImportFile(filePath, DefaultSceneFlags);
                
                if (AiScene == null)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return BuildAssetDescription();
        }
        public TtAssetDescription PreImport(System.IO.Stream ar)
        {
            //FilePath = filePath;
            Assimp.AssimpContext assimpContext = new Assimp.AssimpContext();
            try
            {
                AiScene = assimpContext.ImportFileFromStream(ar, DefaultSceneFlags);
                if (AiScene == null)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return BuildAssetDescription();
        }
        public TtAssetDescription PreImport(byte[] data)
        {
            //FilePath = filePath;
            Assimp.AssimpContext assimpContext = new Assimp.AssimpContext();
            try
            {
                System.IO.MemoryStream ar = new System.IO.MemoryStream(data);
                AiScene = assimpContext.ImportFileFromStream(ar, DefaultSceneFlags);
                if (AiScene == null)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return BuildAssetDescription();
        }
        private TtAssetDescription BuildAssetDescription()
        {
            TtAssetDescription assetsGenerateDescription = new TtAssetDescription();
            assetsGenerateDescription.FileName = Path.GetFileNameWithoutExtension(FilePath);

            if(AiScene.Metadata.TryGetValue("SourceAsset_Format", out var fileFormat))
            {
                assetsGenerateDescription.FileFormat = (string)fileFormat.Data;
            }
            if(AiScene.Metadata.TryGetValue("SourceAsset_FormatVersion", out var fileFormatversion))
            {
                assetsGenerateDescription.FileFormatVersion = (string)fileFormatversion.Data;
            }
            if(AiScene.Metadata.TryGetValue("SourceAsset_Generator", out var fileGenerator))
            {
                assetsGenerateDescription.Generator = (String)fileGenerator.Data;
            }

            var meshNodes = AssimpSceneUtil.FindMeshNodes(AiScene);
            bool nodeHasScale = false;
            bool nodeHasTranslation = false;
            foreach (var meshNode in meshNodes)
            {
                var preAssimpTransform = AssimpSceneUtil.AccumulatePreTransform(meshNode.Parent);
                var preTransform = AssimpSceneUtil.AssimpMatrix4x4Decompose(preAssimpTransform);
                var nodeTransform = AssimpSceneUtil.AssimpMatrix4x4Decompose(AssimpSceneUtil.GetRowMajorMatrix(meshNode.Transform));
                if (preTransform.scaling != Vector3.One || nodeTransform.scaling != Vector3.One)
                {
                    nodeHasScale = true;
                }
                if (preTransform.translation != Vector3.Zero || nodeTransform.translation != Vector3.Zero)
                {
                    nodeHasTranslation = true;
                }
            }
            
            assetsGenerateDescription.MeshesCount = meshNodes.Count;
            assetsGenerateDescription.MeshesHaveScale = nodeHasScale;
            assetsGenerateDescription.MeshesHaveTranslation = nodeHasTranslation;
            assetsGenerateDescription.AnimationsCount = AiScene.AnimationCount;
            if(AiScene.Metadata.TryGetValue("UpAxisSign", out var upAxisSign))
            {
                string[] Axis = new[] { "X", "Y", "Z" };
                assetsGenerateDescription.UpAxis = (Int32)AiScene.Metadata["UpAxisSign"].Data == -1 ? "-" + Axis[(Int32)AiScene.Metadata["UpAxis"].Data] : Axis[(Int32)AiScene.Metadata["UpAxis"].Data];
            }
            else
            {
                assetsGenerateDescription.UpAxis = "Y";
            }
            if(AiScene.Metadata.TryGetValue("UnitScaleFactor", out var unitScaleFactor))
            {
                assetsGenerateDescription.UnitScaleFactor = (float)unitScaleFactor.Data;
            }

            return assetsGenerateDescription;
        }
        public void ReImport(Assimp.PostProcessSteps sceneFlags)
        {
            try
            {
                Assimp.AssimpContext assimpContext = new Assimp.AssimpContext();
                AiScene = assimpContext.ImportFile(FilePath, sceneFlags);
                if (AiScene == null)
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        public List<Assimp.Animation> GetAnimations()
        {
            return AiScene == null ? null : AiScene.Animations;
        }

        #region TtMeshImportSetting
        public static TtMeshImportSetting CreateMeshImporter(string filePath)
        {
            TtAssetImporter AssetImporter = new TtAssetImporter();
            var assetDescription = AssetImporter.PreImport(filePath);
            if (assetDescription == null)
            {
                return null;
            }
            else
            {
                return ToMeshImportSetting(AssetImporter, assetDescription);
            }
        }
        public static TtMeshImportSetting CreateMeshImporter(System.IO.Stream ar)
        {
            TtAssetImporter AssetImporter = new TtAssetImporter();
            var assetDescription = AssetImporter.PreImport(ar);
            if (assetDescription == null)
            {
                return null;
            }
            else
            {
                return ToMeshImportSetting(AssetImporter, assetDescription);
            }
        }
        public static TtMeshImportSetting CreateMeshImporter(byte[] data)
        {
            TtAssetImporter AssetImporter = new TtAssetImporter();
            var assetDescription = AssetImporter.PreImport(data);
            if (assetDescription == null)
            {
                return null;
            }
            else
            {
                return ToMeshImportSetting(AssetImporter, assetDescription);
            }
        }
        private static TtMeshImportSetting ToMeshImportSetting(TtAssetImporter AssetImporter, TtAssetDescription assetDescription)
        {
            TtMeshImportSetting meshImprotSetting = new TtMeshImportSetting();
            meshImprotSetting.FileName = assetDescription.FileName;
            meshImprotSetting.FileFormat = assetDescription.FileFormat;
            meshImprotSetting.FileFormatVersion = assetDescription.FileFormatVersion;
            meshImprotSetting.MeshesCount = assetDescription.MeshesCount;
            meshImprotSetting.MeshesHaveScale = assetDescription.MeshesHaveScale;
            meshImprotSetting.MeshesHaveTranslation = assetDescription.MeshesHaveTranslation;
            meshImprotSetting.UpAxis = assetDescription.UpAxis;
            meshImprotSetting.UnitScaleFactor = assetDescription.UnitScaleFactor;
            meshImprotSetting.Generator = assetDescription.Generator;
            meshImprotSetting.AssetImporter = AssetImporter;
            return meshImprotSetting;
        }
        #endregion

        #region TtAnimImportSettin
        public static TtAnimImportSetting CreateAnimationImporter(string filePath)
        {
            TtAssetImporter assetImporter = new TtAssetImporter();
            var AssetDescription = assetImporter.PreImport(filePath);
            if (AssetDescription == null)
            {
                return null;
            }
            else
            {
                return ToAnimationImportSetting(assetImporter, AssetDescription);
            }
        }
        public static TtAnimImportSetting CreateAnimationImporter(System.IO.Stream ar)
        {
            TtAssetImporter assetImporter = new TtAssetImporter();
            var AssetDescription = assetImporter.PreImport(ar);
            if (AssetDescription == null)
            {
                return null;
            }
            else
            {
                return ToAnimationImportSetting(assetImporter, AssetDescription);
            }
        }
        public static TtAnimImportSetting CreateAnimationImporter(byte[] data)
        {
            TtAssetImporter assetImporter = new TtAssetImporter();
            var AssetDescription = assetImporter.PreImport(data);
            if (AssetDescription == null)
            {
                return null;
            }
            else
            {
                return ToAnimationImportSetting(assetImporter, AssetDescription);
            }
        }
        private static TtAnimImportSetting ToAnimationImportSetting(TtAssetImporter AssetImporter, TtAssetDescription AssetDescription)
        {
            TtAnimImportSetting animImprotSetting = new TtAnimImportSetting();
            animImprotSetting.FileName = AssetDescription.FileName;
            animImprotSetting.FileFormat = AssetDescription.FileFormat;
            animImprotSetting.FileFormatVersion = AssetDescription.FileFormatVersion;
            animImprotSetting.AnimationsCount = AssetDescription.AnimationsCount;
            animImprotSetting.UpAxis = AssetDescription.UpAxis;
            animImprotSetting.UnitScaleFactor = AssetDescription.UnitScaleFactor;
            animImprotSetting.Generator = AssetDescription.Generator;
            animImprotSetting.AssetImporter = AssetImporter;
            return animImprotSetting;
        }
        #endregion
    }

    public class AssimpSceneUtil
    {
        public static bool IsZUpLeftHandCoordinate(Assimp.Scene scene)
        {
            //the scene has already convert to left hand when read
            if(scene.Metadata.TryGetValue("UpAxis", out var upAxis))
            {
                var upAxisValue = upAxis.DataAs<int>();
                return upAxisValue == 2;
            }
            return true;
        }
        public static bool IsFBXFile(Assimp.Scene scene)
        {
            if (scene.Metadata.TryGetValue("SourceAsset_Format", out var fileFormat))
            {
                var fileFormatValue = (string)fileFormat.Data;
                if(fileFormatValue.ToLower().Contains("fbx"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public static System.Numerics.Matrix4x4 GetCoordinateConvertMatrix(Assimp.Scene scene)
        {
            if (!IsFBXFile(scene))
            {
                return System.Numerics.Matrix4x4.Identity;
            }

            var upAxis = scene.Metadata["UpAxis"].DataAs<int>();
            var upAxisSign = scene.Metadata["UpAxisSign"].DataAs<int>();
            var frontAxis = scene.Metadata["FrontAxis"].DataAs<int>();
            var frontAxisSign = scene.Metadata["FrontAxisSign"].DataAs<int>();
            var coordAxis = scene.Metadata["CoordAxis"].DataAs<int>();
            var coordAxisSign = scene.Metadata["CoordAxisSign"].DataAs<int>();
            System.Numerics.Vector3 upVec = upAxis == 0 ? new System.Numerics.Vector3(upAxisSign.Value, 0, 0) : upAxis == 1 ? new System.Numerics.Vector3(0, upAxisSign.Value, 0) : new System.Numerics.Vector3(0, 0, upAxisSign.Value);
            System.Numerics.Vector3 forwardVec = frontAxis == 0 ? new System.Numerics.Vector3(frontAxisSign.Value, 0, 0) : frontAxis == 1 ? new System.Numerics.Vector3(0, frontAxisSign.Value, 0) : new System.Numerics.Vector3(0, 0, frontAxisSign.Value);
            System.Numerics.Vector3 rightVec = coordAxis == 0 ? new System.Numerics.Vector3(coordAxisSign.Value, 0, 0) : coordAxis == 1 ? new System.Numerics.Vector3(0, coordAxisSign.Value, 0) : new System.Numerics.Vector3(0, 0, coordAxisSign.Value);
            System.Numerics.Matrix4x4 coordinateConvert = new System.Numerics.Matrix4x4(rightVec.X, rightVec.Y, rightVec.Z, 0.0f,
                                                            upVec.X, upVec.Y, upVec.Z, 0.0f,
                                                            forwardVec.X, forwardVec.Y, forwardVec.Z, 0.0f,
                                                            0.0f, 0.0f, 0.0f, 1.0f);

            return coordinateConvert;
        }

        public static List<Assimp.Node> FindMeshNodes(Assimp.Scene scene)
        {
            List<Assimp.Node> meshNodes = new List<Assimp.Node>();
            FindAllNodesContainsMeshRecursively(scene, scene.RootNode, ref meshNodes);
            return meshNodes;
        }
        public static void IterateNode(Assimp.Scene scene, Assimp.Node node, Action<Assimp.Node> action)
        {
            action(node);
            foreach (var child in node.Children)
            {
                IterateNode(scene, child, action);
            }
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
        public static Vector2 ConvertVector2(System.Numerics.Vector2 value)
        {
            return new Vector2(value.X, value.Y);
        }
        public static Vector2 ConvertVector2(float x, float y)
        {
            return new Vector2(x, y);
        }
        public static Vector3 ConvertVector3(System.Numerics.Vector3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }
        public static Color4f ConvertColor(System.Numerics.Vector4 value)
        {
            return new Color4f(value.X, value.Y, value.Z, value.W);
        }
        public static Quaternion ConvertQuaternion(System.Numerics.Quaternion value)
        {
            var q = new Quaternion(value.X, value.Y, value.Z, value.W) * Quaternion.FromEuler(new FRotator(0, 90 * FRotator.D2R, 0));
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static System.Numerics.Matrix4x4 AccumulatePreTransform(Assimp.Node preTransformNode)
        {
            var transform = GetRowMajorMatrix(preTransformNode.Transform);
            if (IsParentIs_AssimpFbxPre_Node(preTransformNode))
            {
                return transform * AccumulatePreTransform(preTransformNode.Parent);
            }
            else
            {
                return transform;
            }
        }
        public static System.Numerics.Matrix4x4 GetAbsBoneNodeMatrix(Assimp.Node node, Assimp.Scene scene)
        {
            var nodeTransform = GetRowMajorMatrix(node.Transform);
            if (AssimpSceneUtil.IsSceneRootNode(node.Parent, scene) || node.Parent.HasMeshes)
            {
                return nodeTransform;
            }
            else if (AssimpSceneUtil.IsParentIs_AssimpFbxPre_Node(node))
            {
                return nodeTransform * GetRowMajorMatrix(node.Parent.Transform);
            }
            else
            {
                return nodeTransform * GetAbsBoneNodeMatrix(node.Parent, scene);
            }
        }
        public static System.Numerics.Matrix4x4 GetAbsNodeMatrix(Assimp.Node node, Assimp.Scene scene)
        {
            var nodeTransform = GetRowMajorMatrix(node.Transform);
            if (AssimpSceneUtil.IsSceneRootNode(node.Parent, scene) || node.Parent.HasMeshes)
            {
                return nodeTransform;
            }
            else if (AssimpSceneUtil.IsParentIs_AssimpFbxPre_Node(node))
            {
                return nodeTransform * GetRowMajorMatrix(node.Parent.Transform);
            }
            else
            {
                System.Numerics.Matrix4x4.Decompose(nodeTransform, out var scaling, out var rotation, out var translation);
                if (scaling.X < 0 || scaling.Y < 0 || scaling.Z < 0)
                {
                    
                    var transMat = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);

                    return transMat * GetAbsNodeMatrix(node.Parent, scene);
                }
                else
                {
                    return nodeTransform * GetAbsNodeMatrix(node.Parent, scene);
                }
            }
        }

        public static bool IsSceneRootNode(Assimp.Node node, Assimp.Scene scene)
        {
            if (node == scene.RootNode)
            {
                return true;
            }
            return false;
        }
        public static System.Numerics.Matrix4x4 GetAssimpFbxGeometricTranslation(Assimp.Node node)
        {
            if (node.Parent.Name.Contains(node.Name) && IsParentIs_AssimpFbxPre_Node(node) && node.Parent.Name.Contains("GeometricTranslation"))
            {
                return GetRowMajorMatrix(node.Parent.Transform);
            }
            return Matrix4x4.Identity;
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
        public static System.Numerics.Matrix4x4 GetRowMajorMatrix(System.Numerics.Matrix4x4 matrix)
        {
            return Matrix4x4.Transpose(matrix);
        }
        public static (Vector3 scaling, Quaternion rotation, Vector3 translation) AssimpMatrix4x4Decompose(System.Numerics.Matrix4x4 matrix)
        {
            System.Numerics.Vector3 assimpTrans, assimpScaling;
            System.Numerics.Quaternion assimpQuat;
            System.Numerics.Matrix4x4.Decompose(matrix, out assimpScaling, out assimpQuat, out assimpTrans);
            var translation = new Vector3(assimpTrans.X, assimpTrans.Y, assimpTrans.Z);
            var scaling = new Vector3(assimpScaling.X, assimpScaling.Y, assimpScaling.Z);
            var rotation = new Quaternion(assimpQuat.X, assimpQuat.Y, assimpQuat.Z, assimpQuat.W);
            return (scaling, rotation, translation);
        }

        public static FTransform AssimpMatrix4x4DecomposeToTransform(System.Numerics.Matrix4x4 matrix)
        {
            var result = AssimpMatrix4x4Decompose(matrix);
            return FTransform.CreateTransform(result.translation.AsDVector(), result.scaling, result.rotation);
        }
        public static Matrix AssimpMatrix4x4ToTtMatrix(Matrix4x4 matrix)
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
                    if (boneNode == null)
                    {
                        EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtIOCategory>(EngineNS.Profiler.ELogTag.Warning, $"Assimp bone node is missing: {bone.Name}");
                        continue;
                    }
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
            if (AssimpSceneUtil.IsSceneRootNode(parent, scene) || parent.HasMeshes)
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
                EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtIOCategory>(EngineNS.Profiler.ELogTag.Warning, $"Skip invalid skeleton node: {node?.Name}");
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
            return null;
        }
        
        static TtBoneDesc MakeBoneDesc(Assimp.Scene scene, Node boneNode, Node rootBoneNode, TtAssetImportOption_Mesh importOption)
        {
            TtBoneDesc boneDesc = new TtBoneDesc();
            boneDesc.Name = boneNode.Name;
            boneDesc.NameHash = Standart.Hash.xxHash.xxHash32.ComputeHash(boneDesc.Name);
            var parentNode = GetValidParentNode(boneNode, scene);
            if (rootBoneNode != boneNode && parentNode != null && !AssimpSceneUtil.IsSceneRootNode(parentNode, scene) && !parentNode.HasMeshes)
            {
                boneDesc.ParentName = parentNode.Name;
                boneDesc.ParentHash = Standart.Hash.xxHash.xxHash32.ComputeHash(boneDesc.ParentName);
            }

            var boneAbsNodeTransform = AssimpSceneUtil.GetAbsBoneNodeMatrix(boneNode, scene);


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
                var skeleton = MakeSkeleton(scene, skeletonRootNode, importOption, inOutNodesMap, null);
                if (skeleton.Limbs.Count > 0)
                    skeletonsGenerate.Add(skeleton);
            }
            else
            {
                foreach (var skeletonRootNode in skeletonRootNodes)
                {
                    var skeletonNodes = new HashSet<Assimp.Node>();
                    foreach (var marked in inOutNodesMap)
                    {
                        if (marked.Value && IsNodeUnderSkeletonRoot(marked.Key, skeletonRootNode, scene))
                        {
                            skeletonNodes.Add(marked.Key);
                        }
                    }

                    if (skeletonNodes.Count == 0)
                        continue;

                    var skeleton = MakeSkeleton(scene, skeletonRootNode, importOption, inOutNodesMap, skeletonNodes);
                    if (skeleton.Limbs.Count > 0)
                        skeletonsGenerate.Add(skeleton);
                }
            }
            return skeletonsGenerate;
        }
        static TtSkinSkeleton MakeSkeleton(Assimp.Scene scene, Assimp.Node skeletonRootNode, TtAssetImportOption_Mesh importOption, Dictionary<Assimp.Node, bool> nodesMap, HashSet<Assimp.Node> skeletonNodes)
        {
            TtSkinSkeleton skeleton = new TtSkinSkeleton();
            foreach (var marked in nodesMap)
            {
                if (!marked.Value)
                    continue;
                if (skeletonNodes != null && !skeletonNodes.Contains(marked.Key))
                    continue;

                TtBoneDesc boneDesc = MakeBoneDesc(scene, marked.Key, skeletonRootNode, importOption);
                if (skeletonNodes != null && !string.IsNullOrEmpty(boneDesc.ParentName))
                {
                    var parentNode = GetValidParentNode(marked.Key, scene);
                    if (parentNode == null || !skeletonNodes.Contains(parentNode))
                    {
                        boneDesc.ParentName = null;
                        boneDesc.ParentHash = 0;
                    }
                }
                skeleton.AddLimb(new TtBone(boneDesc));
            }
            skeleton.ConstructHierarchy();
            return skeleton;
        }
        static bool IsNodeUnderSkeletonRoot(Assimp.Node node, Assimp.Node skeletonRootNode, Assimp.Scene scene)
        {
            var current = node;
            while (current != null && !AssimpSceneUtil.IsSceneRootNode(current, scene))
            {
                if (current == skeletonRootNode)
                    return true;
                current = GetValidParentNode(current, scene);
            }
            return current == skeletonRootNode;
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
        public struct TtExpMeshData
        {
            public TtMeshPrimitives Mesh;
            public List<Mesh> Materials;
        }
        public static List<TtExpMeshData> Generate(Assimp.Scene scene, TtAssetImportOption_Mesh importOption)
        {
            var meshNodes = AssimpSceneUtil.FindMeshNodes(scene);
            var skeletons = SkeletonGenerater.Generate(scene, importOption);
            return Generate(meshNodes, skeletons, scene, importOption);
        }
        public static List<TtExpMeshData> Generate(List<TtSkinSkeleton> meshSkeletons, Assimp.Scene scene, TtAssetImportOption_Mesh importOption)
        {
            var meshNodes = AssimpSceneUtil.FindMeshNodes(scene);
            return Generate(meshNodes, meshSkeletons, scene, importOption);
        }
        public static List<TtExpMeshData> GenerateMerged(string meshName, Assimp.Scene scene, TtAssetImportOption_Mesh importOption)
        {
            var meshNodes = AssimpSceneUtil.FindMeshNodes(scene);
            var merged = CreateMergedMeshPrimitives(meshName, meshNodes, scene, importOption, out var meshes);
            if (merged == null)
                return new List<TtExpMeshData>();
            return new List<TtExpMeshData>() { new TtExpMeshData() { Mesh = merged, Materials = meshes } };
        }
        public static List<NxRHI.TtSrView> GenerateTextures(List<TtSkinSkeleton> meshSkeletons, Assimp.Scene scene, TtAssetImportOption_Mesh importOption)
        {
            return null;
        }
        private static List<TtExpMeshData> Generate(List<Assimp.Node> meshNodes, List<TtSkinSkeleton> meshSkeletons, Assimp.Scene scene, TtAssetImportOption_Mesh importOption)
        {
            List<TtExpMeshData> meshPrimitives = new List<TtExpMeshData>();
            foreach (var meshNode in meshNodes)
            {
                var skeleton = AssimpSceneUtil.FindMeshSkeleton(meshNode, meshSkeletons, scene);
                List<Mesh> meshes;
                var meshPrimitive = CreateMeshPrimitives(meshNode, skeleton, scene, importOption, out meshes);
                meshPrimitive.PartialSkeleton = skeleton;
                meshPrimitives.Add(new TtExpMeshData() { Mesh = meshPrimitive, Materials = meshes });
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
        private static FTransform GetVertexPreTransform(Assimp.Node meshNode, Assimp.Scene scene, TtAssetImportOption_Mesh importOption)
        {
            var preAssimpTransform = Matrix4x4.Identity;
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

            var transformTuple = AssimpSceneUtil.AssimpMatrix4x4DecomposeToTransform(preAssimpTransform);
            if (importOption.ApplyTransformToVertex)
            {
                var nodeTransform = AssimpSceneUtil.AssimpMatrix4x4DecomposeToTransform(AssimpSceneUtil.GetAbsNodeMatrix(meshNode, scene));
                FTransform finalTransform;
                FTransform.Multiply(out finalTransform, transformTuple, nodeTransform);
                return FTransform.CreateTransform(finalTransform.Position,
                    finalTransform.Scale * importOption.UnitScale, finalTransform.Quat);
            }
            else
            {
                return FTransform.CreateTransform(Vector3.Zero.AsDVector(),
                    transformTuple.Scale * importOption.UnitScale, transformTuple.Quat);
            }
        }
        private static TtMeshPrimitives CreateMergedMeshPrimitives(string meshName, List<Assimp.Node> meshNodes, Assimp.Scene scene, TtAssetImportOption_Mesh importOption, out List<Mesh> meshes)
        {
            meshes = new List<Mesh>();
            var meshNodeRefs = new List<Assimp.Node>();
            foreach (var meshNode in meshNodes)
            {
                var validMeshes = GetValidMesh(meshNode, scene);
                foreach (var mesh in validMeshes)
                {
                    meshes.Add(mesh);
                    meshNodeRefs.Add(meshNode);
                }
            }
            if (meshes.Count == 0)
                return null;

            TtMeshPrimitives meshPrimitives = new TtMeshPrimitives(meshName, (uint)meshes.Count);
            int vertexCount = 0;
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
                vertexCount += subMesh.VertexCount;
                nextStartIndex += atomDesc.NumPrimitives * 3;
                indicesCount += subMesh.GetIndices().ToList().Count;
            }

            bool hasVertexColor = true;
            for (int i = 0; i < meshes.Count; i++)
            {
                if (!meshes[i].HasVertexColors(0))
                {
                    hasVertexColor = false;
                    break;
                }
            }

            Vector3[] posStream = new Vector3[vertexCount];
            Vector3[] normalStream = new Vector3[vertexCount];
            Vector4[] tangentStream = new Vector4[vertexCount];
            Vector2[] uvStream = new Vector2[vertexCount];
            Vector4[] lightMapStream = new Vector4[vertexCount];
            UInt32[] vertexColorStream = hasVertexColor ? new UInt32[vertexCount] : null;
            bool isIndex32 = indicesCount > 65535;
            UInt16[] renderIndex16 = isIndex32 ? null : new UInt16[indicesCount];
            UInt32[] renderIndex32 = isIndex32 ? new UInt32[indicesCount] : null;

            int indicesIndex = 0;
            int vertexCounting = 0;
            for (int i = 0; i < meshes.Count; i++)
            {
                var subMesh = meshes[i];
                var vertexPreTransform = GetVertexPreTransform(meshNodeRefs[i], scene, importOption);

                var meshIndices = subMesh.GetIndices().ToList();
                for (int j = 0; j < meshIndices.Count; j++)
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

                for (int j = 0; j < subMesh.VertexCount; j++)
                {
                    var vertexIndex = vertexCounting + j;
                    posStream[vertexIndex] = vertexPreTransform.TransformPosition(AssimpSceneUtil.ConvertVector3(subMesh.Vertices[j]).AsDVector()).ToSingleVector3();
                    normalStream[vertexIndex] = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(subMesh.Normals[j]));

                    if (subMesh.HasTangentBasis)
                    {
                        var normal = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(subMesh.Normals[j]));
                        var tangent = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(subMesh.Tangents[j]));
                        var binTan = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(subMesh.BiTangents[j]));
                        float dp = Vector3.Dot(Vector3.Cross(normal, tangent), binTan);
                        float w = dp > 0.0f ? 1.0f : -1.0f;
                        tangentStream[vertexIndex] = new Vector4(tangent, w);
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
                            lightMapStream[vertexIndex] = new Vector4(lightMapChannel[j].X, lightMapChannel[j].Y, 0, 0);
                        }
                    }
                }
                vertexCounting += subMesh.VertexCount;
            }

            SetMeshStreams(meshPrimitives, posStream, normalStream, tangentStream, uvStream, lightMapStream, vertexColorStream, null, null, renderIndex16, renderIndex32, isIndex32, indicesCount, vertexCount, false);
            meshPrimitives.MorphTargets = BuildMorphTargetSet(meshes,
                (int meshIdx) => GetVertexPreTransform(meshNodeRefs[meshIdx], scene, importOption), vertexCount);
            return meshPrimitives;
        }
        private static void SetMeshStreams(TtMeshPrimitives meshPrimitives,
            Vector3[] posStream,
            Vector3[] normalStream,
            Vector4[] tangentStream,
            Vector2[] uvStream,
            Vector4[] lightMapStream,
            UInt32[] vertexColorStream,
            Byte[] skinIndexsStream,
            float[] skinWeightsStream,
            UInt16[] renderIndex16,
            UInt32[] renderIndex32,
            bool isIndex32,
            int indicesCount,
            int vertexCount,
            bool bHasSkin)
        {
            var cmd = TtEngine.Instance.GfxDevice.RenderContext.CreateCommandList();
            unsafe
            {
                fixed (void* data = posStream)
                {
                    meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_Position, data, (uint)(sizeof(Vector3) * vertexCount), (uint)sizeof(Vector3), ECpuAccess.CAS_DEFAULT);
                }
                fixed (void* data = vertexColorStream)
                {
                    if (vertexColorStream != null)
                    {
                        meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_Color, data, (uint)(sizeof(uint) * vertexCount), (uint)sizeof(uint), ECpuAccess.CAS_DEFAULT);
                    }
                }
                fixed (void* data = normalStream)
                {
                    meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_Normal, data, (uint)(sizeof(Vector3) * vertexCount), (uint)sizeof(Vector3), ECpuAccess.CAS_DEFAULT);
                }
                fixed (void* data = tangentStream)
                {
                    meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_Tangent, data, (uint)(sizeof(Vector4) * vertexCount), (uint)sizeof(Vector4), ECpuAccess.CAS_DEFAULT);
                }
                fixed (void* data = uvStream)
                {
                    meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_UV, data, (uint)(sizeof(Vector2) * vertexCount), (uint)sizeof(Vector2), ECpuAccess.CAS_DEFAULT);
                }
                fixed (void* data = lightMapStream)
                {
                    meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_LightMap, data, (uint)(sizeof(Vector4) * vertexCount), (uint)sizeof(Vector4), ECpuAccess.CAS_DEFAULT);
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
                        meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_SkinIndex, data, (uint)(sizeof(Byte) * vertexCount * 4), 4 * (uint)sizeof(Byte), ECpuAccess.CAS_DEFAULT);
                    }
                    fixed (void* data = skinWeightsStream)
                    {
                        meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_SkinWeight, data, (uint)(sizeof(float) * vertexCount * 4), 4 * (uint)sizeof(float), ECpuAccess.CAS_DEFAULT);
                    }
                }
            }

            BoundingBox aabb = new BoundingBox();
            for (int i = 0; i < vertexCount; i++)
            {
                aabb.Merge(posStream[i]);
            }
            meshPrimitives.mCoreObject.SetAABB(ref aabb);
        }
        private static TtMeshPrimitives CreateMeshPrimitives(Assimp.Node meshNode, TtSkinSkeleton skeleton, Assimp.Scene scene, TtAssetImportOption_Mesh importOption, out List<Mesh> meshes)
        {
            Debug.Assert(meshNode.MeshCount > 0);
            var vertexPreTransform = GetVertexPreTransform(meshNode, scene, importOption);

            meshes = GetValidMesh(meshNode, scene);

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
                indicesCount += subMesh.GetIndices().ToList().Count;
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
                    if (skeleton != null)
                    {
                        bHasSkin = true;
                        meshPrimitives.PartialSkeleton = skeleton;
                    }
                    else
                    {
                        EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtIOCategory>(EngineNS.Profiler.ELogTag.Warning, $"Mesh {meshNode.Name} has skin data but no matching skeleton. Import as static mesh for this node.");
                    }
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
                var meshIndices = subMesh.GetIndices().ToList();
                for (int j = 0; j < meshIndices.Count; j++)
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
                        var normal = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(subMesh.Normals[j]));
                        var tangent = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(subMesh.Tangents[j]));
                        var binTan = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(subMesh.BiTangents[j]));
                        float dp = Vector3.Dot(Vector3.Cross(normal, tangent), binTan);
                        float w = dp > 0.0f ? 1.0f : -1.0f;
                        tangentStream[vertexIndex] = new Vector4(tangent, w);
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
                        var limb = skeleton.FindLimb(bone.Name);
                        if (limb == null || !limb.Index.IsValid())
                        {
                            EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtIOCategory>(EngineNS.Profiler.ELogTag.Warning, $"Skip skin weights for missing bone: {bone.Name}");
                            continue;
                        }
                        var boneIndex = limb.Index;
                        for (int k = 0; k < bone.VertexWeightCount; k++)
                        {
                            var weight = bone.VertexWeights[k];
                            var vertexId = weight.VertexID;
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
                    if (totalWeight <= float.Epsilon)
                    {
                        skinIndexsStream[i * 4] = 0;
                        skinWeightsStream[i * 4] = 1.0f;
                        for (int j = 1; j < 4; ++j)
                        {
                            skinIndexsStream[i * 4 + j] = 0;
                            skinWeightsStream[i * 4 + j] = 0;
                        }
                        continue;
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

            meshPrimitives.MorphTargets = BuildMorphTargetSet(meshes,
                (int meshIdx) => vertexPreTransform, vertextCount);

            return meshPrimitives;
        }

        /// <summary>
        /// 位置 delta 小于此长度的顶点不入稀疏表。单位: 米 (CodingGuidelines.md §6),
        /// 1e-5 m = 0.01 mm, 远低于可见阈值, 主要用于滤除 DCC 导出的浮点噪声。
        /// </summary>
        const float MorphPositionDeltaThreshold = 1e-5f;

        /// <summary> 法线 delta 阈值(无量纲向量长度)。 </summary>
        const float MorphNormalDeltaThreshold = 1e-3f;

        /// <summary>
        /// 从 Assimp 的 MeshAnimationAttachments (即 BlendShape / Morph Target) 提取稀疏 delta。
        ///
        /// 索引对齐的依据: 引擎顶点编号 = vertexCounting + j, 其中 j 就是 Assimp 的顶点下标
        /// (见本文件两处 "posStream[vertexCounting + j] = ... subMesh.Vertices[j]" 的写法);
        /// 而 aiAnimMesh 的 Vertices/Normals 数组与 aiMesh 的同序同长, 所以两边可以直接对应,
        /// 不需要像 FBX SDK 那条路径那样做"控制点 -> 展开顶点"的重映射。
        ///
        /// delta 必须在变换后的空间里相减: 两个端点各自过一遍 vertexPreTransform 再作差,
        /// 这样平移分量自然抵消, 旋转与缩放也被正确带入。
        /// </summary>
        /// <returns>没有任何有效 morph 时返回 null (绝大多数 mesh 都是这种情况)。</returns>
        private static TtMorphTargetSet BuildMorphTargetSet(List<Mesh> meshes,
            System.Func<int, FTransform> getVertexPreTransform, int totalVertexCount)
        {
            if (meshes == null || totalVertexCount <= 0)
                return null;

            // 同名 morph 可能出现在多个 sub-mesh 上(头/身体各一份), 合并成一个 target
            var deltasByName = new Dictionary<string, List<FMorphVertexDelta>>();
            var orderedNames = new List<string>();

            int vertexCounting = 0;
            for (int i = 0; i < meshes.Count; i++)
            {
                var subMesh = meshes[i];
                var attachments = subMesh.MeshAnimationAttachments;
                if (attachments == null || attachments.Count == 0)
                {
                    vertexCounting += subMesh.VertexCount;
                    continue;
                }

                var vertexPreTransform = getVertexPreTransform(i);
                for (int a = 0; a < attachments.Count; a++)
                {
                    var attachment = attachments[a];
                    if (attachment == null || attachment.HasVertices == false)
                        continue;

                    // Assimp 对部分格式不一定给出 channel 名, 回退到稳定的位置命名
                    var morphName = string.IsNullOrEmpty(attachment.Name) ? $"Morph_{i}_{a}" : attachment.Name;
                    if (deltasByName.TryGetValue(morphName, out var deltaList) == false)
                    {
                        deltaList = new List<FMorphVertexDelta>();
                        deltasByName.Add(morphName, deltaList);
                        orderedNames.Add(morphName);
                    }

                    var count = System.Math.Min(attachment.VertexCount, subMesh.VertexCount);
                    var hasNormals = attachment.HasNormals && subMesh.HasNormals;
                    for (int j = 0; j < count; j++)
                    {
                        var basePos = vertexPreTransform.TransformPosition(AssimpSceneUtil.ConvertVector3(subMesh.Vertices[j]).AsDVector()).ToSingleVector3();
                        var morphPos = vertexPreTransform.TransformPosition(AssimpSceneUtil.ConvertVector3(attachment.Vertices[j]).AsDVector()).ToSingleVector3();
                        var deltaPosition = morphPos - basePos;

                        var deltaNormal = Vector3.Zero;
                        if (hasNormals)
                        {
                            var baseNormal = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(subMesh.Normals[j]));
                            var morphNormal = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(attachment.Normals[j]));
                            deltaNormal = morphNormal - baseNormal;
                        }

                        if (Vector3.Dot(deltaPosition, deltaPosition) <= MorphPositionDeltaThreshold * MorphPositionDeltaThreshold &&
                            Vector3.Dot(deltaNormal, deltaNormal) <= MorphNormalDeltaThreshold * MorphNormalDeltaThreshold)
                        {
                            continue;
                        }

                        var entry = new FMorphVertexDelta();
                        entry.VertexIndex = (uint)(vertexCounting + j);
                        entry.DeltaPosition = deltaPosition;
                        entry.DeltaNormal = deltaNormal;
                        deltaList.Add(entry);
                    }
                }
                vertexCounting += subMesh.VertexCount;
            }

            var result = new TtMorphTargetSet();
            result.VertexCount = totalVertexCount;
            for (int i = 0; i < orderedNames.Count; i++)
            {
                var deltaList = deltasByName[orderedNames[i]];
                if (deltaList.Count == 0)
                    continue;
                var target = new TtMorphTarget();
                target.Name = orderedNames[i];
                target.Deltas = deltaList.ToArray();
                result.Targets.Add(target);
            }

            if (result.IsValid == false)
                return null;

            Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Info,
                $"Imported {result.Targets.Count} morph target(s) for mesh with {totalVertexCount} vertices");
            return result;
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
        private static System.Numerics.Vector3 GetScaleRecursively(Node node, Assimp.Animation aiAnim, Assimp.Scene scene, int keyIndex)
        {
            if(AssimpSceneUtil.IsSceneRootNode(node, scene))
            {
                return System.Numerics.Vector3.One;
            }
            var element = FindNodeAnimationChannel(node.Name, aiAnim, scene);
            if(element != null)
            {
                var scales = element.ScalingKeys;
                var finalIndex = scales.Count > keyIndex ? keyIndex : scales.Count - 1;
                return scales[finalIndex].Value * GetScaleRecursively(node.Parent, aiAnim, scene, keyIndex);
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
                objectBinding.ClassType = EngineNS.Rtti.TtTypeDesc.TypeOf(typeof(EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableBonePose));
                objectBinding.Name = element.NodeName;

                // position
                {
                    var node = AssimpSceneUtil.FindNode(element.NodeName, aiScene);
                    System.Numerics.Matrix4x4.Decompose(AssimpSceneUtil.GetRowMajorMatrix(node.Transform), out var s, out var r, out var t);
                    var curve = GeneratePositionCurve(element, aiAnim.TicksPerSecond, aiAnim, aiScene, importOption);
                    animChunk.AnimCurvesList.Add(curve.Id, curve);

                    TtAnimatedPropertyDescription propertyBinding = new TtAnimatedPropertyDescription();
                    propertyBinding.ClassType = EngineNS.Rtti.TtTypeDesc.TypeOf<FNullableVector3>();
                    propertyBinding.Name = "Position";
                    propertyBinding.CurveId = curve.Id;
                    objectBinding.TranslationProperty = propertyBinding;
                }

                // rotation
                {
                    var curve = GenerateRotationCurve(element, aiAnim.TicksPerSecond, aiScene);
                    animChunk.AnimCurvesList.Add(curve.Id, curve);

                    TtAnimatedPropertyDescription propertyBinding = new TtAnimatedPropertyDescription();
                    propertyBinding.ClassType = EngineNS.Rtti.TtTypeDesc.TypeOf<FNullableVector3>();
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
                    propertyBinding.ClassType = EngineNS.Rtti.TtTypeDesc.TypeOf<FNullableVector3>();
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
                var transition = posKey.Value;
                if (AssimpSceneUtil.IsZUpLeftHandCoordinate(aiScene))
                {
                    //var coordinateConvertMatrix = AssimpSceneUtil.GetCoordinateConvertMatrix(aiScene);
                    //var finalMat = Matrix4x4.FromTranslation(transition) * coordinateConvertMatrix;
                    //finalMat.Decompose(out var uselessScaling, out var uselessRotation, out transition);
                    //transition = new Vector3D(transition.X, transition.Y, transition.Z);
                }

                var finalScale = importOption.Scale * System.Numerics.Vector3.One;
                if (importOption.IgnoreScale)
                {
                    Debug.Assert(posKey.Time == scaleKeys[i].Time);
                    finalScale = finalScale * GetScaleRecursively(node, aiAnim, aiScene, i);
                }

                var finalPos = transition * finalScale;
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
                var scale = key.Value;
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
                    if(AssimpSceneUtil.IsSceneRootNode(node.Parent, aiScene))
                    {
                        var localRotMat = Matrix4x4.CreateFromQuaternion(key.Value);
                        var coordinateConvertMatrix = AssimpSceneUtil.GetCoordinateConvertMatrix(aiScene);
                        var finalMat = localRotMat * coordinateConvertMatrix;
                        Matrix4x4.Decompose(finalMat, out var uselessScale, out var aiQuat, out var uselessTransition);
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
