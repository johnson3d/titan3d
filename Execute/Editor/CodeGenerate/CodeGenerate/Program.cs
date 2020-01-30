using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Windows.Media;

namespace CodeGenerateSystem
{
    public class CustomConstructionParamsAttribute : System.Attribute
    {
        public Type ConstructionParamsType;
        public CustomConstructionParamsAttribute(Type consParamsType)
        {
            ConstructionParamsType = consParamsType;
        }
    }

    public partial class Program
    {
        public static readonly string MessageCategory = "Macross";
        public static readonly string NodeDefaultName = "None";
        public static string NodeDragType
        {
            get { return "LinkNode"; }
        }

        public static DoubleCollection DebugLineDashArray = new DoubleCollection(new double[] { 2.0, 0.5 });
        public static Brush DebugLineColor = Brushes.Red;
        public static double DebugLineOffsetSpeed = -0.2;
        public static double DebugLineThickness = 10;

        //public static string AICode_NameSpacePre = "AI.Inst.";       // 命名空间前缀
        //public static string AICode_StateSetNamePre = "AI_Inst_";            // StateSet继承类前缀

        /*static System.Reflection.Assembly mAIStatementsEditorAssm = null;
        public static System.Reflection.Assembly AIStatementsEditorAssm
        {
            get
            {
                if (mAIStatementsEditorAssm == null)
                {
                    try
                    {
                        mAIStatementsEditorAssm = CSUtility.Program.GetAssemblyFromDllFileName("AIEditor.dll");
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }

                return mAIStatementsEditorAssm;
            }
        }

        static System.Reflection.Assembly mMaterialStatementsEditorAssm = null;
        public static System.Reflection.Assembly MaterialStatementsEditorAssm
        {
            get
            {
                if (mMaterialStatementsEditorAssm == null)
                {
                    try
                    {
                        mMaterialStatementsEditorAssm = CSUtility.Program.GetAssemblyFromDllFileName("MaterialEditor.dll");
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }

                return mMaterialStatementsEditorAssm;
            }
        }

        static System.Reflection.Assembly mDelegateMethodEditorAssm = null;
        public static System.Reflection.Assembly DelegateMethodEditorAssm
        {
            get
            {
                if (mDelegateMethodEditorAssm == null)
                {
                    try
                    {
                        mDelegateMethodEditorAssm = CSUtility.Program.GetAssemblyFromDllFileName("DelegateMethodEditor.dll");
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }

                return mDelegateMethodEditorAssm;
            }
        }

        public static Type GetType(string typeStr)
        {
#warning 去除Assembly依赖
            if (string.IsNullOrEmpty(typeStr))
                return null;

            Type retType = null;

            retType = Type.GetType(typeStr);
            if (retType == null && AIStatementsEditorAssm != null)
                retType = AIStatementsEditorAssm.GetType(typeStr);

            if (retType == null && MaterialStatementsEditorAssm != null)
                retType = MaterialStatementsEditorAssm.GetType(typeStr);

            if (retType == null && DelegateMethodEditorAssm != null)
                retType = DelegateMethodEditorAssm.GetType(typeStr);

            return retType;
        }*/

        public static object GetDefaultValueFromType(Type type)
        {
            if (type == null || type == typeof(void))
                return null;

            if (type.IsGenericType)
            {
                if (type.BaseType == typeof(System.Threading.Tasks.Task))
                {
                    var genericType = type.GetGenericArguments()[0];
                    return GetDefaultValueFromType(genericType);
                }
                else
                {
                    //return System.Activator.CreateInstance(type);
                    return null;
                }
            }
            else if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            else if (type == typeof(System.String))
                return "";
            else if (type.IsPointer)
                return null;
            else if (type.IsGenericParameter)
                throw new InvalidOperationException();

            return null;
        }

        public static System.CodeDom.CodeExpression GetDefaultValueExpressionFromType(Type type)
        {
            if (type == null || type == typeof(void) || type.IsInterface || type == typeof(System.Type) ||
                type.IsArray)
                return new System.CodeDom.CodePrimitiveExpression(null);
            if (type == typeof(System.String))
                return new System.CodeDom.CodePrimitiveExpression("");
            //else if(type.FullName == "SlimDX.Quaternion" ||
            //        type.FullName == "SlimDX.Matrix" ||
            //        type.FullName == "SlimDX.Matrix3x2" ||
            //        type.FullName == "EngineNS.Vector2" ||
            //        type.FullName == "SlimDX.Vector3" ||
            //        type.FullName == "SlimDX.Vector4")
            //{
            //    return new System.CodeDom.CodeObjectCreateExpression(type, new CodeExpression[0]);
            //}
            if(type == typeof(EngineNS.Color))
            {
                return new System.CodeDom.CodeSnippetExpression("CSUtility.Support.Color.FromArgb(0, 0, 0, 0)");
            }
            if(type.IsEnum)
            {
                if (System.Enum.GetValues(type).Length > 0)
                {
                    var value = System.Enum.GetValues(type).GetValue(0);
                    var name = System.Enum.GetName(type, value);
                    return new System.CodeDom.CodeSnippetExpression(EngineNS.Rtti.RttiHelper.GetAppTypeString(type) + "." + name);
                }                
            }
            if (type.IsClass)
            {
                var cInfo = type.GetConstructor(System.Reflection.BindingFlags.Public, null, new Type[0], null);
                if (cInfo == null)
                    return new System.CodeDom.CodePrimitiveExpression(null);
            }
            return new System.CodeDom.CodeObjectCreateExpression(type, new CodeExpression[0]);
        }

        //static void SetValueStatement(Type valueType, object value, string tempName, string valName, CodeStatementCollection codeStatementCollection, bool isDeclaration)
        //{
        //    CodeExpression proValueExp = null;
        //    if (valueType.IsGenericType)
        //    {
        //        GenerateGenericInitializeCode(codeStatementCollection, valueType, value, tempName, isDeclaration);
        //        proValueExp = new CodeArgumentReferenceExpression(tempName); ;
        //    }
        //    else if (valueType.IsArray)
        //    {
        //        GenerateArrayInitializeCode(codeStatementCollection, valueType, value, tempName, isDeclaration);
        //        proValueExp = new CodeArgumentReferenceExpression(tempName); ;
        //    }
        //    else if(valueType == typeof(string))
        //    {
        //        proValueExp = new System.CodeDom.CodePrimitiveExpression(value);
        //    }
        //    else if (valueType.IsClass)
        //    {
        //        GenerateClassInitializeCode(codeStatementCollection, valueType, value, tempName, isDeclaration);
        //        proValueExp = new CodeArgumentReferenceExpression(tempName); ;
        //    }
        //    else if(valueType == typeof(Guid))
        //    {
        //        proValueExp = new CodeSnippetExpression("CSUtility.Support.IHelper.GuidTryParse(\"" + value.ToString() + "\")");
        //    }
        //    else
        //        proValueExp = new System.CodeDom.CodePrimitiveExpression(value);

        //    // class.property = value;
        //    var sm = new System.CodeDom.CodeAssignStatement(new CodeFieldReferenceExpression(new System.CodeDom.CodeArgumentReferenceExpression(tempName), valName), proValueExp);
        //    codeStatementCollection.Add(sm);
        //}

        public static readonly string CodeGenerateAssemblyKeyName = "CodeGenerateSystem.dll";
        static Dictionary<string, System.Reflection.Assembly> mNodeAssemblys_StrKey = new Dictionary<string, System.Reflection.Assembly>();
        static Dictionary<System.Reflection.Assembly, string> mNodeAssemblys_AssKey = new Dictionary<System.Reflection.Assembly, string>();
        public static void RegisterNodeAssembly(string keyName, System.Reflection.Assembly assembly)
        {
            if (string.IsNullOrEmpty(keyName) || assembly == null)
                return;
            if (mNodeAssemblys_StrKey.ContainsKey(keyName))
                return;

            mNodeAssemblys_StrKey[keyName] = assembly;
            mNodeAssemblys_AssKey[assembly] = keyName;
        }
        public static void UnRegisterNodeAssembly(string keyName)
        {
            var ass = mNodeAssemblys_StrKey[keyName];
            mNodeAssemblys_StrKey.Remove(keyName);
            if (ass != null)
                mNodeAssemblys_AssKey.Remove(ass);
        }
        public static Type GetNodeControlTypeFromSaveString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;
            var idx = str.IndexOf('@');
            if (idx == -1)
                return null;// NodeAssembly.GetType(str);
            else
            {
                var splits = str.Split('@');
                var key = splits[0];
                System.Reflection.Assembly ass;
                if (mNodeAssemblys_StrKey.TryGetValue(key, out ass))
                    return ass.GetType(splits[1]);
            }
            return null;
        }
        public static string GetNodeControlTypeSaveString(Type type)
        {
            if (type == null)
                return "";

            string keyName = "";
            if(mNodeAssemblys_AssKey.TryGetValue(type.Assembly, out keyName))
            {
                return keyName + "@" + type.FullName;
            }
            return "";
        }
        /// <summary>
        /// This method returns a reference to mscorlib.dll or System.Runtime.dll that coresponds to
        /// the version of the framework referenced by ReferencedAssemblies property if it appears
        /// that these references point to a multi-targeting pack.  VBCodeProvider and CSharpCodeProvider
        /// use this method to provide a value for CoreAssemblyFileName if it was not set so the default
        /// mscorlib.dll is not used in cases where it looks like the developer intended to do multi-targeting.
        /// 
        /// The huristic here is as follows:
        /// If there is a reference that contains "\Reference Assemblies\Microsoft\Framework\<SkuName>\v<Version>"
        /// and for each reference of the above form, they all share the same set of directories starting with
        /// Reference Assemblies, then the probable core assembly is mscorlib.dll in that directory.
        /// Otherwise, we do not have a probable core assembly.
        /// 
        /// Note that we do no validation to ensure SkuName or Version are actually valid sku names or versions.
        /// The version component must start with a v but otherwise can be in any arbitrary form.
        /// </summary>
        internal static bool TryGetProbableCoreAssemblyFilePath(CompilerParameters parameters, out string coreAssemblyFilePath)
        {
            string multiTargetingPackRoot = null;
            char[] pathSeperators = new char[] { Path.DirectorySeparatorChar };

            // Valid paths look like "...\Reference Assemblies\Microsoft\Framework\<SkuName>\v<Version>\..."
            string referenceAssemblyFolderPrefix = Path.Combine("Reference Assemblies", "Microsoft", "Framework");

            foreach (string s in parameters.ReferencedAssemblies)
            {

                if (Path.GetFileName(s).Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase))
                {
                    // They already have their own mscorlib.dll, so let's not add another one.
                    coreAssemblyFilePath = string.Empty;
                    return false;
                }

                if (s.IndexOf(referenceAssemblyFolderPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                {

                    String[] dirs = s.Split(pathSeperators, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < dirs.Length - 5; i++)
                    {

                        if (String.Equals(dirs[i], "Reference Assemblies", StringComparison.OrdinalIgnoreCase))
                        {
                            // Here i+5 is the index of the thing after the vX.XXX folder (if one exists) and i+4 should look like a version.
                            // (i.e. start with a v).
                            if (dirs[i + 4].StartsWith("v", StringComparison.OrdinalIgnoreCase))
                            {
                                if (multiTargetingPackRoot != null)
                                {
                                    if (!String.Equals(multiTargetingPackRoot, Path.GetDirectoryName(s), StringComparison.OrdinalIgnoreCase))
                                    {
                                        // We found one reference to one targeting pack and one referece to another.  Bail out.
                                        coreAssemblyFilePath = string.Empty;
                                        return false;
                                    }
                                }
                                else
                                {
                                    multiTargetingPackRoot = Path.GetDirectoryName(s);
                                }
                            }
                        }
                    }
                }
            }

            if (multiTargetingPackRoot != null)
            {
                coreAssemblyFilePath = Path.Combine(multiTargetingPackRoot, "mscorlib.dll");
                return true;
            }

            coreAssemblyFilePath = string.Empty;
            return false;
        }
        internal static int NestedArrayDepth(CodeTypeReference arrayElementType)
        {
            if (arrayElementType == null)
                return 0;

            return 1 + NestedArrayDepth(arrayElementType.ArrayElementType);
        }
        internal static Assembly LoadImageSkipIntegrityCheck(byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence)
        {
            // Using reflection to avoid the need for a new public API.
            MethodInfo methodInfo = typeof(Assembly).GetMethod("LoadImageSkipIntegrityCheck", BindingFlags.NonPublic | BindingFlags.Static);

            // If running against old versions without the internal method, fallback to an older version that does not perform
            // the integrity check.
            Assembly result = methodInfo != null ?
                (Assembly)methodInfo.Invoke(null, new object[] { rawAssembly, rawSymbolStore, securityEvidence }) :
#pragma warning disable 618 // Load with evidence is obsolete - this warning is passed on via the options parameter
                Assembly.Load(rawAssembly, rawSymbolStore, securityEvidence);
#pragma warning restore 618

            return result;
        }
    }
}
