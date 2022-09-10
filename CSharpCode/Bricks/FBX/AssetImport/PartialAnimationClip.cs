using EngineNS.EGui.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Asset
{
    public partial class UAnimationClip
    {
        public partial class ImportAttribute
        {
            AssetImportAndExport.FBX.FBXImporter mFBXImporter; //for now we only have one file to import
            public unsafe partial void FBXCreateCreateDraw(UContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import Animation", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                var visible = true;
                if (ImGuiAPI.BeginPopupModal($"Import Animation", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var sz = new Vector2(-1, 0);
                    if (ImGuiAPI.Button("Select FBX", in sz))
                    {
                        mFileDialog.OpenModal("ChooseFileDlgKey", "Choose File", ".FBX,.fbx", ".");
                    }
                    // display
                    if (mFileDialog.DisplayDialog("ChooseFileDlgKey"))
                    {
                        // action if OK
                        if (mFileDialog.IsOk() == true)
                        {
                            mSourceFile = mFileDialog.GetFilePathName();
                            string filePath = mFileDialog.GetCurrentPath();
                            if (!string.IsNullOrEmpty(mSourceFile))
                            {
                                mFBXImporter = UEngine.Instance.FBXFactoryModule.Instance.CreateImporter();
                                var fileDesc = mFBXImporter.PreImport(mSourceFile);
                                PGAsset.Target = fileDesc;
                                mName = IO.FileManager.GetPureName(mSourceFile);
                            }
                        }
                        // close
                        mFileDialog.CloseDialog();
                    }
                    if (eErrorType != enErrorType.None)
                    {
                        var clr = new Vector4(1, 0, 0, 1);
                        ImGuiAPI.TextColored(in clr, $"Source:{mSourceFile}");
                    }
                    else
                    {
                        var clr = new Vector4(1, 1, 1, 1);
                        ImGuiAPI.TextColored(in clr, $"Source:{mSourceFile}");
                    }
                    ImGuiAPI.Separator();

                    bool nameChanged = ImGuiAPI.InputText("##in_rname", ref mName);
                    if (nameChanged)
                    {
                        if (IO.FileManager.FileExists(mDir.Address + mName + UAnimationClip.AssetExt))
                            eErrorType = enErrorType.IsExisting;
                    }

                    ImGuiAPI.Separator();
                    if (PGAsset.Target != null)
                    {
                        PGAsset.OnDraw(false, false, false);
                    }
                    ImGuiAPI.Separator();

                    sz = new Vector2(0, 0);
                    if (eErrorType == enErrorType.None)
                    {
                        if (ImGuiAPI.Button("Create Asset", in sz))
                        {
                            if (FBXImport())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                ContentBrowser.mAssetImporter = null;
                            }
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", in sz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        ContentBrowser.mAssetImporter = null;
                    }
                    ImGuiAPI.EndPopup();
                }
            }
            private unsafe bool FBXImport()
            {
                var fileDesc = mFBXImporter.GetFileImportDesc();
                bool hasOnlyOneAnim = false;
                if(fileDesc.AnimNum == 1)
                {
                    hasOnlyOneAnim = true;
                }
                for (uint i = 0; i < fileDesc.AnimNum; ++i)
                {
                    System.Diagnostics.Debug.Assert(i == 0);
                    
                    UAnimationClip animClip = null;
                    using (var animImporter = mFBXImporter.CreateAnimImporter(i))
                    {
                         var animDesc = mFBXImporter.GetFBXAnimDesc(i);
                        string animName = null;
                        if (hasOnlyOneAnim)
                        {
                            animName = fileDesc.FileName.c_str();
                        }
                        else
                        {
                            animName = animDesc->Name.c_str();
                        }
                        var rn = RName.GetRName(mDir.Name + animName + UAnimationClip.AssetExt);
                        animImporter.Process();
                        List<AssetImportAndExport.FBX.FBXAnimElement> animElements = new List<AssetImportAndExport.FBX.FBXAnimElement>();
                        for (int animIndex = 0; animIndex < animImporter.GetAnimElementsNum(); ++animIndex)
                        {
                            AssetImportAndExport.FBX.FBXAnimElement animElement = *animImporter.GetAnimElement(animIndex);
                            animElements.Add(animElement);
                        }
                        var chunk = AssetImportAndExport.FBX.FBXAnimationImportUtility.MakeAnimationChunkFromFBX(rn, *mFBXImporter.GetFBXAnimDesc(i), animElements);
                        animClip = new UAnimationClip();
                        animClip.SampleRate = animDesc->SampleRate;
                        animClip.Duration = animDesc->Duration;
                        animClip.AnimationChunkName = chunk.RescouceName;
                        animClip.AnimationChunk = chunk;
                        animClip.SaveAssetTo(rn);
                        if (animClip != null)
                        {
                            var ameta = new UAnimationClipAMeta();
                            ameta.SetAssetName(rn);
                            ameta.AssetId = Guid.NewGuid();
                            ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(UAnimationClip));
                            ameta.Description = $"This is a {typeof(UAnimationClip).FullName}\n";
                            ameta.SaveAMeta();
                            UEngine.Instance.AssetMetaManager.RegAsset(ameta);
                        }
                    }
                }
                mFBXImporter.Dispose();
                return true;
            }

        }
    }
}

namespace AssetImportAndExport.FBX
{
    using EngineNS;
    using EngineNS.Animation.Base;
    using EngineNS.Animation.Curve;

    public static class FBXAnimationImportUtility
    {
        public unsafe static EngineNS.Animation.Asset.UAnimationChunk MakeAnimationChunkFromFBX(RName assetName, FBXAnimImportDesc animDesc, List<FBXAnimElement> animElements)
        {
            var animChunk = new EngineNS.Animation.Asset.UAnimationChunk();
            animChunk.RescouceName = assetName;
            Dictionary<uint, UAnimHierarchy> animHDic = new Dictionary<uint, UAnimHierarchy>();
            for (int i = 0; i < animElements.Count; ++i)
            {
                var animElement = animElements[i];
                UAnimHierarchy animHierarchy = new UAnimHierarchy();

                AnimatableObjectClassDesc objectClassDesc = new AnimatableObjectClassDesc();
                objectClassDesc.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf(typeof(EngineNS.Animation.SkeletonAnimation.AnimatablePose.UAnimatableBonePose));
                var t = objectClassDesc.ClassType;
                objectClassDesc.Name = animElement.Desc.Name.Text;
                uint hash = UniHash.APHash(objectClassDesc.Name.ToString());

                //Position
                {
                    var xCurve = animElement.GetPropertyCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Position_X);
                    var yCurve = animElement.GetPropertyCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Position_Y);
                    var zCurve = animElement.GetPropertyCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Position_Z);

                    if (xCurve != null || yCurve != null || zCurve != null)
                    {
                        Vector3Curve curve = new Vector3Curve();
                        curve.XCurve = CreateCurveFromFbxCurve(xCurve);
                        curve.YCurve = CreateCurveFromFbxCurve(yCurve);
                        curve.ZCurve = CreateCurveFromFbxCurve(zCurve);
                        animChunk.AnimCurvesList.Add(curve.Id, curve);

                        AnimatableObjectPropertyDesc posDesc = new AnimatableObjectPropertyDesc();
                        posDesc.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf<Vector3>();
                        posDesc.Name = "Position";
                        posDesc.CurveId = curve.Id;
                        objectClassDesc.Properties.Add(posDesc);
                    }


                }

                //Rotation

                {
                    var xCurve = animElement.GetPropertyCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Rotation_X);
                    var yCurve = animElement.GetPropertyCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Rotation_Y);
                    var zCurve = animElement.GetPropertyCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Rotation_Z);

                    if (xCurve != null || yCurve != null || zCurve != null)
                    {
                        Vector3Curve curve = new Vector3Curve();
                        curve.XCurve = CreateCurveFromFbxCurve(xCurve);
                        curve.YCurve = CreateCurveFromFbxCurve(yCurve);
                        curve.ZCurve = CreateCurveFromFbxCurve(zCurve);
                        animChunk.AnimCurvesList.Add(curve.Id, curve);

                        AnimatableObjectPropertyDesc rotDesc = new AnimatableObjectPropertyDesc();
                        rotDesc.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf<Vector3>();
                        rotDesc.Name = "Rotation";
                        rotDesc.CurveId = curve.Id;
                        objectClassDesc.Properties.Add(rotDesc);
                    }

                }

                //Sacle

                {
                    var xCurve = animElement.GetPropertyCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Scale_X);
                    var yCurve = animElement.GetPropertyCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Scale_Y);
                    var zCurve = animElement.GetPropertyCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Scale_Z);

                    if (xCurve != null || yCurve != null || zCurve != null)
                    {
                        Vector3Curve curve = new Vector3Curve();
                        curve.XCurve = CreateCurveFromFbxCurve(xCurve);
                        curve.YCurve = CreateCurveFromFbxCurve(yCurve);
                        curve.ZCurve = CreateCurveFromFbxCurve(zCurve);
                        animChunk.AnimCurvesList.Add(curve.Id, curve);

                        AnimatableObjectPropertyDesc scaleDesc = new AnimatableObjectPropertyDesc();
                        scaleDesc.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf<Vector3>();
                        scaleDesc.Name = "Scale";
                        scaleDesc.CurveId = curve.Id;
                        objectClassDesc.Properties.Add(scaleDesc);
                    }

                }


                animHierarchy.Node = objectClassDesc;
                animHDic.Add(hash, animHierarchy);
            }
            //construct hierarchy
            System.Diagnostics.Debug.Assert(animHDic.Count == animElements.Count);
            UAnimHierarchy Root = null;
            bool rootCheck = false;
            for (int i = 0; i < animElements.Count; ++i)
            {
                UAnimHierarchy parent = null;
                var hasParent = animHDic.TryGetValue(animElements[i].Desc.ParentHash, out parent);
                if (hasParent)
                {
                    UAnimHierarchy child = null;
                    var isExist = animHDic.TryGetValue(animElements[i].Desc.NameHash, out child);
                    if (isExist)
                    {
                        parent.Children.Add(child);
                        child.Parent = parent;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Assert(!rootCheck);
                    rootCheck = true;
                    UAnimHierarchy root = null;
                    var isExist = animHDic.TryGetValue(animElements[i].Desc.NameHash, out root);
                    if (isExist)
                    {
                        Root = root;
                    }
                }
            }

            animChunk.AnimatedHierarchy = Root;

            return animChunk;
        }

        private static unsafe UCurve CreateCurveFromFbxCurve(AssetImportAndExport.FBX.FBXAnimCurve* fbxAnimCurve)
        {
            if (fbxAnimCurve == null)
                return null;
            UCurve temp = new UCurve();
            var fbxCurve = *fbxAnimCurve;
            for (int keyIndex = 0; keyIndex < fbxCurve.GetKeyFrameNum(); ++keyIndex)
            {
                var fbxKey = *fbxCurve.GetKeyFrame(keyIndex);
                Keyframe keyFrame = new Keyframe();
                keyFrame.Time = fbxKey.Time;
                keyFrame.Value = fbxKey.Value;
                keyFrame.InSlope = fbxKey.InSlope;
                keyFrame.OutSlope = fbxKey.OutSlope;
                temp.AddKeyframeBack(ref keyFrame);
            }
            return temp;
        }
    }
}