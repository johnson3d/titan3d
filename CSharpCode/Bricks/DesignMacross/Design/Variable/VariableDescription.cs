using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using System;
using System.Reflection;

namespace EngineNS.DesignMacross.Design
{
    public class TtVariableDescription : IVariableDescription
    {
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public string Name { get; set; } = "Variable";
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        public string VariableName { get => TtDescriptionUtil.VariableNamePrefix + Name; }
        [Rtti.Meta]
        public UTypeReference VariableType { get; set; }
        [Rtti.Meta]
        public UExpressionBase InitValue { get; set; }
        [Rtti.Meta]
        public UCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;

        public UVariableDeclaration BuildVariableDeclaration()
        {
            return TtDescriptionUtil.BuildDefaultPartForVariableDeclaration(this);
        }
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
}
