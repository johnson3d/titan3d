using EngineNS.EGui.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Asset
{
    public partial class UAnimationClip
    {
        private unsafe static UAnimationClip MakeAssetFromFBX(RName assetName,AssetImportAndExport.FBX.FBXAnimImportDesc animDesc, List<AssetImportAndExport.FBX.FBXAnimElement> animElements)
        {
            UAnimationClip clip = new UAnimationClip();
            clip.AssetName = assetName;
            UAnimationChunk animChunk = new UAnimationChunk();
            Dictionary<uint, Base.AnimHierarchy> animHDic = new Dictionary<uint, Base.AnimHierarchy>();
            for (int i = 0; i< animElements.Count; ++i)
            {
                var animElement = animElements[i];
                Base.AnimHierarchy animHierarchy = new Base.AnimHierarchy();
                
                Base.AnimatableObjectClassDesc objectClassDesc = new Base.AnimatableObjectClassDesc();
                objectClassDesc.ClassType = Rtti.UTypeDesc.TypeOf(typeof(Pose.UBonePose));
                var t = objectClassDesc.ClassType;
                objectClassDesc.Name = animElement.Desc.Name;
                uint hash = UniHash.APHash(objectClassDesc.Name.ToString());

                Base.AnimatableObjectPropertyDesc posDesc = new Base.AnimatableObjectPropertyDesc();
                posDesc.ClassType = Rtti.UTypeDesc.TypeOf<Vector3>();
                posDesc.Name = VNameString.FromString("Position");
                {
                    var xCurve = animElement.GetAnimCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Position_X);
                    var yCurve = animElement.GetAnimCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Position_Y);
                    var zCurve = animElement.GetAnimCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Position_Z);

                    Curve.Vector3Curve curve = new Curve.Vector3Curve();
                    curve.XCurve = CreateCurveFromFbxCurve(xCurve);
                    curve.YCurve = CreateCurveFromFbxCurve(yCurve);
                    curve.ZCurve = CreateCurveFromFbxCurve(zCurve);
                    if (xCurve != null || yCurve != null || zCurve != null)
                    {
                        animChunk.AnimCurvesList.Add(curve.Id, curve);
                        objectClassDesc.Properties.Add(posDesc);
                    }

                    
                }
                Base.AnimatableObjectPropertyDesc rotDesc = new Base.AnimatableObjectPropertyDesc();
                rotDesc.ClassType = Rtti.UTypeDesc.TypeOf<Vector3>();
                rotDesc.Name = VNameString.FromString("Rotation");
                {
                    var xCurve = animElement.GetAnimCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Rotation_X);
                    var yCurve = animElement.GetAnimCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Rotation_Y);
                    var zCurve = animElement.GetAnimCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Rotation_Z);

                    Curve.Vector3Curve curve = new Curve.Vector3Curve();
                    curve.XCurve = CreateCurveFromFbxCurve(xCurve);
                    curve.YCurve = CreateCurveFromFbxCurve(yCurve);
                    curve.ZCurve = CreateCurveFromFbxCurve(zCurve);
                    if (xCurve != null || yCurve != null || zCurve != null)
                    {
                        animChunk.AnimCurvesList.Add(curve.Id, curve);
                        objectClassDesc.Properties.Add(rotDesc);
                    }
                    
                }
                Base.AnimatableObjectPropertyDesc scaleDesc = new Base.AnimatableObjectPropertyDesc();
                scaleDesc.ClassType = Rtti.UTypeDesc.TypeOf<Vector3>();
                scaleDesc.Name = VNameString.FromString("Scale");
                {
                    var xCurve = animElement.GetAnimCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Scale_X);
                    var yCurve = animElement.GetAnimCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Scale_Y);
                    var zCurve = animElement.GetAnimCurve(AssetImportAndExport.FBX.FBXCurvePropertyType.Scale_Z);

                    Curve.Vector3Curve curve = new Curve.Vector3Curve();
                    curve.XCurve = CreateCurveFromFbxCurve(xCurve);
                    curve.YCurve = CreateCurveFromFbxCurve(yCurve);
                    curve.ZCurve = CreateCurveFromFbxCurve(zCurve);
                    if (xCurve != null || yCurve != null || zCurve != null)
                    {
                        animChunk.AnimCurvesList.Add(curve.Id, curve);
                        objectClassDesc.Properties.Add(scaleDesc);
                    }
                    
                }


                animHierarchy.Value = objectClassDesc;
                animHDic.Add(hash, animHierarchy);
            }
            //construct hierarchy
            System.Diagnostics.Debug.Assert(animHDic.Count == animElements.Count);
            Base.AnimHierarchy Root = null;
            bool rootCheck= false;
            for(int i = 0; i< animElements.Count; ++i)
            {
                Base.AnimHierarchy parent = null;
                var hasParent = animHDic.TryGetValue(animElements[i].Desc.ParentHash, out parent);
                if(hasParent)
                {
                    Base.AnimHierarchy child = null;
                    var isExist = animHDic.TryGetValue(animElements[i].Desc.NameHash, out child);
                    if(isExist)
                    {
                        parent.Children.Add(child);
                        child.Parent = parent;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Assert(!rootCheck);
                    rootCheck = true;
                    Base.AnimHierarchy root = null;
                    var isExist = animHDic.TryGetValue(animElements[i].Desc.NameHash, out root);
                    if (isExist)
                    {
                        Root = root;
                    }
                }
            }

            animChunk.AnimatedHierarchy = Root;
            
            return clip;
        }

        private static unsafe Curve.Curve CreateCurveFromFbxCurve(AssetImportAndExport.FBX.FBXAnimCurve* fbxAnimCurve)
        {
            if (fbxAnimCurve == null)
                return null;
            Curve.Curve temp = new Curve.Curve();
            var fbxCurve = *fbxAnimCurve;
            for (int keyIndex = 0; keyIndex < fbxCurve.GetKeyFrameNum(); ++keyIndex)
            {
                var fbxKey = *fbxCurve.GetKeyFrame(keyIndex);
                Curve.Keyframe keyFrame = new Curve.Keyframe();
                keyFrame.Time = fbxKey.Time;
                keyFrame.Value = fbxKey.Value;
                keyFrame.InSlope = fbxKey.InSlope;
                keyFrame.OutSlope = fbxKey.OutSlope;
                temp.AddKeyframeBack(ref keyFrame);
            }
            return temp;
        }
        public partial class ImportAttribute
        {
            AssetImportAndExport.FBX.FBXImporter mFBXImporter; //for now we only have one file to import
            public unsafe partial void FBXCreateCreateDraw(ContentBrowser ContentBrowser)
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

                    var buffer = BigStackBuffer.CreateInstance(256);
                    buffer.SetText(mName);
                    ImGuiAPI.InputText("##in_rname", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                    var name = buffer.AsText();
                    if (mName != name)
                    {
                        mName = name;
                        if (IO.FileManager.FileExists(mDir.Address + mName + RHI.CShaderResourceView.AssetExt))
                            eErrorType = enErrorType.IsExisting;
                    }
                    buffer.DestroyMe();

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
                for (uint i = 0; i < fileDesc.AnimNum; ++i)
                {
                    System.Diagnostics.Debug.Assert(i == 0);
                    
                    UAnimationClip animClip = null;
                    using (var animImporter = mFBXImporter.CreateAnimImporter(i))
                    {
                         var animDesc = mFBXImporter.GetFBXAnimDesc(i);
                        var animName = animDesc->NativeSuper->Name.Text;
                        var rn = RName.GetRName(mDir.Name + animName + UAnimationClip.AssetExt);
                        animImporter.Process();
                        List<AssetImportAndExport.FBX.FBXAnimElement> animElements = new List<AssetImportAndExport.FBX.FBXAnimElement>();
                        for (int animIndex = 0; animIndex < animImporter.GetAnimElementsNum(); ++animIndex)
                        {
                            AssetImportAndExport.FBX.FBXAnimElement animElement = *animImporter.GetAnimElement(animIndex);
                            animElements.Add(animElement);
                        }
                        animClip = UAnimationClip.MakeAssetFromFBX(rn, *mFBXImporter.GetFBXAnimDesc(i), animElements);
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
