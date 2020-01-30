using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerateSystem.Base
{
    public class GenerateCodeContext_PreNode
    {
        public bool NeedDereferencePoint = false;
    }

    public class GenerateCodeContext_Method
    {
        public Type ReturnValueType = typeof(void);
        public GenerateCodeContext_Class ClassContext
        {
            get;
            private set;
        }

        //给状态机中的状态使用的
        public System.CodeDom.CodeVariableReferenceExpression AnimStateMachineReferenceExpression
        {
            get;
            set;
        }
        //给状态子图使用的
        public System.CodeDom.CodeVariableReferenceExpression AminStateReferenceExpression
        {
            get;
            set;
        }
        //给state添加状态转换时用的
        public System.CodeDom.CodeVariableReferenceExpression ReturnedAminStateReferenceExpression
        {
            get;
            set;
        }
        //状态转换method
        public System.CodeDom.CodeMemberMethod StateTransitionMethodReferenceExpression
        {
            get;
            set;
        }
        //给StateEntry使用的
        public System.CodeDom.CodeVariableReferenceExpression FirstStateReferenceExpression
        {
            get;
            set;
        }
        //状态机的pose
        public System.CodeDom.CodeFieldReferenceExpression StateMachineAnimPoseReferenceExpression
        {
            get;
            set;
        }
        //状态的pose
        public System.CodeDom.CodeFieldReferenceExpression StateAnimPoseReferenceExpression
        {
            get;
            set;
        }
        //宏图的pose
        public System.CodeDom.CodeFieldReferenceExpression InstanceAnimPoseReferenceExpression
        {
            get;
            set;
        }
        //动作资产的pose
        public System.CodeDom.CodeExpression AnimAssetAnimPoseProxyReferenceExpression
        {
            get;
            set;
        }
        //动作所在的容器
        public System.CodeDom.CodeExpression AnimAssetTickHostReferenceExpression
        {
            get;
            set;
        }

        System.CodeDom.CodeMemberMethod mMethod;
        public System.CodeDom.CodeMemberMethod Method
        {
            get { return mMethod; }
        }

        public bool GenerateNext = true;

        public GenerateCodeContext_Method(GenerateCodeContext_Class classContext, System.CodeDom.CodeMemberMethod method)
        {
            ClassContext = classContext;
            mMethod = method;
        }

        public GenerateCodeContext_Method Copy()
        {
            GenerateCodeContext_Method ret = new GenerateCodeContext_Method(ClassContext, Method);
            ret.GenerateNext = GenerateNext;
            ret.ReturnValueType = ReturnValueType;
            return ret;
        }

        public bool IsReturnTypeIsTask()
        {
            if (ReturnValueType == null)
                return false;
            return ((ReturnValueType == typeof(System.Threading.Tasks.Task)) || 
                    (ReturnValueType.BaseType == typeof(System.Threading.Tasks.Task)));
        }

        #region delegate
        public bool IsDelegateInvokeMethod = false;
        public string DelegateMethodName;
        #endregion

        #region Local Param
        public MethodGenerateData MethodGenerateData;
        #endregion
    }

    public class GenerateCodeContext_Class
    {
        public GenerateCodeContext_Namespace NamespaceContext
        {
            get;
            private set;
        }

        public string ClassName
        {
            get => mCodeClass.Name;
        }

        //public Guid DebuggerID = Guid.Empty;
        public EngineNS.Editor.Runner.IEventInfo EventInfo = null;
        public readonly string ScopFieldName = "_mScope";

        System.CodeDom.CodeTypeDeclaration mCodeClass;
        public System.CodeDom.CodeTypeDeclaration CodeClass
        {
            get { return mCodeClass; }
        }
        public System.CodeDom.CodeTypeDeclaration DebugContextClass;

        public List<string> PtrInvokeParamNames
        {
            get;
        } = new List<string>();

        public GenerateCodeContext_Class(GenerateCodeContext_Namespace namespaceContext, System.CodeDom.CodeTypeDeclaration codeClass)
        {
            NamespaceContext = namespaceContext;
            mCodeClass = codeClass;
        }
    }

    public class GenerateCodeContext_Namespace
    {
        public string NamespaceString
        {
            get;
            private set;
        }

        public string Sign
        {
            get;
            set;
        }

        public Guid NameSpaceID = Guid.Empty;

        System.CodeDom.CodeNamespace mNameSpace;
        public System.CodeDom.CodeNamespace NameSpace
        {
            get { return mNameSpace; }
        }

        public GenerateCodeContext_Namespace(string namespaceStr, System.CodeDom.CodeNamespace nameSpace)
        {
            NamespaceString = namespaceStr;
            mNameSpace = nameSpace;
        }
    }

    public class CodeGenerator
    {
        public static System.CodeDom.Compiler.CompilerResults CompileCode(string codeStr, EngineNS.ECSType csType, Guid dllId, string absCodeFile = "", string dllOutputFile = "", bool debug = false)
        {
            System.CodeDom.Compiler.CodeDomProvider cdProvider = new CodeGenerateSystem.CSharpCodeProvider();

            System.CodeDom.Compiler.CompilerParameters compilerParam = new System.CodeDom.Compiler.CompilerParameters();
            compilerParam.GenerateExecutable = false;
            compilerParam.GenerateInMemory = false;

            compilerParam.ReferencedAssemblies.Add("System.dll");

            switch (csType)
            {
                case EngineNS.ECSType.Client:
                    compilerParam.ReferencedAssemblies.Add(EngineNS.CEngine.Instance.FileManager.Bin + "/ClientCommon.dll");
                    compilerParam.ReferencedAssemblies.Add(EngineNS.CEngine.Instance.FileManager.Bin + "/Client.dll");
                    break;

                //case EngineNS.ECSType.Server:
                //    compilerParam.ReferencedAssemblies.Add(EngineNS.CEngine.Instance.FileManager.Root + CSUtility.Support.IFileConfig.Server_Directory + "/ServerCommon.dll");
                //    compilerParam.ReferencedAssemblies.Add(EngineNS.CEngine.Instance.FileManager.Root + CSUtility.Support.IFileConfig.Server_Directory + "/Server.dll");
                //    break;
            }

            if (!string.IsNullOrEmpty(dllOutputFile))
                compilerParam.OutputAssembly = dllOutputFile;

            System.CodeDom.Compiler.CompilerResults compilerResult = null;

            if (debug == true)
            {
                compilerParam.IncludeDebugInformation = true;

                if(string.IsNullOrEmpty(absCodeFile))
                {
                    absCodeFile = $"{EngineNS.CEngine.Instance.FileManager.ProjectSourceRoot}CodeFiles\\{dllId}_{csType.ToString()}.cs";
                }
                var fileDir = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(absCodeFile);
                if (!System.IO.Directory.Exists(fileDir))
                {
                    System.IO.Directory.CreateDirectory(fileDir);
                }
                var fs = new System.IO.FileStream(absCodeFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
                fs.Write(System.Text.Encoding.Default.GetBytes(codeStr), 0, System.Text.Encoding.Default.GetByteCount(codeStr));
                fs.Close();

                absCodeFile = absCodeFile.Replace("/", "\\");
                compilerResult = cdProvider.CompileAssemblyFromFile(compilerParam, new string[] { absCodeFile });

                var purefileName = dllOutputFile.Substring(0, dllOutputFile.Length - 4);
                var pdbFile = purefileName + ".pdb";
                if (EngineNS.CEngine.Instance.FileManager.FileExists(pdbFile))
                {
                    EngineNS.CEngine.Instance.FileManager.CopyFile(pdbFile, purefileName + ".vpdb", true);
                    EngineNS.CEngine.Instance.FileManager.DeleteFile(pdbFile);
                }
            }
            else
                compilerResult = cdProvider.CompileAssemblyFromSource(compilerParam, codeStr);

            return compilerResult;
        }
    }
}
