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
            Vector4[] extraUVStream = new Vector4[vertexCount];
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
                            var extraUVChannel = subMesh.TextureCoordinateChannels[1];
                            extraUVStream[vertexIndex] = new Vector4(extraUVChannel[j].X, extraUVChannel[j].Y, 0, 0);
                        }
                    }
                }
                vertexCounting += subMesh.VertexCount;
            }

            SetMeshStreams(meshPrimitives, posStream, normalStream, tangentStream, uvStream, extraUVStream, vertexColorStream, null, null, renderIndex16, renderIndex32, isIndex32, indicesCount, vertexCount, false);
            meshPrimitives.MorphTargets = BuildMorphTargetSet(meshes,
                (int meshIdx) => GetVertexPreTransform(meshNodeRefs[meshIdx], scene, importOption), vertexCount);
            return meshPrimitives;
        }
        private static void SetMeshStreams(TtMeshPrimitives meshPrimitives,
            Vector3[] posStream,
            Vector3[] normalStream,
            Vector4[] tangentStream,
            Vector2[] uvStream,
            Vector4[] extraUVStream,
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
                fixed (void* data = extraUVStream)
                {
                    meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_ExtraUV, data, (uint)(sizeof(Vector4) * vertexCount), (uint)sizeof(Vector4), ECpuAccess.CAS_DEFAULT);
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
            Vector4[] extraUVStream = new Vector4[vertextCount];
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
                            var extraUVChannel = subMesh.TextureCoordinateChannels[1];
                            var extraUV = new Vector4(extraUVChannel[j].X, extraUVChannel[j].Y, 0, 0);
                            extraUVStream[vertexIndex] = extraUV;
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
                fixed (void* data = extraUVStream)
                {
                    meshPrimitives.mCoreObject.SetGeomtryMeshStream(cmd.mCoreObject, EVertexStreamType.VST_ExtraUV, data, (uint)(sizeof(Vector4) * vertextCount), (uint)sizeof(Vector4), ECpuAccess.CAS_DEFAULT);
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
        /// 切线 delta 阈值(无量纲向量长度)。与法线取同一量级: 两者都是单位向量之差,
        /// 而且切线的可见性并不比法线低 —— 法线贴图的扰动方向整个挂在切线基上。
        /// </summary>
        const float MorphTangentDeltaThreshold = 1e-3f;

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
            int recomputedNormalShapeCount = 0;
            int unrecoverableNormalShapeCount = 0;
            int recomputedTangentShapeCount = 0;
            int unrecoverableTangentShapeCount = 0;

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

                // 重算法线用的 base 侧数据。与 attachment 无关, 所以按 sub-mesh 缓存:
                // 一个角色几十个 blendshape, 不必每个都重新遍历一遍全部面。
                // 延迟到真的遇到"源没带法线"的 attachment 才算, 免得给正常资产白加开销。
                Vector3[] transformedBasePositions = null;
                Vector3[] recomputedBaseNormals = null;
                bool recomputedNormalsFlipped = false;
                Vector3[] recomputedBaseTangents = null;
                bool recomputedTangentsFlipped = false;

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

                    // 这里要判定的不是"源有没有法线数组", 而是"源的法线能不能产出非零 delta"。
                    // 两者不等价, 而且差别正是一类静默失效的来源: DCC 导出 shape 时常把基础网格的
                    // 法线原样复制一份进去(Blender 的 shape key 未勾选重算时就这样), 于是
                    // HasNormals == true 而 morphNormal - baseNormal 恒等于 0。若只看 HasNormals,
                    // 这种文件会落进死角 —— 既不走下面的重算, 又存不出任何 delta, 表现与
                    // "源完全没带法线"一模一样, 但两者的代码路径不同, 排查时极易误判。
                    var sourceNormalsCarryDelta = attachment.HasNormals && subMesh.HasNormals
                        && SourceNormalsDifferFromBase(subMesh, attachment, count);
                    // 切线同理, 而且比法线更少见: Assimp 的 CalculateTangentSpace
                    // (DefaultSceneFlags 里带着)只处理 aiMesh, 不给 aiAnimMesh 算切线,
                    // 所以绝大多数文件走的都是下面的重算路径。
                    var sourceTangentsCarryDelta = attachment.HasTangentBasis && subMesh.HasTangentBasis
                        && SourceTangentsDifferFromBase(subMesh, attachment, count);

                    // FBX/glTF 里 shape 的法线是可选项, Blender/Maya 导出 shape key 默认不写,
                    // 所以"源没有可用法线"是常态而不是个例。这种情况下不能就存零 delta 了事:
                    // VS 里 vNormal + 0 == vNormal, 形状变了而光照仍按基础网格的朝向算,
                    // 大幅形变(box 变球)时明暗完全不跟着动。
                    // 于是用形变后的顶点位置配合基础网格的面拓扑重算法线。
                    //
                    // 关键取舍: base 法线也用同一套算法从 base 顶点重算, delta = morph' - base',
                    // 而不是拿 DCC 给的 subMesh.Normals 当基准。DCC 的平滑组/硬边处理与这里的
                    // 面积加权平均必然存在差异, 若混用, 那个差异会变成与权重无关的常量偏移,
                    // 于是权重为 0 时法线也被推歪 —— 而权重 0 本该与基础网格逐比特一致。
                    Vector3[] recomputedMorphNormals = null;
                    Vector3[] recomputedMorphTangents = null;
                    // 切线多一个前提: 它是 UV 梯度的方向, 没有 UV0 就无从重算(而法线只需要面拓扑)。
                    var needRecomputedNormals = sourceNormalsCarryDelta == false && subMesh.HasFaces;
                    var needRecomputedTangents = sourceTangentsCarryDelta == false && subMesh.HasFaces
                        && subMesh.TextureCoordinateChannelCount > 0;
                    if (needRecomputedNormals || needRecomputedTangents)
                    {
                        // 形变后的顶点位置是法线与切线重算的共同输入, 算一次共用。
                        if (transformedBasePositions == null)
                        {
                            transformedBasePositions = TransformMorphPositions(subMesh.Vertices,
                                subMesh.VertexCount, null, in vertexPreTransform);
                        }
                        // attachment 顶点数少于 base 时缺的那些回退到 base 位置(= 该处无形变)
                        var morphPositions = TransformMorphPositions(attachment.Vertices,
                            subMesh.VertexCount, transformedBasePositions, in vertexPreTransform);

                        if (needRecomputedNormals)
                        {
                            if (recomputedBaseNormals == null)
                            {
                                recomputedBaseNormals = AccumulateVertexNormals(subMesh.Faces, transformedBasePositions);
                                recomputedNormalsFlipped = ShouldFlipRecomputedVectors(recomputedBaseNormals,
                                    subMesh.HasNormals ? subMesh.Normals : null, in vertexPreTransform);
                                if (recomputedNormalsFlipped)
                                    NegateAll(recomputedBaseNormals);
                            }
                            recomputedMorphNormals = AccumulateVertexNormals(subMesh.Faces, morphPositions);
                            // 翻转决定必须与 base 侧一致, 否则 delta 直接反向
                            if (recomputedNormalsFlipped)
                                NegateAll(recomputedMorphNormals);
                            recomputedNormalShapeCount++;
                        }

                        if (needRecomputedTangents)
                        {
                            // 两侧用同一套 base UV: morph 不改 UV, 所以切线 delta 里装的正好是
                            // "几何变形让 UV 梯度方向在 3D 里转了多少", 而这一项是 VS 里那步
                            // Gram-Schmidt 重投影无论如何也恢复不出来的。
                            var baseUVs = subMesh.TextureCoordinateChannels[0];
                            if (recomputedBaseTangents == null)
                            {
                                recomputedBaseTangents = AccumulateVertexTangents(subMesh.Faces, transformedBasePositions, baseUVs);
                                recomputedTangentsFlipped = ShouldFlipRecomputedVectors(recomputedBaseTangents,
                                    subMesh.HasTangentBasis ? subMesh.Tangents : null, in vertexPreTransform);
                                if (recomputedTangentsFlipped)
                                    NegateAll(recomputedBaseTangents);
                            }
                            recomputedMorphTangents = AccumulateVertexTangents(subMesh.Faces, morphPositions, baseUVs);
                            if (recomputedTangentsFlipped)
                                NegateAll(recomputedMorphTangents);
                            recomputedTangentShapeCount++;
                        }
                    }

                    // 没有面拓扑就无从重算(点云/线段网格), 没有 UV 则算不了切线。这种 shape 只能
                    // 留零 delta, 计数出来在日志里点明, 免得又变成一个查不出原因的"光照不跟着变"。
                    if (sourceNormalsCarryDelta == false && needRecomputedNormals == false)
                        unrecoverableNormalShapeCount++;
                    if (sourceTangentsCarryDelta == false && needRecomputedTangents == false)
                        unrecoverableTangentShapeCount++;

                    for (int j = 0; j < count; j++)
                    {
                        var basePos = vertexPreTransform.TransformPosition(AssimpSceneUtil.ConvertVector3(subMesh.Vertices[j]).AsDVector()).ToSingleVector3();
                        var morphPos = vertexPreTransform.TransformPosition(AssimpSceneUtil.ConvertVector3(attachment.Vertices[j]).AsDVector()).ToSingleVector3();
                        var deltaPosition = morphPos - basePos;

                        var deltaNormal = Vector3.Zero;
                        if (sourceNormalsCarryDelta)
                        {
                            var baseNormal = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(subMesh.Normals[j]));
                            var morphNormal = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(attachment.Normals[j]));
                            deltaNormal = morphNormal - baseNormal;
                        }
                        else if (recomputedMorphNormals != null)
                        {
                            var baseNormal = recomputedBaseNormals[j];
                            var morphNormal = recomputedMorphNormals[j];
                            // 退化顶点(孤立点/零面积面)任一侧为零向量时不给 delta:
                            // morph - 0 或 0 - base 都会把 VS 里的法线推到错的方向上去。
                            if (Vector3.Dot(baseNormal, baseNormal) > 0.0f &&
                                Vector3.Dot(morphNormal, morphNormal) > 0.0f)
                            {
                                deltaNormal = morphNormal - baseNormal;
                            }
                        }

                        var deltaTangent = Vector3.Zero;
                        if (sourceTangentsCarryDelta)
                        {
                            var baseTangent = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(subMesh.Tangents[j]));
                            var morphTangent = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(attachment.Tangents[j]));
                            deltaTangent = morphTangent - baseTangent;
                        }
                        else if (recomputedMorphTangents != null)
                        {
                            var baseTangent = recomputedBaseTangents[j];
                            var morphTangent = recomputedMorphTangents[j];
                            // 与法线同样的退化保护。切线这边零向量更容易出现: UV 退化
                            // (整个三角形卡在同一个 UV 点上, lightmap 接缝处常见)就会让该顶点
                            // 拿不到任何有效的 UV 梯度。
                            if (Vector3.Dot(baseTangent, baseTangent) > 0.0f &&
                                Vector3.Dot(morphTangent, morphTangent) > 0.0f)
                            {
                                deltaTangent = morphTangent - baseTangent;
                            }
                        }

                        if (Vector3.Dot(deltaPosition, deltaPosition) <= MorphPositionDeltaThreshold * MorphPositionDeltaThreshold &&
                            Vector3.Dot(deltaNormal, deltaNormal) <= MorphNormalDeltaThreshold * MorphNormalDeltaThreshold &&
                            Vector3.Dot(deltaTangent, deltaTangent) <= MorphTangentDeltaThreshold * MorphTangentDeltaThreshold)
                        {
                            continue;
                        }

                        var entry = new FMorphVertexDelta();
                        entry.VertexIndex = (uint)(vertexCounting + j);
                        entry.DeltaPosition = deltaPosition;
                        entry.DeltaNormal = deltaNormal;
                        entry.DeltaTangent = deltaTangent;
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

            // 把法线/切线是怎么来的写进日志: 这几条路径产生的资产肉眼无法区分, 而它们的
            // 排查方向完全不同(改导出设置 / 看重算质量 / 补面拓扑或 UV)。
            var normalNote = recomputedNormalShapeCount > 0
                ? $", recomputed normals for {recomputedNormalShapeCount} shape(s) whose source carried no usable normal data"
                : "";
            if (unrecoverableNormalShapeCount > 0)
                normalNote += $", {unrecoverableNormalShapeCount} shape(s) left with zero normal deltas (no face topology to recompute from)";
            if (recomputedTangentShapeCount > 0)
                normalNote += $", recomputed tangents for {recomputedTangentShapeCount} shape(s)";
            if (unrecoverableTangentShapeCount > 0)
                normalNote += $", {unrecoverableTangentShapeCount} shape(s) left with zero tangent deltas (no face topology or no UV0 to recompute from)";
            Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Info,
                $"Imported {result.Targets.Count} morph target(s) for mesh with {totalVertexCount} vertices{normalNote}");
            return result;
        }

        /// <summary>
        /// 源 shape 的法线是否真的与基础网格不同。
        ///
        /// 存在的意义: HasNormals 只能说明数组在不在, 说明不了里面是不是 base 的副本。
        /// 按位置 delta 的阅读习惯会以为"数组在 => 能算出差"而直接相减, 但法线不同:
        /// 位置是形变的定义, 必然不同; 法线却完全依赖导出器愿不愿意重算。
        ///
        /// 只要有一个顶点的差异超过阈值就算"带了信息", 立即返回 —— 真带法线的文件
        /// 通常头几个顶点就能判定, 只有副本型的才会真的跑完全程。
        ///
        /// 在 Assimp 原始空间里比就行: vertexPreTransform 对两侧是同一个变换,
        /// 不会把相等变成不相等, 省下白做的变换。
        /// </summary>
        private static bool SourceNormalsDifferFromBase(Mesh subMesh, MeshAnimationAttachment attachment, int count)
        {
            var baseNormals = subMesh.Normals;
            var morphNormals = attachment.Normals;
            if (baseNormals == null || morphNormals == null)
                return false;

            var limit = System.Math.Min(count, System.Math.Min(baseNormals.Count, morphNormals.Count));
            for (int v = 0; v < limit; v++)
            {
                var diff = AssimpSceneUtil.ConvertVector3(morphNormals[v]) - AssimpSceneUtil.ConvertVector3(baseNormals[v]);
                if (Vector3.Dot(diff, diff) > MorphNormalDeltaThreshold * MorphNormalDeltaThreshold)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 源 shape 的切线是否真的与基础网格不同。理由与 SourceNormalsDifferFromBase 一样:
        /// HasTangentBasis 只能说明数组在不在。切线这边副本的概率反而更高 —— 导出器
        /// 很少为 shape 单独算切线, 拿 base 的直接填上是常见做法。
        /// </summary>
        private static bool SourceTangentsDifferFromBase(Mesh subMesh, MeshAnimationAttachment attachment, int count)
        {
            var baseTangents = subMesh.Tangents;
            var morphTangents = attachment.Tangents;
            if (baseTangents == null || morphTangents == null)
                return false;

            var limit = System.Math.Min(count, System.Math.Min(baseTangents.Count, morphTangents.Count));
            for (int v = 0; v < limit; v++)
            {
                var diff = AssimpSceneUtil.ConvertVector3(morphTangents[v]) - AssimpSceneUtil.ConvertVector3(baseTangents[v]);
                if (Vector3.Dot(diff, diff) > MorphTangentDeltaThreshold * MorphTangentDeltaThreshold)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 把 Assimp 顶点位置数组变换到与 delta 相同的空间。
        ///
        /// 法线必须在变换后的空间里算, 而不是在原始空间算完再变换过去: 非均匀缩放下
        /// 法线需要用逆转置矩阵变换, TransformVector3NoScale 做不到。先变位置再叉积就
        /// 自然避开了这个问题。
        /// </summary>
        /// <param name="vertexCount">输出长度, 以基础网格的顶点数为准(面索引指向的是它)。</param>
        /// <param name="fallback">
        /// vertices 不够长时缺的部分取这里的值(传 base 位置 = 该处无形变); 为 null 则留零向量。
        /// </param>
        private static Vector3[] TransformMorphPositions(List<System.Numerics.Vector3> vertices, int vertexCount,
            Vector3[] fallback, in FTransform vertexPreTransform)
        {
            var result = new Vector3[vertexCount];
            var available = vertices != null ? vertices.Count : 0;
            for (int v = 0; v < vertexCount; v++)
            {
                if (v < available)
                    result[v] = vertexPreTransform.TransformPosition(AssimpSceneUtil.ConvertVector3(vertices[v]).AsDVector()).ToSingleVector3();
                else if (fallback != null && v < fallback.Length)
                    result[v] = fallback[v];
            }
            return result;
        }

        /// <summary>
        /// 面法线累加到顶点后归一化, 得到面积加权的顶点法线。
        /// 叉积不归一化直接累加 —— 它的模长正比于 2x 三角形面积, 所以加权是免费的。
        ///
        /// 注意这里不做按位置的顶点缝合: 硬边/UV 接缝处同一位置的多个顶点会各自算出不同
        /// 法线, 与 DCC 的平滑组结果并不相同。这对 delta 无害: base 与 morph 过的是同一套
        /// 算法, 差值里的系统偏差会相互抵消。
        /// </summary>
        private static Vector3[] AccumulateVertexNormals(List<Assimp.Face> faces, Vector3[] positions)
        {
            var normals = new Vector3[positions.Length];
            for (int f = 0; f < faces.Count; f++)
            {
                var indices = faces[f].Indices;
                if (indices == null || indices.Count < 3)
                    continue;

                // DefaultSceneFlags 带 Triangulate, 正常只会是三角形; 仍按扇形拆分处理多边形,
                // 以防谁拿其他 flags 调进来。
                for (int t = 1; t + 1 < indices.Count; t++)
                {
                    int i0 = indices[0], i1 = indices[t], i2 = indices[t + 1];
                    if (i0 < 0 || i1 < 0 || i2 < 0)
                        continue;
                    if (i0 >= positions.Length || i1 >= positions.Length || i2 >= positions.Length)
                        continue;

                    var faceNormal = Vector3.Cross(positions[i1] - positions[i0], positions[i2] - positions[i0]);
                    normals[i0] += faceNormal;
                    normals[i1] += faceNormal;
                    normals[i2] += faceNormal;
                }
            }

            for (int v = 0; v < normals.Length; v++)
            {
                var lenSq = Vector3.Dot(normals[v], normals[v]);
                // 退化顶点(不被任何面引用, 或周围全是零面积面)留零向量,
                // 由调用方识别并跳过, 不能归一化成个任意方向。
                if (lenSq > 1e-24f)
                    normals[v] = normals[v] * (1.0f / (float)System.Math.Sqrt(lenSq));
                else
                    normals[v] = Vector3.Zero;
            }
            return normals;
        }

        /// <summary>
        /// UV 梯度方向的面切线累加到顶点后归一化。与 AccumulateVertexNormals 一样, 两侧过的是
        /// 同一套算法, 所以与 Assimp 自己的 CalculateTangentSpace 之间的系统差异会在 delta 里抵消。
        ///
        /// 公式源自解 P = p0 + u*T + v*B 的线性方程组:
        ///   T ∝ (duv2.y * e1 - duv1.y * e2) / det,  det = duv1.x*duv2.y - duv2.x*duv1.y
        ///
        /// 这里只取 det 的符号而不除 det, 是有意为之: 一旦除了它, 面切线的模长就变成反比于
        /// UV 面积, 于是 UV 被压得很小的三角形(接缝、碎面)会拿到近乎无限大的权重, 把周围
        /// 顶点的切线全拉过去。去掉这个因子后模长正比于几何尺寸, 与法线那边的面积加权是
        /// 同一个口径; 方向则靠 sign(det) 保住 —— det 为负时真正的 T 是反的, 丢了符号会让
        /// UV 镜像区域的切线与邻区相互抵消。
        /// </summary>
        /// <param name="uvs">UV0 通道, 只用 X/Y。与 positions 同长(不够长的面直接跳过)。</param>
        private static Vector3[] AccumulateVertexTangents(List<Assimp.Face> faces, Vector3[] positions,
            List<System.Numerics.Vector3> uvs)
        {
            var tangents = new Vector3[positions.Length];
            var uvCount = uvs != null ? uvs.Count : 0;
            for (int f = 0; f < faces.Count; f++)
            {
                var indices = faces[f].Indices;
                if (indices == null || indices.Count < 3)
                    continue;

                for (int t = 1; t + 1 < indices.Count; t++)
                {
                    int i0 = indices[0], i1 = indices[t], i2 = indices[t + 1];
                    if (i0 < 0 || i1 < 0 || i2 < 0)
                        continue;
                    if (i0 >= positions.Length || i1 >= positions.Length || i2 >= positions.Length)
                        continue;
                    if (i0 >= uvCount || i1 >= uvCount || i2 >= uvCount)
                        continue;

                    var e1 = positions[i1] - positions[i0];
                    var e2 = positions[i2] - positions[i0];
                    float du1 = uvs[i1].X - uvs[i0].X, dv1 = uvs[i1].Y - uvs[i0].Y;
                    float du2 = uvs[i2].X - uvs[i0].X, dv2 = uvs[i2].Y - uvs[i0].Y;

                    var det = du1 * dv2 - du2 * dv1;
                    // UV 退化(三个顶点共线或重合在 UV 空间里)时方程组无解, 跳过这个面。
                    if (det > -1e-20f && det < 1e-20f)
                        continue;
                    var faceTangent = (e1 * dv2 - e2 * dv1) * (det > 0.0f ? 1.0f : -1.0f);

                    tangents[i0] += faceTangent;
                    tangents[i1] += faceTangent;
                    tangents[i2] += faceTangent;
                }
            }

            for (int v = 0; v < tangents.Length; v++)
            {
                var lenSq = Vector3.Dot(tangents[v], tangents[v]);
                // 退化顶点(没有任何非退化 UV 面引用它)留零向量, 由调用方识别并跳过。
                if (lenSq > 1e-24f)
                    tangents[v] = tangents[v] * (1.0f / (float)System.Math.Sqrt(lenSq));
                else
                    tangents[v] = Vector3.Zero;
            }
            return tangents;
        }

        /// <summary>
        /// 判定重算出的向量场是否整体反向。
        ///
        /// 叉积/UV 梯度的朝向都取决于顶点绕序与 UV 轴方向, 而这两项都被 DefaultSceneFlags 里的
        /// MakeLeftHanded|FlipWindingOrder|FlipUVs 改写过。与其把约定硬编进来(日后改 flags 就默默
        /// 坏掉), 不如拿重算的 base 向量与源文件自带的同类向量整体比对, 反向就翻转。
        ///
        /// 源文件没得比时(sourceVectors 为 null)不翻转: 此时 base 与 morph 用的是同一套约定,
        /// delta 依旧自洽。
        /// </summary>
        private static bool ShouldFlipRecomputedVectors(Vector3[] recomputedBaseVectors,
            List<System.Numerics.Vector3> sourceVectors, in FTransform vertexPreTransform)
        {
            if (sourceVectors == null)
                return false;

            var count = System.Math.Min(recomputedBaseVectors.Length, sourceVectors.Count);
            float dotSum = 0.0f;
            for (int v = 0; v < count; v++)
            {
                var sourceVector = vertexPreTransform.TransformVector3NoScale(AssimpSceneUtil.ConvertVector3(sourceVectors[v]));
                dotSum += Vector3.Dot(recomputedBaseVectors[v], sourceVector);
            }
            return dotSum < 0.0f;
        }

        private static void NegateAll(Vector3[] vectors)
        {
            for (int v = 0; v < vectors.Length; v++)
                vectors[v] = -vectors[v];
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
