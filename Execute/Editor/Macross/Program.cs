using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Macross
{
    public class ArrayType2ImageSource : IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
        public static readonly ArrayType2ImageSource Instance = new ArrayType2ImageSource();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var at = (CodeDomNode.VariableType.enArrayType)value;
            switch(at)
            {
                case CodeDomNode.VariableType.enArrayType.Array:
                    return new BitmapImage(new Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/pillarray_16x.png"));
                case CodeDomNode.VariableType.enArrayType.Single:
                    return new BitmapImage(new Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/pill_16x.png"));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Program
    {
        public static readonly string MacrossCategoryItemDragDropTypeName = "MacrossCategoryItem";

        public static async System.Threading.Tasks.Task RefreshSaveFiles()
        {
            var dirs = EngineNS.CEngine.Instance.FileManager.GetDirectories(EngineNS.CEngine.Instance.FileManager.ProjectContent, "*.macross", System.IO.SearchOption.AllDirectories);
            foreach(var dir in dirs)
            {
                // client
                var cMacrossFile = dir + "/" + "data_client.macross";
                var CCtrl = new MacrossLinkControl();
                CCtrl.CSType = EngineNS.ECSType.Client;
                if(await CCtrl.LoadData(cMacrossFile))
                    CCtrl.SaveData(cMacrossFile);

                var sMacrossFile = dir + "/" + "data_server.macross";
                var SCtrl = new MacrossLinkControl();
                SCtrl.CSType = EngineNS.ECSType.Server;
                if(await SCtrl.LoadData(sMacrossFile))
                    SCtrl.SaveData(sMacrossFile);

                var linkFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(dir, "*.link");
                foreach(var linkFile in linkFiles)
                {
                    var assist = new NodesControlAssist();
                    var pureFile = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(linkFile, false);
                    var csIdx = pureFile.LastIndexOf('_');
                    var csTypeStr = pureFile.Substring(csIdx + 1);
                    var preIdx = pureFile.IndexOf('_') + 1;
                    var graphName = pureFile.Substring(preIdx, csIdx - preIdx);
                    switch(csTypeStr)
                    {
                        case "client":
                        case "Client":
                            assist.HostControl = CCtrl;
                            break;
                        case "server":
                        case "Server":
                            assist.HostControl = CCtrl;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    assist.LinkedCategoryItemName = graphName;
                    await assist.Load(linkFile, true);
                    assist.Save(linkFile);

                }
            }
        }

        public static string GetClassNamespace(EngineNS.RName rname)
        {
            if (rname == null)
                return "";
            return rname.RelativePath().Replace("/", ".").TrimEnd('.');
        }
        public static string GetClassNamespace(Macross.ResourceInfos.MacrossResourceInfo info, EngineNS.ECSType csType)
        {
            return GetClassNamespace(info.ResourceName);
        }
        public static string GetClassName(EngineNS.RName rname)
        {
            return rname.PureName();
        }
        public static string GetClassName(Macross.ResourceInfos.MacrossResourceInfo info, EngineNS.ECSType csType)
        {
            if (info.ResourceName == null)
                return "";
            return GetClassName(info.ResourceName);
        }

        public static void GenerateSetValueCode(object value, Type origionType, CodeExpression valueParamExp, CodeStatementCollection codeStatements, bool checkMeta = true)
        {
            if (value == null)
            {
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodePrimitiveExpression(null)));
                return;
            }

            var valueType = value.GetType();
            if (valueType.IsPrimitive)
            {
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodePrimitiveExpression(value)));
            }
            else if (valueType == typeof(string))
            {
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodePrimitiveExpression(value)));
            }
            else if (valueType.IsEnum)
            {
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeSnippetExpression(EngineNS.Rtti.RttiHelper.GetAppTypeString(value.GetType()) + "." + value.ToString())));
            }
            else if (valueType == typeof(EngineNS.Vector2))
            {
                var val = (EngineNS.Vector2)value;
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeObjectCreateExpression(typeof(EngineNS.Vector2),
                                                                                        new CodePrimitiveExpression(val.X),
                                                                                        new CodePrimitiveExpression(val.Y))));
            }
            else if (valueType == typeof(EngineNS.Vector3))
            {
                var val = (EngineNS.Vector3)value;
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeObjectCreateExpression(typeof(EngineNS.Vector3),
                                                                                        new CodePrimitiveExpression(val.X),
                                                                                        new CodePrimitiveExpression(val.Y),
                                                                                        new CodePrimitiveExpression(val.Z))));
            }
            else if (valueType == typeof(EngineNS.Vector4))
            {
                var val = (EngineNS.Vector4)value;
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeObjectCreateExpression(typeof(EngineNS.Vector4),
                                                                                        new CodePrimitiveExpression(val.X),
                                                                                        new CodePrimitiveExpression(val.Y),
                                                                                        new CodePrimitiveExpression(val.Z),
                                                                                        new CodePrimitiveExpression(val.Y))));
            }
            else if (valueType == typeof(EngineNS.Size))
            {
                var val = (EngineNS.Size)value;
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeObjectCreateExpression(typeof(EngineNS.Size),
                                                                                        new CodePrimitiveExpression(val.Width),
                                                                                        new CodePrimitiveExpression(val.Height))));
            }
            else if (valueType == typeof(EngineNS.SizeF))
            {
                var val = (EngineNS.SizeF)value;
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeObjectCreateExpression(typeof(EngineNS.SizeF),
                                                                                        new CodePrimitiveExpression(val.Width),
                                                                                        new CodePrimitiveExpression(val.Height))));
            }
            else if (valueType == typeof(EngineNS.Rectangle))
            {
                var val = (EngineNS.Rectangle)value;
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeObjectCreateExpression(typeof(EngineNS.Rectangle),
                                                                                        new CodePrimitiveExpression(val.X),
                                                                                        new CodePrimitiveExpression(val.Y),
                                                                                        new CodePrimitiveExpression(val.Width),
                                                                                        new CodePrimitiveExpression(val.Height))));
            }
            else if (valueType == typeof(EngineNS.RectangleF))
            {
                var val = (EngineNS.RectangleF)value;
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeObjectCreateExpression(typeof(EngineNS.RectangleF),
                                                                                        new CodePrimitiveExpression(val.X),
                                                                                        new CodePrimitiveExpression(val.Y),
                                                                                        new CodePrimitiveExpression(val.Width),
                                                                                        new CodePrimitiveExpression(val.Height))));
            }
            else if (valueType == typeof(EngineNS.Thickness))
            {
                var val = (EngineNS.Thickness)value;
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeObjectCreateExpression(typeof(EngineNS.Thickness),
                                                                                        new CodePrimitiveExpression(val.Left),
                                                                                        new CodePrimitiveExpression(val.Top),
                                                                                        new CodePrimitiveExpression(val.Right),
                                                                                        new CodePrimitiveExpression(val.Bottom))));
            }
            else if (valueType == typeof(EngineNS.RName))
            {
                var val = (EngineNS.RName)value;
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression("EngineNS.RName"),
                                                                                                         "GetRName",
                                                                                                         new CodePrimitiveExpression(val.Name),
                                                                                                         new CodeSnippetExpression(val.RNameType.GetType().FullName.Replace("+", ".") + "." + val.RNameType.ToString()))));
            }
            else if(valueType == typeof(EngineNS.Color4))
            {
                var val = (EngineNS.Color4)value;
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeObjectCreateExpression(typeof(EngineNS.Color4),
                                                                                        new CodePrimitiveExpression(val.Alpha),
                                                                                        new CodePrimitiveExpression(val.Red),
                                                                                        new CodePrimitiveExpression(val.Green),
                                                                                        new CodePrimitiveExpression(val.Blue))));
            }
            else if (valueType.IsGenericType && (valueType.GetInterface(typeof(IEnumerable).FullName) != null))
            {
                var methodAdd = valueType.GetMethod("Add");
                if (methodAdd == null)
                    return;
                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeObjectCreateExpression(valueType)));
                var genTypes = valueType.GetGenericArguments();

                var methodParamters = methodAdd.GetParameters();
                var enumerableValue = value as IEnumerable;
                if (enumerableValue == null)
                    return;

                if (methodParamters.Length == 1)         // List等
                {
                    foreach (var item in enumerableValue)
                    {
                        var itemParamName = "item_" + Guid.NewGuid().ToString().Replace("-", "_");
                        codeStatements.Add(new CodeVariableDeclarationStatement(item.GetType(), itemParamName));
                        GenerateSetValueCode(item, genTypes[0], new CodeVariableReferenceExpression(itemParamName), codeStatements);
                        codeStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(valueParamExp, "Add", new CodeVariableReferenceExpression(itemParamName)));
                    }
                }
                else if (methodParamters.Length == 2)    // Dictionary等
                {
                    foreach (var item in enumerableValue)
                    {
                        var proKey = item.GetType().GetProperty("Key");
                        var proValue = item.GetType().GetProperty("Value");
                        if (proKey == null || proValue == null)
                            return;

                        var keyValue = proKey.GetValue(item);
                        var keyParamName = "key_" + Guid.NewGuid().ToString().Replace("-", "_");
                        var keyExp = new CodeVariableReferenceExpression(keyParamName);
                        codeStatements.Add(new CodeVariableDeclarationStatement(keyValue.GetType(), keyParamName));
                        GenerateSetValueCode(keyValue, genTypes[0], keyExp, codeStatements);
                        var valValue = proValue.GetValue(item);
                        var valueParamName = "value_" + Guid.NewGuid().ToString().Replace("-", "_");
                        var valueExp = new CodeVariableReferenceExpression(valueParamName);
                        GenerateSetValueCode(valValue, genTypes[1], valueExp, codeStatements);
                        codeStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(valueParamExp, "Add", keyExp, valueExp));
                    }
                }
            }
            else if (valueType.IsArray)
            {
                var valueArray = value as System.Array;
                var rank = valueArray.Rank;
                var lengths = new Int32[rank];
                var idxes = new Int32[rank];
                var createStr = $"new {valueType.GetElementType().FullName}";
                for (int i = 0; i < rank; i++)
                {
                    lengths[i] = valueArray.GetLength(i);
                    idxes[i] = 0;
                    createStr += "[" + lengths[i] + "]";
                }

                codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeSnippetExpression(createStr)));
                var arrayParamStr = valueParamExp.UserData;

                while (true)
                {
                    var idxExps = new CodePrimitiveExpression[rank];
                    for (int i = 0; i < rank; i++)
                    {
                        idxExps[i] = new CodePrimitiveExpression(idxes[i]);
                    }

                    var arrayValue = valueArray.GetValue(idxes);
                    if (arrayValue == null)
                    {
                        codeStatements.Add(new CodeAssignStatement(new CodeArrayIndexerExpression(valueParamExp, idxExps), new CodePrimitiveExpression(null)));
                    }
                    else
                    {
                        var arrayElementName = "arrayValue_" + Guid.NewGuid().ToString().Replace("-", "_");
                        var valExp = new CodeVariableDeclarationStatement(arrayValue.GetType(), arrayElementName);
                        GenerateSetValueCode(arrayValue, valueType, new CodeVariableReferenceExpression(arrayElementName), codeStatements);
                        codeStatements.Add(new CodeAssignStatement(new CodeArrayIndexerExpression(valueParamExp, idxExps), new CodeVariableReferenceExpression(arrayElementName)));
                    }

                    if (CalculateArrayIdx(rank - 1, idxes, valueArray) == false)
                        break;
                }
            }
            else
            {
                var constructor = valueType.GetConstructor(new Type[0]);
                // 只做带无参数构造函数的对象
                if (constructor == null)
                    return;

                CodeExpression tempValExp = valueParamExp;
                if(origionType == valueType)
                {
                    codeStatements.Add(new CodeAssignStatement(valueParamExp, new CodeObjectCreateExpression(valueType)));
                }
                else
                {
                    var tempValName = "temp_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());
                    tempValExp = new CodeVariableReferenceExpression(tempValName);
                    codeStatements.Add(new CodeVariableDeclarationStatement(valueType, tempValName, new CodeObjectCreateExpression(valueType)));
                }

                var pros = valueType.GetProperties();
                var defaultValue = System.Activator.CreateInstance(valueType);
                foreach (var pro in pros)
                {
                    if (pro.SetMethod == null)
                        continue;
                    if (!pro.SetMethod.IsPublic)
                        continue;
                    if(checkMeta)
                    {
                        var atts = pro.GetCustomAttributes(typeof(EngineNS.Rtti.MetaDataAttribute), true);
                        if (atts.Length == 0)
                            continue;
                    }

                    var proValue = pro.GetValue(value);
                    var proDefaultValue = pro.GetValue(defaultValue);
                    if (object.Equals(proValue, proDefaultValue))
                        continue;

                    var proValueType = pro.PropertyType;
                    if (proValue != null)
                        proValueType = proValue.GetType();
                    if ((proValueType.IsClass && proValueType != typeof(string)) || (proValueType.IsValueType && !proValueType.IsPrimitive))
                    {
                        var proName = pro.Name + "_" + Guid.NewGuid().ToString().Replace("-", "_");
                        var proNameExp = new CodeVariableReferenceExpression(proName);
                        codeStatements.Add(new CodeVariableDeclarationStatement(proValueType, proName));
                        GenerateSetValueCode(proValue, pro.PropertyType, proNameExp, codeStatements);
                        codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(tempValExp, pro.Name), proNameExp));
                    }
                    else
                        GenerateSetValueCode(proValue, pro.PropertyType, new CodePropertyReferenceExpression(tempValExp, pro.Name), codeStatements);
                }
                var fields = valueType.GetFields();
                foreach (var field in fields)
                {
                    if(checkMeta)
                    {
                        var atts = field.GetCustomAttributes(typeof(EngineNS.Rtti.MetaDataAttribute), true);
                        if (atts.Length == 0)
                            continue;
                    }

                    var fieldValue = field.GetValue(value);
                    var fieldDefaultValue = field.GetValue(defaultValue);
                    if (fieldValue.Equals(fieldDefaultValue))
                        continue;

                    var fieldValueType = field.FieldType;
                    if (fieldValue != null)
                        fieldValueType = fieldValue.GetType();
                    if ((fieldValueType.IsClass && fieldValueType != typeof(string)) || (fieldValueType.IsValueType && !fieldValueType.IsPrimitive))
                    {
                        var fieldName = field.Name + "_" + Guid.NewGuid().ToString().Replace("-", "_");
                        var fieldNameExp = new CodeVariableReferenceExpression(fieldName);
                        codeStatements.Add(new CodeVariableDeclarationStatement(fieldValueType, fieldName));
                        GenerateSetValueCode(fieldValue, field.FieldType, fieldNameExp, codeStatements);
                        codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(tempValExp, field.Name), fieldNameExp));
                    }
                    else
                        GenerateSetValueCode(fieldValue, field.FieldType, new CodePropertyReferenceExpression(tempValExp, field.Name), codeStatements);
                }

                if (origionType != valueType)
                    codeStatements.Add(new CodeAssignStatement(valueParamExp, tempValExp));
            }
        }
        static bool CalculateArrayIdx(int degree, int[] idxes, Array valueArray)
        {
            idxes[degree]++;
            if (idxes[degree] >= valueArray.GetLength(degree))
            {
                if (degree == 0)
                    return false;
                idxes[degree] = 0;
                return CalculateArrayIdx(degree - 1, idxes, valueArray);
            }
            return true;
        }
    }
}
