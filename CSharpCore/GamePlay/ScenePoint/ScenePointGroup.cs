using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.ScenePoint
{
    public partial class ScenePointGroup : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        /// <summary>
        /// 属性改变时调用的委托事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion


    }
}
