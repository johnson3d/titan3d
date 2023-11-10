using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Design;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace EngineNS.DesignMacross.Base.Description
{
    public class TtDescriptionASTBuildUtil
    {
        public static bool bGenerateDebugable = false;
        public static string ClassNamePrefix => "DMC_";
        public static string VariableNamePrefix => "Var_";
        public static string MethodNamePrefix => "Method_";
        public static string MethodLocalVarNamePrefix => "LocalVar_";
        public static string GenerateClassName(IClassDescription classDescription)
        {
            if(bGenerateDebugable)
            {
                return ClassNamePrefix + GetDescriptionCascadeName(classDescription); 
            }
            else
            {
                return ClassNamePrefix + classDescription.Name + "_" + (uint)classDescription.Id.GetHashCode() ;
            }
        }
        public static string GenerateVariableName(IVariableDescription  variableDescription)
        {
            if (bGenerateDebugable)
            {
                return VariableNamePrefix + GetDescriptionCascadeName(variableDescription);
            }
            else
            {
                return VariableNamePrefix + variableDescription.Name + "_" + (uint)variableDescription.Id.GetHashCode();
            }
        }
        public static string GenerateMethodName(IMethodDescription  methodDescription)
        {
            if (bGenerateDebugable)
            {
                return MethodNamePrefix + GetDescriptionCascadeName(methodDescription);
            }
            else
            {
                return MethodNamePrefix + methodDescription.Name + "_" + (uint)methodDescription.Id.GetHashCode();
            }
        }
        public static string GenerateMethodLocalVarName(IVariableDescription variableDescription)
        {
            if (bGenerateDebugable)
            {
                return MethodLocalVarNamePrefix + GetDescriptionCascadeName(variableDescription);
            }
            else
            {
                return MethodLocalVarNamePrefix + variableDescription.Name + "_" + (uint)variableDescription.Id.GetHashCode();
            }
        }
        static string GetDescriptionCascadeName(IDescription description)
        {
            if(description.Parent != null)
            {
                return GetDescriptionCascadeName(description.Parent) + "_" + description.Name;
            }
            return description.Name;
        }
        public static UClassDeclaration BuildDefaultPartForClassDeclaration(IClassDescription description, ref FClassBuildContext classBuildContext)
        {
            UClassDeclaration declaration = new UClassDeclaration();
            declaration.IsStruct = description.IsStruct;
            declaration.ClassName = description.ClassName;
            declaration.Comment = description.Comment;
            declaration.Namespace = classBuildContext.MainClassDescription.Namespace;
            declaration.VisitMode = description.VisitMode;
            declaration.SupperClassNames = description.SupperClassNames;
            foreach (var variableDesc in description.Variables)
            {
                var varDeclaration = BuildDefaultPartForVariableDeclaration(variableDesc, ref classBuildContext);
                declaration.Properties.Add(varDeclaration);
            }
            foreach (var methodDesc in description.Methods)
            {
                var methodDeclaration = BuildDefaultPartForMethodDeclaration(methodDesc, ref classBuildContext);
                declaration.Methods.Add(methodDeclaration);
            }
            return declaration;
        }
        public static UMethodDeclaration BuildDefaultPartForMethodDeclaration(IMethodDescription description, ref FClassBuildContext classBuildContext)
        {
            var desc = description as Design.TtMethodDescription;
            UMethodDeclaration declaration = new UMethodDeclaration();
            BuildDefaultPartForMethodDeclaration(description, ref declaration, ref classBuildContext);
            return declaration;
        }
        public static void BuildDefaultPartForMethodDeclaration(IMethodDescription description, ref UMethodDeclaration declaration, ref FClassBuildContext classBuildContext)
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
        public static UVariableDeclaration BuildDefaultPartForVariableDeclaration(IVariableDescription description, ref FClassBuildContext classBuildContext)
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
