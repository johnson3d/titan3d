using System;
using System.ComponentModel;

namespace EditorCommon.PluginAssist
{
    public class PluginItem : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        protected Guid mId = Guid.Empty;
        [Browsable(false)]
        public Guid Id
        {
            get { return mId; }
        }

        string mPluginName = "";
        [Browsable(false)]
        public string PluginName
        {
            get { return mPluginName; }
            set
            {
                mPluginName = value;
                OnPropertyChanged("PluginName");
            }
        }

        bool mDefaultSelected = false;
        [DisplayName("默认插件")]
        public bool DefaultSelected
        {
            get { return mDefaultSelected; }
            set
            {
                mDefaultSelected = value;

                EditorCommon.PluginAssist.PluginManager.Instance.SetDefaultPlugin(this, value);
            }
        }


        protected EditorCommon.PluginAssist.IEditorPluginData mPluginData;
        [Browsable(false)]
        public EditorCommon.PluginAssist.IEditorPluginData PluginData
        {
            get { return mPluginData; }
        }

        public System.Windows.Controls.MenuItem HostMeuItem;

        protected EditorCommon.PluginAssist.IEditorPlugin mPluginObject;
        [Browsable(false)]
        public EditorCommon.PluginAssist.IEditorPlugin PluginObject
        {
            get { return mPluginObject; }
        }

        string mAssemblyPath;
        [Browsable(false)]
        public string AssemblyPath
        {
            get { return mAssemblyPath; }
            set
            {
                mAssemblyPath = value;
                OnPropertyChanged("AssemblyPath");
            }
        }

        [Browsable(false)]
        public System.Windows.UIElement InstructionControl
        {
            get
            {
                if (PluginObject == null)
                    return null;

                return PluginObject.InstructionControl;
            }
        }

        string mVersion = "";
        [Browsable(false)]
        public string Version
        {
            get { return mVersion; }
            set
            {
                mVersion = value;
                OnPropertyChanged("Version");
            }
        }

        bool mActive = true;
        [Browsable(false)]
        public bool Active
        {
            get { return mActive; }
            set
            {
                mActive = value;
                OnPropertyChanged("Active");

                if (HostMeuItem != null)
                {
                    if (mActive)
                    {
                        HostMeuItem.IsEnabled = true;
                    }
                    else
                    {
                        HostMeuItem.IsEnabled = false;
                    }
                }

            }
        }


        System.Windows.Visibility mDelBtnVisible = System.Windows.Visibility.Collapsed;
        [Browsable(false)]
        public System.Windows.Visibility DelBtnVisible
        {
            get { return mDelBtnVisible; }
            set
            {
                mDelBtnVisible = value;
                OnPropertyChanged("DelBtnVisible");
            }
        }

        public PluginItem(Guid id, EditorCommon.PluginAssist.IEditorPlugin plugin, EditorCommon.PluginAssist.IEditorPluginData pluginData)
        {
            mId = id;
            mPluginObject = plugin;
            mPluginData = pluginData;

            if (plugin != null)
            {
                PluginName = plugin.PluginName;
                Version = plugin.Version;

                if (plugin is EditorCommon.PluginAssist.IEditorPluginOperation)
                {
                    var obj = plugin as EditorCommon.PluginAssist.IEditorPluginOperation;
                    AssemblyPath = obj.AssemblyPath;
                    DelBtnVisible = System.Windows.Visibility.Visible;
                }
            }
        }
    }
}
