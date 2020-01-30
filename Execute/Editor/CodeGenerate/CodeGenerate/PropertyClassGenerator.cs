using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CodeGenerateSystem.Base
{
    public class GeneratorClassBase : EngineNS.IO.Serializer.Serializer, INotifyPropertyChanged
    {
        public bool IgnoreOnPropertyChangedAction = false;
        bool mEnableUndoRedo = false;
        string mUndoRedoKey;
        string mMsgName;
        public EngineNS.ECSType CSType = EngineNS.ECSType.Client;

        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        public Action<string, object, object> OnPropertyChangedAction;
        protected void OnPropertyChanged(string propertyName, object newValue, object oldValue)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            if(!IgnoreOnPropertyChangedAction && mEnableUndoRedo)
            {
                var newVal = newValue;
                var oldVal = oldValue;
                EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(mUndoRedoKey, null,
                                    (obj) =>
                                    {
                                        IgnoreOnPropertyChangedAction = true;
                                        var pro = GetType().GetProperty(propertyName);
                                        if (pro != null)
                                        {
                                            pro.SetValue(this, newVal);
                                        }
                                        OnPropertyChangedAction?.Invoke(propertyName, newVal, oldVal);
                                        IgnoreOnPropertyChangedAction = false;
                                    }, null,
                                    (obj) =>
                                    {
                                        IgnoreOnPropertyChangedAction = true;
                                        var pro = GetType().GetProperty(propertyName);
                                        if (pro != null)
                                        {
                                            pro.SetValue(this, oldVal);
                                        }
                                        OnPropertyChangedAction?.Invoke(propertyName, newVal, oldVal);
                                        IgnoreOnPropertyChangedAction = false;
                                    }, $"Set {mMsgName}.{propertyName}");

            }
            else if(!IgnoreOnPropertyChangedAction)
                OnPropertyChangedAction?.Invoke(propertyName, newValue, oldValue);
        }
        #endregion

        public void EnableUndoRedo(string undoRedoKey, string msgName)
        {
            mUndoRedoKey = undoRedoKey;
            mMsgName = msgName;
            mEnableUndoRedo = true;
        }
        public void DisableUndoRedo()
        {
            mEnableUndoRedo = false;
        }
    }

    public class CustomPropertyInfo : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public string PropertyName;
        public Type PropertyType;
        public List<Attribute> PropertyAttributes = new List<Attribute>();
        public object DefaultValue;
        object mCurrentValue = null;
        public object CurrentValue
        {
            get { return mCurrentValue; }
            set
            {
                mCurrentValue = value;
                OnPropertyChanged("CurrentValue");
            }
        }

        public Action<TypeBuilder> CustomAction_PreDef = null;
        public Action<ILGenerator, FieldBuilder> CustomAction_PropertySet = null;
        public Action<ILGenerator, FieldBuilder> CustomAction_PropertyGet = null;

        public string GetMethodName
        {
            get { return "get_" + PropertyName; }
        }
        public string SetMethodName
        {
            get { return "set_" + PropertyName; }
        }

        public static CustomPropertyInfo GetFromParamInfo(System.Reflection.ParameterInfo info)
        {
            if (info == null)
                return null;
            
            CustomPropertyInfo retInfo = new CustomPropertyInfo();            
            retInfo.PropertyName = info.Name;
            if (info.IsOut || info.ParameterType.IsByRef)
                retInfo.PropertyType = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(info.ParameterType.FullName.Remove(info.ParameterType.FullName.Length - 1));
            else
                //retInfo.PropertyType = CSUtility.Program.GetTypeFromTypeFullName(info.ParameterType.FullName);
                retInfo.PropertyType = info.ParameterType;

            foreach (var att in info.GetCustomAttributes(true))
            {
                retInfo.PropertyAttributes.Add(att as Attribute);
            }
            retInfo.DefaultValue = CodeGenerateSystem.Program.GetDefaultValueFromType(retInfo.PropertyType);
            retInfo.CurrentValue = retInfo.DefaultValue;
            return retInfo;
        }

        public static CustomPropertyInfo GetFromParamInfo(Type paramType, string paramName, Attribute[] attributes = null)
        {
            if (paramType == null || string.IsNullOrEmpty(paramName))
                return null;

            CustomPropertyInfo retInfo = new CustomPropertyInfo();
            retInfo.PropertyName = paramName;
            retInfo.PropertyType = paramType;

            if(attributes != null)
            {
                foreach (var att in attributes)
                {
                    retInfo.PropertyAttributes.Add(att);
                }
            }
            retInfo.DefaultValue = CodeGenerateSystem.Program.GetDefaultValueFromType(retInfo.PropertyType);
            retInfo.CurrentValue = retInfo.DefaultValue;
            return retInfo;
        }
    }

    /// <summary>
    /// 动态生成属性类以便PropertyGrid使用
    /// </summary>
    public partial class PropertyClassGenerator
    {
        public static bool IsPropertyInfoValid(CustomPropertyInfo proInfo)
        {
            if (proInfo == null || proInfo.PropertyType==null)
                return false;
            if (proInfo.PropertyType.IsGenericParameter || proInfo.PropertyType.IsGenericType || proInfo.PropertyType.IsInterface)
                return false;

            if (EngineNS.Rtti.AttributeHelper.GetCustomAttribute(proInfo.PropertyType, typeof(EngineNS.Editor.Editor_LinkSystemCustomClassPropertyEnableShowAttribute).FullName, false) == null)
            {
                if (proInfo.PropertyType != typeof(string) && proInfo.PropertyType != typeof(System.Type) && proInfo.PropertyType.IsClass)
                    return false;
            }
            return true;
        }

        static Dictionary<string, ModuleBuilder> mModuleBuilderDic = new Dictionary<string, ModuleBuilder>();
        public static Type CreateTypeFromCustomPropertys(List<CustomPropertyInfo> propertys, string assemblyName, string className)
        {
            className = className.Replace("*", "");
            className = className.Replace("+", ".");

            var asmName = new AssemblyName()
            {
                Name = assemblyName
            };
            ModuleBuilder moduleBuilder;
            if(!mModuleBuilderDic.TryGetValue(assemblyName, out moduleBuilder))
            {
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, System.Reflection.Emit.AssemblyBuilderAccess.RunAndCollect);
                moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
                mModuleBuilderDic.Add(assemblyName, moduleBuilder);
            }
            var retType = moduleBuilder.GetType(className);
            if (retType != null)
            {
                return retType;
            }
            var typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Class | TypeAttributes.Public, typeof(GeneratorClassBase), new Type[] { typeof(System.ComponentModel.INotifyPropertyChanged) });

            // HostNode 用于与此对象所属节点通讯，修改dirty等
            //public CodeGenerateSystem.Base.BaseNodeControl HostNode;
            var hostNodeFieldBuilder = typeBuilder.DefineField("HostNode", typeof(CodeGenerateSystem.Base.BaseNodeControl), FieldAttributes.Public);
            var setDirtyMethod = typeof(CodeGenerateSystem.Base.BaseNodeControl).GetMethod("set_IsDirty");
            var onPropertyChangedMethod = typeof(GeneratorClassBase).GetMethod("OnPropertyChanged", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
            foreach(var proInfo in propertys)
            {
                if (!IsPropertyInfoValid(proInfo))
                    continue;

                proInfo.CustomAction_PreDef?.Invoke(typeBuilder);

                var fieldBuilder = typeBuilder.DefineField("m" + proInfo.PropertyName, proInfo.PropertyType, FieldAttributes.Private);
                var fieldOldValueBuilder = typeBuilder.DefineField("m" + proInfo.PropertyName + "OldValue", proInfo.PropertyType, FieldAttributes.Private);
//                fieldBuilder.SetConstant(proInfo.CurrentValue);
                var proBuilder = typeBuilder.DefineProperty(proInfo.PropertyName, PropertyAttributes.HasDefault, proInfo.PropertyType, null);
                //                proBuilder.SetConstant(proInfo.DefaultValue);

                bool needTypeAtt = false;
                if (proInfo.PropertyType == typeof(Type))
                    needTypeAtt = true;
                foreach(var proAtt in proInfo.PropertyAttributes)
                {
                    if (proAtt is EngineNS.Editor.Editor_UseCustomEditorAttribute)
                        needTypeAtt = false;

                    if (proAtt is System.Runtime.InteropServices.OptionalAttribute)
                        continue;
                    else if (proAtt is EngineNS.Editor.Editor_BaseAttribute)
                    {
                        var att = proAtt as EngineNS.Editor.Editor_BaseAttribute;
                        var consParams = att.GetConstructParams();
                        ConstructorInfo attConstructor;
                        if (consParams == null)
                        {
                            attConstructor = proAtt.GetType().GetConstructor(Type.EmptyTypes);
                            consParams = new object[0];
                        }
                        else
                        {
                            Type[] paramTypes = new Type[consParams.Length];
                            for (int i = 0; i < consParams.Length; i++)
                            {
                                var consPar = consParams[i];
                                // consPar不应该为空，这里不判断，直接异常好抓
                                paramTypes[i] = consPar.GetType();
                            }
                            attConstructor = proAtt.GetType().GetConstructor(paramTypes);
                        }
                        var attBuilder = new CustomAttributeBuilder(attConstructor, consParams);
                        proBuilder.SetCustomAttribute(attBuilder);
                    }
                    else if (proAtt.GetType().FullName.Equals(typeof(EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute).FullName))
                    {
                        //needDataTemplateAtt = false;
                        var att = proAtt as EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute;
                        var attConstructor = proAtt.GetType().GetConstructor(new Type[] { typeof(string), typeof(object[]) });
                        var attBuilder = new CustomAttributeBuilder(attConstructor, new object[] { att.DataTemplateType, att.Args });
                        proBuilder.SetCustomAttribute(attBuilder);
                    }
                    else if (proAtt is DescriptionAttribute)
                    {
                        var att = proAtt as DescriptionAttribute;
                        var attConstructor = proAtt.GetType().GetConstructor(new Type[] { typeof(string) });
                        var attBuilder = new CustomAttributeBuilder(attConstructor, new object[] { att.Description });
                        proBuilder.SetCustomAttribute(attBuilder);
                    }
                    else if (proAtt is ReadOnlyAttribute)
                    {
                        var att = proAtt as ReadOnlyAttribute;
                        var attConstructor = proAtt.GetType().GetConstructor(new Type[] { typeof(bool) });
                        var attBuilder = new CustomAttributeBuilder(attConstructor, new object[] { att.IsReadOnly });
                        proBuilder.SetCustomAttribute(attBuilder);
                    }
                    else if (proAtt is DisplayNameAttribute)
                    {
                        var att = proAtt as DisplayNameAttribute;
                        var attConstructor = proAtt.GetType().GetConstructor(new Type[] { typeof(string) });
                        var attBuilder = new CustomAttributeBuilder(attConstructor, new object[] { att.DisplayName });
                        proBuilder.SetCustomAttribute(attBuilder);
                    }
                    else if (proAtt is CategoryAttribute)
                    {
                        var att = proAtt as CategoryAttribute;
                        var attConstructor = proAtt.GetType().GetConstructor(new Type[] { typeof(string) });
                        var attBuilder = new CustomAttributeBuilder(attConstructor, new object[] { att.Category });
                        proBuilder.SetCustomAttribute(attBuilder);
                    }
                    else
                    {
                        bool hasZeroParamCons = false;
                        foreach (var cons in proAtt.GetType().GetConstructors())
                        {
                            var pams = cons.GetParameters();
                            if (pams == null || pams.Length == 0)
                            {
                                hasZeroParamCons = true;
                                break;
                            }
                        }
                        if (hasZeroParamCons)
                        {
                            var attConstructor = proAtt.GetType().GetConstructor(Type.EmptyTypes);
                            var attBuilder = new CustomAttributeBuilder(attConstructor, new object[0]);
                            proBuilder.SetCustomAttribute(attBuilder);
                        }
                    }
                }
                if (needTypeAtt)
                {
                    var tempType = typeof(EngineNS.Editor.Editor_UseCustomEditorAttribute);
                    var attConstructor = tempType.GetConstructor(new Type[0]);
                    var attBuilder = new CustomAttributeBuilder(attConstructor, new object[0]);
                    proBuilder.SetCustomAttribute(attBuilder);
                }
                ////// 添加DataValueAttribute特性，以便自动存取
                ////var dataValueAttConstructor = typeof(CSUtility.Support.DataValueAttribute).GetConstructor(new Type[] { typeof(string), typeof(bool) });
                ////var dataValueAttBuilder = new CustomAttributeBuilder(dataValueAttConstructor, new object[] { proInfo.PropertyName, true });
                ////proBuilder.SetCustomAttribute(dataValueAttBuilder);

                // get方法
                var getMethodBuilder = typeBuilder.DefineMethod(proInfo.GetMethodName, getSetAttr, proInfo.PropertyType, Type.EmptyTypes);
                var getIL = getMethodBuilder.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, fieldBuilder);
                proInfo.CustomAction_PropertyGet?.Invoke(getIL, fieldBuilder);
                getIL.Emit(OpCodes.Ret);

                // set方法
                var setMethodBuilder = typeBuilder.DefineMethod(proInfo.SetMethodName, getSetAttr, null, new Type[] { proInfo.PropertyType });
                var setIL = setMethodBuilder.GetILGenerator();
                var label0 = setIL.DefineLabel();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldfld, fieldBuilder);
                setIL.Emit(OpCodes.Stfld, fieldOldValueBuilder);
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, fieldBuilder);
                proInfo.CustomAction_PropertySet?.Invoke(setIL, fieldBuilder);
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldfld, hostNodeFieldBuilder);
                setIL.Emit(OpCodes.Brfalse_S, label0);
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldfld, hostNodeFieldBuilder);
                setIL.Emit(OpCodes.Ldc_I4_1);
                setIL.Emit(OpCodes.Callvirt, setDirtyMethod);
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldstr, proInfo.PropertyName);
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldfld, fieldBuilder);
                setIL.Emit(OpCodes.Box, proInfo.PropertyType);
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldfld, fieldOldValueBuilder);
                setIL.Emit(OpCodes.Box, proInfo.PropertyType);
                setIL.Emit(OpCodes.Call, onPropertyChangedMethod);
                setIL.MarkLabel(label0);
                setIL.Emit(OpCodes.Ret);

                proBuilder.SetGetMethod(getMethodBuilder);
                proBuilder.SetSetMethod(setMethodBuilder);
            }

            return typeBuilder.CreateType();
        }

        public static CodeGenerateSystem.Base.GeneratorClassBase CreateClassInstanceFromCustomPropertys(List<CustomPropertyInfo> propertys, BaseNodeControl node, string className = null, bool needNameProperty = true)
        {
            var cpPros = new List<CustomPropertyInfo>(propertys);

            // name属性Metadata不存
            if(needNameProperty)
            {
                var nameCPInfo = CustomPropertyInfo.GetFromParamInfo(typeof(string), "Name", new Attribute[] { new CategoryAttribute("Common") });
                cpPros.Add(nameCPInfo);
            }
            var nodeType = node.GetType();
            if(string.IsNullOrEmpty(className))
            {
                className = nodeType.FullName + "_" + EngineNS.Editor.Assist.GetValuedGUIDString(node.Id) + ".PropertyClass";
            }

            var classType = CodeGenerateSystem.Base.PropertyClassGenerator.CreateTypeFromCustomPropertys(cpPros, nodeType.Assembly.GetName().Name + "DynamicAssembly", className);
            var classInstance = System.Activator.CreateInstance(classType) as CodeGenerateSystem.Base.GeneratorClassBase;
            var field = classType.GetField("HostNode");
            if (field != null)
                field.SetValue(classInstance, node);
            if (needNameProperty)
            {
                var namePro = classType.GetProperty("Name");
                if (namePro != null)
                {
                    namePro.SetValue(classInstance, node.NodeName);
                    node.SetNameBinding(classInstance, "Name");
                    //node.SetBinding(BaseNodeControl.NodeNameBinderProperty, new Binding("Name") { Source = classInstance, Mode = BindingMode.TwoWay });
                }
            }
            classInstance.CSType = node.CSParam.CSType;

            return classInstance;
        }

        public static void SaveClassInstanceProperties(CodeGenerateSystem.Base.GeneratorClassBase classInstance, EngineNS.IO.XndAttrib att)
        {
            if (classInstance == null)
                return;

            var pros = classInstance.GetType().GetProperties();
            att.Write((int)pros.Length);
            foreach(var pro in pros)
            {
                att.Write(pro.Name);
                var typeSaveStr = EngineNS.Rtti.RttiHelper.GetTypeSaveString(pro.PropertyType);
                att.Write(typeSaveStr);
                var serializer = EngineNS.IO.Serializer.TypeDescGenerator.Instance.GetSerializer(pro.PropertyType);
                if (serializer == null && pro.PropertyType.IsInterface==false)
                    throw new InvalidOperationException($"保存属性失败,{pro.Name}({pro.PropertyType.FullName})找不到对应的序列化器");
                var infoDesc = new EngineNS.Rtti.PropMemberDesc(pro);
                var val = pro.GetValue(classInstance);
                serializer.WriteValue(val, att);
            }
        }
        public static void LoadClassInstanceProperties(CodeGenerateSystem.Base.GeneratorClassBase classInstance, EngineNS.IO.XndAttrib att)
        {
            if (classInstance == null)
                return;

            int count;
            att.Read(out count);
            var classType = classInstance.GetType();
            for(int i=0; i<count; i++)
            {
                string proName;
                att.Read(out proName);
                string typeSaveStr;
                att.Read(out typeSaveStr);
                var type = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(typeSaveStr);
                var serializer = EngineNS.IO.Serializer.TypeDescGenerator.Instance.GetSerializer(type);
                if (serializer == null)
                    throw new InvalidOperationException($"读取属性失败,{proName}({type.FullName})找不到对应的序列化器");
                var val = serializer.ReadValue(att);
                var pro = classType.GetProperty(proName);
                if (pro == null)
                    continue;
                if (pro.PropertyType != type)
                    continue;
                pro.SetValue(classInstance, val);
            }
        }
        public static void CloneClassInstanceProperties(CodeGenerateSystem.Base.GeneratorClassBase srcClassInstance, CodeGenerateSystem.Base.GeneratorClassBase tagClassInstance)
        {
            if (srcClassInstance == null || tagClassInstance == null)
                return;

            var srcType = srcClassInstance.GetType();
            var tagType = tagClassInstance.GetType();

            foreach(var pro in srcType.GetProperties())
            {
                var tagPro = tagType.GetProperty(pro.Name);
                if (tagPro == null)
                    continue;

                var val = pro.GetValue(srcClassInstance, null);
                tagPro.SetValue(tagClassInstance, val);
            }
        }
    }
}
