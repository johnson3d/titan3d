using System;
using System.Collections.Generic;

namespace EngineNS.Rtti
{
    /// <summary>
    /// 自定义Assembly实现
    /// </summary>
    public class VAssembly
    {
        //Mono.Cecil.ModuleDefinition mModule = null;
        System.Reflection.Assembly mAssembly = null;
        public System.Reflection.Assembly Assembly
        {
            get { return mAssembly; }
        }
        public EngineNS.ECSType CSType
        {
            get;
            private set;
        }

        string mKeyName = "";
        public string KeyName
        {
            get
            {
                if (string.IsNullOrEmpty(mKeyName))
                    throw new ArgumentNullException("KeyName没有设置");
                return mKeyName;
            }
            set => mKeyName = value;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        internal VAssembly(EngineNS.ECSType csType)
        {
            CSType = csType;
        }

        internal VAssembly(System.Reflection.Assembly assembly, EngineNS.ECSType csType)
        {
            mAssembly = assembly;
            CSType = csType;
        }
        //internal VAssembly(Mono.Cecil.ModuleDefinition module, EngineNS.ECSType csType)
        //{
        //    mModule = module;
        //    CSType = csType;
        //}
        /// <summary>
        /// 只读包含此程序集的显示名称
        /// </summary>
        public string Name
        {
            get
            {
                if (mAssembly != null)
                    return mAssembly.GetName().Name;
                return "";
            }
        }
        /// <summary>
        ///只读程序集的显示名称
        /// </summary>
        public string FullName
        {
            get
            {
                if (mAssembly != null)
                    return mAssembly.FullName;
                return "";
            }
        }
        /// <summary>
        /// 创建程序集的实例
        /// </summary>
        /// <param name="typeName">类型名称</param>
        /// <returns>返回object类型的类</returns>
        public object CreateInstance(string typeName)
        {
            if(mAssembly != null)
                return mAssembly.CreateInstance(typeName);

            return null;
        }
 
        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>返回类型</returns>
        public System.Type GetType(string name)
        {
            if (mAssembly != null)
                return mAssembly.GetType(name);

            return null;
        }
        Dictionary<Type, object[]> mTypesWithAttribute = new Dictionary<Type, object[]>();
        object[] mTypes = null;

        /// <summary>
        /// 获取Object
        /// </summary>
        /// <param name="attributeType">类型</param>
        /// <param name="inherit">是否支持继承</param>
        /// <returns></returns>
        public object[] GetTypes(Type attributeType, bool inherit)
        {
            object[] retValue;
            if (mTypesWithAttribute.TryGetValue(attributeType, out retValue))
                return retValue;

            var types = GetCommonTypes();
            var tempList = new System.Collections.Generic.List<object>(types.Length);
            for(int i=0; i<types.Length; i++)
            {
                var type = types[i];
                
                {
                    var systemType = type as Type;
                    var atts = systemType.GetCustomAttributes(attributeType, inherit);
                    if (atts.Length > 0)
                    {
                        tempList.Add(type);
                    }
                }
            }

            mTypesWithAttribute[attributeType] = tempList.ToArray();
            return mTypesWithAttribute[attributeType];
        }

        public Type[] GetTypes()
        {
            try
            {
                if (mAssembly != null)
                {
                    return mAssembly.GetTypes();
                }
            }
            catch (System.Exception e)
            {
                EngineNS.Profiler.Log.WriteException(e);
            }
            return new Type[0];
        }
        public object[] GetCommonTypes()
        {
            if (mTypes != null)
                return mTypes;

            try
            {
                if (mAssembly != null)
                {
                    mTypes = mAssembly.GetTypes();
                    return mTypes;
                }
            }
            catch(System.Exception ex)
            {
                EngineNS.Profiler.Log.WriteException(ex);
                return new Type[0];
            }

            return mTypes;
        }

        public static object GetFieldValue(string name, object obj)
        {
            if (obj == null)
                return null;
            
            {
                var fieldInfo = obj.GetType().GetField(name);
                if (fieldInfo != null)
                    return fieldInfo.GetValue(obj);
            }

            return null;
        }

        public static string GetTypeName(object typeObj)
        {
            {
                var sysType = typeObj as Type;
                if (sysType != null)
                    return sysType.Name;
            }

            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeObj"></param>
        /// <returns></returns>
        public static string GetTypeFullName(object typeObj)
        {
            {
                var sysType = typeObj as Type;
                if (sysType != null)
                    return sysType.FullName;
            }

            return "";
        }
    }
}
