using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for FileEditorControl.xaml
    /// </summary>
    public partial class FileEditorControl : UserControl
    {
        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty =
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(FileEditorControl), new UIPropertyMetadata(null));


        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(FileEditorControl), new UIPropertyMetadata(null));

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set
            {
                SetValue(FilePathProperty, value);
            }
        }

        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(FileEditorControl),
                                                        new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnFilePathChanged)
                                        ));

        public static void OnFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //FileEditorControl ctrl = d as FileEditorControl;
            //if (string.IsNullOrEmpty(ctrl.FilePath))
            //    return;

            //var curPath = System.IO.Directory.GetCurrentDirectory();
            //var subPath = curPath.Remove(curPath.IndexOf("bin"));
        }

        public FileEditorControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string defaultDirection = "";
            List<string> extNames = new List<string>();
            bool openFolder = false;
            foreach (var att in BindProperty.Attributes)
            {
                if (att is EngineNS.Editor.OpenFileEditorAttribute)
                {
                    extNames = ((EngineNS.Editor.OpenFileEditorAttribute)att).ExtNames;
                }
                //////else if (att is EngineNS.Editor.UIEditor_DefaultFontPathAttribute)
                //////{
                //////    defaultDirection = EngineNS.Support.IFileManager.Instance.Root + EngineNS.Support.IFileConfig.DefaultFontDirectory;
                //////}
                else if(att is EngineNS.Editor.OpenFolderEditorAttribute)
                {
                    openFolder = true;
                }
            }

            if (openFolder)
            {
                var ofd = new System.Windows.Forms.FolderBrowserDialog();
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FilePath = ofd.SelectedPath.Replace("\\", "/");
                }
            }
            else
            {
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();

                string tempStr = "";
                foreach (var extName in extNames)
                {
                    tempStr += "|" + "(*." + extName + ")|*." + extName;
                }
                ofd.Filter = "All files (*.*)|*.*" + tempStr;
                //ofd.Filter = "(*." + extName + ")|*." + extName + "|All files (*.*)|*.*";
                if (!string.IsNullOrEmpty(defaultDirection))
                {
                    ofd.InitialDirectory = defaultDirection;
                    ofd.RestoreDirectory = true;
                }
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //var file = ofd.FileName.Substring(ofd.FileName.IndexOf("Release") + 8);
                    var file = ofd.FileName;
                    FilePath = file.Replace('\\', '/');
                }
            }
        }

        private void ResetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            FilePath = "";
        }
    }
}
