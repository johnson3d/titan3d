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
using EngineNS.Bricks.Particle;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;

namespace ParticleEditor
{
    [EngineNS.Rtti.MetaClass]
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Particle, ResourceExts = new string[] { ".macross" })]
    public class ParticleResourceInfo : Macross.ResourceInfos.MacrossResourceInfo,
                                       EditorCommon.Resources.IResourceInfoEditor,
                                       EditorCommon.Resources.IResourceInfoCreateEmpty,
                                       EditorCommon.Resources.IResourceInfoCustomCreateDialog,
                                        EditorCommon.Resources.IResourceInfoDragToViewport,
                                        EditorCommon.Resources.IResourceInfoCreateActor,
                                        EditorCommon.Resources.IResourceInfoCreateComponent
    {
        #region IResourceCreateData
        public class ParticleCreateData : Macross.ResourceInfos.ResourceCreateData
        {
            [Browsable(false)]
            public override ICustomCreateDialog HostDialog { get; set; }
            [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.Particle)]
            public string ParticleName
            {
                get;
                set;
            } = "";

            [Browsable(false)]
            public string TemplateName
            {
                get;
                set;
            }

            public RName.enRNameType RNameType { get; set; }

    }

        #endregion

        #region Interface

        //public static readonly string MacrossLinkExtension = ".link";
        public override string ResourceTypeName => EngineNS.Editor.Editor_RNameTypeAttribute.Particle;

        public override Brush ResourceTypeBrush => new SolidColorBrush(System.Windows.Media.Color.FromRgb(103, 75, 171));

        public override ImageSource ResourceIcon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/pfxicon_64x.png", UriKind.Absolute));

        public override string EditorTypeName => "ParticleEditor";

        public override string CreateMenuPath => "Particle/Particle";

        public override bool IsBaseResource => true;

     
        [EngineNS.Rtti.MetaData]
        public string MacrossResourceInfoFile
        {
            get;
            set;
        }

       
        [EngineNS.Rtti.MetaData]
        public string DataType
        {
            get;
            set;
        } = "";

        [EngineNS.Rtti.MetaData]
        public bool NeedRefresh
        {
            get;
            set;
        } = false;


        public ParticleResourceInfo()
        {
            mBaseType = typeof(EngineNS.Bricks.Particle.McParticleEffector);
        }

        public override async System.Threading.Tasks.Task OpenEditor()
        {
            await EditorCommon.Program.OpenEditor(new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this));
        }

        public override async Task<ResourceInfo> CreateEmptyResource(string absFolder, string rootFolder, IResourceCreateData createData)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var result = new ParticleResourceInfo();

            var data = createData as ParticleCreateData;
            //result.CenterDataTypeName = data.CenterDataTypeName;
            var reName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absFolder + "/" + data.ResourceName, rootFolder);
            reName += EngineNS.CEngineDesc.MacrossExtension;
            result.ResourceName = EngineNS.RName.GetRName(reName, data.RNameType);
            result.BaseTypeIsMacross = data.IsMacrossType;
            if (result.BaseTypeIsMacross)
            {
                result.BaseTypeSaveName = data.ClassType.FullName;
                var baseResInfo = await GetBaseMacrossResourceInfo(this);
                if (baseResInfo != null)
                    ReferenceRNameList.Add(baseResInfo.ResourceName);
            }
            else
                result.BaseTypeSaveName = EngineNS.Rtti.RttiHelper.GetTypeSaveString(data.ClassType);
            result.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Macross;


            //拷贝模板
            if (string.IsNullOrEmpty(data.TemplateName) == false)
            {
                string newfolder = result.ResourceName.Address;

                string tempfolder = RName.GetRName(data.TemplateName).Address;
                EngineNS.CEngine.Instance.FileManager.CreateDirectory(newfolder);
                EngineNS.CEngine.Instance.FileManager.CopyDirectory(tempfolder, newfolder);
                //EngineNS.CEngine.Instance.FileManager.DeleteFile(newfolder + "/particle1_Client.cs");
                //string newcsname = newfolder + "/" + data.ResourceName + "_Client.cs";
                //if (EngineNS.CEngine.Instance.FileManager.FileExists(newcsname))
                //{
                //    EngineNS.CEngine.Instance.FileManager.DeleteFile(newcsname);
                //}

                var csfiles = EngineNS.CEngine.Instance.FileManager.GetFiles(newfolder, "*.cs", System.IO.SearchOption.AllDirectories);
                if (csfiles != null && csfiles.Count != 0)
                {
                    for (int i = 0; i < csfiles.Count; i++)
                    {
                        if (EngineNS.CEngine.Instance.FileManager.FileExists(csfiles[i]))
                        {
                            EngineNS.CEngine.Instance.FileManager.DeleteFile(csfiles[i]);
                        }
                    }
                }
            }
           

            // 创建时走一遍编译，保证当前Macross能够取到this类型
            var csType = ECSType.Client;
            var codeGenerator = new CodeGenerator();
            var ctrl = new ParticleMacrossLinkControl();
            ctrl.CurrentResourceInfo = result;
            ctrl.CSType = csType;
            var codeStr = await codeGenerator.GenerateCode(result, ctrl);
            if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(result.ResourceName.Address))
                EngineNS.CEngine.Instance.FileManager.CreateDirectory(result.ResourceName.Address);
            var codeFile = $"{result.ResourceName.Address}/{result.ResourceName.PureName()}_{csType.ToString()}.cs";
            using (var fs = new System.IO.FileStream(codeFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
            {
                fs.Write(System.Text.Encoding.Default.GetBytes(codeStr), 0, Encoding.Default.GetByteCount(codeStr));
            }
            await codeGenerator.GenerateAndSaveMacrossCollector(csType);
            var files = codeGenerator.CollectionMacrossProjectFiles(csType);
            codeGenerator.GenerateMacrossProject(files.ToArray(), csType);
            EditorCommon.Program.BuildGameDll(true);
            return result;

        }
        #endregion


        //自定义创建面板
        public override ICustomCreateDialog GetCustomCreateDialogWindow()
        {
            var retVal = new CreateParticle();
            retVal.HostResourceInfo = this;
            return retVal;
        }

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

        #region Viewport
        EditorCommon.ViewPort.PreviewActorContainer mPreviewActor = null;
        public async System.Threading.Tasks.Task OnDragEnterViewport(EditorCommon.ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e)
        {
            if (mPreviewActor != null)
            {
                await mPreviewActor.AwaitLoad();
            }
            else
            {
                mPreviewActor = new EditorCommon.ViewPort.PreviewActorContainer();
                GParticleComponent component = new GParticleComponent();
                var param = new EngineNS.Editor.PlantableItemCreateActorParam()
                {
                    Location = new EngineNS.Vector3(0, 0, 0),
                };
                mPreviewActor.mPreviewActor = await component.CreateActor(param, this.ResourceName);
                var viewPortPos = viewport.PointFromScreen(new System.Windows.Point(e.X, e.Y));
                var pos = viewport.GetPickRayLineCheckPosition((float)viewPortPos.X, (float)viewPortPos.Y);
                mPreviewActor.mPreviewActor.Placement.Location = pos;
                mPreviewActor.ReleaseWaitContext();
            }
            mPreviewActor.mPreviewActor.Tag = new EditorCommon.Controls.Outliner.InvisibleInOutliner();
            viewport.AddActor(mPreviewActor.mPreviewActor);
        }
        public async System.Threading.Tasks.Task OnDragLeaveViewport(EditorCommon.ViewPort.ViewPortControl viewport, EventArgs e)
        {
            if (mPreviewActor != null)
            {
                await mPreviewActor.AwaitLoad();
            }

            viewport.World.RemoveActor(mPreviewActor.mPreviewActor.ActorId);
            viewport.World.DefaultScene.RemoveActor(mPreviewActor.mPreviewActor.ActorId);
        }
        public async System.Threading.Tasks.Task OnDragOverViewport(EditorCommon.ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e)
        {
            if (mPreviewActor != null)
            {
                await mPreviewActor.AwaitLoad();
            }

            var viewPortPos = viewport.PointFromScreen(new System.Windows.Point(e.X, e.Y));
            var pos = viewport.GetPickRayLineCheckPosition((float)viewPortPos.X, (float)viewPortPos.Y);
            mPreviewActor.mPreviewActor.Placement.Location = pos;
        }
        public async System.Threading.Tasks.Task OnDragDropViewport(EditorCommon.ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e)
        {
            GParticleComponent component = new GParticleComponent();
            var param = new EngineNS.Editor.PlantableItemCreateActorParam()
            {
                Location = new EngineNS.Vector3(0, 0, 0),
            };
            var dropActor = await component.CreateActor(param, this.ResourceName);
            //var dropActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(this.ResourceName);
            dropActor.SpecialName = EngineNS.GamePlay.SceneGraph.GSceneGraph.GeneratorActorSpecialNameInEditor(this.ResourceName.PureName(), viewport.World);
            EngineNS.CEngine.Instance.HitProxyManager.MapActor(dropActor);
            var viewPortPos = viewport.PointFromScreen(new System.Windows.Point(e.X, e.Y));
            var pos = viewport.GetPickRayLineCheckPosition((float)viewPortPos.X, (float)viewPortPos.Y);
            dropActor.Placement.Location = pos;
            var selActors = new List<EditorCommon.ViewPort.ViewPortControl.SelectActorData>(viewport.GetSelectedActors());

            var redoAction = new Action<object>((obj) =>
            {
                viewport.World.AddActor(dropActor);
                viewport.World.DefaultScene.AddActor(dropActor);
                viewport.SelectActor(dropActor);
            });
            redoAction.Invoke(null);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand("WorldEditOperation", null, redoAction, null, (obj) =>
            {
                viewport.World.RemoveActor(dropActor.ActorId);
                viewport.World.DefaultScene.RemoveActor(dropActor.ActorId);
                dropActor.Selected = false;
                viewport.SelectActors(selActors.ToArray());
            }, "添加对象");

            if (mPreviewActor != null)
            {
                await mPreviewActor.AwaitLoad();
            }
            viewport.World.RemoveActor(mPreviewActor.mPreviewActor.ActorId);
            viewport.World.DefaultScene.RemoveActor(mPreviewActor.mPreviewActor.ActorId);
        }

        protected override Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, EngineNS.RName oldRName)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> RenameOverride(string absFolder, string newName)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ICreateActor
        public async Task<GActor> CreateActor()
        {
            GParticleComponent component = new GParticleComponent();
            var param = new EngineNS.Editor.PlantableItemCreateActorParam()
            {
                Location = new EngineNS.Vector3(0, 0, 0),
            };
            var actor = await component.CreateActor(param, this.ResourceName);
            actor.SpecialName = ResourceName.PureName();
            return actor;
        }

        #endregion ICreateActor
        #region ICreateComponent
        public async Task<GComponent> CreateComponent(IComponentContainer componentContainer)
        {
            GActor hostActor = null;
            IComponentContainer hostContainer = null;
            if (componentContainer is GActor)
            {
                hostActor = componentContainer as GActor;
                hostContainer = componentContainer;
            }
            else if (componentContainer is GComponent)
            {
                hostActor = (componentContainer as GComponent).Host;
                hostContainer = componentContainer;
            }
            var rc = CEngine.Instance.RenderContext;
            var comp = new GParticleComponent();
            comp.ResetMacross(ResourceName);
            comp.SpecialName = ResourceName.PureName();
            return comp;
        }

        #endregion ICreateComponent
    }
}
