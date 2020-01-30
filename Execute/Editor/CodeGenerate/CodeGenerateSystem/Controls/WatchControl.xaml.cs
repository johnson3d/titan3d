using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CodeGenerateSystem.Controls
{
    /// <summary>
    /// WatchControl.xaml 的交互逻辑，用于监视连线工具采集的数据
    /// </summary>
    public partial class WatchControl : UserControl
    {
        // 用于显示数据的临时类
        Dictionary<Guid, CodeGenerateSystem.Base.GeneratorClassBase> mTemplateClassInstanceDic = new Dictionary<Guid, Base.GeneratorClassBase>();
        public EngineNS.ECSType CSType = EngineNS.ECSType.Common;

        public WatchControl()
        {
            InitializeComponent();
        }

        public void ShowValues(Guid id)
        {
            if(id == Guid.Empty)
            {
                PropertyGrid_Values.Instance = null;
                return;
            }

            //switch(CSType)
            //{
            //    case EngineNS.ECSType.Client:
            //        {
            //            var dataValues = CSUtility.Support.Runner.RunnerManager.Instance.GetDataValue(id);
            //            if (dataValues != null)
            //            {
            //                ShowValues(id, dataValues);
            //            }
            //        }
            //        break;
            //    case EngineNS.ECSType.Server:
            //        {
            //            CSUtility.Support.Runner.RunnerManager.Instance.GetDataValue_Remote(CCore.Client.Instance.GateSvrConnect, id, (datas)=>
            //            {
            //                ShowValues(id, datas);
            //            });
            //        }
            //        break;
            //    case EngineNS.ECSType.Common:
            //        {
            //            var dataValues = CSUtility.Support.Runner.RunnerManager.Instance.GetDataValue(id);
            //            if (dataValues != null)
            //            {
            //                ShowValues(id, dataValues);
            //            }
            //        }
            //        break;
            //}
        }

        //private void ShowValues(Guid id, Dictionary<string, EngineNS.Editor.Runner.RunnerManager.ValueData> dataValues, bool reset = false)
        //{
        //    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
        //    {
        //        CodeGenerateSystem.Base.GeneratorClassBase targetClass = null;
        //        mTemplateClassInstanceDic.TryGetValue(id, out targetClass);
        //        if(targetClass != null)
        //        {
        //            var propertys = targetClass.GetType().GetProperties();
        //            if (propertys.Length != dataValues.Count)
        //                reset = true;
        //        }

        //        if (targetClass == null || reset)
        //        {
        //            var RunnerThreadManagerType = typeof(CSUtility.Support.Runner.RunnerManager);
        //            var getRunnerThreadManagerInstanceInfo = RunnerThreadManagerType.GetMethod("get_Instance");
        //            var engineType = typeof(CCore.Engine);
        //            var getEngineInstanceInfo = engineType.GetMethod("get_Instance");
        //            var getClientMethod = engineType.GetMethod("get_Client");
        //            var getGateSvrConnect = typeof(CCore.Client).GetMethod("get_GateSvrConnect");
        //            System.Reflection.MethodInfo setDataValueMethodInfo = null;
        //            switch(CSType)
        //            {
        //                case EngineNS.ECSType.Client:
        //                    setDataValueMethodInfo = RunnerThreadManagerType.GetMethod("SetDataValue");
        //                    break;
        //                case EngineNS.ECSType.Server:
        //                    setDataValueMethodInfo = RunnerThreadManagerType.GetMethod("SetDataValue_Remote");
        //                    break;
        //            }
        //            var idStr = id.ToString();
        //            var guidTryParse = typeof(System.Guid).GetMethod("TryParse");

        //            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
        //            dataValues.For_EachLP((key, value, obj) =>
        //            {
        //                var retInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
        //                retInfo.PropertyName = value.Name;
        //                retInfo.PropertyType = value.ValueType;
        //                if(!value.CanChangeValue)
        //                {
        //                    retInfo.PropertyAttributes.Add(new ReadOnlyAttribute(true));
        //                }
        //                cpInfos.Add(retInfo);
                        
        //                retInfo.CustomAction_PropertySet = (setIL, fieldBuilder) =>
        //                {
        //                    var tempID = setIL.DeclareLocal(typeof(System.Guid));
        //                    setIL.Emit(System.Reflection.Emit.OpCodes.Ldstr, idStr);
        //                    setIL.Emit(System.Reflection.Emit.OpCodes.Ldloca_S, tempID);
        //                    // System.Guid.TryParse
        //                    setIL.Emit(System.Reflection.Emit.OpCodes.Call, guidTryParse);
        //                    setIL.Emit(System.Reflection.Emit.OpCodes.Pop);
        //                    //CSUtility.Support.Runner.RunnerManager.Instance.SetDataValue(id, value.Name, value.Value);
        //                    setIL.Emit(System.Reflection.Emit.OpCodes.Call, getRunnerThreadManagerInstanceInfo);
        //                    switch(CSType)
        //                    {
        //                        case EngineNS.ECSType.Server:
        //                            {
        //                                setIL.Emit(System.Reflection.Emit.OpCodes.Call, getEngineInstanceInfo);
        //                                setIL.Emit(System.Reflection.Emit.OpCodes.Callvirt, getClientMethod);
        //                                setIL.Emit(System.Reflection.Emit.OpCodes.Callvirt, getGateSvrConnect);
        //                            }
        //                            break;
        //                    }
        //                    setIL.Emit(System.Reflection.Emit.OpCodes.Ldloc_0);
        //                    setIL.Emit(System.Reflection.Emit.OpCodes.Ldstr, value.Name);
        //                    setIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
        //                    setIL.Emit(System.Reflection.Emit.OpCodes.Ldfld, fieldBuilder);
        //                    if(value.ValueType.IsValueType)
        //                        setIL.Emit(System.Reflection.Emit.OpCodes.Box, value.ValueType);
        //                    setIL.Emit(System.Reflection.Emit.OpCodes.Callvirt, setDataValueMethodInfo);
        //                };

        //                return CSUtility.Support.EForEachResult.FER_Continue;
        //            }, null);

        //            var classType = CodeGenerateSystem.Base.PropertyClassGenerator.CreateTypeFromCustomPropertys(cpInfos);
        //            targetClass = System.Activator.CreateInstance(classType) as CodeGenerateSystem.Base.GeneratorClassBase;
        //            mTemplateClassInstanceDic[id] = targetClass;
        //        }

        //        foreach (var property in targetClass.GetType().GetProperties())
        //        {
        //            var data = dataValues[property.Name];
        //            if (data == null)
        //                continue;

        //            property.SetValue(targetClass, data.Value);
        //        }

        //        PropertyGrid_Values.Instance = targetClass;
        //    }));            
        //}
    }
}
