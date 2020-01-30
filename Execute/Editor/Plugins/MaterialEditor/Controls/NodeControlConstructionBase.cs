using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaterialEditor.Controls
{
    public class NodeControlConstructionBase : CodeGenerateSystem.Base.ConstructionParams, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public NodeControlConstructionBase()
        {
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(this.GetType());
        }
    }
}
