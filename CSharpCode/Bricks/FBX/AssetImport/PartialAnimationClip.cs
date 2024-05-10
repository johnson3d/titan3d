using EngineNS.EGui.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Asset
{
    public partial class TtAnimationClip
    {
        public partial class ImportAttribute
        {
            AssetImportAndExport.FBX.FBXImporter mFBXImporter; //for now we only have one file to import
            public unsafe partial bool FBXCreateCreateDraw(UContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import Animation", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                var visible = true;
                var retValue = false;
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
                                mName = IO.TtFileManager.GetPureName(mSourceFile);
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
                        if (IO.TtFileManager.FileExists(mDir.Address + mName + TtAnimationClip.AssetExt))
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
                                retValue = true;
                            }
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", in sz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        retValue = true;
                    }
                    ImGuiAPI.EndPopup();
                }

                return retValue;
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
                    
                    TtAnimationClip animClip = null;
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
                        var rn = RName.GetRName(mDir.Name + animName + TtAnimationClip.AssetExt);
                        animImporter.Process();
                        List<AssetImportAndExport.FBX.FBXAnimElement> animElements = new List<AssetImportAndExport.FBX.FBXAnimElement>();
                        for (int animIndex = 0; animIndex < animImporter.GetAnimElementsNum(); ++animIndex)
                        {
                            AssetImportAndExport.FBX.FBXAnimElement animElement = *animImporter.GetAnimElement(animIndex);
                            animElements.Add(animElement);
                        }
                        var chunk = AssetImportAndExport.FBX.FBXAnimationImportUtility.MakeAnimationChunkFromFBX(rn, *mFBXImporter.GetFBXAnimDesc(i), animElements);
                        animClip = new TtAnimationClip();
                        animClip.SampleRate = animDesc->SampleRate;
                        animClip.Duration = animDesc->Duration;
                        animClip.AnimationChunkName = chunk.RescouceName;
                        animClip.AnimationChunk = chunk;
                        animClip.SaveAssetTo(rn);
                        if (animClip != null)
                        {
                            var ameta = new TtAnimationClipAMeta();
                            ameta.SetAssetName(rn);
                            ameta.AssetId = Guid.NewGuid();
                            ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(TtAnimationClip));
                            ameta.Description = $"This is a {typeof(TtAnimationClip).FullName}\n";
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
        public unsafe static EngineNS.Animation.Asset.TtAnimationChunk MakeAnimationChunkFromFBX(RName assetName, FBXAnimImportDesc animDesc, List<FBXAnimElement> animElements)
        {
            var animChunk = new EngineNS.Animation.Asset.TtAnimationChunk();
            animChunk.RescouceName = assetName;
            for (int i = 0; i < animElements.Count; ++i)
            {
                var animElement = animElements[i];
                TtAnimatedObjectDescription objectClassDesc = new TtAnimatedObjectDescription();
                objectClassDesc.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf(typeof(EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableBonePose));
                var t = objectClassDesc.ClassType;
                objectClassDesc.Name = animElement.Desc.Name.Text;
                uint hash = UniHash32.APHash(objectClassDesc.Name.ToString());

                //Position
                {
                    var xCurve = animElement.GetPropertyCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Position_X);
                    var yCurve = animElement.GetPropertyCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Position_Y);
                    var zCurve = animElement.GetPropertyCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Position_Z);

                    if (xCurve != null || yCurve != null || zCurve != null)
                    {
                        TtVector3Curve curve = new TtVector3Curve();
                        curve.XTrack = CreateCurveFromFbxCurve(xCurve);
                        curve.YTrack = CreateCurveFromFbxCurve(yCurve);
                        curve.ZTrack = CreateCurveFromFbxCurve(zCurve);
                        animChunk.AnimCurvesList.Add(curve.Id, curve);

                        TtAnimatedPropertyDescription posDesc = new TtAnimatedPropertyDescription();
                        posDesc.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf<FNullableVector3>();
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
                        TtVector3Curve curve = new TtVector3Curve();
                        curve.XTrack = CreateCurveFromFbxCurve(xCurve);
                        curve.YTrack = CreateCurveFromFbxCurve(yCurve);
                        curve.ZTrack = CreateCurveFromFbxCurve(zCurve);
                        animChunk.AnimCurvesList.Add(curve.Id, curve);

                        TtAnimatedPropertyDescription rotDesc = new TtAnimatedPropertyDescription();
                        rotDesc.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf<FNullableVector3>();
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
                        TtVector3Curve curve = new TtVector3Curve();
                        curve.XTrack = CreateCurveFromFbxCurve(xCurve);
                        curve.YTrack = CreateCurveFromFbxCurve(yCurve);
                        curve.ZTrack = CreateCurveFromFbxCurve(zCurve);
                        animChunk.AnimCurvesList.Add(curve.Id, curve);

                        TtAnimatedPropertyDescription scaleDesc = new TtAnimatedPropertyDescription();
                        scaleDesc.ClassType = EngineNS.Rtti.UTypeDesc.TypeOf<FNullableVector3>();
                        scaleDesc.Name = "Scale";
                        scaleDesc.CurveId = curve.Id;
                        objectClassDesc.Properties.Add(scaleDesc);
                    }

                }
                animChunk.AnimatedObjectDescs.Add(objectClassDesc.Name, objectClassDesc);
            }

            return animChunk;
        }

        private static unsafe TtTrack CreateCurveFromFbxCurve(AssetImportAndExport.FBX.FBXAnimCurve* fbxAnimCurve)
        {
            if (fbxAnimCurve == null)
                return null;
            TtTrack temp = new TtTrack();
            var fbxCurve = *fbxAnimCurve;
            for (int keyIndex = 0; keyIndex < fbxCurve.GetKeyFrameNum(); ++keyIndex)
            {
                var fbxKey = *fbxCurve.GetKeyFrame(keyIndex);
                FKeyframe keyFrame = new FKeyframe();
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