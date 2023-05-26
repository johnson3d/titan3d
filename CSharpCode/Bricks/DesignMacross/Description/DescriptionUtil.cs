using EngineNS.Bricks.CodeBuilder;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace EngineNS.DesignMacross.Description
{
    public class TtDescriptionUtil
    {
        public static String ClassNamePrefix => "Tt";
        public static String VariableNamePrefix => "Var_";
        public static String MethodNamePrefix => "Method_";
        public static String MethodLocalVarNamePrefix => "localVar_";
        public static void BuildDefaultPartForClassDeclaration(IClassDescription description, ref UClassDeclaration declaration)
        {
            declaration.IsStruct = description.IsStruct;
            declaration.ClassName = description.ClassName;
            declaration.Comment = description.Comment;
            declaration.Namespace = description.Namespace;
            declaration.VisitMode = description.VisitMode;
            declaration.SupperClassNames = description.SupperClassNames;
            foreach(var variableDesc in description.Variables)
            {
                var varDeclaration = BuildDefaultPartForVariableDeclaration(variableDesc);
                declaration.Properties.Add(varDeclaration);
            }
            foreach(var methodDesc in description.Methods)
            {
                var methodDeclaration = BuildDefaultPartForMethodDeclaration(methodDesc);
                declaration.Methods.Add(methodDeclaration);
            }
        }
        public static UMethodDeclaration BuildDefaultPartForMethodDeclaration(IMethodDescription description)
        {
            UMethodDeclaration declaration = new UMethodDeclaration();

            return declaration;
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
