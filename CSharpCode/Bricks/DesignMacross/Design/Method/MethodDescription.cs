using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Design
{
    public class TtMethodDescription : IMethodDescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Method";
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        public UCommentStatement Comment { get; set; }
        public UVariableDeclaration ReturnValue { get; set; }
        public string MethodName { get; set; } = "Method";
        public List<UMethodArgumentDeclaration> Arguments { get; set; } = new List<UMethodArgumentDeclaration>();
        public List<UVariableDeclaration> LocalVariables { get; set; } = new List<UVariableDeclaration>();
        public bool IsOverride { get; set; } = false;
        public bool IsAsync { get; set; } = false;

        public UMethodDeclaration BuildMethodDeclaration()
        {
            return TtDescriptionUtil.BuildDefaultPartForMethodDeclaration(this);
        }
        public void UMethodDeclaration(ref UMethodDeclaration methodDeclaration)
        {
            TtDescriptionUtil.BuildDefaultPartForMethodDeclaration(this, ref methodDeclaration);
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
