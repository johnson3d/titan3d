using EngineNS.Bricks.CodeBuilder;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Description
{
    public class TtMethodDescription : IMethodDescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Method";
        public EVisisMode VisitMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public UCommentStatement Comment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public UVariableDeclaration ReturnValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string MethodName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<UMethodArgumentDeclaration> Arguments { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<UVariableDeclaration> LocalVariables { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsOverride { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsAsync { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public UMethodDeclaration BuildMethodDeclaration()
        {
            throw new NotImplementedException();
        }
    }
}
