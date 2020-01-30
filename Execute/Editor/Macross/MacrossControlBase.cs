using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Macross
{
    public class MacrossControlBase : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        protected bool mIsDirty = false;
        public bool IsDirty
        {
            get { return mIsDirty; }
            set
            {
                mIsDirty = value;
                CurrentResourceInfo.IsDirty = mIsDirty;
            }
        }
        public Macross.ResourceInfos.MacrossResourceInfo CurrentResourceInfo
        {
            get;
            protected set;
        }

        public string UndoRedoKey
        {
            get
            {
                if (CurrentResourceInfo != null)
                {
                    return CurrentResourceInfo.Id.ToString();// + CSType.ToString();
                }
                return "";
            }
        }

        public string ParentClassName
        {
            get { return (string)GetValue(ParentClassNameProperty); }
            set { SetValue(ParentClassNameProperty, value); }
        }

        public static readonly DependencyProperty ParentClassNameProperty = DependencyProperty.Register("ParentClassName", typeof(string), typeof(MacrossControlBase), new UIPropertyMetadata(""));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MacrossControlBase), new UIPropertyMetadata(""));
        public bool JumpToErrorNode
        {
            get { return (bool)GetValue(JumpToErrorNodeProperty); }
            set { SetValue(JumpToErrorNodeProperty, value); }
        }
        public static readonly DependencyProperty JumpToErrorNodeProperty = DependencyProperty.Register("JumpToErrorNode", typeof(bool), typeof(MacrossControlBase), new UIPropertyMetadata(false));

        public bool OnlyCompileOpendMacross
        {
            get { return (bool)GetValue(OnlyCompileOpendMacrossProperty); }
            set { SetValue(OnlyCompileOpendMacrossProperty, value); }
        }
        public static readonly DependencyProperty OnlyCompileOpendMacrossProperty = DependencyProperty.Register("OnlyCompileOpendMacross", typeof(bool), typeof(MacrossControlBase), new UIPropertyMetadata(false));


        protected CodeGenerator mCodeGenerator = null;
        protected enum enCompileType
        {
            Debug,
            Release,
        }
        protected enCompileType mCompileType = enCompileType.Debug;
        //protected async Task<System.CodeDom.Compiler.CompilerResults> CompileMacrossCollector(MacrossLinkControlBase ctrl, EngineNS.EPlatformType platform)
        //{
        //    string collectorDllName = "";
        //    List<string> refAssemblys = new List<string>();
        //    refAssemblys.Add("System.dll");
        //    string compileOption = "";
        //    switch (ctrl.CSType)
        //    {
        //        case EngineNS.ECSType.Client:
        //            switch (platform)
        //            {
        //                case EngineNS.EPlatformType.PLATFORM_WIN:
        //                    collectorDllName = EngineNS.CEngine.Instance.FileManager.Bin + EngineNS.Macross.MacrossDataManager.MacrossCollectorDllName;
        //                    refAssemblys.Add(EngineNS.CEngine.Instance.FileManager.Bin + "CoreClient.Windows.dll");
        //                    if (mCompileType == enCompileType.Debug)
        //                        compileOption = "/define:MacrossDebug";
        //                    break;
        //                case EngineNS.EPlatformType.PLATFORM_DROID:
        //                    collectorDllName = EngineNS.CEngine.Instance.FileManager.Bin + EngineNS.Macross.MacrossDataManager.MacrossCollectorDllName_Android;
        //                    refAssemblys.Add(EngineNS.CEngine.Instance.FileManager.Bin + "CoreClient.Android.dll");
        //                    break;
        //                default:
        //                    throw new InvalidOperationException();
        //            }
        //            //refAssemblys.Add(EngineNS.CEngine.Instance.FileManager.Bin + "MacrossScript.dll");
        //            break;
        //        case EngineNS.ECSType.Server:
        //            throw new InvalidOperationException();
        //    }
        //    return await mCodeGenerator.CompileMacrossCollector(ctrl.CSType, collectorDllName, refAssemblys.ToArray(), compileOption, mCompileType == enCompileType.Debug);
           
        //}

        protected async Task<bool> CompileCode(MacrossLinkControlBase ctrl, EngineNS.ECSType csType)
        {
            await mCodeGenerator.GenerateAndSaveMacrossCollector(csType);
            var codeStr = await mCodeGenerator.GenerateCode(CurrentResourceInfo, ctrl);
            if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(CurrentResourceInfo.ResourceName.Address))
                EngineNS.CEngine.Instance.FileManager.CreateDirectory(CurrentResourceInfo.ResourceName.Address);
            var codeFile = $"{CurrentResourceInfo.ResourceName.Address}/{CurrentResourceInfo.ResourceName.PureName()}_{ctrl.CSType.ToString()}.cs";
            using (var fs = new System.IO.FileStream(codeFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
            {
                fs.Write(System.Text.Encoding.Default.GetBytes(codeStr), 0, Encoding.Default.GetByteCount(codeStr));
            }

            var files = mCodeGenerator.CollectionMacrossProjectFiles(csType);
            mCodeGenerator.GenerateMacrossProject(files.ToArray(), csType);

            return await EditorCommon.Program.BuildGameDllImmediately(true);
        }
    }
}
