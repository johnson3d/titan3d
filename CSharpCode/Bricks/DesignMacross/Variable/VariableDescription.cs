using EngineNS.Bricks.CodeBuilder;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Description
{
    public class TtVariableDescription : IVariableDescription
    {
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public string Name { get; set; } = "Variable";
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
    }
}
