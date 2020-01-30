using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EditorCommon.Resources;
using EngineNS;

namespace UVAnimEditor
{
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.UVAnim, ResourceExts = new string[] { ".uvanim" })]
    public class UVAnimResourceInfo : EditorCommon.Resources.ResourceInfo,
                                      EditorCommon.Resources.IResourceInfoEditor,
                                      EditorCommon.Resources.IResourceInfoCreateEmpty
    {
        public override string ResourceTypeName => "UI图元";

        public override Brush ResourceTypeBrush => new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 99, 52));

        public override ImageSource ResourceIcon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/PaperSprite_64x.png", UriKind.Absolute));

        #region IResourceInfoEditor
        public string EditorTypeName => "UVAnimEditor";

        public async Task OpenEditor()
        {
            await EditorCommon.Program.OpenEditor(new ResourceEditorContext(EditorTypeName, this));
        }
        #endregion

        protected override async Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            EngineNS.CEngine.Instance.FileManager.DeleteFile(ResourceName.Address);
            var snapShotFile = ResourceName.Address + EditorCommon.Program.SnapshotExt;
            EngineNS.CEngine.Instance.FileManager.DeleteFile(snapShotFile);
            return true;
        }

        protected override Task<bool> MoveToFolderOverride(string absFolder, RName currentResourceName)
        {
            throw new NotImplementedException();
        }

        protected override Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, RName oldRName)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> RenameOverride(string absFolder, string newName)
        {
            throw new NotImplementedException();
        }

        #region IResourceInfoCreateEmpty
        public string CreateMenuPath => "UI/UI图元";

        public bool IsBaseResource => false;
        public string GetValidName(string absFolder)
        {
            return EditorCommon.Program.GetValidName(absFolder, "UVAnim", EngineNS.CEngineDesc.UVAnimExtension);
        }

        public ValidationResult ResourceNameAvailable(string absFolder, string name)
        {
            // 判断资源名称是否合法
            if(EditorCommon.Program.IsValidRName(name) == false)
            {
                return new ValidationResult(false, "名称不合法!");
            }

            return new ValidationResult(true, null);
        }

        public class ResourceCreateData : IResourceCreateData
        {
            [Browsable(false)]
            public EngineNS.RName.enRNameType RNameType { get; set; }
            [Browsable(false)]
            public string ResourceName { get; set; }
            [Browsable(false)]
            public ICustomCreateDialog HostDialog { get; set; }
            [Browsable(false)]
            public string Description { get; set; }
            [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Texture)]
            public RName TextureRName { get; set; } = EngineNS.CEngineDesc.DefaultUITextureName;
            [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.MaterialInstance)]
            public RName MaterialInstanceRName { get; set; } = EngineNS.CEngineDesc.DefaultUIMaterialInstance;
        }
        public IResourceCreateData GetResourceCreateData(string absFolder)
        {
            return new ResourceCreateData();
        }

        public async Task<ResourceInfo> CreateEmptyResource(string absFolder, string rootFolder, IResourceCreateData createData)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var result = new UVAnimResourceInfo();
            var data = createData as ResourceCreateData;
            var reName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absFolder + "/" + data.ResourceName, rootFolder);
            reName += EngineNS.CEngineDesc.UVAnimExtension;
            result.ResourceName = EngineNS.RName.GetRName(reName, data.RNameType);

            var uvAnim = EngineNS.CEngine.Instance.UVAnimManager.CreateUVAnim(data.TextureRName, data.MaterialInstanceRName);
            // 默认有一帧
            uvAnim.Frames.Add(new EngineNS.UISystem.UVFrame()
            {
                ParentAnim = uvAnim,
            });
            uvAnim.Save2Xnd(result.ResourceName);

            return result;
        }

        #endregion

        protected override async Task<ResourceInfo> CreateResourceInfoFromResourceOverride(RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new UVAnimResourceInfo();
            retValue.ResourceName = ResourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.UVAnim;
            return retValue;
        }

        protected override async Task<bool> InitializeContextMenuOverride(ContextMenu contextMenu)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var textSeparatorStyle = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextMenuSeparatorStyle")) as System.Windows.Style;
            contextMenu.Items.Add(new ResourceLibrary.Controls.Menu.TextSeparator()
            {
                Text = "Common",
                Style = textSeparatorStyle,
            });
            var menuItemStyle = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as System.Windows.Style;
            var menuItem = new MenuItem()
            {
                Name = "UVAnimResInfo_Delete",
                Header = "删除",
                Style = menuItemStyle,
            };
            menuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                await DeleteResource();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_delete_16px.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            return true;
        }

        bool mIsGenerationSnapshot = false;
        public override async Task<ImageSource[]> GetSnapshotImage(bool forceCreate)
        {
            if (mIsGenerationSnapshot)
                return null;
            var snapShotFile = ResourceName.Address + ".snap";
            if(!forceCreate)
            {
                var imgSource = await EditorCommon.ImageInit.GetImage(snapShotFile);
                if (imgSource != null)
                    return imgSource;
            }

            mIsGenerationSnapshot = true;
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var uvAnim = await EngineNS.CEngine.Instance.UVAnimManager.GetUVAnimAsync(rc, ResourceName);
            if (uvAnim == null)
            {
                mIsGenerationSnapshot = false;
                return null;
            }
            var snapCreator = new EditorCommon.SnapshotProcess.SnapshotCreator();
            var scOK = await snapCreator.InitEnviroment();
            if (scOK == false)
            {
                mIsGenerationSnapshot = false;
                return null;
            }
            var img2D = await EngineNS.Graphics.Mesh.CGfxImage2D.CreateImage2D(rc, uvAnim.MaterialInstanceRName, 0, 0, 0, 1, 1);
            if(img2D == null)
                return null;
            var texture = EngineNS.CEngine.Instance.TextureManager.GetShaderRView(rc, uvAnim.TextureRName);
            img2D.SetTexture("texture", texture);
            var desc = texture.TxPicDesc;
            if (desc.Width == 0 || desc.Height == 0)
                return null;
            float drawWidth, drawHeight;
            // todo: 这里要根据Frame的UV计算出真正的大小
            if(desc.Width > desc.Height)
            {
                var delta = snapCreator.mWidth / desc.Width;
                drawWidth = snapCreator.mWidth;
                drawHeight = desc.Height * delta;
            }
            else
            {
                var delta = snapCreator.mHeight / desc.Height;
                drawHeight = snapCreator.mHeight;
                drawWidth = desc.Width * delta;
            }
            img2D.RenderMatrix = EngineNS.Matrix.Scaling(drawWidth, drawHeight, 1.0f);
            snapCreator.TickLogicEvent = (sc, arg) =>
            {
                sc.World?.Tick();
                sc.World?.CheckVisible(rc.ImmCommandList, sc.Camera);
                sc.mRP_Snapshot.OnAfterTickLogicArgument = arg;
                sc.mRP_Snapshot.TickLogic(null, rc);

                var designRect = new EngineNS.RectangleF(0, 0, drawWidth, drawHeight);
                var tempClipRect = designRect;

                uvAnim.CheckAndAutoReferenceFromTemplateUVAnim();
                bool frameChanged;
                var frame = uvAnim.GetUVFrame(EngineNS.Support.Time.GetTickCount(), out frameChanged);

                using (var posData = EngineNS.Support.NativeListProxy<EngineNS.Vector3>.CreateNativeList())
                using (var uvData = EngineNS.Support.NativeListProxy<EngineNS.Vector2>.CreateNativeList())
                {
                    frame.UpdateVertexes(posData, ref designRect, ref tempClipRect);
                    frame.UpdateUVs(uvData, ref designRect, ref tempClipRect);
                    img2D.RenderMatrix = EngineNS.Matrix.Scaling(tempClipRect.Width, tempClipRect.Height, 1) * EngineNS.Matrix.Translate(tempClipRect.Left, tempClipRect.Top, 0.0f);
                    img2D.SetUV(uvData, rc.ImmCommandList);
                    img2D.SetVertexBuffer(posData, rc.ImmCommandList);
                }   
            };
            snapCreator.mRP_Snapshot.OnDrawUI += (cmd, view) =>
            {
                var mtlMesh = img2D.Mesh.MtlMeshArray[0];
                var pass = img2D.GetPass();
                pass.ViewPort = view.Viewport;
                if(pass.RenderPipeline == null)
                {
                    var rplDesc = new EngineNS.CRenderPipelineDesc();
                    pass.RenderPipeline = rc.CreateRenderPipeline(rplDesc);
                }
                pass.RenderPipeline.RasterizerState = mtlMesh.MtlInst.CustomRasterizerState;
                pass.RenderPipeline.DepthStencilState = mtlMesh.MtlInst.CustomDepthStencilState;
                pass.RenderPipeline.BlendState = mtlMesh.MtlInst.CustomBlendState;

                pass.BindCBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_View, view.ScreenViewCB);
                pass.ShadingEnv.BindResources(img2D.Mesh, pass);
                cmd.PushPass(pass);
            };
            await snapCreator.SaveToFile(snapShotFile, 1000, 4);
            mIsGenerationSnapshot = false;
            return await EditorCommon.ImageInit.GetImage(snapShotFile);
        }

        public override async Task Save(bool withSnapshot = false)
        {
            var uvAnim = await EngineNS.CEngine.Instance.UVAnimManager.GetUVAnimAsync(EngineNS.CEngine.Instance.RenderContext, ResourceName);
            RefreshReferenceRNames(uvAnim);
            // 刷新资源引用表
            await EngineNS.CEngine.Instance.GameEditorInstance.RefreshResourceInfoReferenceDictionary(this);

            await base.Save(withSnapshot);
        }
        public void RefreshReferenceRNames(EngineNS.UISystem.UVAnim uvAnim)
        {
            // 资源引用
            ReferenceRNameList.Clear();
            ReferenceRNameList.Add(uvAnim.TextureRName);
            ReferenceRNameList.Add(uvAnim.MaterialInstanceRName);
        }
        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            var uvAnim = new EngineNS.UISystem.UVAnim();
            if (false == await uvAnim.LoadUVAnimAsync(EngineNS.CEngine.Instance.RenderContext, data.RNameMapper.Name))
                return false;
            data.RNameMapper.ResObject = uvAnim;
            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var uvAnim = data.ResObject as EngineNS.UISystem.UVAnim;
            var newFile = data.GetTargetAbsFileName();
            uvAnim.Save2Xnd(newFile);
            RefreshReferenceRNames(uvAnim);

            return true;
        }

    }
}
