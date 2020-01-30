using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EditorCommon.Resources;
using EngineNS;

namespace CoreEditor.WorldEditor
{
    [EngineNS.Rtti.MetaClass]
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Scene, ResourceExts = new string[] { ".map" })]
    public class SceneResourceInfo : EditorCommon.Resources.ResourceInfo,
                                     EditorCommon.Resources.IResourceInfoEditor,
                                     EditorCommon.Resources.IResourceInfoCreateEmpty,
                                     EditorCommon.Resources.IResourceInfoCustomCreateDialog
    {
        string mDescription = "无";
        [EngineNS.Rtti.MetaData]
        public string Description
        {
            get => mDescription;
            set
            {
                mDescription = value;
                OnPropertyChanged("Description");
            }
        }

        public class SceneResourceInfoCreateData : IResourceCreateData
        {
            [Browsable(false)]
            public string ResourceName { get; set; }
            [Browsable(false)]
            public ICustomCreateDialog HostDialog { get; set; }
            [Browsable(false)]
            public EngineNS.RName.enRNameType RNameType { get; set; }
            public RName SrcSceneName;
            [Browsable(false)]
            public string Description { get; set; }
        }

        public override string ResourceTypeName => EngineNS.Editor.Editor_RNameTypeAttribute.Scene;

        public override Brush ResourceTypeBrush => new SolidColorBrush(System.Windows.Media.Color.FromRgb(121, 121, 121));

        public override ImageSource ResourceIcon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/World_64x.png", UriKind.Absolute));

        public string EditorTypeName => "SceneEditor";

        public string CreateMenuPath => "";

        public bool IsBaseResource => true;

        public override async Task<ImageSource[]> GetSnapshotImage(bool forceCreate)
        {
            var file = ResourceName.Address + EditorCommon.Program.SnapshotExt;
            if (EngineNS.CEngine.Instance.FileManager.FileExists(file))
            {
                var imgs = await EditorCommon.ImageInit.GetImage(file);
                Snapshot = imgs[0];
                return imgs;
            }
            return null;
        }

        public async Task<ResourceInfo> CreateEmptyResource(string absFolder, string rootFolder, IResourceCreateData createData)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var result = new SceneResourceInfo();
            var data = createData as SceneResourceInfoCreateData;
            var reName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absFolder + "/" + data.ResourceName, rootFolder);
            reName += EngineNS.CEngineDesc.SceneExtension;
            result.ResourceName = EngineNS.RName.GetRName(reName, data.RNameType);

            // 复制模板场景
            if (data.SrcSceneName != null)
            {
                EngineNS.CEngine.Instance.FileManager.CopyDirectory(data.SrcSceneName.Address, result.ResourceName.Address);
                EngineNS.CEngine.Instance.FileManager.CopyFile(data.SrcSceneName.Address + EditorCommon.Program.ResourceInfoExt, result.ResourceName.Address + EditorCommon.Program.ResourceInfoExt, true);
                EngineNS.CEngine.Instance.FileManager.CopyFile(data.SrcSceneName.Address + EditorCommon.Program.SnapshotExt, result.ResourceName.Address + EditorCommon.Program.SnapshotExt, true);
            }

            return result;
        }

        public IResourceCreateData GetResourceCreateData(string absFolder)
        {
            return new SceneResourceInfoCreateData();
        }

        public string GetValidName(string absFolder)
        {
            return EditorCommon.Program.GetValidName(absFolder, "Scene", EngineNS.CEngineDesc.MacrossExtension);
        }

        public static async Task OpenEditorByRName(RName name)
        {
            if (name == null)
                return;
            var rinfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(name.Address) as EditorCommon.Resources.IResourceInfoEditor;
            await rinfo.OpenEditor();
        }

        public async Task OpenEditor()
        {
            var editor = EngineNS.CEngine.Instance.GameEditorInstance as CoreEditor.CEditorInstance;
            if (editor == null)
                return;
            var worldEditor = editor.WorldEditorControl;
            if (worldEditor == null)
                return;

            if (worldEditor.IsWorldOpening)
            {
                MessageBox.Show("上一个场景正在打开中，无法打开当前场景");
                return;
            }

            worldEditor.IsWorldOpening = true;
            // 打开场景
            editor.CleanWorld();

            // scene
            EngineNS.GamePlay.SceneGraph.GSceneGraph scene = null;
            var xnd = await EngineNS.IO.XndHolder.LoadXND(ResourceName.Address + "/scene.map");
            if (xnd == null)
            {
                worldEditor.IsWorldOpening = false;
                throw new InvalidOperationException($"找不到地图{ResourceName.PureName()}");
            }
            else
            {
                var type = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(xnd.Node.GetName());
                if (type == null)
                    scene = null;
                scene = EngineNS.GamePlay.SceneGraph.GSceneGraph.NewSceneGraphWithoutInit(editor.World, type, new EngineNS.GamePlay.SceneGraph.GSceneGraphDesc());//默认先开启物理
                scene.Name = ResourceName.Name;
                worldEditor.ShowLoadingProgress(scene);
                if (false == await scene.LoadXnd(EngineNS.CEngine.Instance.RenderContext, xnd.Node, ResourceName))
                    scene = null;
                if (scene != null)
                {
                    scene.SceneFilename = ResourceName;
                    scene.RemovePhysicsTick();
                    if (scene.SunActor != null)
                    {
                        var sunComp = scene.SunActor.GetComponent<EngineNS.GamePlay.Component.GDirLightComponent>();
                        if (sunComp != null)
                        {
                            var erp = editor.RenderPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile;
                            sunComp.View = erp.BaseSceneView;
                        }
                    }

                    // 所有对象加hitproxy
                    foreach (var act in scene.Actors)
                    {
                        EngineNS.GamePlay.Actor.GActor actor = act.Value;
                        EngineNS.CEngine.Instance.HitProxyManager.MapActor(actor);
                    }

                    // 摄像机位置
                    if (editor.World.SceneNumber == 0)
                    {
                        var att = xnd.Node.FindAttrib("ED_info");
                        if (att != null)
                        {
                            att.BeginRead();
                            EngineNS.Vector3 pos, lookAt, up;
                            att.Read(out pos);
                            att.Read(out lookAt);
                            att.Read(out up);
                            att.EndRead();

                            editor.GetMainViewCamera().LookAtLH(pos, lookAt, up);
                        }
                    }

                    xnd.Node.TryReleaseHolder();
                }
            }

            if (scene != null)
            {
                scene.Name = ResourceName.PureName();
                editor.World.AddScene(ResourceName, scene);
            }

          
            worldEditor.CurrentResourceInfo = this;
            worldEditor.IsWorldOpening = false;

            editor._OnWorldLoaded();
            System.GC.Collect();
            System.GC.WaitForFullGCComplete();
            System.GC.Collect();
            System.GC.WaitForFullGCComplete();
            System.GC.Collect();
            System.GC.WaitForFullGCComplete();
            System.GC.WaitForPendingFinalizers();

            // 刷新编辑器显示
            worldEditor.RefreshWhenWorldLoaded();
       
        }

        public ValidationResult ResourceNameAvailable(string absFolder, string name)
        {
            // 判断资源名称是否合法
            if (EditorCommon.Program.IsValidRName(name) == false)
            {
                return new ValidationResult(false, "名称不合法!");
            }

            foreach(var content in EngineNS.CEngine.Instance.FileManager.AllContentsWithoutEditor)
            {
                var dirs = EngineNS.CEngine.Instance.FileManager.GetDirectories(content, name + EngineNS.CEngineDesc.MacrossExtension, SearchOption.AllDirectories);
                if (dirs.Count > 0)
                {
                    return new ValidationResult(false, "已包含同名的Macross文件!");
                }
            }

            return new ValidationResult(true, null);
        }

        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            // 判断当前地图是否正在编辑中
            var editor = EngineNS.CEngine.Instance.GameEditorInstance as CoreEditor.CEditorInstance;
            using(var i = editor.World.GetSceneEnumerator())
            {
                while(i.MoveNext())
                {
                    if (i.Current.Value.SceneFilename == ResourceName)
                    {
                        EditorCommon.MessageBox.Show("当前地图正在编辑中，不能删除!");
                        return false;
                    }
                }
            }

            EngineNS.CEngine.Instance.FileManager.DeleteDirectory(ResourceName.Address, true);
            return true;
        }

        protected override async Task<bool> MoveToFolderOverride(string absFolder, RName currentResourceName)
        {
            throw new NotImplementedException();
        }

        protected override async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResourceOverride(RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new SceneResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Scene;

            return retValue;
        }

        protected override Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, EngineNS.RName oldRName)
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> RenameOverride(string absFolder, string newName)
        {
            var tagDir = absFolder + newName + EngineNS.CEngineDesc.SceneExtension;
            var editor = EngineNS.CEngine.Instance.GameEditorInstance as CoreEditor.CEditorInstance;
            using(var i = editor.World.GetSceneEnumerator())
            {
                while(i.MoveNext())
                {
                    if (i.Current.Value.SceneFilename == ResourceName)
                    {
                        EditorCommon.MessageBox.Show("当前地图正在编辑中，不能重命名!");
                        return false;
                    }
                }
            }
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            EngineNS.CEngine.Instance.FileManager.MoveDirectory(ResourceName.Address, tagDir);


            return true;
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
            // <MenuItem Header="Add Feature or Content Pack..." menu:MenuAssist.Icon="/ResourceLibrary;component/Icons/Icons/icon_file_saveall_40x.png" Style="{DynamicResource {ComponentResourceKey ResourceId=MenuItem_Default, TypeInTargetAssembly={x:Type res:CustomResources}}}"/>
            var menuItemStyle = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as System.Windows.Style;
            var menuItem = new MenuItem()
            {
                Header = "编辑...",
                Style = menuItemStyle,
                Name = "MenuItem_SceneResourceInfo_Edit"
            };
            menuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                var noUsed = OpenEditor();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Edit/icon_Edit_16x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem()
            {
                Header = "重命名",
                Style = menuItemStyle,
                Name = "MenuItem_SceneResourceInfo_Rename"
            };
            menuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                await Rename();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Icon_Asset_Rename_16x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem()
            {
                Header = "删除",
                Style = menuItemStyle,
                Name = "MenuItem_SceneResourceInfo_Delete"
            };
            menuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                await DeleteResource();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_delete_16px.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);

            return true;
        }

        public ICustomCreateDialog GetCustomCreateDialogWindow()
        {
            var retVal = new CreateScene();
            return retVal;
        }

        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            var xnd = await EngineNS.IO.XndHolder.LoadXND(data.RNameMapper.Name.Address + "/scene.map");
            if(xnd == null)
            {
                throw new InvalidOperationException($"找不到地图{ResourceName.PureName()}");
            }
            else
            {
                var type = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(xnd.Node.GetName());
                var scene = EngineNS.GamePlay.SceneGraph.GSceneGraph.NewSceneGraphWithoutInit(null, type, new EngineNS.GamePlay.SceneGraph.GSceneGraphDesc());
                data.RNameMapper.ResObject = await scene.LoadXnd(EngineNS.CEngine.Instance.RenderContext, xnd.Node, data.RNameMapper.Name);
            }

            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            var graph = data.ResObject as EngineNS.GamePlay.SceneGraph.GSceneGraph;

            var xnd = EngineNS.IO.XndHolder.NewXNDHolder();
            await graph.Save2Xnd(xnd.Node);

            var tagAbs = data.GetTargetAbsFileName();
            var srcAbs = data.GetSourceAbsFileName();
            if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(tagAbs))
                EngineNS.CEngine.Instance.FileManager.CreateDirectory(tagAbs);
            EngineNS.IO.XndHolder.SaveXND(tagAbs + "/scene.map", xnd);

            // navmesh.dat
            EngineNS.CEngine.Instance.FileManager.CopyFile(srcAbs + "/navmesh.dat", tagAbs + "/navmesh.dat", true);

            return true;
        }

    }
}
