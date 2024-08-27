using EngineNS.Bricks.AssetImpExp;
using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Animation.Asset
{
    public class TtAnimImprotSetting
    {
        [Category("FileInfo"), ReadOnly(true)]
        public string FileName { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public int AnimationsCount { get; set; } = 0;
        [Category("FileInfo"), ReadOnly(true)]
        public string UpAxis { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public float UnitScaleFactor { get; set; } = 1;
        [Category("FileInfo"), ReadOnly(true)]
        public string Generator { get; set; } = "";
        [Category("ImportSetting")]
        public float UnitScale { get; set; } = 0.01f;
        [Category("ImportSetting")]
        public bool IgnoreScale { get; set; } = true;
        [Category("ImportSetting"), Browsable(false)]
        public TtAssetImporter AssetImporter = null;
    }
    public partial class TtAnimationClip
    {
        public partial class ImportAttribute
        {
            List<TtAnimImprotSetting> AnimImprotSettings = new ();
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
                        mFileDialog.OpenModalWithMutiSelect("ChooseFileDlgKey", "Choose File", ".FBX,.fbx", ".", int.MaxValue - 1);
                    }
                    // display
                    if (mFileDialog.DisplayDialog("ChooseFileDlgKey"))
                    {
                        // action if OK
                        if (mFileDialog.IsOk() == true)
                        {
                            var count = mFileDialog.GetSelectedCount();
                            for (int i = 0; i < count; ++i)
                            {
                                var path = mFileDialog.GetFilePathByIndex(i);
                                TtAnimImprotSetting animImprotSetting = new TtAnimImprotSetting();
                                if (!string.IsNullOrEmpty(path))
                                {
                                    TtAssetImporter assetImporter = new TtAssetImporter();
                                    var AssetDescription = assetImporter.PreImport(path);
                                    if (AssetDescription == null)
                                    {
                                        eErrorType = enErrorType.EmptyName;
                                    }
                                    else
                                    {
                                        animImprotSetting .FileName = AssetDescription.FileName;
                                        animImprotSetting .AnimationsCount = AssetDescription.AnimationsCount;
                                        animImprotSetting .UpAxis = AssetDescription.UpAxis;
                                        animImprotSetting .UnitScaleFactor = AssetDescription.UnitScaleFactor;
                                        animImprotSetting .Generator = AssetDescription.Generator;
                                        animImprotSetting.AssetImporter = assetImporter;
                                        if (i == 0)
                                        {
                                            mName = IO.TtFileManager.GetPureName(path);
                                            PGAsset.Target = animImprotSetting;
                                            mName = IO.TtFileManager.GetPureName(path);
                                        }
                                        AnimImprotSettings.Add(animImprotSetting);
                                    }
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
                        if (IO.TtFileManager.FileExists(mDir.Address + mName + TtAnimationClip.AssetExt))
                            eErrorType = enErrorType.IsExisting;
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

                    ImGuiAPI.Separator();
                    if (PGAsset.Target != null)
                    {
                        PGAsset.OnDraw(false, false, false);
                    }

                    ImGuiAPI.EndPopup();
                }

                return retValue;
            }

            private unsafe bool DoImport()
            {
               foreach(var setting in AnimImprotSettings)
                {
                    ImportAndSaveAnimation(setting);
                }
                return true;
            }

            private unsafe bool ImportAndSaveAnimation(TtAnimImprotSetting animImprotSetting)
            {
                bool hasOnlyOneAnim = false;
                if (animImprotSetting.AnimationsCount == 1)
                {
                    hasOnlyOneAnim = true;
                }
                var animations = animImprotSetting.AssetImporter.GetAnimations();
                foreach (var anim in animations)
                {
                    string animName = null;
                    if (hasOnlyOneAnim)
                    {
                        animName = animImprotSetting.FileName;
                    }
                    else
                    {
                        animName = anim.Name;
                    }
                    var rn = RName.GetRName(mDir.Name + animName + TtAnimationClip.AssetExt);
                    var importSetting = new TtAssetImportOption_Animation();
                    importSetting.Scale = animImprotSetting.UnitScale;
                    importSetting.IgnoreScale = animImprotSetting.IgnoreScale;
                    var chunk = AnimationChunkGenerater.Generate(rn, anim, animImprotSetting.AssetImporter.AiScene, importSetting);
                    var animClip = new TtAnimationClip();
                    animClip.SampleRate = (float)anim.TicksPerSecond;
                    animClip.Duration = (float)(anim.DurationInTicks / anim.TicksPerSecond);
                    animClip.AnimationChunkName = chunk.RescouceName;
                    animClip.AnimationChunk = chunk;
                    animClip.SaveAssetTo(rn);
                    EngineNS.TtEngine.Instance.AnimationModule.AnimationChunkManager.Remove(rn);
                    EngineNS.TtEngine.Instance.AnimationModule.AnimationClipManager.Remove(rn);
                    if (animClip != null)
                    {
                        var ameta = new TtAnimationClipAMeta();
                        ameta.SetAssetName(rn);
                        ameta.AssetId = Guid.NewGuid();
                        ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(TtAnimationClip));
                        ameta.Description = $"This is a {typeof(TtAnimationClip).FullName}\n";
                        ameta.SaveAMeta(animClip);
                        TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                    }
                }
                return true;
            }
        }
    }
}
