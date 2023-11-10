using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.Rtti;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.DesignMacross.Design
{
    [OutlineElement_Leaf(typeof(TtOutlineElement_Variable))]
    public class TtVariableDescription : IVariableDescription, EGui.Controls.PropertyGrid.IPropertyCustomization, Bricks.NodeGraph.UEditableValue.IValueEditNotify
    {
        public IDescription Parent { get; set; }
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public string Name { get; set; } = "Variable";
        public string VariableName { get => TtDescriptionASTBuildUtil.VariableNamePrefix + Name; }
        [Rtti.Meta]
        public UTypeReference VariableType { get; set; } = new UTypeReference(UTypeDesc.TypeOf<bool>());
        [Rtti.Meta]
        public UExpressionBase InitValue { get; set; }
        [Rtti.Meta]
        public UCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;

        public bool IsPropertyVisibleDirty { get; set; } = false;

        public UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtDescriptionASTBuildUtil.BuildDefaultPartForVariableDeclaration(this, ref classBuildContext);
        }

        public void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            var pros = TypeDescriptor.GetProperties(this);
            var thisType = Rtti.UTypeDesc.TypeOf(this.GetType());
            foreach (PropertyDescriptor prop in pros)
            {
                var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                proDesc.InitValue(this, thisType, prop, parentIsValueType);
                switch (proDesc.Name)
                {
                    case "VariableType":
                        {
                            List<Rtti.UTypeDesc> types = new List<Rtti.UTypeDesc>(200);
                            proDesc.PropertyType = Rtti.UTypeDesc.TypeOf(typeof(System.Type));
                            foreach (var service in Rtti.UTypeDescManager.Instance.Services.Values)
                            {
                                foreach (var type in service.Types.Values)
                                {
                                    types.Add(type);
                                }
                            }
                            proDesc.CustomValueEditor = new EGui.Controls.PropertyGrid.PGTypeEditorAttribute(types.ToArray());
                        }
                        break;
                    case "InitValue":
                        {
                            if (VariableType.TypeDesc == null)
                            {
                                proDesc.ReleaseObject();
                                continue;
                            }
                            var editor = Bricks.NodeGraph.UEditableValue.CreateEditableValue(this, VariableType.TypeDesc, proDesc);
                            if (editor == null)
                            {
                                proDesc.ReleaseObject();
                                continue;
                            }
                            proDesc.PropertyType = VariableType.TypeDesc;
                            proDesc.CustomValueEditor = editor;
                        }
                        break;
                    case "Comment":
                        {
                            proDesc.PropertyType = Rtti.UTypeDesc.TypeOf(typeof(System.String));
                        }
                        break;
                }
                collection.Add(proDesc);
            }
        }

        public object GetPropertyValue(string propertyName)
        {
            switch (propertyName)
            {
                case "VariableType":
                    return VariableType.TypeDesc;
                case "InitValue":
                    {
                        var pe = InitValue as UPrimitiveExpression;
                        if (pe != null)
                            return pe.GetValue();
                    }
                    break;
                case "Comment":
                    return Comment?.CommentString;
                default:
                    {
                        var proInfo = GetType().GetProperty(propertyName);
                        if (proInfo != null)
                            return proInfo.GetValue(this);
                    }
                    break;
            }
            return null;
        }

        public void OnValueChanged(UEditableValue ev)
        {
            var pe = InitValue as UPrimitiveExpression;
            if (pe != null)
            {
                pe.ValueStr = UPrimitiveExpression.CalculateValueString(pe.Type, ev.Value);
            }
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            switch (propertyName)
            {
                case "VariableType":
                    {
                        var tagType = value as Rtti.UTypeDesc;
                        if (tagType != VariableType.TypeDesc)
                        {
                            InitValue = new UPrimitiveExpression(tagType, tagType.IsValueType ? Rtti.UTypeDescManager.CreateInstance(tagType) : null);
                            VariableType.TypeDesc = tagType;
                        }
                    }
                    break;
                case "InitValue":
                    {
                        var pe = InitValue as UPrimitiveExpression;
                        if (pe != null)
                            pe.ValueStr = UPrimitiveExpression.CalculateValueString(pe.Type, value);
                    }
                    break;
                case "Comment":
                    {
                        Comment.CommentString = (string)value;
                    }
                    break;
                default:
                    {
                        var proInfo = GetType().GetProperty(propertyName);
                        if (proInfo != null)
                            proInfo.SetValue(this, value);
                    }
                    break;
            }
        }
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            if(hostObject is IDescription parentDescription)
            {
                Parent = parentDescription;
            }    
            else
            {
                Debug.Assert(false);
            }
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
}
