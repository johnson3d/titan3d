using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI.Controls
{
    public enum enBindMode
    {
        OnWay,
        OnWayToSource,
        TwoWay,
    }

    //public class TtVariableBindInfo
    //{
    //    //public UIElement BindFromUIElement;
    //    public int BindFromUIElementId;
    //    public string BindFromPropertyName;
    //    public Type BindFromPropertyType;

    //    public int BindToUIElementId;
    //    //public UIElement BindToUIElment;
    //    public Type BindToVariableType;

    //    string mVariableName;
    //    public string VariableName
    //    {
    //        get => mVariableName;
    //        set
    //        {
    //            mVariableName = value;
    //        }
    //    }

    //    enBindMode mBindMode = enBindMode.OnWay;
    //    public enBindMode BindMode
    //    {
    //        get => mBindMode;
    //        set
    //        {
    //            mBindMode = value;
    //        }
    //    }

    //    public string FunctionName_Get;
    //    public string FunctionName_Set;

    //    public TtVariableBindInfo()
    //    {
    //        //BindFromUIElement = hostUI;
    //    }
    //}

    public partial class TtUIElement
    {
        //// 属性与绑定函数的字典表
        //public Dictionary<string, string> PropertyBindFunctions = new Dictionary<string, string>();
        //public Action<TtUIElement, string, Type> PropertyCustomBindAddAction;
        //public Action<TtUIElement, string> PropertyCustomBindFindAction;
        //public Action<TtUIElement, string> PropertyCustomBindRemoveAction;

        //// 属性与成员变量的绑定字典表
        //public Dictionary<string, List<TtVariableBindInfo>> VariableBindInfosDic = new Dictionary<string, List<TtVariableBindInfo>>();
        //public Action<TtVariableBindInfo> VariableBindAddAction;
        //public Action<TtVariableBindInfo> VariableBindFindAction;
        //public Action<TtVariableBindInfo> VariableBindRemoveAction;
    }
}
