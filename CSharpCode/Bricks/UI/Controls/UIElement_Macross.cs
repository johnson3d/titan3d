using EngineNS.Bricks.CodeBuilder;
using EngineNS.IO;
using EngineNS.UI.Bind;
using EngineNS.UI.Editor;
using EngineNS.UI.Event;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtUIElement
    {
        public Macross.UMacrossGetter<TtUIMacrossBase> MacrossGetter;

        #region Property bind

        public class UIBindingData_Element : IO.ISerializer
        {
            [Rtti.Meta]
            public string PropertyName { get; set; }
            [Rtti.Meta]
            public Rtti.UTypeDesc PropertyType { get; set; }
            [Rtti.Meta]
            public UInt64 Id { get; set; }

            public bool IsSame(UIBindingData_Element data)
            {
                return ((PropertyName == data.PropertyName) &&
                        (PropertyType == data.PropertyType) &&
                        (Id == data.Id));
            }

            public void GenerateStatement(TtUIEditor editor, List<TtStatementBase> statements)
            {
                var findElementInvokeStatement = new TtMethodInvokeStatement(
                    "FindBindObject",
                    new TtVariableDeclaration()
                    {
                        VariableName = "object_" + Id,
                        VariableType = new TtTypeReference(typeof(IBindableObject)),
                    },
                    new TtVariableReferenceExpression("HostElement"),
                    new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(Id)))
                {
                    DeclarationReturnValue = true,
                };
                for (int i = 0; i < statements.Count; i++)
                {
                    if (findElementInvokeStatement.Equals(statements[i]))
                        return;
                }
                statements.Add(findElementInvokeStatement);
            }
            public TtExpressionBase GetVariableExpression()
            {
                return new TtVariableReferenceExpression("object_" + Id);
            }
            public Rtti.UTypeDesc GetVariableType()
            {
                return PropertyType;
            }
            public string GetBindPath()
            {
                return PropertyName;
            }
            public bool IsSameTarget<T>(T target)
            {
                var element = target as TtUIElement;
                if (element != null)
                    return element.Id == Id;
                return false;
            }
            public void Draw(Editor.EditorUIHost host, in ImDrawList drawList)
            {
                var element = host.FindElement(Id);
                ImGuiAPI.Text(Editor.TtUIEditor.GetElementShowName(element));
                ImGuiAPI.SameLine(0, -1);
                ImGuiAPI.Text(PropertyName);
            }

            public void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {
            }

            public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
            {
            }
        }
        public abstract class BindingDataBase : IO.BaseSerializer
        {
            public virtual void DrawBindInfo(Editor.EditorUIHost host, in ImDrawList drawList) { }
            public virtual void GenerateStatement(TtUIEditor editor, List<TtStatementBase> sequence) { }
            public virtual void OnRemove(TtUIEditor editor) { }
            public abstract bool IsSame(BindingDataBase target);
        }

        // 辅助记录属性之间绑定数据，用于生成代码
        public class BindingData_Property : BindingDataBase
        {
            [Rtti.Meta]
            public UIBindingData_Element Source { get; set; }
            [Rtti.Meta]
            public UIBindingData_Element Target { get; set; }
            [Rtti.Meta]
            public UI.Bind.EBindingMode Mode { get; set; } = UI.Bind.EBindingMode.Default;

            public override void DrawBindInfo(Editor.EditorUIHost host, in ImDrawList drawList)
            {
                Source.Draw(host, drawList);
                ImGuiAPI.SameLine(0, -1);
                ImGuiAPI.Text(" (" + Mode.ToString() + ")");
            }
            public override void GenerateStatement(TtUIEditor editor, List<TtStatementBase> sequence)
            {
                Source.GenerateStatement(editor, sequence);
                Target.GenerateStatement(editor, sequence);
                var bindCall = new TtMethodInvokeStatement()
                {
                    MethodName = "SetBinding",
                    Host = new TtClassReferenceExpression(Rtti.UTypeDesc.TypeOf(typeof(Bind.TtBindingOperations))),
                };
                bindCall.GenericTypes.Add(Target.GetVariableType());
                bindCall.GenericTypes.Add(Source.GetVariableType());
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(Target.GetVariableExpression()));
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(Target.GetBindPath())));
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(Source.GetVariableExpression()));
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(Source.GetBindPath())));
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(Mode)));
                for (int i = 0; i < sequence.Count; i++)
                {
                    if (bindCall.Equals(sequence[i]))
                        return;
                }
                sequence.Add(bindCall);
            }

            public override bool IsSame(BindingDataBase target)
            {
                var bdp = target as BindingData_Property;
                if (bdp == null)
                    return false;
                return Source.IsSame(bdp.Source) &&
                       Target.IsSame(bdp.Target) &&
                       (Mode == bdp.Mode);
            }
        }

        // 辅助记录属性和macross函数绑定，用于生成代码
        public class BindingData_Method : BindingDataBase
        {
            [Rtti.Meta]
            public UIBindingData_Element Target { get; set; }
            [Rtti.Meta]
            public string SetMethodName { get; set; }
            [Rtti.Meta]
            public string GetMethodName { get; set; }
            [Rtti.Meta]
            public UI.Bind.EBindingMode Mode { get; set; } = UI.Bind.EBindingMode.Default;

            public override void DrawBindInfo(EditorUIHost host, in ImDrawList drawList)
            {
                ImGuiAPI.AlignTextToFramePadding();
                ImGuiAPI.Text("MethodBind:");
                ImGuiAPI.SameLine(0, -1);
                ImGuiAPI.BeginGroup();
                if (!string.IsNullOrEmpty(SetMethodName))
                    ImGuiAPI.Text("Set " + Target.PropertyName);
                if (!string.IsNullOrEmpty(GetMethodName))
                    ImGuiAPI.Text("Get " + Target.PropertyName);
                ImGuiAPI.EndGroup();
                ImGuiAPI.SameLine(0, -1);
                ImGuiAPI.AlignTextToFramePadding();
                ImGuiAPI.Text(" (" + Mode.ToString() + ")");
            }

            public override void GenerateStatement(TtUIEditor editor, List<TtStatementBase> sequence)
            {
                Target.GenerateStatement(editor, sequence);
                var bindCall = new TtMethodInvokeStatement()
                {
                    MethodName = "SetMethodBinding",
                    Host = new TtClassReferenceExpression(Rtti.UTypeDesc.TypeOf(typeof(Bind.TtBindingOperations))),
                };
                bindCall.GenericTypes.Add(Target.GetVariableType());
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(Target.GetVariableExpression()));
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(Target.GetBindPath())));
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(string.IsNullOrEmpty(GetMethodName)? (new TtNullValueExpression()) : (new TtVariableReferenceExpression(GetMethodName))));
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(string.IsNullOrEmpty(SetMethodName)? (new TtNullValueExpression()) : (new TtVariableReferenceExpression(SetMethodName))));
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(Mode)));
                for (int i = 0; i < sequence.Count; i++)
                {
                    if (bindCall.Equals(sequence[i]))
                        return;
                }
                sequence.Add(bindCall);
            }

            public override void OnRemove(TtUIEditor editor)
            {
                if(!string.IsNullOrEmpty(SetMethodName))
                {
                    var methodDesc = editor.UIAsset.MacrossEditor.DefClass.FindMethod(SetMethodName);
                    if(methodDesc != null)
                        editor.UIAsset.MacrossEditor.RemoveMethod(methodDesc);
                }
                if(!string.IsNullOrEmpty(GetMethodName))
                {
                    var methodDesc = editor.UIAsset.MacrossEditor.DefClass.FindMethod(GetMethodName);
                    if (methodDesc != null)
                        editor.UIAsset.MacrossEditor.RemoveMethod(methodDesc);
                }
            }
            public override bool IsSame(BindingDataBase target)
            {
                var tg = target as BindingData_Method;
                if (tg == null)
                    return false;
                return Target.IsSame(tg.Target) &&
                       (Mode == tg.Mode) &&
                       (SetMethodName == tg.SetMethodName) &&
                       (GetMethodName == tg.GetMethodName);
            }
        }
        public class BindingData_SelfProperty : BindingDataBase
        {
            [Rtti.Meta]
            public UIBindingData_Element Target { get; set; }
            [Rtti.Meta]
            public string PropertyName { get; set; }
            [Rtti.Meta]
            public UI.Bind.EBindingMode Mode { get; set; } = UI.Bind.EBindingMode.Default;
            
            public override void DrawBindInfo(EditorUIHost host, in ImDrawList drawList)
            {
                var prop = host.HostEditor.UIAsset.MacrossEditor.DefClass.FindMember(PropertyName);
                if(prop == null)
                {
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.ErrorStringColor);
                    ImGuiAPI.Text("Error bind property: " + PropertyName);
                    ImGuiAPI.PopStyleColor(1);
                }
                else
                {
                    ImGuiAPI.Text(prop.DisplayName);
                    ImGuiAPI.SameLine(0, -1);
                    ImGuiAPI.Text(" (" + Mode.ToString() + ")");
                }
            }
            public override void GenerateStatement(TtUIEditor editor, List<TtStatementBase> sequence)
            {
                var prop = editor.UIAsset.MacrossEditor.DefClass.FindMember(PropertyName);
                if (prop == null)
                    return;
                Target.GenerateStatement(editor, sequence);
                var bindCall = new TtMethodInvokeStatement()
                {
                    MethodName = "SetBinding",
                    Host = new TtClassReferenceExpression(Rtti.UTypeDesc.TypeOf(typeof(Bind.TtBindingOperations))),
                };
                bindCall.GenericTypes.Add(Target.GetVariableType());
                bindCall.GenericTypes.Add(prop.VariableType.TypeDesc);
                var targetVariableExp = Target.GetVariableExpression();
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(targetVariableExp));
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(Target.GetBindPath())));
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtSelfReferenceExpression()));
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(PropertyName)));
                bindCall.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(Mode)));
                var conditionStatement = new TtIfStatement()
                {
                    Condition = new TtBinaryOperatorExpression()
                    {
                        Left = targetVariableExp,
                        Right = new TtNullValueExpression(),
                        Operation = TtBinaryOperatorExpression.EBinaryOperation.Equality,
                    },
                    TrueStatement = bindCall,
                };
                sequence.Add(conditionStatement);
            }
            public override bool IsSame(BindingDataBase target)
            {
                var tg = target as BindingData_SelfProperty;
                if (tg == null)
                    return false;
                return Target.IsSame(tg.Target) &&
                       (Mode == tg.Mode) &&
                       (PropertyName == tg.PropertyName);
            }
        }

        //[Browsable(false)]
        //[Rtti.Meta]
        //public Dictionary<string, BindingDataBase> BindingDatas
        //{
        //    get;
        //    set;
        //} = new Dictionary<string, BindingDataBase>();

        //public virtual void ClearBindExpression(EngineNS.UI.Bind.TtBindableProperty bp)
        //{
        //    if(bp == null)
        //    {
        //        lock (mBindExprDic)
        //        {
        //            mBindExprDic.Clear();
        //        }
        //        foreach(var data in BindingDatas)
        //        {
        //            var bdMethod = data.Value as BindingData_Method;
        //            if (bdMethod == null)
        //                continue;

        //            MacrossMethodData md;
        //            if(MacrossMethods.TryGetValue(data.Key, out md))
        //            {
        //                var pbData = md as MacrossPropertyBindMethodData;
        //                if(pbData != null)
        //                {
        //                    if (pbData.GetDesc != null)
        //                        mMethodDisplayNames.Remove(pbData.GetDesc);
        //                    if (pbData.SetDesc != null)
        //                        mMethodDisplayNames.Remove(pbData.SetDesc);
        //                }
        //            }
        //        }

        //        BindingDatas.Clear();
        //    }
        //    else
        //    {
        //        lock (mBindExprDic)
        //        {
        //            mBindExprDic.Remove(bp);
        //        }

        //        BindingDatas.Remove(bp.Name);

        //        MacrossMethodData data;
        //        if(MacrossMethods.TryGetValue(bp.Name, out data))
        //        {
        //            var pbData = data as MacrossPropertyBindMethodData;
        //            if(pbData != null)
        //            {
        //                if(pbData.GetDesc != null)
        //                    mMethodDisplayNames.Remove(pbData.GetDesc);
        //                if(pbData.SetDesc != null)
        //                    mMethodDisplayNames.Remove(pbData.SetDesc);
        //            }
        //            MacrossMethods.Remove(bp.Name);
        //        }
        //    }
        //}

        #endregion

        //public string GetPropertyBindMethodName(string propertyName, bool isSet)
        //{
        //    if (isSet)
        //        return "Set_" + propertyName + "_" + Id;
        //    else
        //        return "Get_" + propertyName + "_" + Id;
        //}

        //public string GetEventMethodName(string eventName)
        //{
        //    return "On_" + eventName + "_" + Id;
        //}
        public class MacrossMethodData : BaseSerializer
        {
            public IBindableObject HostObject;
            public bool DisplayNameDirty = true;

            public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {
                HostObject = hostObject as IBindableObject;
            }

            public virtual string GetDisplayName(Bricks.CodeBuilder.TtMethodDeclaration desc)
            {
                if (desc == null)
                    return "none";
                return desc.MethodName;
            }
        }

        // 用于辅助记录event绑定到macross函数
        public class MacrossEventMethodData : MacrossMethodData
        {
            public Bricks.CodeBuilder.TtMethodDeclaration Desc;
            [Rtti.Meta]
            public string EventName { get; set; }
            string mDisplayName;
            public override string GetDisplayName(TtMethodDeclaration desc)
            {
                if(DisplayNameDirty)
                {
                    mDisplayName = $"On {EventName}({HostObject.Name})";
                    DisplayNameDirty = false;
                }
                return mDisplayName;
            }
        }

        // 用于辅助记录属性绑定到macross函数
        public class MacrossPropertyBindMethodData : MacrossMethodData
        {
            [Rtti.Meta]
            public string PropertyName { get; set; }

            public Bricks.CodeBuilder.TtMethodDeclaration GetDesc;
            public Bricks.CodeBuilder.TtMethodDeclaration SetDesc;
            string mGetMethodDisplayName;
            string mSetMethodDisplayName;
            public override string GetDisplayName(TtMethodDeclaration desc)
            {
                if (DisplayNameDirty)
                {
                    mSetMethodDisplayName = $"Set {PropertyName}({HostObject.Name})";
                    mGetMethodDisplayName = $"Get {PropertyName}({HostObject.Name})";
                    DisplayNameDirty = false;
                }
                if (desc == GetDesc)
                    return mGetMethodDisplayName;
                else if(desc == SetDesc)
                    return mSetMethodDisplayName;
                return "none";
            }
        }

        //internal Dictionary<Bricks.CodeBuilder.UMethodDeclaration, MacrossMethodData> mMethodDisplayNames = new Dictionary<Bricks.CodeBuilder.UMethodDeclaration, MacrossMethodData>();
        //public string GetMethodDisplayName(Bricks.CodeBuilder.UMethodDeclaration desc)
        //{
        //    MacrossMethodData val = null;
        //    if(mMethodDisplayNames.TryGetValue(desc, out val))
        //    {
        //        return val.GetDisplayName(desc);
        //    }
        //    return desc.MethodName;
        //}
        //void SetMethodDisplayNamesDirty()
        //{
        //    foreach(var name in mMethodDisplayNames)
        //    {
        //        name.Value.DisplayNameDirty = true;
        //    }
        //}

        //Dictionary<string, MacrossMethodData> mMacrossMethods = new Dictionary<string, MacrossMethodData>();
        //[Rtti.Meta]
        //[Browsable(false)]
        //public Dictionary<string, MacrossMethodData> MacrossMethods
        //{
        //    get => mMacrossMethods;
        //    set
        //    {
        //        mMacrossMethods = value;
        //    }
        //}
        //public void SetEventBindMethod(string eventName, Bricks.CodeBuilder.UMethodDeclaration desc)
        //{
        //    var data = new TtUIElement.MacrossEventMethodData()
        //    {
        //        EventName = eventName,
        //        Desc = desc,
        //        HostObject = this,
        //    };
        //    MacrossMethods[eventName] = data;
        //    mMethodDisplayNames[desc] = data;
        //}
        //public bool HasMethod(string keyName, out string methodDisplayName)
        //{
        //    MacrossMethodData data;
        //    if (MacrossMethods.TryGetValue(keyName, out data))
        //    {
        //        methodDisplayName = data.GetDisplayName(null);
        //        return true;
        //    }
        //    methodDisplayName = "";
        //    return false;
        //}

        //public void SetPropertyBindMethod(string propertyName, Bricks.CodeBuilder.UMethodDeclaration desc, bool isSet)
        //{
        //    MacrossMethodData data;
        //    if(MacrossMethods.TryGetValue(propertyName, out data))
        //    {
        //        var pbData = data as MacrossPropertyBindMethodData;
        //        if (isSet)
        //            pbData.SetDesc = desc;
        //        else
        //            pbData.GetDesc = desc;
        //    }
        //    else
        //    {
        //        var pbData = new TtUIElement.MacrossPropertyBindMethodData()
        //        {
        //            PropertyName = propertyName,
        //            HostObject = this,
        //        };
        //        if (isSet)
        //            pbData.SetDesc = desc;
        //        else
        //            pbData.GetDesc = desc;
        //        MacrossMethods[propertyName] = pbData;
        //        data = pbData;
        //    }
        //    mMethodDisplayNames[desc] = data;
        //}

        public string GetVariableDisplayName(Bricks.CodeBuilder.TtVariableDeclaration desc)
        {
            return this.Name;
        }
    }
}
