using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace EngineNS.Rtti
{
    public class AttributeHelper
    {
        /// <summary>
        /// 获取函数指定名称的特性
        /// </summary>
        /// <param name="methodInfo">函数属性信息</param>
        /// <param name="attributeFullName">特性全名称（不包含Assembly）</param>
        /// <param name="inherit">是否搜索此成员的继承链以查找这些特性</param>
        /// <returns></returns>
        public static Attribute GetCustomAttribute(MethodInfo methodInfo, string attributeFullName, bool inherit)
        {
            foreach (var att in methodInfo.GetCustomAttributes(inherit))
            {
                if (att.GetType().FullName.Equals(attributeFullName))
                    return att as Attribute;
            }

            return null;
        }

        /// <summary>
        /// 获取属性指定名称的特性
        /// </summary>
        /// <param name="propertyInfo">属性信息</param>
        /// <param name="attributeFullName">特性全名称（不包含Assembly）</param>
        /// <param name="inherit">是否搜索此成员的继承链以查找这些特性</param>
        /// <returns></returns>
        public static Attribute GetCustomAttribute(PropertyInfo propertyInfo, string attributeFullName, bool inherit)
        {
            foreach (var att in propertyInfo.GetCustomAttributes(inherit))
            {
                if (att.GetType().FullName.Equals(attributeFullName))
                    return att as Attribute;
            }

            return null;
        }
        /// <summary>
        /// 获取字段指定名称的特性
        /// </summary>
        /// <param name="info">字段信息</param>
        /// <param name="attributeFullName">特性全名称（不包含Assembly）</param>
        /// <param name="inherit">是否搜索此成员的继承链以查找这些特性</param>
        /// <returns></returns>
        public static Attribute GetCustomAttribute(FieldInfo info, string attributeFullName, bool inherit)
        {
            foreach (var att in info.GetCustomAttributes(inherit))
            {
                if (att.GetType().FullName.Equals(attributeFullName))
                    return att as Attribute;
            }

            return null;
        }

        /// <summary>
        /// 获取参数指定名称的特性
        /// </summary>
        /// <param name="info">参数信息</param>
        /// <param name="attributeFullName">特性全名称（不包含Assembly）</param>
        /// <param name="inherit">是否搜索此成员的继承链以查找这些特性</param>
        /// <returns></returns>
        public static Attribute GetCustomAttribute(ParameterInfo info, string attributeFullName, bool inherit)
        {
            foreach (var att in info.GetCustomAttributes(inherit))
            {
                if (att.GetType().FullName.Equals(attributeFullName))
                    return att as Attribute;
            }
            return null;
        }

        /// <summary>
        /// 获取类型指定名称的特性
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="attributeFullName">特性全名称（不包含Assembly）</param>
        /// <param name="inherit">是否搜索此成员的继承链以查找这些特性</param>
        /// <returns></returns>
        public static Attribute GetCustomAttribute(Type type, string attributeFullName, bool inherit)
        {
            if (type == null)
                return null;
            foreach (var att in type.GetCustomAttributes(inherit))
            {
                if (att.GetType().FullName.Equals(attributeFullName))
                    return att as Attribute;
            }

            return null;
        }

        /// <summary>
        /// 获取指定特性的指定属性值
        /// </summary>
        /// <param name="attribute">特性</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public static object GetCustomAttributePropertyValue(Attribute attribute, string propertyName)
        {
            if (attribute == null)
                return null;

            var info = attribute.GetType().GetProperty(propertyName);
            if (info == null)
                return null;

            return info.GetValue(attribute, null);
        }

        /// <summary>
        /// 获取类型指定特性的属性的值
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="attributeFullName">特性全名称（包含命名空间，没有assembly信息）</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="inherit">是否搜索此成员的继承链以查找这些特性</param>
        /// <returns></returns>
        public static object GetCustomAttributePropertyValue(Type type, string attributeFullName, string propertyName, bool inherit)
        {
            var att = GetCustomAttribute(type, attributeFullName, inherit);
            return GetCustomAttributePropertyValue(att, propertyName);
        }
        /// <summary>
        /// 获取属性指定特性的属性的值
        /// </summary>
        /// <param name="info">属性</param>
        /// <param name="attributeFullName">特性全名称（包含命名空间，没有assembly信息）</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="inherit">是否搜索此成员的继承链以查找这些特性</param>
        /// <returns></returns>
        public static object GetCustomAttributePropertyValue(PropertyInfo info, string attributeFullName, string propertyName, bool inherit)
        {
            var att = GetCustomAttribute(info, attributeFullName, inherit);
            return GetCustomAttributePropertyValue(att, propertyName);
        }
        /// <summary>
        /// 获取函数指定特性的属性的值
        /// </summary>
        /// <param name="info">函数</param>
        /// <param name="attributeFullName">特性全名称（包含命名空间，没有assembly信息）</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="inherit">是否搜索此成员的继承链以查找这些特性</param>
        /// <returns></returns>
        public static object GetCustomAttributePropertyValue(MethodInfo info, string attributeFullName, string propertyName, bool inherit)
        {
            var att = GetCustomAttribute(info, attributeFullName, inherit);
            return GetCustomAttributePropertyValue(att, propertyName);
        }
        /// <summary>
        /// 获取字段指定特性的属性的值
        /// </summary>
        /// <param name="info">字段</param>
        /// <param name="attributeFullName">特性全名称（包含命名空间，没有assembly信息）</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="inherit">是否搜索此成员的继承链以查找这些特性</param>
        /// <returns></returns>
        public static object GetCustomAttributePropertyValue(FieldInfo info, string attributeFullName, string propertyName, bool inherit)
        {
            var att = GetCustomAttribute(info, attributeFullName, inherit);
            return GetCustomAttributePropertyValue(att, propertyName);
        }
        /// <summary>
        /// 获取字段指定参数的属性的值
        /// </summary>
        /// <param name="info">参数</param>
        /// <param name="attributeFullName">特性全名称（包含命名空间，没有assembly信息）</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="inherit">是否搜索此成员的继承链以查找这些特性</param>
        /// <returns></returns>
        public static object GetCustomAttributePropertyValue(ParameterInfo info, string attributeFullName, string propertyName, bool inherit)
        {
            var att = GetCustomAttribute(info, attributeFullName, inherit);
            return GetCustomAttributePropertyValue(att, propertyName);
        }

        public static MethodInfo GetCustomAttributeMethod(Attribute attribute, string methodName, Type[] methodParams)
        {
            if (attribute == null)
                return null;

            return attribute.GetType().GetMethod(methodName, methodParams);
        }
    }
}
