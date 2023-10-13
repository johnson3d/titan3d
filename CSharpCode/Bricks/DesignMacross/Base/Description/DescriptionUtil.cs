using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace EngineNS.DesignMacross.Base.Description
{
    public class TtDescriptionUtil
    {
        public static string ClassNamePrefix => "Tt";
        public static string VariableNamePrefix => "Var_";
        public static string MethodNamePrefix => "Method_";
        public static string MethodLocalVarNamePrefix => "localVar_";
        public static void BuildDefaultPartForClassDeclaration(IClassDescription description, ref UClassDeclaration declaration)
        {
            declaration.IsStruct = description.IsStruct;
            declaration.ClassName = description.ClassName;
            declaration.Comment = description.Comment;
            declaration.Namespace = description.Namespace;
            declaration.VisitMode = description.VisitMode;
            declaration.SupperClassNames = description.SupperClassNames;
            foreach (var variableDesc in description.Variables)
            {
                var varDeclaration = BuildDefaultPartForVariableDeclaration(variableDesc);
                declaration.Properties.Add(varDeclaration);
            }
            foreach (var methodDesc in description.Methods)
            {
                var methodDeclaration = BuildDefaultPartForMethodDeclaration(methodDesc);
                declaration.Methods.Add(methodDeclaration);
            }
        }
        public static UMethodDeclaration BuildDefaultPartForMethodDeclaration(IMethodDescription description)
        {
            var desc = description as Design.TtMethodDescription;
            UMethodDeclaration declaration = new UMethodDeclaration();
            BuildDefaultPartForMethodDeclaration(description, ref declaration);
            return declaration;
        }
        public static void BuildDefaultPartForMethodDeclaration(IMethodDescription description, ref UMethodDeclaration declaration)
        {
            var desc = description as Design.TtMethodDescription;
            declaration.VisitMode = desc.VisitMode;
            declaration.ReturnValue = desc.ReturnValue;
            declaration.MethodName = desc.MethodName;
            declaration.Arguments = desc.Arguments;
            declaration.LocalVariables = desc.LocalVariables;
            declaration.Comment = desc.Comment;
            declaration.IsOverride = desc.IsOverride;
            declaration.IsAsync = desc.IsAsync;
        }
        public static UVariableDeclaration BuildDefaultPartForVariableDeclaration(IVariableDescription description)
        {
            UVariableDeclaration declaration = new UVariableDeclaration();
            declaration.VariableName = description.VariableName;
            declaration.VariableType = description.VariableType;
            declaration.InitValue = description.InitValue;
            declaration.Comment = description.Comment;
            declaration.VisitMode = description.VisitMode;
            return declaration;
        }
    }
}
