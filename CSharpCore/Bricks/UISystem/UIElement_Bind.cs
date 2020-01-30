using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UISystem
{
    public enum enBindMode
    {
        OnWay,
        OnWayToSource,
        TwoWay,
    }
    public class VariableBindInfo : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        //public UIElement BindFromUIElement;
        public int BindFromUIElementId;
        public string BindFromPropertyName;
        public Type BindFromPropertyType;

        public int BindToUIElementId;
        //public UIElement BindToUIElment;
        public Type BindToVariableType;

        string mVariableName;
        public string VariableName
        {
            get => mVariableName;
            set
            {
                mVariableName = value;
                OnPropertyChanged("VariableName");
            }
        }

        enBindMode mBindMode = enBindMode.OnWay;
        public enBindMode BindMode
        {
            get => mBindMode;
            set
            {
                mBindMode = value;
                OnPropertyChanged("BindMode");
            }
        }

        public string FunctionName_Get;
        public string FunctionName_Set;

        public VariableBindInfo()
        {
            //BindFromUIElement = hostUI;
        }
    }
    public partial class UIElement
    {
        // 属性与绑定函数的字典表
        public Dictionary<string, string> PropertyBindFunctions = new Dictionary<string, string>();
        public Action<UIElement, string, Type> PropertyCustomBindAddAction;
        public Action<UIElement, string> PropertyCustomBindFindAction;
        public Action<UIElement, string> PropertyCustomBindRemoveAction;

        // 属性与成员变量的绑定字典表
        public Dictionary<string, List<VariableBindInfo>> VariableBindInfosDic = new Dictionary<string, List<VariableBindInfo>>();
        public Action<VariableBindInfo> VariableBindAddAction;
        public Action<VariableBindInfo> VariableBindFindAction;
        public Action<VariableBindInfo> VariableBindRemoveAction;
    }
}
