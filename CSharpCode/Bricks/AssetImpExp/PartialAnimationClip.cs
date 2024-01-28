using EngineNS.Bricks.AssetImpExp;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Asset
{
    public partial class UAnimationClip
    {
        public partial class ImportAttribute
        {
            AssetImporter AssetImporter = new AssetImporter();
            AssetDescription AssetDescription = null;
            public unsafe partial bool AssimpCreateCreateDraw(EGui.Controls.UContentBrowser ContentBrowser)
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
                                AssetDescription = AssetImporter.PreImport(mSourceFile);
                                if(AssetDescription == null)
                                {
                                    eErrorType = enErrorType.EmptyName;
                                }
                                else
                                {
                                    PGAsset.Target = AssetDescription;
                                    mName = IO.TtFileManager.GetPureName(mSourceFile);
                                }
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
                        if (IO.TtFileManager.FileExists(mDir.Address + mName + UAnimationClip.AssetExt))
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
                            if (DoImport())
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

            private unsafe bool DoImport()
            {
                bool hasOnlyOneAnim = false;
                if (AssetDescription.AnimationsCount == 1)
                {
                    hasOnlyOneAnim = true;
                }
                var animations = AssetImporter.GetAnimations();
                foreach(var anim in animations)
                {
                    string animName = null;
                    if (hasOnlyOneAnim)
                    {
                        animName = AssetDescription.FileName;
                    }
                    else
                    {
                        animName = anim.Name;
                    }
                    var rn = RName.GetRName(mDir.Name + animName + UAnimationClip.AssetExt);
                    
                    var chunk = AnimationChunkGenerater.Generate(rn, anim, AssetImporter.AiScene);
                    var animClip = new UAnimationClip();
                    animClip.SampleRate = (float)anim.TicksPerSecond;
                    animClip.Duration = (float)(anim.DurationInTicks / anim.TicksPerSecond);
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
                return true;
            }
        }
    }
}
