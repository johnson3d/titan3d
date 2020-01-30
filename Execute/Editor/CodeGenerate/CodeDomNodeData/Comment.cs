using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(CommentConstructParam))]
    public partial class Comment : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class CommentConstructParam : CodeGenerateSystem.Base.ConstructionParams, INotifyPropertyChanged
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion

            string mComment = "";
            [EngineNS.Rtti.MetaData]
            public string Comment
            {
                get => mComment;
                set
                {
                    mComment = value;
                    OnPropertyChanged("Comment");
                }
            }
            [EngineNS.Rtti.MetaData]
            public double Width { get; set; } = 300;
            [EngineNS.Rtti.MetaData]
            public double Height { get; set; } = 200;
            EngineNS.Color mColor = EngineNS.Color.White;
            [EngineNS.Rtti.MetaData]
            public EngineNS.Color Color
            {
                get => mColor;
                set
                {
                    mColor = value;
                    OnPropertyChanged("Color");
                }
            }

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as CommentConstructParam;
                retVal.Width = Width;
                retVal.Height = Height;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as CommentConstructParam;
                if (param == null)
                    return false;
                if (Width != param.Width ||
                    Height != param.Height)
                    return false;
                return true;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + Width.ToString() + Height.ToString()).GetHashCode();
            }
        }

        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        partial void InitConstruction();
        public Comment(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            var cpInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
            cpInfo.PropertyName = "Comment";
            cpInfo.PropertyType = typeof(string);
            cpInfo.PropertyAttributes.Add(new DisplayNameAttribute("注释"));
            cpInfos.Add(cpInfo);
            cpInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
            cpInfo.PropertyName = "Color";
            cpInfo.PropertyType = typeof(EngineNS.Color);
            cpInfo.PropertyAttributes.Add(new DisplayNameAttribute("颜色"));
            cpInfo.PropertyAttributes.Add(new EngineNS.Editor.Editor_ColorPicker());
            cpInfos.Add(cpInfo);
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);

            InitTemplateClass_WPF();
        }
        partial void InitTemplateClass_WPF();
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {

        }
    }
}
